// Copyright © 2018 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.RegularExpressions;
using Newtonsoft.Json;
using VirtualRadar.Configuration;

namespace VirtualRadar.TileServer
{
    /// <summary>
    /// Default implementation of <see cref="IDownloadedTileServerSettingsStorage"/>.
    /// </summary>
    class DownloadedTileServerSettingsStorage(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<TileServerSettings> _Settings,
        IFileSystem _FileSystem,
        IWorkingFolder _WorkingFolder,
        ILog _Log
        #pragma warning restore IDE1006
    ) : IDownloadedTileServerSettingsStorage
    {
        internal const string _DownloadedTileServerSettingsFileName = "TileServerSettings-Downloaded.json";
        internal const string _CustomTileServerSettingsFileName =     "TileServerSettings-Custom.json";
        internal const string _ReadMeFileName =                       "TileServerSettings-ReadMe.txt";

        private string Folder
        {
            get {
                var overrideFolder = _Settings.LatestValue.FolderOverride;
                return String.IsNullOrEmpty(overrideFolder)
                    ? _WorkingFolder.Folder
                    : overrideFolder;
            }
        }

        /// <inheritdoc/>
        public bool DownloadedSettingsFileExists()
        {
            var fullPath = _FileSystem.Combine(Folder, _DownloadedTileServerSettingsFileName);
            return _FileSystem.FileExists(fullPath);
        }

        /// <inheritdoc/>
        public IReadOnlyList<DownloadedTileServerSettings> Load()
        {
            var result = new List<DownloadedTileServerSettings>();

            LoadIfFileExists(result, _DownloadedTileServerSettingsFileName, isCustom: false);
            LoadIfFileExists(result, _CustomTileServerSettingsFileName, isCustom: true);

            return result;
        }

        /// <inheritdoc/>
        public void SaveDownloadedSettings(IEnumerable<DownloadedTileServerSettings> settings)
        {
            var folder = Folder;

            _FileSystem.CreateDirectoryIfNotExists(folder);
            var fullPath = _FileSystem.Combine(folder, _DownloadedTileServerSettingsFileName);

            var jsonText = JsonConvert.SerializeObject(settings, Formatting.Indented);
            _FileSystem.WriteAllText(fullPath, jsonText);
        }

        private void LoadIfFileExists(
            List<DownloadedTileServerSettings> results,
            string fileName,
            bool isCustom = false
        )
        {
            var fullPath = _FileSystem.Combine(Folder, fileName);
            if(_FileSystem.FileExists(fullPath)) {
                try {
                    var jsonText = _FileSystem.ReadAllText(fullPath);
                    var settingsList = JsonConvert.DeserializeObject<DownloadedTileServerSettings[]>(jsonText);

                    foreach(var setting in settingsList) {
                        setting.Name = (setting.Name ?? "").Trim();
                        if(setting.Name == "") {
                            continue;
                        }
                        if(isCustom) {
                            setting.Name = $"* {setting.Name}";
                        }

                        if(!results.Any(r => String.Equals(r.Name, setting.Name, StringComparison.OrdinalIgnoreCase) && r.MapProvider == setting.MapProvider)) {
                            setting.IsCustom = isCustom;
                            setting.IsDefault = !isCustom && setting.IsDefault;

                            setting.Url = (setting.Url ?? "").Trim();
                            if(setting.Url == "") {
                                continue;
                            }

                            setting.Attribution = (setting.Attribution ?? "").Trim();
                            if(setting.Attribution == "") {
                                continue;
                            }

                            PortBrightnessClasses(setting);

                            results.Add(setting);
                        }
                    }
                } catch(JsonException ex) {
                    // These can occur if the user is writing their own custom JSON and they've knackered it
                    // up. If JSON coming from the server is throwing these then they're more serious, they
                    // need addressing.
                    if(!isCustom) {
                        throw;
                    }
                    _Log.Message($"Caught exception parsing {fullPath}: {ex}");
                }
            }
        }

        private static readonly Regex _BrightnessRegex = new(
            @"\b(?<class>vrs-brightness-(?<brightness>10|20|30|40|50|60|70|80|90|100|110|120|130|140|150))\b"
        );

        /// <summary>
        /// Converts version 1 brightness classes to default brightness values.
        /// </summary>
        /// <param name="setting"></param>
        /// <remarks>
        /// The first version of this used classes to infer brightness. In v1.01 brightness is configurable
        /// and a default brightness can be assigned. If we're loading an old definition that still uses
        /// classes for brightness then this removes the brightness classes and turns them into default
        /// brightness values.
        /// </remarks>
        private void PortBrightnessClasses(DownloadedTileServerSettings setting)
        {
            var match = _BrightnessRegex.Match(setting?.ClassName ?? "");
            if(match.Success) {
                var classGroup = match.Groups["class"];
                var brightnessGroup = match.Groups["brightness"];

                setting.ClassName = setting.ClassName
                    .Remove(classGroup.Index, classGroup.Length)
                    .Trim();

                setting.DefaultBrightness = int.Parse(brightnessGroup.Value);
            }
        }

        /// <inheritdoc/>
        public void CreateReadme()
        {
            var folder = Folder;
            _FileSystem.CreateDirectoryIfNotExists(folder);
            _FileSystem.WriteAllText(
                _FileSystem.Combine(folder, _ReadMeFileName),
                "ReadMe text goes here"     // TODO
            );
        }
    }
}
