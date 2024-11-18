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
using VirtualRadar.Message;
using VirtualRadar.Receivers;
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
        private MockFileSystem _FileSystem;

        [TestInitialize]
        public void TestInitialise()
        {
            _Args = new();
            _Filter = new();

            _AllAircraft = [];
            _AircraftListToArrayStamp = 0L;
            _AircraftList = MockHelper.CreateMock<IAircraftList>();
            _AircraftList
                .Setup(r => r.ToArray(out It.Ref<long>.IsAny))
                .Returns((out long stamp) => {
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

            _FileSystem = new();

            _Builder = new(
                _AircraftMapSettings,
                _AircraftPictureSettings,
                _InternetClientSettings,
                _OperatorAndTypeFlagSettings,
                _ReceiverFactory.Object,
                _FileSystem
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
            id ??= _AllAircraft.DefaultIfEmpty().Max(aircraft => aircraft?.Id ?? 0) + 1;
            aircraft ??= new(id.Value);
            Assert.AreEqual(id.Value, aircraft.Id);

            if(!_AllAircraft.Contains(aircraft)) {
                _AllAircraft.Add(aircraft);
            }

            if(fillMessage != null) {
                var message = new TransponderMessage(id.Value);
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
                aircraft.CopyFromLookup(lookup);
            }

            return aircraft;
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
    }
}
