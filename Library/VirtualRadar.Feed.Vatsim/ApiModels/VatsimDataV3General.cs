// Copyright © 2022 onwards, Andrew Whewell
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
    public class VatsimDataV3General
    {
        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "reload")]
        public int Reload { get; set; }

        [DataMember(Name = "update")]
        public string Update { get; set; }

        [DataMember(Name = "update_timestamp")]
        public string UpdateTimestamp { get; set; }

        [DataMember(Name = "connected_clients")]
        public int ConnectedClients { get; set; }

        [DataMember(Name = "unique_users")]
        public int UniqueUsers { get; set; }

        public static VatsimDataV3General CopyFrom(VatsimDataV3General copyFrom)
        {
            VatsimDataV3General result = null;
            
            if(copyFrom != null) {
                result = new() {
                    Version =          copyFrom.Version,
                    Reload =           copyFrom.Reload,
                    Update =           copyFrom.Update,
                    UpdateTimestamp =  copyFrom.UpdateTimestamp,
                    ConnectedClients = copyFrom.ConnectedClients,
                    UniqueUsers =      copyFrom.UniqueUsers,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3General)} {{"
                + $" {nameof(Version)}: {Version}"
                + $" {nameof(Reload)}: {Reload}"
                + $" {nameof(Update)}: {Update}"
                + $" {nameof(UpdateTimestamp)}: {UpdateTimestamp}"
                + $" {nameof(ConnectedClients)}: {ConnectedClients}"
                + $" {nameof(UniqueUsers)}: {UniqueUsers}"
                + " }";
        }
    }
}
