// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Moq;
using Tests.Mocks;
using VirtualRadar;
using VirtualRadar.Configuration;
using VirtualRadar.Convert;
using VirtualRadar.Message;
using VirtualRadar.Receivers;
using VirtualRadar.StandingData;
using VirtualRadar.WebSite;

namespace Tests.VirtualRadar.WebSite
{
    [TestClass]
    public class AircraftListJsonBuilder_Tests
    {
        private AircraftListJsonBuilder _Builder;
        private AircraftListJsonBuilderArgs _Args;
        private AircraftListJsonBuilderFilter _Filter;
        private Mock<IReceiverFactory> _ReceiverFactory;
        private Mock<IReceiver> _Receiver;
        private List<Aircraft> _AllAircraft;
        private long _AircraftListToArrayStamp;
        private Mock<IAircraftList> _AircraftList;
        private List<Mock<IReceiver>> _AllReceivers;
        private MockSettings<AircraftMapSettings> _AircraftMapSettings;
        private MockSettings<AircraftPictureSettings> _AircraftPictureSettings;
        private MockSettings<InternetClientSettings> _InternetClientSettings;
        private MockSettings<OperatorAndTypeFlagSettings> _OperatorAndTypeFlagSettings;
        private MockSettings<WebClientSettings> _WebClientSettings;
        private MockFileSystem _FileSystem;
        private MockClock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _Args = new();
            _Filter = new();

            _AllAircraft = [];
            _AircraftListToArrayStamp = 0L;
            _AircraftList = MockHelper.CreateMock<IAircraftList>();
            _AircraftList
                .Setup(r => r.ToArray(out It.Ref<long>.IsAny, true))
                .Returns((out long stamp, bool _) => {
                    stamp = _AircraftListToArrayStamp;
                    return _AllAircraft.ToArray();
                });

            _AllReceivers = [];
            _Receiver = SetupReceiver("Mock Receiver", receiver: null, aircraftList: _AircraftList);

            _ReceiverFactory = MockHelper.CreateMock<IReceiverFactory>();
            _ReceiverFactory
                .Setup(r => r.FindById(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((int _, bool _, bool _) => _Receiver?.Object);
            _ReceiverFactory
                .SetupGet(r => r.Receivers)
                .Returns(() => _AllReceivers.Select(r => r.Object).ToArray());

            _AircraftMapSettings = new(new());
            _AircraftPictureSettings = new(new());
            _InternetClientSettings = new(new());
            _OperatorAndTypeFlagSettings = new(new());
            _WebClientSettings = new(new());

            _FileSystem = new();
            _Clock = new();

            _Builder = new(
                _AircraftMapSettings,
                _AircraftPictureSettings,
                _InternetClientSettings,
                _OperatorAndTypeFlagSettings,
                _WebClientSettings,
                _ReceiverFactory.Object,
                _FileSystem,
                _Clock
            );
        }

        private Mock<IReceiver> SetupReceiver(
            string name,
            Mock<IReceiver> receiver,
            int? id = null,
            Mock<IAircraftList> aircraftList = null
        )
        {
            receiver ??= MockHelper.CreateMock<IReceiver>();
            if(!_AllReceivers.Contains(receiver)) {
                _AllReceivers.Add(receiver);
            }

            id ??= _AllReceivers.DefaultIfEmpty().Max(receiver => receiver?.Object.Id ?? 0) + 1;

            receiver.SetupGet(r => r.Id).Returns(id.Value);
            receiver.SetupGet(r => r.Name).Returns(name);

            aircraftList ??= MockHelper.CreateMock<IAircraftList>();
            receiver.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);

            return receiver;
        }

        private Aircraft SetupAircraft(
            Aircraft aircraft = null,
            int? id = null,
            long? stamp = null,
            Action<TransponderMessage> fillMessage = null,
            LookupOutcome lookup = null
        )
        {
            if(aircraft == null) {
                id ??= _AllAircraft.DefaultIfEmpty().Max(aircraft => aircraft?.Id ?? 0) + 1;
                aircraft = new(id.Value, _Clock);
            }

            if(!_AllAircraft.Contains(aircraft)) {
                _AllAircraft.Add(aircraft);
            }

            if(fillMessage != null) {
                var message = new TransponderMessage(aircraft.Id);
                fillMessage(message);
                if(stamp != null) {
                    PostOffice.SetNextStampForUnitTest(stamp.Value);
                }
                aircraft.CopyFromMessage(message);
            }
            if(lookup != null) {
                if(stamp != null) {
                    PostOffice.SetNextStampForUnitTest(stamp.Value);
                }
                lookup.Success = true;
                aircraft.CopyFromLookup(lookup);
            }

            return aircraft;
        }

