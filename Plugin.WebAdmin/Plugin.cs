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
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;
using VirtualRadar.Plugin.WebAdmin.View;

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

        /// <summary>
        /// A localiser that can substitute strings into HTML.
        /// </summary>
        private IHtmlLocaliser _HtmlLocaliser;

        /// <summary>
        /// A map from a server path and file (in lower-case) to the <see cref="ViewMap"/> representing a view.
        /// </summary>
        private Dictionary<string, ViewMap> _PathAndFileViewMap = new Dictionary<string,ViewMap>();

        /// <summary>
        /// The full path to the head template file.
        /// </summary>
        private string _HeadTemplateFileName;
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
        public string Name { get { return WebAdminStrings.PluginName; } }

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
            EventHelper.Raise(StatusChanged, this, args);
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

            _HtmlLocaliser = Factory.Singleton.Resolve<IHtmlLocaliser>();
            _HtmlLocaliser.Initialise();
            _HtmlLocaliser.AddResourceStrings(typeof(WebAdminStrings));

            _HeadTemplateFileName = Path.Combine(parameters.PluginFolder, "Web");
            _HeadTemplateFileName = Path.Combine(_HeadTemplateFileName, "WebAdmin");
            _HeadTemplateFileName = Path.Combine(_HeadTemplateFileName, "templates");
            _HeadTemplateFileName = Path.Combine(_HeadTemplateFileName, "head.html");

            AddView("Index.html", new View.MainView(parameters.UPnpManager, parameters.FlightSimulatorAircraftList));
            AddView("Options.html", new View.SettingsView());
            AddView("Log.html", new View.LogView());
            AddView("About.html", new View.AboutView());

            _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = _Options.Enabled;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.PageHandlers.Add("/WebAdmin/webAdminStrings.js", WebAdminStringsJavaScript.SendJavaScript);
            AddJsonHandlers();
            _WebSiteExtender.Initialise(parameters);
            _WebSiteExtender.ProtectFolder("WebAdmin");

            parameters.WebSite.HtmlLoadedFromFile += WebSite_HtmlLoadedFromFile;

            ApplyOptions();
        }

        private void AddView(string htmlFileName, BaseView view)
        {
            var viewMap = new ViewMap("/WebAdmin", htmlFileName, view);
            _PathAndFileViewMap.Add(viewMap.ViewPathAndFile.ToLower(), viewMap);
        }

        private void AddJsonHandlers()
        {
            foreach(var viewMap in _PathAndFileViewMap.Values) {
                _WebSiteExtender.PageHandlers.Add(viewMap.ViewDataPathAndFile, viewMap.View.SendData);
            }
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
            _WebSiteExtender.Enabled = _Options.Enabled;
            OnStatusChanged(EventArgs.Empty);
        }
        #endregion

        #region ExpandTemplateMarkerFromFile
        private string ExpandTemplateMarkerFromFile(string html, string templateMarker, string contentFileName)
        {
            if(html.IndexOf(templateMarker) != -1) {
                var templateContent = File.ReadAllText(contentFileName);
                html = html.Replace(templateMarker, templateContent);
            }

            return html;
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
            if(key.StartsWith("/webadmin/")) {
                ViewMap viewMap;
                if(_PathAndFileViewMap.TryGetValue(key, out viewMap)) {
                    // Subtitute simple strings
                    e.Content = _HtmlLocaliser.Html(e.Content, e.Encoding);

                    // Substitute in the content of the head template file
                    e.Content = ExpandTemplateMarkerFromFile(e.Content, "@head.html@", _HeadTemplateFileName);

                    // Ensure that there is a presenter up and running for the page
                    viewMap.View.ShowView();
                }
            }
        }
        #endregion
    }
}
