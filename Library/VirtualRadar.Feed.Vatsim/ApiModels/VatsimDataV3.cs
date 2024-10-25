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
    public class VatsimDataV3
    {
        [DataMember(Name = "general")]
        public VatsimDataV3General General { get; set; } = new();

        [DataMember(Name = "pilots")]
        public List<VatsimDataV3Pilot> Pilots { get; } = [];

        [DataMember(Name = "controllers")]
        public List<VatsimDataV3Controller> Controllers { get; } = [];

        [DataMember(Name = "atis")]
        public List<VatsimDataV3Atis> Atis { get; } = [];

        [DataMember(Name = "servers")]
        public List<VatsimDataV3Server> Servers { get; } = [];

        [DataMember(Name = "prefiles")]
        public List<VatsimDataV3Prefile> Prefiles { get; } = [];

        [DataMember(Name = "facilities")]
        public List<VatsimDataV3Facility> Facilities { get; } = [];

        [DataMember(Name = "ratings")]
        public List<VatsimDataV3Rating> Ratings { get; } = [];

        [DataMember(Name = "pilot_ratings")]
        public List<VatsimDataV3PilotRating> PilotRatings { get; } = [];

        public override string ToString()
        {
            return $"{nameof(Status)}: {{"
                + $" {nameof(General)}: {General?.ToString() ?? "<null>"}"
                + $" {nameof(Pilots)}: {Pilots?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Controllers)}: {Controllers?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Atis)}: {Atis?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Servers)}: {Servers?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Prefiles)}: {Prefiles?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Facilities)}: {Facilities?.Count.ToString() ?? "<null>"}"
                + $" {nameof(Ratings)}: {Ratings?.Count.ToString() ?? "<null>"}"
                + $" {nameof(PilotRatings)}: {PilotRatings?.Count.ToString() ?? "<null>"}"
                + " }";
        }
    }
}
