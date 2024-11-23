// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.StandingData;

namespace VirtualRadar.Message
{
    /// <summary>
    /// Describes the outcome of a lookup of aircraft detail.
    /// </summary>
    public class LookupOutcome
    {
        /// <summary>
        /// True if information could be found for it.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// How long ago was this information established - e.g. if this is from a cache
        /// of online lookups then this would be the date and time that the lookup was
        /// cached. This is a UTC timestamp.
        /// </summary>
        public DateTime SourceAgeUtc { get; set; }

        public string Registration { get; set; }

        public string ConstructionNumber { get; set; }

        public string Country { get; set; }

        public EnginePlacement? EnginePlacement { get; set; }

        public EngineType? EngineType { get; set; }

        public string ModelIcao { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Icao24Country { get; set; }

        public bool? IsCharterFlight { get; set; }

        public bool? IsMilitary { get; set; }

        public bool? IsPositioningFlight { get; set; }

        public string NumberOfEngines { get; set; }

        public string OperatorIcao { get; set; }

        public string Operator { get; set; }

        public LookupImageFile AircraftPicture { get; set; }

        public Route Route { get; set; }

        public string Serial { get; set; }

        public string UserNotes { get; set; }

        public string UserTag { get; set; }

        public int? YearFirstFlight { get; set; }

        /// <summary>
        /// True if this lookup was an attempt to lookup the aircraft's air pressure. Always set this
        /// for air pressure lookups, even if the attempt was unsuccessful.
        /// </summary>
        public bool? AirPressureLookupAttempted { get; set; }

        /// <summary>
        /// The air pressure in inches of mercury at sea level at the aircraft's current location.
        /// </summary>
        public float? AirPressureInHg { get; set; }
    }
}
