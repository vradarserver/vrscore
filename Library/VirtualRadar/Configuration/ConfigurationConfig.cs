using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VirtualRadar.Collections;
using VirtualRadar.Extensions;
using VirtualRadar.Reflection;

namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Configures the types that are stored within the Settings.json file in
    /// %LOCALAPPDATA%\VirtualRadarCore.
    /// </summary>
    public static class ConfigurationConfig
    {
        private static readonly object _SyncLock = new();
        private static readonly Dictionary<string, Type> _ProviderNameToSettingsProviderTypeMap = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, JObject> _SettingKeyToDefaultsMap = [];
        private static readonly Dictionary<Type, string> _SettingsDtoTypeToKeyMap = [];

        /// <summary>
        /// Calls the various automatic registration functions on the assembly passed
        /// across, or the calling assembly if null is passed.
        /// </summary>
        /// <param name="addToServices"></param>
        /// <param name="assembly"></param>
        public static void RegisterAssembly(IServiceCollection addToServices, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            RegisterSettingProviders(assembly);
            RegisterAssemblySettingDtos(addToServices, assembly);
        }

        /// <summary>
        /// Registers a settings provider type (usually a settings DTO) with a provider
        /// name. If more than one registration is made for the same provider name then
        /// the last one wins.
        /// </summary>
        /// <param name="providerName">Case-insensitive provider name.</param>
        /// <param name="settingsProviderType">
        /// The type of settings provider object (usually a settings DTO) that implements
        /// <see cref="ISettingsProvider"/>.
        /// </param>
        public static void RegisterProvider(string providerName, Type settingsProviderType)
        {
            try {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(providerName);
                ArgumentOutOfRangeException.ThrowIfEqual(
                    false,
                    typeof(ISettingsProvider).IsAssignableFrom(settingsProviderType)
                );

                lock(_SyncLock) {
                    _ProviderNameToSettingsProviderTypeMap[providerName] = settingsProviderType;
                }
            } catch(Exception ex) {
                ex.AddStringData("ProviderName",         () => providerName);
                ex.AddStringData("SettingsProviderType", () => settingsProviderType?.FullName);
                throw;
            }
        }

        /// <summary>
        /// Registers a settings provider type (usually a settings DTO) against a provider
        /// name. If more than one registration is made for the same provider name then
        /// the last one wins.
        /// </summary>
        /// <typeparam name="TSettingsProvider"></typeparam>
        /// <param name="providerName">Case-insensitive provider name.</param>
        public static void RegisterProvider<TSettingsProvider>(string providerName) where TSettingsProvider : ISettingsProvider
        {
            RegisterProvider(providerName, typeof(TSettingsProvider));
        }

        /// <summary>
        /// Calls <see cref="RegisterProvider"/> on all public types that implement <see
        /// cref="ISettingsProvider"/> in the assembly passed across.
        /// </summary>
        /// <param name="assembly">
        /// The optional assembly, defaults to the calling assembly if not supplied.
        /// </param>
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
        /// Returns the type associated with the settings provider name passed across.
        /// Returns null if the provider name has not been registered.
        /// </summary>
        /// <param name="providerName">Case-insensitive provider name.</param>
        /// <returns></returns>
        public static Type ProviderType(string providerName)
        {
            lock(_SyncLock) {
                _ProviderNameToSettingsProviderTypeMap.TryGetValue(providerName, out var result);
                return result;
            }
        }

        /// <summary>
        /// Returns the type associated with the <see
        /// cref="ISettingsProvider.SettingsProvider"/> value from the (presumably
        /// partially parsed) object passed across.
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <returns></returns>
        public static Type ProviderType(ISettingsProvider settingsProvider) => ProviderType(settingsProvider.SettingsProvider);

        /// <summary>
        /// Registers a settings type and default value to a key. If more than one object
        /// is registered against a key then both defaults are merged together (with the
        /// later call taking precedence over the first on common property names, case
        /// sensitive) and both types are registered against the key name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="settingsDtoType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="addToServices"></param>
        public static void RegisterKey(string key, Type settingsDtoType, object defaultValue, IServiceCollection addToServices)
        {
            try {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(key);
                ArgumentNullException.ThrowIfNull(settingsDtoType);
                ArgumentNullException.ThrowIfNull(defaultValue);
                ArgumentOutOfRangeException.ThrowIfEqual(false, defaultValue.GetType().IsAssignableTo(settingsDtoType), nameof(defaultValue));

                var defaultJObject = JObject.FromObject(
                    defaultValue,
                    JsonConfiguration.JsonSerialiser
                );
                lock(_SyncLock) {
                    if(_SettingKeyToDefaultsMap.TryGetValue(key, out var mergedObject)) {
                        mergedObject.Merge(defaultJObject, new() {
                            MergeArrayHandling = MergeArrayHandling.Replace,
                            MergeNullValueHandling = MergeNullValueHandling.Merge,
                        });
                        defaultJObject = mergedObject;
                    }
                    _SettingKeyToDefaultsMap[key] = defaultJObject;
                    _SettingsDtoTypeToKeyMap[settingsDtoType] = key;

                    if(addToServices != null) {
                        Type[] genericParameters = [ settingsDtoType ];
                        var serviceType = typeof(ISettings<>).MakeGenericType(genericParameters);
                        var implementationType = typeof(Settings<>).MakeGenericType(genericParameters);
                        addToServices.AddLifetime(serviceType, implementationType);
                    }
                }
            } catch(Exception ex) {
                ex.AddStringData("SettingKey",      () => key);
                ex.AddStringData("SettingsDtoType", () => settingsDtoType?.FullName);
                throw;
            }
        }

        /// <summary>
        /// Registers the default values and type for a top-level key in the settings DTO
        /// object. If more than one object is registered against a key then both defaults
        /// are merged together (with the later call taking precedence over the first on
        /// common property names, case sensitive) and both types are registered against
        /// the key name.
        /// </summary>
        /// <typeparam name="TSettingsDto"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="addToServices"></param>
        public static void RegisterKey<TSettingsDto>(string key, TSettingsDto defaultValue, IServiceCollection addToServices)
        {
            RegisterKey(key, typeof(TSettingsDto), defaultValue, addToServices);
        }

        /// <summary>
        /// Searches the assembly for all objects that have been tagged with <see
        /// cref="SettingsDtoAttribute"/> and registers them all.
        /// </summary>
        /// <param name="addToServices">
        /// The optional services to add an <see cref="ISettings{TSettingsDto}"/>
        /// configuration to.
        /// </param>
        /// <param name="assembly">
        /// The assembly to search for objects tagged with <see
        /// cref="SettingsDtoAttribute"/>. If this is null then the calling assembly is
        /// searched.
        /// </param>
        public static void RegisterAssemblySettingDtos(IServiceCollection addToServices, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            try {
                foreach(var typeAttr in AttributeTags.TaggedTypes<SettingsDtoAttribute>(assembly)) {
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
        /// Returns the name of the key that was registered against the settings DTO type
        /// passed across.
        /// </summary>
        /// <param name="settingsDtoType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static string GetKeyForSettingsDtoType(Type settingsDtoType)
        {
            ArgumentNullException.ThrowIfNull(settingsDtoType);

            lock(_SyncLock) {
                if(!_SettingsDtoTypeToKeyMap.TryGetValue(settingsDtoType, out var result)) {
                    throw new InvalidOperationException($"A setting key has not been configured for the setting DTO type {settingsDtoType.Name}");
                }
                return result;
            }
        }

        /// <summary>
        /// Returns a dictionary of top-level key names to the types that are registered
        /// against that key.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, List<Type>> GetTopLevelTypesMap()
        {
            var result = new Dictionary<string, List<Type>>();

            lock(_SyncLock) {
                foreach(var kvp in _SettingsDtoTypeToKeyMap) {
                    var type = kvp.Key;
                    var topLevelKey = kvp.Value;

                    if(!result.TryGetValue(topLevelKey, out var typesForKey)) {
                        typesForKey = [];
                        result.Add(topLevelKey, typesForKey);
                    }

                    typesForKey.Add(type);
                }
            }

            return result;
        }
    }
}
