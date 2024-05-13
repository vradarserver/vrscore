// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics.CodeAnalysis;

namespace VirtualRadar
{
    /// <summary>
    /// A WGS84 coordinate.
    /// </summary>
    public struct Location
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

        /// <inheritdoc/>
        public static bool operator ==(Location lhs, Location rhs)
        {
            return lhs.Latitude == rhs.Latitude
                && (lhs.Longitude == rhs.Longitude || (lhs.IsAntiMeridian && rhs.IsAntiMeridian));
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

        /// <inheritdoc/>
        public override string ToString() => $"{Latitude:0.000000} / {Longitude:0.000000}";

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is Location other
                && this == other;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Latitude.GetHashCode();
    }
}
