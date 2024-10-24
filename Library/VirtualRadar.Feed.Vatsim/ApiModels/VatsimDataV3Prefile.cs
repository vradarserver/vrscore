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
    public class VatsimDataV3Prefile
    {
        [DataMember(Name = "cid")]
        public int Cid { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "callsign")]
        public string Callsign { get; set; }

        [DataMember(Name = "flight_plan")]
        public VatsimDataV3FlightPlan FlightPlan { get; set; }

        [DataMember(Name = "last_updated")]
        public DateTimeOffset LastUpdated { get; set; }

        public static VatsimDataV3Prefile CopyFrom(VatsimDataV3Prefile copyFrom)
        {
            VatsimDataV3Prefile result = null;

            if(copyFrom != null) {
                result = new() {
                    Cid =           copyFrom.Cid,
                    Name =          copyFrom.Name,
                    Callsign =      copyFrom.Callsign,
                    FlightPlan =    VatsimDataV3FlightPlan.CopyFrom(copyFrom.FlightPlan),
                    LastUpdated =   copyFrom.LastUpdated,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3Prefile)} {{" +
                $" Cid: {Cid}" +
                $" Name: {Name}" +
                $" Callsign: {Callsign}" +
                $" FlightPlan: {FlightPlan}" +
                $" LastUpdated: {LastUpdated}" +
                " }";
        }
    }
}
