# Virtual Radar Server

A .NET Core application for Windows, Linux and macOS that can decode Mode-S and ADS-B
transmissions from aircraft transponders and plot their positions on a map.

## Compilation

You will need the .NET 8 SDK, available here:

`https://dotnet.microsoft.com/en-us/download/dotnet/8.0`

Confirm that you have a .NET Core 8 SDK installed:

| Operating System | Command |
| ---              | --- |
| All              | `dotnet --list-sdks` |

If this is your first build then restore the NuGet packages:

| Operating System | Command |
| ---              | --- |
| Windows          | `build restore` |

Then to build everything:

| Operating System | Command |
| ---              | --- |
| Windows          | `build solution` |
