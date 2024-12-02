// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

(function(VRS, /** jQuery= */ $, /** object= */ undefined)
{
    // Singleton declaration
    VRS.$$ = VRS.$$ || {};

    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths = false;

    // [[ MARKER START SIMPLE STRINGS ]]
    VRS.$$.Add = 'Add';
    VRS.$$.AddAircraftLookup = 'Add lookup details';
    VRS.$$.AddCondition = 'Add Condition';
    VRS.$$.AddCriteria = 'Add Criteria';
    VRS.$$.AddFilter = 'Add Filter';
    VRS.$$.ADSB = 'ADS-B';
    VRS.$$.ADSB0 = 'ADS-B v0';
    VRS.$$.ADSB1 = 'ADS-B v1';
    VRS.$$.ADSB2 = 'ADS-B v2';
    VRS.$$.AircraftClass = 'Aircraft Class';
    VRS.$$.AircraftDetailShort = 'Detail';
    VRS.$$.AircraftNotTransmittingCallsign = 'Aircraft is not transmitting its callsign';
    VRS.$$.Airport = 'Airport';
    VRS.$$.AirportDataThumbnails = 'Thumbnails (airport-data.com)';
    VRS.$$.AirPressure = 'Air Pressure';
    VRS.$$.AllAircraft = 'List all';
    VRS.$$.AllAltitudes = 'All altitudes';
    VRS.$$.AllRows = 'All rows';
    VRS.$$.Altitude = 'Altitude';
    VRS.$$.AltitudeAndSpeedGraph = 'Altitude & Speed Graph';
    VRS.$$.AltitudeAndVerticalSpeed = 'Altitude & VSI';
    VRS.$$.AltitudeGraph = 'Altitude Graph';
    VRS.$$.AltitudeType = 'Altitude Type';
    VRS.$$.Amphibian = 'Amphibian';
    VRS.$$.AnnounceSelected = 'Announce details of selected aircraft';
    VRS.$$.Ascending = 'Ascending';
    VRS.$$.AutoSelectAircraft = 'Auto-select aircraft';
    VRS.$$.AverageSignalLevel = 'Avg. Signal Level';
    VRS.$$.Barometric = 'Barometric';
    VRS.$$.Bearing = 'Bearing';
    VRS.$$.Between = 'Between';
    VRS.$$.Callsign = 'Callsign';
    VRS.$$.CallsignAndShortRoute = 'Callsign & Route';
    VRS.$$.CallsignMayNotBeCorrect = 'Callsign may not be correct';
    VRS.$$.CentreOnSelectedAircraft = 'Show on map';
    VRS.$$.CharterFlight = 'Charter flight';
    VRS.$$.CharterFlightShort = 'CHARTER';
    VRS.$$.Civil = 'Civil';
    VRS.$$.CivilOrMilitary = 'Civil / Military';
    VRS.$$.ClosestToCurrentLocation = 'Closest to';
    VRS.$$.CofACategory = 'C/A Category';
    VRS.$$.CofAExpiry = 'C/A Expiry';
    VRS.$$.Columns = 'Columns';
    VRS.$$.Contains = 'Contains';
    VRS.$$.CountAdsb = 'ADS-B Count';
    VRS.$$.CountModeS = 'Mode-S Count';
    VRS.$$.CountPositions = 'Position Count';
    VRS.$$.Country = 'Country';
    VRS.$$.Criteria = 'Criteria';
    VRS.$$.CurrentLocationInstruction = 'To set your current location click "Set current location" and drag the marker.';
    VRS.$$.CurrentRegDate = 'Current Reg. Date';
    VRS.$$.Date = 'Date';
    VRS.$$.DateClose = 'Done';
    VRS.$$.DateCurrent = 'Today';
    VRS.$$.DateNext = 'Next';
    VRS.$$.DatePrevious = 'Prev';
    VRS.$$.DateTimeShort = '{0} {1}';
    VRS.$$.DateWeekAbbr = 'Wk';
    VRS.$$.DateYearSuffix = '';
    VRS.$$.DefaultSetting = '< Default >';
    VRS.$$.DegreesAbbreviation = '{0}°';
    VRS.$$.DeRegDate = 'De-reg. Date';
    VRS.$$.DesktopPage = 'Desktop Page';
    VRS.$$.DesktopReportPage = 'Desktop Report Page';
    VRS.$$.DetailItems = 'Aircraft Detail Items';
    VRS.$$.DetailPanel = 'Detail Panel';
    VRS.$$.DisableAutoSelect = 'Disable auto-select';
    VRS.$$.Distance = 'Distance';
    VRS.$$.Distances = 'Distances';
    VRS.$$.DoNotImportAutoSelect = 'Do not import auto-select settings';
    VRS.$$.DoNotImportCurrentLocation = 'Do not import current location';
    VRS.$$.DoNotImportLanguageSettings = 'Do not import language settings';
    VRS.$$.DoNotImportRequestFeedId = 'Do not import request feed ID';
    VRS.$$.DoNotImportSplitters = 'Do not import splitters';
    VRS.$$.DoNotShow = 'Do not show';
    VRS.$$.Duration = 'Duration';
    VRS.$$.Electric = 'Electric';
    VRS.$$.EnableAutoSelect = 'Enable auto-select';
    VRS.$$.EnableFilters = 'Enable filters';
    VRS.$$.EnableInfoWindow = 'Enable info window';
    VRS.$$.End = 'End';
    VRS.$$.EndsWith = 'Ends With';
    VRS.$$.EndTime = 'End Time';
    VRS.$$.Engines = 'Engines';
    VRS.$$.EngineType = 'Engine Type';
    VRS.$$.Equal = 'Is';
    VRS.$$.EraseBeforeImport = 'Erase all settings before import';
    VRS.$$.ExportSettings = 'Export Settings';
    VRS.$$.Feet = 'Feet';
    VRS.$$.FeetAbbreviation = '{0} ft';
    VRS.$$.FeetPerMinuteAbbreviation = '{0} ft/m';
    VRS.$$.FeetPerSecondAbbreviation = '{0} ft/s';
    VRS.$$.FetchPage = 'Fetch';
    VRS.$$.FillOpacity = 'Fill opacity';
    VRS.$$.Filters = 'Filters';
    VRS.$$.FindAllPermutationsOfCallsign = 'Find all permutations of callsign';
    VRS.$$.First = 'First';
    VRS.$$.FirstAltitude = 'First Altitude';
    VRS.$$.FirstFlightLevel = 'First FL';
    VRS.$$.FirstHeading = 'First Heading';
    VRS.$$.FirstLatitude = 'First Latitude';
    VRS.$$.FirstLongitude = 'First Longitude';
    VRS.$$.FirstOnGround = 'First On Ground';
    VRS.$$.FirstRegDate = 'First Reg.Date';
    VRS.$$.FirstSpeed = 'First Speed';
    VRS.$$.FirstSquawk = 'First Squawk';
    VRS.$$.FirstVerticalSpeed = 'First Vertical Speed';
    VRS.$$.FlightDetailShort = 'Detail';
    VRS.$$.FlightLevel = 'Flight Level';
    VRS.$$.FlightLevelAbbreviation = 'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed = 'FL & VSI';
    VRS.$$.FlightLevelHeightUnit = 'Flight level height unit';
    VRS.$$.FlightLevelTransitionAltitude = 'Flight level transition altitude';
    VRS.$$.FlightsCount = 'Flights Count';
    VRS.$$.FlightSimPage = 'Flight Sim Page';
    VRS.$$.FlightSimTitle = 'Virtual Radar - FSX';
    VRS.$$.FlightsListShort = 'Flights';
    VRS.$$.ForcePhoneOff = 'Is not phone';
    VRS.$$.ForcePhoneOn = 'Is phone';
    VRS.$$.ForceTabletOff = 'Is not tablet';
    VRS.$$.ForceTabletOn = 'Is tablet';
    VRS.$$.FromAltitude = 'From {0}';
    VRS.$$.FromToAltitude = '{0} to {1}';
    VRS.$$.FromToDate = '{0} to {1}';
    VRS.$$.FromToFlightLevel = '{0} to {1}';
    VRS.$$.FromToSpeed = '{0} to {1}';
    VRS.$$.FromToSquawk = '{0} to {1}';
    VRS.$$.FurthestFromCurrentLocation = 'Furthest from current location';
    VRS.$$.GenericName = 'Generic Name';
    VRS.$$.Geometric = 'Geometric';
    VRS.$$.GeometricAltitude = 'Altitude (AMSL)';
    VRS.$$.GeometricAltitudeIndicator = 'amsl';
    VRS.$$.GoogleMapsCouldNotBeLoaded = 'Google Maps could not be loaded';
    VRS.$$.GotoCurrentLocation = 'Go to current location';
    VRS.$$.GotoSelectedAircraft = 'Go to selected aircraft';
    VRS.$$.Ground = 'Ground';
    VRS.$$.GroundAbbreviation = 'GND';
    VRS.$$.GroundTrack = 'Ground track';
    VRS.$$.GroundVehicle = 'Ground Vehicle';
    VRS.$$.Gyrocopter = 'Gyrocopter';
    VRS.$$.HadAlert = 'Had Alert';
    VRS.$$.HadEmergency = 'Had Emergency';
    VRS.$$.HadSPI = 'Had Ident';
    VRS.$$.Heading = 'Heading';
    VRS.$$.HeadingType = 'Heading Type';
    VRS.$$.Heights = 'Heights';
    VRS.$$.Helicopter = 'Helicopter';
    VRS.$$.Help = 'Help';
    VRS.$$.HideAircraftNotOnMap = 'Hide aircraft not on map';
    VRS.$$.HideEmptyPinTextLines = 'Hide empty label lines';
    VRS.$$.HideNoPosition = 'Has position';
    VRS.$$.HighContrastMap = 'Contrast';
    VRS.$$.Icao = 'ICAO';
    VRS.$$.IDENT = 'IDENT';
    VRS.$$.IdentActive = 'Ident Active';
    VRS.$$.Import = 'Import';
    VRS.$$.ImportFailedBody = 'Could not import your settings. The reported error was: {0}';
    VRS.$$.ImportFailedTitle = 'Import Settings Failed';
    VRS.$$.ImportSettings = 'Import Settings';
    VRS.$$.Index = 'Index';
    VRS.$$.IndicatedAirSpeed = 'Indicated';
    VRS.$$.IndicatedAirSpeedShort = 'IAS';
    VRS.$$.InHgAbbreviation = '{0} inHg';
    VRS.$$.InHgDescription = 'Inches of Mercury';
    VRS.$$.Interesting = 'Interesting';
    VRS.$$.IntervalSeconds = 'Update interval (secs)';
    VRS.$$.IsMilitary = 'Military';
    VRS.$$.Jet = 'Jet';
    VRS.$$.JustPositions = 'Positions';
    VRS.$$.KilometreAbbreviation = '{0} km';
    VRS.$$.Kilometres = 'Kilometres';
    VRS.$$.KilometresPerHour = 'Kilometres/Hour';
    VRS.$$.KilometresPerHourAbbreviation = '{0} km/h';
    VRS.$$.Knots = 'Knots';
    VRS.$$.KnotsAbbreviation = '{0} kts';
    VRS.$$.LandPlane = 'Landplane';
    VRS.$$.LanguageCode = 'en-EN';
    VRS.$$.Last = 'Last';
    VRS.$$.LastAltitude = 'Last Altitude';
    VRS.$$.LastFlightLevel = 'Last FL';
    VRS.$$.LastHeading = 'Last Heading';
    VRS.$$.LastLatitude = 'Last Latitude';
    VRS.$$.LastLongitude = 'Last Longitude';
    VRS.$$.LastOnGround = 'Last On Ground';
    VRS.$$.LastSpeed = 'Last Speed';
    VRS.$$.LastSquawk = 'Last Squawk';
    VRS.$$.LastVerticalSpeed = 'Last Vertical Speed';
    VRS.$$.Latitude = 'Latitude';
    VRS.$$.Layout = 'Layout';
    VRS.$$.Layout1 = 'Classic';
    VRS.$$.Layout2 = 'Tall Detail, Map Top';
    VRS.$$.Layout3 = 'Tall Detail, Map Bottom';
    VRS.$$.Layout4 = 'Tall List, Map Top';
    VRS.$$.Layout5 = 'Tall List, Map Bottom';
    VRS.$$.Layout6 = 'List Left, Detail Right, No Map';
    VRS.$$.ListAircraftClass = 'A/C Class';
    VRS.$$.ListAirportDataThumbnails = 'Thumbnails (airport-data.com)';
    VRS.$$.ListAirPressure = 'Pressure';
    VRS.$$.ListAltitude = 'Altitude';
    VRS.$$.ListAltitudeAndVerticalSpeed = 'Alt & VSI';
    VRS.$$.ListAltitudeType = 'Alt. Type';
    VRS.$$.ListAverageSignalLevel = 'Avg. Sig';
    VRS.$$.ListBearing = 'Brng.';
    VRS.$$.ListCallsign = 'Callsign';
    VRS.$$.ListCivOrMil = 'Civ/Mil';
    VRS.$$.ListCofACategory = 'C/A Cat.';
    VRS.$$.ListCofAExpiry = 'C/A Expiry';
    VRS.$$.ListCountAdsb = 'ADS-B Msgs.';
    VRS.$$.ListCountMessages = 'Msgs.';
    VRS.$$.ListCountModeS = 'Mode-S Msgs.';
    VRS.$$.ListCountPositions = 'Posn. Msgs.';
    VRS.$$.ListCountry = 'Country';
    VRS.$$.ListCurrentRegDate = 'Current Reg.';
    VRS.$$.ListDeRegDate = 'De-reg Date';
    VRS.$$.ListDistance = 'Distance';
    VRS.$$.ListDuration = 'Duration';
    VRS.$$.ListEndTime = 'Last Message';
    VRS.$$.ListEngines = 'Engines';
    VRS.$$.ListFirstAltitude = 'From Alt.';
    VRS.$$.ListFirstFlightLevel = 'From FL';
    VRS.$$.ListFirstHeading = 'From Hdg.';
    VRS.$$.ListFirstLatitude = 'From Lat.';
    VRS.$$.ListFirstLongitude = 'From Lng.';
    VRS.$$.ListFirstOnGround = 'From On Gnd.';
    VRS.$$.ListFirstRegDate = 'First Reg.';
    VRS.$$.ListFirstSpeed = 'From Speed';
    VRS.$$.ListFirstSquawk = 'From Squawk';
    VRS.$$.ListFirstVerticalSpeed = 'From VSI';
    VRS.$$.ListFlightLevel = 'FL';
    VRS.$$.ListFlightLevelAndVerticalSpeed = 'FL & VSI';
    VRS.$$.ListFlightsCount = 'Seen';
    VRS.$$.ListGenericName = 'Generic Name';
    VRS.$$.ListGeometricAltitude = 'Altitude (AMSL)';
    VRS.$$.ListHadAlert = 'Alert';
    VRS.$$.ListHadEmergency = 'Emergency';
    VRS.$$.ListHadSPI = 'SPI';
    VRS.$$.ListHeading = 'Hdg.';
    VRS.$$.ListHeadingType = 'Hdg. Type';
    VRS.$$.ListIcao = 'ICAO';
    VRS.$$.ListIdentActive = 'Ident Active';
    VRS.$$.ListInteresting = 'Interesting';
    VRS.$$.ListLastAltitude = 'To Alt.';
    VRS.$$.ListLastFlightLevel = 'To FL';
    VRS.$$.ListLastHeading = 'To Hdg.';
    VRS.$$.ListLastLatitude = 'To Lat.';
    VRS.$$.ListLastLongitude = 'To Lng.';
    VRS.$$.ListLastOnGround = 'To On Gnd.';
    VRS.$$.ListLastSpeed = 'To Speed';
    VRS.$$.ListLastSquawk = 'To Squawk';
    VRS.$$.ListLastVerticalSpeed = 'To VSI';
    VRS.$$.ListLatitude = 'Lat.';
    VRS.$$.ListLongitude = 'Lng.';
    VRS.$$.ListManufacturer = 'Manufacturer';
    VRS.$$.ListMaxTakeoffWeight = 'Max T/O Weight';
    VRS.$$.ListMlat = 'MLAT';
    VRS.$$.ListModel = 'Model';
    VRS.$$.ListModelIcao = 'Type';
    VRS.$$.ListModelSilhouette = 'Silhouette';
    VRS.$$.ListModelSilhouetteAndOpFlag = 'Flags';
    VRS.$$.ListModeSCountry = 'Mode-S Country';
    VRS.$$.ListNotes = 'Notes';
    VRS.$$.ListOperator = 'Operator';
    VRS.$$.ListOperatorFlag = 'Flag';
    VRS.$$.ListOperatorIcao = 'Op. Code';
    VRS.$$.ListOwnershipStatus = 'Ownership Status';
    VRS.$$.ListPicture = 'Picture';
    VRS.$$.ListPopularName = 'Popular Name';
    VRS.$$.ListPositionAge = 'Posn. Age';
    VRS.$$.ListPressureAltitude = 'Altitude (ISA)';
    VRS.$$.ListPreviousId = 'Previous ID';
    VRS.$$.ListReceiver = 'Receiver';
    VRS.$$.ListRegistration = 'Reg.';
    VRS.$$.ListRoute = 'Route';
    VRS.$$.ListRowNumber = 'Row';
    VRS.$$.ListSerialNumber = 'Serial';
    VRS.$$.ListSignalLevel = 'Sig';
    VRS.$$.ListSpecies = 'Species';
    VRS.$$.ListSpeed = 'Speed';
    VRS.$$.ListSpeedType = 'Speed Type';
    VRS.$$.ListSquawk = 'Squawk';
    VRS.$$.ListStartTime = 'Seen';
    VRS.$$.ListStatus = 'Status';
    VRS.$$.ListTargetAltitude = 'A/P Alt.';
    VRS.$$.ListTargetHeading = 'A/P Hdg.';
    VRS.$$.ListTisb = 'TIS-B';
    VRS.$$.ListTotalHours = 'Total Hours';
    VRS.$$.ListTransponderType = 'Transponder';
    VRS.$$.ListTransponderTypeFlag = '';
    VRS.$$.ListUserTag = 'Tag';
    VRS.$$.ListVerticalSpeed = 'V.Speed';
    VRS.$$.ListVerticalSpeedType = 'V.Speed Type';
    VRS.$$.ListWtc = 'WTC';
    VRS.$$.ListYearBuilt = 'Built';
    VRS.$$.Longitude = 'Longitude';
    VRS.$$.Manufacturer = 'Manufacturer';
    VRS.$$.Map = 'Map';
    VRS.$$.MapBrightness = 'Map Brightness';
    VRS.$$.MapLayers = 'Map Layers';
    VRS.$$.MaxTakeoffWeight = 'MaxTakeoffWeight';
    VRS.$$.Menu = 'Menu';
    VRS.$$.MenuBack = 'back';
    VRS.$$.MessageCount = 'Message Count';
    VRS.$$.MetreAbbreviation = '{0} m';
    VRS.$$.MetrePerMinuteAbbreviation = '{0} m/min';
    VRS.$$.MetrePerSecondAbbreviation = '{0} m/sec';
    VRS.$$.Metres = 'Metres';
    VRS.$$.MilesPerHour = 'Miles/Hour';
    VRS.$$.MilesPerHourAbbreviation = '{0} mph';
    VRS.$$.Military = 'Military';
    VRS.$$.MillibarAbbreviation = '{0} mb';
    VRS.$$.MillibarDescription = 'Millibars';
    VRS.$$.Mlat = 'MLAT';
    VRS.$$.MmHgAbbreviation = '{0} mmHg';
    VRS.$$.MmHgDescription = 'Millimetres of Mercury';
    VRS.$$.MobilePage = 'Mobile Page';
    VRS.$$.MobileReportPage = 'Mobile Report Page';
    VRS.$$.Model = 'Model';
    VRS.$$.ModelIcao = 'Model Code';
    VRS.$$.ModeS = 'Mode-S';
    VRS.$$.ModeSCountry = 'Mode-S Country';
    VRS.$$.MovingMap = 'Moving Map';
    VRS.$$.MuteOff = 'Mute Off';
    VRS.$$.MuteOn = 'Mute';
    VRS.$$.NauticalMileAbbreviation = '{0} nmi';
    VRS.$$.NauticalMiles = 'Nautical Miles';
    VRS.$$.Neither = 'Neither';
    VRS.$$.No = 'No';
    VRS.$$.NoLocalStorage = 'This browser does not support local storage. Your configuration settings will not be saved.\n\nIf you are browsing in "Private Mode" then try switching if off. Private Mode will disable local storage on some browsers.';
    VRS.$$.None = 'None';
    VRS.$$.NoSettingsFound = 'No settings found';
    VRS.$$.NotBetween = 'Is Not Between';
    VRS.$$.NotContains = 'Does Not Contain';
    VRS.$$.NotEndsWith = 'Does Not End With';
    VRS.$$.NotEquals = 'Is Not';
    VRS.$$.Notes = 'Notes';
    VRS.$$.NotStartsWith = 'Does Not Start With';
    VRS.$$.OffRadarAction = 'When aircraft go out of range:';
    VRS.$$.OffRadarActionEnableAutoSelect = 'Enable auto-select';
    VRS.$$.OffRadarActionNothing = 'Do nothing';
    VRS.$$.OffRadarActionWait = 'Deselect the aircraft';
    VRS.$$.OfPages = 'of {0:N0}';
    VRS.$$.OnlyAircraftOnMap = 'List only visible';
    VRS.$$.OnlyAutoSelected = 'Only announce details of auto-selected aircraft';
    VRS.$$.OnlyUsePre22Icons = 'Only show old style aircraft markers';
    VRS.$$.Operator = 'Operator';
    VRS.$$.OperatorCode = 'Operator Code';
    VRS.$$.OperatorFlag = 'Operator Flag';
    VRS.$$.Options = 'Options';
    VRS.$$.OverwriteExistingSettings = 'Overwrite existing settings';
    VRS.$$.OwnershipStatus = 'Ownership Status';
    VRS.$$.PageAircraft = 'Aircraft';
    VRS.$$.PageFirst = 'First';
    VRS.$$.PageGeneral = 'General';
    VRS.$$.PageLast = 'Last';
    VRS.$$.PageList = 'List';
    VRS.$$.PageListShort = 'List';
    VRS.$$.PageMapShort = 'Map';
    VRS.$$.PageNext = 'Next';
    VRS.$$.PagePrevious = 'Previous';
    VRS.$$.PaneAircraftDisplay = 'Aircraft Display';
    VRS.$$.PaneAircraftTrails = 'Aircraft Trails';
    VRS.$$.PaneAudio = 'Audio';
    VRS.$$.PaneAutoSelect = 'Auto-selection';
    VRS.$$.PaneCurrentLocation = 'Current Location';
    VRS.$$.PaneDataFeed = 'Data Feed';
    VRS.$$.PaneDetailSettings = 'Aircraft Details';
    VRS.$$.PaneInfoWindow = 'Aircraft Info Window';
    VRS.$$.PaneListSettings = 'List Settings';
    VRS.$$.PaneManyAircraft = 'Multiple Aircraft Reports';
    VRS.$$.PanePermanentLink = 'Permanent Link';
    VRS.$$.PaneRangeCircles = 'Range Circles';
    VRS.$$.PaneReceiverRange = 'Receiver Range';
    VRS.$$.PaneSingleAircraft = 'Single Aircraft Reports';
    VRS.$$.PaneSortAircraftList = 'Sort Aircraft List';
    VRS.$$.PaneSortReport = 'Sort Report';
    VRS.$$.PaneUnits = 'Units';
    VRS.$$.Pause = 'Pause';
    VRS.$$.Picture = 'Picture';
    VRS.$$.PictureOrThumbnails = 'Picture or Thumbnails';
    VRS.$$.PinTextLines = 'Number of label lines';
    VRS.$$.PinTextNumber = 'Aircraft label line {0}';
    VRS.$$.Piston = 'Piston';
    VRS.$$.Pixels = 'pixels';
    VRS.$$.PopularName = 'Popular Name';
    VRS.$$.PositionAge = 'Position Age';
    VRS.$$.PositionAndAltitude = 'Position and altitude';
    VRS.$$.PositionAndSpeed = 'Position and speed';
    VRS.$$.PositioningFlight = 'Positioning flight';
    VRS.$$.PositioningFlightShort = 'POSITION';
    VRS.$$.PoweredByVRS = 'Powered by Virtual Radar Server';
    VRS.$$.PressureAltitude = 'Altitude (Pressure)';
    VRS.$$.Pressures = 'Pressures';
    VRS.$$.PreviousId = 'Previous ID';
    VRS.$$.Quantity = 'Quantity';
    VRS.$$.RadioMast = 'Radio Mast';
    VRS.$$.RangeCircleEvenColour = 'Even circle colour';
    VRS.$$.RangeCircleOddColour = 'Odd circle colour';
    VRS.$$.RangeCircles = 'Range Circles';
    VRS.$$.Receiver = 'Receiver';
    VRS.$$.ReceiverRange = 'Receiver Range';
    VRS.$$.Refresh = 'Refresh';
    VRS.$$.Registration = 'Registration';
    VRS.$$.RegistrationAndIcao = 'Reg. & Icao';
    VRS.$$.Remove = 'Remove';
    VRS.$$.RemoveAll = 'Remove All';
    VRS.$$.ReportCallsignInvalid = 'Callsign Report';
    VRS.$$.ReportCallsignValid = 'Callsign Report for {0}';
    VRS.$$.ReportEmpty = 'No records were found for the criteria supplied';
    VRS.$$.ReportFreeForm = 'Free-form Report';
    VRS.$$.ReportIcaoInvalid = 'ICAO Report';
    VRS.$$.ReportIcaoValid = 'ICAO Report for {0}';
    VRS.$$.ReportRegistrationInvalid = 'Registration Report';
    VRS.$$.ReportRegistrationValid = 'Registration Report for {0}';
    VRS.$$.Reports = 'Reports';
    VRS.$$.ReportsAreDisabled = 'Server permissions prohibit the running of reports';
    VRS.$$.ReportTodaysFlights = 'Today\'s Flights';
    VRS.$$.ReportYesterdaysFlights = 'Yesterday\'s Flights';
    VRS.$$.ResetClustererZoomLevel = 'Reset cluster aircraft zoom level';
    VRS.$$.Resume = 'Resume';
    VRS.$$.Reversing = 'Reversing';
    VRS.$$.ReversingShort = 'REV';
    VRS.$$.Rocket = 'Rocket';
    VRS.$$.Route = 'Route';
    VRS.$$.RouteFull = 'Route (full)';
    VRS.$$.RouteNotKnown = 'Route not known';
    VRS.$$.RouteShort = 'Route (short)';
    VRS.$$.RowNumber = 'Row Number';
    VRS.$$.Rows = 'Rows';
    VRS.$$.RunReport = 'Run Report';
    VRS.$$.SayAlpha = 'alfuh';
    VRS.$$.SayAy = 'Ey';
    VRS.$$.SayBravo = 'bravo';
    VRS.$$.SayCallsign = 'Call sign {0}.';
    VRS.$$.SayCharlie = 'charlie';
    VRS.$$.SayDelta = 'delta';
    VRS.$$.SayEcho = 'echo';
    VRS.$$.SayFoxtrot = 'foxed-rot';
    VRS.$$.SayFromTo = 'Travelling from {0} to {1}.';
    VRS.$$.SayFromToVia = 'Travelling from {0} via {1} to {2}.';
    VRS.$$.SayGolf = 'golf';
    VRS.$$.SayHotel = 'hotel';
    VRS.$$.SayHyphen = 'hyphen';
    VRS.$$.SayIcao = 'eye co {0}.';
    VRS.$$.SayIndia = 'india';
    VRS.$$.SayJuliet = 'juliet';
    VRS.$$.SayKilo = 'key-low';
    VRS.$$.SayLima = 'leamah';
    VRS.$$.SayMike = 'mike';
    VRS.$$.SayModelIcao = 'Type {0}.';
    VRS.$$.SayNovember = 'november';
    VRS.$$.SayOperator = 'Operated by {0}.';
    VRS.$$.SayOscar = 'oscar';
    VRS.$$.SayPapa = 'papa';
    VRS.$$.SayQuebec = 'quebec';
    VRS.$$.SayRegistration = 'Registration {0}.';
    VRS.$$.SayRomeo = 'romeo';
    VRS.$$.SayRouteNotKnown = 'Route not known.';
    VRS.$$.SaySierra = 'sierra';
    VRS.$$.SayTango = 'tango';
    VRS.$$.SayUniform = 'uniform';
    VRS.$$.SayVictor = 'victor';
    VRS.$$.SayWhiskey = 'whiskey';
    VRS.$$.SayXRay = 'x-ray';
    VRS.$$.SayYankee = 'yankee';
    VRS.$$.SayZed = 'ZED';
    VRS.$$.SayZulu = 'zulu';
    VRS.$$.SeaPlane = 'Seaplane';
    VRS.$$.Select = 'Select';
    VRS.$$.SeparateTwoValues = ' and ';
    VRS.$$.SerialNumber = 'Serial';
    VRS.$$.ServerFetchFailedBody = 'Could not fetch from the server. The error is "{0}" and the status is "{1}".';
    VRS.$$.ServerFetchFailedTitle = 'Fetch Failed';
    VRS.$$.ServerFetchTimedOut = 'The request has timed out.';
    VRS.$$.ServerReportExceptionBody = 'The server encountered an exception while generating the report. The exception was "{0}"';
    VRS.$$.ServerReportExceptionTitle = 'Server Exception';
    VRS.$$.SetClustererZoomLevel = 'Cluster aircraft at this zoom level';
    VRS.$$.SetCurrentLocation = 'Set current location';
    VRS.$$.Settings = 'Settings';
    VRS.$$.SettingsPage = 'Settings Page';
    VRS.$$.Shortcuts = 'Shortcuts';
    VRS.$$.ShowAltitudeStalk = 'Show altitude stalk';
    VRS.$$.ShowAltitudeType = 'Show altitude type';
    VRS.$$.ShowCurrentLocation = 'Show current location';
    VRS.$$.ShowDetail = 'Show detail';
    VRS.$$.ShowEmergencySquawks = 'Show emergency squawks';
    VRS.$$.ShowEmptyValues = 'Show empty values';
    VRS.$$.ShowForAllAircraft = 'Show for all aircraft';
    VRS.$$.ShowForSelectedOnly = 'Show just for the selected aircraft';
    VRS.$$.ShowInterestingAircraft = 'Show interesting aircraft';
    VRS.$$.ShowRangeCircles = 'Show range circles';
    VRS.$$.ShowShortTrails = 'Show short trails';
    VRS.$$.ShowSpeedType = 'Show speed type';
    VRS.$$.ShowTrackType = 'Show heading type';
    VRS.$$.ShowUnits = 'Show units';
    VRS.$$.ShowVerticalSpeedType = 'Show vertical speed type';
    VRS.$$.ShowVsiInSeconds = 'Show vertical speed per second';
    VRS.$$.SignalLevel = 'Signal Level';
    VRS.$$.Silhouette = 'Silhouette';
    VRS.$$.SilhouetteAndOpFlag = 'Sil. & Op. Flag';
    VRS.$$.SiteTimedOut = 'The site has been paused due to inactivity. Close this message box to resume updates.';
    VRS.$$.SortBy = 'Sort by';
    VRS.$$.Species = 'Species';
    VRS.$$.Speed = 'Speed';
    VRS.$$.SpeedGraph = 'Speed Graph';
    VRS.$$.Speeds = 'Speeds';
    VRS.$$.SpeedType = 'Speed Type';
    VRS.$$.Squawk = 'Squawk';
    VRS.$$.Squawk7000 = 'No squawk assigned';
    VRS.$$.Squawk7500 = 'Aircraft hijacking';
    VRS.$$.Squawk7600 = 'Radio failure';
    VRS.$$.Squawk7700 = 'General emergency';
    VRS.$$.SquawkAndIdent = 'Squawk & Ident';
    VRS.$$.Start = 'Start';
    VRS.$$.StartsWith = 'Starts With';
    VRS.$$.StartTime = 'Start Time';
    VRS.$$.Status = 'Status';
    VRS.$$.StatuteMileAbbreviation = '{0} mi';
    VRS.$$.StatuteMiles = 'Statute Miles';
    VRS.$$.StorageEngine = 'Storage engine';
    VRS.$$.StorageSize = 'Storage size';
    VRS.$$.StrokeOpacity = 'Stroke opacity';
    VRS.$$.SubmitRoute = 'Submit route';
    VRS.$$.SubmitRouteCorrection = 'Submit route correction';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut = 'Suppress altitude stalk when zoomed out';
    VRS.$$.TargetAltitude = 'Target Altitude';
    VRS.$$.TargetHeading = 'Target Heading';
    VRS.$$.ThenBy = 'then by';
    VRS.$$.Tiltwing = 'Tiltwing';
    VRS.$$.TimeTracked = 'Time Tracked';
    VRS.$$.Tisb = 'TIS-B';
    VRS.$$.TitleAircraftDetail = 'Aircraft Detail';
    VRS.$$.TitleAircraftList = 'Aircraft List';
    VRS.$$.TitleFlightDetail = 'Detail';
    VRS.$$.TitleFlightsList = 'Flights';
    VRS.$$.TitleSiteTimedOut = 'Timed Out';
    VRS.$$.ToAltitude = 'To {0}';
    VRS.$$.TotalHours = 'Total Hours';
    VRS.$$.TrackingCountAircraft = 'Tracking {0:N0} aircraft';
    VRS.$$.TrackingCountAircraftOutOf = 'Tracking {0:N0} aircraft (out of {1:N0})';
    VRS.$$.TrackingOneAircraft = 'Tracking 1 aircraft';
    VRS.$$.TrackingOneAircraftOutOf = 'Tracking 1 aircraft (out of {0:N0})';
    VRS.$$.TransponderType = 'Transponder';
    VRS.$$.TransponderTypeFlag = 'Transponder Flag';
    VRS.$$.TrueAirSpeed = 'True';
    VRS.$$.TrueAirSpeedShort = 'TAS';
    VRS.$$.TrueHeading = 'True heading';
    VRS.$$.TrueHeadingShort = 'True';
    VRS.$$.Turbo = 'Turbo';
    VRS.$$.Unknown = 'Unknown';
    VRS.$$.UpdateAircraftLookup = 'Update lookup details';
    VRS.$$.UseBrowserLocation = 'Use GPS location';
    VRS.$$.UsePressureAltitude = 'Use pressure altitude';
    VRS.$$.UseRelativeDates = 'Use relative dates';
    VRS.$$.UserTag = 'User Tag';
    VRS.$$.UseShortLabels = 'Use short labels';
    VRS.$$.VerticalSpeed = 'Vertical Speed';
    VRS.$$.VerticalSpeedType = 'Vertical Speed Type';
    VRS.$$.VirtualRadar = 'Virtual Radar';
    VRS.$$.Volume = 'Volume';
    VRS.$$.VrsVersion = 'Version {0}';
    VRS.$$.WakeTurbulenceCategory = 'Wake Turbulence';
    VRS.$$.Warning = 'Warning';
    VRS.$$.WorkingInOfflineMode = 'Working in offline mode';
    VRS.$$.WtcHeavy = 'Heavy';
    VRS.$$.WtcLight = 'Light';
    VRS.$$.WtcMedium = 'Medium';
    VRS.$$.YearBuilt = 'Year Built';
    VRS.$$.Yes = 'Yes';
    // [[ MARKER END SIMPLE STRINGS ]]

    /*
        See the notes below against 'Complicated strings'. This function takes an array of stopovers in a route and
        joins them together into a single sentence for the text-to-speech conversion. So if it is passed an arary
        of "First stopover", "Second stopover" and "Third stopover" then it will return the string
        "First stopover, Second stopover and Third stopover".
     */
    VRS.$$.sayStopovers = function(stopovers)
    {
        var result = '';
        var length = stopovers.length;
        for(var i = 0;i < length;++i) {
            var stopover = stopovers[i];
            var isFirstStopover = i === 0;
            var isLastStopover = i + 1 === length;
            var isMiddleStopover = !isFirstStopover && !isLastStopover;

            if(isLastStopover)        result += ' and ';
            else if(isMiddleStopover) result += ', ';

            result += stopover;
        }

        return result;
    };

    // Complicated strings
    /*
       These are javascript functions that take a number of parameters and format some text from them. A function is
       always of this form:
         VRS.$$.<name of function> = function(parameter 1, parameter 2, ..., parameter n)
         {
            ... body of function ...
         };
       Only translate text within single apostrophes in functions. If the English version of a function will suffice
       for your language then delete the function entirely so that the site falls back onto the English version.

       If you are not comfortable with translating text within functions then let me know how the text should be
       displayed in your language and I'll do the function for you.
    */

    /**
     * Returns an elapsed time as a string.
     * @param {number} hours
     * @param {number} minutes
     * @param {number} seconds
     * @param {bool=} showZeroHours
     * @returns {string}
     */
    VRS.$$.formatHoursMinutesSeconds = function(hours, minutes, seconds, showZeroHours)
    {
        /*
            jQuery Globalize only allows formatting of full date-times, which is no good when we want to display spans
            of time larger than 24 hours. The English version of this returns either H:MM:SS or MM:SS depending on
            whether hours is zero and whether showZeroHours is true or false.
        */
        var result = '';
        if(hours || showZeroHours) result = hours.toString() + ':';
        result += VRS.stringUtility.formatNumber(minutes, '00') + ':';
        result += VRS.stringUtility.formatNumber(seconds, '00');

        return result;
    };

    /**
     * Returns the count of engines and the engine type as a translated string.
     * @param {string} countEngines
     * @param {string} engineType
     * @returns {string}
     */
    VRS.$$.formatEngines = function(countEngines, engineType)
    {
        /*
           Returns a string showing the count of engines and the engine type. Examples in English are:
             countEngines = '1' and engine type = VRS.EngineType.Jet:     'Single jet'
             countEngines = '10' and engine type = VRS.EngineType.Piston: '10 piston'
        */
        var result = '';

        switch(countEngines) {
            case 'C':       result = 'Coupled'; break;
            case '1':       result = 'Single'; break;
            case '2':       result = 'Twin'; break;
            case '3':       result = 'Three'; break;
            case '4':       result = 'Four'; break;
            case '5':       result = 'Five'; break;
            case '6':       result = 'Six'; break;
            case '7':       result = 'Seven'; break;
            case '8':       result = 'Eight'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' electric'; break;
            case VRS.EngineType.Jet:        result += ' jet'; break;
            case VRS.EngineType.Piston:     result += ' piston'; break;
            case VRS.EngineType.Rocket:     result += ' rocket'; break;
            case VRS.EngineType.Turbo:      result += ' turbo'; break;
        }

        return result;
    };

    /**
     * Translates the wake turbulence category description.
     * @param {string} category
     * @param {bool} ignoreNone
     * @param {bool} expandedDescription
     * @returns {string}
     */
    VRS.$$.formatWakeTurbulenceCategory = function(category, ignoreNone, expandedDescription)
    {
        /*
           Returns a string showing the wake turbulence category. What makes this different from a simple
           substitution is that in some places I want to show the weight limits for each category. In
           English these follow the category - e.g. Light (up to 7 tons) - but this may not be appropriate
           in other locales.
        */

        var result = '';
        if(category) {
            switch(category) {
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = 'None'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = 'Light'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = 'Medium'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = 'Heavy'; break;
                default: throw 'Unknown wake turbulence category ' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (up to 7 tons)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (up to 135 tons)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (over 135 tons)'; break;
                }
            }
        }

        return result;
    };

    /**
     * Returns the full route details.
     * @param {string} from
     * @param {string} to
     * @param {string[]} via
     * @returns {string}
     */
    VRS.$$.formatRoute = function(from, to, via)
    {
        /*
            Returns a string showing the full route. From and to are strings describing the airport (in English - these
            come out of a database of thousands of English airport descriptions, it would be a nightmare to translate them)
            and via is an array of strings describing airports. In English the end result would be one of:
                From AIRPORT to AIRPORT
                To AIRPORT
                From AIRPORT to AIRPORT via AIRPORT
                From AIRPORT to AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
                To AIRPORT via AIRPORT
                To AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
         */
        var result = '';
        if(from) result = 'From ' + from;
        if(to) {
            if(result.length) result += ' to ';
            else              result = 'To ';
            result += to;
        }
        var stopovers = via ? via.length : 0;
        if(stopovers > 0) {
            result += ' via';
            for(var i = 0;i < stopovers;++i) {
                var stopover = via[i];
                if(i > 0) result += ',';
                result += ' ' + stopover;
            }
        }

        return result;
    };

    /**
     * Translates the country name.
     * @param {string} englishCountry
     * @returns {string}
     */
    VRS.$$.translateCountry = function(englishCountry)
    {
        /*
            Returns a translation of the country. If you are happy with English country names then just delete this function
            and the English version will be used. Otherwise you can delete the following line:
        */

        return englishCountry;

        /*
            and then remove the "//" from the start of every line after these comments and fill in the translations for the
            lines that start with 'case'. The format for every translated country should be:
                case 'English country name':  return 'ENTER YOUR TRANSLATION HERE';
            Unfortunately the English country names are coming out of the StandingData.sqb database file on the server. You
            can either extract them from there (if you can use sqlite3) or email me and I'll send you a version of this
            function with all of the English country names filled in for the current set of countries. You'll need to update
            this as-and-when the countries change. There are currently about 250 countries in the Standing Data database but
            any that you do not provide a translation for will just be shown in English. Delete the case lines for countries
            where your language's name is the same as the English name.

            If you need to use an apostrophe in your translation then change the single-quotes around the name to double-
            quotes, e.g.
                case 'Ivory Coast':     return "Republique de Cote d'Ivoire";
        */

        // switch(englishCountry) {
        //     case 'Germany':          return 'Allemagne';
        //     case 'United Kingdom':   return 'Grande-Bretagne';
        //     default:                 return englishCountry;
        // }
    };
}(window.VRS = window.VRS || {}, jQuery));
