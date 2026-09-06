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
using VirtualRadar.CommandLine;

namespace VirtualRadar.Server
{
    static class Commands
    {
        static Commands()
        {
            Root.EnforceInHouseStandards();

            StartServer.Validators.Add(commandResult => {
                if(commandResult.GetValue(Options.NoHttp) && commandResult.GetValue(Options.NoHttps)) {
                    commandResult.AddError("The server must accept at least one of HTTP or HTTPS");
                }
            });

            StartServer.SetAction(async (parse, _) => {
                var command = new Command_Start {
                    Dev           = parse.GetRequiredValue(Options.Dev),
                    WorkingFolder = parse.GetRequiredValue(Options.Folder_Default),
                    HttpPort      = parse.GetRequiredValue(Options.HttpPort_Default_5001),
                    HttpsPort     = parse.GetRequiredValue(Options.HttpsPort_Default_6001),
                    NoBrowser     = parse.GetRequiredValue(Options.NoBrowser),
                    NoHttp        = parse.GetRequiredValue(Options.NoHttp),
                    NoHttps       = parse.GetRequiredValue(Options.NoHttps),
                    ShowLog       = parse.GetRequiredValue(Options.ShowLog),
                };
                Program.Worked = await command.RunAsync();
            });
        }

        public static Command StartServer = new("start", "Start the web server") {
            Options.Dev,
            Options.Folder_Default,
            Options.HttpPort_Default_5001,
            Options.HttpsPort_Default_6001,
            Options.NoBrowser,
            Options.NoHttp,
            Options.NoHttps,
            Options.ShowLog,
        };

        public static RootCommand Root = new("VRSCore server") {
            StartServer,
        };
    }
}
