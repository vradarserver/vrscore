using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Console.WriteLine($"  version               Show version information");

            if (!String.IsNullOrEmpty(message)) {
                Console.WriteLine();
                Console.WriteLine(message);
            }

            Environment.Exit(1);
        }
    }
}
