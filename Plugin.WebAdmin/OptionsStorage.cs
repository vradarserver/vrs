// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using InterfaceFactory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.WebAdmin
{
    class OptionsStorage
    {
        // Field names in the configuration file
        private const string Key = "Options";

        /// <summary>
        /// Loads the plugin's options.
        /// </summary>
        /// <returns></returns>
        public static Options Load()
        {
            var pluginStorage = Factory.Singleton.ResolveSingleton<IPluginSettingsStorage>();
            var pluginSettings = pluginStorage.Load();

            var jsonOptions = pluginSettings.ReadString(Plugin.Singleton, Key);
            var result = String.IsNullOrEmpty(jsonOptions) ? Options.Default() : JsonConvert.DeserializeObject<Options>(jsonOptions);

            // There was a bug in preview releases of the plugin that could double-up entries in the Access object every time
            // the settings were saved. This block cleans up the options for those users that might have hit the bug:
            var distinctAccessAddresses = result.Access.Addresses.Distinct().ToArray();
            result.Access.Addresses.Clear();
            result.Access.Addresses.AddRange(distinctAccessAddresses);

            return result;
        }

        /// <summary>
        /// Saves the plugin's options.
        /// </summary>
        /// <param name="options"></param>
        public static void Save(Options options)
        {
            var currentOptions = Load();
            if(options.DataVersion != currentOptions.DataVersion) {
                throw new ConflictingUpdateException($"The options you are trying to save have changed since you loaded them. You are editing version {options.DataVersion}, the current version is {currentOptions.DataVersion}");
            }
            ++options.DataVersion;

            var storage = Factory.Singleton.ResolveSingleton<IPluginSettingsStorage>();

            var pluginSettings = storage.Load();
            pluginSettings.Write(Plugin.Singleton, Key, JsonConvert.SerializeObject(options));

            storage.Save(pluginSettings);

            Plugin.Singleton.RaiseSettingsChanged(new EventArgs<Options>(options));
        }
    }
}
