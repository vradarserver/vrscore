// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.AircraftHistory
{
    /// <summary>
    /// These are persisted to disk and must remain constant throughout the lifetime of the program.
    /// </summary>
    public enum AircraftHistoryField : byte
    {
        Icao24 =                    0,
        SignalLevel =               1,
        SignalLevelSent =           2,
        Callsign =                  3,
        CallsignIsSuspect =         4,
        AltitudePressureFeet =      5,
        AltitudeRadarFeet =         6,
        AltitudeType =              7,
        OnGround =                  8,
        AirPressureInHg =           9,

        TargetAltitudeFeet =        10,
        GroundSpeedKnots =          11,
        GroundSpeedType =           12,
        Location =                  13,
        IsTisb =                    14,
        GroundTrackDegrees =        15,
        GroundTrackIsHeading =      16,
        TargetHeadingDegrees =      17,
        VerticalRateFeetPerMinute = 18,
        VerticalRateType =          19,

        Squawk =                    20,
        SquawkIsEmergency =         21,
        IdentActive =               22,
        TransponderType =           23,
        Registration =              24,
        Country =                   25,
        EnginePlacement =           26,
        EngineType =                27,
        Icao24Country =             28,
        IsCharterFlight =           29,

        IsMilitary =                30,
        IsPositioningFlight =       31,
        ModelIcao =                 32,
        Manufacturer =              33,
        Model =                     34,
        NumberOfEngines =           35,
        OperatorIcao =              36,
        Operator =                  37,
        AircraftPicture =           38,
        Serial =                    39,

        Species =                   40,
        ConstructionNumber =        41,
        UserNotes =                 42,
        UserTag =                   43,
        WakeTurbulenceCategory =    44,
        YearBuilt =                 45,
        Route =                     46,

        CountFields = Route + 1,
  }
}
