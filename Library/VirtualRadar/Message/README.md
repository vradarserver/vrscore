# VirtualRadar.Message

Classes and enums that together describe the content of a Mode-S / ADS-B
message sent by an aircraft.

The program accepts messages in different feed formats. All of these
different feeds are eventually converted into our internal message format,
which is described by `TransponderMessage`.
