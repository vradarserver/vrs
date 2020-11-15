// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Wraps the .NET file system watcher class so that file watchers can be used in testable classes.
    /// </summary>
    public interface IFileSystemWatcher : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the watcher is active or not.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the path of the directory to watch or the full path of the file to watch.
        /// </summary>
        /// <remarks>
        /// Wildcards are acceptable. This property may be case sensitive on mono.
        /// </remarks>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the filter string used to determine which files are monitored in a directory.
        /// </summary>
        /// <remarks>
        /// This property may be case sensitive when running under mono.
        /// </remarks>
        string Filter { get; set; }

        /// <summary>
        /// Gets or sets the flags that determine what kinds of file changes should trigger an event.
        /// </summary>
        NotifyFilters NotifyFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether changes in subdirectories should be monitored.
        /// </summary>
        bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Gets or sets the size of the notifications buffer.
        /// </summary>
        /// <remarks>
        /// This should be a multiple of 4KB. It cannot be less than 4KB and cannot exceed 64KB. Note that
        /// the buffer is held in non-paged memory.
        /// </remarks>
        int InternalBufferSize { get; set; }

        /// <summary>
        /// Raised when a file or directory is modified.
        /// </summary>
        event FileSystemEventHandler Changed;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is created.
        /// </summary>
        event FileSystemEventHandler Created;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is deleted.
        /// </summary>
        event FileSystemEventHandler Deleted;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is renamed.
        /// </summary>
        event RenamedEventHandler Renamed;

        /// <summary>
        /// Raised when the connection to the <see cref="Path"/> being watched is lost or the number of
        /// changes overwhelms the notifications buffer.
        /// </summary>
        event ErrorEventHandler Error;
    }
}
