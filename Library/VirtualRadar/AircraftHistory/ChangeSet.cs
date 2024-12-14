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
using VirtualRadar.StandingData;

namespace VirtualRadar.AircraftHistory
{
    public class ChangeSet
    {
        private List<IChangedValue> _ChangedValues = [];
        public IReadOnlyList<IChangedValue> ChangedValues => _ChangedValues;

        public long Stamp { get; }

        public DateTime Utc { get; private set; }

        public bool Locked { get; private set; }

        public bool Changed => _ChangedValues.Count > 0;

        public ChangeSet(long stamp, DateTime utc)
        {
            Stamp = stamp;
            Utc = utc;
        }

        public void Lock() => Locked = true;

        /**
var regex = new Regex(@"public\s+StampedValue<(?<type>[^>]+)>\s+(?<name>\w+)\s*{", RegexOptions.Compiled);
foreach(var line in File.ReadAllLines(@"<PATH TO SOURCE ROOT>\VRSCore\Library\VirtualRadar\Aircraft.cs")) {
    var match = regex.Match(line);
    if(match.Success) {
        var type = match.Groups["type"].Value;
        var name = match.Groups["name"].Value;
        if(name != "LookupAgeUtc") {
            Console.WriteLine($"        public {type} {name} => GetField<{type}>(AircraftHistoryField.{name})?.NewValue;");
            Console.WriteLine();
        }
    }
}
        **/

        public Icao24? Icao24 => GetField<Icao24?>(AircraftHistoryField.Icao24)?.NewValue;

        public int? SignalLevel => GetField<int?>(AircraftHistoryField.SignalLevel)?.NewValue;

        public bool? SignalLevelSent => GetField<bool?>(AircraftHistoryField.SignalLevelSent)?.NewValue;

        public string Callsign => GetField<string>(AircraftHistoryField.Callsign)?.NewValue;

        public bool? CallsignIsSuspect => GetField<bool?>(AircraftHistoryField.CallsignIsSuspect)?.NewValue;

        public int? AltitudePressureFeet => GetField<int?>(AircraftHistoryField.AltitudePressureFeet)?.NewValue;

        public int? AltitudeRadarFeet => GetField<int?>(AircraftHistoryField.AltitudeRadarFeet)?.NewValue;

        public AltitudeType? AltitudeType => GetField<AltitudeType?>(AircraftHistoryField.AltitudeType)?.NewValue;

        public bool? OnGround => GetField<bool?>(AircraftHistoryField.OnGround)?.NewValue;

        public float? AirPressureInHg => GetField<float?>(AircraftHistoryField.AirPressureInHg)?.NewValue;

        public int? TargetAltitudeFeet => GetField<int?>(AircraftHistoryField.TargetAltitudeFeet)?.NewValue;

        public float? GroundSpeedKnots => GetField<float?>(AircraftHistoryField.GroundSpeedKnots)?.NewValue;

        public SpeedType? GroundSpeedType => GetField<SpeedType?>(AircraftHistoryField.GroundSpeedType)?.NewValue;

        public Location Location => GetField<Location>(AircraftHistoryField.Location)?.NewValue;

        public bool? IsTisb => GetField<bool?>(AircraftHistoryField.IsTisb)?.NewValue;

        public float? GroundTrackDegrees => GetField<float?>(AircraftHistoryField.GroundTrackDegrees)?.NewValue;

        public bool? GroundTrackIsHeading => GetField<bool?>(AircraftHistoryField.GroundTrackIsHeading)?.NewValue;

        public float? TargetHeadingDegrees => GetField<float?>(AircraftHistoryField.TargetHeadingDegrees)?.NewValue;

        public int? VerticalRateFeetPerMinute => GetField<int?>(AircraftHistoryField.VerticalRateFeetPerMinute)?.NewValue;

        public AltitudeType? VerticalRateType => GetField<AltitudeType?>(AircraftHistoryField.VerticalRateType)?.NewValue;

        public int? Squawk => GetField<int?>(AircraftHistoryField.Squawk)?.NewValue;

        public bool? SquawkIsEmergency => GetField<bool?>(AircraftHistoryField.SquawkIsEmergency)?.NewValue;

        public bool? IdentActive => GetField<bool?>(AircraftHistoryField.IdentActive)?.NewValue;

        public TransponderType? TransponderType => GetField<TransponderType?>(AircraftHistoryField.TransponderType)?.NewValue;

        public string Registration => GetField<string>(AircraftHistoryField.Registration)?.NewValue;

        public string Country => GetField<string>(AircraftHistoryField.Country)?.NewValue;

