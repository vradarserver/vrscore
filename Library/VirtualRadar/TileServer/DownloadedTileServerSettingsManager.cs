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
using VirtualRadar.Configuration;

namespace VirtualRadar.TileServer
{
    /// <summary>
    /// The default implementation of <see cref="IDownloadedTileServerSettingsManager"/>.
    /// </summary>
    class DownloadedTileServerSettingsManager(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettings<TileServerSettings> _Settings,
        IDownloadedTileServerSettingsStorage _Storage,
        IDownloadedTileServerSettingsDownloader _Downloader,
        ILog _Log
        #pragma warning restore IDE1006
    ) : IDownloadedTileServerSettingsManager, IDisposable
    {
        private readonly object _SyncLock = new();
        private bool _Started;
        private bool _Stopped;
        private readonly CallbackNoParamList _DownloadedCallbackList = new();
        private Timer _RefetchTimer;

        /// <summary>
        /// The collection of tile server settings last downloaded / loaded. Take a copy of the reference
        /// before reading, only overwrite within a lock.
        /// </summary>
        private DownloadedTileServerSettings[] _DownloadedSettings = [];

        /// <inheritdoc/>
        public DateTime LastDownloadUtc { get; private set; }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DownloadedTileServerSettingsManager() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _DownloadedCallbackList.Dispose();
                _RefetchTimer?.Dispose();
            }
        }

        /// <inheritdoc/>
        public ICallbackHandle AddTileServerSettingsDownloadedCallback(Action callback)
        {
            return _DownloadedCallbackList.Add(callback);
        }

        /// <inheritdoc/>
        public string Start()
        {
            var result = new StringBuilder();

            if(!_Started) {
                lock(_SyncLock) {
                    if(!_Started) {
                        _Started = true;

                        var settings = _Settings.LatestValue;

                        CatchErrorsDuringInitialise(result, "downloading tile server settings", () => {
                            if(!_Storage.DownloadedSettingsFileExists()) {
                                DoDownloadTileServerSettings(settings.BlockingDownloadTimeoutSeconds);
                            }
                        });

                        CatchErrorsDuringInitialise(result, "loading tile server settings", () => {
                            LoadTileServerSettings();
                        });

                        CatchErrorsDuringInitialise(result, "creating tile server settings readme", () => {
                            _Storage.CreateReadme();
                        });

                        StartRefetchTimer();
                    }
                }
            }

            return result.ToString();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if(_Started && !_Stopped) {
                _Stopped = true;
                var timer = _RefetchTimer;
                if(timer != null) {
                    try {
                        timer.Dispose();
                    } catch {
                    }
                    _RefetchTimer = null;
                }
            }
        }

        private void CatchErrorsDuringInitialise(StringBuilder messageBuffer, string describeAction, Action action)
        {
            try {
                action();
            } catch(Exception ex) {
                var msg = $"Caught exception while {describeAction} at startup. See log for more details.";
                if(messageBuffer.Length > 0) {
                    messageBuffer.AppendLine();
                }
                messageBuffer.AppendLine(msg);
                _Log.Exception(ex, $"Caught exception while {describeAction} at startup");
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public DownloadedTileServerSettings GetDefaultTileServerSettings(MapProvider mapProvider)
        {
            var settings = _DownloadedSettings;
            return settings.FirstOrDefault(setting =>
                   setting.IsDefault
                && !setting.IsCustom
                && !setting.IsLayer
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="fallbackToDefaultIfMissing"></param>
        /// <returns></returns>
        public DownloadedTileServerSettings GetTileServerSettings(
            MapProvider mapProvider,
            string name,
            bool fallbackToDefaultIfMissing
        )
        {
            var settings = _DownloadedSettings;
            var result = settings.FirstOrDefault(setting =>
                   setting.MapProvider == mapProvider
                && String.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase)
                && !setting.IsLayer
            );
            if(result == null && fallbackToDefaultIfMissing) {
                result = GetDefaultTileServerSettings(mapProvider);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="includeTileServers"></param>
        /// <param name="includeTileLayers"></param>
        /// <returns></returns>
        public DownloadedTileServerSettings GetTileServerOrLayerSettings(
            MapProvider mapProvider,
            string name,
            bool includeTileServers,
            bool includeTileLayers
        )
        {
            var settings = _DownloadedSettings;
            return settings.FirstOrDefault(setting =>
                   setting.MapProvider == mapProvider
                && String.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase)
                && ((!setting.IsLayer && includeTileServers) || (setting.IsLayer &&  includeTileLayers))
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public IReadOnlyList<DownloadedTileServerSettings> GetAllTileServerSettings(MapProvider mapProvider)
        {
            return GetAllTileServerSettings(mapProvider, isLayer: false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public IReadOnlyList<DownloadedTileServerSettings> GetAllTileLayerSettings(MapProvider mapProvider)
        {
            return GetAllTileServerSettings(mapProvider, isLayer: true);
        }

        private IReadOnlyList<DownloadedTileServerSettings> GetAllTileServerSettings(
            MapProvider mapProvider,
            bool isLayer
        )
        {
            var settings = _DownloadedSettings;
            return settings
                .Where(setting => setting.MapProvider == mapProvider && setting.IsLayer == isLayer)
                .ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void DownloadTileServerSettings()
        {
            var settings = _Settings.LatestValue;
            DoDownloadTileServerSettings(settings.BlockingDownloadTimeoutSeconds);
        }

        private void DoDownloadTileServerSettings(int timeoutSeconds)
        {
            lock(_SyncLock) {
                IReadOnlyList<DownloadedTileServerSettings> settings = null;

                try {
                    settings = _Downloader.Download(timeoutSeconds);
                    if(settings != null) {
                        LastDownloadUtc = DateTime.UtcNow;
                    }
                } catch(Exception ex) {
                    _Log.Exception(ex, "Caught exception downloading new tile server settings");
                    settings = null;
                }

                if(settings != null) {
                    _Storage.SaveDownloadedSettings(settings);
                    var ex = _DownloadedCallbackList.InvokeWithoutExceptions();
                    if(ex != null) {
                        _Log.Exception(ex, "Caught exception during callbacks after downloading new tile server settings");
                    }
                }

                LoadTileServerSettings();
            }
        }

        /// <summary>
        /// Loads tile server settings from disk and records them for later use.
        /// </summary>
        private void LoadTileServerSettings()
        {
            lock(_SyncLock) {
                var settings = new List<DownloadedTileServerSettings>(
                    _Storage.Load()
                );

                AddDefaultLeafletTileServer(settings);
                EnsureSingleDefaultIsPresent(settings, MapProvider.Leaflet);
                FixupAttributions(settings);

                _DownloadedSettings = settings.ToArray();
            }
        }

        private static void EnsureSingleDefaultIsPresent(
            List<DownloadedTileServerSettings> results,
            MapProvider mapProvider
        )
        {
            var allDefaults = results.Where(setting =>
                   setting.MapProvider == mapProvider
                && setting.IsDefault
                && !setting.IsCustom
            ).ToArray();

            switch(allDefaults.Length) {
                case 0:
                    var nominated = results
                        .OrderBy(setting => (setting.Name ?? ""), StringComparer.InvariantCultureIgnoreCase)
                        .FirstOrDefault(setting => setting.MapProvider == mapProvider && !setting.IsCustom);
                    if(nominated != null) {
                        nominated.IsDefault = true;
                    }
                    break;
                case 1:
                    break;
                default:
                    var makeNotDefault = allDefaults
                        .OrderBy(setting => (setting.Name ?? ""), StringComparer.InvariantCultureIgnoreCase)
                        .Skip(1);
                    foreach(var notDefault in makeNotDefault) {
                        notDefault.IsDefault = false;
                    }
                    break;
            }
        }

        private static void AddDefaultLeafletTileServer(List<DownloadedTileServerSettings> results)
        {
            if(!results.Any(r => r.MapProvider == MapProvider.Leaflet)) {
                results.Add(new DownloadedTileServerSettings() {
                    MapProvider =   MapProvider.Leaflet,
                    Name =          "OpenStreetMap",
                    IsDefault =     true,
                    Url =           "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                    Attribution =   "[c] [a href=http://www.openstreetmap.org/copyright]OpenStreetMap[/a]",
                    ClassName =     "vrs-brightness-70",
                    MaxZoom =       19,
                });
            }
        }

        static readonly Regex _AttributionRegex = new(@"\[attribution (?<name>.+?)\]", RegexOptions.Compiled);

        private static void FixupAttributions(List<DownloadedTileServerSettings> allSettings)
        {
            foreach(var mapProvider in allSettings.Select(setting => setting.MapProvider).Distinct()) {
                var providerSettings = allSettings
                    .Where(setting => setting.MapProvider == mapProvider)
                    .ToArray();
                foreach(var setting in providerSettings) {
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var completed = false;

                    do {
                        var match = _AttributionRegex.Match(setting.Attribution ?? "");
                        completed = !match.Success;
                        if(!completed) {
                            var name = match.Groups["name"].Value ?? "";
                            if(usedNames.Contains(name)) {
                                throw new InvalidOperationException(
                                    $"Found recursive reference to {name} when expanding attribute for {mapProvider} server {setting.Name}"
                                );
                            }
                            usedNames.Add(name);

                            var otherSetting = providerSettings.FirstOrDefault(candidate =>
                                String.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)
                            );
                            var buffer = new StringBuilder(setting.Attribution);
                            buffer.Remove(match.Index, match.Length);
                            buffer.Insert(match.Index, otherSetting?.Attribution ?? $"Unknown attribution ID {name}");
                            setting.Attribution = buffer.ToString();
                        }
                    } while(!completed);
                }
            }
        }

        private void StartRefetchTimer()
        {
            if(_RefetchTimer != null) {
                _RefetchTimer.Dispose();
                _RefetchTimer = null;
            }

            // Note that the backoff timer will be used under two circumstances:
            //   1. We tried to download settings at startup but it timed out / was bad.
            //   2. We found the downloaded settings cache file and loaded that on startup,
            //      which means the "last downloaded" time has not yet been set and we are
            //      going to fetch a fresh copy while the system uses the cached version.

            var settings = _Settings.LatestValue;
            var intervalMilliseconds = LastDownloadUtc == DateTime.MinValue
                ? settings.BackoffPeriodSeconds * 1000
                : settings.RefetchPeriodHours * 60 * 60 * 1000;

            if(intervalMilliseconds > 0) {
                _RefetchTimer = new(
                    RefetchSettings,
                    state: null,
                    intervalMilliseconds,
                    Timeout.Infinite
                );
            }
        }

        private void RefetchSettings(object _)
        {
            if(!_Stopped) {
                try {
                    var settings = _Settings.LatestValue;
                    DoDownloadTileServerSettings(settings.BackgroundDownloadTimeoutSeconds);
                } catch(Exception ex) {
                    _Log.Exception(ex, "Caught exception when refetching the tile server settings");
                }
                StartRefetchTimer();
            }
        }
    }
}
