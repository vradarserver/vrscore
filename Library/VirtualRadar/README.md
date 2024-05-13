# VirtualRadar

The base library - holds interfaces, extensions to the runtime. Doesn't link
to anything.

## Dependency Injection

Add the library's services by calling `AddVirtualRadarGroup`.

## Namespaces

| Namespace                    | Usage |
| ---                          | --- |
| `VirtualRadar`               | General interfaces, utility classes, enums etc. |
| `VirtualRadar.Configuration` | Config objects that can be passed at startup to influence library behaviour. |
| `VirtualRadar.Connection`    | Abstraction of objects that can pull feeds from sources. |
| `VirtualRadar.Extensions`    | Extensions to .NET CLR classes. |
| `VirtualRadar.Format`        | Common string formatting. |
| `VirtualRadar.Message`       | The internal format of a message from an aircraft. |
