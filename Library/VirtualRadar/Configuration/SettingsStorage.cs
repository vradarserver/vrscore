using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Do not instantiate this directly. Instantitate <see cref="ISettingsStorage"/> via DI
    /// instead. This is only public so that it can be unit tested.
    /// </summary>
    public class SettingsStorage : ISettingsStorage
    {
        class ParsedContent
        {
            public long ParsedFromContentVersion;
            public object ParsedValue;
        }

        internal const string FileName = "Settings.json";

        private const string _Schema_Original = "1";
        private const string _Schema_Current = _Schema_Original;

        private readonly IFileSystem _FileSystem;
        private readonly IWorkingFolder _WorkingFolder;
        private readonly ISettingsConfiguration _SettingsConfiguration;

        private readonly object _SyncLock = new();
        private Dictionary<string, JObject> _Content;
        private long _ContentVersion;
        private readonly Dictionary<string, ParsedContent> _ParsedContent = [];
        private string _ContentFileName;
        private JsonSerializerSettings _JsonDeserialiserSettings;

        private static string ParsedContentKey(string key, Type parsedType) => $"{key}-{parsedType.Name}";

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="workingFolder"></param>
        /// <param name="settingsConfiguration"></param>
        public SettingsStorage(
            IFileSystem fileSystem,
            IWorkingFolder workingFolder,
            ISettingsConfiguration settingsConfiguration
        )
        {
            _FileSystem = fileSystem;
            _WorkingFolder = workingFolder;
            _SettingsConfiguration = settingsConfiguration;

            _JsonDeserialiserSettings = new();
            _JsonDeserialiserSettings.Converters.Add(new SettingsProviderJsonConverter());
        }

        /// <inheritdoc/>
        public object LatestValue(Type optionType)
        {
            LoadContent();

            var contentKey = _SettingsConfiguration.GetKeyForOptionType(optionType);
            var parsedContentKey = ParsedContentKey(contentKey, optionType);

            ParsedContent parsedContent;
            lock(_SyncLock) {
                _ParsedContent.TryGetValue(parsedContentKey, out parsedContent);

                if(parsedContent?.ParsedFromContentVersion != _ContentVersion) {
                    _Content.TryGetValue(contentKey, out var content);
                    if(content == null) {
                        throw new InvalidOperationException(
                            $"There is no default and no content stored for the \"{contentKey}\" key assigned to options of type {optionType.Name}"
                        );
                    }
                    var deserialised = JsonConvert.DeserializeObject(
                        content.ToString(),
                        optionType,
                        _JsonDeserialiserSettings
                    );

                    if(Object.Equals(parsedContent?.ParsedValue, deserialised)) {
                        parsedContent.ParsedFromContentVersion = _ContentVersion;
                    } else {
                        parsedContent = new() {
                            ParsedFromContentVersion = _ContentVersion,
                            ParsedValue = deserialised,
                        };
                        _ParsedContent[parsedContentKey] = parsedContent;
                    }
                }
            }

            return parsedContent.ParsedValue;
        }

        /// <inheritdoc/>
        public TObject LatestValue<TObject>() => (TObject)LatestValue(typeof(TObject));

        private void LoadContent()
        {
            lock(_SyncLock) {
                var contentFileName = _FileSystem.Combine(
                    _WorkingFolder.Folder,
                    FileName
                );

                if(_Content == null || _ContentFileName != contentFileName) {
                    ++_ContentVersion;
                    _ContentFileName = contentFileName;

                    _Content = [];

                    var defaultKeys = _SettingsConfiguration.GetDefaultKeys();
                    foreach(var kvp in defaultKeys) {
                        _Content[kvp.Key] = kvp.Value;
                    }

                    var saveSettings = !_FileSystem.FileExists(_ContentFileName);
                    if(!saveSettings) {
                        var json = _FileSystem.ReadAllText(_ContentFileName);
                        var loaded = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);

                        foreach(var kvp in _Content) {
                            if(!loaded.TryGetValue(kvp.Key, out var loadedContent)) {
                                saveSettings = true;
                                break;
                            }
                            if(!kvp.Value.Equals(loadedContent)) {
                                saveSettings = true;
                                break;
                            }
                        }

                        foreach(var kvp in loaded) {
                            var key = kvp.Key;
                            var actualContent = kvp.Value;

                            if(_Content.TryGetValue(key, out var defaultContent)) {
                                if(defaultContent is JObject mergedContent) {
                                    mergedContent.Merge(actualContent, new() {
                                        MergeArrayHandling = MergeArrayHandling.Replace,
                                        MergeNullValueHandling = MergeNullValueHandling.Merge
                                    });
                                    actualContent = mergedContent;
                                }
                            }

                            _Content[key] = actualContent;
                        }
                    }

                    if(saveSettings) {
                        SaveContent();
                    }
                }
            }
        }

        private void SaveContent()
        {
            lock(_SyncLock) {
                _FileSystem.CreateDirectoryIfNotExists(_WorkingFolder.Folder);
                var contentFileName = _FileSystem.Combine(
                    _WorkingFolder.Folder,
                    FileName
                );

                var json = JsonConvert.SerializeObject(_Content, Formatting.Indented);
                _FileSystem.WriteAllText(contentFileName, json);
            }
        }
    }
}
