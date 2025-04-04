﻿// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Message;
using VirtualRadar.StandingData;

namespace VirtualRadar.Feed.Vatsim
{
    /// <summary>
    /// Lookup information that we would like to persist between refreshes of VATSIM state data
    /// because it can be a bit expensive to keep looking it up.
    /// </summary>
    public class PilotState
    {
        /// <summary>
        /// The generation that the state was built from.
        /// </summary>
        public long Generation { get; set; }

        /// <summary>
        /// The transponder message built from the pilot information.
        /// </summary>
        public TransponderMessage TransponderMessage { get; set; }

        /// <summary>
        /// The lookup outcome built from the VATSIM download.
        /// </summary>
        public LookupByAircraftIdOutcome LookupOutcome { get; set; }

        /// <summary>
        /// The registration as sent by VATSIM.
        /// </summary>
        public string RegistrationOriginal { get; set; }

        /// <summary>
        /// The corrected registration as built from <see cref="RegistrationOriginal"/> and registration
        /// prefix data.
        /// </summary>
        public string RegistrationCorrected { get; set; }

        /// <summary>
        /// The current callsign for the aircraft.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// The current operator code for the aircraft.
        /// </summary>
        public string OperatorIcao { get; set; }

        /// <summary>
        /// The current model ICAO code for the aircraft.
        /// </summary>
        public string ModelIcao { get; set; }

        /// <summary>
        /// The route derived from the pilot's flight plan.
        /// </summary>
        public Route Route { get; set; }

        /// <summary>
        /// The origin airport used to derive <see cref="Route"/>.
        /// </summary>
        public string RouteFromAirportCode { get; set; }

        /// <summary>
        /// The destination airport used to derive <see cref="Route"/>.
        /// </summary>
        public string RouteToAirportCode { get; set; }
    }
}
