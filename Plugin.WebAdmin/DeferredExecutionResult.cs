using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// The result sent in a <see cref="JsonResponse"/> for web methods that are to be executed
    /// after the response has been sent for the original request.
    /// </summary>
    public class DeferredExecutionResult
    {
        /// <summary>
        /// Gets a value indicating that execution of the method has been deferred.
        /// </summary>
        public bool DeferredExecution { get { return true; } }

        /// <summary>
        /// Gets or sets the job identifier to send back to the browser for a deferred
        /// execution method.
        /// </summary>
        public string JobId { get; set; }
    }
}
