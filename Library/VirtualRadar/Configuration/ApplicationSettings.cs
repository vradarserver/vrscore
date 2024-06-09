using System.Reflection;

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
        /// The application's version information, as parsed from the <see cref="AssemblyInformationalVersionAttribute"/>.
        /// </summary>
        public InformationalVersion InformationalVersion { get; set; }

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
            InformationalVersion.TryParse(
                assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                out var version
            );

            ApplicationName =       assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "No name configured";
            InformationalVersion =  version;
            BuildDate =             assembly == null ? default : PEHeader.ExtractBuildDate(assembly);
            CultureInfo =           CultureInfo.CurrentCulture;
            LocalTimeZone =         TimeZoneInfo.Local;
        }
    }
}
