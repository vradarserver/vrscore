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
