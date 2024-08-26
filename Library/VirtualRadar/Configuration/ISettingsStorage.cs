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
        /// Returns the current value of the type passed across, assuming that the type uniquely identifies
        /// a registered top-level key.
        /// </summary>
        /// <param name="optionType">The type of option to load. This must have been previously registered
        /// with <see cref="ConfigurationConfig.RegisterKey"/>.</param>
        /// <returns></returns>
        object LatestValue(Type optionType);

        /// <summary>
        /// Returns the current value of the type passed across, assuming that the type uniquely identifies
        /// a registered top-level key.
        /// </summary>
        /// <typeparam name="TObject">The type of option to load. This must have been previously registered
        /// with <see cref="ConfigurationConfig.RegisterKey"/>.</typeparam>
        /// <returns></returns>
        TObject LatestValue<TObject>();
    }
}
