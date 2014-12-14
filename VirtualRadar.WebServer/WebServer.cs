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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.WebServer;
using System.Net;
using VirtualRadar.Interface;
using System.Web;
using System.Net.Sockets;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using System.Diagnostics;

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
                _HttpListener.AuthenticationSchemes = AuthenticationSchemes;
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
                    if(!Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono) {
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

        #region Fields
        /// <summary>
        /// The cache of authenticated user credentials.
        /// </summary>
        private Dictionary<string, string> _AuthenticatedUserCache = new Dictionary<string,string>();
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
                var result = ipAddresses == null || ipAddresses.Length == 0 ? null : ipAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
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
            if(ExternalAddressChanged != null) ExternalAddressChanged(this, args);
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
            if(AuthenticationRequired != null) AuthenticationRequired(this, args);
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
            if(OnlineChanged != null) OnlineChanged(this, args);
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
            if(BeforeRequestReceived != null) BeforeRequestReceived(this, args);
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
            if(RequestReceived != null) RequestReceived(this, args);
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
            if(AfterRequestReceived != null) AfterRequestReceived(this, args);
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
            if(ResponseSent != null) ResponseSent(this, args);
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
            if(ExceptionCaught != null) ExceptionCaught(this, args);
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
                            if(Authenticated(context)) {
                                var startTime = Provider.UtcNow;
                                OnBeforeRequestReceived(requestArgs);
                                OnRequestReceived(requestArgs);
                                OnAfterRequestReceived(requestArgs);

                                if(!requestArgs.Handled) context.Response.StatusCode = HttpStatusCode.NotFound;

                                var fullClientAddress = String.Format("{0}:{1}", requestArgs.ClientAddress, context.Request.RemoteEndPoint.Port);
                                var responseArgs = new ResponseSentEventArgs(requestArgs.PathAndFile, fullClientAddress, requestArgs.ClientAddress,
                                                                             context.Response.ContentLength, requestArgs.Classification, context.Request,
                                                                             (int)context.Response.StatusCode, (int)(Provider.UtcNow - startTime).TotalMilliseconds);
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
            }
        }

        /// <summary>
        /// Authenticates the request from the browser.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool Authenticated(IContext context)
        {
            bool result = false;

            switch(AuthenticationScheme) {
                case AuthenticationSchemes.None:
                case AuthenticationSchemes.Anonymous:
                    result = true;
                    break;
                case AuthenticationSchemes.Basic:
                    bool useCache = CacheCredentials;
                    if(useCache && context.BasicUserName != null) {
                        string password;
                        result = _AuthenticatedUserCache.TryGetValue(context.BasicUserName, out password) && context.BasicPassword == password;
                    }

                    if(!result) {
                        var args = new AuthenticationRequiredEventArgs(context.BasicUserName, context.BasicPassword);
                        OnAuthenticationRequired(args);
                        result = args.IsAuthenticated;
                        if(result) {
                            if(useCache && args.User != null) _AuthenticatedUserCache.Add(args.User, args.Password);
                        } else {
                            context.Response.StatusCode = HttpStatusCode.Unauthorized;
                            context.Response.AddHeader("WWW-Authenticate", String.Format(@"Basic Realm=""{0}""", Provider.ListenerRealm));
                        }
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
    }
}
