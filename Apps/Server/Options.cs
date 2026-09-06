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
using VirtualRadar.CommandLine.Parsers;

namespace VirtualRadar.Server
{
    static class Options
    {
        public static Option<bool> Dev = new("--dev") {
            Description = "Force ASPNETCORE_ENVIRONMENT to Development",
            DefaultValueFactory = _ => false,
        };

        public static Option<string> Folder_Default = new("--folder") {
            Description = "Working folder",
            DefaultValueFactory = _ => Defaults.DefaultWorkingFolder,
        };

        public static Option<int> HttpPort_Default_5001 = new("--http") {
            Description = "HTTP port",
            CustomParser = arg => PortParser.ParsePort(arg),
            DefaultValueFactory = _ => 5001,
        };

        public static Option<int> HttpsPort_Default_6001 = new("--https") {
            Description = "HTTPS port",
            CustomParser = arg => PortParser.ParsePort(arg),
            DefaultValueFactory = _ => 6001,
        };

        public static Option<bool> NoBrowser = new("--no-browser") {
            Description = "Do not open a browser on start",
            DefaultValueFactory = _ => false,
        };

        public static Option<bool> NoHttp = new("--no-http") {
            Description = "Do not accept HTTP requests",
            DefaultValueFactory = _ => false,
        };

        public static Option<bool> NoHttps = new("--no-https") {
            Description = "Do not accept HTTPS requests",
            DefaultValueFactory = _ => false,
        };

        public static Option<bool> ShowLog = new("--show-log") {
            Description = "Show the server log on screen",
            DefaultValueFactory = _ => false,
        };
    }
}
