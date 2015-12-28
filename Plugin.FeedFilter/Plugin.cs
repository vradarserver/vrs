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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.Plugin.FeedFilter.Json;

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// The entry point for the plugin that can filter aircraft out of the feeds.
    /// </summary>
    public class Plugin : IPlugin
    {
        #region Fields
        /// <summary>
        /// The plugin's protected folder.
        /// </summary>
        private static readonly string ProtectedFolder = "FeedFilter";

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
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id { get { return "VirtualRadarServer.Plugin.FeedFilter"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return FeedFilterStrings.PluginName; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.3.0"; } }

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
        #endregion

        #region Plugin methods
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            Filter.Initialise(this);

            OriginalImplementationFactory.RecordCurrentImplementation<IListener>();
            classFactory.Register<IListener, ListenerWrapper>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            var options = OptionsStorage.Load(this);

            _HtmlLocaliser = Factory.Singleton.Resolve<IHtmlLocaliser>();
            _HtmlLocaliser.Initialise();
            _HtmlLocaliser.AddResourceStrings(typeof(FeedFilterStrings));

            _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = false;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.PageHandlers.Add(String.Format("/{0}/FetchFilterConfiguration.json", ProtectedFolder), FetchFilterConfiguration);
            _WebSiteExtender.PageHandlers.Add(String.Format("/{0}/SaveFilterConfiguration.json", ProtectedFolder), SaveFilterConfiguration);
            _WebSiteExtender.Initialise(parameters);

            parameters.WebSite.HtmlLoadedFromFile += WebSite_HtmlLoadedFromFile;

            ApplyOptions(options);
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
                var webServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton.WebServer;

                dialog.Options = OptionsStorage.Load(this);
                dialog.FilterSettingsUrl = String.Format("{0}/FeedFilter/index.html", webServer.LocalAddress);

                if(dialog.ShowDialog() == DialogResult.OK) {
                    OptionsStorage.Save(this, dialog.Options);
                    ApplyOptions(dialog.Options);
                }
            }
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
            _WebSiteExtender.ProtectFolder(ProtectedFolder);

            if(!options.Enabled) {
                Status = FeedFilterStrings.StatusDisabled;
            } else {
                Status = FeedFilterStrings.StatusEnabled;
            }
        }
        #endregion

        #region JSON page handlers - FetchFilterConfiguration, SaveFilterConfiguration
        /// <summary>
        /// Returns the current filter settings.
        /// </summary>
        /// <param name="args"></param>
        private void FetchFilterConfiguration(RequestReceivedEventArgs args)
        {
            if(args.Request.HttpMethod == "GET") {
                var json = new FilterConfigurationJson();

                try {
                    json.FromFilterConfiguration(FilterConfigurationStorage.Load());
                } catch(Exception ex) {
                    json.Exception = LogException(ex, "Exception caught during FeedFilter FetchFilterConfiguration: {0}", ex.ToString());
                }

                SendJsonResponse(args, json);
            }
        }

        /// <summary>
        /// Saves the settings passed across and returns the saved values.
        /// </summary>
        /// <param name="args"></param>
        private void SaveFilterConfiguration(RequestReceivedEventArgs args)
        {
            if(args.Request.HttpMethod == "POST") {
                var json = new SaveFilterConfigurationJson();

                try {
                    json.DataVersion = long.Parse(args.Request.FormValues["DataVersion"], CultureInfo.InvariantCulture);
                    json.ProhibitMlat = bool.Parse(args.Request.FormValues["ProhibitMlat"]);
                    json.ProhibitIcaos = bool.Parse(args.Request.FormValues["ProhibitIcaos"]);
                    json.Icaos = args.Request.FormValues["Icaos"];

                    var filterConfiguration = json.ToFilterConfiguration(json.DuplicateIcaos, json.InvalidIcaos);

                    json.WasStaleData = !FilterConfigurationStorage.Save(this, filterConfiguration);
                    if(!json.WasStaleData) {
                        json.FromFilterConfiguration(filterConfiguration);
                    }
                } catch(Exception ex) {
                    json.Exception = LogException(ex, "Exception caught during FeedFilter SaveFilterConfiguration: {0}", ex.ToString());
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
            if(key.StartsWith("/feedfilter/")) {
                e.Content = _HtmlLocaliser.Html(e.Content, e.Encoding);
            }
        }
        #endregion
    }
}
