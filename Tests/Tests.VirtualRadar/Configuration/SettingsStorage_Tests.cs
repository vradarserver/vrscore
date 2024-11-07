// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tests.Mocks;
using VirtualRadar.Collections;
using VirtualRadar.Configuration;

namespace Tests.VirtualRadar.Configuration
{
    [TestClass]
    public class SettingsStorage_Tests
    {
        record Options(int Id, string Name);
        readonly Options _DefaultOptions = new(0, null);

        class ArrayOfStrings
        {
            public IReadOnlyList<string> Lines { get; }

            public ArrayOfStrings(string[] lines)
            {
                Lines = lines;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result && obj is ArrayOfStrings other) {
                    result = (Lines == null && other.Lines == null)
                          || (Lines != null && other.Lines != null && Lines.SequenceEqual(other.Lines));
                }
                return result;
            }

            public override int GetHashCode() => HashCode.Combine(Lines);
        }

        readonly ArrayOfStrings _DefaultArrayOfStrings = new([]);

        private SettingsStorage _Service;
        private MockFileSystem _FileSystem;
        private MockWorkingFolder _WorkingFolder;
        private Mock<ISettingsConfiguration> _SettingsConfig;

        private Dictionary<Type, string> _OptionTypeToKey;
        private Dictionary<string, JObject> _OptionKeyToDefaultValue;

        [TestInitialize]
        public void TestInitialise()
        {
            _FileSystem = new();
            _WorkingFolder = new();

            _OptionTypeToKey = [];
            _OptionKeyToDefaultValue = [];
            _SettingsConfig = MockHelper.CreateMock<ISettingsConfiguration>();
            _SettingsConfig
                .Setup(r => r.GetKeyForOptionType(It.IsAny<Type>()))
                .Returns((Type type) => {
                    if(!_OptionTypeToKey.TryGetValue(type, out var result)) {
                        throw new InvalidOperationException($"Type not registered");
                    }
                    return result;
                });
            _SettingsConfig
                .Setup(r => r.GetDefaultKeys())
                .Returns(() => ShallowCollectionCopier.Copy(_OptionKeyToDefaultValue));

            _Service = CreateService();
        }

        private SettingsStorage CreateService() => new(
            _FileSystem,
            _WorkingFolder,
            _SettingsConfig.Object
        );

        private void SetupConfigForType<T>(string key, T defaultValue)
        {
            _OptionTypeToKey[typeof(T)] = key;
            _OptionKeyToDefaultValue[key] = JObject.FromObject(defaultValue);
        }

        private string ExpectedSettingsFileName()
        {
            return Path.Combine(
                _WorkingFolder.Folder,
                "Settings.json"
            );
        }

        private void SetupConfigFile(string content, string folder = null)
        {
            if(folder != null) {
                _WorkingFolder.Folder = folder;
            }
            _FileSystem.AddFileContent(
                ExpectedSettingsFileName(),
                content,
                Encoding.UTF8
            );
        }

        private void CreateFakeConfigFile<T>(string key, T defaultValue, T content)
        {
            var json = $"{{ \"{key}\": {JsonConvert.SerializeObject(content, Formatting.Indented)} }}";

            SetupConfigForType<T>(key, defaultValue);
            SetupConfigFile(json);
        }

        private void AssertContent(string expected)
        {
            var actual = _FileSystem.GetFileContentAsString(ExpectedSettingsFileName());

            static string normalise(string json)
            {
                if(!String.IsNullOrWhiteSpace(json)) {
                    var parsedJson = JsonConvert.DeserializeObject(json);
                    json = JsonConvert.SerializeObject(parsedJson);
                }
                return json;
            }

            var normalisedExpected = normalise(expected);
            var normalisedActual = normalise(actual);

            Assert.AreEqual(normalisedExpected, normalisedActual);
        }

        [TestMethod]
        public void LatestValue_Returns_Default_When_File_Missing()
        {
            SetupConfigForType("options", _DefaultOptions);

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(_DefaultOptions, actual);
        }

