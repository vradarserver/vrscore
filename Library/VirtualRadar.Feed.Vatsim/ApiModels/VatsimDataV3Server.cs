﻿// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Serialization;

namespace VirtualRadar.Feed.Vatsim.ApiModels
{
    [DataContract]
    public class VatsimDataV3Server
    {
        [DataMember(Name = "ident")]
        public string Ident { get; set; }

        [DataMember(Name = "hostname_or_ip")]
        public string HostnameOrIPAddress { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "clients_connection_allowed")]
        public int ClientsConnectionAllowed { get; set; }

        [DataMember(Name = "client_connections_allowed")]
        public bool ClientConnectionsAllowed { get; set; }

        [DataMember(Name = "is_sweatbox")]
        public bool IsSweatbox { get; set; }

        public static VatsimDataV3Server CopyFrom(VatsimDataV3Server copyFrom)
        {
            VatsimDataV3Server result = null;

            if(copyFrom != null) {
                result = new() {
                    Ident =                     copyFrom.Ident,
                    HostnameOrIPAddress =       copyFrom.HostnameOrIPAddress,
                    Location =                  copyFrom.Location,
                    Name =                      copyFrom.Name,
                    ClientsConnectionAllowed =  copyFrom.ClientsConnectionAllowed,
                    ClientConnectionsAllowed =  copyFrom.ClientConnectionsAllowed,
                    IsSweatbox =                copyFrom.IsSweatbox,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3Server)} {{"
                + $" {nameof(Ident)}: {Ident}"
                + $" {nameof(HostnameOrIPAddress)}: {HostnameOrIPAddress}"
                + $" {nameof(Location)}: {Location}"
                + $" {nameof(Name)}: {Name}"
                + $" {nameof(ClientsConnectionAllowed)}: {ClientsConnectionAllowed}"
                + $" {nameof(ClientConnectionsAllowed)}: {ClientConnectionsAllowed}"
                + $" {nameof(IsSweatbox)}: {IsSweatbox}"
                + " }";
        }
    }
}
