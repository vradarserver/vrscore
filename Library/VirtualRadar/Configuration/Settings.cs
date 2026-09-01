namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="ISettings{TSettingsDto}"/>.
    /// </summary>
    /// <typeparam name="TSettingsDto"></typeparam>
    /// <param name="_Settings"></param>
    class Settings<TSettingsDto>(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        ISettingsStorage _Settings
        #pragma warning restore IDE1006 // .editorconfig does not support naming rules for primary ctors
    ) : ISettings<TSettingsDto>
    {
        public TSettingsDto LatestValue => _Settings.LatestValue<TSettingsDto>();
    }
}
