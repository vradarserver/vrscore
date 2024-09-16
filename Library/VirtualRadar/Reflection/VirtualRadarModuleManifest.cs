using System.IO;
using System.Reflection;
using VirtualRadar.Configuration;

namespace VirtualRadar.Reflection
{
    /// <summary>
    /// The information parsed out of [NAME OF MODULE].manifest.json
    /// </summary>
    public record VirtualRadarModuleManifest(
        string ModuleName,
        string MinVersion,
        string MaxVersion
    )
    {
        private InformationalVersion _MinimumSupportedVirtualRadarVersion;
        /// <summary>
        /// Gets <see cref="MinVersion"/> parsed into an <see cref="InformationalVersion"/>.
        /// </summary>
        public InformationalVersion MinimumSupportedVirtualRadarVersion
        {
            get {
                if(_MinimumSupportedVirtualRadarVersion == null) {
                    try {
                        _MinimumSupportedVirtualRadarVersion = InformationalVersion.Parse(MinVersion);
                    } catch {
                        _MinimumSupportedVirtualRadarVersion = InformationalVersion.Parse("9999.9999.9999");
                        throw;
                    }
                }
                return _MinimumSupportedVirtualRadarVersion;
            }
        }

        private InformationalVersion _MaximumSupportedVirtualRadarVersion;
        /// <summary>
        /// Gets <see cref="MaxVersion"/> parsed into an <see cref="InformationalVersion"/>.
        /// </summary>
        public InformationalVersion MaximumSupportedVirtualRadarVersion
        {
            get {
                if(_MaximumSupportedVirtualRadarVersion == null) {
                    try {
                        _MaximumSupportedVirtualRadarVersion = InformationalVersion.Parse(MaxVersion);
                    } catch {
                        _MaximumSupportedVirtualRadarVersion = InformationalVersion.Parse("9999.9999.9999");
                        throw;
                    }
                }
                return _MaximumSupportedVirtualRadarVersion;
            }
        }

        /// <summary>
        /// Returns a manifest for a module that has already been loaded. If it's already in the AppDomain
        /// then we accept that the application wanted it to be loaded and ignore whatever is on disk, and
        /// in particular we don't bother with a manifest for VirtualRadar.dll.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static VirtualRadarModuleManifest CreateForPreLoadedModule(Assembly assembly)
        {
            var version = InformationalVersion.FromAssembly(assembly);
            return new(
                assembly.FullName,
                version.ToString(),
                version.ToString()
            );
        }

        /// <summary>
        /// Returns true if the manifest version numbers indicate a minimum and maximum version that matches
        /// the version of Virtual Radar Server that it's been loaded into.
        /// </summary>
        /// <returns></returns>
        public bool IsForThisVersion()
        {
            return InformationalVersion.VirtualRadarVersion.CompareTo(MinimumSupportedVirtualRadarVersion) >= 0
                && InformationalVersion.VirtualRadarVersion.CompareTo(MaximumSupportedVirtualRadarVersion) <= 0;
        }
    }
}
