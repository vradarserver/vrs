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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Plugin.Vatsim
{
    public class Plugin : IPlugin
    {
        private object _SyncLock = new object();

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
            var feedManager = Factory.ResolveSingleton<IFeedManager>();

            // Master feed - this can be a teensy-weensy bit punishing on browsers when VATSIM is busy...
            var masterFeed = new VatsimFeed();
            feedManager.AddCustomFeed(masterFeed);
            masterFeed.Connect();

            foreach(var geofencedFeedOption in Options.GeofencedFeeds) {
                var geofencedFeed = new VatsimFeed(geofencedFeedOption);
                feedManager.AddCustomFeed(geofencedFeed);
                geofencedFeed.Connect();
            }

            VatsimDownloader.Start();
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
                if(optionsView.ShowDialog() == DialogResult.OK) {
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
