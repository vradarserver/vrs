// Copyright © 2017 onwards, Andrew Whewell
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

namespace VirtualRadar.Plugin.SqlServer
{
    public class Plugin : IPlugin
    {
        /// <summary>
        /// The instance of the plugin that VRS has created and is using.
        /// </summary>
        public static Plugin Singleton { get; private set; }

        /// <summary>
        /// The options used when the plugin was first started. Note that these do not change
        /// over the lifetime of the program - options are only loaded when the plugin first
        /// starts.
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id => "VirtualRadarServer.Plugin.SqlServer";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name => SqlServerStrings.PluginName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version => "2.4.0";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status
        {
            get => Options?.Enabled ?? false ? SqlServerStrings.Enabled : SqlServerStrings.Disabled;
            set {;}
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription
        {
            get => Options?.Enabled ?? false ? SqlServerStrings.Enabled : SqlServerStrings.Disabled;
            set {;}
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Raises <see cref="StatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStatusChanged(EventArgs args)
        {
            EventHelper.Raise(StatusChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            Singleton = this;
            Options = OptionsStorage.Load(this);
            if(Options.Enabled) {
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var dialog = new WinForms.OptionsView()) {
                dialog.Options = OptionsStorage.Load(this);
                if(dialog.ShowDialog() == DialogResult.OK) {
                    OptionsStorage.Save(this, dialog.Options);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
        }
    }
}
