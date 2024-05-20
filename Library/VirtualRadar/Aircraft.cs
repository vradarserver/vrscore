// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Message;

namespace VirtualRadar
{
    /// <summary>
    /// Describes an aircraft.
    /// </summary>
    public class Aircraft
    {
        private object _SyncLock = new();

        /// <summary>
        /// A marker indicating when the aircraft was last changed. This value is
        /// only allowed to increment.
        /// </summary>
        public long Stamp { get; private set; }

        private void SetStamp(long newStamp)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(newStamp, Stamp);
            Stamp = newStamp;
        }

        /// <summary>
        /// The ICAO24 of the aircraft. This is established when the object is created and
        /// can never change.
        /// </summary>
        public Icao24 Icao24 { get; }

        public DateTimeOffset FirstMessageReceived { get; private set; }

        public StampedTimedValue<long> CountMessagesReceived { get; } = new();

        public StampedValue<int?> SignalLevel { get; } = new();

        public StampedValue<string> Callsign { get; } = new();

        public StampedValue<int?> AltitudeFeet { get; } = new();

        public StampedValue<AltitudeType> AltitudeType { get; } = new();

        public StampedTimedValue<float?> AirPressureInHg { get; } = new();

        public StampedValue<int?> TargetAltitudeFeet { get; } = new();

        public StampedValue<float?> GroundSpeedKnots { get; } = new();

        public StampedValue<SpeedType> GroundSpeedType { get; } = new();

        public StampedTimedValue<Location> Location { get; } = new();

        public StampedValue<bool> IsTisb { get; } = new();

        public StampedValue<float?> GroundTrackDegrees { get; } = new();

        public StampedValue<bool> GroundTrackIsHeading { get; } = new();

        public StampedValue<float?> TargetHeadingDegrees { get; } = new();

        public StampedValue<int?> VerticalRateFeetPerMinute { get; } = new();

        public StampedValue<AltitudeType> VerticalRateType { get; } = new();

        public StampedValue<int?> Squawk { get; } = new();

        public StampedValue<bool?> IdentActive { get; } = new();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="icao24"></param>
        public Aircraft(Icao24 icao24)
        {
            Icao24 = icao24;
        }

        /// <summary>
        /// Creates a shallow copy of the aircraft.
        /// </summary>
        /// <returns></returns>
        public Aircraft ShallowCopy()
        {
            lock(_SyncLock) {
                var result = new Aircraft(Icao24) {
                    Stamp =                 Stamp,
                    FirstMessageReceived =  FirstMessageReceived,
                };
                AirPressureInHg             .CopyTo(result.AirPressureInHg);
                AltitudeFeet                .CopyTo(result.AltitudeFeet);
                AltitudeType                .CopyTo(result.AltitudeType);
                Callsign                    .CopyTo(result.Callsign);
                CountMessagesReceived       .CopyTo(result.CountMessagesReceived);
                GroundSpeedKnots            .CopyTo(result.GroundSpeedKnots);
                GroundSpeedType             .CopyTo(result.GroundSpeedType);
                GroundTrackDegrees          .CopyTo(result.GroundTrackDegrees);
                GroundTrackIsHeading        .CopyTo(result.GroundTrackIsHeading);
                IdentActive                 .CopyTo(result.IdentActive);
                IsTisb                      .CopyTo(result.IsTisb);
                Location                    .CopyTo(result.Location);
                SignalLevel                 .CopyTo(result.SignalLevel);
                Squawk                      .CopyTo(result.Squawk);
                TargetAltitudeFeet          .CopyTo(result.TargetAltitudeFeet);
                TargetHeadingDegrees        .CopyTo(result.TargetHeadingDegrees);
                VerticalRateType            .CopyTo(result.VerticalRateType);
                VerticalRateFeetPerMinute   .CopyTo(result.VerticalRateFeetPerMinute);

                return result;
            }
        }

        /// <summary>
        /// Sets aircraft values from the message passed across.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stamp"></param>
        public bool CopyFromMessage(TransponderMessage message, long stamp)
        {
            var changed = false;

            if(message != null) {
                if(message.Icao24 != Icao24) {
                    throw new ArgumentException(
                        $"An attempt was made to assign values transmitted from ICAO {message.Icao24} " +
                        $"to the aircraft object for {message.Icao24}"
                    );
                }

                lock(_SyncLock) {
                    if(FirstMessageReceived == default) {
                        changed = true;
                        FirstMessageReceived = DateTimeOffset.Now;
                    }

                    changed = AltitudeFeet              .SetIfNotDefault(   message.AltitudeFeet, stamp)                || changed;
                    changed = AltitudeType              .Set(               message.AltitudeType, stamp)                || changed;
                    changed = Callsign                  .SetIfNotDefault(   message.Callsign, stamp)                    || changed;
                    changed = GroundSpeedKnots          .SetIfNotDefault(   message.GroundSpeedKnots, stamp)            || changed;
                    changed = GroundSpeedType           .Set(               message.GroundSpeedType, stamp)             || changed;
                    changed = GroundTrackDegrees        .SetIfNotDefault(   message.GroundTrackDegrees, stamp)          || changed;
                    changed = GroundTrackIsHeading      .Set(               message.GroundTrackIsHeading, stamp)        || changed;
                    changed = IdentActive               .SetIfNotDefault(   message.IdentActive, stamp)                 || changed;
                    changed = IsTisb                    .Set(               message.IsTisb, stamp)                      || changed;
                    changed = Location                  .SetIfNotDefault(   message.Location, stamp)                    || changed;
                    changed = SignalLevel               .SetIfNotDefault(   message.SignalLevel, stamp)                 || changed;
                    changed = Squawk                    .SetIfNotDefault(   message.Squawk, stamp)                      || changed;
                    changed = TargetAltitudeFeet        .SetIfNotDefault(   message.TargetAltitudeFeet, stamp)          || changed;
                    changed = TargetHeadingDegrees      .SetIfNotDefault(   message.TargetHeadingDegrees, stamp)        || changed;
                    changed = VerticalRateType          .Set(               message.VerticalRateType, stamp)            || changed;
                    changed = VerticalRateFeetPerMinute .SetIfNotDefault(   message.VerticalRateFeetPerMinute, stamp)   || changed;

                    if(changed) {
                        SetStamp(stamp);
                        CountMessagesReceived.Set(CountMessagesReceived + 1, stamp);
                    }
                }
            }

            return changed;
        }
    }
}
