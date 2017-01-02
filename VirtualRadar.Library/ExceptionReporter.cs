using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of the exception reporter.
    /// </summary>
    class ExceptionReporter : IExceptionReporter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        public void ShowUnhandledException(Exception ex)
        {
            ProgramLifetime.ShowException(ex);
        }
    }
}
