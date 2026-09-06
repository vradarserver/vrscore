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
using VirtualRadar.CommandLine.Validators;

namespace VirtualRadar.Utility.CLIConsole
{
    static class Options
    {
        public static Option<string> Code_Required = new("--code") {
            Description = "The code to look up",
            Required = true,
        };

        public static Option<string> FeedFormat_Default_VrsBaseStation = new("--feed-format") {
            Description = "Feed format to use for parsing packet",
            DefaultValueFactory = _ => "vrs-basestation",
        };

        public static Option<Icao24[]> Icao24List_Required = new("--icaos") {
            Description = "Comma-separated list of ICAO24s",
            CustomParser = arg => Icao24ListParser.ParseIcao24List(arg),
            Required = true,
        };

        public static Option<IPAddress> IPAddress_Default_127_0_0_1 = new("--ip-address", "-ip") {
            Description = "The IP address",
            CustomParser = arg => IPAddressParser.ParseIPAddress(arg),
            DefaultValueFactory = _ => IPAddress.Parse("127.0.0.1"),
        };

        public static Argument<ListEntity> ListEntityArgument = new("entity") {
            Description = "The entity type to list",
        };

        public static Argument<FileInfo> LoadFileInfoArgument = new("filename") {
            Description = "The recorded feed file to dump",
            Validators = { FileExistsValidator.Validate },
        };

        public static Argument<LookupEntity> LookupEntityArgument = new("entity") {
            Description = "The entity type to look up",
        };

        public static Argument<OpenEntity> OpenEntityArgument = new("entity") {
            Description = "The entity type to open",
        };

        public static Option<bool> ParseMessage_Default_False = new("--parse-message") {
            Description = "Parse and display messages from feed",
            DefaultValueFactory = _ => false,
        };

        public static Option<int> Port_Default_30003 = new("--port", "-p") {
            Description = "The IP port",
            CustomParser = arg => PortParser.ParsePort(arg),
            DefaultValueFactory = _ => 30003,
        };

        public static Option<FileInfo> SaveFileInfo = new("--save") {
            Description = "Save filename",
        };

        public static Option<FileInfo> SaveFileInfo_Required = new("--save") {
            Description = "Save filename",
            Required = true,
        };

        public static Option<bool> Show_Default_False = new("--show") {
            Description = "Show content",
            DefaultValueFactory = _ => false,
        };

        public static Argument<StandingDataEntity> StandingDataEntityArgument = new("entity") {
            Description = "The standing-data entity to dump",
        };

        public static Option<bool> Update_Default_False = new("--update") {
            Description = "Load and re-save settings to add missing entries",
            DefaultValueFactory = _ => false,
        };
    }
}
