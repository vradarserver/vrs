// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace BaseStationImport
{
    static class OptionsParser
    {
        /// <summary>
        /// Parses the command-line options passed across.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Options Parse(string[] args)
        {
            var result = new Options();

            if(args.Length == 0) {
                Usage(null);
            }

            for(var i = 0;i < args.Length;++i) {
                var arg = (args[i] ?? "");
                var normalisedArg = arg.ToLower();
                var nextArg = GetNextArg(args, i);

                switch(normalisedArg) {
                    case "-dst":
                        result.Target.Engine = ParseEnum<DatabaseEngine>(nextArg, ref i);
                        nextArg = GetNextArg(args, i);
                        result.Target.ConnectionString = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-from":
                        result.EarliestFlight = ParseDate(arg, nextArg, ref i);
                        break;
                    case "-import":
                        result.Command = Command.Import;
                        break;
                    case "-noaircraft":
                        result.ImportAircraft = false;
                        break;
                    case "-noflights":
                        result.ImportFlights = false;
                        break;
                    case "-nolocations":
                        result.ImportLocations = false;
                        break;
                    case "-noschema":
                        result.SuppressSchemaUpdate = true;
                        break;
                    case "-nosessions":
                        result.ImportSessions = false;
                        break;
                    case "-src":
                        result.Source.Engine = ParseEnum<DatabaseEngine>(nextArg, ref i);
                        nextArg = GetNextArg(args, i);
                        result.Source.ConnectionString = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-to":
                        result.LatestFlight = ParseDate(arg, nextArg, ref i);
                        break;
                    case "-verbose":
                        // This is handled by Main(), we just want to ignore it here
                        break;
                    default:
                        Usage($"Invalid argument {arg}");
                        break;
                }
            }

            return result;
        }

        private static string GetNextArg(string[] args, int i)
        {
            return i + 1 < args.Length ? args[i + 1] : null;
        }

        private static DateTime ParseDate(string arg, string nextArg, ref int argIndex)
        {
            if(!DateTime.TryParseExact(nextArg, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)) {
                Usage($"Could not parse {arg} date {nextArg}");
            }
            ++argIndex;

            return result;
        }

        private static string UseNextArg(string arg, string nextArg, ref int argIndex)
        {
            if(String.IsNullOrWhiteSpace(nextArg)) {
                Usage($"{arg} argument missing");
            }
            ++argIndex;

            return nextArg;
        }

        private static T ParseEnum<T>(string arg, ref int argIndex)
        {
            if(arg == null) {
                Usage($"{typeof(T).Name} value missing");
            }
            ++argIndex;

            try {
                return (T)Enum.Parse(typeof(T), arg ?? "", ignoreCase: true);
            } catch(ArgumentException) {
                Usage($"{arg} is not a recognised {typeof(T).Name} value");
                throw;
            }
        }

        private static int ParseInt(string arg, ref int argIndex)
        {
            if(arg == null) {
                Usage("Numeric parameter missing");
            }
            ++argIndex;

            if(!int.TryParse(arg, out int result)) {
                Usage($"{arg} is not an integer");
            }

            return result;
        }

        public static void Usage(string message)
        {
            var defaults = new Options();
            var databaseEngines = ListEnum<DatabaseEngine>(new DatabaseEngine[] { DatabaseEngine.None });

            //                  123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789
            Console.WriteLine($"usage: BaseStationImport <command> [options]");
            Console.WriteLine($"  -import                  Copy BaseStation data from one database to another");
            Console.WriteLine();
            Console.WriteLine($"Parameter types:");
            Console.WriteLine($"  <date>                   A date in yyyy-MM-dd ISO format");
            Console.WriteLine($"  <dbtype>                 {databaseEngines}");
            Console.WriteLine($"  <file|con>               A filename (SQLite only) or connection string");
            Console.WriteLine();
            Console.WriteLine($"-import options:");
            Console.WriteLine($"  -src <dbtype> <file|con> The source database and how to connect to it");
            Console.WriteLine($"  -dst <dbtype> <file|con> The target database and how to connect to it");
            Console.WriteLine($"  -noSchema                Do not create or update schema on target before import");
            Console.WriteLine($"  -noAircraft              Do not import aircraft");
            Console.WriteLine($"  -noLocations             Do not import locations");
            Console.WriteLine($"  -noSessions              Do not import sessions");
            Console.WriteLine($"  -noFlights               Do not import flights");
            Console.WriteLine($"    -from <date>           Earliest flight to import from [{Describe.IsoDate(defaults.EarliestFlight)}]");
            Console.WriteLine($"    -to <date>             Latest flight to import to [{Describe.IsoDate(defaults.LatestFlight)}]");
            Console.WriteLine();
            Console.WriteLine($"Common options:");
            Console.WriteLine($"  -verbose                 Show more information in error messages");

            if(!String.IsNullOrEmpty(message)) {
                Console.WriteLine();
                Console.WriteLine(message);
            }

            Environment.Exit(1);
        }

        private static string ListEnum<T>(T[] exclude = null)
            where T: struct
        {
            return String.Join(" | ",
                    Enum.GetNames(typeof(T))
                   .OfType<string>()
                   .OrderBy(r => r.ToLower())
                   .Where(r => exclude == null || !exclude.Contains((T)Enum.Parse(typeof(T), r)))
            );
        }
    }
}
