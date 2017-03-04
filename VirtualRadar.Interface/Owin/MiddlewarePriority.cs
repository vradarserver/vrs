using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// An enumeration of standard priorities to use when adding callbacks to
    /// <see cref="IWebAppConfiguration"/>.
    /// </summary>
    public enum MiddlewarePriority
    {
        /// <summary>
        /// The default priority for middleware.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The priority to use for middleware that should appear early in the pipeline.
        /// </summary>
        Early = -100,

        /// <summary>
        /// The priority used by VRS for middleware that it wants to appear at the start of the pipeline.
        /// </summary>
        First = -1000,

        /// <summary>
        /// The priority to use for middleware that should appear late in the pipeline.
        /// </summary>
        Late = 100,

        /// <summary>
        /// The priority used by VRS for middleware that it wants to appear at the end of the pipeline.
        /// </summary>
        Last = 1000,
    }
}
