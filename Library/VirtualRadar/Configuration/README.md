# VirtualRadar.Configuration

Configuration objects for various services and for the application as a whole.



## Naming Conventions

### Setup

Classes with names that end with "Setup" are intended to be used with the
`Microsoft.Extensions.Options` library. They are typically declared as mutable
records - mutable because the options library needs them to be mutable, records
because value equality can be useful with configuration settings.

### SettingsDto

Classes with names that end with "SettingsDto" are DTO objects intended to be stored
and loaded with the homebrew `ISettings` object.

Not to be confused with the odd type here and there that has a "Settings" suffix.
Those are typically DTOs for settings objects sent by the SDM site, and just have
the Settings suffix to be consistent with the site.



## ISettings

The `ISettings` DI-injected service stores and retrieves settings on behalf of the
application and the plugins. It does not know at compile time what is being stored
in it.

Settings are stored in `SettingsDto` objects. A DTO carries the settings for a
single part of the system - E.G. `AircraftMapSettingsDto` carries the configuration
for the V3 aircraft map.

Every DTO object has to be tagged with a `SettingsDto` attribute. The attribute has
a single parameter, which is a string. This is the top-level name that the settings
service will store the DTO under. Keys have to be unique, if two keys clash within
the program then one will win at random.

For every `SettingsDto` discovered by the program an `ISettings` interface will be
registered with DI for the setting DTO type. This setting has a single property,
`LatestValue`. It always returns the last value loaded from settings for that type.
For example:

```C#
private ISettings<AircraftMapSettingsDto> _AircraftMapSettings;

public MyClassCtor(
  ISettings<AircraftMapSettingsDto> aircraftMapSettingsDto
)
{
    _AircraftMapSettings = aircraftMapSettingsDto;
}

private void SomeFunction()
{
    var mapConfig = _AircraftMapSettings.LatestValue;
}
```



## ISettingsProvider

Sometimes you do not know at compile-time what types a particular setting DTO will
be referencing. For example, a `ReceiverSettingsDto` has these three properties:

```C#
public IReceiverConnectorSettingsDto?    Connector    { get; init; }
public IReceiverFeedDecoderSettingsDto?  FeedDecoder  { get; init; }
public IReceiverAircraftListSettingsDto? AircraftList { get; init; }
```

A receiver needs a connector, but the actual type of connector will not be known
until run-time. We need something that we can write to the settings JSON that
indicates which type of connector has been chosen by the user.

The `ISettingsProvider` specifies that the implementation has a string property
called `ServiceProvider`. Each DTO that can be used as a ServiceProvider returns
a unique name for the property.

For example, `TcpPullConnectorSettingsDto` uses the provider name `TcpPullConnector`.
This is emitted along with the rest of the TCP pull connector's settings to the
settings JSON. When the settings are read back the service provider is read first
and used to work out which settings DTO to deserialise to.
