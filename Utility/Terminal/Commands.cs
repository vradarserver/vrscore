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
using Microsoft.Extensions.DependencyInjection;
using VirtualRadar.CommandLine;

namespace VirtualRadar.Utility.Terminal
{
    static class Commands
    {
        static Commands()
        {
            Root.EnforceInHouseStandards();

            ShowTerminal.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_ShowTerminal>();
                command.Address = parse.GetRequiredValue(Options.Address_Default_127_0_0_1);
                command.Port = parse.GetRequiredValue(Options.Port_Default_30003);
                command.RecordingFileInfo = parse.GetValue(Options.RecordingFileInfo);
                command.PlaybackSpeed = parse.GetRequiredValue(Options.PlaybackSpeed_Default_1);
                command.ReceiverName = parse.GetValue(Options.ReceiverName);
                Program.Worked = await command.RunAsync();
            });
        }

        public static Command ShowTerminal = new("terminal", "Show the terminal UI") {
            Options.Address_Default_127_0_0_1,
            Options.Port_Default_30003,
            Options.ReceiverName,
            Options.RecordingFileInfo,
            Options.PlaybackSpeed_Default_1,
        };

        public static RootCommand Root = new("VRSCore terminal") {
            ShowTerminal,
        };
    }
}
