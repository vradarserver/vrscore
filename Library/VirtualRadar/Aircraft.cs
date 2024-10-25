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
    /// <remarks>
    /// Aircraft lists must not expose live aircraft objects. Clone the aircraft before returning it.
    /// </remarks>
    public class Aircraft
    {
        private readonly object _SyncLock = new();

        /// <summary>
        /// The unique identifier of the aircraft. For most feeds this will be the <see cref="Icao24"/> but
        /// for synthesised feeds, or sources that are not tracking real aircraft, this could be any number
        /// that uniquely distinguishes one aircraft from another. It is established on receipt of the first
        /// message and never changes.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// A value indicating when the aircraft was last changed. See <see cref="PostOffice"/> for the rules
        /// about how this value is established, and what you can infer from it.
        /// </summary>
        public long Stamp { get; private set; }

        private void SetStamp(long newStamp)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(newStamp, Stamp);
            Stamp = newStamp;
        }

        //
        //                                  TRANSMITTED INFORMATION
        //

        public DateTime FirstMessageReceivedUtc { get; private set; }

        public StampedTimedValue<long> CountMessagesReceived { get; } = new();

        public StampedValue<Icao24> Icao24 { get; } = new();

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

        //
        //                              LOOKUP OUTCOME(S)
        //

        public StampedValue<DateTime> LookupAgeUtc { get; } = new();

        public StampedValue<string> Registration { get; } = new();

        public StampedValue<string> Country { get; } = new();

        public StampedValue<string> ModelIcao { get; } = new();

        public StampedValue<string> Manufacturer { get; } = new();

        public StampedValue<string> Model { get; } = new();

        public StampedValue<string> OperatorIcao { get; } = new();

        public StampedValue<string> Operator { get; } = new();

        public StampedValue<string> Serial { get; } = new();

        public StampedValue<int?> YearFirstFlight { get; } = new();


        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="id"></param>
        public Aircraft(uint id)
        {
            Id = id;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(Aircraft)} {{"
                 + $" nameof(Id): 0x{Id:X}"
                 + $" nameof(Icao24): {Icao24}"
                 + $" nameof(Stamp): {Stamp}"
                 + " ... }";
        }

        /// <summary>
        /// Creates a shallow copy of the aircraft.
        /// </summary>
        /// <returns></returns>
        public Aircraft ShallowCopy()
        {
            lock(_SyncLock) {
                var result = new Aircraft(Id) {
                    Stamp =                   Stamp,
                    FirstMessageReceivedUtc = FirstMessageReceivedUtc,
                };

                // TRANSMITTED VALUES
                AirPressureInHg             .CopyTo(result.AirPressureInHg);
                AltitudeFeet                .CopyTo(result.AltitudeFeet);
                AltitudeType                .CopyTo(result.AltitudeType);
                Callsign                    .CopyTo(result.Callsign);
                CountMessagesReceived       .CopyTo(result.CountMessagesReceived);
                GroundSpeedKnots            .CopyTo(result.GroundSpeedKnots);
                GroundSpeedType             .CopyTo(result.GroundSpeedType);
                GroundTrackDegrees          .CopyTo(result.GroundTrackDegrees);
                GroundTrackIsHeading        .CopyTo(result.GroundTrackIsHeading);
                Icao24                      .CopyTo(result.Icao24);
                IdentActive                 .CopyTo(result.IdentActive);
                IsTisb                      .CopyTo(result.IsTisb);
                Location                    .CopyTo(result.Location);
                SignalLevel                 .CopyTo(result.SignalLevel);
                Squawk                      .CopyTo(result.Squawk);
                TargetAltitudeFeet          .CopyTo(result.TargetAltitudeFeet);
                TargetHeadingDegrees        .CopyTo(result.TargetHeadingDegrees);
                VerticalRateType            .CopyTo(result.VerticalRateType);
                VerticalRateFeetPerMinute   .CopyTo(result.VerticalRateFeetPerMinute);

                // LOOKED-UP VALUES
                Country                     .CopyTo(result.Country);
                LookupAgeUtc                .CopyTo(result.LookupAgeUtc);
                Manufacturer                .CopyTo(result.Manufacturer);
                Model                       .CopyTo(result.Model);
                ModelIcao                   .CopyTo(result.ModelIcao);
                Operator                    .CopyTo(result.Operator);
                OperatorIcao                .CopyTo(result.OperatorIcao);
                Registration                .CopyTo(result.Registration);
                Serial                      .CopyTo(result.Serial);
                YearFirstFlight             .CopyTo(result.YearFirstFlight);

                return result;
            }
        }

        /// <summary>
        /// Sets aircraft values from the message passed across.
        /// </summary>
        /// <param name="message"></param>
        public bool CopyFromMessage(TransponderMessage message)
        {
            var changed = false;

            if(message != null) {
                if(message.AircraftId != Id) {
                    throw new ArgumentException(
                        $"An attempt was made to assign values transmitted from Id 0x{message.AircraftId:X} " +
                        $"to the aircraft object for 0x{Id:X}"
                    );
                }

                lock(_SyncLock) {
                    if(FirstMessageReceivedUtc == default) {
                        changed = true;
                        FirstMessageReceivedUtc = DateTime.UtcNow;          // <-- as of .NET 8 this is still the fastest UTC function, faster that DateTimeOffset.UtcNow.
                    }

                    var stamp = PostOffice.GetStamp();

                    // TRANSMITTED VALUES
                    changed = AltitudeFeet              .SetIfNotDefault(   message.AltitudeFeet, stamp)                || changed;
                    changed = AltitudeType              .Set(               message.AltitudeType, stamp)                || changed;
                    changed = Callsign                  .SetIfNotDefault(   message.Callsign, stamp)                    || changed;
                    changed = GroundSpeedKnots          .SetIfNotDefault(   message.GroundSpeedKnots, stamp)            || changed;
                    changed = GroundSpeedType           .Set(               message.GroundSpeedType, stamp)             || changed;
                    changed = GroundTrackDegrees        .SetIfNotDefault(   message.GroundTrackDegrees, stamp)          || changed;
                    changed = GroundTrackIsHeading      .Set(               message.GroundTrackIsHeading, stamp)        || changed;
                    changed = Icao24                    .Set(               message.Icao24, stamp)                      || changed;
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

        /// <summary>
        /// Sets aircraft details from the successful lookup outcome passed across.
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool CopyFromLookup(LookupOutcome lookup)
        {
            var changed = false;

            if(lookup?.Success ?? false) {
                lock(_SyncLock) {
                    var stamp = PostOffice.GetStamp();

                    // LOOKED-UP VALUES
                    changed = Country           .SetIfNotDefault(lookup.Country, stamp)         || changed;
                    changed = LookupAgeUtc      .SetIfNotDefault(lookup.SourceAgeUtc, stamp)    || changed;
                    changed = Manufacturer      .SetIfNotDefault(lookup.Manufacturer, stamp)    || changed;
                    changed = Model             .SetIfNotDefault(lookup.Model, stamp)           || changed;
                    changed = ModelIcao         .SetIfNotDefault(lookup.ModelIcao, stamp)       || changed;
                    changed = Operator          .SetIfNotDefault(lookup.Operator, stamp)        || changed;
                    changed = OperatorIcao      .SetIfNotDefault(lookup.OperatorIcao, stamp)    || changed;
                    changed = Registration      .SetIfNotDefault(lookup.Registration, stamp)    || changed;
                    changed = Serial            .SetIfNotDefault(lookup.Serial, stamp)          || changed;
                    changed = YearFirstFlight   .SetIfNotDefault(lookup.YearFirstFlight, stamp) || changed;

                    if(changed) {
                        SetStamp(stamp);
                    }
                }
            }

            return changed;
        }
    }
}
