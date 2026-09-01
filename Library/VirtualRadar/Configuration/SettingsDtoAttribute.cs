namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Describes a settings object that is associated with a key in the settings file. Automatically
    /// registers the settings with <see cref="ConfigurationConfig"/> on startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SettingsDtoAttribute : Attribute
    {
        public string SettingsKey { get; set; }

        public SettingsDtoAttribute(string settingsKey)
        {
            SettingsKey = settingsKey;
        }
    }
}
