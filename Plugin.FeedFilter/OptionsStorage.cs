// Copyright © 2015 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;
using System.IO;
using System.Xml.Serialization;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// Manages the loading and saving of the database editor's options.
    /// </summary>
    static class OptionsStorage
    {
        // Field names in the configuration file
        private const string Key = "Options";

        /// <summary>
        /// Raised when the options have been changed.
        /// </summary>
        public static EventHandler<EventArgs<Options>> OptionsChanged;

        /// <summary>
        /// Raises <see cref="OptionsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private static void OnOptionsChanged(EventArgs<Options> args)
        {
            if(OptionsChanged != null) OptionsChanged(null, args);
        }

        /// <summary>
        /// Loads the plugin's options.
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public static Options Load(Plugin plugin)
        {
            var pluginStorage = Factory.ResolveSingleton<IPluginSettingsStorage>();
            var pluginSettings = pluginStorage.Load();
            var serialisedOptions = pluginSettings.ReadString(plugin, Key);

            Options result = serialisedOptions == null ? new Options() : null;
            if(result == null) {
                try {
                    using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialisedOptions))) {
                        var serialiser = Factory.Resolve<IXmlSerialiser>();
                        result = serialiser.Deserialise<Options>(stream);
                    }
                } catch {
                    result = new Options();
                }
            }

            return result;
        }

        /// <summary>
        /// Saves the plugin's options.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="options"></param>
        public static void Save(Plugin plugin, Options options)
        {
            var currentOptions = Load(plugin);
            if(options.DataVersion != currentOptions.DataVersion) {
                throw new ConflictingUpdateException($"The options you are trying to save have changed since you loaded them. You are editing version {options.DataVersion}, the current version is {currentOptions.DataVersion}");
            }
            ++options.DataVersion;

            using(var stream = new MemoryStream()) {
                var serialiser = Factory.Resolve<IXmlSerialiser>();
                serialiser.Serialise(options, stream);

                var pluginStorage = Factory.ResolveSingleton<IPluginSettingsStorage>();
                var pluginSettings = pluginStorage.Load();
                pluginSettings.Write(plugin, Key, Encoding.UTF8.GetString(stream.ToArray()));
                pluginStorage.Save(pluginSettings);
            }

            OnOptionsChanged(new EventArgs<Options>(options));
        }
    }
}
