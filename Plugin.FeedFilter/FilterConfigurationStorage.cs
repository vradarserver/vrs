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
            get { return Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Folder; }
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
