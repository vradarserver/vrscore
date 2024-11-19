// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Connection;
using VirtualRadar.Feed;
using VirtualRadar.Message;

namespace VirtualRadar.Receivers
{
    /// <summary>
    /// Brings together a source of messages, a decoded feed of those messages and an aircraft list.
    /// </summary>
    public class Receiver : IReceiver
    {
        private IAircraftOnlineLookupService _AircraftLookupService;

        /// <inheritdoc/>
        public ReceiverOptions Options { get; }

        /// <inheritdoc/>
        public int Id => Options.Id;

        /// <inheritdoc/>
        public string Name => Options.Name;

        /// <inheritdoc/>
        public bool Enabled => Options.Enabled;

        /// <inheritdoc/>
        public bool Hidden => Options.Hidden;

        private CancellationTokenSource _ConnectionCancellationTokenSource = new();
        /// <inheritdoc/>
        public CancellationToken ConnectionCancellationToken => _ConnectionCancellationTokenSource.Token;

        /// <inheritdoc/>
        public IReceiveConnector Connector { get; }

        /// <inheritdoc/>
        public IFeedDecoder FeedDecoder { get; }

        /// <inheritdoc/>
        public IAircraftList AircraftList { get; } = new AircraftList();

        private long _CountPacketsReceived;
        /// <inheritdoc/>
        public long CountPacketsReceived => _CountPacketsReceived;

        private long _CountMessagesReceived;
        /// <inheritdoc/>
        public long CountMessagesReceived => _CountMessagesReceived;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connector"></param>
        /// <param name="feedDecoder"></param>
        /// <param name="aircraftLookupService"></param>
        internal Receiver(
            ReceiverOptions options,
            IReceiveConnector connector,
            IFeedDecoder feedDecoder,
            IAircraftOnlineLookupService aircraftLookupService
        )
        {
            Options = options;
            Connector = connector;
            FeedDecoder = feedDecoder;

            _AircraftLookupService = aircraftLookupService;
            _AircraftLookupService.LookupCompleted += AircraftLookupService_LookupCompleted;

            Connector.PacketReceived += Connector_PacketReceived;
            FeedDecoder.MessageReceived += FeedDecoder_MessageReceived;
            FeedDecoder.LookupReceived += FeedDecoder_LookupReceived;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Receiver() => Dispose(false);

        /// <inheritdoc/>
        public override string ToString() => Name ?? "";

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                var lookupService = _AircraftLookupService;
                if(lookupService != null) {
                    lookupService.LookupCompleted -= AircraftLookupService_LookupCompleted;
                }
                _AircraftLookupService = null;

                Connector.PacketReceived -= Connector_PacketReceived;
                FeedDecoder.MessageReceived -= FeedDecoder_MessageReceived;
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            Task.Run(() => Connector.OpenAsync(ConnectionCancellationToken));
        }

        private void AircraftLookupService_LookupCompleted(object sender, BatchedLookupOutcome<LookupByIcaoOutcome> args)
        {
            AircraftList.ApplyLookup(args);
        }

        private void Connector_PacketReceived(object sender, ReadOnlyMemory<byte> args)
        {
            Interlocked.Increment(ref _CountPacketsReceived);
            FeedDecoder.ParseFeedPacket(args);
        }

        private void FeedDecoder_LookupReceived(object sender, LookupByAircraftIdOutcome lookupOutcome)
        {
            AircraftList.ApplyLookup(lookupOutcome);
        }

        private void FeedDecoder_MessageReceived(object sender, TransponderMessage args)
        {
            Interlocked.Increment(ref _CountMessagesReceived);
            if(AircraftList.ApplyMessage(args).AddedAircraft) {
                if(!args.SuppressLookup && (args.Icao24?.IsValid ?? false)) {
                    var lookupService = _AircraftLookupService;
                    lookupService?.Lookup(args.Icao24.Value);
                }
            }
        }
    }
}
