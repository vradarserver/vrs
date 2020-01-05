// Copyright © 2010 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using System.Windows.Forms;
using System.Collections.Specialized;
using VirtualRadar.Interface.Owin;
using System.Web;
using Microsoft.Owin;
using System.Threading;
using AWhewell.Owin.Utility;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Implements <see cref="IWebSite"/>.
    /// </summary>
    class WebSite : IWebSite
    {
        #region Private class - SimpleRequest
        /// <summary>
        /// The fake request object used by <see cref="RequestSimpleContent"/>.
        /// </summary>
        class SimpleRequest : IRequest, IDisposable
        {
            private static readonly IPEndPoint DummyRemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 10000);

            private static readonly IPEndPoint DummyLocalEndPoint = new IPEndPoint(IPAddress.Loopback, 10001);

            private static readonly NameValueCollection DummyFormValues = new NameValueCollection();

            private static readonly NameValueCollection DummyHeaders = new NameValueCollection();

            private static readonly CookieCollection DummyCookies = new CookieCollection();

            public CookieCollection Cookies { get { return DummyCookies; } }

            public NameValueCollection FormValues { get { return DummyFormValues; } }

            public NameValueCollection Headers { get { return DummyHeaders; } }

            public string HttpMethod { get { return "GET"; } }

            private Stream _InputStream;
            public Stream InputStream
            {
                get {
                    if(_InputStream == null) _InputStream = new MemoryStream(new byte[0]);
                    return _InputStream;
                }
            }

            public bool IsLocal { get { return true; } }

            public int MaximumPostBodySize { get; set; }

            public string RawUrl { get; private set; }

            public IPEndPoint RemoteEndPoint { get { return DummyRemoteEndPoint; } }

            public IPEndPoint LocalEndPoint { get { return DummyLocalEndPoint; } }

            public Uri Url { get; private set; }

            public string UserAgent { get { return "FAKE REQUEST"; } }

            public string UserHostName { get { return "FAKE.HOST.NAME"; } }

            public bool IsValidCorsRequest { get; set; }

            public string CorsOrigin { get { return ""; } }

            public SimpleRequest(string root, string pathAndFile)
            {
                RawUrl = String.Format("{0}{1}", root, pathAndFile);
                Url = new Uri(String.Format("http://{0}{1}", RemoteEndPoint.Address, RawUrl));
            }

            ~SimpleRequest()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if(disposing) {
                    if(_InputStream != null) {
                        _InputStream.Dispose();
                        _InputStream = null;
                    }
                }
            }

            public string ReadBodyAsString(Encoding encoding)
            {
                return "";
            }
        }
        #endregion

        #region Private class - SimpleResponse
        /// <summary>
        /// The fake response object used by <see cref="RequestSimpleContent"/>.
        /// </summary>
        class SimpleResponse : IResponse
        {
            public CookieCollection Cookies { get; set; }
            public long ContentLength { get; set; }
            public string MimeType { get; set; }
            public Stream OutputStream { get; private set; }
            public HttpStatusCode StatusCode { get; set; }
            public Encoding ContentEncoding { get; set; }

            public SimpleResponse(Stream outputStream)
            {
                Cookies = new CookieCollection();
                OutputStream = outputStream;
                StatusCode = HttpStatusCode.NotFound;
            }

            public void Abort()                                 { ; }
            public void AddHeader(string name, string value)    { ; }
            public void Close()                                 { ; }
            public void Dispose()                               { ; }
            public void EnableCompression(IRequest request)     { ; }
            public void Redirect(string url)                    { ; }
            public void SetCookie(Cookie cookie)                { ; }
        }
        #endregion

        /// <summary>
        /// The object that synchronises writes to fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that will inject web site strings into the site for us.
        /// </summary>
        private WebSiteStringsManipulator _WebSiteStringsManipulator = new WebSiteStringsManipulator();

        /// <summary>
        /// The object that will inject content into HTML files for us.
        /// </summary>
        private HtmlManipulator _HtmlManipulator = new HtmlManipulator();

        /// <summary>
        /// The object that performs fetches of content without going through the web server.
        /// </summary>
        private ILoopbackHost _LoopbackHost;

        /// <summary>
        /// The currently configured authentication scheme.
        /// </summary>
        private AuthenticationSchemes _AuthenticationScheme;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabase BaseStationDatabase { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager StandingDataManager { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<TextContentEventArgs> HtmlLoadedFromFile;

        /// <summary>
        /// Raises <see cref="HtmlLoadedFromFile"/>.
        /// </summary>
        /// <param name="args"></param>
        internal void OnHtmlLoadedFromFile(TextContentEventArgs args)
        {
            EventHelper.RaiseQuickly(HtmlLoadedFromFile, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="server"></param>
        public void AttachSiteToServer(IWebServer server)
        {
            if(server == null) throw new ArgumentNullException("server");
            if(WebServer != server) {
                if(WebServer != null) {
                    throw new InvalidOperationException("The web site can only be attached to one server");
                }

                var pluginManager = Factory.ResolveSingleton<IPluginManager>();
                pluginManager.RegisterOwinMiddleware();

                var sharedConfig = Factory.ResolveSingleton<ISharedConfiguration>();
                sharedConfig.ConfigurationChanged += SharedConfiguration_ConfigurationChanged;

                WebServer = server;
                server.Root = "/VirtualRadar";

                var installerSettingsStorage = Factory.Resolve<IInstallerSettingsStorage>();
                var installerSettings = installerSettingsStorage.Load();
                server.Port = installerSettings.WebServerPort;

                var redirection = Factory.ResolveSingleton<IRedirectionConfiguration>();
                redirection.AddRedirection("/", "/desktop.html", RedirectionContext.Any);
                redirection.AddRedirection("/", "/mobile.html", RedirectionContext.Mobile);

                var fileServerConfiguration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
                fileServerConfiguration.TextLoadedFromFile += FileSystemConfiguration_TextLoadedFromFile;
                AddDefaultSiteRoot(fileServerConfiguration);

                var htmlManipulatorConfig = Factory.ResolveSingleton<IHtmlManipulatorConfiguration>();
                htmlManipulatorConfig.AddTextResponseManipulator(_HtmlManipulator);

                var javascriptManipulatorConfig = Factory.ResolveSingleton<IJavascriptManipulatorConfiguration>();
                javascriptManipulatorConfig.AddTextResponseManipulator(_WebSiteStringsManipulator);

                _LoopbackHost = Factory.Resolve<ILoopbackHost>();
                _LoopbackHost.ConfigureStandardPipeline();

                LoadConfiguration();
            }
        }

        private void AddDefaultSiteRoot(IFileSystemServerConfiguration fileServerConfiguration)
        {
            var runtime = Factory.ResolveSingleton<IRuntimeEnvironment>();
            var defaultSiteRoot = new SiteRoot() {
                Folder = String.Format("{0}{1}", Path.Combine(runtime.ExecutablePath, "Web"), Path.DirectorySeparatorChar),
                Priority = 0,
            };

            var checksumsFileName = Path.Combine(runtime.ExecutablePath, "Checksums.txt");
            if(!File.Exists(checksumsFileName)) {
                throw new FileNotFoundException($"Cannot find {checksumsFileName}");
            }
            defaultSiteRoot.Checksums.AddRange(ChecksumFile.Load(File.ReadAllText(checksumsFileName), enforceContentChecksum: true));

            fileServerConfiguration.AddSiteRoot(defaultSiteRoot);
        }

        /// <summary>
        /// Loads and applies the configuration from disk.
        /// </summary>
        /// <returns>True if the server should be restarted because of changes to the configuration.</returns>
        private bool LoadConfiguration()
        {
            var configuration = Factory.ResolveSingleton<ISharedConfiguration>().Get();

            var result = false;
            lock(_SyncLock) {
                if(_AuthenticationScheme != configuration.WebServerSettings.AuthenticationScheme) {
                    result = true;
                    _AuthenticationScheme = configuration.WebServerSettings.AuthenticationScheme;
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void AddSiteRoot(SiteRoot siteRoot)
        {
            var configuration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
            configuration.AddSiteRoot(siteRoot);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void RemoveSiteRoot(SiteRoot siteRoot)
        {
            var configuration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
            configuration.RemoveSiteRoot(siteRoot);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        /// <param name="folderMustMatch"></param>
        /// <returns></returns>
        public bool IsSiteRootActive(SiteRoot siteRoot, bool folderMustMatch)
        {
            var configuration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
            return configuration.IsSiteRootActive(siteRoot, folderMustMatch);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSiteRootFolders()
        {
            var configuration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
            return configuration.GetSiteRootFolders();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="args"></param>
        public void RequestContent(RequestReceivedEventArgs args)
        {
            if(args == null) throw new ArgumentNullException("args");

            var owinEnvironment = ConvertEventArgsIntoOwinEnvironment(args);
            var simpleContent = _LoopbackHost?.SendSimpleRequest(args.Request.RawUrl, owinEnvironment);

            args.Response.StatusCode = simpleContent.HttpStatusCode;
            args.Response.ContentLength = simpleContent.Content.Length;
            args.Response.OutputStream.Write(simpleContent.Content, 0, simpleContent.Content.Length);
        }

        private IDictionary<string, object> ConvertEventArgsIntoOwinEnvironment(RequestReceivedEventArgs args)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var queryString = args.Request.Url?.Query;
            if(queryString?.Length > 0) {
                queryString = queryString.Substring(1);
            }

            var context = AWhewell.Owin.Utility.OwinContext.Create(result);
            context.CallCancelled =         new CancellationToken();
            context.Version =               "1.0.0";
            context.RequestHeaders =        new RequestHeadersDictionary() {
                                                ["Host"] = args.Request.Url.Authority,
                                            };
            context.RequestBody =           Stream.Null;
            context.RequestMethod =         "GET";
            context.RequestScheme =         "http";
            context.RequestPath =           args.PathAndFile;
            context.RequestPathBase =       args.Root;
            context.RequestProtocol =       Formatter.FormatHttpProtocol(HttpProtocol.Http1_1);
            context.RequestQueryString =    queryString;
            context.ServerRemoteIpAddress = args.Request.RemoteEndPoint.Address.ToString();
            context.ServerRemotePort =      args.Request.RemoteEndPoint.Port.ToString();

            context.ResponseHeaders =       new ResponseHeadersDictionary();
            context.ResponseBody =          new MemoryStream();

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        public SimpleContent RequestSimpleContent(string pathAndFile)
        {
            if(pathAndFile == null) throw new ArgumentNullException("pathAndFile");
            var result = new SimpleContent();

            const string root = "/Root";
            using(var simpleRequest = new SimpleRequest(root, pathAndFile)) {
                using(var memoryStream = new MemoryStream()) {
                    var simpleResponse = new SimpleResponse(memoryStream);
                    var args = new RequestReceivedEventArgs(simpleRequest, simpleResponse, root);
                    RequestContent(args);

                    result.HttpStatusCode = simpleResponse.StatusCode;
                    result.Content = memoryStream.ToArray();
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void AddHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            _HtmlManipulator.AddHtmlContentInjector(contentInjector);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void RemoveHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            _HtmlManipulator.RemoveHtmlContentInjector(contentInjector);
        }

        /// <summary>
        /// Handles changes to the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SharedConfiguration_ConfigurationChanged(object sender, EventArgs args)
        {
            if(WebServer != null && LoadConfiguration() && WebServer.Online) {
                WebServer.Online = false;
                WebServer.Online = true;
            }
        }

        /// <summary>
        /// Raised when the OWIN file system server is about to serve a text file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FileSystemConfiguration_TextLoadedFromFile(object sender, TextContentEventArgs args)
        {
            if(args.MimeType == MimeType.Html) {
                OnHtmlLoadedFromFile(args);
            }
        }
    }
}
