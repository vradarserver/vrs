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
using VirtualRadar.Interface.Settings;
using System.IO;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="IPluginManifestStorage"/>.
    /// </summary>
    class PluginManifestStorage : IPluginManifestStorage
    {
        /// <summary>
        /// The default implementation of <see cref="IPluginManifestStorageProvider"/>.
        /// </summary>
        class DefaultProvider : IPluginManifestStorageProvider
        {
            public bool FileExists(string fileName)     { return File.Exists(fileName); }
            public string ReadAllText(string fileName)  { return File.ReadAllText(fileName); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPluginManifestStorageProvider Provider { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginManifestStorage()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public PluginManifest LoadForPlugin(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException("fileName");
            if(fileName == "") throw new InvalidOperationException("Plugin filename must be supplied");

            var extension = Path.GetExtension(fileName);
            if(!extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase)) throw new InvalidOperationException($"{fileName} is not a DLL");

            var manifestFileName = Path.ChangeExtension(fileName, ".xml");

            PluginManifest result = null;

            if(Provider.FileExists(manifestFileName)) {
                var fileContent = Provider.ReadAllText(manifestFileName);
                using(var textReader = new StringReader(fileContent)) {
                    var serialiser = Factory.Resolve<IXmlSerialiser>();
                    result = serialiser.Deserialise<PluginManifest>(textReader);
                }
            }

            return result;
        }
    }
}
