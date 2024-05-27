# Todo

These are the things that need to be done next.

- [ ] Can I run ipv6 over home network?
- [ ] Add ipvX selection to Terminal
- [ ] TCP listener
- [ ] Console option to write feed to file with timestamps as offset from start of recording
    * 4 byte unsigned big-endian millisecond offset since start of recording, limits recording to ~49 days
    * 2 byte big-endian packet size
    * n byte packet
    * If RTC changes during recording then don't let ms offset go backwards, but otherwise no special considerations for it
    * Have option to stop recording after N minutes (int), both default and max is 48 days (69120)
    * Parcel writes - every 5 seconds?
- [ ] Console option to play back feed from file to listeners using offsets from connection
    * Configurable port number
- [ ] UDP listener + connector
    * In principle a datagram should equal 1 chunk, but for TCP we have a stream + chunker in separate classes
- [ ] Add protocol selection to Console playback
- [ ] Add protocol selection to Terminal
- [ ] Port SBS-3 format - example of raw message feed
- [ ] Add format selection to Terminal
- [ ] Port VATSIM format - example of complex state + lookup feed
- [ ] TransponderMessage needs to indicate when lookup is not required because it is fake
- [ ] Terminal noddy listener needs to filter out ICAO24s for fake aircraft when requesting lookups
- [ ] Console option to open Explorer / Finder etc. on working folder
    * Not tested on Linux
    * Not tested on FreeBSD

These are the things that need doing but I don't need to think about right now. Very much not exhaustive.

- [ ] Text log - use Microsoft's logger, needs provider to write to text file
- [ ] Raspberry Pi support
- [ ] Plugin support
- [ ] Configuration
    * JSON store
    * Same store for both application and plugins
    * Can cope when plugin is uninstalled and its configuration remains
- [ ] Support per-install configuration of Options objects
- [ ] Translation - needs to be done before real UI gets done
    * Easy for users to do, no RESX editors
    * Same translations for C# and TypeScript
    * Plugins can use common translations or supply their own
    * Handles plurals
    * Handles quantities
- [ ] Terminal UI - wait for Terminal.Gui v2?
- [ ] Aircraft Online Lookup ephemeral log, can wait until GUI
- [ ] Queue ephemeral log, can wait until GUI
