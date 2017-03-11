using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// A static class enumeration of all of the priorities for standard pipeline middleware.
    /// </summary>
    public static class StandardPipelinePriority
    {
        /// <summary>
        /// The normal priority for access checking.
        /// </summary>
        public static readonly int Access = -1000000;

        /// <summary>
        /// The normal priority for authentication.
        /// </summary>
        public static readonly int Authentication = Access + 100;
    }
}
