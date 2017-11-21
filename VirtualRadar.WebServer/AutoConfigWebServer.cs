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
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using System.Diagnostics;

namespace VirtualRadar.WebServer
{
    /// <summary>
    /// The default implementation of <see cref="IAutoConfigWebServer"/>.
    /// </summary>
    sealed class AutoConfigWebServer : IAutoConfigWebServer
    {
        #region Fields
        /// <summary>
        /// The number of minutes to wait between successive attempts to fetch the external IP address.
        /// </summary>
        private static readonly int MinutesBetweenExternalAddressAttempts = 5;

        /// <summary>
        /// Set to true once the external IP address has been fetched.
        /// </summary>
        private bool _FetchedExternalIPAddress;

        /// <summary>
        /// The date and time of the last attempt to fetch the external IP address.
        /// </summary>
        private DateTime _LastFetchAttempt;
        #endregion

        #region Properties
        private static readonly IAutoConfigWebServer _Singleton = new AutoConfigWebServer();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAutoConfigWebServer Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; private set; }
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AutoConfigWebServer()
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
            if(disposing && WebServer != null) WebServer.Dispose();
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            WebServer = Factory.Singleton.Resolve<IWebServer>();
            WebServer.Port = Factory.Singleton.Resolve<IInstallerSettingsStorage>().Load().WebServerPort;

            Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            Factory.Singleton.ResolveSingleton<IExternalIPAddressService>().AddressUpdated += ExternalIPAddressService_AddressUpdated;

            LoadConfiguration();
            LoadExternalIPAddress();

            var heartbeat = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
            heartbeat.SlowTick += Heartbeat_SlowTick;
            heartbeat.SlowTickNow();
        }
        #endregion

        #region LoadConfiguration, LoadExternalIPAddress
        /// <summary>
        /// Loads values from the configuration to the web server object.
        /// </summary>
        private void LoadConfiguration()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

            WebServer.ExternalPort = configuration.WebServerSettings.UPnpPort;
            WebServer.Provider.EnableCompression = configuration.GoogleMapSettings.EnableCompression;
        }

        /// <summary>
        /// Copies the external IP address from the ExternalIPAddressService singleton to the web server.
        /// </summary>
        private void LoadExternalIPAddress()
        {
            var service = Factory.Singleton.ResolveSingleton<IExternalIPAddressService>();

            WebServer.ExternalIPAddress = service.Address;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the configuration has been changed by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Called when the external IP address service indicates that it has determined the address of the PC on the Internet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ExternalIPAddressService_AddressUpdated(object sender, EventArgs<string> args)
        {
            LoadExternalIPAddress();
        }

        /// <summary>
        /// Called periodically on a background thread by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            if(!_FetchedExternalIPAddress) {
                try {
                    var clock = Factory.Singleton.Resolve<IClock>();
                    var threshold = _LastFetchAttempt.AddMinutes(MinutesBetweenExternalAddressAttempts);
                    if(clock.UtcNow >= threshold) {
                        _LastFetchAttempt = clock.UtcNow;
                        Factory.Singleton.ResolveSingleton<IExternalIPAddressService>().GetExternalIPAddress();
                        _FetchedExternalIPAddress = true;
                    }
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("AutoConfigWebServer.Heartbeat_SlowTick caught exception {0}", ex.ToString()));
                    var log = Factory.Singleton.ResolveSingleton<ILog>();
                    log.WriteLine("Exception caught during fetch of external IP address: {0}", ex.ToString());
                }
            }
        }
        #endregion
    }
}
