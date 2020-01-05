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
    /// The interface for the object that manages plugins on behalf of the program.
    /// </summary>
    [Singleton]
    public interface IPluginManager
    {
        /// <summary>
        /// Gets or sets the object that abstracts away the environment for testing.
        /// </summary>
        IPluginManagerProvider Provider { get; set; }

        /// <summary>
        /// Gets a list of every plugin that's been loaded into VRS.
        /// </summary>
        IList<IPlugin> LoadedPlugins { get; }

        /// <summary>
        /// Gets a map of the reason why a plugin was not loaded indexed by the full path and filename of the plugin DLL.
        /// </summary>
        IDictionary<string, string> IgnoredPlugins { get; }

        /// <summary>
        /// Loads the DLLs in the Plugins folder.
        /// </summary>
        void LoadPlugins();

        /// <summary>
        /// Calls the <see cref="IPlugin.RegisterImplementations"/> methods for all loaded plugins.
        /// </summary>
        void RegisterImplementations();

        /// <summary>
        /// Calls the <see cref="IPlugin_V2.RegisterOwinMiddleware"/> methods for all loaded plugins that
        /// implement <see cref="IPlugin_V2"/>.
        /// </summary>
        void RegisterOwinMiddleware();
    }
}
