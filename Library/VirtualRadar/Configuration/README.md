# VirtualRadar.Configuration

Configuration objects for various services and for the application
as a whole.

## Options

Classes with names that end with "Options" are intended to be used
with the `Microsoft.Extensions.Options` library. They are typically
declared as mutable records - mutable because the options library
needs them to be mutable, records because value equality can be
useful with configuration settings.
