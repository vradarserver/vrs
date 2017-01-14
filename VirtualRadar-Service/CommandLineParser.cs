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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// Parses command-line options.
    /// </summary>
    static class CommandLineParser
    {
        /// <summary>
        /// Parses the command-line arguments into an <see cref="Options"/> object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Options Parse(string[] args)
        {
            var result = new Options();

            args = args ?? new string[] {};
            for(var i = 0;i < args.Length;++i) {
                var arg = args[i];
                var normalisedArg = (arg ?? "").ToLower().Trim();
                var nextArg = i + 1 < args.Length ? args[i + 1] : null;

                switch(normalisedArg) {
                    case "-?":
                    case "/?":
                    case "--help":
                        Usage(null);
                        break;
                    case "-install":
                        result.Command = Command.Install;
                        break;
                    case "-uninstall":
                        result.Command = Command.Uninstall;
                        break;
                    case "-user":
                        result.UserName = UseNextArg("user name", nextArg, ref i);
                        break;
                    case "-password":
                        result.Password = UseNextArg("password", nextArg, ref i);
                        break;
                    case "-startup":
                        result.StartupType = ParseEnum<StartupType>(UseNextArg("startup type", nextArg, ref i));
                        break;
                    case "-nowebadmin":
                        result.SkipWebAdminPluginCheck = true;
                        break;
                    default:
                        Usage($"Unrecognised argument {arg}");
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the next argument if one is present.
        /// </summary>
        /// <param name="nextArgMeaning"></param>
        /// <param name="nextArg"></param>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        private static string UseNextArg(string nextArgMeaning, string nextArg, ref int argIndex)
        {
            if(nextArg == null) {
                Usage($"Missing {nextArgMeaning}");
            }

            argIndex++;
            return nextArg;
        }

        /// <summary>
        /// Parses an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static T ParseEnum<T>(string arg)
        {
            if(String.IsNullOrEmpty(arg)) {
                Usage($"Missing {typeof(T).Name} value");
            }

            T result = default(T);
            try {
                result = (T)Enum.Parse(typeof(T), arg, ignoreCase: true);
            } catch {
                Usage($"{arg} is not a valid {typeof(T).Name} value");
            }

            return result;
        }

        /// <summary>
        /// Displays the command-line arguments usage message and then exits the program.
        /// </summary>
        /// <param name="message"></param>
        public static void Usage(string message)
        {
            var defaults = new Options();

            //                  123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789
            Console.WriteLine($"usage: VirtualRadar-Service <command>");
            Console.WriteLine($"  -install         Installs the service");
            Console.WriteLine($"  -uninstall       Uninstalls the service");
            Console.WriteLine();
            Console.WriteLine($"Install options:");
            Console.WriteLine($"  -user name       The account that will run the service [{defaults.UserName}]");
            Console.WriteLine($"  -password value  The password for the account [will prompt if not supplied]");
            Console.WriteLine($"  -startup value   How to start the service [{defaults.StartupType}]");
            Console.WriteLine( "                     {0}", String.Join(" ", Enum.GetNames(typeof(StartupType))));
            Console.WriteLine($"  -noWebAdmin      Skip the check for the web admin plugin");
            Console.WriteLine();
            Console.WriteLine($"Service options:");
            Console.WriteLine($"  -debugOnStart    Trigger a debug interrupt when starting the service");
            Console.WriteLine($"  <standard VRS options>");

            var hasError = !String.IsNullOrEmpty(message);
            if(hasError) {
                Console.WriteLine();
                Console.WriteLine(message);
            }

            Environment.Exit(hasError ? 1 : 0);
        }

        /// <summary>
        /// Prompts the user for a password.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public static string AskForPassword(string prompt)
        {
            Console.Write($"{prompt}: ");

            var result = new StringBuilder();
            for(var finished = false;!finished;) {
                var key = Console.ReadKey(intercept: true);
                switch(key.Key) {
                    case ConsoleKey.Enter:
                        finished = true;
                        break;
                    case ConsoleKey.Backspace:
                        if(result.Length > 0) {
                            Console.Write("\b \b");
                            result.Remove(result.Length - 1, 1);
                        }
                        break;
                    default:
                        if(key.KeyChar > 0) {
                            result.Append(key.KeyChar);
                            Console.Write("*");
                        }
                        break;
                }
            }

            return result.ToString();
        }
    }
}