        public EnginePlacement? EnginePlacement => GetField<EnginePlacement?>(AircraftHistoryField.EnginePlacement)?.NewValue;

        public EngineType? EngineType => GetField<EngineType?>(AircraftHistoryField.EngineType)?.NewValue;

        public string Icao24Country => GetField<string>(AircraftHistoryField.Icao24Country)?.NewValue;

        public bool? IsCharterFlight => GetField<bool?>(AircraftHistoryField.IsCharterFlight)?.NewValue;

        public bool? IsMilitary => GetField<bool?>(AircraftHistoryField.IsMilitary)?.NewValue;

        public bool? IsPositioningFlight => GetField<bool?>(AircraftHistoryField.IsPositioningFlight)?.NewValue;

        public string ModelIcao => GetField<string>(AircraftHistoryField.ModelIcao)?.NewValue;

        public string Manufacturer => GetField<string>(AircraftHistoryField.Manufacturer)?.NewValue;

        public string Model => GetField<string>(AircraftHistoryField.Model)?.NewValue;

        public string NumberOfEngines => GetField<string>(AircraftHistoryField.NumberOfEngines)?.NewValue;

        public string OperatorIcao => GetField<string>(AircraftHistoryField.OperatorIcao)?.NewValue;

        public string Operator => GetField<string>(AircraftHistoryField.Operator)?.NewValue;

        public LookupImageFile AircraftPicture => GetField<LookupImageFile>(AircraftHistoryField.AircraftPicture)?.NewValue;

        public string Serial => GetField<string>(AircraftHistoryField.Serial)?.NewValue;

        public Species? Species => GetField<Species?>(AircraftHistoryField.Species)?.NewValue;

        public string ConstructionNumber => GetField<string>(AircraftHistoryField.ConstructionNumber)?.NewValue;

        public string UserNotes => GetField<string>(AircraftHistoryField.UserNotes)?.NewValue;

        public string UserTag => GetField<string>(AircraftHistoryField.UserTag)?.NewValue;

        public WakeTurbulenceCategory? WakeTurbulenceCategory => GetField<WakeTurbulenceCategory?>(AircraftHistoryField.WakeTurbulenceCategory)?.NewValue;

        public int? YearBuilt => GetField<int?>(AircraftHistoryField.YearBuilt)?.NewValue;

        public Route Route => GetField<Route>(AircraftHistoryField.Route)?.NewValue;

        /** END OF GetField()-BASED PROPERTIES **/

        public bool Set<T>(AircraftHistoryField historyField, StampedValue<T> stampedValue, T candidateValue)
        {
            AssertUnlocked();
            var isDifferent = stampedValue.Set(candidateValue, Stamp);
            if(isDifferent) {
                _ChangedValues.Add(new ChangedValue<T>(historyField, stampedValue.Value));
            }
            return isDifferent;
        }

        public bool SetIfNotDefault<T>(AircraftHistoryField historyField, StampedValue<T> stampedValue, T candidateValue)
        {
            AssertUnlocked();
            var isDifferent = stampedValue.SetIfNotDefault(candidateValue, Stamp);
            if(isDifferent) {
                _ChangedValues.Add(new ChangedValue<T>(historyField, stampedValue.Value));
            }
            return isDifferent;
        }

        public bool SetIfNotDefault<T, TRaw>(AircraftHistoryField historyField, StampedValue<T> stampedValue, TRaw candidateValue, Func<TRaw, T> toValue)
        {
            AssertUnlocked();
            var isDifferent = stampedValue.SetIfNotDefault(candidateValue, Stamp, toValue);
            if(isDifferent) {
                _ChangedValues.Add(new ChangedValue<T>(historyField, stampedValue.Value));
            }
            return isDifferent;
        }

        public void EnsureLaterThan(DateTime utc)
        {
            AssertUnlocked();
            if(Utc < utc) {
                Utc = utc.AddTicks(1);
            }
        }

        public bool ContainsChangesTo(params AircraftHistoryField[] fields)
        {
            return fields?.Length > 0
                && _ChangedValues.Any(change => fields.Contains(change.Field));
        }

        private void AssertUnlocked()
        {
            if(Locked) {
                throw new InvalidOperationException($"Cannot alter {nameof(ChangeSet)} {Stamp}:{Utc}, it is locked");
            }
        }

        private ChangedValue<T> GetField<T>(AircraftHistoryField field)
        {
            ChangedValue<T> result = null;

            for(var idx = 0;idx < _ChangedValues.Count;++idx) {
                if(_ChangedValues[idx].Field == field) {
                    result = (ChangedValue<T>)_ChangedValues[idx];
                    break;
                }
            }

            return result;
        }
    }
}
