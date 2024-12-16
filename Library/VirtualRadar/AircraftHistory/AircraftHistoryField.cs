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
        LocationReceivedUtc =       14,
        IsTisb =                    15,
        GroundTrackDegrees =        16,
        GroundTrackIsHeading =      17,
        TargetHeadingDegrees =      18,
        VerticalRateFeetPerMinute = 19,

        VerticalRateType =          20,
        Squawk =                    21,
        SquawkIsEmergency =         22,
        IdentActive =               23,
        TransponderType =           24,
        Registration =              25,
        Country =                   26,
        EnginePlacement =           27,
        EngineType =                28,
        Icao24Country =             29,

        IsCharterFlight =           30,
        IsMilitary =                31,
        IsPositioningFlight =       32,
        ModelIcao =                 33,
        Manufacturer =              34,
        Model =                     35,
        NumberOfEngines =           36,
        OperatorIcao =              37,
        Operator =                  38,
        AircraftPicture =           39,

        Serial =                    40,
        Species =                   41,
        ConstructionNumber =        42,
        UserNotes =                 43,
        UserTag =                   44,
        WakeTurbulenceCategory =    45,
        YearBuilt =                 46,
        Route =                     47,

        CountFields = Route + 1,
  }
}
