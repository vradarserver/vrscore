using System.Reflection;
using VirtualRadar.Extensions;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// A settings object that the application is expected to provide when initialising
    /// the library.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// The application's name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The application's version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Non-zero if this is an alpha version. Alpha versions will not check online for upgrades.
        /// </summary>
        public int AlphaRevision { get; set; }

        /// <summary>
        /// Non-zero if this is a beta version. Full versions will not report beta versions as an upgrade,
        /// but beta versions will report full versions matching <see cref="Version"/> or higher as an upgrade.
        /// </summary>
        public int BetaRevision { get; set; }

        /// <summary>
        /// Returns the short version number with a suffix indicating the alpha or beta revision, if any.
        /// </summary>
        public string VersionDescription
        {
            get {
                var result = new StringBuilder(Version.ToShortVersion());
                if(AlphaRevision > 0) {
                    result.AppendFormat("-alpha-{0:0000}", AlphaRevision);
                } else if(BetaRevision > 0) {
                    result.AppendFormat("-beta-{0:0000}", BetaRevision);
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// Gets the build date of the application.
        /// </summary>
        public DateTimeOffset BuildDate { get; set; }

        /// <summary>
        /// Gets the culture info that the application is using for its user interface.
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// Gets the local time zone for date conversions in the user interface.
        /// </summary>
        public TimeZoneInfo LocalTimeZone { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ApplicationSettings() : this(Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="assembly">The assembly to extract all information from.</param>
        public ApplicationSettings(Assembly assembly)
        {
            ApplicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "No name configured";
            Version =         assembly?.GetName()?.Version ?? new Version();
            BuildDate =       assembly == null ? default : PEHeader.ExtractBuildDate(assembly);
            CultureInfo =     CultureInfo.CurrentCulture;
            LocalTimeZone =   TimeZoneInfo.Local;
        }
    }
}
