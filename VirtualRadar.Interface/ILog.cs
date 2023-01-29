// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can manage log files for us.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Gets the full path and filename of the log file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Writes a line of text to the log.
        /// </summary>
        /// <param name="message"></param>
        void WriteLine(string message);

        /// <summary>
        /// Asynchronously writes a line of text to the log. Note that these may appear out of call order
        /// in the log.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task WriteLineAsync(string message);

        /// <summary>
        /// Immediately flushes the log to disk, blocking until it has been flushed.
        /// </summary>
        void Flush();

        /// <summary>
        /// Asynchronously flushes the log to disk.
        /// </summary>
        Task FlushAsync();

        /// <summary>
        /// Returns clones of the messages held by the log. Changing the clones will not affect the log.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<LogMessage> GetContent();
    }
}
