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
    /// A pair of locations that together form a rectangle on the surface of the earth.
    /// </summary>
    public class LocationRectangle
    {
        /// <summary>
        /// A rectangle that starts and ends at 0, 0.
        /// </summary>
        public static readonly LocationRectangle Empty = new(Location.Zero, Location.Zero);

        /// <summary>
        /// Gets the top-left point of the rectangle.
        /// </summary>
        public Location TopLeft { get; }

        /// <summary>
        /// Gets the bottom-right point of the rectangle.
        /// </summary>
        public Location BottomRight { get; }

        /// <summary>
        /// Gets the westerly-most longitude of the rectangle.
        /// </summary>
        public double WesterlyLongitude => TopLeft.Longitude;

        /// <summary>
        /// Gets the easterly-most longitude of the rectangle.
        /// </summary>
        public double EasterlyLongitude => BottomRight.Longitude;

        /// <summary>
        /// Gets the northerly-most latitude of the rectangle.
        /// </summary>
        public double NortherlyLatitude => TopLeft.Latitude;

        /// <summary>
        /// Gets the southerly-most latitude of the rectangle.
        /// </summary>
        public double SoutherlyLatitude => BottomRight.Latitude;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        public LocationRectangle(Location topLeft, Location bottomRight)
        {
            ArgumentNullException.ThrowIfNull(topLeft);
            ArgumentNullException.ThrowIfNull(bottomRight);

            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="north"></param>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        public LocationRectangle(double north, double west, double south, double east)
            : this(new(north, west), new(south, east))
        {;}

        /// <summary>
        /// Calculates a location rectangle given a centre location and a width and height.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="distanceUnit"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static LocationRectangle FromCentre(Location centre, DistanceUnit distanceUnit, double width, double height)
        {
            ArgumentNullException.ThrowIfNull(centre);

            var halfWidthKm = distanceUnit.To(width / 2.0, DistanceUnit.Kilometres);
            var halfHeightKm = distanceUnit.To(height / 2.0, DistanceUnit.Kilometres);

            var topLeft = new Location(
                GreatCircleMaths.Destination(centre, 0.0,   halfHeightKm).Latitude,
                GreatCircleMaths.Destination(centre, 270.0, halfWidthKm).Longitude
            );
            var bottomRight = new Location(
                GreatCircleMaths.Destination(centre, 180.0, halfHeightKm).Latitude,
                GreatCircleMaths.Destination(centre, 90.0,  halfWidthKm).Longitude
            );

            return new(topLeft, bottomRight);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is LocationRectangle other) {
                result = TopLeft.Equals(other.TopLeft)
                      && BottomRight.Equals(other.BottomRight);
            }

            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(TopLeft, BottomRight);

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(LocationRectangle)}: {{ TopLeft: {TopLeft}, BottomRight: {BottomRight} }}";

        /// <summary>
        /// Returns true if the location passed across falls within the bounds of the rectangle.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool Contains(Location location)
        {
            return IsWithinBounds(
                location?.Latitude,
                location?.Longitude,
                TopLeft.Latitude,
                TopLeft.Longitude,
                BottomRight.Latitude,
                BottomRight.Longitude
            );
        }

        /// <summary>
        /// Returns true if the location passed across is within the 'rectangle' on the surface of the earth
        /// described by the pair of coordinates passed across.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="bottom"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool IsWithinBounds(double? latitude, double? longitude, double top, double left, double bottom, double right)
        {
            var result = latitude != null && longitude != null;

            if(result) {
                // Latitude is simple because we assume there is nothing past the poles
                result = top >= latitude && bottom <= latitude;

                if(result) {
                    // Longitude is harder because if the bounding box straddles the anti-meridian then the
                    // normal comparison of coordinates fails. When it straddles the anti-meridian the left
                    // edge is a +ve value < 180 and the right edge is a -ve value > -180. You can also end up
                    // with a left edge that is larger than the right edge (e.g. left is 170, Alaska-ish,
                    // while right is 60, Russia-ish). On top of that -180 and 180 are the same value. The
                    // easiest way to cope is to normalise all longitudes to a linear scale of angles from 0
                    // through 360 and then check that the longitude lies between the left and right.
                    //
                    // If the left degree is larger than the right degree then the bounds straddle the
                    // meridian, in which case we need to allow all longitudes from the left to 0/360 and all
                    // longitudes from 0/360 to the right. If left < right then it's easier, we just have to
                    // have a longitude between left and right.
                    //
                    // One final twist - if you zoom out enough so that you can see the entire span of the
                    // globe in one go then Google will give up and report a boundary of -180 on the left and
                    // 180 on the right... in other words, the same longitude. Still, there's not much else
                    // they can do.
                    longitude = Convert.Longitude.ToLinear(longitude.Value);
                    left = Convert.Longitude.ToLinear(left);
                    right = Convert.Longitude.ToLinear(right);

                    if(left != 180.0 || right != 180.0) {
                        if(left == right) {
                            result = longitude == left;
                        } else if(left > right) {
                            result = (longitude >= left && longitude <= 360.0)
                                  || (longitude >= 0.0 && longitude <= right);
                        } else {
                            result = longitude >= left && longitude <= right;
                        }
                    }
                }
            }

            return result;
        }
    }
}
