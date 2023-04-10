// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="IConfigurationStorage"/>.
    /// </summary>
    sealed class ConfigurationStorage : IConfigurationStorage
    {
        private EnvironmentOptions _EnvironmentOptions;
        private IFileSystem _FileSystem;
        private ILog _Log;
        private IUserManager _UserManager;
        private IXmlSerialiser _XmlSerialiser;
        private object _FileAccessLock = new object();

        /// <inheritdoc/>
        public int CoarseListenerTimeout { get; set; }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        private string FileName => _FileSystem.Combine(
            _EnvironmentOptions.WorkingFolder,
            "Configuration.xml"
        );

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ConfigurationChanged;

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanged, this, args, ex => {
                _Log.WriteLine($"Caught exception in ConfigurationChanged event handler: {ex}");
            });
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConfigurationStorage(IOptions<EnvironmentOptions> environmentOptions, IFileSystem fileSystem, ILog log, IXmlSerialiser xmlSerialiser, IUserManager userManager)
        {
            _EnvironmentOptions = environmentOptions.Value;
            _FileSystem = fileSystem;
            _Log = log;
            _UserManager = userManager;
            _XmlSerialiser = xmlSerialiser;
            _XmlSerialiser.UseDefaultEnumValueIfUnknown = true;
        }

        /// <inheritdoc/>
        public void Erase()
        {
            var raiseEvent = false;

            lock(_FileAccessLock) {
                if(_FileSystem.FileExists(FileName)) {
                    _FileSystem.DeleteFile(FileName);
                    raiseEvent = true;
                }
            }

            if(raiseEvent) {
                OnConfigurationChanged(EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public Configuration Load()
        {
            var result = LoadIfExists(acquireLock: true);
            if(result != null) {
                bool needResave = false;
                needResave = GenerateRebroadcastServerUniqueIds(result) || needResave;

                if(result.Receivers.Count == 0) {
                    CreateInitialReceiverLocations(result);
                }

                if(needResave) {
                    Save(result);
                }
            }

            if(result == null) {
                result = new Configuration() {
                    Receivers = { new Receiver() {
                        Name = "Receiver",
                        Address = "127.0.0.1",
                        AutoReconnectAtStartup = true,
                        ConnectionType = ConnectionType.TCP,
                        DataSource = DataSource.Port30003,
                        Enabled = true,
                        Port = 30003,
                        UniqueId = 1,
                    } },
                };
                result.GoogleMapSettings.ClosestAircraftReceiverId = 1;
                result.GoogleMapSettings.FlightSimulatorXReceiverId = 1;
                result.GoogleMapSettings.WebSiteReceiverId = 1;
            }

            // Force retired settings to their expected values
            result.BaseStationSettings.IgnoreBadMessages = true;

            return result;
        }

        /// <summary>
        /// Loads the configuration or returns null if the configuration does not exist. Does not
        /// attempt to massage old versions of the configuration into the correct form.
        /// </summary>
        /// <param name="acquireLock"></param>
        /// <returns></returns>
        private Configuration LoadIfExists(bool acquireLock)
        {
            Configuration result = null;

            void readFile()
            {
                if(_FileSystem.FileExists(FileName)) {
                    using(System.IO.StreamReader stream = new(FileName, Encoding.UTF8)) {
                        result = _XmlSerialiser.Deserialise<Configuration>(stream);
                    }
                }
            }

            if(!acquireLock) {
                readFile();
            } else {
                lock(_FileAccessLock) {
                    readFile();
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns unique IDs to rebroadcast servers with missing unique IDs.
        /// </summary>
        /// <param name="configuration"></param>
        /// <remarks>The original version of the rebroadcast servers settings didn't have a unique ID,
        /// this just fills in any unique IDs from that version of the settings so that they are all
        /// greater than 0 and unique.</remarks>
        private bool GenerateRebroadcastServerUniqueIds(Configuration configuration)
        {
            var result = false;

            if(configuration.RebroadcastSettings.Count > 0) {
                var oldRecords = configuration.RebroadcastSettings.Where(r => r.UniqueId == 0).ToArray();
                if(oldRecords.Length > 0) {
                    result = true;
                    var uniqueId = configuration.RebroadcastSettings.Select(r => r.UniqueId).Max() + 1;
                    foreach(var oldRecord in oldRecords) {
                        oldRecord.UniqueId = uniqueId++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds an initial receiver from the existing BaseStation settings.
        /// </summary>
        /// <param name="configuration"></param>
        /// <remarks><para>
        /// In the beginning VRS only supported one receiver, and the settings for those were (mostly) stored in BaseStationSettings.
        /// Then support for multiple receivers was added, the <see cref="Receiver"/> settings object created and a list of receivers
        /// added to the <see cref="Configuration"/> object. It would be nice if we can load old configuration files and create a new
        /// receiver object from the old settings for existing users but also support new users who haven't configured anything yet,
        /// so this function was written.
        /// </para><para>
        /// The short version is that if the Receivers list is empty then we create a new Receiver culled from the various values that
        /// were previously used. These values still exist in the configuration objects, so this will work for new users as well. If
        /// the list is not empty then it is not touched. If we add new properties to Receiver in the future then this will still work,
        /// any values that need defaulting will be defaulted by the constructor and won't be touched by this function.
        /// </para></remarks>
        private void CreateInitialReceiverLocations(Configuration configuration)
        {
            var baseStation = configuration.BaseStationSettings;
            var rawDecoding = configuration.RawDecodingSettings;
            configuration.Receivers.Add(new Receiver() {
                Address = baseStation.Address,
                AutoReconnectAtStartup = baseStation.AutoReconnectAtStartup,
                BaudRate = baseStation.BaudRate,
                ComPort = baseStation.ComPort,
                ConnectionType = baseStation.ConnectionType,
                DataBits = baseStation.DataBits,
                DataSource = baseStation.DataSource,
                Enabled = true,
                Name = "Receiver",
                Port = baseStation.Port,
                ReceiverLocationId = rawDecoding.ReceiverLocationId,
                ShutdownText = baseStation.ShutdownText,
                StartupText = baseStation.StartupText,
                UniqueId = 1,
            });

            configuration.GoogleMapSettings.ClosestAircraftReceiverId =
            configuration.GoogleMapSettings.FlightSimulatorXReceiverId =
            configuration.GoogleMapSettings.WebSiteReceiverId = configuration.Receivers[0].UniqueId;

            foreach(var rebroadcastServer in configuration.RebroadcastSettings) {
                rebroadcastServer.ReceiverId = configuration.Receivers[0].UniqueId;
            }
        }

        /// <inheritdoc/>
        public void Save(Configuration configuration)
        {
            lock(_FileAccessLock) {
                _FileSystem.CreateDirectoryIfNotExists(_EnvironmentOptions.WorkingFolder);

                Configuration currentConfiguration = null;
                var ignoreLoadException = configuration.DataVersion == 0;       // i.e. it's a brand new configuration
                try {
                    currentConfiguration = LoadIfExists(acquireLock: false);
                } catch {
                    if(ignoreLoadException) {
                        currentConfiguration = null;
                    } else {
                        throw;
                    }
                }

                if(currentConfiguration != null && currentConfiguration.DataVersion != configuration.DataVersion) {
                    throw new ConflictingUpdateException($"Cannot save the configuration - it has been saved elsewhere since you last loaded it. " +
                                                         $"Current data version is {currentConfiguration.DataVersion}, you are attempting to save version {configuration.DataVersion}.");
                }

                BackupOldSettings();

                ++configuration.DataVersion;

                using(System.IO.StreamWriter stream = new(FileName, false, Encoding.UTF8)) {
                    _XmlSerialiser.Serialise(configuration, stream);
                }
            }

            OnConfigurationChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Copies the settings file into the ConfigBackups folder and gives it a timestamp.
        /// </summary>
        private void BackupOldSettings()
        {
            if(_FileSystem.FileExists(FileName)) {
                var folder = _FileSystem.Combine(_EnvironmentOptions.WorkingFolder, "ConfigBackups");
                _FileSystem.CreateDirectoryIfNotExists(folder);

                var fileInfo = new System.IO.FileInfo(FileName);

                var backupFileName = _FileSystem.Combine(folder, String.Format("Configuration-{0:yyyy}-{0:MM}-{0:dd}-{0:HH}-{0:mm}-{0:ss}.xml", fileInfo.LastWriteTimeUtc));
                if(!_FileSystem.FileExists(backupFileName)) {
                    _FileSystem.CopyFile(FileName, backupFileName, overwrite: false);
                }
            }
        }
    }
}
