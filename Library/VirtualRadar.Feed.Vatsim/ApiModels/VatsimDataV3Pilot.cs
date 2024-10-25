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
    public class VatsimDataV3Pilot
    {
        [DataMember(Name = "cid")]
        public int Cid { get; set; }

        [DataMember(Name = "name")]
        public string Name  { get; set; }

        [DataMember(Name = "callsign")]
        public string Callsign { get; set; }

        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "pilot_rating")]
        public int PilotRating { get; set; }

        [DataMember(Name = "latitude")]
        public float Latitude { get; set; }

        [DataMember(Name = "longitude")]
        public float Longitude { get; set; }

        private Location _Location;
        public Location Location
        {
            get {
                if(_Location == null || _Location.Latitude != (double)Latitude || _Location.Longitude != (double)Longitude) {
                    _Location = new(Latitude, Longitude);
                }
                return _Location;
            }
        }

        [DataMember(Name = "altitude")]
        public int AltitudeGeometricFeet { get; set; }

        [DataMember(Name = "groundspeed")]
        public int GroundSpeedKnots { get; set; }

        [DataMember(Name = "transponder")]
        public string Transponder { get; set; }

        private string _TransponderText;
        private int? _TransponderParsed;
        /// <summary>
        /// See <see cref="Transponder"/> parsed into a base-10 integer.
        /// </summary>
        public int? Squawk
        {
            get {
                if(!String.Equals(Transponder, _TransponderText)) {
                    _TransponderText = Transponder;
                    if(!int.TryParse(Transponder ?? "", out var parsed)) {
                        _TransponderParsed = null;
                    } else {
                        _TransponderParsed = parsed;
                    }
                }
                return _TransponderParsed;
            }
        }

        [DataMember(Name = "heading")]
        public int HeadingDegrees { get; set; }

        [DataMember(Name = "qnh_i_hg")]
        public float QnhInchesMercury { get; set; }

        [DataMember(Name = "qnh_mb")]
        public int QnhMillibars { get; set; }

        [DataMember(Name = "flight_plan")]
        public VatsimDataV3FlightPlan FlightPlan { get; set; }

        [DataMember(Name = "logon_time")]
        public DateTimeOffset LogonTime { get; set; }

        [DataMember(Name = "last_updated")]
        public DateTimeOffset LastUpdated { get; set; }

        public static VatsimDataV3Pilot CopyFrom(VatsimDataV3Pilot copyFrom)
        {
            VatsimDataV3Pilot result = null;

            if(copyFrom != null) {
                result = new() {
                    Cid =                   copyFrom.Cid,
                    Name =                  copyFrom.Name,
                    Callsign =              copyFrom.Callsign,
                    Server =                copyFrom.Server,
                    PilotRating =           copyFrom.PilotRating,
                    Latitude =              copyFrom.Latitude,
                    Longitude =             copyFrom.Longitude,
                    AltitudeGeometricFeet = copyFrom.AltitudeGeometricFeet,
                    GroundSpeedKnots =      copyFrom.GroundSpeedKnots,
                    Transponder =           copyFrom.Transponder,
                    HeadingDegrees =        copyFrom.HeadingDegrees,
                    QnhInchesMercury =      copyFrom.QnhInchesMercury,
                    QnhMillibars =          copyFrom.QnhMillibars,
                    FlightPlan =            VatsimDataV3FlightPlan.CopyFrom(copyFrom.FlightPlan),
                    LogonTime =             copyFrom.LogonTime,
                    LastUpdated =           copyFrom.LastUpdated,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3Pilot)} {{"
                + $" {nameof(Cid)}: {Cid}"
                + $" {nameof(Name)}: {Name}"
                + $" {nameof(Callsign)}: {Callsign}"
                + $" {nameof(Server)}: {Server}"
                + $" {nameof(PilotRating)}: {PilotRating}"
                + $" {nameof(Latitude)}: {Latitude}"
                + $" {nameof(Longitude)}: {Longitude}"
                + $" {nameof(AltitudeGeometricFeet)}: {AltitudeGeometricFeet}"
                + $" {nameof(GroundSpeedKnots)}: {GroundSpeedKnots}"
                + $" {nameof(Transponder)}: {Transponder}"
                + $" {nameof(HeadingDegrees)}: {HeadingDegrees}"
                + $" {nameof(QnhInchesMercury)}: {QnhInchesMercury}"
                + $" {nameof(QnhMillibars)}: {QnhMillibars}"
                + $" {nameof(FlightPlan)}: {FlightPlan}"
                + $" {nameof(LogonTime)}: {LogonTime}"
                + $" {nameof(LastUpdated)}: {LastUpdated}"
                + " }";
        }
    }
}
