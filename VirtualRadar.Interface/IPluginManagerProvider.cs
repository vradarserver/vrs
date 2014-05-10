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
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that abstract away the environment for <see cref="IPluginManager"/> implementations.
    /// </summary>
    public interface IPluginManagerProvider
    {
        /// <summary>
        /// Gets the folder that the program was started from.
        /// </summary>
        string ApplicationStartupPath { get; }

        /// <summary>
        /// Returns all of the files matching the search pattern in the folder passed across.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        IEnumerable<string> DirectoryGetFiles(string folder, string searchPattern);

        /// <summary>
        /// Returns all of the folders in the folder passed across.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        IEnumerable<string> DirectoryGetDirectories(string folder);

        /// <summary>
        /// Returns true if the folder exists.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool DirectoryExists(string folder);

        /// <summary>
        /// Returns a collection of every type contained within what *could* be a .NET DLL. Could throw
        /// any number of exceptions.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        IEnumerable<Type> LoadTypes(string fullPath);

        /// <summary>
        /// Takes a snapshot of the current state of the class factory.
        /// </summary>
        /// <returns></returns>
        IClassFactory ClassFactoryTakeSnapshot();

        /// <summary>
        /// Restores the global class factory back to the state it was when a snapshot was taken.
        /// </summary>
        /// <param name="snapshot"></param>
        void ClassFactoryRestoreSnapshot(IClassFactory snapshot);
    }
}
