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
        /// The lowest priority that any VRS middleware uses.
        /// </summary>
        public static readonly int LowestVrsMiddlewarePriority = -1000000;

        /// <summary>
        /// The normal priority for exception handling.
        /// </summary>
        public static readonly int Exception = LowestVrsMiddlewarePriority;

        /// <summary>
        /// The normal priority for access checking.
        /// </summary>
        public static readonly int Access = Exception + 1000;

        /// <summary>
        /// The normal priority for authentication.
        /// </summary>
        public static readonly int Authentication = Access + 1000;

        /// <summary>
        /// The normal priority for redirecting requests.
        /// </summary>
        public static readonly int Redirection = Authentication + 1000;

        /// <summary>
        /// The normal priority for handling CORS requests.
        /// </summary>
        public static readonly int Cors = Redirection + 1000;

        /// <summary>
        /// The normal priority for wrapping the response stream so that it can be manipulated after the pipeline has finished.
        /// </summary>
        public static readonly int ResponseStreamWrapper = Cors + 1000;

        /// <summary>
        /// The normal priority for the shim middleware that fires the IWebServer events.
        /// </summary>
        public static readonly int ShimServerPriority = ResponseStreamWrapper + 1000;

        /***********************************************************************************************
         * 3rd party frameworks that short-circuit pipeline processing if they handle a request
         **********************************************************************************************/

        /// <summary>
        /// The normal priority for callbacks that configure HttpConfiguration for Web API.
        /// </summary>
        public static readonly int WebApiConfiguration = WebApi - 1000;

        /// <summary>
        /// The normal priority for Microsft Web API requests.
        /// </summary>
        public static readonly int WebApi = 0;

        /***********************************************************************************************
         * VRS middleware that runs only if 3rd party frameworks have not yet handled the request
         **********************************************************************************************/

        /// <summary>
        /// The lowest priority for VRS content middleware (image content, file content etc.)
        /// </summary>
        public static readonly int LowestVrsContentMiddlewarePriority = WebApi + 1000000;

        /// <summary>
        /// The normal priority for JavaScript bundle requests.
        /// </summary>
        public static readonly int BundlerServer = LowestVrsContentMiddlewarePriority;

        /// <summary>
        /// The normal priority for file system requests.
        /// </summary>
        public static readonly int FileSystemServer = BundlerServer + 1000000;

        /// <summary>
        /// The normal priority for image requests.
        /// </summary>
        public static readonly int ImageServer = FileSystemServer + 1000000;

        /// <summary>
        /// The normal priority for audio requests.
        /// </summary>
        public static readonly int AudioServer = ImageServer + 1000000;

        /// <summary>
        /// The highest priority used by VRS content middleware.
        /// </summary>
        public static readonly int HighestVrsContentMiddlewarePriority = AudioServer;
    }
}
