// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;
using System.IO;
using System.Net;
using VirtualRadar.CommandLine.Parsers;

namespace VirtualRadar.Utility.Terminal
{
    static class Options
    {
        public static Option<IPAddress> Address_Default_127_0_0_1 = new("--address") {
            Description = "IP address of the BaseStation feed",
            CustomParser = arg => IPAddressParser.ParseIPAddress(arg),
            DefaultValueFactory = _ => IPAddress.Parse("127.0.0.1"),
        };

        public static Option<double> PlaybackSpeed_Default_1 = new("--speed") {
            Description = "Playback speed multiplier for a recording",
            DefaultValueFactory = _ => 1.0,
        };

        public static Option<int> Port_Default_30003 = new("--port", "-p") {
            Description = "TCP port of the BaseStation feed",
            CustomParser = arg => PortParser.ParsePort(arg),
            DefaultValueFactory = _ => 30003,
        };

        public static Option<string> ReceiverName = new("--receiver") {
            Description = "Show a configured receiver by name instead of an ad-hoc feed",
        };

        public static Option<FileInfo> RecordingFileInfo = new("--recording") {
            Description = "Replay a feed recording file instead of connecting to a live feed",
        };
    }
}
