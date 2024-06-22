// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using VirtualRadar.Connection;

namespace VirtualRadar.Feed.Recording
{
    /// <summary>
    /// A VRS connector whose source of feed messages is a recording.
    /// </summary>
    public class RecordingPlaybackConnector : IReceiveConnector
    {
        internal PlaybackConnectorState _State;

        /// <summary>
        /// The options used to create the connector.
        /// </summary>
        public RecordingPlaybackConnectorOptions Options { get; }

        /// <inheritdoc/>
        public string Description => $"{Options.RecordingFileName} x{Options.PlaybackSpeed}";

        private ConnectionState _ConnectionState;
        /// <inheritdoc/>
        public ConnectionState ConnectionState
        {
            get => _ConnectionState;
            internal set {
                if(value != ConnectionState) {
                    _ConnectionState = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
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
        public event EventHandler<ReadOnlyMemory<byte>> PacketReceived;

        /// <summary>
        /// Raised <see cref="PacketReceived"/>.
        /// </summary>
        /// <param name="packet"></param>
        protected virtual void OnPacketReceived(ReadOnlyMemory<byte> packet)
        {
            PacketReceived?.Invoke(this, packet);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="options"></param>
        public RecordingPlaybackConnector(RecordingPlaybackConnectorOptions options)
        {
            Options = options;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            try {
                _State?.TearDown();
            } catch(ConnectionTearDownException) {
                ;
            }
            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            if(ConnectionState != ConnectionState.Closed) {
                throw new InvalidOperationException($"You cannot open a connection that is in the {ConnectionState} state");
            }

            PlaybackConnectorState state = null;
            try {
                ConnectionState = ConnectionState.Opening;

                state = new() {
                    Parent = this,
                    PlaybackSync = new(Options.PlaybackSpeed),
                    Reader = new(),
                    FileStream = new FileStream(Options.RecordingFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                };
                state.SetupCancellationTokens(cancellationToken);

                await state.Reader.InitialiseStreamAsync(state.FileStream, leaveOpen: true);

                _State = state;
                _State.PumpTask = RunPacketPump(state);
            } finally {
                if(ConnectionState != ConnectionState.Open) {
                    state?.TearDown();
                }
            }
        }

        /// <inheritdoc/>
        public Task CloseAsync()
        {
            if(_State != null) {
                _State.TearDown();
                _State = null;
            }

            return Task.CompletedTask;
        }

        private async Task RunPacketPump(PlaybackConnectorState state)
        {
            try {
                Parcel parcel = null;
                while((parcel = await state.Reader.GetNextAsync(state.LinkedCancellationToken.Token)) != null) {
                    if(state.LinkedCancellationToken.Token.IsCancellationRequested) {
                        break;
                    }

                    await state.PlaybackSync.WaitForEventAsync(
                        parcel.MillisecondReceived,
                        state.LinkedCancellationToken.Token
                    );

                    OnPacketReceived(parcel.Packet);
                }
            } catch(OperationCanceledException) {
                ;
            } catch(Exception) {
                // TODO: Log exceptions that force the connection closed
                try {
                    state.TearDown();
                } catch {
                    ;
                }
            }
        }
    }
}
