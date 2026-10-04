// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.AirportDataThumbnails
{
    public class Plugin : IPlugin
    {
        internal static Plugin Singleton { get; private set; }

        internal static IAirportDataDotCom OriginalImplementation { get; private set; }

        private Options _Options;
        internal Options Options
        {
            get { return _Options; }
            set {
                _Options = value;
                Status = value.Enabled
                    ? AirportDataThumbnailsStrings.StatusEnabled
                    : AirportDataThumbnailsStrings.StatusDisabled;
            }
        }

        /// <inheritdoc/>
        public string Id { get { return "AirportDataThumbnails"; } }

        /// <inheritdoc/>
        public string Name { get { return AirportDataThumbnailsStrings.PluginName; } }

        /// <inheritdoc/>
        public string Version { get { return "3.1.0"; } }

        private string _Status;
        /// <inheritdoc/>
        public string Status
        {
            get { return _Status; }
            private set {
                if(value != _Status) {
                    _Status = value;
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public string StatusDescription { get { return ""; } }

        /// <inheritdoc/>
        public bool HasOptions { get { return true; } }

        /// <inheritdoc/>
        public string PluginFolder { get; set; }

        /// <inheritdoc/>
        public event EventHandler StatusChanged;

        private void OnStatusChanged(EventArgs args)
        {
            EventHelper.Raise(StatusChanged, this, args);
        }

        /// <inheritdoc/>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            Singleton = this;
            Options = OptionsStorage.Load();

            OriginalImplementation = classFactory.Resolve<IAirportDataDotCom>();
            classFactory.Register<IAirportDataDotCom, AirportDataDotCom>();
        }

        /// <inheritdoc/>
        public void Startup(PluginStartupParameters parameters)
        {
        }

        /// <inheritdoc/>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.ResolveSingleton<IWebAdminViewManager>();
            webAdminViewManager.AddWebAdminView(
                new WebAdminView(
                    "/WebAdmin/",
                    "AirportDataThumbnailsPluginOptions.html",
                    AirportDataThumbnailsStrings.WebAdminMenuName,
                    () => new WebAdmin.OptionsView(),
                    typeof(AirportDataThumbnailsStrings)
                ) {
                    Plugin = this,
                }
            );
            webAdminViewManager.RegisterWebAdminViewFolder(PluginFolder, "Web");
        }

        /// <inheritdoc/>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = new WinForms.OptionsView()) {
                var options = OptionsStorage.Load();
                view.PluginEnabled = options.Enabled;

                if(view.ShowDialog() == DialogResult.OK) {
                    options.Enabled = view.PluginEnabled;
                    OptionsStorage.Save(options);
                }
            }
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
        }
    }
}
