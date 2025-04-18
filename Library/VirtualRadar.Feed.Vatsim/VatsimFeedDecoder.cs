﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using VirtualRadar.Feed.Vatsim.ApiModels;
using VirtualRadar.Message;
using VirtualRadar.StandingData;

namespace VirtualRadar.Feed.Vatsim
{
    [FeedDecoder(typeof(VatsimFeedDecoderOptions))]
    public class VatsimFeedDecoder : IFeedDecoder
    {
        private CommonFeedParser _CommonFeedParser;

        /// <summary>
        /// The options used to create the decoder.
        /// </summary>
        public VatsimFeedDecoderOptions Options { get; }

        /// <inheritdoc/>
        IFeedDecoderOptions IFeedDecoder.Options => Options;

        /// <inheritdoc/>
        public bool FeedContainsLookups => true;

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

        /// <inheritdoc/>
        public event EventHandler<LookupByAircraftIdOutcome> LookupReceived;

        /// <summary>
        /// Raises <see cref="LookupReceived"/>.
        /// </summary>
        /// <param name="lookupOutcome"></param>
        protected virtual void OnLookupReceived(LookupByAircraftIdOutcome lookupOutcome)
        {
            LookupReceived?.Invoke(this, lookupOutcome);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="commonFeedParser"></param>
        public VatsimFeedDecoder(
            VatsimFeedDecoderOptions options,
            CommonFeedParser commonFeedParser
        )
        {
            Options = options;
            _CommonFeedParser = commonFeedParser;
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
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Converts a <see cref="FilteredFeed"/> back into an object and translates from that into a whole
        /// pile of transponder messages to get the correct effect in the aircraft list.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public Task ParseFeedPacket(ReadOnlyMemory<byte> packet)
        {
            var jsonText = Encoding.UTF8.GetString(packet.ToArray());
            var filteredFeed = JsonConvert.DeserializeObject<FilteredFeed>(jsonText);

            foreach(var pilot in filteredFeed.Pilots) {
                GenerateMessagesForPilot(pilot);
                GenerateLookupForPilot(pilot);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Converts VATSIM state into aircraft messages.
        /// </summary>
        /// <param name="pilot"></param>
        private void GenerateMessagesForPilot(VatsimDataV3Pilot pilot)
        {
            var pilotState = _CommonFeedParser.BuildOrReusePilotState(pilot);
            OnMessageReceived(pilotState.TransponderMessage);
        }

        /// <summary>
        /// Converts VATSIM state into lookup outcomes.
        /// </summary>
        /// <param name="pilot"></param>
        private void GenerateLookupForPilot(VatsimDataV3Pilot pilot)
        {
            var pilotState = _CommonFeedParser.BuildOrReusePilotState(pilot);
            OnLookupReceived(pilotState.LookupOutcome);
        }
    }
}
