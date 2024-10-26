// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using VirtualRadar.Connection;
using VirtualRadar.Feed.Vatsim.ApiModels;
using VirtualRadar.StandingData;

namespace VirtualRadar.Feed.Vatsim
{
    /// <summary>
    /// The connector for VATSIM feeds. This just latches onto the <see cref="VatsimDownloader"/>.
    /// </summary>
    [ReceiveConnector(typeof(VatsimConnectorOptions))]
    public class VatsimConnector : IReceiveConnector
    {
        private object _SyncLock = new();
        private ICallbackHandle _DownloaderDataDownloadedHandle;

        /// <summary>
        /// The options that the connector was created with.
        /// </summary>
        public VatsimConnectorOptions Options { get; }

        /// <inheritdoc/>
        IConnectorOptions IConnector.Options => Options;

        /// <summary>
        /// The downloader that the connector is listening to for VATSIM data.
        /// </summary>
        public IVatsimDownloader VatsimDownloader { get; }

        /// <summary>
        /// The standing data manager we're using.
        /// </summary>
        public IStandingDataManager StandingData { get; }

        /// <inheritdoc/>
        public string Description => "VATSIM connector";

        private ConnectionState _ConnectionState;
        /// <inheritdoc/>
        public ConnectionState ConnectionState
        {
            get => _ConnectionState;
            protected set {
                if(value != ConnectionState) {
                    _ConnectionState = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        private TimestampedException _LastException;
        /// <inheritdoc/>
        public TimestampedException LastException
        {
            get => _LastException;
            protected set {
                if(value != _LastException) {
                    _LastException = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<ReadOnlyMemory<byte>> PacketReceived;

        /// <summary>
        /// Raises <see cref="PacketReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPacketReceived(ReadOnlyMemory<byte> args)
        {
            PacketReceived?.Invoke(this, args);
        }

        /// <inheritdoc/>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs args)
        {
            ConnectionStateChanged?.Invoke(this, args);
        }

        /// <inheritdoc/>
        public event EventHandler LastExceptionChanged;

        /// <summary>
        /// Raises <see cref="LastExceptionChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLastExceptionChanged(EventArgs args)
        {
            LastExceptionChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="vatsimDownloader"></param>
        /// <param name="options"></param>
        /// <param name="standingData"></param>
        public VatsimConnector(
            IVatsimDownloader vatsimDownloader,
            VatsimConnectorOptions options,
            IStandingDataManager standingData
        )
        {
            VatsimDownloader = vatsimDownloader;
            Options = options;
            StandingData = standingData;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if(disposing) {
                lock(_SyncLock) {
                    if(_DownloaderDataDownloadedHandle != null) {
                        _DownloaderDataDownloadedHandle.Dispose();
                        _DownloaderDataDownloadedHandle = null;
                    }
                }
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OpenAsync(CancellationToken cancellationToken)
        {
            ConnectionState = ConnectionState.Opening;
            lock(_SyncLock) {
                if(_DownloaderDataDownloadedHandle == null) {
                    _DownloaderDataDownloadedHandle = VatsimDownloader.AddDataDownloadedCallback(
                        VatsimDataDownloaded
                    );
                }
            }
            ConnectionState = ConnectionState.Open;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            ConnectionState = ConnectionState.Closing;
            lock(_SyncLock) {
                if(_DownloaderDataDownloadedHandle != null) {
                    _DownloaderDataDownloadedHandle.Dispose();
                    _DownloaderDataDownloadedHandle = null;
                }
            }
            ConnectionState = ConnectionState.Closed;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by <see cref="VatsimDownloader"/> whenever data has been downloaded from VATSIM.
        /// This filters out the aircraft that are not of interest and serialises it to JSON. This
        /// deserialisation (by the downloader), reserialise by the connector and then deserialise
        /// by the feed is inefficient, but it happens once every 15+ seconds so I'm not too fussed
        /// about figuring out a better way just yet.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void VatsimDataDownloaded(VatsimDataV3 data)
        {
            if(data != null) {
                var geofence = BuildGeofence(data);
                var filteredFeed = new FilteredFeed();
                filteredFeed.Pilots.AddRange(data
                    .Pilots
                    .Where(pilot => geofence.Contains(pilot.Location))
                );

                var jsonText = JsonConvert.SerializeObject(filteredFeed);
                var jsonBytes = Encoding.UTF8.GetBytes(jsonText);
                OnPacketReceived(jsonBytes);
            }
        }

        private LocationRectangle BuildGeofence(VatsimDataV3 data)
        {
            Location centre;
            switch(Options.CentreOn) {
                case GeofenceCentreOn.Airport:
                    var airport = StandingData.FindAirportForCode(Options.CentreOnAirport);
                    centre = airport?.Location;
                    break;
                case GeofenceCentreOn.Coordinate:
                    centre = Options.CentreOnLocation;
                    break;
                case GeofenceCentreOn.PilotCid:
                    var pilot = data.Pilots.FirstOrDefault(pilot => pilot.Cid == Options.CentreOnPilotCid);
                    centre = pilot?.Location;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return centre == null
                ? LocationRectangle.Empty
                : LocationRectangle.FromCentre(
                      centre,
                      Options.GeofenceDistanceUnit,
                      Options.GeofenceWidth,
                      Options.GeofenceHeight
                  );
        }
    }
}
