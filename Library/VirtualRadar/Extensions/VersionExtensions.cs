namespace VirtualRadar.Extensions
{
    /// <summary>
    /// Extends System.Version.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// Returns <see cref="Version.Major"/>.<see cref="Version.Minor"/>.<see cref="Version.Build"/>
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string ToShortVersion(this Version version) => $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
