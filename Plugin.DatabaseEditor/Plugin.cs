// Copyright © 2015 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.Plugin.DatabaseEditor.Models;

namespace VirtualRadar.Plugin.DatabaseEditor
{
    /// <summary>
    /// The entry point for the plugin that adds web pages that let administrator users
    /// edit aircraft records.
    /// </summary>
    public class Plugin : IPlugin
    {
        /// <summary>
        /// The plugin's protected folder.
        /// </summary>
        private static readonly string ProtectedFolder = "DatabaseEditor";

        /// <summary>
        /// The plugin's options.
        /// </summary>
        private Options _Options;

        /// <summary>
        /// The object that hooks us into the web site.
        /// </summary>
        private IWebSiteExtender _WebSiteExtender;

        /// <summary>
        /// The object that can substitute translations into double-colon-delimited place-holders
        /// in a web page for us.
        /// </summary>
        private IHtmlLocaliser _HtmlLocaliser;

        /// <summary>
        /// Gets the last initialised instance of the plugin object. At run-time only one plugin
        /// object gets created and initialised.
        /// </summary>
        public static Plugin Singleton { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id { get { return "VirtualRadarServer.Plugin.DatabaseEditor"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return DatabaseEditorStrings.PluginName; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "3.0.0"; } }

        private string _Status;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status
        {
            get { return _Status; }
            internal set {
                if(value != _Status) {
                    _Status = value;
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        private string _StatusDescription;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription
        {
            get { return _StatusDescription; }
            internal set {
                if(value != _StatusDescription) {
                    _StatusDescription = value;
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// The object that handles access to the database.
        /// </summary>
        internal IBaseStationDatabase BaseStationDatabase { get; private set; }

        /// <summary>
        /// The number of searches made through the plugin.
        /// </summary>
        private int _SearchCount;
        internal int SearchCount => _SearchCount;
        internal void IncrementSearchCount() => Interlocked.Increment(ref _SearchCount);

        /// <summary>
        /// The number of updates made through the plugin.
        /// </summary>
        private int _UpdateCount;
        internal int UpdateCount => _UpdateCount;
        internal void IncrementUpdateCount() => Interlocked.Increment(ref _UpdateCount);

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
        /// Raised when <see cref="OptionsStorage"/> saves a new set of options.
        /// </summary>
        public event EventHandler<EventArgs<Options>> SettingsChanged;

        /// <summary>
        /// Raises <see cref="SettingsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        internal void RaiseSettingsChanged(EventArgs<Options> args)
        {
            ApplyOptions(args.Value);
            EventHelper.Raise(SettingsChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(InterfaceFactory.IClassFactory classFactory)
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            Singleton = this;
            var options = OptionsStorage.Load(this);

            _HtmlLocaliser = Factory.Resolve<IHtmlLocaliser>();
            _HtmlLocaliser.Initialise();
            _HtmlLocaliser.AddResourceStrings(typeof(DatabaseEditorStrings));

            BaseStationDatabase = Factory.ResolveSingleton<IAutoConfigBaseStationDatabase>().Database;
            BaseStationDatabase.WriteSupportEnabled = true;

            _WebSiteExtender = Factory.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = false;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.InjectContent = @"<script src=""script-DatabaseEditor/inject.js"" type=""text/javascript"">";
            _WebSiteExtender.InjectMapPages();
            _WebSiteExtender.InjectReportPages();
            _WebSiteExtender.Initialise(parameters);
            _WebSiteExtender.ProtectFolder(ProtectedFolder);

            parameters.WebSite.HtmlLoadedFromFile += WebSite_HtmlLoadedFromFile;

            var redirection = Factory.ResolveSingleton<IRedirectionConfiguration>();
            redirection.AddRedirection("/DatabaseEditor",  "/DatabaseEditor/index.html", RedirectionContext.Any);
            redirection.AddRedirection("/DatabaseEditor/", "/DatabaseEditor/index.html", RedirectionContext.Any);

            ApplyOptions(options);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.ResolveSingleton<IWebAdminViewManager>();
            webAdminViewManager.RegisterTranslations(typeof(DatabaseEditorStrings), "DatabaseEditorPlugin");
            webAdminViewManager.AddWebAdminView(new WebAdminView("/WebAdmin/", "DatabaseEditorPluginOptions.html", DatabaseEditorStrings.WebAdminMenuName, () => new WebAdmin.OptionsView(), typeof(DatabaseEditorStrings)) {
                Plugin = this,
            });
            webAdminViewManager.RegisterWebAdminViewFolder(PluginFolder, "Web-WebAdmin");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            if(_WebSiteExtender != null) _WebSiteExtender.Dispose();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var dialog = new WinForms.OptionsView()) {
                dialog.IndexPageAddress = GetIndexPageAddress();
                dialog.Options = OptionsStorage.Load(this);

                if(dialog.ShowDialog() == DialogResult.OK) {
                    OptionsStorage.Save(this, dialog.Options);
                }
            }
        }

        /// <summary>
        /// Returns the address of the index page.
        /// </summary>
        /// <returns></returns>
        internal string GetIndexPageAddress()
        {
            var webServer = Factory.ResolveSingleton<IAutoConfigWebServer>().WebServer;
            return String.Format("{0}/{1}", webServer.LocalAddress, "DatabaseEditor/index.html");
        }

        /// <summary>
        /// Applies the settings from the currently loaded options.
        /// </summary>
        private void ApplyOptions(Options options)
        {
            _Options = options;
            _WebSiteExtender.Enabled = options.Enabled;
            _WebSiteExtender.RestrictAccessToFolder(ProtectedFolder, options.Access);

            using(new CultureSwitcher()) {
                if(!options.Enabled) {
                    Status = DatabaseEditorStrings.StatusDisabled;
                } else {
                    Status = Strings.Enabled;
                }
            }

            UpdateStatusTotals();
        }

        /// <summary>
        /// Shows the counters on the status description line.
        /// </summary>
        internal void UpdateStatusTotals()
        {
            using(new CultureSwitcher()) {
                StatusDescription = String.Format(DatabaseEditorStrings.StatusStatistics, _SearchCount, _UpdateCount);
            }
        }

        /// <summary>
        /// Logs an exception and returns the message portion.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        internal string LogException(Exception ex, string logMessage)
        {
            var log = Factory.ResolveSingleton<ILog>();
            log.WriteLine(logMessage);

            return ex.Message;
        }

        /// <summary>
        /// Logs an exception and returns the message portion.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="formatLogMessage"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal string LogException(Exception ex, string formatLogMessage, params object[] args)
        {
            var logMessage = String.Format(formatLogMessage, args);
            return LogException(ex, logMessage);
        }

        /// <summary>
        /// Called whenever the web site fetches an HTML file from disk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSite_HtmlLoadedFromFile(object sender, TextContentEventArgs e)
        {
            var key = e.PathAndFile.ToLower();
            if(key.StartsWith("/databaseeditor/")) {
                e.Content = _HtmlLocaliser.Html(e.Content, e.Encoding);
            }
        }
    }
}
