// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.AspNetCore.Mvc;
using VirtualRadar.Filtering;
using VirtualRadar.Receivers;
using VirtualRadar.StandingData;
using VirtualRadar.WebSite;
using VirtualRadar.WebSite.Models;

namespace VirtualRadar.Server.ApiControllers
{
    [ApiController]
    public class FeedsController(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        IReceiverFactory _ReceiverFactory,
        IAircraftListJsonBuilder _AircraftListBuilder
        #pragma warning restore IDE1006
    ) : ControllerBase
    {
        /// <summary>
        /// Returns a list of every public facing feed.
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/3.00/feeds")]
        public FeedJson[] GetFeeds()
        {
            return _ReceiverFactory
                .Receivers
                .Where(receiver => !receiver.Hidden)
                .Select(receiver => FeedJson.FromReceiver(receiver))
                .ToArray();
        }

        /// <summary>
        /// Returns details for a single feed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("api/3.00/feeds/{id}")]
        public FeedJson GetFeed(int id)
        {
            var receiver = _ReceiverFactory.FindById(id);
            if(receiver?.Hidden ?? false) {
                receiver = null;
            }

            return FeedJson.FromReceiver(receiver);
        }

        /// <summary>
        /// Returns the polar plot for a feed.
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/3.00/feeds/polar-plot/{feedId}")]
        [Route("PolarPlot.json")]                       // pre-version 3 route
        public object GetPolarPlot(int feedId = -1)
        {
            // TODO
            return null;
        }

        /// <summary>
        /// Returns a list of all aircraft on the feed.
        /// </summary>
        /// <param name="feedId">The numeric feed ID. If not supplied then the default feed is used.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/3.00/feeds/aircraft-list/{feedId?}")]
        public AircraftListJson AircraftList(GetAircraftListModel model, int feedId = -1)
        {
            var args = new AircraftListJsonBuilderArgs() {
                BrowserLatitude =       model?.Latitude,
                BrowserLongitude =      model?.Longitude,
                IsFlightSimulatorList = model?.FlightSimulator ?? false,
                PreviousDataVersion =   model?.LastDataVersion ?? -1L,
                ResendTrails =          model?.ResendTrails ?? false,
                SelectedAircraftId =    model?.SelectedAircraft ?? -1,
                ServerTimeTicks =       model?.ServerTicks ?? -1L,
                ReceiverId =            feedId,
                Filter =                ExpandModelFilters(model?.Filters),
                TrailType =             model?.TrailType ?? TrailType.None,
            };
            SortByFromModel(args, model);
            PreviousAircraftFromModel(args, model);

            return BuildAircraftList(args);
        }

        private AircraftListJson BuildAircraftList(AircraftListJsonBuilderArgs builderArgs)
        {
            // TODO: Figure out whether request came from the internet
            builderArgs.IsInternetClient = false;

            var result = _AircraftListBuilder.Build(
                builderArgs,
                ignoreInvisibleSources: true,
                fallbackToDefaultSource: true
            );

            // TODO: Set ServerConfigChanged

            return result;
        }

        private void SortByFromModel(AircraftListJsonBuilderArgs args, GetAircraftListModel model)
        {
            if(model == null || model.SortBy == null || model.SortBy.Count == 0) {
                SetDefaultAircraftListSortBy(args);
            } else {
                for(var i = 0;i < Math.Min(2, model.SortBy.Count);++i) {
                    var sortBy = model.SortBy[i];
                    args.SortBy.Add(new KeyValuePair<string, bool>(sortBy.Col, sortBy.Asc));
                }
            }
        }

        private void PreviousAircraftFromModel(AircraftListJsonBuilderArgs args, GetAircraftListModel model)
        {
            if(model != null && model.PreviousAircraft != null && model.PreviousAircraft.Count > 0) {
                foreach(var icao in model.PreviousAircraft) {
                    if(Icao24.TryParse(icao, out var parsed)) {
                        args.PreviousAircraft.Add(parsed);
                    }
                }
            }
        }

        private AircraftListJsonBuilderFilter ExpandModelFilters(List<GetAircraftListFilter> filters)
        {
            AircraftListJsonBuilderFilter result = null;

            if(filters != null) {
                foreach(var jsonFilter in filters) {
                    switch(jsonFilter.Field) {
                        case GetAircraftListFilterField.Airport:        result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Airport = v); break;
                        case GetAircraftListFilterField.Altitude:       result = JsonToIntRangeFilter(jsonFilter, result,     (f,v) => f.Altitude = v); break;
                        case GetAircraftListFilterField.Callsign:       result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Callsign = v); break;
                        case GetAircraftListFilterField.Country:        result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Icao24Country = v); break;
                        case GetAircraftListFilterField.Distance:       result = JsonToDoubleRangeFilter(jsonFilter, result,  (f,v) => f.Distance = v); break;
                        case GetAircraftListFilterField.HasPosition:    result = JsonToBoolFilter(jsonFilter, result,         (f,v) => f.MustTransmitPosition = v); break;
                        case GetAircraftListFilterField.Icao:           result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Icao24 = v); break;
                        case GetAircraftListFilterField.IsInteresting:  result = JsonToBoolFilter(jsonFilter, result,         (f,v) => f.IsInteresting = v); break;
                        case GetAircraftListFilterField.IsMilitary:     result = JsonToBoolFilter(jsonFilter, result,         (f,v) => f.IsMilitary = v); break;
                        case GetAircraftListFilterField.ModelIcao:      result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Type = v); break;
                        case GetAircraftListFilterField.Operator:       result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Operator = v); break;
                        case GetAircraftListFilterField.OperatorIcao:   result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.OperatorIcao = v); break;
                        case GetAircraftListFilterField.PositionBounds: result = JsonToCoordPair(jsonFilter, result,          (f,v) => f.PositionWithin = v); break;
                        case GetAircraftListFilterField.Registration:   result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.Registration = v); break;
                        case GetAircraftListFilterField.Squawk:         result = JsonToIntRangeFilter(jsonFilter, result,     (f,v) => f.Squawk = v); break;
                        case GetAircraftListFilterField.UserTag:        result = JsonToStringFilter(jsonFilter, result,       (f,v) => f.UserTag = v); break;

                        case GetAircraftListFilterField.EngineType:     result = JsonToEnumFilter<EngineType>(jsonFilter, result,             (f,v) => f.EngineType = v); break;
                        case GetAircraftListFilterField.Species:        result = JsonToEnumFilter<Species>(jsonFilter, result,                (f,v) => f.Species = v); break;
                        case GetAircraftListFilterField.WTC:            result = JsonToEnumFilter<WakeTurbulenceCategory>(jsonFilter, result, (f,v) => f.WakeTurbulenceCategory = v); break;
                    }
                }
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToBoolFilter(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, FilterBool> assignFilter
        )
        {
            switch(jsonFilter.Condition) {
                case FilterCondition.Missing:
                case FilterCondition.Equals:
                    if(jsonFilter.Is != null) {
                        DoAssignFilter(ref result, assignFilter, new FilterBool() {
                            Condition =         FilterCondition.Equals,
                            Value =             jsonFilter.Is.Value,
                            ReverseCondition =  jsonFilter.Not,
                        });
                    }
                    break;
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToCoordPair(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, LocationRectangle> _
        )
        {
            if(jsonFilter.North != null && jsonFilter.South != null && jsonFilter.West != null && jsonFilter.East != null) {
                result ??= new AircraftListJsonBuilderFilter();
                result.PositionWithin = new LocationRectangle(
                    north: jsonFilter.North.Value,
                    west:  jsonFilter.West.Value,
                    south: jsonFilter.South.Value,
                    east:  jsonFilter.East.Value
                );
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToDoubleRangeFilter(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, FilterRange<double>> assignFilter
        )
        {
            switch(jsonFilter.Condition) {
                case FilterCondition.Missing:
                case FilterCondition.Between:
                    DoAssignFilter(ref result, assignFilter, new FilterRange<double>() {
                        Condition =         FilterCondition.Between,
                        LowerValue =        jsonFilter.From,
                        UpperValue =        jsonFilter.To,
                        ReverseCondition =  jsonFilter.Not,
                    });
                    break;
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToEnumFilter<T>(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, FilterEnum<T>> assignFilter
        )
            where T: struct, IComparable
        {
            switch(jsonFilter.Condition) {
                case FilterCondition.Missing:
                case FilterCondition.Equals:
                    if(!String.IsNullOrEmpty(jsonFilter.Value) && Enum.TryParse<T>(jsonFilter.Value, out T enumValue)) {
                        if(Enum.IsDefined(typeof(T), enumValue)) {
                            DoAssignFilter(ref result, assignFilter, new FilterEnum<T>() {
                                Condition =         FilterCondition.Equals,
                                Value =             enumValue,
                                ReverseCondition =  jsonFilter.Not,
                            });
                        }
                    }
                    break;
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToIntRangeFilter(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, FilterRange<int>> assignFilter
        )
        {
            switch(jsonFilter.Condition) {
                case FilterCondition.Missing:
                case FilterCondition.Between:
                    DoAssignFilter(ref result, assignFilter, new FilterRange<int>() {
                        Condition =         FilterCondition.Between,
                        LowerValue =        (int?)jsonFilter.From,
                        UpperValue =        (int?)jsonFilter.To,
                        ReverseCondition =  jsonFilter.Not,
                    });
                    break;
            }

            return result;
        }

        private AircraftListJsonBuilderFilter JsonToStringFilter(
            GetAircraftListFilter jsonFilter,
            AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, FilterString> assignFilter
        )
        {
            switch(jsonFilter.Condition) {
                case FilterCondition.Contains:
                case FilterCondition.EndsWith:
                case FilterCondition.Equals:
                case FilterCondition.StartsWith:
                    DoAssignFilter(ref result, assignFilter, new FilterString() {
                        Condition =         jsonFilter.Condition,
                        Value =             jsonFilter.Value ?? "",
                        ReverseCondition =  jsonFilter.Not,
                    });
                    break;
            }

            return result;
        }

        private void DoAssignFilter<T>(
            ref AircraftListJsonBuilderFilter result,
            Action<AircraftListJsonBuilderFilter, T> assignFilter,
            T filter
        )
        {
            result ??= new AircraftListJsonBuilderFilter();
            assignFilter(result, filter);
        }

        private static void SetDefaultAircraftListSortBy(AircraftListJsonBuilderArgs args)
        {
            args.SortBy.Add(new KeyValuePair<string, bool>(
                AircraftComparerColumn.FirstSeen, false
            ));
        }
    }
}
