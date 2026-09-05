// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
        private InformationalVersion? _MinimumSupportedVirtualRadarVersion;
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

        private InformationalVersion? _MaximumSupportedVirtualRadarVersion;
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
                assembly.FullName ?? throw new InvalidOperationException($"Assembly \"{assembly}\" does not have a full name"),
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
