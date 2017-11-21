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
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using System.IO;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="IInstallerSettingsStorage"/>.
    /// </summary>
    class InstallerSettingsStorage : IInstallerSettingsStorage
    {
        /// <summary>
        /// The default implementation of <see cref="IInstallerSettingsStorageProvider"/>.
        /// </summary>
        class DefaultProvider : IInstallerSettingsStorageProvider
        {
            public string Folder { get { return Factory.Singleton.ResolveSingleton<IConfigurationStorage>().Folder; } }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IInstallerSettingsStorageProvider Provider { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public InstallerSettingsStorage()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public InstallerSettings Load()
        {
            InstallerSettings result = new InstallerSettings();

            string fileName = Path.Combine(Provider.Folder, "InstallerConfiguration.xml");
            if(File.Exists(fileName)) {
                using(StreamReader reader = new StreamReader(fileName)) {
                    var serialiser = Factory.Singleton.Resolve<IXmlSerialiser>();
                    result = serialiser.Deserialise<InstallerSettings>(reader);
                }
            }

            return result;
        }
    }
}
