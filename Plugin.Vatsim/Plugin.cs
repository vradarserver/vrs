// Copyright © 2022 onwards, Andrew Whewell
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
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// The object that interfaces with VRS.
    /// </summary>
    public class Plugin : IPlugin
    {
        private object _SyncLock = new object();
        private List<VatsimFeed> _Feeds = new List<VatsimFeed>();

        /// <summary>
        /// The single instance of the plugin created by VRS.
        /// </summary>
        internal static Plugin Singleton { get; private set; }

        /// <summary>
        /// Gets the options being used by the plugin.
        /// </summary>
        internal static Options Options { get; set; } = new Options();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id => "VirtualRadarServer.Plugin.Vatsim";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name => VatsimStrings.PluginName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version => "3.0.0";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status => Options.Enabled ? VatsimStrings.Enabled : VatsimStrings.Disabled;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription => Status;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs args) => StatusChanged?.Invoke(this, args);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            Singleton = this;
            lock(_SyncLock) {
                Options = OptionsStorage.Load();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            ApplyOptions();
        }

        private void ApplyOptions()
        {
            lock(_SyncLock) {
                var feedManager = Factory.ResolveSingleton<IFeedManager>();

                DeleteFeedsNoLongerInOptions(feedManager);
                AddOrUpdateFeedsFromOptions(feedManager);

                if(VatsimDownloader.Started != Options.Enabled) {
                    if(Options.Enabled) {
                        VatsimDownloader.Start();
                    } else {
                        VatsimDownloader.Stop();
                    }
                }
            }
        }

        private void AddOrUpdateFeedsFromOptions(IFeedManager feedManager)
        {
            if(Options.Enabled) {
                var masterFeed = _Feeds.FirstOrDefault(r => r.IsMasterFeed);
                if(masterFeed != null) {
                    masterFeed.VatsimAircraftList.ApplyOptions();
                } else {
                    masterFeed = new VatsimFeed("Everything");
                    feedManager.AddCustomFeed(masterFeed);
                    _Feeds.Add(masterFeed);
                    masterFeed.Connect();
                }

                foreach(var geofencedFeedOption in Options.GeofencedFeeds) {
                    var feed = _Feeds.FirstOrDefault(r => r.GeofenceFeedOption?.ID == geofencedFeedOption.ID);

                    if(feed != null) {
                        feed.RefreshOptions(geofencedFeedOption);
                        feed.VatsimAircraftList.ApplyOptions();
                    } else {
                        var geofencedFeed = new VatsimFeed(geofencedFeedOption);
                        feedManager.AddCustomFeed(geofencedFeed);
                        _Feeds.Add(geofencedFeed);
                        geofencedFeed.Connect();
                    }
                }
            }
        }

        private void DeleteFeedsNoLongerInOptions(IFeedManager feedManager)
        {
            IEnumerable<VatsimFeed> deleteFeeds = _Feeds;

            if(Options.Enabled) {
                deleteFeeds = deleteFeeds.Where(feed =>
                       feed.GeofenceFeedOption != null
                    && !Options.GeofencedFeeds.Any(feedOption => feedOption.ID == feed.GeofenceFeedOption.ID)
                );
            }

            foreach(var deleteFeed in deleteFeeds.ToArray()) {
                try {
                    deleteFeed.Disconnect();
                } finally {
                    try {
                        feedManager.RemoveCustomFeed(deleteFeed);
                    } finally {
                        _Feeds.Remove(deleteFeed);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var optionsView = new WinForms.OptionsView()) {
                optionsView.Options = OptionsStorage.Load();
                if(optionsView.ShowDialog() == DialogResult.OK) {
                    optionsView.Options.NormaliseBeforeSave();
                    lock(_SyncLock) {
                        OptionsStorage.Save(optionsView.Options);
                        Options = optionsView.Options;
                        ApplyOptions();
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            ;
        }
    }
}
