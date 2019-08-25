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
using NATUPNPLib;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using System.Diagnostics;
using VirtualRadar.Interface;

namespace VirtualRadar.WebServer
{
    /// <summary>
    /// The default implementation of <see cref="IUniversalPlugAndPlayManager"/>.
    /// </summary>
    sealed class DotNetUniversalPlugAndPlayManager : IUniversalPlugAndPlayManager
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of the provider that abstracts away the environment.
        /// </summary>
        sealed class DefaultProvider : IUniversalPlugAndPlayManagerProvider
        {
            /// <summary>
            /// The COM component that this object wraps.
            /// </summary>
            private IUPnPNAT _UPnPNAT;

            /// <summary>
            /// See interface docs.
            /// </summary>
            public void CreateUPnPComComponent()
            {
                _UPnPNAT = new UPnPNATClass();
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            /// <returns></returns>
            public List<IPortMapping> GetPortMappings()
            {
                List<IPortMapping> result = null;
                if(_UPnPNAT != null && _UPnPNAT.StaticPortMappingCollection != null) {
                    result = new List<IPortMapping>();
                    foreach(var portMapping in _UPnPNAT.StaticPortMappingCollection.OfType<IStaticPortMapping>()) {
                        result.Add(new PortMapping(portMapping));
                    }
                }

                return result;
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            /// <param name="externalPort"></param>
            /// <param name="protocol"></param>
            /// <param name="internalPort"></param>
            /// <param name="internalClient"></param>
            /// <param name="startEnabled"></param>
            /// <param name="description"></param>
            public void AddMapping(int externalPort, string protocol, int internalPort, string internalClient, bool startEnabled, string description)
            {
                if(_UPnPNAT != null) {
                    _UPnPNAT.StaticPortMappingCollection.Add(externalPort, protocol, internalPort, internalClient, startEnabled, description);
                }
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            /// <param name="externalPort"></param>
            /// <param name="protocol"></param>
            public void RemoveMapping(int externalPort, string protocol)
            {
                if(_UPnPNAT != null) _UPnPNAT.StaticPortMappingCollection.Remove(externalPort, protocol);
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Gets the description that is used against every port mapping configured by the application.
        /// </summary>
        private const string Description = "Virtual Radar Server";

        /// <summary>
        /// True if <see cref="Initialise"/> has been called.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The external port that has been read from the configuration.
        /// </summary>
        private int _ExternalPort;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUniversalPlugAndPlayManagerProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsRouterPresent { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSupported { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool PortForwardingPresent { get; private set; }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// Raises <see cref="StateChanged"/>. Note this class is sealed.
        /// </summary>
        /// <param name="args"></param>
        private void OnStateChanged(EventArgs args)
        {
            EventHelper.Raise(StateChanged, this, args);
        }
        #endregion

        #region Constructor and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public DotNetUniversalPlugAndPlayManager()
        {
            Provider = new DefaultProvider();

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DotNetUniversalPlugAndPlayManager()
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
        /// Disposes of or finalises the object. Note that the class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Initialised) TakeServerOffInternet();
            }
        }
        #endregion

        #region LoadConfiguration
        /// <summary>
        /// Reads properties from the configuration.
        /// </summary>
        /// <param name="canRaiseStateChanged"></param>
        /// <returns></returns>
        private Configuration LoadConfiguration(bool canRaiseStateChanged)
        {
            var result = Factory.ResolveSingleton<IConfigurationStorage>().Load();
            _ExternalPort = result.WebServerSettings.UPnpPort;

            if(IsEnabled != result.WebServerSettings.EnableUPnp) {
                IsEnabled = result.WebServerSettings.EnableUPnp;
                OnStateChanged(EventArgs.Empty); // if you add more situations where StateChanged can be raised to this then ensure it's only raised once
            }

            return result;
        }
        #endregion

        #region Initialise, PutServerOntoInternet, TakeServerFromInternet
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(WebServer == null) throw new InvalidOperationException("The web server must be set before initialising the UPnP Manager");

            var configuration = LoadConfiguration(false);

            try {
                Provider.CreateUPnPComComponent();
                IEnumerable<IPortMapping> portMappings = Provider.GetPortMappings();
                IsRouterPresent = portMappings != null;

                if(portMappings != null) {
                    var trashMappings = portMappings.Where(p => p.Description == Description);
                    if(!configuration.WebServerSettings.IsOnlyInternetServerOnLan) {
                        trashMappings = trashMappings.Where(p => p.ExternalPort == _ExternalPort &&
                                                                    p.InternalClient == WebServer.NetworkIPAddress &&
                                                                    p.InternalPort == WebServer.Port);
                    }
                    foreach(var trashMapping in trashMappings) {
                        Provider.RemoveMapping(trashMapping.ExternalPort, trashMapping.Protocol);
                    }
                }
            } catch(Exception ex) {
                // Any exceptions coming from the COM component (and experience shows it's not shy about throwing them) and
                // the whole deal is off, we just flag UPnP as unsupported / unavailable. Logging them is pointless, on some
                // machines they would be a permanent fixture in the logs and they don't tell us anything we don't already
                // know; namely "UPnP is borked on this machine".
                Debug.WriteLine(String.Format("UniversalPlugAndPlayManager.Initialise caught exception {0}", ex.ToString()));
            }

            _Initialised = true;
            OnStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void PutServerOntoInternet() { DoMapping(true); }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void TakeServerOffInternet()
        {
            bool restartWebServer = WebServer != null && WebServer.Online && _Initialised && PortForwardingPresent;

            if(restartWebServer) WebServer.Online = false;
            DoMapping(false);
            if(restartWebServer) WebServer.Online = true;
        }

        /// <summary>
        /// Adds or removes the port forwarding mapping from the Internet to the web server.
        /// </summary>
        /// <param name="setMappingPresent"></param>
        private void DoMapping(bool setMappingPresent)
        {
            if(!_Initialised) throw new InvalidOperationException("You must initialise the UPnP manager before use");

            var isMappingPresent = PortForwardingPresent;

            if(IsRouterPresent) {
                try {
                    isMappingPresent = DoesDirectMappingExist();
                    if(IsEnabled && isMappingPresent != setMappingPresent) {
                        if(setMappingPresent)   Provider.AddMapping(_ExternalPort, "TCP", WebServer.Port, WebServer.NetworkIPAddress, true, Description);
                        else                    Provider.RemoveMapping(_ExternalPort, "TCP");
                        isMappingPresent = setMappingPresent;
                    }
                } catch(Exception ex) {
                    // Exceptions thrown by the COM stuff are discarded, see notes in Initialise
                    Debug.WriteLine(String.Format("UniversalPlugAndPlayManager.DoMapping caught exception {0}", ex.ToString()));
                }
            }

            if(isMappingPresent != PortForwardingPresent) {
                PortForwardingPresent = setMappingPresent;
                OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the UPnP router already has a mapping to this computer and port on the
        /// configured external port.
        /// </summary>
        /// <returns></returns>
        private bool DoesDirectMappingExist()
        {
            return Provider.GetPortMappings().Where(m => m.ExternalPort == _ExternalPort &&
                                                         m.InternalClient == WebServer.NetworkIPAddress &&
                                                         m.InternalPort == WebServer.Port &&
                                                         m.Protocol == "TCP").Any();
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            bool webServerOnline = WebServer != null && WebServer.Online;
            bool portForwardingPresent = _Initialised && PortForwardingPresent;

            if(webServerOnline) WebServer.Online = false;
            if(portForwardingPresent) DoMapping(false);

            LoadConfiguration(true);

            if(portForwardingPresent) PutServerOntoInternet();
            if(webServerOnline) WebServer.Online = true;
        }
        #endregion
    }
}
