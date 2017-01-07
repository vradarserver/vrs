using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// An enumeration of the different startup types for installs.
    /// </summary>
    public enum StartupType
    {
        /// <summary>
        /// Service must be manually started.
        /// </summary>
        Manual,

        /// <summary>
        /// Service is started during Windows bootup.
        /// </summary>
        Automatic,

        /// <summary>
        /// Service is started two minutes (by default) after Windows starts.
        /// </summary>
        DelayedAutomatic,
    }
}
