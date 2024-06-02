# VirtualRadar.Feed.Recording

All about recording and playing back a raw network feed. The intention isn't for end-users
to use this to have rewindable content, although it could be used for that - it is more to
make life easier during development, to have recordings of feeds that reproduce bugs, make
developing feed format decoders easier etc.

## Initialisation

When setting up your dependency injection services include this namespace:

```
using VirtualRadar.Feed.Recording;
```

and call the `AddFeedRecordingGroup()` extension method on `IServiceCollection`. E.G.:

```
var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices((context, services) => {
    services
        .AddVirtualRadarGroup()
        .AddFeedRecordingGroup()          // <-- this
```

## Feed Format

### Header

A recording starts with a 23-byte header:

| Offset | Bytes | Content |
| ---    | --:   | --- |
| 0      | 5     | The magic string `VRSFR` |
| 5      | 1     | The format version number as a binary value |
| 6      | 17    | The date and time at UTC of the start of the recording |

There is only one valid version number, it is 1 (0x01).

The date and time are formatted as an ASCII string:

    YYYYMMDDHHmmSSFFF

| Portion | Meaning |
| ---     | --- |
| YYYY    | Year |
| MM      | Month with leading zeros |
| DD      | Day with leading zeros |
| HH      | 24-hour hour with leading zeros |
| mm      | 24-hour minute with leading zeros |
| SS      | 24-hour second with leading zeros |
| FFF     | 24-hour millisecond with leading zeros |

### Body

The body is a sequential series of parcels. The first parcel starts immediately
after the header, and each subsequent parcel starts immediately after the previous.

#### Parcel

A parcel records the number of milliseconds into the recording that a packet
was recorded, the length of the packet and then the packet itself.

| Offset | Bytes  | Content
| ---    | --:    | --- |
| 0      | 4      | Big-endian unsigned milliseconds since recording started |
| 4      | 2      | Big-endian unsigned length of packet |
| 6      | Varies | Packet content |

If the real-time clock changes during recording then the number of milliseconds
since recording started could potentially go backwards.

Using a 4 byte signed integer number of milliseconds limits playback to millisecond
resolution and recording length to just under 49 days. The maximum allowable duration
of a recording is 48 days.

Using a 2 byte signed integer for the packet size limits packets to 64 KB, which
should be enough to record all TCP and UDP sources.
