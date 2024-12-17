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

namespace VirtualRadar.Feed.BaseStation
{
    /// <summary>
    /// Converts between <see cref="BaseStationMessage"/>s and <see cref="TransponderMessage"/>s.
    /// </summary>
    public class BaseStationMessageConverter(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        BaseStationMessageParser _Parser
        #pragma warning restore IDE1006
    ) : ITransponderMessageConverter
    {
        /// <inheritdoc/>
        public BaseStationMessageConverterOptions Options { get; set; } = new();

        /// <inheritdoc/>
        public TransponderMessage[] FromFeedMessage(ReadOnlyMemory<byte> chunk)
        {
            return ConvertBaseStationToTransponder(_Parser.FromFeed(chunk));
        }

        private TransponderMessage[] ConvertBaseStationToTransponder(BaseStationMessage baseStationMessage)
        {
            TransponderMessage[] result = null;

            if(baseStationMessage != null && baseStationMessage.IsAircraftMessage) {
                if(Icao24.TryParse(baseStationMessage.Icao24, out var icao24, ignoreNonHexDigits: Options.Icao24CanHaveNonHexDigits)) {
                    result = [ new(icao24) {
                        Icao24 =                    icao24,
                        AltitudeFeet =              baseStationMessage.Altitude,
                        AltitudeType =              AltitudeType.AirPressure,
                        Callsign =                  baseStationMessage.Callsign,
                        CallsignIsSuspect =         false,
                        GroundSpeedKnots =          baseStationMessage.GroundSpeed,
                        GroundSpeedType =           SpeedType.GroundSpeed,
                        GroundTrackDegrees =        baseStationMessage.Track,
                        IdentActive =               baseStationMessage.IdentActive,
                        IsTisb =                    false,
                        Location =                  Location.FromLatLng(baseStationMessage.Latitude, baseStationMessage.Longitude),
                        MessageReceived =           baseStationMessage.MessageGenerated,
                        OnGround =                  baseStationMessage.OnGround,
                        Squawk =                    baseStationMessage.Squawk,
                        VerticalRateFeetPerMinute = baseStationMessage.VerticalRate,
                        VerticalRateType =          AltitudeType.AirPressure,
                        SignalLevel =               0,
                        SignalLevelSent =           false
                    }];
                }
            }

            return result ?? [];
        }
    }
}
