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
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    /// <summary>
    /// A class that is capable of loading and storing <see cref="Options"/> using VRS' plugin settings mechanism.
    /// </summary>
    class OptionsStorage
    {
        // Field names in the configuration file
        private const string OptionsField = "Options";

        // Old field names from when the options were saved field-by-field
        private const string EnabledField = "Enabled";
        private const string AllowUpdateOfOtherDatabasesField = "AllowUpdateOfOtherDatabases";
        private const string ReceiverId = "ReceiverId";

        /// <summary>
        /// Loads the plugin's options.
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public Options Load(Plugin plugin)
        {
            var storage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;
            var pluginSettings = storage.Load();

            var jsonText = pluginSettings.ReadString(plugin, OptionsField);
            var result = String.IsNullOrEmpty(jsonText) ? null : JsonConvert.DeserializeObject<Options>(jsonText);

            if(result == null) {
                // Support reading old format options from plugin settings
                result = new Options() {
                    Enabled = pluginSettings.ReadBool(plugin, EnabledField).GetValueOrDefault(),
                    AllowUpdateOfOtherDatabases = pluginSettings.ReadBool(plugin, AllowUpdateOfOtherDatabasesField).GetValueOrDefault(),
                    ReceiverId = pluginSettings.ReadInt(plugin, ReceiverId).GetValueOrDefault(),
                };
            }

            return result;
        }

        /// <summary>
        /// Saves the plugin's options.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="options"></param>
        public void Save(Plugin plugin, Options options)
        {
            var storage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;

            var pluginSettings = storage.Load();
            pluginSettings.Write(plugin, OptionsField, JsonConvert.SerializeObject(options));

            storage.Save(pluginSettings);
        }
    }
}