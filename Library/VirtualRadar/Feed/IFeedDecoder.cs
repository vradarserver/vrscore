﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Message;

namespace VirtualRadar.Feed
{
    /// <summary>
    /// Describes an object that can take packets of bytes and decode them into transponder messages.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IFeedDecoder : IAsyncDisposable
    {
        /// <summary>
        /// Gets the options that were used to create this decoder instance.
        /// </summary>
        IFeedDecoderOptions Options { get; }

        /// <summary>
        /// True if the feed contains lookup details, information that is not transmitted by Mode-S
        /// and/or ADS-B.
        /// </summary>
        bool FeedContainsLookups { get; }

        /// <summary>
        /// Raised for every transponder message extracted from the feed.
        /// </summary>
        event EventHandler<TransponderMessage> MessageReceived;

        /// <summary>
        /// Raised when <see cref="FeedContainsLookups"/> is true and information about an aircraft
        /// has been decoded from the feed.
        /// </summary>
        event EventHandler<LookupByAircraftIdOutcome> LookupReceived;

        /// <summary>
        /// Presents a packet to the decoder for parsing. The packet might be incomplete.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        Task ParseFeedPacket(ReadOnlyMemory<byte> packet);
    }
}
