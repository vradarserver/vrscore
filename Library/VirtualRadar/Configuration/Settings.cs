namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="ISettings{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="_Settings"></param>
    class Settings<TOptions>(
        ISettingsStorage _Settings
    ) : ISettings<TOptions>
    {
        public TOptions LatestValue => _Settings.LatestValue<TOptions>();
    }
}
