using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// The installer class that ManagedInstallerClass will call.
    /// </summary>
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        /// <summary>
        /// The name of the Virtual Radar Server service.
        /// </summary>
        internal static readonly string ServiceName = "VirtualRadarServerService";

        /// <summary>
        /// Gets or sets the options that control how the service is installed.
        /// </summary>
        internal static Options Options { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller() {
                Username = Options.UserName,
                Password = Options.Password,
            };

            var serviceInstaller = new ServiceInstaller() {
                Description =       "A web server that shows the positions of aircraft on a map",
                DisplayName =       "Virtual Radar Server",
                ServiceName =       ServiceName,
                StartType =         Options.StartupType == StartupType.Manual ? ServiceStartMode.Manual : ServiceStartMode.Automatic,
                DelayedAutoStart =  Options.StartupType == StartupType.DelayedAutomatic,
            };

            Installers.AddRange(new Installer[] {
                serviceProcessInstaller,
                serviceInstaller,
            });
        }
    }
}
