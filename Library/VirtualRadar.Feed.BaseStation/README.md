# VirtualRadar.Feed.BaseStation

All about reading and writing BaseStation format feeds.

## Initialisation

When setting up your dependency injection services include this namespace:

```
using VirtualRadar.Feed.BaseStation;
```

and call the `AddBaseStationFeedGroup()` extension method on `IServiceCollection`. E.G.:

```
var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices((context, services) => {
    services
        .AddVirtualRadarGroup()
        .AddBaseStationFeedGroup()          // <-- this
```