        private void AddTrailData(
            Aircraft aircraft,
            long stamp,
            Location location,
            float? heading = null,
            int? altitude = null,
            float? speed = null
        )
        {
            var message = new TransponderMessage(aircraft.Id) {
                Location = location,
                AltitudeFeet = altitude,
                GroundSpeedKnots = speed,
                GroundTrackDegrees = heading,
            };
            PostOffice.SetNextStampForUnitTest(stamp);
            aircraft.CopyFromMessage(message);
        }

        private static Route SetupRoute(Airport fromAirport, Airport toAirport, Airport[] stopovers = null)
        {
            var result = new Route() {
                From =      fromAirport,
                To =        toAirport,
            };
            result.Stopovers.AddRange(stopovers ?? []);
            return result;
        }

        private static Airport SetupAirport(string icao, string iata)
        {
            return new() {
                IcaoCode = icao,
                IataCode = iata,
            };
        }

        private static Route LHR_SIN_SYD()
        {
            return SetupRoute(
                fromAirport: SetupAirport("EGLL", "LHR"),
                toAirport:   SetupAirport("YSSY", "SYD"),
                stopovers:   [ SetupAirport("WSSS", "SIN"), ]
            );
        }

        [TestMethod]
        public void Build_Returns_Empty_Json_For_Empty_AircraftList()
        {
            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true,  false)]
        [DataRow(true,  true)]
        public void Build_Passes_Receiver_Modifiers_To_Receiver_Factory(
            bool ignoreHidden,
            bool fallbackToDefault
        )
        {
            _Builder.Build(
                _Args,
                ignoreInvisibleSources: ignoreHidden,
                fallbackToDefaultSource: fallbackToDefault
            );

            _ReceiverFactory.Verify(r => r.FindById(
                It.IsAny<int>(),
                ignoreHidden,
                fallbackToDefault
            ), Times.Once());
        }

