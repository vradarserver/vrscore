﻿namespace VirtualRadar.Configuration
{
    /// <summary>
    /// The interface that the bindable configuration object for all configurable providers has to implement.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// The name of the provider that can decode this configuration object.
        /// </summary>
        string SettingsProvider { get; }
    }
}
