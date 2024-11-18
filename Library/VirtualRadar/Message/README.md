# VirtualRadar.Message

Classes and enums that together describe the content of a Mode-S / ADS-B message
sent by an aircraft.

The program accepts messages in different feed formats. All of these different
feeds are eventually converted into our internal message format, which is
described by `TransponderMessage`. That gets described here, along with a common
interface for classes that can convert feeds into the internal message format.

Along with that we also have the LookupOutcome here. Now, technically - not a
message. It's the outcome of a lookup of an aircraft's details, either in a
local database or from an online resource or whatever. However, it makes life
easier if all the lookup sources boil down into a common format as well, that
way we can translate between them.

We don't have a common interface for lookup converters because there's no common
source. Aircraft messages come from feeds, streams. Lookups could be coming from
files, databases, online resources etc.
