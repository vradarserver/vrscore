# Console

Command-line console for Virtual Radar Server.

## CLI options

Running the program without any parameters will show the help.

```
Console command [options]
  version             Show version information
  list <entity>       List stuff: FeedFormats
  connectTCP          Connect to TCP address
  lookup <entity>     Look something up: Aircraft

CONNECT OPTIONS
  -address <address>  Address to connect to [127.0.0.1]
  -port    <port>     Port to connect to [30003]
  -show               Show feed content [False]
  -save    <filename> Save feed content []

LOOKUP OPTIONS
  -id <string>        Hyphen-separated list of things to lookup []
```
