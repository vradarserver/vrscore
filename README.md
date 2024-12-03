# Virtual Radar Server

A .NET Core application for Windows, Linux and macOS that can decode Mode-S and
ADS-B transmissions from aircraft transponders and plot their positions on a
map.

This is in the very early stages of development. The mature version of VRS is
here:

https://github.com/vradarserver/vrs



## Compilation

You will need the .NET 8 SDK, available here:

https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Confirm that you have a .NET Core 8 SDK installed:

| Operating System | Command |
| ---              | --- |
| All              | `dotnet --list-sdks` |

To build everything:

| Operating System | Command |
| ---              | --- |
| Linux            | `./build.sh solution` |
| macOS            | `./build.sh solution` |
| Windows          | `build solution` |

To build and run the server:

| Operating System | Command |
| ---              | --- |
| Linux            | `./build.sh server -run` |
| macOS            | `./build.sh server -run` |
| Windows          | `build server -run` |

To just run the server without building first:

| Operating System | Command |
| ---              | --- |
| Linux            | `./run.sh server` |
| macOS            | `./run.sh server` |
| Windows          | `run server` |



## Server Command-Line Options

| Parameter              | Meaning |
| ---                    | --- |
| `-http <port number>`  | Specify the HTTP port to listen to. Note that ports below 1024 might be restricted or in use. Defaults to 5001. |
| `-https <port number>` | Specify the HTTPS port to listen to. Note that ports below 1024 might be restricted or in use. Defaults to 6001. |
| `-noBrowser`           | Don't open the default browser on the site after starting the server |
| `-noHTTP`              | Disable support for HTTP |
| `-noHTTPS`             | Disable support for HTTPS |
| `-showLog`             | Show web server logging on the console |
| `-dev`                 | Force development mode |



## Configuration

Configuration is via a JSON file called `Settings.json` in the working folder.
You can create a default JSON file (and add new defaults to an existing JSON
file) via the command-line console:

```
WINDOWS:
build console -run settings -update

LINUX / MAC
./build.sh console -run settings -update
```

The command will also tell you where the settings file is located.

### Receivers

Console support will be added for creating receiver entries in the settings file.
However, as of time of writing that does not exist. You need to add them yourself. They
are declared in the `Receivers` array under `MessageSources`. Here is an example of one
receiver that is listening to a BaseStation feed:

```
...
"MessageSources": {
  "Receivers": [
    {
      "Id": 1,
      "Name": "Example BaseStation Feed",
      "Enabled": true,
      "Hidden": false,
      "Connector": {
        "SettingsProvider": "TcpPullConnector",
        "Address": "192.168.0.100",
        "Port": 33003
      },
      "FeedDecoder": {
        "SettingsProvider": "BaseStationFeedDecoder"
      }
    }
  ]
},
...
```

and here is an example receiver for a VATSIM feed:

```
    {
      "Id": 2,
      "Name": "VATSIM-UK",
      "Enabled": true,
      "Hidden": false,
      "Connector": {
        "SettingsProvider": "VatsimConnector",
        "CentreOn": "Coordinate",
        "CentreOnLocation": {
          "Latitude": 54.497989,
          "Longitude": -4.555628
        },
        "CentreOnPilotCid": 0,
        "CentreOnAirport": null,
        "GeofenceDistanceUnit": "Miles",
        "GeofenceWidth": 560,
        "GeofenceHeight": 740
      },
      "FeedDecoder": {
        "SettingsProvider": "VatsimFeedDecoder",
        "AssumeSlowAircraftAreOnGround": true,
        "SlowAircraftThresholdSpeedKnots": 40,
        "InferModelFromModelType": true,
        "ShowInvalidRegistrations": false
      }
    }
```

As of time of writing no other feeds types have been ported across.
