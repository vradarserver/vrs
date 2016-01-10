// Copyright © 2016 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// The plugin's implementation of the view manager interface.
    /// </summary>
    class WebAdminViewManager : IWebAdminViewManager
    {
        /// <summary>
        /// The web site that the manager has hooked itself into.
        /// </summary>
        private IWebSite _WebSite;

        /// <summary>
        /// The protected folder. All views should live under this folder somewhere.
        /// </summary>
        private string _ProtectedFolder;

        /// <summary>
        /// A list of registered full paths to web admin views.
        /// </summary>
        private HashSet<string> _RegisteredWebAdminFolders;

        /// <summary>
        /// A dictionary of registered web admin views indexed by the normalised path and view.
        /// </summary>
        private Dictionary<string, WebAdminView> _WebAdminViewMapByFullPath;

        /// <summary>
        /// The object that will map view methods for us.
        /// </summary>
        private ViewMethodMapper _ViewMethodMapper;

        /// <summary>
        /// A map of resource types to HTML localisers.
        /// </summary>
        private Dictionary<Type, IHtmlLocaliser> _HtmlLocalisers;

        /// <summary>
        /// The site roots for every registered web admin folder.
        /// </summary>
        private List<SiteRoot> _SiteRoots;

        /// <summary>
        /// A map of template markers to the HTML files to substitute into their place in HTML files.
        /// </summary>
        private Dictionary<string, string> _TemplateMarkerFileNames;

        /// <summary>
        /// The lock object that controls access to the fields.
        /// </summary>
        private object _SyncLock = new object();

        private static WebAdminViewManager _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebAdminViewManager Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new WebAdminViewManager();
                }
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WebAdminPluginInstalled
        {
            get { return true; }
        }

        private bool _Enabled;
        /// <summary>
        /// Gets or sets a value indicating that the view manager is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set {
                if(_Enabled != value) {
                    lock(_SyncLock) {
                        if(_Enabled != value) {
                            _Enabled = value;
                            EnableDisableSiteRoots();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialises the object.
        /// </summary>
        /// <param name="protectedFolder"></param>
        public void Initialise(string protectedFolder)
        {
            if(_RegisteredWebAdminFolders != null) {
                throw new InvalidOperationException("Only initialise the WebAdminViewManager once");
            }

            _HtmlLocalisers = new Dictionary<Type,IHtmlLocaliser>();
            _ProtectedFolder = protectedFolder;
            _RegisteredWebAdminFolders = new HashSet<string>();
            _SiteRoots = new List<SiteRoot>();
            _TemplateMarkerFileNames = new Dictionary<string,string>();
            _ViewMethodMapper = new ViewMethodMapper();
            _WebAdminViewMapByFullPath = new Dictionary<string,WebAdminView>();
        }

        /// <summary>
        /// Hooks the manager into the web site.
        /// </summary>
        /// <param name="webSite"></param>
        public void Startup(IWebSite webSite)
        {
            if(_WebSite != null) throw new InvalidOperationException("Only call Startup once");

            lock(_SyncLock) {
                _WebSite = webSite;
                _WebSite.WebServer.RequestReceived += WebServer_RequestReceived;
                _WebSite.HtmlLoadedFromFile += WebSite_HtmlLoadedFromFile;

                EnableDisableSiteRoots();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pluginFolder"></param>
        /// <param name="subFolder"></param>
        public void RegisterWebAdminViewFolder(string pluginFolder, string subFolder)
        {
            if(String.IsNullOrEmpty(pluginFolder)) throw new ArgumentException("The plugin folder or the parent folder for the sub folder must be supplied");
            if(String.IsNullOrEmpty(subFolder)) throw new ArgumentException("The sub folder must be supplied");

            var path = Path.GetFullPath(Path.Combine(pluginFolder, subFolder));
            if(!Directory.Exists(path)) {
                throw new InvalidOperationException(String.Format("{0} does not exist", path));
            }

            var normalisedPath = NormaliseFullPath(path, appendTrailingSlash: true);
            lock(_SyncLock) {
                if(!_RegisteredWebAdminFolders.Contains(normalisedPath)) {
                    var newSet = CollectionHelper.ShallowCopy(_RegisteredWebAdminFolders);
                    newSet.Add(normalisedPath);
                    var siteRoot = new SiteRoot() {
                        Folder = path,
                    };
                    _RegisteredWebAdminFolders = newSet;

                    _SiteRoots.Add(siteRoot);
                    if(Enabled && _WebSite != null) {
                        EnableDisableSiteRoot(siteRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Registers or deregisters every site root with the web site.
        /// </summary>
        private void EnableDisableSiteRoots()
        {
            lock(_SyncLock) {
                if(_WebSite != null) {
                    foreach(var siteRoot in _SiteRoots) {
                        EnableDisableSiteRoot(siteRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Registers or deregisters a single site root with the web site. Call within a lock.
        /// </summary>
        /// <param name="siteRoot"></param>
        private void EnableDisableSiteRoot(SiteRoot siteRoot)
        {
            if(_WebSite != null) {
                if(_Enabled) {
                    _WebSite.AddSiteRoot(siteRoot);
                } else {
                    _WebSite.RemoveSiteRoot(siteRoot);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webAdminView"></param>
        public void AddWebAdminView(WebAdminView webAdminView)
        {
            var normalisedPathAndFile = NormaliseFullPath(webAdminView.PathAndFile);
            if(!normalisedPathAndFile.StartsWith(String.Format("/{0}/", NormaliseFullPath(_ProtectedFolder)))) {
                throw new InvalidOperationException(String.Format("{0} is not under {1}", webAdminView.PathAndFile, _ProtectedFolder));
            }

            lock(_SyncLock) {
                if(!_WebAdminViewMapByFullPath.ContainsKey(normalisedPathAndFile)) {
                    var newMap = CollectionHelper.ShallowCopy(_WebAdminViewMapByFullPath);
                    newMap.Add(normalisedPathAndFile, webAdminView);
                    _WebAdminViewMapByFullPath = newMap;
                }

                if(webAdminView.StringResources != null) {
                    if(!_HtmlLocalisers.ContainsKey(webAdminView.StringResources)) {
                        var htmlLocaliser = Factory.Singleton.Resolve<IHtmlLocaliser>();
                        htmlLocaliser.Initialise();
                        htmlLocaliser.AddResourceStrings(webAdminView.StringResources);

                        var newMap = CollectionHelper.ShallowCopy(_HtmlLocalisers);
                        newMap.Add(webAdminView.StringResources, htmlLocaliser);
                        _HtmlLocalisers = newMap;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="templateMarker"></param>
        /// <param name="templateHtmlFullPath"></param>
        public void RegisterTemplateFileName(string templateMarker, string templateHtmlFullPath)
        {
            if(String.IsNullOrEmpty(templateMarker)) throw new ArgumentNullException("templateMarker");
            if(String.IsNullOrEmpty(templateHtmlFullPath)) throw new ArgumentNullException("templateHtmlFullPath");
            if(!File.Exists(templateHtmlFullPath)) throw new InvalidOperationException(String.Format("{0} does not exist", templateHtmlFullPath));

            lock(_SyncLock) {
                if(!_TemplateMarkerFileNames.ContainsKey(templateMarker)) {
                    var newMap = CollectionHelper.ShallowCopy(_TemplateMarkerFileNames);
                    newMap.Add(templateMarker, templateHtmlFullPath);
                    _TemplateMarkerFileNames = newMap;
                }
            }
        }

        /// <summary>
        /// Returns a normalised path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="appendTrailingSlash"></param>
        /// <returns></returns>
        private string NormaliseFullPath(string fullPath, bool appendTrailingSlash = false)
        {
            var result = String.IsNullOrEmpty(fullPath) ? "" : fullPath.ToLower();
            if(appendTrailingSlash && (result.Length == 0 || result[result.Length - 1] != '/')) {
                result = String.Format("{0}/", result);
            }

            return result;
        }

        /// <summary>
        /// Replaces the template marker in the HTML with the content of the HTML file passed across.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="templateMarker"></param>
        /// <param name="contentFileName"></param>
        /// <returns></returns>
        private string ExpandTemplateMarkerFromFile(string html, string templateMarker, string contentFileName)
        {
            if(html.IndexOf(templateMarker) != -1) {
                var templateContent = File.ReadAllText(contentFileName);
                html = html.Replace(templateMarker, templateContent);
            }

            return html;
        }

        /// <summary>
        /// Called whenever the website loads HTML from disk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebSite_HtmlLoadedFromFile(object sender, TextContentEventArgs args)
        {
            var normalisedPathAndFile = NormaliseFullPath(args.PathAndFile);

            var htmlLocalisers = _HtmlLocalisers;
            var webAdminViewMap = _WebAdminViewMapByFullPath;
            var templateMarkerFileNames = _TemplateMarkerFileNames;

            if(htmlLocalisers != null && webAdminViewMap != null && templateMarkerFileNames != null) {
                WebAdminView webAdminView;
                if(webAdminViewMap.TryGetValue(normalisedPathAndFile, out webAdminView)) {
                    IHtmlLocaliser htmlLocaliser;
                    if(webAdminView.StringResources != null && htmlLocalisers.TryGetValue(webAdminView.StringResources, out htmlLocaliser)) {
                        args.Content = htmlLocaliser.Html(args.Content, args.Encoding);
                    }

                    foreach(var kvp in templateMarkerFileNames) {
                        args.Content = ExpandTemplateMarkerFromFile(args.Content, kvp.Key, kvp.Value);
                    }

                    _ViewMethodMapper.ViewRequested(webAdminView);
                }
            }
        }

        /// <summary>
        /// Called whenever the web server receives a request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebServer_RequestReceived(object sender, RequestReceivedEventArgs args)
        {
            if(Enabled && _ViewMethodMapper.ProcessJsonRequest(args)) {
                args.Handled = true;
            }
        }
    }
}
