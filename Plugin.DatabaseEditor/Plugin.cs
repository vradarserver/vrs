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
using System.Web;
using System.Windows.Forms;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.Plugin.DatabaseEditor.Json;

namespace VirtualRadar.Plugin.DatabaseEditor
{
    /// <summary>
    /// The entry point for the plugin that adds web pages that let administrator users
    /// edit aircraft records.
    /// </summary>
    public class Plugin : IPlugin
    {
        #region Fields
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
        /// The object that handles access to the database.
        /// </summary>
        private IBaseStationDatabase _BaseStationDatabase;

        /// <summary>
        /// The number of searches made through the plugin.
        /// </summary>
        private int _SearchCount;

        /// <summary>
        /// The number of updates made through the plugin.
        /// </summary>
        private int _UpdateCount;
        #endregion

        #region Properties
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
        public string Version { get { return "2.4.0"; } }

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
        #endregion

        #region Plugin methods
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

            _HtmlLocaliser = Factory.Singleton.Resolve<IHtmlLocaliser>();
            _HtmlLocaliser.Initialise();
            _HtmlLocaliser.AddResourceStrings(typeof(DatabaseEditorStrings));

            _BaseStationDatabase = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
            _BaseStationDatabase.WriteSupportEnabled = true;

            _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = false;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.InjectContent = @"<script src=""script-DatabaseEditor/inject.js"" type=""text/javascript"">";
            _WebSiteExtender.InjectMapPages();
            _WebSiteExtender.InjectReportPages();
            _WebSiteExtender.PageHandlers.Add(String.Format("/{0}/SingleAircraftSearch.json", ProtectedFolder), SingleAircraftSearch);
            _WebSiteExtender.PageHandlers.Add(String.Format("/{0}/SingleAircraftSave.json", ProtectedFolder), SingleAircraftSave);
            _WebSiteExtender.Initialise(parameters);
            _WebSiteExtender.ProtectFolder(ProtectedFolder);

            parameters.WebSite.HtmlLoadedFromFile += WebSite_HtmlLoadedFromFile;

            ApplyOptions(options);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.Singleton.Resolve<IWebAdminViewManager>().Singleton;
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
            var webServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton.WebServer;
            return String.Format("{0}/{1}", webServer.LocalAddress, "DatabaseEditor/index.html");
        }
        #endregion

        #region ApplyOptions
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
        private void UpdateStatusTotals()
        {
            using(new CultureSwitcher()) {
                StatusDescription = String.Format(DatabaseEditorStrings.StatusStatistics, _SearchCount, _UpdateCount);
            }
        }
        #endregion

        #region JSON page handlers - SingleAircraftSearch, SingleAircraftSave
        /// <summary>
        /// Handles searches for a single aircraft.
        /// </summary>
        /// <param name="args"></param>
        private void SingleAircraftSearch(RequestReceivedEventArgs args)
        {
            if(args.Request.HttpMethod == "GET") {
                var json = new SingleSearchResultsJson();
                string icao = null;

                try {
                    icao = (args.QueryString["icao"] ?? "").ToUpper();
                    if(icao != "") {
                        json.Aircraft = _BaseStationDatabase.GetAircraftByCode(icao);
                        ++_SearchCount;
                        UpdateStatusTotals();
                    }
                } catch(Exception ex) {
                    json.Exception = LogException(ex, "Exception caught during DatabaseEditor SingleAircraftSearch ({0}): {1}", icao, ex.ToString());
                }

                SendJsonResponse(args, json);
            }
        }

        /// <summary>
        /// Handles requests to save a single aircraft's data.
        /// </summary>
        /// <param name="args"></param>
        private void SingleAircraftSave(RequestReceivedEventArgs args)
        {
            if(args.Request.HttpMethod == "POST") {
                var json = new SingleAircraftSaveResultsJson();

                try {
                    var aircraftJson = args.Request.ReadBodyAsString(Encoding.UTF8);
                    json.Aircraft = JsonConvert.DeserializeObject<BaseStationAircraft>(aircraftJson);

                    if(json.Aircraft.ModeS != null && json.Aircraft.ModeS.Length == 6) {
                        if(json.Aircraft.Registration != null)      json.Aircraft.Registration =     json.Aircraft.Registration.ToUpper();
                        if(json.Aircraft.ICAOTypeCode != null)      json.Aircraft.ICAOTypeCode =     json.Aircraft.ICAOTypeCode.ToUpper();
                        if(json.Aircraft.OperatorFlagCode != null)  json.Aircraft.OperatorFlagCode = json.Aircraft.OperatorFlagCode.ToUpper();
                        json.Aircraft.LastModified = DateTime.Now;

                        if(json.Aircraft.UserString1 == "Missing") {
                            json.Aircraft.UserString1 = null;
                        }

                        if(json.Aircraft.AircraftID == 0) {
                            _BaseStationDatabase.InsertAircraft(json.Aircraft);
                        } else {
                            _BaseStationDatabase.UpdateAircraft(json.Aircraft);
                        }

                        ++_UpdateCount;
                        UpdateStatusTotals();
                    }
                } catch(Exception ex) {
                    var aircraftID = json.Aircraft == null ? "<no aircraft>" : json.Aircraft.AircraftID.ToString();
                    var icao = json.Aircraft == null ? "<no aircraft>" : json.Aircraft.ModeS;
                    json.Exception = LogException(ex, "Exception caught during DatabaseEditor SingleAircraftSave ({0}/{1}): {2}", aircraftID, icao, ex.ToString());
                }

                SendJsonResponse(args, json);
            }
        }

        /// <summary>
        /// Logs an exception and returns the message portion.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string LogException(Exception ex, string logMessage)
        {
            var log = Factory.Singleton.Resolve<ILog>().Singleton;
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
        private string LogException(Exception ex, string formatLogMessage, params object[] args)
        {
            var logMessage = String.Format(formatLogMessage, args);
            return LogException(ex, logMessage);
        }

        /// <summary>
        /// Sends the object passed across as a JSON response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="json"></param>
        private void SendJsonResponse<T>(RequestReceivedEventArgs args, T json)
        {
            var jsonText = JsonConvert.SerializeObject(json);
            var responder = Factory.Singleton.Resolve<IResponder>();
            responder.SendText(args.Request, args.Response, jsonText, Encoding.UTF8, MimeType.Json);
            args.Handled = true;
        }
        #endregion

        #region Events subscribed
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
        #endregion
    }
}
