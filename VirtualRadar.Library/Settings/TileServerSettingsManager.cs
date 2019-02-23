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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="ITileServerSettingsManager"/>.
    /// </summary>
    class TileServerSettingsManager : ITileServerSettingsManager
    {
        internal const int DownloadTimeoutSeconds = 20;
        internal const int InitialiseTimeoutSeconds = 15;
        internal const int DownloadIntervalMinutes = 24 * 60;

        /// <summary>
        /// The object used to lock multithreaded writes.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// True if the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The date and time of the last successful download of settings from the mothership.
        /// </summary>
        private DateTime _LastSuccessfulDownloadUtc;

        /// <summary>
        /// The date and time of the last download attempt.
        /// </summary>
        private DateTime _LastDownloadAttemptUtc;

        /// <summary>
        /// The collection of tile server settings last downloaded / loaded. Take a copy of the reference
        /// before reading, only write within a lock.
        /// </summary>
        private List<TileServerSettings> _TileServerSettings = new List<TileServerSettings>();

        /// <summary>
        /// The singleton object that can store tile server settings locally.
        /// </summary>
        private ITileServerSettingsStorage _Storage;

        private static ITileServerSettingsManager _Singleton = new TileServerSettingsManager();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ITileServerSettingsManager Singleton
        {
            get { return _Singleton; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(!_Initialised) {
                lock(_SyncLock) {
                    if(!_Initialised) {
                        _Initialised = true;
                        _Storage = Factory.Singleton.Resolve<ITileServerSettingsStorage>().Singleton;
                        var reportErrors = new StringBuilder();

                        CatchErrorsDuringInitialise(reportErrors, "downloading tile server settings", () => {
                            if(!_Storage.DownloadedSettingsFileExists()) {
                                DoDownloadTileServerSettings(InitialiseTimeoutSeconds);
                            }
                        });

                        CatchErrorsDuringInitialise(reportErrors, "loading tile server settings", () => {
                            LoadTileServerSettings();
                        });

                        CatchErrorsDuringInitialise(reportErrors, "creating tile server settings readme", () => {
                            _Storage.CreateReadme();
                        });

                        if(reportErrors.Length > 0) {
                            var messageBox = Factory.Singleton.Resolve<IMessageBox>();
                            messageBox.Show(reportErrors.ToString(), "Errors While Initialising Tile Server Settings Manager");
                        }

                        var heartbeat = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
                        heartbeat.SlowTick += Heartbeat_SlowTick;
                    }
                }
            }
        }

        private void CatchErrorsDuringInitialise(StringBuilder messageBuffer, string describeAction, Action action)
        {
            try {
                action();
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                var msg = $"Caught exception while {describeAction} at startup. See log for more details.";
                if(messageBuffer.Length > 0) {
                    messageBuffer.AppendLine();
                }
                messageBuffer.AppendLine(msg);

                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine($"Caught exception while {describeAction} at startup: {Describe.ExceptionMultiLine(ex)}");
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public bool MapProviderUsesTileServers(MapProvider mapProvider)
        {
            switch(mapProvider) {
                case MapProvider.GoogleMaps:    return false;
                case MapProvider.Leaflet:       return true;
                default:                        throw new NotImplementedException();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public TileServerSettings GetDefaultTileServerSettings(MapProvider mapProvider)
        {
            var settings = _TileServerSettings;
            return settings.FirstOrDefault(r => r.IsDefault && !r.IsCustom);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="fallbackToDefaultIfMissing"></param>
        /// <returns></returns>
        public TileServerSettings GetTileServerSettings(MapProvider mapProvider, string name, bool fallbackToDefaultIfMissing)
        {
            var settings = _TileServerSettings;
            var result = settings.FirstOrDefault(r => r.MapProvider == mapProvider && String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
            if(result == null && fallbackToDefaultIfMissing) {
                result = GetDefaultTileServerSettings(mapProvider);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public TileServerSettings[] GetAllTileServerSettings(MapProvider mapProvider)
        {
            var result = new List<TileServerSettings>();

            var settings = _TileServerSettings;
            result.AddRange(settings.Where(r => r.MapProvider == mapProvider));

            return result.ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void DownloadTileServerSettings()
        {
            DoDownloadTileServerSettings(DownloadTimeoutSeconds);
        }

        private void DoDownloadTileServerSettings(int timeoutSeconds)
        {
            lock(_SyncLock) {
                TileServerSettings[] settings = null;

                var downloader = Factory.Singleton.Resolve<ITileServerSettingsDownloader>();
                try {
                    _LastDownloadAttemptUtc = DateTime.UtcNow;
                    settings = downloader.Download(timeoutSeconds);
                    _LastSuccessfulDownloadUtc = DateTime.UtcNow;
                } catch(WebException ex) {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception downloading new tile server settings: {0}", Describe.ExceptionMultiLine(ex));
                    settings = null;
                }

                if(settings != null) {
                    _Storage.SaveDownloadedSettings(settings);
                }

                LoadTileServerSettings();
            }
        }

        /// <summary>
        /// Loads tile server settings from disk and records them for later use.
        /// </summary>
        private void LoadTileServerSettings()
        {
            lock(_SyncLock) {
                var settings = _Storage.Load();

                AddDefaultLeafletTileServer(settings);
                EnsureSingleDefaultIsPresent(settings, MapProvider.Leaflet);
                FixupAttributions(settings);

                _TileServerSettings = settings;
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

        static Regex _AttributionRegex = new Regex(@"\[attribution (?<name>.+?)\]");

        private void FixupAttributions(List<TileServerSettings> allSettings)
        {
            foreach(var mapProvider in allSettings.Select(r => r.MapProvider).Distinct()) {
                var providerSettings = allSettings.Where(r => r.MapProvider == mapProvider).ToArray();
                foreach(var setting in providerSettings) {
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var completed = false;

                    do {
                        var match = _AttributionRegex.Match(setting.Attribution ?? "");
                        completed = !match.Success;
                        if(!completed) {
                            var name = match.Groups["name"].Value ?? "";
                            if(usedNames.Contains(name)) {
                                throw new InvalidOperationException(String.Format("Found recursive reference to {0} when expanding attribute for {1} server {2}", name, mapProvider, setting.Name));
                            }
                            usedNames.Add(name);

                            var otherSetting = providerSettings.FirstOrDefault(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
                            var buffer = new StringBuilder(setting.Attribution);
                            buffer.Remove(match.Index, match.Length);
                            buffer.Insert(match.Index, otherSetting?.Attribution ?? $"Unknown attribution ID {name}");
                            setting.Attribution = buffer.ToString();
                        }
                    } while(!completed);
                }
            }
        }

        /// <summary>
        /// Downloads the settings once a day in the background.
        /// </summary>
        private void DownloadSettingsOnHeartbeatThread()
        {
            var now = DateTime.UtcNow;
            var downloadThreshold = now.AddMinutes(-DownloadIntervalMinutes);
            if(_LastSuccessfulDownloadUtc <= downloadThreshold) {
                lock(_SyncLock) {
                    if(_LastSuccessfulDownloadUtc <= downloadThreshold && _LastDownloadAttemptUtc <= now.AddSeconds(-5)) {
                        DownloadTileServerSettings();
                    }
                }
            }
        }

        /// <summary>
        /// Called every few seconds by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs e)
        {
            DownloadSettingsOnHeartbeatThread();
        }
    }
}
