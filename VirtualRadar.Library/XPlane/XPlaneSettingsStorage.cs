// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.XPlane;

namespace VirtualRadar.Library.XPlane
{
    /// <summary>
    /// Default implementation of <see cref="IXPlaneSettingsStorage"/>.
    /// </summary>
    class XPlaneSettingsStorage : IXPlaneSettingsStorage
    {
        // The file in the configuration folder that will hold the UI settings.
        const string FileName = "XPlaneSettings.json";

        /// <summary>
        /// Gets the full path to the settings file.
        /// </summary>
        public string FullPath
        {
            get {
                var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                return Path.Combine(configurationStorage.Folder, FileName);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public XPlaneSettings Load()
        {
            XPlaneSettings result = null;

            var fileName = FullPath;
            if(File.Exists(fileName)) {
                result = JsonConvert.DeserializeObject<XPlaneSettings>(
                    File.ReadAllText(fileName)
                );
            }

            return result ?? new XPlaneSettings();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="settings"></param>
        public void Save(XPlaneSettings settings)
        {
            var fileName = FullPath;
            File.WriteAllText(
                fileName,
                JsonConvert.SerializeObject(settings, Formatting.Indented)
            );
        }
    }
}
