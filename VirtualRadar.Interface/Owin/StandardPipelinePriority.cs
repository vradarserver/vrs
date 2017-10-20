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
        /***********************************************************************************************
         * Middleware that has to run before *ANYTHING* else
         **********************************************************************************************/

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
        /// The normal priority for handling CORS requests.
        /// </summary>
        public static readonly int Cors = Redirection + 100;

        /// <summary>
        /// The normal priority for wrapping the response stream so that it can be manipulated after the pipeline has finished.
        /// </summary>
        public static readonly int ResponseStreamWrapper = Cors + 100;

        /***********************************************************************************************
         * 3rd party frameworks that short-circuit pipeline processing if they handle a request
         **********************************************************************************************/

        /// <summary>
        /// The normal priority for callbacks that configure HttpConfiguration for Web API.
        /// </summary>
        public static readonly int WebApiConfiguration = WebApi - 100;

        /// <summary>
        /// The normal priority for Microsft Web API requests.
        /// </summary>
        public static readonly int WebApi = 0;

        /***********************************************************************************************
         * VRS middleware that runs only if 3rd party frameworks have not yet handled the request
         **********************************************************************************************/

        /// <summary>
        /// The normal priority for JavaScript bundle requests.
        /// </summary>
        public static readonly int BundlerServer = WebApi + 100;

        /// <summary>
        /// The normal priority for file system requests.
        /// </summary>
        public static readonly int FileSystemServer = BundlerServer + 100;

        /// <summary>
        /// The normal priority for image requests.
        /// </summary>
        public static readonly int ImageServer = FileSystemServer + 100;

        /// <summary>
        /// The normal priority for audio requests.
        /// </summary>
        public static readonly int AudioServer = ImageServer + 100;
    }
}
