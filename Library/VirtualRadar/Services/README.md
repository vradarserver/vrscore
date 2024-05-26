# VirtualRadar.Services

Implementations of service interfaces. Some services are exposed as plain old classes,
but if a service needs to be replaced with a mock for unit tests then it is exposed
as an interface and the implementation is often kept private.

Services are also declared as interfaces if they're the kind of thing that a plugin
might want to swap out at runtime with its own implementation.

These are implementations for services exposed at the root of the VirtualRadar namespace
as interfaces. Not *all* of them are here, sometimes it makes more sense to have the
implementation elsewhere. But most of them are here.