        [TestMethod]
        public void Build_Returns_Empty_Json_When_There_Are_No_Feeds()
        {
            _Receiver = null;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);
            Assert.AreEqual(-1, json.SourceFeedId);
        }

        [TestMethod]
        public void Build_Returns_Requested_SourceFeedId_When_There_Are_No_Feeds()
        {
            _Receiver = null;
            _Args.ReceiverId = 9;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(9, json.SourceFeedId);
        }

        [TestMethod]
        public void Build_Returns_Full_List_Of_Feeds()
        {
            var anotherReceiver = SetupReceiver("Another Receiver", receiver: null);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(2, json.Feeds.Count);

            var receiverFeed = json.Feeds.Single(feed => feed.UniqueId == _Receiver.Object.Id);
            Assert.AreEqual(_Receiver.Object.Name, receiverFeed.Name);

            var anotherFeed = json.Feeds.Single(feed => feed.UniqueId == anotherReceiver.Object.Id);
            Assert.AreEqual(anotherReceiver.Object.Name, anotherFeed.Name);
        }

        [TestMethod]
        public void Build_Does_Not_Return_Feeds_If_Not_Required()
        {
            _Args.FeedsNotRequired = true;
            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            Assert.AreEqual(0, json.Feeds.Count);
        }

        [TestMethod]
        public void Build_Returns_Configured_Operator_And_Type_Flag_Dimensions()
        {
            _OperatorAndTypeFlagSettings.Value = new(
                FlagHeightPixels: 987,
                FlagWidthPixels:  654
            );

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(987, json.FlagHeight);
            Assert.AreEqual(654, json.FlagWidth);
        }

        [TestMethod]
        public void Build_Sets_ServerTime_Correctly()
        {
            // In earlier versions of VRS this was the actual server time but with code to ensure that
            // the ticks could never go backwards. In VRS Core we have the post office stamp for that.
            PostOffice.SetNextStampForUnitTest(1234);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(1234L, json.ServerTime);
        }

        [TestMethod]
        [DataRow("/foo", null,   false, "operator flags folder is null")]
        [DataRow("/foo", "",     false, "operator flags folder is an empty string")]
        [DataRow("/foo", "/foo", true,  "operator flags folder exists")]
        [DataRow("/foo", "/dne", false, "operator flags folder does not exist")]
        public void Build_Sets_ShowFlags_If_Configured_Folder_Exists(
            string fileSystemFolder,
            string operatorFlagsFolder,
            bool expected,
            string condition
        )
        {
            if(!String.IsNullOrEmpty(fileSystemFolder)) {
                _FileSystem.AddFolder(fileSystemFolder);
            }
            _OperatorAndTypeFlagSettings.Value = new(OperatorFlagsFolder: operatorFlagsFolder);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.ShowFlags, $"Failed when {condition}");
            Assert.AreEqual(false, json.ShowSilhouettes);
        }

        [TestMethod]
        [DataRow("/foo", null,   false, "type flags folder is null")]
        [DataRow("/foo", "",     false, "type flags folder is an empty string")]
        [DataRow("/foo", "/foo", true,  "type flags folder exists")]
        [DataRow("/foo", "/dne", false, "type flags folder does not exist")]
        public void Build_Sets_ShowSilhouettes_If_Configured_Folder_Exists(
            string fileSystemFolder,
            string typeFlagsFolder,
            bool expected,
            string condition
        )
        {
            if(!String.IsNullOrEmpty(fileSystemFolder)) {
                _FileSystem.AddFolder(fileSystemFolder);
            }
            _OperatorAndTypeFlagSettings.Value = new(TypeFlagsFolder: typeFlagsFolder);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.ShowSilhouettes, $"Failed when {condition}");
            Assert.AreEqual(false, json.ShowFlags);
        }

        [TestMethod]
        [DataRow("/foo", null,   false, "configured folder is null")]
        [DataRow("/foo", "",     false, "configured folder is an empty string")]
        [DataRow("/foo", "/foo", true,  "configured folder exists")]
        [DataRow("/foo", "/dne", false, "configured folder does not exist")]
        public void Build_Sets_ShowPictures_If_Configured_Folder_Exists(
            string fileSystemFolder,
            string pictureFolder,
            bool expected,
            string condition
        )
        {
            if(!String.IsNullOrEmpty(fileSystemFolder)) {
                _FileSystem.AddFolder(fileSystemFolder);
            }
            _AircraftPictureSettings.Value = new(
                LocalPicturesFolder: pictureFolder
            );

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.ShowPictures, $"Failed when {condition}");
        }

        [TestMethod]
        [DataRow(false, false, true,  "is not Internet client, cannot show to Internet client")]
        [DataRow(false, true,  true,  "is not Internet client, can show to Internet client")]
        [DataRow(true,  false, false, "is Internet client, cannot show to Internet client")]
        [DataRow(true,  true,  true,  "is Internet client, can show to Internet client")]
        public void Build_Can_Hide_Pictures_From_Internet_Clients(
            bool isInternetClient,
            bool canShowToInternetClient,
            bool expected,
            string condition
        )
        {
            _Args.IsInternetClient = isInternetClient;
            _FileSystem.AddFolder("/foo");
            _AircraftPictureSettings.Value = new(LocalPicturesFolder: "/foo");
            _InternetClientSettings.Value = new(CanShowPictures: canShowToInternetClient);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.ShowPictures, $"Failed when {condition}");
        }

        [TestMethod]
        public void Build_Sets_ShortTrailLength_From_Configuration_Options()
        {
            _AircraftMapSettings.Value = new(ShortTrailLengthSeconds: 1234);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(1234, json.ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void Build_Sends_ShortTrailLength_Even_If_Browser_Requested_Full_Trails()
        {
            _Args.TrailType = TrailType.Full;
            _AircraftMapSettings.Value = new(ShortTrailLengthSeconds: 88);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(88, json.ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void Build_Always_Sets_Source_To_1()
        {
            // In earlier versions of VRS there was the concept of a list of fake aircraft and a list of
            // flight sim aircraft. This version has flags against the aircraft to cover that, the aircraft
            // list can contain a mix of fake and real aircraft.
            //
            // Setting source to 1 maintains some kind of backwards compatability with legacy sites.
            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(1, json.Source);
        }

        [TestMethod]
        public void Build_Returns_Aircraft_In_List()
        {
            var aircraft1 = SetupAircraft();
            var aircraft2 = SetupAircraft();

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(2, json.Aircraft.Count);
            Assert.IsTrue(json.Aircraft.Any(aircraft => aircraft.UniqueId == aircraft1.Id));
            Assert.IsTrue(json.Aircraft.Any(aircraft => aircraft.UniqueId == aircraft2.Id));
        }

        [TestMethod]
        public void Build_Returns_Count_Of_Aircraft_In_List()
        {
            SetupAircraft();
            SetupAircraft();

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(2, json.AvailableAircraft);
        }

        [TestMethod]
        public void Build_Sets_LastDataVersion_From_AircraftList_Stamp()
        {
            _AircraftListToArrayStamp = 1234L;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual("1234", json.LastDataVersion);
        }

        [TestMethod]
        public void Build_Sets_LastDataVersion_To_Zero_If_Receiver_Is_Missing()
        {
            _Receiver = null;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual("0", json.LastDataVersion);
        }

        [TestMethod]
        [DataRow(1010.0F, true,  1010.0F)]
        [DataRow(1010.0F, false, null)]
        public void Build_Sets_Aircraft_AirPressure(float? value, bool hasChanged, float? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { AirPressureInHg = value, AirPressureLookupAttempted = true });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].AirPressureInHg);
        }

        [TestMethod]
        [DataRow(1425, true, 1425)]
        [DataRow(1425, false, null)]
        public void Build_Sets_Aircraft_Altitude(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.AltitudeFeet = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Altitude);
        }

        [TestMethod]
        [DataRow(AltitudeType.Radar, true,  (int)AltitudeType.Radar)]
        [DataRow(AltitudeType.Radar, false, null)]
        public void Build_Sets_Aircraft_AltitudeType(AltitudeType value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.AltitudeType = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].AltitudeType);
        }

        [TestMethod]
        [DataRow("AGW123", true,  "AGW123")]
        [DataRow("AGW123", false, null)]
        public void Build_Sets_Aircraft_Callsign(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Callsign = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Callsign);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_CallsignIsSuspect(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.CallsignIsSuspect = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].CallsignIsSuspect);
        }

        [TestMethod]
        [DataRow("A1", true,  "A1")]
        [DataRow("A1", false, null)]
        public void Build_Sets_Aircraft_ConstructionNumber(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { ConstructionNumber = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].ConstructionNumber);
        }

        [TestMethod]
        public void Build_Sets_Aircraft_CountMessagesReceived()
        {
            SetupAircraft(fillMessage: m => m.Icao24 = new(0x123456));

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(1, json.Aircraft[0].CountMessagesReceived);
        }

        [TestMethod]
        [DataRow(AirportCodeType.Iata, true,  "SYD")]
        [DataRow(AirportCodeType.Icao, true,  "YSSY")]
        [DataRow(AirportCodeType.Iata, false, null)]
        [DataRow(AirportCodeType.Icao, false, null)]
        public void Build_Sets_Aircraft_Destination(AirportCodeType showType, bool hasChanged, string expected)
        {
            _WebClientSettings.Value = new(PreferredAirportCodeType: showType);
            SetupAircraft(stamp: 2, lookup: new() { Route = LHR_SIN_SYD() });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Destination);
        }

        [TestMethod]
        [DataRow(null, 7500, true,  true)]
        [DataRow(7500, 7501, true,  false)]
        [DataRow(null, 7600, true,  true)]
        [DataRow(7600, 7601, true,  false)]
        [DataRow(null, 7700, true,  true)]
        [DataRow(7700, 7701, true,  false)]
        [DataRow(7500, 7500, false, null)]
        public void Build_Sets_Aircraft_Emergency(int? oldSquawk, int? squawk, bool hasChanged, bool? expected)
        {
            var aircraft = SetupAircraft(stamp: 1, fillMessage: m => m.Squawk = oldSquawk);
            SetupAircraft(aircraft, stamp: 2, fillMessage: m => m.Squawk = squawk);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Emergency);
        }

        [TestMethod]
        [DataRow(EnginePlacement.Unknown,    true,  0)]
        [DataRow(EnginePlacement.AftMounted, true,  1)]
        [DataRow(EnginePlacement.AftMounted, false, null)]
        public void Build_Sets_Aircraft_EnginePlacement(EnginePlacement value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { EnginePlacement = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].EnginePlacement);
        }

        [TestMethod]
        [DataRow(EngineType.None,   true,  0)]
        [DataRow(EngineType.Jet,    true,  3)]
        [DataRow(EngineType.Jet,    false, null)]
        public void Build_Sets_Aircraft_EngineType(EngineType value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { EngineType = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].EngineType);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Build_Sets_Aircraft_FirstSeen(bool hasChanged)
        {
            _Clock.Now = new DateTimeOffset(2013, 12, 11, 10, 9, 8, TimeSpan.Zero);
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(1));
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            var actual = json.Aircraft[0].FirstSeen;
            if(!hasChanged) {
                Assert.IsNull(actual);
            } else {
                Assert.IsNotNull(actual);
                Assert.AreEqual(_Clock.UtcNow, actual.Value);
            }
        }

        [TestMethod]
        [DataRow(1425, true, 1425)]
        [DataRow(1425, false, null)]
        public void Build_Sets_Aircraft_GeometricAltitude(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => {
                m.AltitudeFeet = value;
                m.AltitudeType = AltitudeType.Radar;
            });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].GeometricAltitude);
        }

        [TestMethod]
        [DataRow(200.5F, true,  200.5F)]
        [DataRow(200.5F, false, null)]
        public void Build_Sets_Aircraft_GroundSpeed(float? value, bool hasChanged, float? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.GroundSpeedKnots = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].GroundSpeed);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_HasPicture(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { AircraftPicture = value == null
                ? null
                : value.Value
                    ? new LookupImageFile("/foo", 1, 1)
                    : new LookupImageFile("", 0, 0)
            });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].HasPicture);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_HasSignalLevel(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.SignalLevelSent = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].HasSignalLevel);
        }

        [TestMethod]
        [DataRow(0xabc123, true,  "ABC123")]
        [DataRow(0xabc123, false, null)]
        public void Build_Sets_Aircraft_Icao24(int value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(value));
            _Args.AlwaysShowIcao = false;
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Icao24);
        }

        [TestMethod]
        public void Build_Sets_Aircraft_Icao24_When_Args_Request_It_Even_If_Unchanged()
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(0xabc123));
            _Args.AlwaysShowIcao = true;
            _Args.PreviousDataVersion = 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual("ABC123", json.Aircraft[0].Icao24);
        }

        [TestMethod]
        public void Build_Sets_Aircraft_Icao24_When_Original_Is_Invalid()
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = Icao24.Invalid);
            _Args.PreviousDataVersion = 1;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual("", json.Aircraft[0].Icao24);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Icao24Country(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Icao24Country = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Icao24Country);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_Icao24Invalid(bool? isInvalid, bool icaoHasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => {
                if(isInvalid != null) {
                    m.Icao24 = isInvalid.Value
                        ? Icao24.Invalid
                        : new Icao24(1);
                }
            });
            _Args.PreviousDataVersion = icaoHasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Icao24Invalid);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_IdentActive(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.IdentActive = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].IdentActive);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_IsCharterFlight(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { IsCharterFlight = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].IsCharterFlight);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_IsMilitary(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { IsMilitary = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].IsMilitary);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_IsPositioningFlight(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { IsPositioningFlight = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].IsPositioningFlight);
        }

        [TestMethod]
        public void Build_Sets_Aircraft_IsSatcomFeed_Always_Null()
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(1));
            _Args.PreviousDataVersion = 1;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.IsNull(json.Aircraft[0].IsSatcomFeed);
        }

        [TestMethod]
        [DataRow(51.234, true,  51.234)]
        [DataRow(51.234, false, null)]
        public void Build_Sets_Aircraft_Latitude(double value, bool hasChanged, double? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Location = new(value, 0));
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Latitude);
        }

        [TestMethod]
        [DataRow(51.234, true,  51.234)]
        [DataRow(51.234, false, null)]
        public void Build_Sets_Aircraft_Longitude(double value, bool hasChanged, double? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Location = new(0, value));
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Longitude);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Manufacturer(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Manufacturer = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Manufacturer);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Model(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Model = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Model);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_NumberOfEngines(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { NumberOfEngines = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].NumberOfEngines);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_OnGround(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.OnGround = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].OnGround);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Operator(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Operator = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Operator);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_OperatorIcao(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { OperatorIcao = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].OperatorIcao);
        }

        [TestMethod]
        [DataRow(AirportCodeType.Iata, true,  "LHR")]
        [DataRow(AirportCodeType.Icao, true,  "EGLL")]
        [DataRow(AirportCodeType.Iata, false, null)]
        [DataRow(AirportCodeType.Icao, false, null)]
        public void Build_Sets_Aircraft_Origin(AirportCodeType showType, bool hasChanged, string expected)
        {
            _WebClientSettings.Value = new(PreferredAirportCodeType: showType);
            SetupAircraft(stamp: 2, lookup: new() { Route = LHR_SIN_SYD() });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Origin);
        }

        [TestMethod]
        [DataRow(100, true,  100)]
        [DataRow(100, false, null)]
        public void Build_Sets_Aircraft_PictureHeight(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { AircraftPicture = value == null ? null : new(
                FileName: "/foo",
                WidthPixels: 1,
                HeightPixels: value.Value
            )});
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].PictureHeight);
        }

        [TestMethod]
        [DataRow(100, true,  100)]
        [DataRow(100, false, null)]
        public void Build_Sets_Aircraft_PictureWidth(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { AircraftPicture = value == null ? null : new(
                FileName: "/foo",
                WidthPixels: value.Value,
                HeightPixels: 1
            )});
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].PictureWidth);
        }

        [TestMethod]
        public void Build_Sets_Aircraft_PositionIsMlat_To_Null()
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(1));
            _Args.PreviousDataVersion = 1;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.IsNull(json.Aircraft[0].PositionIsMlat);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Build_Sets_Aircraft_Position_Time_To_Milliseconds_After_Unix_Epoc(bool hasChanged)
        {
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, TimeSpan.Zero);
            SetupAircraft(stamp: 2, fillMessage: m => m.Location = new(1, 1));
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            if(hasChanged) {
                Assert.AreEqual(_Clock.UtcNowUnixMilliseconds, json.Aircraft[0].PositionTime ?? 0L);
            } else {
                Assert.IsNull(json.Aircraft[0].PositionTime);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Build_Sets_Aircraft_ReceiverId_To_Receiver_Used(bool previousDataVersionSent)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Icao24 = new(1));
            _Args.PreviousDataVersion = previousDataVersionSent ? 1 : -1;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            if(!previousDataVersionSent) {
                Assert.AreEqual(_Receiver.Object.Id, json.Aircraft[0].ReceiverId);
            } else {
                Assert.IsNull(json.Aircraft[0].ReceiverId);
            }
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Registration(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Registration = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Registration);
        }

        [TestMethod]
        [DataRow(100, true,  100)]
        [DataRow(100, false, null)]
        public void Build_Sets_Aircraft_SignalLevel(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => {
                m.SignalLevel = value;
                m.SignalLevelSent = true;
            });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].SignalLevel);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(false)]
        public void Build_Sets_Aircraft_SignalLevel_Null_If_SignalLevelSent_Is_Not_True(bool? signalLevelSent)
        {
            SetupAircraft(stamp: 2, fillMessage: m => {
                m.SignalLevel = 1;
                m.SignalLevelSent = signalLevelSent;
            });
            _Args.PreviousDataVersion = 1;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.IsNull(json.Aircraft[0].SignalLevel);
        }

        [TestMethod]
        [DataRow(Species.None,       true,  0)]
        [DataRow(Species.Helicopter, true,  4)]
        [DataRow(Species.Helicopter, false, null)]
        public void Build_Sets_Aircraft_Species(Species value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { Species = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Species);
        }

        [TestMethod]
        [DataRow(SpeedType.TrueAirSpeed, true,  3)]
        [DataRow(SpeedType.TrueAirSpeed, false, null)]
        public void Build_Sets_Aircraft_SpeedType(SpeedType value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.GroundSpeedType = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].SpeedType);
        }

        [TestMethod]
        [DataRow(1234, true, "1234")]
        [DataRow(1,    true, "0001")]
        [DataRow(1234, false, null)]
        public void Build_Sets_Aircraft_Squawk(int? value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.Squawk = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Squawk);
        }

        [TestMethod]
        [DataRow(100, true,  100)]
        [DataRow(100, false, null)]
        public void Build_Sets_Aircraft_TargetAltitude(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.TargetAltitudeFeet = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].TargetAltitude);
        }

        [TestMethod]
        [DataRow(12.34F, true,  12.34F)]
        [DataRow(12.34F, false, null)]
        public void Build_Sets_Aircraft_TargetTrack(float? value, bool hasChanged, float? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.TargetHeadingDegrees = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].TargetTrack);
        }

        [TestMethod]
        [DataRow(12.34F, true,  12.34F)]
        [DataRow(12.34F, false, null)]
        public void Build_Sets_Aircraft_Track(float? value, bool hasChanged, float? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.GroundTrackDegrees = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Track);
        }

        [TestMethod]
        [DataRow(null,  true,  null)]
        [DataRow(false, true,  false)]
        [DataRow(true,  true,  true)]
        [DataRow(null,  false, null)]
        [DataRow(false, false, null)]
        [DataRow(true,  false, null)]
        public void Build_Sets_Aircraft_TrackIsHeading(bool? value, bool hasChanged, bool? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.GroundTrackIsHeading = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].TrackIsHeading);
        }

        [TestMethod]
        [DataRow(TransponderType.Adsb2, true,  5)]
        [DataRow(TransponderType.Adsb2, false, null)]
        public void Build_Sets_Aircraft_TransponderType(TransponderType value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.TransponderType = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].TransponderType);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_Type(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { ModelIcao = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].Type);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Build_Sets_Aircraft_UniqueId_Unconditionally(bool hasChanged)
        {
            SetupAircraft(stamp: 2, id: 7, fillMessage: m => m.Icao24 = new(1));
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(7, json.Aircraft[0].UniqueId);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_UserNotes(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { UserNotes = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].UserNotes);
        }

        [TestMethod]
        [DataRow("A", true,  "A")]
        [DataRow("A", false, null)]
        public void Build_Sets_Aircraft_UserTag(string value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { UserTag = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].UserTag);
        }

        [TestMethod]
        [DataRow(1425, true,  1425)]
        [DataRow(1425, false, null)]
        public void Build_Sets_Aircraft_VerticalRate(int? value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.VerticalRateFeetPerMinute = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].VerticalRate);
        }

        [TestMethod]
        [DataRow(AltitudeType.Radar, true,  1)]
        [DataRow(AltitudeType.Radar, false, null)]
        public void Build_Sets_Aircraft_VerticalRateType(AltitudeType value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, fillMessage: m => m.VerticalRateType = value);
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].VerticalRateType);
        }

        [TestMethod]
        [DataRow(WakeTurbulenceCategory.Heavy, true,  3)]
        [DataRow(WakeTurbulenceCategory.Heavy, false, null)]
        public void Build_Sets_Aircraft_WakeTurbulenceCategory(WakeTurbulenceCategory value, bool hasChanged, int? expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { WakeTurbulenceCategory = value });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].WakeTurbulenceCategory);
        }

        [TestMethod]
        [DataRow(2021, true,  "2021")]
        [DataRow(2021, false, null)]
        public void Build_Sets_Aircraft_YearBuilt(int? value, bool hasChanged, string expected)
        {
            SetupAircraft(stamp: 2, lookup: new() { YearBuilt = 2021 });
            _Args.PreviousDataVersion = hasChanged ? 1 : 2;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);

            Assert.AreEqual(expected, json.Aircraft[0].YearBuilt);
        }

        [TestMethod]
        public void Build_All_Trails_Sends_Nothing_When_No_Trails_Requested()
        {
            _Args.TrailType = TrailType.None;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var aircraftJson = json.Aircraft[0];

            Assert.IsNull(aircraftJson.TrailType);
            Assert.IsNull(aircraftJson.FullCoordinates);
            Assert.IsNull(aircraftJson.ShortCoordinates);
        }

        [TestMethod]
        public void Build_Full_Trail_Sends_Simple_Set()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var aircraftJson = json.Aircraft[0];
            var actual = aircraftJson.FullCoordinates;

            Assert.AreEqual("", aircraftJson.TrailType);
            Assert.IsNull(aircraftJson.ShortCoordinates);
            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual(10, actual[0]);
            Assert.AreEqual(11, actual[1]);
            Assert.AreEqual(90, actual[2]);
        }

        [TestMethod]
        public void Build_Full_Trail_Sends_Start_And_End()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(6, actual.Count);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);

            Assert.AreEqual(12, actual[idx++]);
            Assert.AreEqual(13, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Ignores_Changes_Of_Location_Without_Change_Of_Heading()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));
            AddTrailData(aircraft, stamp: 4, new(14, 15), heading: 91);

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(6, actual.Count);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);

            Assert.AreEqual(14, actual[idx++]);
            Assert.AreEqual(15, actual[idx++]);
            Assert.AreEqual(91, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Always_Sends_At_Least_First_And_Last_Point()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));
            AddTrailData(aircraft, stamp: 4, new(14, 15));
            AddTrailData(aircraft, stamp: 5, new(16, 17));

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(6, actual.Count);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);

            Assert.AreEqual(16, actual[idx++]);
            Assert.AreEqual(17, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Does_Not_Resend_Coordinates()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));
            AddTrailData(aircraft, stamp: 4, new(14, 15));
            AddTrailData(aircraft, stamp: 5, new(16, 17));
            _Args.PreviousDataVersion = 4;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual(false, json.Aircraft[0].ResetTrail);
            var idx = 0;

            Assert.AreEqual(16, actual[idx++]);
            Assert.AreEqual(17, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Does_Not_Send_New_Coordinate_If_It_Is_Unchanged()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));
            AddTrailData(aircraft, stamp: 4, new(12, 13));
            _Args.PreviousDataVersion = 3;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void Build_Full_Trails_Will_Resend_Coordinates_If_Requested()
        {
            _Args.TrailType = TrailType.Full;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13));
            AddTrailData(aircraft, stamp: 4, new(14, 15));
            _Args.PreviousDataVersion = 3;
            _Args.ResendTrails = true;

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(6, actual.Count);
            Assert.AreEqual(true, json.Aircraft[0].ResetTrail);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);

            Assert.AreEqual(14, actual[idx++]);
            Assert.AreEqual(15, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Can_Send_Altitudes()
        {
            _Args.TrailType = TrailType.FullAltitude;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
                m.AltitudeFeet = 1000;
            });

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var aircraftJson = json.Aircraft[0];
            var actual = aircraftJson.FullCoordinates;

            Assert.AreEqual("a", aircraftJson.TrailType);
            Assert.IsNull(aircraftJson.ShortCoordinates);
            Assert.AreEqual(4, actual.Count);
            Assert.AreEqual(10, actual[0]);
            Assert.AreEqual(11, actual[1]);
            Assert.AreEqual(90, actual[2]);
            Assert.AreEqual(1000, actual[3]);
        }

        [TestMethod]
        public void Build_Full_Trail_Can_Send_Speeds()
        {
            _Args.TrailType = TrailType.FullSpeed;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
                m.GroundSpeedKnots = 200;
            });

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var aircraftJson = json.Aircraft[0];
            var actual = aircraftJson.FullCoordinates;

            Assert.AreEqual("s", aircraftJson.TrailType);
            Assert.IsNull(aircraftJson.ShortCoordinates);
            Assert.AreEqual(4, actual.Count);
            Assert.AreEqual(10, actual[0]);
            Assert.AreEqual(11, actual[1]);
            Assert.AreEqual(90, actual[2]);
            Assert.AreEqual(200, actual[3]);
        }

        [TestMethod]
        public void Build_Full_Trail_Shows_Changes_In_Altitude_Even_If_Heading_Is_Unchanged()
        {
            _Args.TrailType = TrailType.FullAltitude;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
                m.AltitudeFeet = 1000;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13), altitude: 1025);
            AddTrailData(aircraft, stamp: 4, new(14, 15));

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(12, actual.Count);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(1000, actual[idx++]);

            Assert.AreEqual(12, actual[idx++]);
            Assert.AreEqual(13, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(1025, actual[idx++]);

            Assert.AreEqual(14, actual[idx++]);
            Assert.AreEqual(15, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(1025, actual[idx++]);
        }

        [TestMethod]
        public void Build_Full_Trail_Shows_Changes_In_Speed_Even_If_Heading_Is_Unchanged()
        {
            _Args.TrailType = TrailType.FullSpeed;
            var aircraft = SetupAircraft(stamp: 2, fillMessage: m => {
                m.Location = new(10, 11);
                m.GroundTrackDegrees = 90;
                m.GroundSpeedKnots = 200;
            });
            AddTrailData(aircraft, stamp: 3, new(12, 13), speed: 210);
            AddTrailData(aircraft, stamp: 4, new(14, 15));

            var json = _Builder.Build(_Args, ignoreInvisibleSources: true, fallbackToDefaultSource: true);
            var actual = json.Aircraft[0].FullCoordinates;

            Assert.AreEqual(12, actual.Count);
            var idx = 0;

            Assert.AreEqual(10, actual[idx++]);
            Assert.AreEqual(11, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(200, actual[idx++]);

            Assert.AreEqual(12, actual[idx++]);
            Assert.AreEqual(13, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(210, actual[idx++]);

            Assert.AreEqual(14, actual[idx++]);
            Assert.AreEqual(15, actual[idx++]);
            Assert.AreEqual(90, actual[idx++]);
            Assert.AreEqual(210, actual[idx++]);
        }
    }
}
