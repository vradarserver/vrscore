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
using System.Text.RegularExpressions;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// The version number information extracted from an assembly's InformationalVersion attribute.
    /// </summary>
    /// <param name="VersionTag">
    /// The content of the Version tag in an attribute, as read via an assembly's InformationalVersion
    /// attribute.
    /// </param>
    /// <param name="Major">The major version number parsed from <see cref="VersionTag"/>.</param>
    /// <param name="Minor">The minor version number parsed from <see cref="VersionTag"/>.</param>
    /// <param name="Patch">The patch version number parsed from <see cref="VersionTag"/>.</param>
    /// <param name="ReleaseType">The type of release parsed from <see cref="VersionTag"/>.</param>
    /// <param name="Revision">The alpha or beta revision number parsed from <see cref="VersionTag"/>.</param>
    /// <param name="CommitHash">The optional version control identifier that identifies the version of the source that the application was built from. Parsed from <see cref="VersionTag"/>.</param>
    /// <remarks>
    /// <para>The informational version is assumed to be in one of three forms:</para>
    /// <para>Normal release: major.minor.patch</para>
    /// <para>Alpha release: major.minor.patch-alpha-revision</para>
    /// <para>Beta release: major.minor.patch-beta-revision</para>
    /// <para>
    /// NuGet does not recognise revision numbers as numbers for sorting so you should use leading zeros with
    /// them. You do not need leading zeros on major, minor or patch, NuGet recognises those.
    /// </para><para>
    /// .NET compilers will often append the git hash for the commit that the source was built from in the
    /// form '+HASH'. The parser will optionally extract that and record it in the <see cref="CommitHash"/>
    /// property.
    /// </para></remarks>
    public record InformationalVersion(
        string VersionTag,
        ReleaseType ReleaseType,
        int Major,
        int Minor,
        int Patch,
        int Revision,
        string CommitHash
    ) : IComparable<InformationalVersion>
    {
        static Regex _ParseRegex = new Regex(
            @"(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(-(?<revisionType>alpha|beta)-(?<revision>\d+))?(\+(?<commitHash>.+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Gets the version of the VirtualRadar.dll library. This is distinct to the application's version.
        /// </summary>
        public static InformationalVersion VirtualRadarVersion { get; } = FromAssembly(
            Assembly.GetExecutingAssembly()
        );

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public InformationalVersion() : this("", 0, 0, 0, 0, 0, "")
        {
        }

        /// <inheritdoc/>
        public override string ToString() => ReleaseType == ReleaseType.Stable
            ? $"{Major}.{Minor}.{Patch}"
            : $"{Major}.{Minor}.{Patch}-{(ReleaseType == ReleaseType.Alpha ? "alpha" : "beta")}-{Revision}";

        /// <inheritdoc/>
        public int CompareTo(InformationalVersion other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var result = Major - other.Major;

            if(result == 0) {
                result = Minor - other.Minor;
            }
            if(result == 0) {
                result = Patch - other.Patch;
            }
            if(result == 0) {
                result = (int)ReleaseType - (int)other.ReleaseType;
            }
            if(result == 0) {
                result = Revision - other.Revision;
            }

            return result;
        }

        /// <summary>
        /// Returns true if this version is earlier than the other version, taking into consideration whether
        /// the newer version is alpha or beta and we're stable, alpha or beta.
        /// </summary>
        /// <param name="other">The version to test against</param>
        /// <param name="allowUpdateToAlpha">
        /// True if we want to permit updates from stable or beta to a new alpha.
        /// </param>
        /// <param name="allowUpdateToBeta">
        /// True if we want to permit updates from stable to a new beta.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// <paramref name="allowUpdateToAlpha"/> is always true if the current version is an alpha release.
        /// <paramref name="allowUpdateToBeta"/> is always true if the current version is an alpha or beta
        /// release.
        /// </remarks>
        public bool CanUpdateTo(InformationalVersion other, bool allowUpdateToAlpha, bool allowUpdateToBeta)
        {
            ArgumentNullException.ThrowIfNull(other);

            allowUpdateToAlpha = allowUpdateToAlpha || ReleaseType == ReleaseType.Alpha;
            allowUpdateToBeta =  allowUpdateToBeta  || ReleaseType != ReleaseType.Stable;

            var result = (other.ReleaseType != ReleaseType.Alpha || allowUpdateToAlpha)
                      && (other.ReleaseType != ReleaseType.Beta  || allowUpdateToBeta)
                      && CompareTo(other) < 0;

            return result;
        }

        /// <summary>
        /// Reads an InformationalVersion attribute from the assembly passed across.
        /// </summary>
        /// <param name="assembly">The assembly to extract an informational version from.</param>
        /// <returns>The informational version or null if the assembly has not been tagged.</returns>
        public static InformationalVersion FromAssembly(Assembly assembly)
        {
            var versionText = assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            TryParse(versionText, out var result);

            return result;
        }

        /// <summary>
        /// Parses a version tag into a version object.
        /// </summary>
        /// <param name="versionTag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static InformationalVersion Parse(string versionTag)
        {
            if(!TryParse(versionTag, out var result)) {
                throw new ArgumentOutOfRangeException($"{versionTag} cannot be parsed into an {nameof(InformationalVersion)}");
            }
            return result;
        }

        /// <summary>
        /// Tries to parse a version tag into a version.
        /// </summary>
        /// <param name="versionTag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool TryParse(string versionTag, out InformationalVersion version)
        {
            version = default;      // <-- VS2022 cannot figure out that there's no path where version is not assigned, it needs this to compile

            var match = _ParseRegex.Match(versionTag ?? "");
            var result = match.Success;

            if(result) {
                int major, minor = 0, patch = 0, revision = 0;

                result = int.TryParse(match.Groups["major"].Value, CultureInfo.InvariantCulture, out major)
                      && int.TryParse(match.Groups["minor"].Value, CultureInfo.InvariantCulture, out minor)
                      && int.TryParse(match.Groups["patch"].Value, CultureInfo.InvariantCulture, out patch);
                var revisionType = "";
                if(result && match.Groups["revisionType"].Success) {
                    revisionType = match.Groups["revisionType"].Value.ToLower();
                    result = match.Groups["revision"].Success;
                    result = result && int.TryParse(match.Groups["revision"].Value, CultureInfo.InvariantCulture, out revision);
                }
                var commitHash = match.Groups["commitHash"].Value;

                if(result) {
                    version = new(
                        versionTag,
                        revisionType == "alpha"
                            ? ReleaseType.Alpha
                            : revisionType == "beta"
                                ? ReleaseType.Beta
                                : ReleaseType.Stable,
                        major,
                        minor,
                        patch,
                        revision,
                        commitHash
                    );
                }
            }

            if(!result) {
                version = new();
            }

            return result;
        }
    }
}
