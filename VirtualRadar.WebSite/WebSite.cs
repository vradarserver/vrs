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

        #region Private class - CachedUser
        /// <summary>
        /// The cached information about a valid user.
        /// </summary>
        class CachedUser
        {
            public IUser User;

            public string KnownGoodPassword;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that synchronises threads that are performing authentication tasks for the site.
        /// </summary>
        private object _AuthenticationSyncLock = new object();

        /// <summary>
        /// A map of every user that is allowed to view the web site, indexed by login name.
        /// </summary>
        Dictionary<string, CachedUser> _BasicAuthenticationUsers = new Dictionary<string,CachedUser>();

        /// <summary>
        /// A map of all administrator users indexed by login name.
        /// </summary>
        Dictionary<string, CachedUser> _AdministratorUsers = new Dictionary<string,CachedUser>();

        /// <summary>
        /// The object that will inject web site strings into the site for us.
        /// </summary>
        private WebSiteStringsManipulator _WebSiteStringsManipulator = new WebSiteStringsManipulator();

        /// <summary>
        /// A reference to the singleton user manager - saves us having to keep reloading it.
        /// </summary>
        private IUserManager _UserManager;

        /// <summary>
        /// The type of proxy that the server is sitting behind.
        /// </summary>
        private ProxyType _ProxyType;

        /// <summary>
        /// The lock object that protects <see cref="_HtmlContentInjectors"/>. Do not call event handlers
        /// with this locked.
        /// </summary>
        private object _HtmlContentInjectorsLock = new object();

        /// <summary>
        /// The list of content injectors.
        /// </summary>
        private List<HtmlContentInjector> _HtmlContentInjectors = new List<HtmlContentInjector>();

        /// <summary>
        /// A list of objects that can supply content for us.
        /// </summary>
        private List<Page> _Pages = new List<Page>();

        /// <summary>
        /// The page that handles requests for report rows.
        /// </summary>
        private ReportRowsJsonPage _ReportRowsJsonPage;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebSiteProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFlightSimulatorAircraftList FlightSimulatorAircraftList
        {
            get { return Factory.Singleton.ResolveSingleton<IFlightSimulatorAircraftList>(); }
            set { ; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabase BaseStationDatabase
        {
            get { return _ReportRowsJsonPage.BaseStationDatabase; }
            set { _ReportRowsJsonPage.BaseStationDatabase = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager StandingDataManager
        {
            get { return _ReportRowsJsonPage.StandingDataManager; }
            set { _ReportRowsJsonPage.StandingDataManager = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; private set; }
        #endregion

        #region Events exposed
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
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSite()
        {
            Provider = Factory.Singleton.Resolve<IWebSiteProvider>();

            _ReportRowsJsonPage = new ReportRowsJsonPage(this);
        }
        #endregion

        #region AttachSiteToServer, LoadConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="server"></param>
        public void AttachSiteToServer(IWebServer server)
        {
            if(server == null) throw new ArgumentNullException("server");
            if(WebServer != server) {
                if(WebServer != null) throw new InvalidOperationException("The web site can only be attached to one server");

                _UserManager = Factory.Singleton.Resolve<IUserManager>().Singleton;

                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

                WebServer = server;
                server.Root = "/VirtualRadar";

                var installerSettingsStorage = Factory.Singleton.Resolve<IInstallerSettingsStorage>();
                var installerSettings = installerSettingsStorage.Load();
                server.Port = installerSettings.WebServerPort;

                server.AuthenticationRequired += Server_AuthenticationRequired;

                var redirection = Factory.Singleton.Resolve<IRedirectionConfiguration>().Singleton;
                redirection.AddRedirection("/", "/desktop.html", RedirectionContext.Any);
                redirection.AddRedirection("/", "/mobile.html", RedirectionContext.Mobile);

                _Pages.Add(new TextPage(this));
                _Pages.Add(_ReportRowsJsonPage);

                var fileSystemConfiguration = Factory.Singleton.Resolve<IFileSystemServerConfiguration>().Singleton;
                fileSystemConfiguration.TextLoadedFromFile += FileSystemConfiguration_TextLoadedFromFile;

                var javascriptManipulatorConfig = Factory.Singleton.ResolveSingleton<IJavascriptManipulatorConfiguration>();
                javascriptManipulatorConfig.AddTextResponseManipulator(_WebSiteStringsManipulator);

                foreach(var page in _Pages) {
                    page.Provider = Provider;
                }

                LoadConfiguration();

                server.RequestReceived += Server_RequestReceived;
            }
        }

        /// <summary>
        /// Loads and applies the configuration from disk.
        /// </summary>
        /// <returns>True if the server should be restarted because of changes to the configuration.</returns>
        private bool LoadConfiguration()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

            bool result = false;
            lock(_AuthenticationSyncLock) {
                _AdministratorUsers.Clear();
                _BasicAuthenticationUsers.Clear();

                CacheUsers(_BasicAuthenticationUsers, configuration.WebServerSettings.BasicAuthenticationUserIds);
                CacheUsers(_AdministratorUsers, configuration.WebServerSettings.AdministratorUserIds);

                if(WebServer.AuthenticationScheme != configuration.WebServerSettings.AuthenticationScheme) {
                    result = true;
                    WebServer.AuthenticationScheme = configuration.WebServerSettings.AuthenticationScheme;
                }
            }

            _ProxyType = configuration.GoogleMapSettings.ProxyType;

            foreach(var page in _Pages) {
                page.LoadConfiguration(configuration);
            }

            return result;
        }

        private void CacheUsers(Dictionary<string, CachedUser> cacheDictionary, IEnumerable<string> userIds)
        {
            foreach(var user in _UserManager.GetUsersByUniqueId(userIds)) {
                if(user.Enabled && !cacheDictionary.ContainsKey(user.LoginName)) {
                    var key = _UserManager.LoginNameIsCaseSensitive ? user.LoginName : user.LoginName.ToUpperInvariant();
                    cacheDictionary.Add(key, new CachedUser() {
                        User = user,
                    });
                }
            }
        }
        #endregion

        #region AddSiteRoot, RemoveSiteRoot, IsSiteRootActive, GetSiteRootFolders
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void AddSiteRoot(SiteRoot siteRoot)
        {
            var configuration = Factory.Singleton.Resolve<IFileSystemServerConfiguration>().Singleton;
            configuration.AddSiteRoot(siteRoot);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void RemoveSiteRoot(SiteRoot siteRoot)
        {
            var configuration = Factory.Singleton.Resolve<IFileSystemServerConfiguration>().Singleton;
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
            var configuration = Factory.Singleton.Resolve<IFileSystemServerConfiguration>().Singleton;
            return configuration.IsSiteRootActive(siteRoot, folderMustMatch);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSiteRootFolders()
        {
            var configuration = Factory.Singleton.Resolve<IFileSystemServerConfiguration>().Singleton;
            return configuration.GetSiteRootFolders();
        }
        #endregion

        #region RequestContent, RequestSimpleContent
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="args"></param>
        public void RequestContent(RequestReceivedEventArgs args)
        {
            if(args == null) throw new ArgumentNullException("args");

            foreach(var page in _Pages) {
                page.HandleRequest(WebServer, args);
            }
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
        #endregion

        #region AddHtmlContentInjector, RemoveHtmlContentInjector
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void AddHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            if(contentInjector == null) throw new ArgumentNullException("contentInjector");
            lock(_HtmlContentInjectorsLock) _HtmlContentInjectors.Add(contentInjector);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void RemoveHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            lock(_HtmlContentInjectorsLock) _HtmlContentInjectors.Remove(contentInjector);
        }

        /*
        /// <summary>
        /// Injects content into HTML files.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="textContent"></param>
        /// <returns></returns>
        internal string InjectHtmlContent(string pathAndFile, TextContent textContent)
        {
            var result = textContent.Content;

            List<HtmlContentInjector> injectors;
            lock(_HtmlContentInjectorsLock) {
                injectors = _HtmlContentInjectors.Where(r =>
                    !String.IsNullOrEmpty(r.Element) &&
                    r.Content != null &&
                    (r.PathAndFile == null || r.PathAndFile.Equals(pathAndFile, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if(injectors.Any()) {
                var document = new HtmlAgilityPack.HtmlDocument() {
                    OptionCheckSyntax = false,
                    OptionDefaultStreamEncoding = textContent.Encoding,
                };
                document.LoadHtml(result);

                var modified = false;
                foreach(var injector in injectors.OrderByDescending(r => r.Priority)) {
                    var elements = document.DocumentNode.Descendants(injector.Element);
                    var element = injector.AtStart ? elements.FirstOrDefault() : elements.LastOrDefault();
                    var content = element == null ? null : injector.Content();
                    if(element != null && !String.IsNullOrEmpty(content)) {
                        var subDocument = new HtmlAgilityPack.HtmlDocument() {
                            OptionCheckSyntax = false,
                        };
                        subDocument.LoadHtml(injector.Content());

                        if(injector.AtStart) element.PrependChild(subDocument.DocumentNode);
                        else                 element.AppendChild(subDocument.DocumentNode);
                        modified = true;
                    }
                }

                if(modified) {
                    using(var stream = new MemoryStream()) {
                        document.Save(stream);
                        stream.Position = 0;
                        using(var streamReader = new StreamReader(stream, textContent.Encoding, true)) {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            textContent.Content = result;
            return result;
        }
        */
        #endregion

        #region Events consumed
        /// <summary>
        /// Handles changes to the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            if(WebServer != null && LoadConfiguration()) {
                WebServer.Online = false;
                WebServer.Online = true;
            }
        }

        /// <summary>
        /// Handles the authentication events from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Server_AuthenticationRequired(object sender, AuthenticationRequiredEventArgs args)
        {
            lock(_AuthenticationSyncLock) {
                if(!args.IsHandled) {
                    args.IsAuthenticated = args.User != null;
                    if(args.IsAuthenticated) {
                        CachedUser cachedUser;
                        var isAdministrator = false;
                        var key = _UserManager.LoginNameIsCaseSensitive ? args.User : args.User.ToUpperInvariant();

                        if(_AdministratorUsers.TryGetValue(key, out cachedUser)) {
                            isAdministrator = true;
                        } else {
                            _BasicAuthenticationUsers.TryGetValue(key, out cachedUser);
                        }

                        if(cachedUser == null) args.IsAuthenticated = false;
                        else if(cachedUser.KnownGoodPassword != null && cachedUser.KnownGoodPassword == args.Password) args.IsAuthenticated = true;
                        else {
                            args.IsAuthenticated = _UserManager.PasswordMatches(cachedUser.User, args.Password);
                            if(args.IsAuthenticated) cachedUser.KnownGoodPassword = args.Password;
                        }

                        if(args.IsAuthenticated) args.IsAdministrator = isAdministrator;
                    }
                    args.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Handles the request for content by a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Server_RequestReceived(object sender, RequestReceivedEventArgs args)
        {
            RequestContent(args);
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
        #endregion
    }
}
