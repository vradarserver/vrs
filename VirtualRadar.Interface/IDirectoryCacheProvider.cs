﻿// Copyright © 2012 onwards, Andrew Whewell
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
using System.IO;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for the object that abstracts away the environment for <see cref="IDirectoryCache"/> implementations.
    /// </summary>
    public interface IDirectoryCacheProvider
    {
        /// <summary>
        /// Returns true if the folder passed across exists.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool FolderExists(string folder);

        /// <summary>
        /// Returns a collection of objects describing files in a folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<IDirectoryCacheProviderFileInfo> GetFilesInFolder(string folder);

        /// <summary>
        /// Returns a collection of fully pathed sub-folders within the folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<string> GetSubFoldersInFolder(string folder);

        /// <summary>
        /// Returns the file information about the file passed across or null if the file does not exist.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IDirectoryCacheProviderFileInfo GetFileInfo(string fileName);
    }

    /// <summary>
    /// The interface for an object that exposes information about a file.
    /// </summary>
    public interface IDirectoryCacheProviderFileInfo
    {
        /// <summary>
        /// Gets the name of the file without the path.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the last time the file was modified.
        /// </summary>
        DateTime LastWriteTimeUtc { get; set; }
    }
}
