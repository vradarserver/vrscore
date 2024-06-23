// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.IO;
using VirtualRadar.Message;

namespace VirtualRadar.Feed.BaseStation
{
    /// <summary>
    /// The default implementation of <see cref="IFeedDecoder"/> for BaseStation feeds.
    /// </summary>
    class BaseStationFeedDecoder : IFeedDecoder, IOneTimeConfigurable<BaseStationFeedDecoderOptions>
    {
        private OneTimeConfigurableImplementer<BaseStationFeedDecoderOptions> _OneTimeConfig = new(nameof(BaseStationFeedDecoder), new());
        private AsciiLineChunker _StreamChunker = new();
        private IStreamChunkerState _StreamChunkerState;
        private BaseStationMessageConverter _MessageConverter;

        /// <inheritdoc/>
        public BaseStationFeedDecoderOptions Options => _OneTimeConfig.Options;

        /// <inheritdoc/>
        public event EventHandler<TransponderMessage> MessageReceived;

        /// <summary>
        /// Raises <see cref="MessageReceived"/>.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageReceived(TransponderMessage message)
        {
            MessageReceived?.Invoke(this, message);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="formatConfig"></param>
        public BaseStationFeedDecoder(BaseStationMessageConverter messageConverter)
        {
            _MessageConverter = messageConverter;
            _StreamChunker.ChunkRead += (_, chunk) => {
                foreach(var message in _MessageConverter.FromFeedMessage(chunk)) {
                    OnMessageReceived(message);
                }
            };
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if(_StreamChunkerState != null) {
                _StreamChunkerState.Dispose();
                _StreamChunkerState = null;
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void Configure(BaseStationFeedDecoderOptions options)
        {
            _OneTimeConfig.Configure(options);
            _MessageConverter.Configure(new() {
                Icao24CanHaveNonHexDigits = options.Icao24CanHaveNonHexDigits,
            });
        }

        /// <inheritdoc/>
        public Task ParseFeedPacket(ReadOnlyMemory<byte> packet)
        {
            _OneTimeConfig.AssertConfigured();
            _StreamChunkerState = _StreamChunker.ParseBlock(packet, _StreamChunkerState);

            return Task.CompletedTask;
        }
    }
}
