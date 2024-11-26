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
        /// Adds a callback that is called whenever <see cref="ChangeValue"/> changes a value associated with
        /// a key.
        /// </summary>
        /// <param name="callback">
        /// A callback that is passed a tuple, the first parameter of which is the key of the option that was
        /// changed and the second is the new value assigned to that key.
        /// </param>
        /// <returns>A handle that must be disposed of whenever the caller is disposed.</returns>
        ICallbackHandle AddValueChangedCallback(Action<ValueChangedCallbackArgs> callback);

        /// <summary>
        /// Adds a callback that is called whenever <see cref="SaveChanges"/> saves some changes.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>A handle that must be disposed of whenever the caller is disposed.</returns>
        ICallbackHandle AddSavedChangesCallback(Action callback);

        /// <summary>
        /// Returns the current value of the type passed across, assuming that the type uniquely identifies a
        /// registered top-level key.
        /// </summary>
        /// <param name="optionType">
        /// The type of option to load. This must have been previously registered with <see
        /// cref="ConfigurationConfig"/>.
        /// </param>
        /// <returns></returns>
        object LatestValue(Type optionType);

        /// <summary>
        /// Returns the current value of the type passed across, assuming that the type uniquely identifies a
        /// registered top-level key.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of option to load. This must have been previously registered with <see
        /// cref="ConfigurationConfig"/>.
        /// </typeparam>
        /// <returns></returns>
        TObject LatestValue<TObject>();

        /// <summary>
        /// Assigns a new value to the options associated with the option type passed across, assuming that
        /// the option type uniquely identifies a top-level key. This does not update persistent storage.
        /// </summary>
        /// <param name="optionType">
        /// The type of option to overwrite. This must have been previously registered with <see
        /// cref="ConfigurationConfig"/>.
        /// </param>
        /// <param name="newValue">
        /// The new value for the option. It must be derivable from <paramref name="optionType"/>.
        /// </param>
        void ChangeValue(Type optionType, object newValue);

        /// <summary>
        /// Assigns a new value to the options associated with the option type passed across, assuming that
        /// the option type uniquely identifies a top-level key. This does not update persistent storage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newValue"></param>
        void ChangeValue<T>(T newValue);

        /// <summary>
        /// Saves changes to the settings back to persistent storage.
        /// </summary>
        void SaveChanges();
    }
}
