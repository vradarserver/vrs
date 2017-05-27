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

        /// <summary>
        /// The normal priority for redirecting requests.
        /// </summary>
        public static readonly int Redirection = Authentication + 100;

        /// <summary>
        /// The normal priority for callbacks that configure HttpConfiguration for Web API.
        /// </summary>
        public static readonly int WebApiConfiguration = WebApi - 100;

        /// <summary>
        /// The normal priority for Microsft Web API requests.
        /// </summary>
        public static readonly int WebApi = 0;

        /// <summary>
        /// The normal priority for file system requests.
        /// </summary>
        public static readonly int FileSystemServer = WebApi + 100;
    }
}