        [TestMethod]
        public void LatestValue_Can_Deserialise_From_Configuration_File()
        {
            var expected = new Options(1, "Zaltor");
            CreateFakeConfigFile("options", _DefaultOptions, expected);

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Defaults_Values_Not_In_Json()
        {
            var expected = new Options(1, "Zaltor");

            SetupConfigForType<Options>("options", new Options(0, "Zaltor"));
            SetupConfigFile(@"{ ""options"": { ""Id"": 1 } }");

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Ignores_Extraneous_Json_Values()
        {
            var expected = new Options(1, "Zaltor");

            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": { ""Id"": 1, ""Name"": ""Zaltor"", ""Title"": ""The Merciless"" } }");

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Allows_Block_Comments_In_Json()
        {
            var expected = new Options(1, "Zaltor");
            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": /* Block Comments */ { ""Id"": 1, ""Name"": ""Zaltor"" } }");

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Allows_Line_Comments_In_Json()
        {
            var expected = new Options(1, "Zaltor");
            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": {
                // This line should be ignored
                ""Id"": 1, ""Name"": ""Zaltor""
            } }");

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Ignores_Trailing_Comma_In_Object_Json()
        {
            var expected = new Options(1, "Zaltor");

            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": { ""Id"": 1, ""Name"": ""Zaltor"", } }");

            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LatestValue_Does_Not_Trigger_Save()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);

            _Service.LatestValue<Options>();

            Assert.IsFalse(_FileSystem.FileExists(ExpectedSettingsFileName()));
        }

        [TestMethod]
        public void ChangeValue_Overwrites_Existing_Options()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            var newOptions = new Options(2, "New Name");

            _Service.ChangeValue(typeof(Options), newOptions);

            var actual = _Service.LatestValue<Options>();
            Assert.AreEqual(newOptions, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChangeValue_Throws_If_Passed_Null_Type()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            _Service.ChangeValue(null, new Options(1, ""));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChangeValue_Throws_If_Passed_Null_Value()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            _Service.ChangeValue(typeof(Options), null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChangeValue_Throws_If_Value_Type_Different_To_Options()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            _Service.ChangeValue(typeof(Options), "");
        }

        [TestMethod]
        public void SaveChanges_Writes_New_Files()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);

            _Service.SaveChanges();

            AssertContent(@"{ ""options"": { ""Id"": 0, ""Name"": null } }");
        }

        [TestMethod]
        public void SaveChanges_Saves_New_Values()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);

            _Service.ChangeValue<Options>(new(2, "Gale"));
            _Service.SaveChanges();

            AssertContent(@"{ ""options"": { ""Id"": 2, ""Name"": ""Gale"" } }");
        }

        [TestMethod]
        public void SaveChanges_Overwrites_Existing_Values()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            _Service.SaveChanges();

            _Service.ChangeValue<Options>(new(2, "Gale"));
            _Service.SaveChanges();

            AssertContent(@"{ ""options"": { ""Id"": 2, ""Name"": ""Gale"" } }");
        }

        [TestMethod]
        public void SaveChanges_Merges_Unused_Existing_Values()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": { ""Id"": 1, ""Name"": ""Zaltor"", ""Title"": ""The Merciless"" } }");

            _Service.ChangeValue<Options>(new(Id: 2, Name: "Foo"));
            _Service.SaveChanges();

            var content = _FileSystem.GetFileContentAsString(ExpectedSettingsFileName());
            Assert.IsTrue(content.Contains(@"""Title"":"));
            Assert.IsTrue(content.Contains(@"""The Merciless"""));
        }

        [TestMethod]
        public void SaveChanges_Overwrites_Existing_Value_Types()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": { ""Id"": 1, ""Name"": ""Zaltor"" } }");

            var expected = new Options(Id: 2, Name: "Foo");
            _Service.ChangeValue<Options>(expected);
            _Service.SaveChanges();

            var newService = CreateService();
            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SaveChanges_Can_Overwrite_NonNull_With_Null()
        {
            SetupConfigForType<Options>("options", _DefaultOptions);
            SetupConfigFile(@"{ ""options"": { ""Id"": 1, ""Name"": ""Zaltor"" } }");

            var expected = new Options(Id: 2, Name: null);
            _Service.ChangeValue<Options>(expected);
            _Service.SaveChanges();

            var newService = CreateService();
            var actual = _Service.LatestValue<Options>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SaveChanges_Can_Overwrite_Array_Content()
        {
            SetupConfigForType<ArrayOfStrings>("strings", _DefaultArrayOfStrings);
            SetupConfigFile(@"{ ""strings"": { ""Lines"": [ ""L1"", ""L2"" ] } }");

            var expected = new ArrayOfStrings([ "J1" ]);
            _Service.ChangeValue<ArrayOfStrings>(expected);
            _Service.SaveChanges();

            var newService = CreateService();
            var actual = _Service.LatestValue<ArrayOfStrings>();

            Assert.AreEqual(expected, actual);
        }
    }
}
