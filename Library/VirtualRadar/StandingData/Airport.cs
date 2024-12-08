// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;

namespace VirtualRadar.StandingData
{
    /// <summary>
    /// Describes an airport in the standing data files.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Gets or sets the IATA code for the airport.
        /// </summary>
        public string IataCode { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the airport.
        /// </summary>
        public string IcaoCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the airport.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the town or city served by the airport.
        /// </summary>
        public string Town { get; set; }

        /// <summary>
        /// Gets or sets the country that the airport is in.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the airport's location on the surface of the earth.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet of the airport.
        /// </summary>
        public int? AltitudeFeet { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append(!String.IsNullOrEmpty(IcaoCode)
                ? IcaoCode
                : IataCode
            );

            result.AppendWithSeparator(" ", Name);

            if(!String.IsNullOrEmpty(Country)) {
                result.AppendWithSeparator(", ", Country);
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns the preferred code if present, otherwise the non-prefferred code.
        /// If both codes are missing then an empty string is returned.
        /// </summary>
        /// <param name="preferred"></param>
        /// <returns></returns>
        public string PreferredAirportCode(AirportCodeType preferred)
        {
            var result = preferred == AirportCodeType.Iata
                ? IataCode
                : IcaoCode;
            if(String.IsNullOrEmpty(result)) {
                result = preferred == AirportCodeType.Iata
                    ? IcaoCode
                    : IataCode;
            }
            return result ?? "";
        }

        /// <summary>
        /// Either the <see cref="Name"/> and <see cref="Town"/> concatenated together or,
        /// if <see cref="Name"/> already includes <see cref="Town"/>, then just the <see cref="Name"/>.
        /// </summary>
        /// <returns></returns>
        public string NameAndTown()
        {
            var result = new StringBuilder(Name ?? "");

            if(!String.IsNullOrEmpty(Town)) {
                if(   result.Length == 0
                   || (
                          Name != Town
                       && !Name.StartsWith($"{Town} ", StringComparison.InvariantCultureIgnoreCase)
                       && !Name.Contains($" {Town}", StringComparison.InvariantCultureIgnoreCase)
                    )
                ) {
                    result.AppendWithSeparator(", ", Town);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Describes the airport.
        /// </summary>
        /// <param name="preferredCode"></param>
        /// <param name="showCode"></param>
        /// <param name="showName"></param>
        /// <param name="showTown"></param>
        /// <param name="showCountry"></param>
        /// <returns></returns>
        public string Describe(
            AirportCodeType preferredCode,
            bool showCode = true,
            bool showName = true,
            bool showTown = false,
            bool showCountry = true
        )
        {
            var result = new StringBuilder();

            if(showCode) {
                result.Append(PreferredAirportCode(preferredCode));
            }
            string nameAndTown;
            if(showName && showTown && (nameAndTown = NameAndTown()) != "") {
                result.AppendWithSeparator(" ", nameAndTown);
            } else if(showName && !String.IsNullOrEmpty(Name)) {
                result.AppendWithSeparator(" ", Name);
            }
            if(showCountry && !String.IsNullOrEmpty(Country)) {
                result.AppendWithSeparator(", ", Country);
            }

            return result.ToString();
        }
    }
}
