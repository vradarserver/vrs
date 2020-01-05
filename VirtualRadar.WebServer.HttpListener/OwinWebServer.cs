// Copyright © 2017 onwards, Andrew Whewell
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
using System.Net.Sockets;
using AWhewell.Owin.Interface;
using InterfaceFactory;
using Microsoft.Owin.Hosting;
using Owin;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// An implementation of <see cref="IWebServer"/> that hooks into OWIN and uses
    /// Microsoft.Owin.Hosting to host the web site.
    /// </summary>
    class OwinWebServer : IWebServer
    {
        /// <summary>
        /// The shim that integrates the old style non-OWIN web site in with OWIN.
        /// </summary>
        private WebServerShim _OldServerShim;

        /// <summary>
        /// The handle for the callback we have registered with the standard pipeline builder.
        /// </summary>
        private IMiddlewareBuilderCallbackHandle _BuilderCallbackHandle;

        /// <summary>
        /// The handle to the OWIN web application.
        /// </summary>
        private IDisposable _WebApp;

        /// <summary>
        /// A reference to the singleton <see cref="IAuthenticationConfiguration"/>.
        /// </summary>
        private IAuthenticationConfiguration _AuthenticationConfiguration;

        /// <summary>
        /// A reference to the singleton <see cref="IAccessConfiguration"/>.
        /// </summary>
        private IAccessConfiguration _AccessConfiguration;

        /// <summary>
        /// True if the heartbeat timer has been hooked.
        /// </summary>
        private bool _HookedHeartbeat;

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
                return result?.ToString();
            }
        }

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
                        StopHosting();
                    }
                } else {
                    if(!_Online) {
                        StartHosting();
                    }
                }
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
        public string Prefix
        {
            get { return String.Format("http://*:{0}{1}/", Port, Root == "/" ? "" : Root);; }
        }

        public IWebServerProvider Provider { get; set; } = new OwinWebServerProvider();

        private string _Root = "/";
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Root
        {
            get { return _Root; }
            set
            {
                var root = value ?? "/";
                if(root.Length == 0) root = "/";
                else {
                    if(root[0] != '/') root = String.Format("/{0}", root);
                    if(root.Length > 1 && root[root.Length - 1] == '/') root = root.Substring(0, root.Length - 1);
                }

                _Root = root;
            }
        }

        public event EventHandler<RequestReceivedEventArgs> AfterRequestReceived;

        internal void OnAfterRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(AfterRequestReceived, this, args);
        }

        public event EventHandler<RequestReceivedEventArgs> BeforeRequestReceived;

        internal void OnBeforeRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(BeforeRequestReceived, this, args);
        }

        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        internal void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        public event EventHandler ExternalAddressChanged;

        internal void OnExternalAddressChanged(EventArgs args)
        {
            EventHelper.Raise(ExternalAddressChanged, this, args);
        }

        public event EventHandler OnlineChanged;

        internal void OnOnlineChanged(EventArgs args)
        {
            EventHelper.Raise(OnlineChanged, this, args);
        }

        public event EventHandler<EventArgs<long>> RequestFinished;

        internal void OnRequestFinished(EventArgs<long> args)
        {
            EventHelper.RaiseQuickly(RequestFinished, this, args);
        }

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        internal void OnRequestReceived(RequestReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(RequestReceived, this, args);
        }

        public event EventHandler<ResponseSentEventArgs> ResponseSent;

        internal void OnResponseSent(ResponseSentEventArgs args)
        {
            EventHelper.RaiseQuickly(ResponseSent, this, args);
        }

        public OwinWebServer()
        {
            _AuthenticationConfiguration = Factory.ResolveSingleton<IAuthenticationConfiguration>();
            _AccessConfiguration = Factory.ResolveSingleton<IAccessConfiguration>();
        }

        public void AddAdministratorPath(string pathFromRoot)
        {
            _AuthenticationConfiguration.AddAdministratorPath(pathFromRoot);
        }

        public void Dispose()
        {
            UnhookHeartbeat();
            DeregisterConfigureCallback();
            StopHosting();
        }

        public string[] GetAdministratorPaths()
        {
            return _AuthenticationConfiguration.GetAdministratorPaths();
        }

        public IDictionary<string, Access> GetRestrictedPathsMap()
        {
            return _AccessConfiguration.GetRestrictedPathsMap();
        }

        public void RemoveAdministratorPath(string pathFromRoot)
        {
            _AuthenticationConfiguration.RemoveAdministratorPath(pathFromRoot);
        }

        public void SetRestrictedPath(string pathFromRoot, Access access)
        {
            _AccessConfiguration.SetRestrictedPath(pathFromRoot, access);
        }

        private void RegisterConfigureCallback()
        {
            if(_BuilderCallbackHandle == null) {
                _BuilderCallbackHandle = Factory.ResolveSingleton<IWebSitePipelineBuilder>()
                    .PipelineBuilder
                    .RegisterMiddlewareBuilder(AddShimMiddleware, StandardPipelinePriority.ShimServerPriority);
            }
        }

        private void DeregisterConfigureCallback()
        {
            if(_BuilderCallbackHandle != null) {
                var handle = _BuilderCallbackHandle;
                _BuilderCallbackHandle = null;

                Factory.ResolveSingleton<IWebSitePipelineBuilder>()
                    .PipelineBuilder
                    .DeregisterMiddlewareBuilder(handle);
            }
        }

        private void StartHosting()
        {
            if(_WebApp == null) {
                HookHeartbeat();
                RegisterConfigureCallback();

                var startOptions = new StartOptions() {
                };
                startOptions.Urls.Add(Prefix);

                _Online = true;
                OnOnlineChanged(EventArgs.Empty);
            }
        }

        private void StopHosting()
        {
            if(_WebApp != null) {
                try {
                    _WebApp.Dispose();
                } finally {
                    _WebApp = null;
                    _Online = false;
                    OnOnlineChanged(EventArgs.Empty);
                }
            }
        }

        private void AddShimMiddleware(IPipelineBuilderEnvironment builderEnv)
        {
            _OldServerShim = new WebServerShim(this);
            builderEnv.UseMiddleware(_OldServerShim.ShimMiddleware);
        }

        private void HookHeartbeat()
        {
            if(!_HookedHeartbeat) {
                _HookedHeartbeat = true;
                var heartbeatTimer = Factory.ResolveSingleton<IHeartbeatService>();
                heartbeatTimer.FastTick += HeartbeatService_FastTick;
            }
        }

        private void UnhookHeartbeat()
        {
            if(_HookedHeartbeat) {
                _HookedHeartbeat = false;
                var heartbeatTimer = Factory.ResolveSingleton<IHeartbeatService>();
                heartbeatTimer.FastTick -= HeartbeatService_FastTick;
            }
        }

        private void HeartbeatService_FastTick(object sender, EventArgs e)
        {
            _OldServerShim.RaiseRequestFinishedEvents();
        }
    }
}
