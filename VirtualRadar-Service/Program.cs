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
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if(!System.Environment.UserInteractive) {
                StartService();
            } else {
                try {
                    var options = CommandLineParser.Parse(args);
                    ProjectInstaller.Options = options;

                    switch(options.Command) {
                        case Command.Install:   InstallService(options); break;
                        case Command.Uninstall: UninstallService(options); break;
                        default:                CommandLineParser.Usage("Missing command"); break;
                    }
                } catch(Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Starts the installed service.
        /// </summary>
        static void StartService()
        {
            ServiceBase.Run(new ServiceBase[] {
                new Service(),
            });
        }

        /// <summary>
        /// Returns true if the service is installed.
        /// </summary>
        /// <returns></returns>
        static bool ServiceIsInstalled()
        {
            return ServiceController.GetServices().Any(r => (r.ServiceName ?? "")
                .Equals(ProjectInstaller.ServiceName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true if the web admin plugin is installed.
        /// </summary>
        /// <returns></returns>
        static bool WebAdminPluginIsInstalled()
        {
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginFullPath = Path.Combine(root, "Plugins", "WebAdmin", "VirtualRadar.Plugin.WebAdmin.dll");

            return File.Exists(pluginFullPath);
        }

        /// <summary>
        /// Installs the service.
        /// </summary>
        /// <param name="options"></param>
        static void InstallService(Options options)
        {
            Console.WriteLine("Installing service");

            if(ServiceIsInstalled()) {
                Console.WriteLine("Service is already installed");
            } else if(!options.SkipWebAdminPluginCheck && !WebAdminPluginIsInstalled()) {
                Console.WriteLine("The web admin plugin has not been installed");
            } else {
                if(!String.IsNullOrEmpty(options.UserName) && String.IsNullOrEmpty(options.Password)) {
                    options.Password = CommandLineParser.AskForPassword($"Password for {options.UserName}");
                }

                ManagedInstallerClass.InstallHelper(new string[] { ServiceFullPath() });
            }
        }

        /// <summary>
        /// Uninstalls the service.
        /// </summary>
        /// <param name="options"></param>
        static void UninstallService(Options options)
        {
            Console.WriteLine("Uninstalling service");
            if(!ServiceIsInstalled()) {
                Console.WriteLine("Service is not installed");
            } else {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", ServiceFullPath() });
            }
        }

        /// <summary>
        /// The full path and filename of the service executable.
        /// </summary>
        /// <returns></returns>
        static string ServiceFullPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }
    }
}
