// Copyright © 2014 onwards, Andrew Whewell
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using System.ComponentModel;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// Implements <see cref="IPlugin"/> to tell VRS about our plugin.
    /// </summary>
    public class Plugin : IPlugin
    {
        #region Fields
        /// <summary>
        /// The options that govern the plugin's behaviour.
        /// </summary>
        private Options _Options = new Options();

        /// <summary>
        /// The object that we use to extend the website.
        /// </summary>
        private IWebSiteExtender _WebSiteExtender;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id { get { return "VirtualRadar.Plugin.WebAdmin"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return "Web Admin Pages"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.0.3"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription { get; private set; }
        #endregion

        #region Events
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
            if(StatusChanged != null) StatusChanged(this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Plugin()
        {
            Status = WebAdminStrings.Disabled;
        }
        #endregion

        #region RegisterImplementations
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
        }
        #endregion

        #region Startup
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            _Options = OptionsStorage.Load(this);

            _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = _Options.Enabled;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.Initialise(parameters);

            ApplyOptions();
        }
        #endregion

        #region GuiThreadStartup
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
        }
        #endregion

        #region ShowWinFormsOptionsUI
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
        }
        #endregion

        #region ApplyOptions
        /// <summary>
        /// Applies the options.
        /// </summary>
        private void ApplyOptions()
        {
            OnStatusChanged(EventArgs.Empty);
        }
        #endregion
    }
}
