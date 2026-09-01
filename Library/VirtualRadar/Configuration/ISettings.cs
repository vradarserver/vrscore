namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Fetches the latest value of a DTO object that has been stored in persistent
    /// settings.
    /// </summary>
    /// <typeparam name="TSettingsDto"></typeparam>
    [Lifetime(Lifetime.Singleton)]
    public interface ISettings<TSettingsDto>
    {
        /// <summary>
        /// Fetches the latest value. If you need to guarantee that the value does not
        /// change over a set of operations then take a reference to this value and use
        /// the reference. The latest value can change at any time, and two threads
        /// reading the latest value can block each other while <see
        /// cref="ISettingsStorage"/> resolves any re-fetches after a change in settings
        /// is detected.
        /// </summary>
        public TSettingsDto LatestValue { get; }
    }
}
