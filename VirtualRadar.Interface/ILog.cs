// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can manage log files for us.
    /// </summary>
    /// <remarks>
    /// You should only use the Singleton version of this object to write to the program log. Using your own
    /// instance will work but it will not be thread-safe whereas the Singleton log is guaranteed to be thread-safe.
    /// </remarks>
    /// <example>
    /// <code>
    /// ILog log = Factory.Singleton.Resolve&lt;ILog&gt;().Singleton;
    /// log.WriteLine("This will be written to the program log");
    /// </code>
    /// </example>
    public interface ILog : ISingleton<ILog>
    {
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for the tests.
        /// </summary>
        ILogProvider Provider { get; set; }

        /// <summary>
        /// Gets the full path and filename of the log file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Writes a line of text to the log file.
        /// </summary>
        /// <param name="message"></param>
        void WriteLine(string message);

        /// <summary>
        /// Writes a line of text to the log file.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void WriteLine(string format, params object[] args);

        /// <summary>
        /// Truncates the log file to the last nn kilobytes.
        /// </summary>
        /// <param name="kbLength">The number of kilobytes to preserve at the end of the file.</param>
        void Truncate(int kbLength);

        /// <summary>
        /// Returns the content of the log as an array of lines, optionally truncated to the last so-many lines.
        /// </summary>
        /// <param name="lines">The number of tail lines to return or 0 to return all of them.</param>
        /// <returns></returns>
        string[] GetContent(int lines);
    }
}
