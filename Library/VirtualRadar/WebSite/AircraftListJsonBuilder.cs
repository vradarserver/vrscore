// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Configuration;
using VirtualRadar.Convert;
using VirtualRadar.Receivers;
using VirtualRadar.StandingData;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Do not instantiate this directly, use the DI container instead. This is only public so that it can be
    /// unit tested.
    /// </summary>
    public class AircraftListJsonBuilder(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<AircraftMapSettings>          _AircraftMapSettings,
        ISettings<AircraftPictureSettings>      _AircraftPictureSettings,
        ISettings<InternetClientSettings>       _InternetClientSettings,
        ISettings<OperatorAndTypeFlagSettings>  _OperatorAndFlagSettings,
        IReceiverFactory                        _ReceiverFactory,
        IFileSystem                             _FileSystem
        #pragma warning restore IDE1006
    ) : IAircraftListJsonBuilder
    {
        // Carries state into all of the build functions, and in particular maintains a consistent
        // set of latest settings throughout the build.
        record BuildState(
            AircraftListJsonBuilderArgs Args,
            IReceiver                   Receiver,
            AircraftMapSettings         AircraftMapSettings,
            AircraftPictureSettings     AircraftPictureSettings,
            InternetClientSettings      InternetClientSettings,
            OperatorAndTypeFlagSettings OperatorAndTypeFlagSettings,
            AircraftListJson            Json
        )
        {
        }

        /// <inheritdoc/>
        public AircraftListJson Build(
            AircraftListJsonBuilderArgs args,
            bool ignoreInvisibleSources,
            bool fallbackToDefaultSource
        )
        {
            var receiver = _ReceiverFactory.FindById(
                -1,
                ignoreInvisibleSources,
                fallbackToDefaultSource
            );

            var state = new BuildState(
                args,
                receiver,
                _AircraftMapSettings.LatestValue,
                _AircraftPictureSettings.LatestValue,
                _InternetClientSettings.LatestValue,
                _OperatorAndFlagSettings.LatestValue,
                new()
            );

            state.Json.ShortTrailLengthSeconds =    state.AircraftMapSettings.ShortTrailLengthSeconds;
            state.Json.Source =                     1; // <-- for backwards compatability, we don't have the concept of fake and/or flight sim aircraft lists in VRS Core
            state.Json.SourceFeedId =               args.ReceiverId;
            state.Json.LastDataVersion =            receiver?.AircraftList.Stamp.ToString();
            state.Json.ServerTime =                 PostOffice.GetStamp();

            AddAircraft(state);
            AddFeeds(state);
            AddFlags(state);
            AddPictures(state);

            return state.Json;
        }

        private void AddAircraft(BuildState state)
        {
            var stamp = 0L;

            var aircraftList = state.Receiver?.AircraftList;
            if(aircraftList != null) {
                var allAircraft = aircraftList.ToArray(out stamp);
                var oldStamp = state.Args.PreviousDataVersion;

                foreach(var aircraft in allAircraft) {
                    var aircraftJson = new AircraftJson() {
                        UniqueId =                  aircraft.Id,
                        AirPressureInHg =           aircraft.AirPressureInHg            .ValueIfChanged(oldStamp),
                        Altitude =                  aircraft.AltitudePressureFeet       .ValueIfChanged(oldStamp),
                        AltitudeType =              aircraft.AltitudeType               .ValueIfChanged(oldStamp, v => (int?)v),
                        Callsign =                  aircraft.Callsign                   .ValueIfChanged(oldStamp),
                        CallsignIsSuspect =         aircraft.CallsignIsSuspect          .ValueIfChanged(oldStamp),
                        ConstructionNumber =        aircraft.ConstructionNumber         .ValueIfChanged(oldStamp),
                        Emergency =                 aircraft.SquawkIsEmergency          .ValueIfChanged(oldStamp),
                        EnginePlacement =           aircraft.EnginePlacement            .ValueIfChanged(oldStamp, v => (int?)v),
                        EngineType =                aircraft.EngineType                 .ValueIfChanged(oldStamp, v => (int?)v),
                        GeometricAltitude =         aircraft.AltitudeRadarFeet          .ValueIfChanged(oldStamp),
                        GroundSpeed =               aircraft.GroundSpeedKnots           .ValueIfChanged(oldStamp),
                        HasSignalLevel =            aircraft.SignalLevelSent            .ValueIfChanged(oldStamp),
                        Icao24Country =             aircraft.Icao24Country              .ValueIfChanged(oldStamp),
                        IdentActive =               aircraft.IdentActive                .ValueIfChanged(oldStamp),
                        IsCharterFlight =           aircraft.IsCharterFlight            .ValueIfChanged(oldStamp),
                        IsMilitary =                aircraft.IsMilitary                 .ValueIfChanged(oldStamp),
                        IsPositioningFlight =       aircraft.IsPositioningFlight        .ValueIfChanged(oldStamp),
                        Manufacturer =              aircraft.Manufacturer               .ValueIfChanged(oldStamp),
                        Model =                     aircraft.Model                      .ValueIfChanged(oldStamp),
                        NumberOfEngines =           aircraft.NumberOfEngines            .ValueIfChanged(oldStamp),
                        OnGround =                  aircraft.OnGround                   .ValueIfChanged(oldStamp),
                        Operator =                  aircraft.Operator                   .ValueIfChanged(oldStamp),
                        OperatorIcao =              aircraft.OperatorIcao               .ValueIfChanged(oldStamp),
                        Registration =              aircraft.Registration               .ValueIfChanged(oldStamp),
                        Species =                   aircraft.Species                    .ValueIfChanged(oldStamp, v => (int?)v),
                        SpeedType =                 aircraft.GroundSpeedType            .ValueIfChanged(oldStamp, v => (int?)v),
                        Squawk =                    aircraft.Squawk                     .ValueIfChanged(oldStamp, v => v?.ToString("0000")),
                        TargetAltitude =            aircraft.TargetAltitudeFeet         .ValueIfChanged(oldStamp),
                        TargetTrack =               aircraft.TargetHeadingDegrees       .ValueIfChanged(oldStamp),
                        Track =                     aircraft.GroundTrackDegrees         .ValueIfChanged(oldStamp),
                        TrackIsHeading =            aircraft.GroundTrackIsHeading       .ValueIfChanged(oldStamp),
                        TransponderType =           aircraft.TransponderType            .ValueIfChanged(oldStamp, v => (int?)v),
                        Type =                      aircraft.ModelIcao                  .ValueIfChanged(oldStamp),
                        UserNotes =                 aircraft.UserNotes                  .ValueIfChanged(oldStamp),
                        UserTag =                   aircraft.UserTag                    .ValueIfChanged(oldStamp),
                        VerticalRate =              aircraft.VerticalRateFeetPerMinute  .ValueIfChanged(oldStamp),
                        VerticalRateType =          aircraft.VerticalRateType           .ValueIfChanged(oldStamp, v => (int?)v),
                        WakeTurbulenceCategory =    aircraft.WakeTurbulenceCategory     .ValueIfChanged(oldStamp, v => (int?)v),
                        YearBuilt =                 aircraft.YearBuilt                  .ValueIfChanged(oldStamp, v => v?.ToString("0000")),
                    };
                    if(state.Args.PreviousDataVersion <= 0) {
                        aircraftJson.ReceiverId = state.Receiver?.Id ?? 0;
                    }
                    if(aircraft.FirstStamp > oldStamp) {
                        aircraftJson.FirstSeen = aircraft.FirstMessageReceivedUtc;
                    }
                    if(aircraft.Stamp > oldStamp) {
                        aircraftJson.CountMessagesReceived = aircraft.CountMessagesReceived;
                    }
                    if(state.Args.AlwaysShowIcao || aircraft.Icao24.Stamp > oldStamp) {
                        aircraftJson.Icao24 = (aircraft.Icao24.Value?.IsValid ?? false)
                            ? aircraft.Icao24.Value?.ToString()
                            : "";
                        aircraftJson.Icao24Invalid = aircraft.Icao24.Value == null
                            ? null
                            : !aircraft.Icao24.Value.Value.IsValid;
                    }
                    if((aircraft.SignalLevelSent.Value ?? false) && aircraft.SignalLevel.Stamp > oldStamp) {
                        aircraftJson.SignalLevel = aircraft.SignalLevel.Value;
                    }
                    if(aircraft.Location.Stamp > oldStamp) {
                        aircraftJson.Latitude =     aircraft.Location.Value?.Latitude;
                        aircraftJson.Longitude =    aircraft.Location.Value?.Longitude;
                        aircraftJson.PositionTime = (long)(aircraft.Location.LastChangedUtc - Time.UnixEpocUtc).TotalMilliseconds;
                    }
                    AddPicture(state, aircraftJson, aircraft);
                    AddRoute(state, aircraftJson, aircraft.Route);
                    state.Json.Aircraft.Add(aircraftJson);
                }
            }

            state.Json.LastDataVersion = stamp.ToString(CultureInfo.InvariantCulture);
        }

        private void AddFeeds(BuildState state)
        {
            if(!state.Args.FeedsNotRequired) {
                foreach(var receiver in _ReceiverFactory.Receivers) {
                    state.Json.Feeds.Add(new FeedJson() {
                        UniqueId = receiver.Id,
                        Name = receiver.Name,
                        // TODO: Fill HasPolarPlot in feeds list
                        // HasPolarPlot = plottingAircraftList?.PolarPlotter != null
                    });
                }
            }
        }

        private void AddFlags(BuildState state)
        {
            var settings = state.OperatorAndTypeFlagSettings;
            state.Json.FlagWidth = settings.FlagWidthPixels;
            state.Json.FlagHeight = settings.FlagHeightPixels;

            state.Json.ShowFlags =       IsDirectoryConfiguredAndExists(settings.OperatorFlagsFolder);
            state.Json.ShowSilhouettes = IsDirectoryConfiguredAndExists(settings.TypeFlagsFolder);
        }

        private void AddPictures(BuildState state)
        {
            var internetClientSettings = state.Args.IsInternetClient
                ? state.InternetClientSettings
                : null;
            if(internetClientSettings?.CanShowPictures ?? true) {
                state.Json.ShowPictures = IsDirectoryConfiguredAndExists(
                    state.AircraftPictureSettings.LocalPicturesFolder
                );
            }
        }

        private void AddPicture(BuildState state, AircraftJson aircraftJson, Aircraft aircraft)
        {
            var lookupImage = aircraft.AircraftPicture.Value;
            if(aircraft.AircraftPicture.Stamp > state.Args.PreviousDataVersion) {
                aircraftJson.HasPicture =    !lookupImage?.DoesNotExist;
                aircraftJson.PictureHeight = lookupImage?.HeightPixels;
                aircraftJson.PictureWidth =  lookupImage?.WidthPixels;
            }
        }

        private void AddRoute(BuildState state, AircraftJson aircraftJson, StampedValue<Route> route)
        {
            var preferredCodeType = state.AircraftMapSettings.PreferredAirportCodeType;

            if(route.Stamp > state.Args.PreviousDataVersion && route.Value != null) {
                aircraftJson.Destination = route.Value.To?.PreferredAirportCode(preferredCodeType);
                aircraftJson.Origin =      route.Value.From?.PreferredAirportCode(preferredCodeType);
            }
        }

        private bool IsDirectoryConfiguredAndExists(string directory)
        {
            return !String.IsNullOrEmpty(directory)
                && _FileSystem.DirectoryExists(directory);
        }
    }
}
