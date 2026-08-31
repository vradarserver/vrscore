# VirtualRadar.Configuration

Configuration objects for various services and for the application as a whole.



## Setup

Classes with names that end with "Setup" are intended to be used with the
`Microsoft.Extensions.Options` library. They are typically declared as mutable
records - mutable because the options library needs them to be mutable, records
because value equality can be useful with configuration settings.



## SettingsDto

Classes with names that end with "SettingsDto" are DTO objects intended to be stored
and loaded with the homebrew `ISettings` object.

Not to be confused with the odd type here and there that has a "Settings" suffix.
Those are typically DTOs for settings objects sent by the SDM site, and just have
the Settings suffix to be consistent with the site.
