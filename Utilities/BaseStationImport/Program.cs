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
using InterfaceFactory;
using VirtualRadar.Interface;

namespace BaseStationImport
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = 0;
            var verbose = (args ?? new string[0]).Any(r => String.Equals(r, "-verbose", StringComparison.OrdinalIgnoreCase));

            try {
                ProgramLifetime.Headless = true;
                ProgramLifetime.InitialiseUnhandledExceptionHandling();
                Factory.Singleton.Register<IApplicationInformation, ApplicationInformation>();

                VirtualRadar.SQLiteWrapper.Implementations.Register(Factory.Singleton);
                VirtualRadar.Headless.Implementations.Register(Factory.Singleton);
                VirtualRadar.Library.Implementations.Register(Factory.Singleton);
                VirtualRadar.Database.Implementations.Register(Factory.Singleton);

                ProgramLifetime.InitialiseManagers();
                LoadDatabasePlugins();

                var appInfo = Factory.Singleton.Resolve<IApplicationInformation>();
                Console.WriteLine($"{appInfo.ApplicationName}, version {appInfo.ShortVersion}, built {appInfo.BuildDate} UTC");
                Console.WriteLine(appInfo.Copyright);
                Console.WriteLine();

                CommandRunner commandRunner = null;
                var options = OptionsParser.Parse(args);
                switch(options.Command) {
                    case Command.ApplySchema:   commandRunner = new CommandRunner_ApplySchema(); break;
                    case Command.Import:        commandRunner = new CommandRunner_Import(); break;
                    default:                    OptionsParser.Usage("Missing command"); break;
                }
                commandRunner.Options = options;
                if(!commandRunner.Run()) {
                    exitCode = 1;
                }
            } catch(Exception ex) {
                if(!verbose) {
                    Console.WriteLine($"Caught exception: {ex.Message}");
                } else {
                    Console.WriteLine($"Caught exception: {ex.ToString()}");
                    Console.WriteLine();
                }

                try {
                    var log = Factory.Singleton.ResolveSingleton<ILog>();
                    log.WriteLine($"Caught exception in BaseStationImport: {ex.ToString()}");
                    Console.WriteLine("Full details have been recorded in the log");
                } catch(Exception iEx) {
                    Console.WriteLine($"The exception could not be logged: {iEx.Message}");
                }

                exitCode = 2;
            }

            Environment.Exit(exitCode);
        }

        private static void LoadDatabasePlugins()
        {
            var pluginManager = Factory.Singleton.ResolveSingleton<IPluginManager>();
            ProgramLifetime.LoadPlugins();

            foreach(var plugin in pluginManager.LoadedPlugins) {
                switch(plugin.Id) {
                    case "VirtualRadarServer.Plugin.SqlServer":
                        plugin.RegisterImplementations(Factory.Singleton);
                        break;
                }
            }
        }
    }
}
