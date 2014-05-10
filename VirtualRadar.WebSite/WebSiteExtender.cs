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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using System.IO;
using InterfaceFactory;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IWebSiteExtender"/>.
    /// </summary>
    class WebSiteExtender : IWebSiteExtender
    {
        #region Fields
        /// <summary>
        /// The web site that we are extending.
        /// </summary>
        private IWebSite _WebSite;

        /// <summary>
        /// The web server we've hooked.
        /// </summary>
        private IWebServer _WebServer;

        /// <summary>
        /// The site root to add to the web site, if any.
        /// </summary>
        private SiteRoot _SiteRoot;

        /// <summary>
        /// The content injectors to add to the web site, if any.
        /// </summary>
        private List<HtmlContentInjector> _ContentInjectors = new List<HtmlContentInjector>();

        /// <summary>
        /// The page handlers to add to the web site, if any.
        /// </summary>
        private Dictionary<string, Action<RequestReceivedEventArgs>> _PageHandlers = new Dictionary<string,Action<RequestReceivedEventArgs>>();
        #endregion

        #region Properties
        private bool _Enabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if(_Enabled != value) {
                    _Enabled = value;
                    EnableDisableContent();
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebRootSubFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string InjectContent { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<string> InjectPages { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDictionary<string, Action<RequestReceivedEventArgs>> PageHandlers { get; private set; }
        #endregion

        #region Ctors and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSiteExtender()
        {
            InjectPages = new List<string>();
            PageHandlers = new Dictionary<string, Action<RequestReceivedEventArgs>>();
            Priority = 100;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~WebSiteExtender()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Enabled = false;
                if(_WebServer != null) {
                    _WebServer.BeforeRequestReceived -= WebServer_BeforeRequestReceived;
                    _WebServer = null;
                }
                _WebSite = null;
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pluginStartupParameters"></param>
        public void Initialise(PluginStartupParameters pluginStartupParameters)
        {
            _WebSite = pluginStartupParameters.WebSite;

            if(!String.IsNullOrEmpty(WebRootSubFolder)) {
                _SiteRoot = new SiteRoot() {
                    Folder = Path.Combine(pluginStartupParameters.PluginFolder, WebRootSubFolder),
                    Priority = Priority,
                };
            }

            if(!String.IsNullOrEmpty(InjectContent)) {
                var injectContent = InjectContent;
                Func<string, HtmlContentInjector> createInjector = (string pathAndFile) => new HtmlContentInjector() {
                    Content = () => injectContent,
                    Element = "HEAD",
                    PathAndFile = pathAndFile,
                    Priority = Priority,
                };
                if(InjectPages.Count == 0) {
                    _ContentInjectors.Add(createInjector(null));
                } else {
                    foreach(var injectPage in InjectPages) {
                        _ContentInjectors.Add(createInjector(injectPage));
                    }
                }
            }

            if(PageHandlers.Count > 0) {
                _WebServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton.WebServer;
                _WebServer.BeforeRequestReceived += WebServer_BeforeRequestReceived;

                foreach(var pageHandler in PageHandlers) {
                    _PageHandlers.Add(pageHandler.Key, pageHandler.Value);
                }
            }

            EnableDisableContent();
        }
        #endregion

        #region InjectMapPages, InjectReportPages
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IWebSiteExtender InjectMapPages()
        {
            InjectPages.Add("/desktop.html");
            InjectPages.Add("/mobile.html");

            return this;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IWebSiteExtender InjectReportPages()
        {
            InjectPages.Add("/desktopReport.html");
            InjectPages.Add("/mobileReport.html");

            return this;
        }
        #endregion

        #region EnableDisableContent
        /// <summary>
        /// Adds or removes content to the web site.
        /// </summary>
        private void EnableDisableContent()
        {
            if(_WebSite != null) {
                if(!Enabled) {
                    if(_SiteRoot != null) _WebSite.RemoveSiteRoot(_SiteRoot);
                    foreach(var contentInjector in _ContentInjectors) {
                        _WebSite.RemoveHtmlContentInjector(contentInjector);
                    }
                } else {
                    if(_SiteRoot != null) _WebSite.AddSiteRoot(_SiteRoot);
                    foreach(var contentInjector in _ContentInjectors) {
                        _WebSite.AddHtmlContentInjector(contentInjector);
                    }
                }
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called when the web server receives a request if there are page handlers that need to be serviced.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebServer_BeforeRequestReceived(object sender, RequestReceivedEventArgs args)
        {
            if(Enabled) {
                foreach(var pageHandler in _PageHandlers) {
                    if(pageHandler.Key.Equals(args.PathAndFile, StringComparison.OrdinalIgnoreCase)) pageHandler.Value(args);
                }
            }
        }
        #endregion
    }
}
