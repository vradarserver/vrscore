// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualRadar.Database.EntityFramework.StandingData.Entities
{
    class Airport
    {
        [Key]
        public long AirportId { get; set; }

        [MaxLength(4)]
        public string Icao { get; set; }

        [MaxLength(3)]
        public string Iata { get; set; }

        [MaxLength(80)]
        public string Name { get; set; }

        [MaxLength(80), Column("Location")]
        public string Town { get; set; }

        public long CountryId { get; set; }

        public Country Country { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        private Location _Location;
        private double? _LocationLatitude;
        private double? _LocationLongitude;
        [NotMapped]
        public Location Location
        {
            get {
                var result = _Location;
                if(_LocationLatitude != Latitude || _LocationLongitude != Longitude) {
                    _LocationLatitude = Latitude;
                    _LocationLongitude = Longitude;
                    result = Location.FromNullable(_LocationLatitude, _LocationLongitude);
                    _Location = result;
                }
                return result;
            }
        }

        public int? Altitude { get; set; }

        public VirtualRadar.StandingData.Airport ToAirport() => new() {
            AltitudeFeet =      Altitude,
            Country =           Country?.Name ?? "",
            IataCode =          Iata,
            IcaoCode =          Icao,
            Location =          Location.FromNullable(Latitude, Longitude),
            Name =              Name,
        };
    }
}
