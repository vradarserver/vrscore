# VirtualRadar

The base library - holds interfaces, extensions to the runtime. Doesn't link
to anything.

## Dependency Injection

Add the library's services by calling `AddVirtualRadarGroup`.

### Lifetime Attributes

The default Microsoft DI framework lets services be re-registered, and the plugins
rely on this behaviour to supply their own implementations of interfaces that they
want to take over. However, things can get a bit uncomfortable if the new
implementation of the service has a different lifetime to the original. If it is
shorter than the original then you can end up with references getting pinned in
memory and not disposed of when expected, leading to resource leaks.

To address this there is a set of overloads to `IServiceCollection` called
`AddLifetime` that look on the service type for an attribute called `Lifetime`.
That attribute indicates the lifetime that the service should normally be
registered with.

You don't have to use the attribute or the overloads but it is recommended that,
unless you have a good reason not to, you should use them when reimplementing
VRS services.

## Namespaces

| Namespace                    | Usage |
| ---                          | --- |
| `VirtualRadar`               | General interfaces, utility classes, enums etc. |
| `VirtualRadar.Configuration` | Config objects that can be passed at startup to influence library behaviour. |
| `VirtualRadar.Connection`    | Abstraction of objects that can pull feeds from sources. |
| `VirtualRadar.Extensions`    | Extensions to .NET CLR classes. |
| `VirtualRadar.Format`        | Common string formatting. |
| `VirtualRadar.Message`       | The internal format of a message from an aircraft. |
