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
using VirtualRadar.Interface;
using InterfaceFactory;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IPluginManager"/>.
    /// </summary>
    class PluginManager : IPluginManager
    {
        /// <summary>
        /// The default implementation of <see cref="IPluginManagerProvider"/>.
        /// </summary>
        class DefaultProvider : IPluginManagerProvider
        {
            public string ApplicationStartupPath { get { return Application.StartupPath; } }

            public IEnumerable<string> DirectoryGetFiles(string folder, string searchPattern)
            {
                return Directory.GetFiles(folder, searchPattern);
            }

            public IEnumerable<string> DirectoryGetDirectories(string folder)
            {
                return Directory.GetDirectories(folder);
            }

            public bool DirectoryExists(string folder)
            {
                return Directory.Exists(folder);
            }

            public IEnumerable<Type> LoadTypes(string fullPath)
            {
                List<Type> result = new List<Type>();

                var assembly = Assembly.LoadFile(fullPath);
                foreach(var module in assembly.GetModules(false)) {
                    result.AddRange(module.GetTypes());
                }

                return result;
            }

            public IClassFactory ClassFactoryTakeSnapshot()
            {
                return Factory.TakeSnapshot();
            }

            public void ClassFactoryRestoreSnapshot(IClassFactory snapshot)
            {
                Factory.RestoreSnapshot(snapshot);
            }
        }

        /// <summary>
        /// A map of plugin IDs to the DLL filename.
        /// </summary>
        private Dictionary<string, string> _PluginIDToFileNameMap = new Dictionary<string, string>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPluginManagerProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<IPlugin> LoadedPlugins { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDictionary<string, string> IgnoredPlugins { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginManager()
        {
            Provider = new DefaultProvider();
            LoadedPlugins = new List<IPlugin>();
            IgnoredPlugins = new Dictionary<string, string>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void LoadPlugins()
        {
            var log = Factory.Singleton.ResolveSingleton<ILog>();
            var manifestStorage = Factory.Singleton.Resolve<IPluginManifestStorage>();
            var applicationVersion = Factory.Singleton.Resolve<IApplicationInformation>().Version;

            var rootFolder = Path.Combine(Provider.ApplicationStartupPath, "Plugins");
            if(Provider.DirectoryExists(rootFolder)) {
                foreach(var subFolder in Provider.DirectoryGetDirectories(rootFolder)) {
                    foreach(var dllFileName in Provider.DirectoryGetFiles(subFolder, "VirtualRadar.Plugin.*.dll")) {
                        if(ManifestAllowsLoad(manifestStorage, applicationVersion, dllFileName)) {
                            try {
                                var pluginTypes = Provider.LoadTypes(dllFileName).Where(t => t.IsClass && typeof(IPlugin).IsAssignableFrom(t)).ToList();
                                if(pluginTypes.Count != 1) {
                                    IgnoredPlugins.Add(dllFileName, Strings.PluginDoesNotHaveJustOneIPlugin);
                                    continue;
                                }

                                var pluginType = pluginTypes[0];
                                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                                plugin.PluginFolder = subFolder;

                                if(!_PluginIDToFileNameMap.ContainsKey(plugin.Id)) {
                                    _PluginIDToFileNameMap.Add(plugin.Id, dllFileName);
                                    LoadedPlugins.Add(plugin);
                                }
                            } catch(Exception ex) {
                                var exceptionMessage = FormatException(ex);
                                Debug.WriteLine(String.Format("PluginManager.LoadPlugins caught exception: {0}", exceptionMessage));
                                log.WriteLine("Caught exception loading plugin {0}: {1}", dllFileName, exceptionMessage);

                                IgnoredPlugins.Add(dllFileName, String.Format(Strings.PluginCannotBeLoaded, ex.Message));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RegisterImplementations()
        {
            var log = Factory.Singleton.ResolveSingleton<ILog>();

            foreach(var plugin in LoadedPlugins.ToArray()) {
                var snapshot = Provider.ClassFactoryTakeSnapshot();
                try {
                    plugin.RegisterImplementations(Factory.Singleton);
                } catch(Exception ex) {
                    Provider.ClassFactoryRestoreSnapshot(snapshot);

                    var dllFileName = _PluginIDToFileNameMap[plugin.Id];
                    _PluginIDToFileNameMap.Remove(plugin.Id);
                    LoadedPlugins.Remove(plugin);
                    IgnoredPlugins.Add(dllFileName, String.Format(Strings.PluginCannotBeLoaded, ex.Message));
                    log.WriteLine("Caught exception loading plugin {0}: {1}", dllFileName, FormatException(ex));
                }
            }
        }

        /// <summary>
        /// Returns a string showing a recursive dump of the entire exception, inner exception and loader exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string FormatException(Exception ex)
        {
            var buffer = new StringBuilder();
            FormatException(buffer, ex, "<<TOP LEVEL>>");

            return buffer.ToString();
        }

        private void FormatException(StringBuilder buffer, Exception ex, string category)
        {
            buffer.AppendLine(String.Format("{0}: {1}", category, ex));
            if(ex.InnerException != null) FormatException(buffer, ex.InnerException, "<<INNER>>");

            var reflectionException = ex as ReflectionTypeLoadException;
            if(reflectionException != null && reflectionException.LoaderExceptions != null) {
                foreach(var loadEx in reflectionException.LoaderExceptions) {
                    FormatException(buffer, loadEx, "<<LOADER EXCEPTION>>");
                }
            }
        }

        /// <summary>
        /// Loads the manifest and returns true if it permits the loading of the plugin. If it prohibits
        /// the load then <see cref="IgnoredPlugins"/> is updated.
        /// </summary>
        /// <param name="manifestStorage"></param>
        /// <param name="applicationVersion"></param>
        /// <param name="dllFileName"></param>
        /// <returns></returns>
        private bool ManifestAllowsLoad(IPluginManifestStorage manifestStorage, Version applicationVersion, string dllFileName)
        {
            PluginManifest manifest = null;
            try {
                manifest = manifestStorage.LoadForPlugin(dllFileName);
                if(manifest == null) IgnoredPlugins.Add(dllFileName, Strings.CouldNotFindManifest);
            } catch(Exception ex) {
                IgnoredPlugins.Add(dllFileName, String.Format(Strings.CouldNotParseManifest, ex.Message));
            }

            bool result = manifest != null;
            if(result && !String.IsNullOrEmpty(manifest.MinimumVersion)) result = CompareManifestVersions(manifest.MinimumVersion, applicationVersion, dllFileName, true);
            if(result && !String.IsNullOrEmpty(manifest.MaximumVersion)) result = CompareManifestVersions(manifest.MaximumVersion, applicationVersion, dllFileName, false);

            return result;
        }

        private bool CompareManifestVersions(string manifestVersion, Version applicationVersion, string dllFileName, bool isMinimum)
        {
            bool result = false;

            try {
                int comparison = VersionComparer.Compare(manifestVersion, applicationVersion);
                result = isMinimum ? comparison <= 0 : comparison >= 0;
                if(!result) IgnoredPlugins.Add(dllFileName, isMinimum ? String.Format(Strings.PluginMinimumVersionNotMet, manifestVersion) : String.Format(Strings.PluginMaximumVersionNotMet, manifestVersion));
            } catch {
                IgnoredPlugins.Add(dllFileName, isMinimum ? Strings.PluginMinumumVersionUnparseable : Strings.PluginMaximumVersionUnparseable);
            }

            return result;
        }
    }
}
