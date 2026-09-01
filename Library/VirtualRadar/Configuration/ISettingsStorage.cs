namespace VirtualRadar.Configuration
{
    /// <summary>
    /// The interface for objects that support the reading and writing of settings to persistent
    /// storage.
    /// </summary>
    [Lifetime(Lifetime.Singleton)]
    public interface ISettingsStorage
    {
        /// <summary>
        /// Returns a description of where the settings are being stored - a database instance, a filename,
        /// whatever.
        /// </summary>
        /// <returns></returns>
        string SettingsLocation();

        /// <summary>
        /// Adds a callback that is called whenever <see cref="ChangeValue"/> changes a
        /// value associated with a key.
        /// </summary>
        /// <param name="callback">
        /// A callback that is passed a tuple, the first parameter of which is the key of
        /// the settings DTO that was changed and the second is the new value assigned to
        /// that key.
        /// </param>
        /// <returns>
        /// A handle that must be disposed of whenever the caller is disposed.
        /// </returns>
        ICallbackHandle AddValueChangedCallback(Action<ValueChangedCallbackArgs> callback);

        /// <summary>
        /// Adds a callback that is called whenever <see cref="SaveChanges"/> saves some changes.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>A handle that must be disposed of whenever the caller is disposed.</returns>
        ICallbackHandle AddSavedChangesCallback(Action callback);

        /// <summary>
        /// Returns the current value of the settings DTO type passed across, assuming
        /// that the type uniquely identifies a registered top-level key.
        /// </summary>
        /// <param name="settingsDtoType">
        /// The type of settings DTO to load. This must have been previously registered
        /// with <see cref="ConfigurationConfig"/>.
        /// </param>
        /// <returns></returns>
        object LatestValue(Type settingsDtoType);

        /// <summary>
        /// Returns the current value of the settings DTO type passed across, assuming
        /// that the type uniquely identifies a registered top-level key.
        /// </summary>
        /// <typeparam name="TSettingsDto">
        /// The type of settings DTO to load. This must have been previously registered
        /// with <see cref="ConfigurationConfig"/>.
        /// </typeparam>
        /// <returns></returns>
        TSettingsDto LatestValue<TSettingsDto>();

        /// <summary>
        /// True if there are settings in the configuration for the type passed across, false
        /// if defaults will be used if this type is passed to <see cref="LatestValue(Type)"/>.
        /// </summary>
        /// <param name="settingsDtoType"></param>
        /// <returns></returns>
        bool IsConfigured(Type settingsDtoType);

        /// <summary>
        /// True if there are settings in the configuration for the type passed across, false
        /// if defaults will be used if this type is passed to <see cref="LatestValue{TSettingsDto}()"/>.
        /// </summary>
        /// <typeparam name="TSettingsDto"></typeparam>
        /// <returns></returns>
        bool IsConfigured<TSettingsDto>();

        /// <summary>
        /// Assigns a new value to the DTO associated with the type passed across,
        /// assuming that the settings DTO type uniquely identifies a top-level key. This
        /// does not update persistent storage.
        /// </summary>
        /// <param name="settingsDtoType">
        /// The type of DTO to overwrite. This must have been previously registered with
        /// <see cref="ConfigurationConfig"/>.
        /// </param>
        /// <param name="newSettingsDto">
        /// The new value for the DTO. It must be derivable from <paramref
        /// name="settingsDtoType"/>.
        /// </param>
        void ChangeValue(Type settingsDtoType, object newSettingsDto);

        /// <summary>
        /// Assigns a new value to the settings DTO associated with the DTO type passed
        /// across, assuming that the type uniquely identifies a top-level key. This does
        /// not update persistent storage.
        /// </summary>
        /// <typeparam name="TSettingsDto"></typeparam>
        /// <param name="newSettingsDto"></param>
        void ChangeValue<TSettingsDto>(TSettingsDto newSettingsDto);

        /// <summary>
        /// Saves changes to the settings back to persistent storage.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Returns a string representation of a settings DTO object.
        /// </summary>
        /// <param name="settingsDto"></param>
        /// <returns></returns>
        string ToString(object settingsDto);
    }
}
