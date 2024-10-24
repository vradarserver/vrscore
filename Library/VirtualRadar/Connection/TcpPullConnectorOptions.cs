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
using Newtonsoft.Json;
using VirtualRadar.Configuration;
using VirtualRadar.Receivers;

namespace VirtualRadar.Connection
{
    /// <summary>
    /// Carries options for a <see cref="TcpPullConnector"/>.
    /// </summary>
    /// <param name="Address">
    /// The text of the IP address to connect to. This can be IPv4 or IPv6. See <see cref="ParsedAddress"/>
    /// for the parsed version of this.
    /// </param>
    /// <param name="Port">
    /// The port to connect to.
    /// </param>
    [SettingsProvider(_ProviderName)]
    public record TcpPullConnectorSettings(
        string Address,
        int Port
    ) : IReceiverConnectorOptions
    {
        private const string _ProviderName = "TcpPullConnector";

        /// <inheritdoc/>
        public string SettingsProvider => _ProviderName;

        private IPAddress _ParsedAddress;
        private bool _CannotParseAddress;
        /// <summary>
        /// <see cref="Address"/> parsed into a System.Net.IPAddress. If the address is invalid then
        /// this will be null.
        /// </summary>
        [JsonIgnore]
        public IPAddress ParsedAddress
        {
            get {
                if(_ParsedAddress == null && !_CannotParseAddress) {
                    var parsed = IPAddress.TryParse(Address, out var parsedAddress);
                    if(!parsed) {
                        _CannotParseAddress = true;
                    } else {
                        _ParsedAddress = parsedAddress;
                    }
                }
                return _ParsedAddress;
            }
        }

        /// <summary>
        /// Default ctor.
        /// </summary>
        public TcpPullConnectorSettings() : this(IPAddress.None.ToString(), 0)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Address}:{Port}";
    }
}
