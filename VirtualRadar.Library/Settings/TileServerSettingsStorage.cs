// Copyright © 2018 onwards, Andrew Whewell
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
using System.Threading;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// Default implementation of <see cref="ITileServerSettingsStorage"/>.
    /// </summary>
    class TileServerSettingsStorage : ITileServerSettingsStorage
    {
        internal const string DownloadedTileServerSettingsFileName = "TileServerSettings-Downloaded.json";
        internal const string CustomTileServerSettingsFileName =     "TileServerSettings-Custom.json";
        internal const string ReadMeFileName =                       "TileServerSettings-ReadMe.txt";

        private static ITileServerSettingsStorage _Singleton = new TileServerSettingsStorage();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ITileServerSettingsStorage Singleton
        {
            get { return _Singleton; }
        }

        private string _Folder;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Folder
        {
            get { return _Folder ?? Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Folder; }
            set { _Folder = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool DownloadedSettingsFileExists()
        {
            var fullPath = Path.Combine(Folder, DownloadedTileServerSettingsFileName);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<TileServerSettings> Load()
        {
            var result = new List<TileServerSettings>();

            LoadIfFileExists(result, DownloadedTileServerSettingsFileName, isCustom: false);
            LoadIfFileExists(result, CustomTileServerSettingsFileName, isCustom: true);

            AddDefaultLeafletTileServer(result);
            EnsureSingleDefaultIsPresent(result, MapProvider.Leaflet);

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="settings"></param>
        public void SaveDownloadedSettings(TileServerSettings[] settings)
        {
            var folder = Folder;

            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            var fullPath = Path.Combine(folder, DownloadedTileServerSettingsFileName);

            var jsonText = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(fullPath, jsonText);
        }

        private void LoadIfFileExists(List<TileServerSettings> results, string fileName, bool isCustom = false)
        {
            var fullPath = Path.Combine(Folder, fileName);
            if(File.Exists(fullPath)) {
                try {
                    var jsonText = File.ReadAllText(fullPath);
                    var settingsList = JsonConvert.DeserializeObject<TileServerSettings[]>(jsonText);

                    foreach(var setting in settingsList) {
                        setting.Name = (setting.Name ?? "").Trim();
                        if(setting.Name == "") {
                            continue;
                        }
                        if(isCustom) {
                            setting.Name = String.Format("* {0}", setting.Name);
                        }

                        if(!results.Any(r => String.Equals(r.Name, setting.Name, StringComparison.OrdinalIgnoreCase) && r.MapProvider == setting.MapProvider)) {
                            setting.IsCustom = isCustom;
                            setting.IsDefault = isCustom ? false : setting.IsDefault;

                            setting.Url = (setting.Url ?? "").Trim();
                            if(setting.Url == "") {
                                continue;
                            }

                            setting.Attribution = (setting.Attribution ?? "").Trim();
                            if(setting.Attribution == "") {
                                continue;
                            }

                            results.Add(setting);
                        }
                    }
                } catch(JsonException ex) {
                    // These can occur if the user is writing their own custom JSON and they've knackered it up. If JSON coming from
                    // the server is throwing these then they're more serious, they need addressing.
                    if(!isCustom) {
                        throw;
                    }
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception parsing {0}: {1}", fullPath, ex.ToString());
                }
            }
        }

        private void EnsureSingleDefaultIsPresent(List<TileServerSettings> results, MapProvider mapProvider)
        {
            var allDefaults = results.Where(r => r.MapProvider == mapProvider && r.IsDefault && !r.IsCustom).ToArray();
            switch(allDefaults.Length) {
                case 0:
                    var nominated = results.OrderBy(r => (r.Name ?? "").ToLower()).FirstOrDefault(r => r.MapProvider == mapProvider && !r.IsCustom);
                    if(nominated != null) {
                        nominated.IsDefault = true;
                    }
                    break;
                case 1:
                    break;
                default:
                    foreach(var notDefault in allDefaults.OrderBy(r => (r.Name ?? "").ToLower()).Skip(1)) {
                        notDefault.IsDefault = false;
                    }
                    break;
            }
        }

        private void AddDefaultLeafletTileServer(List<TileServerSettings> results)
        {
            if(!results.Any(r => r.MapProvider == MapProvider.Leaflet)) {
                results.Add(new TileServerSettings() {
                    MapProvider =   MapProvider.Leaflet,
                    Name =          "OpenStreetMap",
                    IsDefault =     true,
                    Url =           "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                    Attribution =   "[c] [a href=http://www.openstreetmap.org/copyright]OpenStreetMap[/a]",
                    ClassName =     "vrs-brightness-70",
                    MaxZoom =       19,
                });
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void CreateReadme()
        {
            var folder = Folder;
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(Path.Combine(folder, ReadMeFileName), Strings.TileServerSettings_ReadMe);
        }
    }
}
