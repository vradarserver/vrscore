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
    public class VatsimDataV3Atis
    {
        [DataMember(Name = "cid")]
        public int Cid { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "callsign")]
        public string Callsign { get; set; }

        [DataMember(Name = "frequency")]
        public string Frequency { get; set; }

        [DataMember(Name = "facility")]
        public int Facility { get; set; }

        [DataMember(Name = "rating")]
        public int Rating { get; set; }

        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "visual_range")]
        public int VisualRange { get; set; }

        [DataMember(Name = "atis_code")]
        public string AtisCode { get; set; }

        [DataMember(Name = "text_atis")]
        public List<string> TextAtis { get; } = [];

        [DataMember(Name = "last_updated")]
        public DateTimeOffset LastUpdated { get; set; }

        [DataMember(Name = "logon_time")]
        public DateTimeOffset LogonTime { get; set; }

        public static VatsimDataV3Atis CopyFrom(VatsimDataV3Atis copyFrom)
        {
            VatsimDataV3Atis result = null;

            if(copyFrom != null) {
                result = new() {
                    Cid =           copyFrom.Cid,
                    Name =          copyFrom.Name,
                    Callsign =      copyFrom.Callsign,
                    Frequency =     copyFrom.Frequency,
                    Facility =      copyFrom.Facility,
                    Rating =        copyFrom.Rating,
                    Server =        copyFrom.Server,
                    VisualRange =   copyFrom.VisualRange,
                    AtisCode =      copyFrom.AtisCode,
                    LastUpdated =   copyFrom.LastUpdated,
                    LogonTime =     copyFrom.LogonTime,
                };
                result.TextAtis.AddRange(copyFrom.TextAtis);
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3Atis)} {{"
                + $" {nameof(Cid)}: {Cid}"
                + $" {nameof(Name)}: {Name}"
                + $" {nameof(Callsign)}: {Callsign}"
                + $" {nameof(Frequency)}: {Frequency}"
                + $" {nameof(Facility)}: {Facility}"
                + $" {nameof(Rating)}: {Rating}"
                + $" {nameof(Server)}: {Server}"
                + $" {nameof(VisualRange)}: {VisualRange}"
                + $" {nameof(AtisCode)}: {AtisCode}"
                + $" {nameof(TextAtis)}: {String.Join(",", TextAtis)}"
                + $" {nameof(LastUpdated)}: {LastUpdated}"
                + $" {nameof(LogonTime)}: {LogonTime}"
                + " }";
        }
    }
}
