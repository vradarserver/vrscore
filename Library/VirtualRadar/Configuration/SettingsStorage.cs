// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Do not instantiate this directly. Instantitate <see cref="ISettingsStorage"/> via DI
    /// instead. This is only public so that it can be unit tested.
    /// </summary>
    public class SettingsStorage : ISettingsStorage, IDisposable
    {
        internal const string FileName = "Settings.json";

        private const string _Schema_Original = "1";
        private const string _Schema_Current = _Schema_Original;

        private readonly IFileSystem _FileSystem;
        private readonly IWorkingFolder _WorkingFolder;
        private readonly ISettingsConfiguration _SettingsConfiguration;
        private readonly ILog _Log;

        private readonly object _SyncLock = new();
        private Dictionary<string, JObject> _SettingKeyToJObject;
        private readonly Dictionary<string, object> _ParsedContent = [];
        private string _ContentFileName;
        private JsonSerializerSettings _JsonSerialiserSettings;
        private JsonSerializer _JsonSerialiser;
        private JsonSerializerSettings _JsonDeserialiserSettings;
        private readonly CallbackWithParamList<ValueChangedCallbackArgs> _ValueChangedCallbacks = new();
        private readonly CallbackNoParamList _SavedChangesCallbacks = new();

        private static string ParsedContentKey(string key, Type parsedType) => $"{key}-{parsedType.Name}";

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="workingFolder"></param>
        /// <param name="settingsConfiguration"></param>
        /// <param name="log"></param>
        public SettingsStorage(
            IFileSystem fileSystem,
            IWorkingFolder workingFolder,
            ISettingsConfiguration settingsConfiguration,
            ILog log
        )
        {
            _FileSystem = fileSystem;
            _WorkingFolder = workingFolder;
            _SettingsConfiguration = settingsConfiguration;
            _Log = log;

            _JsonDeserialiserSettings = new();
            _JsonDeserialiserSettings.Converters.Add(new SettingsProviderJsonConverter());

            _JsonSerialiserSettings = new();
            _JsonSerialiserSettings.Converters.Add(new StringEnumConverter());

            _JsonSerialiser = JsonSerializer.Create(_JsonSerialiserSettings);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~SettingsStorage() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _ValueChangedCallbacks.Dispose();
                _SavedChangesCallbacks.Dispose();
            }
        }

        /// <inheritdoc/>
        public string SettingsLocation()
        {
            var folder = _WorkingFolder.Folder;
            var fileName = FileName;
            return _FileSystem.Combine(folder, fileName);
        }

        /// <inheritdoc/>
        public ICallbackHandle AddValueChangedCallback(Action<ValueChangedCallbackArgs> callback)
        {
            return _ValueChangedCallbacks.Add(callback);
        }

        /// <inheritdoc/>
        public ICallbackHandle AddSavedChangesCallback(Action callback)
        {
            return _SavedChangesCallbacks.Add(callback);
        }

        /// <inheritdoc/>
        public TObject LatestValue<TObject>() => (TObject)LatestValue(typeof(TObject));

        /// <inheritdoc/>
        public object LatestValue(Type optionType)
        {
            var contentKey = _SettingsConfiguration.GetKeyForOptionType(optionType);
            return LatestValue(contentKey, optionType);
        }

        private object LatestValue(string contentKey, Type optionType)
        {
            var parsedContentKey = ParsedContentKey(contentKey, optionType);

            LoadContent();

            object result;
            lock(_SyncLock) {
                if(!_ParsedContent.TryGetValue(parsedContentKey, out result)) {
                    _SettingKeyToJObject.TryGetValue(contentKey, out var fileJObject);
                    if(fileJObject == null) {
                        throw new InvalidOperationException(
                            $"There is no default and no content stored for the \"{contentKey}\" key assigned to options of type {optionType.Name}"
                        );
                    }

                    var deserialised = JsonConvert.DeserializeObject(
                        fileJObject.ToString(),
                        optionType,
                        _JsonDeserialiserSettings
                    );

                    result = deserialised;
                    _ParsedContent[parsedContentKey] = result;
                }
            }

            return result;
        }

        private void LoadContent()
        {
            var contentFileName = SettingsLocation();

            bool contentNeedsLoading() => _SettingKeyToJObject == null
                                       || _ContentFileName != contentFileName;

            if(contentNeedsLoading()) {
                lock(_SyncLock) {
                    if(contentNeedsLoading()) {
                        _ContentFileName = _FileSystem.Combine(_WorkingFolder.Folder, FileName);
                        _SettingKeyToJObject = [];

                        var defaultKeys = _SettingsConfiguration.GetDefaultKeys();
                        foreach(var kvp in defaultKeys) {
                            _SettingKeyToJObject[kvp.Key] = kvp.Value;
                        }

                        if(_FileSystem.FileExists(_ContentFileName)) {
                            var json = _FileSystem.ReadAllText(_ContentFileName);
                            var loaded = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);

                            foreach(var kvp in _SettingKeyToJObject) {
                                if(!loaded.TryGetValue(kvp.Key, out var loadedContent)) {
                                    break;
                                }
                                if(!kvp.Value.Equals(loadedContent)) {
                                    break;
                                }
                            }

                            foreach(var kvp in loaded) {
                                var key = kvp.Key;
                                var actualContent = kvp.Value;

                                if(_SettingKeyToJObject.TryGetValue(key, out var currentJObject)) {
                                    MergeJObjects(currentJObject, actualContent);
                                    actualContent = currentJObject;
                                }

                                _SettingKeyToJObject[key] = actualContent;
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void ChangeValue(Type optionType, object newValue)
        {
            ArgumentNullException.ThrowIfNull(optionType);
            ArgumentNullException.ThrowIfNull(newValue);

            var contentKey = _SettingsConfiguration.GetKeyForOptionType(optionType);
            if(!optionType.IsAssignableFrom(newValue.GetType())) {
                throw new InvalidOperationException(
                      $"Cannot store a value of type {newValue.GetType().Name} against the entry for type "
                    + $"{optionType.Name} under the key \"{contentKey}\""
                );
            }

            var parsedContentKey = ParsedContentKey(contentKey, optionType);

            var runCallbacks = false;
            lock(_SyncLock) {
                var latestValue = LatestValue(contentKey, optionType);
                if(!latestValue.Equals(newValue)) {
                    var newJObject = JObject.FromObject(newValue, _JsonSerialiser);

                    if(_SettingKeyToJObject.TryGetValue(contentKey, out var currentJObject)) {
                        MergeJObjects(currentJObject, newJObject);
                        newJObject = currentJObject;
                    }

                    _SettingKeyToJObject[contentKey] = newJObject;
                    _ParsedContent[parsedContentKey] = newValue;

                    runCallbacks = true;
                }
            }

            if(runCallbacks) {
                var exception = _ValueChangedCallbacks.InvokeWithoutExceptions(new(contentKey, newValue));
                if(exception != null) {
                    _Log.Exception(exception, $"Thrown when running callbacks after setting the \"{contentKey}\" value to {newValue}");
                }
            }
        }

        /// <inheritdoc/>
        public void ChangeValue<T>(T newValue) => ChangeValue(typeof(T), newValue);

        /// <inheritdoc/>
        public void SaveChanges()
        {
            string contentFileName = null;

            lock(_SyncLock) {
                if(_SettingKeyToJObject == null) {
                    LoadContent();
                }

                _FileSystem.CreateDirectoryIfNotExists(_WorkingFolder.Folder);
                contentFileName = _FileSystem.Combine(
                    _WorkingFolder.Folder,
                    FileName
                );
            
                var json = JsonConvert.SerializeObject(
                    _SettingKeyToJObject,
                    Formatting.Indented,
                    _JsonSerialiserSettings
                );
                _FileSystem.WriteAllText(contentFileName, json);
            }

            var exception = _SavedChangesCallbacks.InvokeWithoutExceptions();
            if(exception != null) {
                _Log.Exception(exception, $"Thrown when running callbacks after saving settings to {contentFileName}");
            }
        }

        private static void MergeJObjects(JObject target, JObject source)
        {
            target.Merge(source, new() {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });
        }
    }
}
