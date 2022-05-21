using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="IWebAddressManager"/>.
    /// </summary>
    class WebAddressManager : IWebAddressManager
    {
        /// <summary>
        /// Controls writes to the fields.
        /// </summary>
        object _SyncLock = new object();

        /// <summary>
        /// True if the file has been loaded.
        /// </summary>
        volatile bool _Loaded;

        /// <summary>
        /// The content of the address file. Never change the collection - always take a reference to it when reading
        /// outside of a lock, and always overwrite it instead of changing it.
        /// </summary>
        volatile Dictionary<string, string> _Store = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string AddressFileFullPath { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebAddressManager()
        {
            var configStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            AddressFileFullPath = Path.Combine(
                configStorage.Folder,
                "WebAddresses.json"
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="oldAddresses"></param>
        /// <returns></returns>
        public string RegisterAddress(string name, string address, IList<string> oldAddresses = null)
        {
            name = (name ?? "").Trim();
            if(name == "") {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
            address = (address ?? "").Trim();
            if(address == "") {
                throw new ArgumentOutOfRangeException(nameof(address));
            }
            if(!Uri.TryCreate(address, UriKind.Absolute, out _)) {
                throw new ArgumentOutOfRangeException(nameof(address), $"{address} is not a valid URL");
            }

            var extant = LookupAddress(name);
            var changeCollection = extant == null;

            if(!changeCollection && extant != address) {
                // If the existing address isn't the same as the one we want to record then we want to preserve it
                // on the understanding that it's a custom address entered by the user.
                //
                // If the extant and new address differ only by case then we overwrite the extant with the new
                // address - this allows for fixes to old addresses where the wrong case was used.
                //
                // If the extant is known to be an old address that the program or plugin used in the past but no
                // longer supports then we are also allowed to change it.

                changeCollection = String.Equals(extant, address, StringComparison.OrdinalIgnoreCase);
                if(!changeCollection && oldAddresses != null) {
                    changeCollection = oldAddresses.Any(oldAddress => String.Equals(address, oldAddress, StringComparison.OrdinalIgnoreCase));
                }
            }

            if(changeCollection) {
                lock(_SyncLock) {
                    var newCollection = CollectionHelper.ShallowCopy(_Store);
                    newCollection[name] = address;
                    _Store = newCollection;
                    Save();
                }
            }

            return LookupAddress(name);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string LookupAddress(string name)
        {
            Load();

            var collection = _Store;
            collection.TryGetValue(name, out var result);

            return result;
        }

        /// <summary>
        /// Loads the address file from disk if it has not already been loaded.
        /// </summary>
        private void Load()
        {
            if(!_Loaded) {
                lock(_SyncLock) {
                    if(!_Loaded) {
                        _Loaded = true;

                        if(File.Exists(AddressFileFullPath)) {
                            var jsonText = File.ReadAllText(AddressFileFullPath);

                            Dictionary<string, string> rawDictionary;
                            try {
                                rawDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
                            } catch(Newtonsoft.Json.JsonException) {
                                BackupOldFileAsBad();
                                Save();
                                rawDictionary = new Dictionary<string, string>();
                            }

                            var newStore = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach(var kvp in rawDictionary) {
                                var key = (kvp.Key ?? "").Trim();
                                var value = (kvp.Value ?? "").Trim();

                                if(key != "" && value != "" && Uri.TryCreate(value, UriKind.Absolute, out _)) {
                                    newStore.Add(key, value);
                                }
                            }

                            _Store = newStore;
                        }
                    }
                }
            }
        }

        private void BackupOldFileAsBad()
        {
            lock(_SyncLock) {
                if(File.Exists(AddressFileFullPath)) {
                    var backupFileName = $"{AddressFileFullPath}-bad";
                    File.Copy(AddressFileFullPath, backupFileName, overwrite: true);
                }
            }
        }

        private void Save()
        {
            lock(_SyncLock) {
                var collection = _Store;
                var jsonText = JsonConvert.SerializeObject(collection, Formatting.Indented);
                File.WriteAllText(AddressFileFullPath, jsonText);
            }
        }
    }
}
