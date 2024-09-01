namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Describes the configuration of a receiver.
    /// </summary>
    public class ReceiverSettings
    {
        public string Name { get; set; }

        public ISettingsProvider Connector { get; set; }

        public ISettingsProvider Translator { get; set; }

        public ISettingsProvider AircraftList { get; set; }
    }
}
