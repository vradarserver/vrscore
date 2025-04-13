using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VirtualRadar.Configuration
{
    static class JsonConfiguration
    {
        public static JsonSerializerSettings JsonSerialiserSettings { get; }

        public static JsonSerializer JsonSerialiser { get; }

        public static JsonSerializerSettings JsonDeserialiserSettings { get; }

        static JsonConfiguration()
        {
            JsonDeserialiserSettings = new();
            JsonDeserialiserSettings.Converters.Add(new SettingsProviderJsonConverter());

            JsonSerialiserSettings = new();
            JsonSerialiserSettings.Converters.Add(new StringEnumConverter());

            JsonSerialiser = JsonSerializer.Create(JsonSerialiserSettings);
        }
    }
}
