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
    public class VatsimDataV3FlightPlan
    {
        [DataMember(Name = "flight_rules")]
        public string FlightRules { get; set; }

        [DataMember(Name = "aircraft")]
        public string Aircraft { get; set; }

        [DataMember(Name = "aircraft_faa")]
        public string AircraftFAA { get; set; }

        [DataMember(Name = "aircraft_short")]
        public string AircraftShort { get; set; }

        [DataMember(Name = "departure")]
        public string Departure { get; set; }

        [DataMember(Name = "arrival")]
        public string Arrival { get; set; }

        [DataMember(Name = "alternate")]
        public string Alternate { get; set; }

        [DataMember(Name = "cruise_tas")]
        public string CruiseTrueAirspeedKnots { get; set; }

        [DataMember(Name = "altitude")]
        public string AltitudeFeet { get; set; }

        [DataMember(Name = "deptime")]
        public string DepartureTime { get; set; }

        [DataMember(Name = "enroute_time")]
        public string EnrouteTime { get; set; }

        [DataMember(Name = "fuel_time")]
        public string FuelTime { get; set; }

        [DataMember(Name = "remarks")]
        public string Remarks { get; set; }

        [DataMember(Name = "route")]
        public string Route { get; set; }

        [DataMember(Name = "revision_id")]
        public int RevisionId { get; set; }

        [DataMember(Name = "assigned_transponder")]
        public string AssignedTransponder { get; set; }

        public static VatsimDataV3FlightPlan CopyFrom(VatsimDataV3FlightPlan copyFrom)
        {
            VatsimDataV3FlightPlan result = null;

            if(copyFrom != null) {
                result = new() {
                    FlightRules =               copyFrom.FlightRules,
                    Aircraft =                  copyFrom.Aircraft,
                    AircraftFAA =               copyFrom.AircraftFAA,
                    AircraftShort =             copyFrom.AircraftShort,
                    Departure =                 copyFrom.Departure,
                    Arrival =                   copyFrom.Arrival,
                    Alternate =                 copyFrom.Alternate,
                    CruiseTrueAirspeedKnots =   copyFrom.CruiseTrueAirspeedKnots,
                    AltitudeFeet =              copyFrom.AltitudeFeet,
                    DepartureTime =             copyFrom.DepartureTime,
                    EnrouteTime =               copyFrom.EnrouteTime,
                    FuelTime =                  copyFrom.FuelTime,
                    Remarks =                   copyFrom.Remarks,
                    Route =                     copyFrom.Route,
                    RevisionId =                copyFrom.RevisionId,
                    AssignedTransponder =       copyFrom.AssignedTransponder,
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(VatsimDataV3FlightPlan)} {{" +
                $" FlightRules: {FlightRules}" +
                $" Aircraft: {Aircraft}" +
                $" AircraftFAA: {AircraftFAA}" +
                $" AircraftShort: {AircraftShort}" +
                $" Departure: {Departure}" +
                $" Arrival: {Arrival}" +
                $" Alternate: {Alternate}" +
                $" CruiseTrueAirspeedKnots: {CruiseTrueAirspeedKnots}" +
                $" AltitudeFeet: {AltitudeFeet}" +
                $" DepartureTime: {DepartureTime}" +
                $" EnrouteTime: {EnrouteTime}" +
                $" FuelTime: {FuelTime}" +
                $" Remarks: {Remarks}" +
                $" Route: {Route}" +
                $" RevisionId: {RevisionId}" +
                $" AssignedTransponder: {AssignedTransponder}" +
                " }";
        }
    }
}
