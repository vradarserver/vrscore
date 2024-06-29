// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Utility.CLIConsole
{
    static class OptionsParser
    {
        public static Options Parse(string[] args)
        {
            var result = new Options();
            for(var i = 0;i < args.Length;++i) {
                var arg = args[i];
                var nextArg = i + 1 < args.Length ? args[i + 1] : null;
                var normalisedArg = (arg ?? "").ToLower();

                switch(normalisedArg) {
                    case "-?":
                    case "/?":
                        Usage();
                        break;
                    case "-address":
                        result.Address = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "connect":
                        result.Command = ParseCommand(result, Command.ConnectListener);
                        break;
                    case "dumpfeed":
                        result.Command = ParseCommand(result, Command.DumpFeed);
                        result.LoadFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-feedformat":
                        result.FeedFormat = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-id":
                        result.Id = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "list":
                        result.Command = ParseCommand(result, Command.List);
                        result.ListEntity = ParseEnum<ListEntity>(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "lookup":
                        result.Command = ParseCommand(result, Command.Lookup);
                        result.LookupEntity = ParseEnum<LookupEntity>(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "open":
                        result.Command = ParseCommand(result, Command.Open);
                        result.OpenEntity = ParseEnum<OpenEntity>(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-parsemessage":
                        result.ParseMessage = true;
                        break;
                    case "-port":
                        result.Port = ParseInteger(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "record":
                        result.Command = ParseCommand(result, Command.RecordFeed);
                        break;
                    case "-save":
                        result.SaveFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-show":
                        result.Show = true;
                        break;
                    case "updatesdm":
                        result.Command = ParseCommand(result, Command.UpdateStandingData);
                        break;
                    case "version":
                        result.Command = ParseCommand(result, Command.ShowVersion);
                        break;
                    default:
                        Usage($"Unrecognised parameter {arg}");
                        break;
                }
            }

            return result;
        }

        private static string UseNextArg(string arg, string nextArg, ref int i)
        {
            if(String.IsNullOrEmpty(nextArg)) {
                Usage($"Missing {arg} argument");
            }
            ++i;

            return nextArg;
        }

        private static Command ParseCommand(Options options, Command command)
        {
            if(options.Command != Command.None) {
                Usage("You can only specify one command at a time");
            }

            return command;
        }

        private static T ParseEnum<T>(string arg)
        {
            if(String.IsNullOrEmpty(arg)) {
                Usage($"Missing {typeof(T).Name} value");
            }

            try {
                return (T)Enum.Parse(typeof(T), arg, ignoreCase: true);
            } catch {
                Usage($"{arg} is not a valid {typeof(T).Name} value");
                throw;
            }
        }

        private static decimal ParseDecimal(string arg)
        {
            if(!decimal.TryParse(arg, out var result)) {
                Usage($"{arg} is not a floating point number");
            }
            return result;
        }

        private static int ParseInteger(string arg)
        {
            if(!int.TryParse(arg, out var result)) {
                Usage($"{arg} is not an integer");
            }

            return result;
        }

        private static long ParseLong(string arg)
        {
            if(!long.TryParse(arg, out var result)) {
                Usage($"{arg} is not a long");
            }

            return result;
        }

        public static void Usage(string message = null)
        {
            var defaults = new Options();

                             // 123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789
            Console.WriteLine($"Console command [options]");
            Console.WriteLine($"  version             Show version information");
            Console.WriteLine($"  list <entity>       List stuff: {String.Join(", ", Enum.GetNames<ListEntity>())}");
            Console.WriteLine($"  lookup <entity>     Look something up: {String.Join(", ", Enum.GetNames<LookupEntity>())}");
            Console.WriteLine($"  open <entity>       Open something: {String.Join(", ", Enum.GetNames<OpenEntity>())}");
            Console.WriteLine($"  connect             Connect to feed");
            Console.WriteLine($"  record              Record a feed for future playback");
            Console.WriteLine($"  dumpFeed <filename> Load and dump a recorded feed");
            Console.WriteLine($"  updateSDM           Update standing data");
            Console.WriteLine();
            Console.WriteLine($"CONNECT OPTIONS");
            Console.WriteLine($"  -address <address>  Address to connect to [{defaults.Address}]");
            Console.WriteLine($"  -port    <port>     Port to connect to [{defaults.Port}]");
            Console.WriteLine($"  -show               Show feed content [{defaults.Show}]");
            Console.WriteLine($"  -save    <filename> Save feed content [{defaults.SaveFileName}]");
            Console.WriteLine();
            Console.WriteLine($"LOOKUP OPTIONS");
            Console.WriteLine($"  -id <string>        Hyphen-separated list of things to lookup [{defaults.Id}]");
            Console.WriteLine();
            Console.WriteLine($"RECORD FEED OPTIONS");
            Console.WriteLine($"  -address <address>  Address to connect to [{defaults.Address}]");
            Console.WriteLine($"  -port    <port>     Port to connect to [{defaults.Port}]");
            Console.WriteLine($"  -save    <filename> Filename to save to [{defaults.SaveFileName}]");
            Console.WriteLine();
            Console.WriteLine($"DUMP FEED OPTIONS");
            Console.WriteLine($"  -show               Dump packet content [{defaults.Show}]");
            Console.WriteLine($"  -parseMessage       Parse and display messages from feed [{defaults.ParseMessage}]");
            Console.WriteLine($"  -feedFormat <name>  Feed format to use for parsing packet [{defaults.FeedFormat}]");


            if (!String.IsNullOrEmpty(message)) {
                Console.WriteLine();
                Console.WriteLine(message);
            }

            Environment.Exit(1);
        }
    }
}
