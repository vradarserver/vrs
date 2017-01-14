using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// Configuration options when installing or uninstalling the service.
    /// </summary>
    class Options
    {
        /// <summary>
        /// Gets or sets the mandatory command.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// Gets or sets the optional user-name. If this is set to an empty string then .NET will ask for the
        /// user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the optional password. If this is empty/null and <see cref="UserName"/> has been
        /// supplied then the program will ask for the password at run-time.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how the service should start up.
        /// </summary>
        public StartupType StartupType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the installer should not check to see whether the web admin plugin has been installed.
        /// </summary>
        public bool SkipWebAdminPluginCheck { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Options()
        {
            UserName = $"{Environment.UserDomainName}\\{Environment.UserName}";
        }
    }
}
