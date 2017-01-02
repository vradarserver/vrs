using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that handle signals.
    /// </summary>
    public interface IShutdownSignalHandler : ISingleton<IShutdownSignalHandler>
    {
        /// <summary>
        /// Ensures that the main view's CloseView method is called when a shutdown signal is raised by the OS.
        /// </summary>
        /// <remarks>
        /// Under Mono this adds a signal handler for SIGINT that calls CloseView when raised. Under .NET it
        /// hooks the console's Ctrl+C event to achieve the same result.
        /// </remarks>
        void CloseMainViewOnShutdownSignal();

        /// <summary>
        /// Shuts down any background threads that might be running, unhooks events etc.
        /// </summary>
        void Cleanup();
    }
}
