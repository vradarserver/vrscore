﻿// Copyright © 2010 onwards, Andrew Whewell
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
    /// An immutable object that describes a point where an aircraft was seen and the time at which its
    /// position was recorded.
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// Gets the time that the aircraft was at this coordinate.
        /// </summary>
        public long Tick { get; }

        /// <summary>
        /// Gets the aircraft's <see cref="Aircraft.Stamp"/> that was current when the coordinate was
        /// recorded.
        /// </summary>
        public long Stamp { get; }

        /// <summary>
        /// Gets the latitude that the aircraft was at.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the longitude that the aircraft was at.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Gets the direction the aircraft was pointing in.
        /// </summary>
        public float? GroundTrackDegrees { get; }

        /// <summary>
        /// Gets the altitude of the aircraft.
        /// </summary>
        public int? AltitudeFeet { get; private set; }

        /// <summary>
        /// Gets the ground speed of the aircraft.
        /// </summary>
        public float? GroundSpeedKnots { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public Coordinate(double latitude, double longitude)
            : this(0L, 0L, latitude, longitude, null, null, null)
        {;}

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="stamp">
        /// The <see cref="Aircraft.Stamp"/> current when the coordinate was recorded.
        /// </param>
        /// <param name="tick">The time the coordinate was recorded.</param>
        /// <param name="latitude">The latitude of the aircraft.</param>
        /// <param name="longitude">The longitude of the aircraft.</param>
        /// <param name="heading">
        /// The heading in degrees from north that the aircraft was pointing in, if known.
        /// </param>
        public Coordinate(
            long stamp,
            long tick,
            double latitude,
            double longitude,
            float? heading
        ) : this(stamp, tick, latitude, longitude, heading, null, null)
        {;}

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="stamp">
        /// The <see cref="Aircraft.Stamp"/> current when the coordinate was recorded.
        /// </param>
        /// <param name="tick">The time the coordinate was recorded.</param>
        /// <param name="latitude">The latitude of the aircraft.</param>
        /// <param name="longitude">The longitude of the aircraft.</param>
        /// <param name="groundTrackDegrees">
        /// The heading in degrees from north that the aircraft was pointing in, if known.
        /// </param>
        /// <param name="altitudeFeet">The altitude of the aircraft.</param>
        /// <param name="groundSpeedKnots">The speed of the aircraft.</param>
        public Coordinate(
            long stamp,
            long tick,
            double latitude,
            double longitude,
            float? groundTrackDegrees,
            int? altitudeFeet,
            float? groundSpeedKnots
        )
        {
            Stamp = stamp;
            Tick = tick;
            Latitude = latitude;
            Longitude = longitude;
            GroundTrackDegrees = groundTrackDegrees;
            AltitudeFeet = altitudeFeet;
            GroundSpeedKnots = groundSpeedKnots;
        }

        /// <summary>
        /// Returns true if the other object represents the same coordinate as this one. Two coordinates are
        /// equal if their <see cref="Latitude"/>, <see cref="Longitude"/>, <see cref="Altitude"/> and <see
        /// cref="GroundSpeed"/> are equal - the other properties are not considered.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Coordinate other) {
                result = other != null && 
                         other.Latitude == Latitude &&
                         other.Longitude == Longitude &&
                         other.AltitudeFeet == AltitudeFeet &&
                         other.GroundTrackDegrees == GroundTrackDegrees;
            }

            return result;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Latitude.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(Coordinate)} {{" +
                $" Stamp: {Stamp}" +
                $" Tick: {Tick}" +
                $" Lat: {Latitude}" +
                $" Lng: {Longitude}" +
                $" Track: {GroundTrackDegrees}°" +
                $" Alt: {AltitudeFeet}'" +
                $" Spd: {GroundSpeedKnots} kts" +
                " }";
        }
    }
}
