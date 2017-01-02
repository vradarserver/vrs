using System;
using System.Collections.Generic;
using System.Configuration.Install;
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
            var projectInstaller = new ProjectInstaller();
            var serviceName = (projectInstaller.VrsServiceInstaller.ServiceName ?? "").ToLower();

            return ServiceController.GetServices().Any(r => (r.ServiceName ?? "").ToLower() == serviceName);
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
            } else {
                if(!String.IsNullOrEmpty(options.UserName) && String.IsNullOrEmpty(options.Password)) {
                    options.Password = CommandLineParser.AskForPassword($"Password for {options.UserName}");
                }

                var args = new List<string>();
                if(!String.IsNullOrEmpty(options.UserName) && !String.IsNullOrEmpty(options.Password)) {
                    args.Add($"/username={options.UserName}");
                    args.Add($"/password={options.Password}");
                }
                args.Add(ServiceFullPath());

                ManagedInstallerClass.InstallHelper(args.ToArray());
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
