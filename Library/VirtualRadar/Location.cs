// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar
{
    /// <summary>
    /// A WGS84 coordinate.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// A location whose latitude and longitude is both zero. Typically used to indicate a missing
        /// location in objects that don't allow null locations.
        /// </summary>
        public static readonly Location Zero = new(0.0, 0.0);

        /// <summary>
        /// The latitude, ranging across 180 degress from +90 (north pole) to 0 (equator) to -90 (south pole).
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The longitude, ranging across 360 degrees from 0 (the prime meridian) to -180 in the west (the
        /// anti-meridian) and +180 in the east (the anti-meridian). -180 and +180 are the same line of
        /// longitude.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// True if <see cref="Longitude"/> is either +180 or -180.
        /// </summary>
        public bool IsAntiMeridian => Longitude == 180.0 || Longitude == -180.0;

        /// <summary>
        /// Returns the longitude as a value between 0 and 360, where -0 to -180 translates to 180 to 360.
        /// </summary>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public double LinearLongitude => Convert.Longitude.ToLinear(Longitude);

        /// <inheritdoc/>
        public static bool operator ==(Location lhs, Location rhs)
        {
            var lhsNull = lhs is null;
            var rhsNull = rhs is null;
            return (lhsNull && rhsNull)
                || (!lhsNull && !rhsNull && lhs.Equals(rhs));
        }

        /// <inheritdoc/>
        public static bool operator!=(Location lhs, Location rhs) => !(lhs == rhs);

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Creates a new Location unless either latitude or longitude are null,
        /// in which case it returns null.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static Location FromNullable(double? latitude, double? longitude)
        {
            return latitude == null || longitude == null
                ? null
                : new(latitude.Value, longitude.Value);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Latitude:0.000000} / {Longitude:0.000000}";

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Location other) {
                result = Latitude == other.Latitude
                      && (Longitude == other.Longitude || (IsAntiMeridian && other.IsAntiMeridian));
            }

            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

        /// <summary>
        /// Returns a location from the parts passed across. If either latitude or
        /// longitude are null then null is returned.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="zeroZeroIsNull"></param>
        /// <returns></returns>
        public static Location FromLatLng(double? latitude, double? longitude, bool zeroZeroIsNull = false)
        {
            return (latitude == null || longitude == null || (latitude == 0.0 && longitude == 0.0 && zeroZeroIsNull))
                ? null
                : new(latitude.Value, longitude.Value);
        }
    }
}
