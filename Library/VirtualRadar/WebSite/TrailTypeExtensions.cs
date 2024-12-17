// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.AircraftHistory;

namespace VirtualRadar.WebSite
{
    public static class TrailTypeExtensions
    {
        public static TrailType TrailTypeFromCode(string trFmt)
        {
            var result = TrailType.None;

            if(!String.IsNullOrEmpty(trFmt)) {
                switch(trFmt.ToUpperInvariant()) {
                    case "F":   result = TrailType.Full; break;
                    case "FA":  result = TrailType.FullAltitude; break;
                    case "FS":  result = TrailType.FullSpeed; break;
                    case "S":   result = TrailType.Short; break;
                    case "SA":  result = TrailType.ShortAltitude; break;
                    case "SS":  result = TrailType.ShortSpeed; break;
                }
            }

            return result;
        }

        public static bool IsFullTrail(this TrailType trailType)
        {
            switch(trailType) {
                case TrailType.Full:
                case TrailType.FullAltitude:
                case TrailType.FullSpeed:
                    return true;
            }
            return false;
        }

        public static bool IsShortTrail(this TrailType trailType)
        {
            switch(trailType) {
                case TrailType.Short:
                case TrailType.ShortAltitude:
                case TrailType.ShortSpeed:
                    return true;
            }
            return false;
        }

        public static bool IncludesAltitude(this TrailType trailType) => trailType == TrailType.FullAltitude || trailType == TrailType.ShortAltitude;

        public static bool IncludesSpeed(this TrailType trailType) => trailType == TrailType.FullSpeed || trailType == TrailType.ShortSpeed;

        public static string ToAircraftListTrailType(this TrailType trailType)
        {
            switch(trailType) {
                case TrailType.Full:
                case TrailType.Short:           return "";
                case TrailType.FullAltitude:
                case TrailType.ShortAltitude:   return "a";
                case TrailType.FullSpeed:
                case TrailType.ShortSpeed:      return "s";
                default:                        return null;
            }
        }

        private static readonly AircraftHistoryField[] _FullPositions = [
            AircraftHistoryField.Location,
            AircraftHistoryField.GroundTrackDegrees,
        ];

        private static readonly AircraftHistoryField[] _FullPositionsAndAltitude = [
            AircraftHistoryField.Location,
            AircraftHistoryField.GroundTrackDegrees,
            AircraftHistoryField.AltitudePressureFeet,
        ];

        private static readonly AircraftHistoryField[] _FullPositionsAndSpeed = [
            AircraftHistoryField.Location,
            AircraftHistoryField.GroundTrackDegrees,
            AircraftHistoryField.GroundSpeedKnots,
        ];

        private static readonly AircraftHistoryField[] _ShortPositions = [
            AircraftHistoryField.Location,
        ];

        private static readonly AircraftHistoryField[] _ShortPositionsAndAltitude = [
            AircraftHistoryField.Location,
            AircraftHistoryField.AltitudePressureFeet,
        ];

        private static readonly AircraftHistoryField[] _ShortPositionsAndSpeed = [
            AircraftHistoryField.Location,
            AircraftHistoryField.GroundSpeedKnots,
        ];

        public static AircraftHistoryField[] ToAircraftHistoryFields(this TrailType trailType, bool useRadarAltitude)
        {
            switch(trailType) {
                case TrailType.Full:            return _FullPositions;
                case TrailType.FullAltitude:    return _FullPositionsAndAltitude;
                case TrailType.FullSpeed:       return _FullPositionsAndSpeed;
                case TrailType.Short:           return _ShortPositions;
                case TrailType.ShortAltitude:   return _ShortPositionsAndAltitude;
                case TrailType.ShortSpeed:      return _ShortPositionsAndSpeed;
                default:                        return [];
            }
        }
    }
}
