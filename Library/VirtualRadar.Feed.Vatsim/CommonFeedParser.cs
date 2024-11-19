// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;
using VirtualRadar.Feed.Vatsim.ApiModels;
using VirtualRadar.Message;
using VirtualRadar.StandingData;

namespace VirtualRadar.Feed.Vatsim
{
    /// <summary>
    /// Translates the VATSIM feed into messages on behalf of the various feeds that are being extracted from
    /// it. Each feed is consuming the same original VATSIM download, so it's more efficient for them to share
    /// the same code to parse messages and lookups generated from that download.
    /// </summary>
    public class CommonFeedParser(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IStandingDataManager _StandingDataManager
        #pragma warning restore IDE1006
    )
    {
        private readonly Dictionary<int, PilotState> _PilotStates = [];

        public long Generation { get; private set; }

        /// <summary>
        /// Increments <see cref="Generation"/>. Used to detect pilot states that have already been built for
        /// the current generation, and pilot states that are no longer required.
        /// </summary>
        public void StartNewGeneration()
        {
            ++Generation;
        }

        /// <summary>
        /// Removes state that was not accessed by the current generation.
        /// </summary>
        public void CleanupPreviousGenerations()
        {
            var removeStateIds = _PilotStates
                .Where(kvp => kvp.Value.Generation < Generation)
                .Select(kvp => kvp.Key)
                .ToArray();
            foreach(var removeId in removeStateIds) {
                _PilotStates.Remove(removeId);
            }
        }

        /// <summary>
        /// Extracts the transponder message aircraft ID from the pilot record.
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public int BuildAircraftId(VatsimDataV3Pilot pilot) => pilot.Cid;

        /// <summary>
        /// Builds the messages and lookups for the pilot passed across.
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public PilotState BuildOrReusePilotState(VatsimDataV3Pilot pilot)
        {
            if(!_PilotStates.TryGetValue(pilot.Cid, out var result) || result.Generation != Generation) {
                result ??= new();
                _PilotStates[pilot.Cid] = result;
                BuildStateForPilot(pilot, result);
            }

            return result;
        }

        private void BuildStateForPilot(VatsimDataV3Pilot pilot, PilotState pilotState)
        {
            var aircraftId = BuildAircraftId(pilot);
            var remarks = new RemarksParser(pilot.FlightPlan?.Remarks);

            var pressureAltitude = Convert.Altitude.GeometricAltitudeToPressureAltitude(
                pilot.AltitudeGeometricFeet,
                pilot.QnhMillibars,
                AirPressureUnit.Millibar,
                roundTo25FeetIncrements: true
            );

            var transponderMessage = new TransponderMessage(aircraftId) {
                AltitudeFeet =          pressureAltitude,
                AltitudeType =          AltitudeType.AirPressure,
                GroundSpeedKnots =      pilot.GroundSpeedKnots,
                GroundSpeedType =       SpeedType.GroundSpeed,
                GroundTrackDegrees =    pilot.HeadingDegrees,
                IsFakeAircraft =        true,
                Location =              new(pilot.Latitude, pilot.Longitude),
                Squawk =                pilot.Squawk,
            };
            var modeSCode = remarks.ModeSCode;
            if(modeSCode != "" && Icao24.TryParse(modeSCode, out var icao24)) {
                transponderMessage.Icao24 = icao24;
            }

            var lookupOutcome = new LookupByAircraftIdOutcome(aircraftId, success: true) {
                SourceAgeUtc =  DateTime.UtcNow,
                UserNotes =     $"{pilot.FlightPlan?.Route}\r\n{pilot.FlightPlan?.Remarks}",
                UserTag =       $"[{pilot.Cid}] {pilot.Name}",
                Country =       pilot.Server,
            };

            var registration = remarks.Registration;
            if(registration == "") {
                registration = pilotState.Registration;
            }
            var operatorIcao = remarks.OperatorIcao;
            if(operatorIcao == "") {
                operatorIcao = pilotState.OperatorIcao;
            }

            IEnumerable<Airline> airlinesForOperatorIcao = null;
            if(pilotState.Callsign != pilot.Callsign) {
                transponderMessage.Callsign = pilot.Callsign;
                if(remarks.Registration.AsciiAlphanumeric() != pilot.Callsign) {        // <-- they never seem to have dashes in their registrations to begin with, but just in case...
                    var callsign = new Callsign(pilot.Callsign);
                    airlinesForOperatorIcao = callsign.IsOriginalCallsignValid
                        ? _StandingDataManager.FindAirlinesForCode(callsign.Code)
                        : null;
                    var hasAirlineForCallsign = airlinesForOperatorIcao?.Any() ?? false;
                    if(hasAirlineForCallsign && String.IsNullOrEmpty(operatorIcao)) {
                        operatorIcao = callsign.Code;
                    }
                }
            }

            if(pilotState.OperatorIcao != operatorIcao) {
                lookupOutcome.OperatorIcao = operatorIcao;
                var airlines = airlinesForOperatorIcao
                    ?? _StandingDataManager.FindAirlinesForCode(operatorIcao);
                lookupOutcome.Operator = airlines
                    .FirstOrDefault()
                    ?.Name
                    ?? operatorIcao;
                pilotState.OperatorIcao = lookupOutcome.OperatorIcao;
            }

            if(pilotState.Registration != registration) {
                // TODO: Port across the registration fixups
                lookupOutcome.Registration = registration;
                pilotState.Registration = lookupOutcome.Registration;
            }

            pilotState.TransponderMessage = transponderMessage;
            pilotState.LookupOutcome = lookupOutcome;
            pilotState.Generation = Generation;
        }
    }
}
