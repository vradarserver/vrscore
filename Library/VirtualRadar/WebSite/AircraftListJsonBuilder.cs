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
                        UniqueId =              aircraft.Id,
                        Altitude =              aircraft.AltitudeFeet       .ValueIfChanged(oldStamp),
                        AltitudeType =          aircraft.AltitudeType       .ValueIfChanged(oldStamp, v => (int?)v),
                        Callsign =              aircraft.Callsign           .ValueIfChanged(oldStamp),
                        CallsignIsSuspect =     aircraft.CallsignIsSuspect  .ValueIfChanged(oldStamp),
                        ConstructionNumber =    aircraft.ConstructionNumber .ValueIfChanged(oldStamp),
                        Emergency =             aircraft.SquawkIsEmergency  .ValueIfChanged(oldStamp),
                        Squawk =                aircraft.Squawk             .ValueIfChanged(oldStamp, v => v?.ToString("0000")),
                    };
                    if(aircraft.Stamp > oldStamp) {
                        aircraftJson.CountMessagesReceived = aircraft.CountMessagesReceived;
                    }
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

        private void AddRoute(BuildState state, AircraftJson aircraftJson, StampedValue<Route> route)
        {
            var preferredCodeType = state.AircraftMapSettings.PreferredAirportCodeType;

            if(route.Stamp > state.Args.PreviousDataVersion && route.Value != null) {
                aircraftJson.Destination = route.Value.To?.PreferredAirportCode(preferredCodeType);
            }
        }

        private bool IsDirectoryConfiguredAndExists(string directory)
        {
            return !String.IsNullOrEmpty(directory)
                && _FileSystem.DirectoryExists(directory);
        }
    }
}
