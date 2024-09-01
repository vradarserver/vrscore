namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Describes the configuration of a receiver.
    /// </summary>
    public record ReceiverSettings
    {
        public string Name { get; init; }

        public bool Enabled { get; init; }

        public ISettingsProvider Connector { get; init; }

        public ISettingsProvider Translator { get; init; }

        public ISettingsProvider AircraftList { get; init; }

        public ReceiverSettings(
            string name = null,
            bool enabled = true,
            ISettingsProvider connector = null,
            ISettingsProvider translator = null,
            ISettingsProvider aircraftList = null
        )
        {
            Name = name ?? "Receiver";
            Enabled = enabled;
            Connector = connector;
            Translator = translator;
            AircraftList = aircraftList;
        }
    }
}
