// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;
using VirtualRadar.Receivers;
using VirtualRadar.TileServer;
using VirtualRadar.WebSite.Models;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Factory class that can build <see cref="ServerConfigJson"/> models.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public class ServerConfigJsonFactory(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<AircraftMapSettings> _AircraftMapSettings,
        ISettings<InternetClientSettings> _InternetClientSettings,
        ISettings<WebClientSettings> _WebClientSettings,
        IDownloadedTileServerSettingsManager _TileServerManager,
        IReceiverFactory _ReceiverFactory
        #pragma warning restore IDE1006
    )
    {
        /// <summary>
        /// Creates a new <see cref="ServerConfigJson"/>.
        /// </summary>
        /// <param name="isLocalAddress"></param>
        /// <returns></returns>
        public ServerConfigJson Build(bool isLocalAddress)
        {
            var aircraftMapSettings = _AircraftMapSettings.LatestValue;
            var internetClientSettings = _InternetClientSettings.LatestValue;
            var webClientSettings = _WebClientSettings.LatestValue;

            var tileServerSettings = _TileServerManager.GetTileServerSettings(
                aircraftMapSettings.MapProvider,
                aircraftMapSettings.TileServerName,
                fallbackToDefaultIfMissing: true
            );

            var version = InformationalVersion.VirtualRadarVersion;

            var result = new ServerConfigJson() {
                InitialDistanceUnit =                   GetDistanceUnit(aircraftMapSettings.InitialDistanceUnit),
                InitialHeightUnit =                     GetHeightUnit(aircraftMapSettings.InitialHeightUnit),
                InitialLatitude =                       aircraftMapSettings.InitialMapLatitude,
                InitialLongitude =                      aircraftMapSettings.InitialMapLongitude,
                InitialMapType =                        GetMapType(aircraftMapSettings.InitialMapType),
                InitialSettings =                       null,       // TODO
                InitialSpeedUnit =                      GetSpeedUnit(aircraftMapSettings.InitialSpeedUnit),
                InitialZoom =                           aircraftMapSettings.InitialMapZoom,
                InternetClientCanRunReports =           internetClientSettings.CanRunReports,
                InternetClientCanShowPinText =          internetClientSettings.CanShowPinText,
                InternetClientsCanPlayAudio =           internetClientSettings.CanPlayAudio,
                InternetClientsCanSubmitRoutes =        internetClientSettings.CanSubmitRoutes,
                InternetClientsCanSeeAircraftPictures = internetClientSettings.CanShowPictures,
                InternetClientsCanSeePolarPlots =       internetClientSettings.CanShowPolarPlots,
                InternetClientTimeoutMinutes =          internetClientSettings.TimeoutMinutes,
                IsAudioEnabled =                        webClientSettings.IsAudioEnabled,
                IsLocalAddress =                        isLocalAddress,
                IsMono =                                false,
                UseMarkerLabels =                       aircraftMapSettings.UseMarkerLabels,
                UseSvgGraphicsOnDesktop =               webClientSettings.UseSvgGraphicsOnDesktop,
                UseSvgGraphicsOnMobile =                webClientSettings.UseSvgGraphicsOnMobile,
                UseSvgGraphicsOnReports =               webClientSettings.UseSvgGraphicsOnReports,
                MinimumRefreshSeconds =                 aircraftMapSettings.MinimumRefreshSeconds,
                RefreshSeconds =                        aircraftMapSettings.InitialRefreshSeconds,
                TileServerSettings =                    tileServerSettings,
                VrsVersion =                            $"Core {version}",
            };

            result.Receivers.AddRange(_ReceiverFactory
                .Receivers
                .Select(r => ServerReceiverJson.ToModel(r))
                .Where(r => r != null)
            );

            result.TileServerLayers.AddRange(_TileServerManager
                .GetAllTileLayerSettings(aircraftMapSettings.MapProvider)
                .OrderBy(r => r.DisplayOrder)
            );

            if(!isLocalAddress || aircraftMapSettings.UseGoogleMapsAPIKeyWithLocalRequests) {
                result.GoogleMapsApiKey = aircraftMapSettings.GoogleMapsApiKey;
                if(String.IsNullOrWhiteSpace(result.GoogleMapsApiKey)) {
                    result.GoogleMapsApiKey = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Translates from a Google map type to a JavaScript map type.
        /// </summary>
        /// <param name="mapType"></param>
        /// <returns></returns>
        private static string GetMapType(AircraftMapType mapType)
        {
            switch(mapType) {
                case AircraftMapType.Hybrid:        return "h";
                case AircraftMapType.Roadmap:       return "m";
                case AircraftMapType.Terrain:       return "t";
                case AircraftMapType.Satellite:     return "s";
                case AircraftMapType.HighContrast:  return "o";
                default:                            return null;
            }
        }

        /// <summary>
        /// Translates from an internal distance unit to a JavaScript distance unit.
        /// </summary>
        /// <param name="distanceUnit"></param>
        /// <returns></returns>
        private static string GetDistanceUnit(DistanceUnit distanceUnit)
        {
            switch(distanceUnit) {
                case DistanceUnit.Kilometres:       return "km";
                case DistanceUnit.Miles:            return "sm";
                case DistanceUnit.NauticalMiles:    return "nm";
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Translates from an internal height unit to a JavaScript height unit.
        /// </summary>
        /// <param name="heightUnit"></param>
        /// <returns></returns>
        private static string GetHeightUnit(HeightUnit heightUnit)
        {
            switch(heightUnit) {
                case HeightUnit.Feet:               return "f";
                case HeightUnit.Metres:             return "m";
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Translates from an internal speed unit to a JavaScript speed unit.
        /// </summary>
        /// <param name="speedUnit"></param>
        /// <returns></returns>
        private static string GetSpeedUnit(SpeedUnit speedUnit)
        {
            switch(speedUnit) {
                case SpeedUnit.KilometresPerHour:   return "km";
                case SpeedUnit.Knots:               return "kt";
                case SpeedUnit.MilesPerHour:        return "ml";
                default:                            throw new NotImplementedException();
            }
        }
    }
}
