using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VirtualRadar.Collections;
using VirtualRadar.Extensions;
using VirtualRadar.Reflection;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Configures the types that are stored within the Settings.json file in %LOCALAPPDATA%\VirtualRadarCore.
    /// </summary>
    public static class ConfigurationConfig
    {
        private static readonly object _SyncLock = new();
        private static readonly Dictionary<string, Type> _ProviderNameToConfigurationTypeMap = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, JObject> _SettingKeyToDefaultsMap = [];
        private static readonly Dictionary<Type, string> _SettingTypeToKeyMap = [];

        /// <summary>
        /// Calls the various automatic registration functions on the assembly passed across, or the
        /// calling assembly if null is passed.
        /// </summary>
        /// <param name="addToServices"></param>
        /// <param name="assembly"></param>
        public static void RegisterAssembly(IServiceCollection addToServices, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            RegisterSettingProviders(assembly);
            RegisterAssemblySettingObjects(addToServices, assembly);
        }

        /// <summary>
        /// Registers a configuration type with a provider name. If more than one registration is made for the
        /// same provider name then the last one wins.
        /// </summary>
        /// <param name="providerName">Case-insensitive provider name.</param>
        /// <param name="configurationType">
        /// The type of configuration object. It must implement <see cref="ISettingsProvider"/>.
        /// </param>
        public static void RegisterProvider(string providerName, Type configurationType)
        {
            try {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(providerName);
                ArgumentOutOfRangeException.ThrowIfEqual(
                    false,
                    typeof(ISettingsProvider).IsAssignableFrom(configurationType)
                );

                lock(_SyncLock) {
                    _ProviderNameToConfigurationTypeMap[providerName] = configurationType;
                }
            } catch(Exception ex) {
                ex.AddStringData("ProviderName",        () => providerName);
                ex.AddStringData("ConfigurationType",   () => configurationType?.FullName);
                throw;
            }
        }

        /// <summary>
        /// Registers a configuration type against a provider name. If more than one registration is made for
        /// the same provider name then the last one wins.
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="providerName">Case-insensitive provider name.</param>
        public static void RegisterProvider<TConfig>(string providerName) where TConfig : ISettingsProvider
        {
            RegisterProvider(providerName, typeof(TConfig));
        }

        /// <summary>
        /// Calls <see cref="RegisterProvider"/> on all public types that implement <see cref="ISettingsProvider"/>
        /// in the assembly passed across.
        /// </summary>
        /// <param name="assembly">The optional assembly, defaults to the calling assembly if not supplied.</param>
        public static void RegisterSettingProviders(Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            try {
                foreach(var typeAttr in AttributeTags.TaggedTypes<SettingsProviderAttribute>(assembly)) {
                    RegisterProvider(typeAttr.Attribute.Provider, typeAttr.Type);
                }
            } catch(Exception ex) {
                ex.AddStringData("Assembly", () => assembly.FullName);
                throw;
            }
        }

        /// <summary>
        /// Returns the type associated with the configuration provider name passed across. Returns null if
        /// the provider name has not been registered.
        /// </summary>
        /// <param name="providerName">Case-insensitive provider name.</param>
        /// <returns></returns>
        public static Type ProviderType(string providerName)
        {
            lock(_SyncLock) {
                _ProviderNameToConfigurationTypeMap.TryGetValue(providerName, out var result);
                return result;
            }
        }

        /// <summary>
        /// Returns the type associated with the <see cref="ISettingsProvider.SettingsProvider"/> value from
        /// the (presumably partially parsed) object passed across.
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <returns></returns>
        public static Type ProviderType(ISettingsProvider settingsProvider) => ProviderType(settingsProvider.SettingsProvider);

        /// <summary>
        /// Registers a settings type and default value to a key. If more than one object is registered
        /// against a key then both defaults are merged together (with the later call taking precedence over
        /// the first on common property names, case sensitive) and both types are registered against the key
        /// name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="optionsType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="addToServices"></param>
        public static void RegisterKey(string key, Type optionsType, object defaultValue, IServiceCollection addToServices)
        {
            try {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(key);
                ArgumentNullException.ThrowIfNull(optionsType);
                ArgumentNullException.ThrowIfNull(defaultValue);
                ArgumentOutOfRangeException.ThrowIfEqual(false, defaultValue.GetType().IsAssignableTo(optionsType), nameof(defaultValue));

                var defaultJObject = JObject.FromObject(defaultValue);
                lock(_SyncLock) {
                    if(_SettingKeyToDefaultsMap.TryGetValue(key, out var mergedObject)) {
                        mergedObject.Merge(defaultJObject, new() {
                            MergeArrayHandling = MergeArrayHandling.Replace,
                            MergeNullValueHandling = MergeNullValueHandling.Merge,
                        });
                        defaultJObject = mergedObject;
                    }
                    _SettingKeyToDefaultsMap[key] = defaultJObject;
                    _SettingTypeToKeyMap[optionsType] = key;

                    if(addToServices != null) {
                        Type[] genericParameters = [ optionsType ];
                        var serviceType = typeof(ISettings<>).MakeGenericType(genericParameters);
                        var implementationType = typeof(Settings<>).MakeGenericType(genericParameters);
                        addToServices.AddLifetime(serviceType, implementationType);
                    }
                }
            } catch(Exception ex) {
                ex.AddStringData("SettingKey",  () => key);
                ex.AddStringData("OptionsType", () => optionsType?.FullName);
                throw;
            }
        }

        /// <summary>
        /// Registers the default values and type for a top-level key in the settings object. If more than
        /// one object is registered against a key then both defaults are merged together (with the later
        /// call taking precedence over the first on common property names, case sensitive) and both types
        /// are registered against the key name.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="addToServices"></param>
        public static void RegisterKey<TOptions>(string key, TOptions defaultValue, IServiceCollection addToServices)
        {
            RegisterKey(key, typeof(TOptions), defaultValue, addToServices);
        }

        /// <summary>
        /// Searches the assembly for all objects that have been tagged with <see cref="SettingsAttribute"/>
        /// and registers them all.
        /// </summary>
        /// <param name="addToServices">
        /// The optional services to add an <see cref="ISettings{TOptions}"/> configuration to.
        /// </param>
        /// <param name="assembly">
        /// The assembly to search for objects tagged with <see cref="SettingsAttribute"/>. If this is null
        /// then the calling assembly is searched.
        /// </param>
        public static void RegisterAssemblySettingObjects(IServiceCollection addToServices, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            try {
                foreach(var typeAttr in AttributeTags.TaggedTypes<SettingsAttribute>(assembly)) {
                    var defaultValue = typeAttr.Type.CreateDefaultInstance();
                    RegisterKey(
                        typeAttr.Attribute.SettingsKey,
                        typeAttr.Type,
                        defaultValue,
                        addToServices
                    );
                }
            } catch(Exception ex) {
                ex.AddStringData("Assembly", () => assembly?.FullName);
                throw;
            }
        }

        /// <summary>
        /// Returns a copy of <see cref="_SettingKeyToDefaultsMap"/>.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, JObject> GetDefaultKeys()
        {
            lock(_SyncLock) {
                var result = ShallowCollectionCopier.Copy(_SettingKeyToDefaultsMap);
                return result;
            }
        }

        /// <summary>
        /// Returns the name of the key that was registered against the option type passed across.
        /// </summary>
        /// <param name="optionType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static string GetKeyForOptionType(Type optionType)
        {
            ArgumentNullException.ThrowIfNull(optionType);

            lock(_SyncLock) {
                if(!_SettingTypeToKeyMap.TryGetValue(optionType, out var result)) {
                    throw new InvalidOperationException($"A setting key has not been configured for the option type {optionType.Name}");
                }
                return result;
            }
        }
    }
}
