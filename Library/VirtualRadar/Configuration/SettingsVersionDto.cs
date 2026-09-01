namespace VirtualRadar.Configuration
{
    /// <summary>
    /// Describes the schema of the Settings.json file.
    /// </summary>
    /// <param name="SchemaVersion"></param>
    [Settings("Version")]
    public record SettingsVersionDto(
        int SchemaVersion = 1
    )
    {
    }
}
