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
| `-showLog`             | Show web server logging on the console |
