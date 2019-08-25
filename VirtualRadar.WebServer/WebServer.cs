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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer
{
    /// <summary>
    /// The default implementation of <see cref="IWebServer"/>.
    /// </summary>
    sealed class WebServer : IWebServer
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IWebServerProvider"/>.
        /// </summary>
        sealed class DefaultProvider : IWebServerProvider
        {
            public DateTime UtcNow { get { return DateTime.UtcNow; } }

            private HttpListener _HttpListener = new HttpListener();

            private IPAddress[] _LocalIpAddresses;

            public string ListenerPrefix
            {
                get { return _HttpListener.Prefixes.Count == 0 ? null : _HttpListener.Prefixes.First(); }
                set { if(ListenerPrefix != value) { _HttpListener.Prefixes.Clear(); _HttpListener.Prefixes.Add(value); } }
            }

            public AuthenticationSchemes AuthenticationSchemes { get; set; }

            public AuthenticationSchemes AdministratorAuthenticationScheme { get; set; }

            public string ListenerRealm
            {
                get { return _HttpListener.Realm; }
            }

            public bool IsListening
            {
                get { return _HttpListener.IsListening; }
            }

            public bool EnableCompression { get; set; }

            ~DefaultProvider()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if(disposing) {
                    ((IDisposable)_HttpListener).Dispose();
                }
            }

            public void StartListener()
            {
                _HttpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous | AuthenticationSchemes.Basic;
                _HttpListener.IgnoreWriteExceptions = true;
                _HttpListener.Start();
            }

            public void StopListener()
            {
                _HttpListener.Stop();
            }

            public IAsyncResult BeginGetContext(AsyncCallback callback)
            {
                return _HttpListener.BeginGetContext(callback, _HttpListener);
            }

            public IContext EndGetContext(IAsyncResult asyncResult)
            {
                HttpListener listener = (HttpListener)asyncResult.AsyncState;
                var listenerContext = listener.EndGetContext(asyncResult);

                return new Context(listenerContext, EnableCompression);
            }

            public IPAddress[] GetHostAddresses()
            {
                if(_LocalIpAddresses == null) {
                    if(!Factory.ResolveSingleton<IRuntimeEnvironment>().IsMono) {
                        _LocalIpAddresses = Dns.GetHostAddresses("");
                    } else {
                        // Ugh...
                        try {
                            _LocalIpAddresses = Dns.GetHostAddresses("");
                        } catch {
                            try {
                                // Copied from here:
                                // http://forums.xamarin.com/discussion/348/acquire-device-ip-addresses-monotouch-since-ios6-0
                                // Except this doesn't work on Mono under OSX because of a bug in GetAllNetworkinterfaces...
                                var addresses = new List<IPAddress>();
                                foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                                    if (netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 ||
                                        netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet) {
                                        foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses) {
                                            if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
                                                var ipAddress = addrInfo.Address;
                                                addresses.Add(ipAddress);
                                            }
                                        }
                                    }  
                                }
                                _LocalIpAddresses = addresses.ToArray();
                            } catch {
                                _LocalIpAddresses = new IPAddress[0];
                            }
                        }
                    }
                }
                return _LocalIpAddresses;
            }
        }
        #endregion

        #region Priavte class - CachedCredential
        /// <summary>
        /// Collects together information about a cached credential.
        /// </summary>
        class CachedCredential
        {
            /// <summary>
            /// Gets or sets the password for the cached user.
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the user is an administrator.
            /// </summary>
            public bool IsAdministrator { get; set; }
        }
        #endregion

        #region Private class - FailedLogin
        /// <summary>
        /// A class that represents the credentials used on the last failed login.
        /// </summary>
        class FailedLogin
        {
            public string User { get; set; }

            public string Password { get; set; }

            public int Attempts { get; set; }
        }
        #endregion

        #region Private class - RestrictedPath
        /// <summary>
        /// Describes a restricted path.
        /// </summary>
        class RestrictedPath
        {
            public string NormalisedPath;
            public IAccessFilter Filter;
        }
        #endregion

        #region Fields
        /// <summary>
        /// A map of cached user names to their credentials.
        /// </summary>
        private Dictionary<string, CachedCredential> _AuthenticatedUserCache = new Dictionary<string, CachedCredential>();

        /// <summary>
        /// A map of IPAddresses to credentials used in failed logins.
        /// </summary>
        private ExpiringDictionary<IPAddress, FailedLogin> _FailedLoginAttempts = new ExpiringDictionary<IPAddress, FailedLogin>(10 * 60000, 60000);

        /// <summary>
        /// Protects the administrator paths from multi-threaded access.
        /// </summary>
        private SpinLock _AdministratorPathSpinLock = new SpinLock();

        /// <summary>
        /// The list of paths that require administrator permissions.
        /// </summary>
        private List<string> _AdministratorPaths = new List<string>();

        /// <summary>
        /// The lock object that protects access to <see cref="_RestrictedPaths"/> from concurrent access.
        /// </summary>
        private SpinLock _RestrictedPathSpinLock = new SpinLock();

        /// <summary>
        /// The dictionary of restricted paths to the filters that indicate whether an IP address can access the path.
        /// </summary>
        private List<RestrictedPath> _RestrictedPaths = new List<RestrictedPath>();
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServerProvider Provider { get; set; }

        private string _Root = "/";
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Root
        {
            get { return _Root; }
            set
            {
                string root = value ?? "/";
                if(root.Length == 0) root = "/";
                else {
                    if(root[0] != '/') root = String.Format("/{0}", root);
                    if(root.Length > 1 && root[root.Length - 1] == '/') root = root.Substring(0, root.Length - 1);
                }

                _Root = root;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PortText
        {
            get { return Port == 80 ? "" : String.Format(":{0}", Port); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LocalAddress
        {
            get { return String.Format("http://127.0.0.1{0}{1}", PortText, Root); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NetworkAddress
        {
            get
            {
                var ipAddress = NetworkIPAddress;
                return ipAddress == null ? null : String.Format("http://{0}{1}{2}", ipAddress, PortText, Root);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NetworkIPAddress
        {
            get
            {
                var ipAddresses = Provider.GetHostAddresses();
                var result = ipAddresses == null || ipAddresses.Length == 0 ? null : ipAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                if(result != null && IPAddressHelper.IsLinkLocal(result)) {
                    var alternate = ipAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddressHelper.IsLinkLocal(a));
                    if(alternate != null) {
                        result = alternate;
                    }
                }
                return result == null ? null : result.ToString();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ExternalAddress
        {
            get
            {
                string result = null;
                if(!String.IsNullOrEmpty(ExternalIPAddress)) {
                    result = String.Format("http://{0}{1}{2}",
                                ExternalIPAddress,
                                ExternalPort == 80 ? "" : String.Format(":{0}", ExternalPort),
                                Root);
                }
                return result;
            }
        }

        private string _ExternalIPAddress;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ExternalIPAddress
        {
            get { return _ExternalIPAddress; }
            set
            {
                if(_ExternalIPAddress != value) {
                    _ExternalIPAddress = value;
                    OnExternalAddressChanged(EventArgs.Empty);
                }
            }
        }

        private int _ExternalPort;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ExternalPort
        {
            get { return _ExternalPort; }
            set
            {
                if(_ExternalPort != value) {
                    _ExternalPort = value;
                    OnExternalAddressChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Prefix
        {
            get { return String.Format("http://*:{0}{1}/", Port, Root == "/" ? "" : Root);; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public AuthenticationSchemes AuthenticationScheme
        {
            get { return Provider.AuthenticationSchemes; }
            set { Provider.AuthenticationSchemes = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CacheCredentials { get; set; }

        private bool _Online;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Online
        {
            get { return _Online; }
            set
            {
                if(!value) {
                    if(_Online) {
                        Provider.StopListener();
                        _Online = false;
                        OnOnlineChanged(EventArgs.Empty);
                    }
                } else {
                    if(!_Online) {
                        Provider.ListenerPrefix = Prefix;
                        Provider.StartListener();
                        _Online = true;
                        OnOnlineChanged(EventArgs.Empty);
                        Provider.BeginGetContext(GetContextHandler);
                    }
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ExternalAddressChanged;

        /// <summary>
        /// Raises <see cref="ExternalAddressChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnExternalAddressChanged(EventArgs args)
        {
            EventHelper.Raise(ExternalAddressChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<AuthenticationRequiredEventArgs> AuthenticationRequired;

        /// <summary>
        /// Raises <see cref="OnAuthenticationRequired"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnAuthenticationRequired(AuthenticationRequiredEventArgs args)
        {
            EventHelper.RaiseQuickly(AuthenticationRequired, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler OnlineChanged;

        /// <summary>
        /// Raises <see cref="OnlineChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnOnlineChanged(EventArgs args)
        {
            EventHelper.Raise(OnlineChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<RequestReceivedEventArgs> BeforeRequestReceived;

        /// <summary>
        /// Raises <see cref="BeforeRequestReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnBeforeRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(BeforeRequestReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Raises <see cref="RequestReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(RequestReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<RequestReceivedEventArgs> AfterRequestReceived;

        /// <summary>
        /// Raises <see cref="AfterRequestReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnAfterRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(AfterRequestReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<ResponseSentEventArgs> ResponseSent;

        /// <summary>
        /// Raises <see cref="ResponseSent"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnResponseSent(ResponseSentEventArgs args)
        {
            EventHelper.RaiseQuickly(ResponseSent, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<long>> RequestFinished;

        /// <summary>
        /// Raises <see cref="RequestFinished"/>
        /// </summary>
        /// <param name="args"></param>
        private void OnRequestFinished(EventArgs<long> args)
        {
            EventHelper.RaiseQuickly(RequestFinished, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }
        #endregion

        #region Constructor, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebServer()
        {
            Provider = new DefaultProvider();
            Port = ExternalPort = 80;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~WebServer()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes of the object.
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
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(Provider != null) Provider.Dispose();
            }
        }
        #endregion

        #region GetContextHandler
        /// <summary>
        /// Raised by the provider when a new request is received from the user.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void GetContextHandler(IAsyncResult asyncResult)
        {
            long requestReceivedEventArgsId = -1;

            if(Provider.IsListening) {
                bool providerIsStable = true;
                IContext context = null;

                try {
                    try {
                        context = Provider.EndGetContext(asyncResult);
                    } catch(HttpListenerException ex) {
                        // These are just discarded, they're usually disconnections by the user made before we can process the request
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                    } catch(ObjectDisposedException ex) {
                        // These are usually thrown during program shutdown, after the provider has gone away but while requests are outstanding
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        providerIsStable = false;
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        OnExceptionCaught(new EventArgs<Exception>(ex));
                        providerIsStable = false;
                    }

                    try {
                        if(providerIsStable) Provider.BeginGetContext(GetContextHandler);
                    } catch(HttpListenerException ex) {
                        // These can be thrown if the server is taken offline between the EndGetContext above and this BeginGetContext
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        context = null;
                    } catch(ObjectDisposedException ex) {
                        // These are usually thrown during program shutdown for the same reasons that EndGetContext can throw them
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        context = null;
                    } catch(InvalidOperationException ex) {
                        // These are thrown when the provider is taken offline between the check above for Provider.IsListening and
                        // the call to BeginGetContext
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        context = null;
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        OnExceptionCaught(new EventArgs<Exception>(ex));
                        context = null;
                    }

                    if(context != null) {
                        try {
                            var requestArgs = new RequestReceivedEventArgs(context.Request, context.Response, Root);
                            requestReceivedEventArgsId = requestArgs.UniqueId;

                            if(IsRestricted(requestArgs)) {
                                context.Response.StatusCode = HttpStatusCode.Forbidden;
                            } else if(Authenticated(context, requestArgs)) {
                                var startTime = Provider.UtcNow;
                                OnBeforeRequestReceived(requestArgs);
                                OnRequestReceived(requestArgs);
                                OnAfterRequestReceived(requestArgs);

                                if(!requestArgs.Handled) context.Response.StatusCode = HttpStatusCode.NotFound;

                                var fullClientAddress = String.Format("{0}:{1}", requestArgs.ClientAddress, context.Request.RemoteEndPoint.Port);
                                var responseArgs = new ResponseSentEventArgs(requestArgs.PathAndFile, fullClientAddress, requestArgs.ClientAddress,
                                                                             context.Response.ContentLength, requestArgs.Classification, context.Request,
                                                                             (int)context.Response.StatusCode, (int)(Provider.UtcNow - startTime).TotalMilliseconds,
                                                                             context.BasicUserName);
                                OnResponseSent(responseArgs);
                            }
                        } catch(HttpListenerException ex) {
                            // These are usually thrown when the browser disconnects while the event handler tries to send data to it. You can get a lot
                            // of these, we just discard them to prevent them spamming logs or the display with messages we can't do anything about.
                            Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        } catch(Exception ex) {
                            Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                            OnExceptionCaught(new EventArgs<Exception>(new RequestException(context.Request, ex)));
                        }

                        try {
                            context.Response.Close();
                        } catch(Exception ex) {
                            // We can get a lot of exceptions from closing the stream if the client browser has disconnected, it's exception spam.
                            Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception {0}", ex.ToString()));
                        }
                    }
                } finally {
                    try {
                        if(context != null && context.Response != null) context.Response.Dispose();
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("WebServer.GetContextHandler caught exception while disposing of response: {0}", ex.ToString()));
                    }
                }

                try {
                    if(requestReceivedEventArgsId != -1) {
                        OnRequestFinished(new EventArgs<long>(requestReceivedEventArgsId));
                    }
                } catch(Exception ex) {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine("Caught exception in RequestFinished event handler: {0}", ex);
                }
            }
        }

        /// <summary>
        /// Authenticates the request from the browser.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestArgs"></param>
        /// <returns></returns>
        private bool Authenticated(IContext context, RequestReceivedEventArgs requestArgs)
        {
            bool result = false;

            var authenticationScheme = AuthenticationScheme;
            var isAdministratorPath = IsAdministratorPath(requestArgs);
            if(isAdministratorPath) authenticationScheme = AuthenticationSchemes.Basic;

            switch(authenticationScheme) {
                case AuthenticationSchemes.None:
                case AuthenticationSchemes.Anonymous:
                    if(isAdministratorPath) throw new InvalidOperationException("Anonymous access to administrator paths is not supported");
                    result = true;
                    break;
                case AuthenticationSchemes.Basic:
                    bool useCache = CacheCredentials;
                    if(useCache && context.BasicUserName != null) {
                        CachedCredential cachedCredential;
                        if(_AuthenticatedUserCache.TryGetValue(context.BasicUserName, out cachedCredential)) {
                            result = context.BasicPassword == cachedCredential.Password;
                            if(result) {
                                result = !isAdministratorPath || cachedCredential.IsAdministrator;
                            }
                        }
                    }

                    if(!result) {
                        var args = new AuthenticationRequiredEventArgs(context.BasicUserName, context.BasicPassword);
                        OnAuthenticationRequired(args);
                        result = args.IsAuthenticated && (!isAdministratorPath || args.IsAdministrator);
                        if(result) {
                            if(useCache && args.User != null) {
                                var cachedCredential = new CachedCredential() {
                                    IsAdministrator = args.IsAdministrator,
                                    Password = context.BasicPassword,
                                };
                                _AuthenticatedUserCache.Add(context.BasicUserName, cachedCredential);
                            }
                        } else {
                            var failedLogin = _FailedLoginAttempts.GetAndRefreshOrCreate(context.Request.RemoteEndPoint.Address, (unused) => new FailedLogin());
                            var sameCredentials = context.BasicUserName == failedLogin.User && context.BasicPassword == failedLogin.Password;
                            failedLogin.User = context.BasicUserName;
                            failedLogin.Password = context.BasicPassword;
                            if(!sameCredentials) {
                                if(failedLogin.Attempts < int.MaxValue) ++failedLogin.Attempts;
                                if(failedLogin.Attempts > 2) {
                                    var pauseMilliseconds = (Math.Min(failedLogin.Attempts, 14) - 2) * 5000;
                                    Thread.Sleep(pauseMilliseconds);
                                }
                            }

                            context.Response.StatusCode = HttpStatusCode.Unauthorized;
                            context.Response.AddHeader("WWW-Authenticate", String.Format(@"Basic Realm=""{0}""", Provider.ListenerRealm));
                        }
                    }

                    if(result) {
                        requestArgs.UserName = context.BasicUserName;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
        #endregion

        #region ResetCredentialCache
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ResetCredentialCache()
        {
            _AuthenticatedUserCache.Clear();
        }
        #endregion

        #region GetAdministratorPaths, AddAdministratorPath, IsAdministratorPath
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public string[] GetAdministratorPaths()
        {
            using(_AdministratorPathSpinLock.AcquireLock()) {
                return _AdministratorPaths.ToArray();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        public void AddAdministratorPath(string pathFromRoot)
        {
            pathFromRoot = (pathFromRoot ?? "").Trim().ToLower();
            if(!pathFromRoot.StartsWith("/")) pathFromRoot = String.Format("/{0}", pathFromRoot);
            if(!pathFromRoot.EndsWith("/")) pathFromRoot = String.Format("{0}/", pathFromRoot);

            using(_AdministratorPathSpinLock.AcquireLock()) {
                if(!_AdministratorPaths.Contains(pathFromRoot)) _AdministratorPaths.Add(pathFromRoot);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        public void RemoveAdministratorPath(string pathFromRoot)
        {
            pathFromRoot = (pathFromRoot ?? "").Trim().ToLower();
            if(!pathFromRoot.StartsWith("/")) pathFromRoot = String.Format("/{0}", pathFromRoot);
            if(!pathFromRoot.EndsWith("/")) pathFromRoot = String.Format("{0}/", pathFromRoot);

            using(_AdministratorPathSpinLock.AcquireLock()) {
                if(_AdministratorPaths.Contains(pathFromRoot)) _AdministratorPaths.Remove(pathFromRoot);
            }
        }

        /// <summary>
        /// Returns true if the context represents a path that is marked as for administrators only.
        /// </summary>
        /// <param name="requestArgs"></param>
        /// <returns></returns>
        private bool IsAdministratorPath(RequestReceivedEventArgs requestArgs)
        {
            var result = false;

            var path = requestArgs.PathAndFile.ToLower();
            _AdministratorPathSpinLock.Lock();
            try {
                for(var i = 0;i < _AdministratorPaths.Count;++i) {
                    if(path.StartsWith(_AdministratorPaths[i])) {
                        result = true;
                        break;
                    }
                }
            } finally {
                _AdministratorPathSpinLock.Unlock();
            }

            return result;
        }
        #endregion

        #region GetRestrictedPathsMap, SetRestrictedPath
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Access> GetRestrictedPathsMap()
        {
            var result = new Dictionary<string, Access>();

            using(_RestrictedPathSpinLock.AcquireLock()) {
                foreach(var entry in _RestrictedPaths) {
                    result.Add(entry.NormalisedPath, entry.Filter.Access);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="access"></param>
        public void SetRestrictedPath(string pathFromRoot, Access access)
        {
            pathFromRoot = (pathFromRoot ?? "").Trim().ToLower();
            if(!pathFromRoot.StartsWith("/")) pathFromRoot = String.Format("/{0}", pathFromRoot);
            if(!pathFromRoot.EndsWith("/")) pathFromRoot = String.Format("{0}/", pathFromRoot);

            var removeEntry = access == null || access.DefaultAccess == DefaultAccess.Unrestricted;
            using(_RestrictedPathSpinLock.AcquireLock()) {
                var restrictedPath = _RestrictedPaths.FirstOrDefault(r => r.NormalisedPath == pathFromRoot);

                if(removeEntry) {
                    if(restrictedPath != null) _RestrictedPaths.Remove(restrictedPath);
                } else {
                    if(restrictedPath != null) {
                        restrictedPath.Filter.Initialise(access);
                    } else {
                        var filter = Factory.Resolve<IAccessFilter>();
                        filter.Initialise(access);

                        restrictedPath = new RestrictedPath() {
                            NormalisedPath = pathFromRoot,
                            Filter = filter,
                        };

                        _RestrictedPaths.Add(restrictedPath);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the request is for a restricted path and the address is not allowed to access it.
        /// </summary>
        /// <param name="requestArgs"></param>
        /// <returns></returns>
        private bool IsRestricted(RequestReceivedEventArgs requestArgs)
        {
            var result = false;

            if(requestArgs.PathAndFile != null && requestArgs.Request != null && requestArgs.Request.RemoteEndPoint != null) {
                var path = requestArgs.PathAndFile.ToLower();
                IAccessFilter filter = null;
                _RestrictedPathSpinLock.Lock();
                try {
                    for(var i = 0;i < _RestrictedPaths.Count;++i) {
                        var restrictedPath = _RestrictedPaths[i];
                        if(path.StartsWith(restrictedPath.NormalisedPath)) {
                            filter = restrictedPath.Filter;
                            break;
                        }
                    }
                } finally {
                    _RestrictedPathSpinLock.Unlock();
                }

                if(filter != null) {
                    result = !filter.Allow(requestArgs.Request.RemoteEndPoint.Address);
                }
            }

            return result;
        }
        #endregion
    }
}
