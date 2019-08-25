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
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// A static class that handles the saving and loading of the filter configuration.
    /// </summary>
    static class FilterConfigurationStorage
    {
        /// <summary>
        /// Ensures that saves are single-threaded.
        /// </summary>
        private static object _SyncLock = new object();

        /// <summary>
        /// Gets the configuration folder.
        /// </summary>
        private static string Folder
        {
            get { return Factory.Resolve<IConfigurationStorage>().Singleton.Folder; }
        }

        /// <summary>
        /// Gets the configuration filename
        /// </summary>
        private static string FileName
        {
            get { return Path.Combine(Folder, "FeedFilterPlugin-FilterConfiguration.json"); }
        }

        /// <summary>
        /// Raised when the options have been changed.
        /// </summary>
        public static EventHandler<EventArgs<FilterConfiguration>> FilterConfigurationChanged;

        /// <summary>
        /// Raises <see cref="FilterConfigurationChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private static void OnFilterConfigurationChanged(EventArgs<FilterConfiguration> args)
        {
            if(FilterConfigurationChanged != null) FilterConfigurationChanged(null, args);
        }

        /// <summary>
        /// Loads the filter configuration.
        /// </summary>
        /// <returns></returns>
        public static FilterConfiguration Load()
        {
            FilterConfiguration result = null;

            var fileName = FileName;
            if(File.Exists(fileName)) {
                var fileText = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<FilterConfiguration>(fileText);
            }

            return result ?? new FilterConfiguration();
        }

        /// <summary>
        /// Saves the plugin's options.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="filterConfiguration"></param>
        /// <returns>
        /// True if the configuration was saved, false if the configuration was out-of-date.
        /// </returns>
        public static bool Save(Plugin plugin, FilterConfiguration filterConfiguration)
        {
            var result = false;

            lock(_SyncLock) {
                var folder = Folder;
                if(!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }

                var currentSettings = Load();
                result = currentSettings.DataVersion == filterConfiguration.DataVersion;
                if(result) {
                    ++filterConfiguration.DataVersion;

                    var json = JsonConvert.SerializeObject(filterConfiguration);
                    File.WriteAllText(FileName, json);
                }
            }

            if(result) {
                OnFilterConfigurationChanged(new EventArgs<FilterConfiguration>(filterConfiguration));
            }

            return result;
        }
    }
}
