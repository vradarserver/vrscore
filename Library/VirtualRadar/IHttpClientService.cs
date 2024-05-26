// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;

namespace VirtualRadar
{
    /// <summary>
    /// Manages the creation of HttpClient instances without the overhead of IHttpClientFactory.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface IHttpClientService
    {
        /// <summary>
        /// Gets the shared instance. Prefer this if you do not need cookie containers. Do not
        /// set any base addresses, default headers etc. on this instance, you will break other
        /// code. Do not dispose of the client after use.
        /// </summary>
        HttpClient Shared { get; }

        /// <summary>
        /// Be careful with this. Do not create many instances, HttpClient is fragile and does not implement
        /// the correct Dispose() behaviour.
        /// </summary>
        /// <param name="pooledConnectionLifetimeMinutes">
        /// Number of minutes that a pooled connection will be held for, 0 to leave at default, null to use
        /// service default (60 as of time of writing).
        /// </param>
        /// <param name="automaticDecompressionMethods">
        /// The automatic decompression methods to enable, null to use the service default (All as of time of
        /// writing).
        /// </param>
        /// <returns></returns>
        HttpClient CreateNew(
            int? pooledConnectionLifetimeMinutes = null,
            DecompressionMethods? automaticDecompressionMethods = null
        );
    }
}
