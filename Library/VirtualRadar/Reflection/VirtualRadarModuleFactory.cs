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
using System.Reflection;
using Newtonsoft.Json;
using VirtualRadar.Configuration;

namespace VirtualRadar.Reflection
{
    /// <summary>
    /// Manages the discovery and instantiation of VRS modules, both application and plugin.
    /// </summary>
    static class VirtualRadarModuleFactory
    {
        private static readonly object _SyncLock = new();
        private static readonly string _ModuleFolder;
        private static readonly string _PluginsFolder;
        private static bool _DiscoveryDone;

        /// <summary>
        /// Gets the folder where all of the modules live.
        /// </summary>
        public static string ModuleFolder => _ModuleFolder;

        /// <summary>
        /// Gets the top-level folder where all of the plugins live.
        /// </summary>
        public static string PluginsFolder => _PluginsFolder;

        private static VirtualRadarModuleInfo[] _LoadedModules = [];
        /// <summary>
        /// Gets a list of loaded modules sorted by priority.
        /// </summary>
        public static VirtualRadarModuleInfo[] LoadedModules => _LoadedModules;

        private static VirtualRadarModuleReject[] _RejectedModules = [];
        /// <summary>
        /// Gets a list of rejected modules sorted by priority.
        /// </summary>
        public static VirtualRadarModuleReject[] RejectedModules => _RejectedModules;

        /// <summary>
        /// Static ctor.
        /// </summary>
        static VirtualRadarModuleFactory()
        {
            _ModuleFolder = Assembly.GetExecutingAssembly().Location;
            _PluginsFolder = Path.Combine(_ModuleFolder, "Plugins");
        }

        /// <summary>
        /// Finds all VRS modules in the application folder.
        /// </summary>
        public static void DiscoverModules()
        {
            if(!_DiscoveryDone) {
                lock(_SyncLock) {
                    if(!_DiscoveryDone) {
                        _DiscoveryDone = true;

                        var loadedModules = new List<VirtualRadarModuleInfo>();
                        var rejectedModules = new List<VirtualRadarModuleReject>();
                        var currentAssemblies = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .Where(assembly => {
                                var hasLocation = false;
                                try {
                                    hasLocation = Directory.Exists(
                                        Path.GetFullPath(assembly.Location)
                                    );
                                } catch {
                                    hasLocation = false;
                                }
                                return hasLocation;
                            })
                            .ToDictionary(
                                assembly => Path.GetFullPath(assembly.Location),
                                assembly => assembly,
                                StringComparer.InvariantCultureIgnoreCase
                            );

                        void loadDll(string dllFileName)
                        {
                            var fullyPathedFileName = Path.GetFullPath(dllFileName);

                            var rejectionReason = "";
                            try {
                                VirtualRadarModuleManifest manifest;
                                if(currentAssemblies.TryGetValue(fullyPathedFileName, out var loadedAssembly)) {
                                    manifest = VirtualRadarModuleManifest.CreateForPreLoadedModule(loadedAssembly);
                                } else {
                                    manifest = LoadManifest(dllFileName, ref rejectionReason);
                                }
                                if(manifest != null && !manifest.IsForThisVersion()) {
                                    rejectionReason = $"Only works with versions {manifest.MinVersion} to {manifest.MaxVersion}, VirtualRadar.dll is version {InformationalVersion.VirtualRadarVersion}";
                                }

                                if(rejectionReason == "") {
                                    if(loadedAssembly == null) {
                                        loadedAssembly = Assembly.LoadFrom(fullyPathedFileName);
                                    }
                                    var instanceModule = CreateModuleInstance(loadedAssembly, ref rejectionReason);
                                    if(instanceModule != null) {
                                        loadedModules.Add(new(fullyPathedFileName, manifest, instanceModule));
                                    }
                                }
                            } catch(Exception ex) {
                                rejectionReason = $"Caught exception during load: {ex}";
                            }

                            if(rejectionReason != "") {
                                rejectedModules.Add(new(fullyPathedFileName, rejectionReason));
                            }
                        }

                        foreach(var fileName in Directory.GetFiles(_ModuleFolder, "VirtualRadar.*.dll", SearchOption.TopDirectoryOnly)) {
                            loadDll(fileName);
                        }
                        if(Directory.Exists(_PluginsFolder)) {
                            foreach(var fileName in Directory.GetFiles(_PluginsFolder, "Plugin.VirtualRadar.*.dll", SearchOption.AllDirectories)) {
                                loadDll(fileName);
                            }
                        }

                        _LoadedModules = [..loadedModules
                            .OrderBy(r => r.ModuleInstance.Priority)
                            .ThenBy(r => r.FileName, StringComparer.InvariantCultureIgnoreCase)
                        ];
                        _RejectedModules = [..rejectedModules
                            .OrderBy(r => r.FileName, StringComparer.InvariantCultureIgnoreCase)
                        ];
                    }
                }
            }
        }

        private static VirtualRadarModuleManifest LoadManifest(string dllFileName, ref string rejectionReason)
        {
            VirtualRadarModuleManifest result = null;

            var manifestFileName = Path.Combine(
                Path.GetDirectoryName(dllFileName),
                $"{Path.GetFileNameWithoutExtension(dllFileName)}.manifest.json"
            );
            if(!File.Exists(manifestFileName)) {
                rejectionReason = $"Missing manifest file \"{manifestFileName}\"";
            } else {
                try {
                    var json = File.ReadAllText(manifestFileName);
                    result = JsonConvert.DeserializeObject<VirtualRadarModuleManifest>(json);
                } catch(Exception ex) {
                    result = null;
                    rejectionReason = $"Cannot parse manifest file \"{manifestFileName}\": {ex}";
                }
            }

            return result;
        }

        private static IVirtualRadarModule CreateModuleInstance(Assembly loadedAssembly, ref string rejectionReason)
        {
            IVirtualRadarModule result = null;

            var instanceModuleTypes = loadedAssembly
                .GetTypes()
                .Where(type => type.GetInterfaces().Any(inf => inf == typeof(IVirtualRadarModule)))
                .ToArray();

            if(instanceModuleTypes.Length == 0) {
                rejectionReason = $"No {nameof(IVirtualRadarModule)} implementation found";
            } else if(instanceModuleTypes.Length > 1) {
                rejectionReason = $"More than one {nameof(IVirtualRadarModule)} implementation found ({String.Join(", ", instanceModuleTypes.Select(type => type.Name))})";
            } else {
                if(Activator.CreateInstance(instanceModuleTypes[0]) is not IVirtualRadarModule instanceModule) {
                    rejectionReason = $"Created an instance of {instanceModuleTypes[0].Name} but it did not implement {nameof(IVirtualRadarModule)}";
                } else {
                    result = instanceModule;
                }
            }

            return result;
        }

        /// <summary>
        /// Calls all loaded modules.
        /// </summary>
        /// <param name="moduleAction"></param>
        /// <param name="ignoreExceptions"></param>
        /// <param name="inReverseOrder"></param>
        public static void CallLoadedModules(Action<VirtualRadarModuleInfo> moduleAction, bool ignoreExceptions = false, bool inReverseOrder = false)
        {
            DiscoverModules();

            var modules = LoadedModules;
            if(inReverseOrder) {
                modules = [..modules.Reverse()];
            }

            foreach(var module in modules) {
                try {
                    moduleAction(module);
                } catch {
                    if(!ignoreExceptions) {
                        throw;
                    }
                }
            }
        }
    }
}
