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
using InterfaceFactory;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="IPluginSettingsStorage"/>.
    /// </summary>
    sealed class PluginSettingsStorage : IPluginSettingsStorage
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The defaultImplementation of IPluginSettingsStorageProvider
        /// </summary>
        class DefaultProvider : IPluginSettingsStorageProvider
        {
            public bool FileExists(string fileName)                         { return File.Exists(fileName); }
            public string[] FileReadAllLines(string fileName)               { return File.ReadAllLines(fileName); }
            public void FileWriteAllLines(string fileName, string[] lines)  { File.WriteAllLines(fileName, lines); }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object which is locked to ensure single thread access to <see cref="Load"/> and <see cref="Save"/>.
        /// </summary>
        private object _SyncLock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPluginSettingsStorageProvider Provider { get; set; }

        private static readonly IPluginSettingsStorage _Singleton = new PluginSettingsStorage();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPluginSettingsStorage Singleton { get { return _Singleton; } }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        private string FileName
        {
            get
            {
                return Path.Combine(Factory.Resolve<IConfigurationStorage>().Singleton.Folder, "PluginsConfiguration.txt");
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanged, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginSettingsStorage()
        {
            Provider = new DefaultProvider();
        }
        #endregion

        #region Load, Save
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public PluginSettings Load()
        {
            var result = new PluginSettings();

            lock(_SyncLock) {
                var fileName = FileName;
                if(Provider.FileExists(fileName)) {
                    foreach(var line in Provider.FileReadAllLines(fileName)) {
                        var splitPosn = line.IndexOf('=');
                        if(splitPosn > 0) {
                            var key = line.Substring(0, splitPosn);
                            var value = HttpUtility.UrlDecode(line.Substring(splitPosn + 1));
                            result.Values.Add(key, value);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pluginSettings"></param>
        public void Save(PluginSettings pluginSettings)
        {
            if(pluginSettings == null) throw new ArgumentNullException("pluginSettings");

            lock(_SyncLock) {
                List<string> lines = new List<string>();
                for(int i = 0;i < pluginSettings.Values.Count;++i) {
                    var key = pluginSettings.Values.Keys[i];
                    var value = pluginSettings.Values[i];
                    if(value != null) value = HttpUtility.UrlEncode(value);
                    lines.Add(String.Format("{0}={1}", key, value));
                }
                Provider.FileWriteAllLines(FileName, lines.ToArray());
            }

            OnConfigurationChanged(EventArgs.Empty);
        }
        #endregion
    }
}
