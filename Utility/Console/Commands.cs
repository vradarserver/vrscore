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

namespace VirtualRadar.Utility.CLIConsole
{
    static class Commands
    {
        static Commands()
        {
            Root.EnforceInHouseStandards();

            Connect.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Connect>();
                command.Address = parse.GetRequiredValue(Options.IPAddress_Default_127_0_0_1);
                command.Port = parse.GetRequiredValue(Options.Port_Default_30003);
                command.ShowContent = parse.GetRequiredValue(Options.Show_Default_False);
                command.SaveFileInfo = parse.GetValue(Options.SaveFileInfo);
                Program.Worked = await command.RunAsync();
            });

            DumpFeed.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_DumpFeed>();
                command.LoadFileInfo = parse.GetRequiredValue(Options.LoadFileInfoArgument);
                command.Show = parse.GetRequiredValue(Options.Show_Default_False);
                command.ParseMessage = parse.GetRequiredValue(Options.ParseMessage_Default_False);
                command.FeedFormat = parse.GetRequiredValue(Options.FeedFormat_Default_VrsBaseStation);
                Program.Worked = await command.RunAsync();
            });

            List.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_List>();
                command.Entity = parse.GetRequiredValue(Options.ListEntityArgument);
                Program.Worked = await command.RunAsync();
            });

            Lookup.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Lookup>();
                command.Entity = parse.GetRequiredValue(Options.LookupEntityArgument);
                command.Ids = parse.GetRequiredValue(Options.Icao24List_Required);
                Program.Worked = await command.RunAsync();
            });

            Log.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Log>();
                Program.Worked = await command.RunAsync();
            });

            Open.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Open>();
                command.Entity = parse.GetRequiredValue(Options.OpenEntityArgument);
                Program.Worked = await command.RunAsync();
            });

            Record.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Record>();
                command.Address = parse.GetRequiredValue(Options.IPAddress_Default_127_0_0_1);
                command.Port = parse.GetRequiredValue(Options.Port_Default_30003);
                command.SaveFileInfo = parse.GetRequiredValue(Options.SaveFileInfo_Required);
                Program.Worked = await command.RunAsync();
            });

            Settings.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Settings>();
                command.Update = parse.GetRequiredValue(Options.Update_Default_False);
                Program.Worked = await command.RunAsync();
            });

            ShowModules.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_ShowModules>();
                Program.Worked = await command.RunAsync();
            });

            StandingDataListCommand.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_StandingData_List>();
                command.Entity = parse.GetRequiredValue(Options.StandingDataEntityArgument);
                command.Code = parse.GetRequiredValue(Options.Code_Required);
                Program.Worked = await command.RunAsync();
            });

            StandingDataUpdateCommand.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_StandingData_Update>();
                Program.Worked = await command.RunAsync();
            });

            Version.SetAction(async (parse, _) => {
                var command = Program.Scope.GetRequiredService<Command_Version>();
                Program.Worked = await command.RunAsync();
            });
        }

        public static Command Connect = new("connect", "Connect to feed") {
            Options.IPAddress_Default_127_0_0_1,
            Options.Port_Default_30003,
            Options.Show_Default_False,
            Options.SaveFileInfo,
        };

        public static Command DumpFeed = new("dumpfeed", "Dump a recorded feed") {
            Options.LoadFileInfoArgument,
            Options.Show_Default_False,
            Options.ParseMessage_Default_False,
            Options.FeedFormat_Default_VrsBaseStation,
        };

        public static Command List = new("list", "List entities") {
            Options.ListEntityArgument,
        };

        public static Command Lookup = new("lookup", "Look something up") {
            Options.LookupEntityArgument,
            Options.Icao24List_Required,
        };

        public static Command Log = new("log", "Show the log");

        public static Command Open = new("open", "Open something") {
            Options.OpenEntityArgument,
        };

        public static Command Record = new("record", "Record a feed for future playback") {
            Options.IPAddress_Default_127_0_0_1,
            Options.Port_Default_30003,
            Options.SaveFileInfo_Required,
        };

        public static Command Settings = new("settings", "Display or update the settings file") {
            Options.Update_Default_False,
        };

        public static Command ShowModules = new("showmodules", "Show loaded module information");

        public static Command StandingDataListCommand = new("list", "Dump a standing-data record") {
            Options.StandingDataEntityArgument,
            Options.Code_Required,
        };

        public static Command StandingDataUpdateCommand = new("update", "Download a refresh of the standing data if it is stale");

        public static Command StandingData = new("standingdata", "Download and dump standing data") {
            StandingDataListCommand,
            StandingDataUpdateCommand,
        };

        public static Command Version = new("version", "Show version information");

        public static RootCommand Root = new("VRSCore console") {
            Commands.Version,
            Commands.Log,
            Commands.List,
            Commands.Lookup,
            Commands.Open,
            Commands.Connect,
            Commands.Record,
            Commands.DumpFeed,
            Commands.ShowModules,
            Commands.StandingData,
            Commands.Settings,
        };
    }
}
