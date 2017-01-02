using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// An enumeration of the install / uninstall commands.
    /// </summary>
    enum Command
    {
        /// <summary>
        /// No command specified.
        /// </summary>
        None,

        /// <summary>
        /// The service is to be installed.
        /// </summary>
        Install,

        /// <summary>
        /// The service is to be uninstalled.
        /// </summary>
        Uninstall,
    }
}
