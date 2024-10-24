# VirtualRadar.Feed.Vatsim

This namespace holds objects that can download the [VATSIM](https://vatsim.net/)
feed and translate it into messages that Virtual Radar Server can use.

VATSIM expose a single global feed. We don't want to have every feed connecting
to that global feed and downloading their own copy, it would be wasteful, so
instead there is a singleton feed object to do the downloading. Each VATSIM
feed connects to the singleton feed. It only downloads the VATSIM feed if at
least one feed is connected to it.
