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
    /// The interface for objects that abstract away the environment for <see cref="ILog"/> implementations.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Gets the ID number of the current thread.
        /// </summary>
        int CurrentThreadId { get; }

        /// <summary>
        /// Returns true if the file described by the fullPath exists.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        bool FileExists(string fullPath);

        /// <summary>
        /// Returns true if the folder exists.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool FolderExists(string folder);

        /// <summary>
        /// Creates the folder if it doesn't exist.
        /// </summary>
        /// <param name="folder"></param>
        void CreateFolder(string folder);

        /// <summary>
        /// Appends the text to the file passed across. If the file does not exist then it is created.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="text"></param>
        void AppendAllText(string fullPath, string text);

        /// <summary>
        /// Truncates the file to the last so-many bytes in the original.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="bytes"></param>
        void TruncateTo(string fullPath, int bytes);
    }
}
