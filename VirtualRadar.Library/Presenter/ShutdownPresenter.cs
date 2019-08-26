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
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IShutdownPresenter"/>.
    /// </summary>
    class ShutdownPresenter : IShutdownPresenter
    {
        /// <summary>
        /// The view that this presenter is controlling.
        /// </summary>
        private IShutdownView _View;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUniversalPlugAndPlayManager UPnpManager { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IShutdownView view)
        {
            _View = view;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShutdownApplication()
        {
            ILog log;
            try {
                log = Factory.ResolveSingleton<ILog>();
            } catch {
                log = null;
            }

            Action<string, Action> isolatedShutdown = (functionName, action) => {
                try {
                    action();
                } catch(Exception ex) {
                    try {
                        if(log != null) log.WriteLine("Caught exception during shutdown in {0}: {1}", functionName, ex.ToString());
                    } catch {}
                }
            };

            isolatedShutdown("ShutdownAircraftOnlineLookupManager", ShutdownAircraftOnlineLookupManager);  // Do this first so if the database writer was caching it won't temporarily fall back to the standalone cache during rest of shutdown
            isolatedShutdown("ShutdownAirPressureManager",          ShutdownAirPressureManager);
            isolatedShutdown("ShutdownPlugins",                     ShutdownPlugins);
            isolatedShutdown("ShutdownConnectionLogger",            ShutdownConnectionLogger);
            isolatedShutdown("ShutdownUPnpManager",                 ShutdownUPnpManager);
            isolatedShutdown("ShutdownWebServer",                   ShutdownWebServer);
            isolatedShutdown("ShutdownRebroadcastServers",          ShutdownRebroadcastServers);
            isolatedShutdown("SavePolarPlots",                      SavePolarPlots);
            isolatedShutdown("ShutdownListeners",                   ShutdownListeners);
            isolatedShutdown("ShutdownBaseStationDatabase",         ShutdownBaseStationDatabase);
            isolatedShutdown("ShutdownUserManager",                 ShutdownUserManager);
            isolatedShutdown("ShutdownLogDatabase",                 ShutdownLogDatabase);
        }

        private void ShutdownAircraftOnlineLookupManager()
        {
            _View.ReportProgress(Strings.ShuttingDownOnlineLookupManager);
            Factory.ResolveSingleton<IAircraftOnlineLookupManager>().Dispose();
        }

        private void ShutdownAirPressureManager()
        {
            _View.ReportProgress(Strings.ShuttingDownAirPressureManager);
            Factory.ResolveSingleton<IAirPressureManager>().Stop();
        }

        private void ShutdownRebroadcastServers()
        {
            _View.ReportProgress(Strings.ShuttingDownRebroadcastServer);
            Factory.Resolve<IRebroadcastServerManager>().Singleton.Dispose();
        }

        private void SavePolarPlots()
        {
            _View.ReportProgress(Strings.SavingPolarPlots);
            Factory.Resolve<ISavedPolarPlotStorage>().Singleton.Save();
        }

        private void ShutdownListeners()
        {
            _View.ReportProgress(Strings.ShuttingDownBaseStationListener);
            Factory.ResolveSingleton<IFeedManager>().Dispose();
        }

        private void ShutdownPlugins()
        {
            var plugins = Factory.ResolveSingleton<IPluginManager>().LoadedPlugins;
            foreach(var plugin in plugins) {
                _View.ReportProgress(String.Format(Strings.ShuttingDownPlugin, plugin.Name));

                try {
                    plugin.Shutdown();
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("ShutdownPresenter.ShutdownPlugins caught exception: {0}", ex.ToString()));
                    Factory.ResolveSingleton<ILog>().WriteLine("Plugin {0} threw an exception during shutdown: {1}", plugin.Name, ex.ToString());
                }
            }
        }

        private void ShutdownUPnpManager()
        {
            _View.ReportProgress(Strings.ShuttingDownUPnpManager);
            if(UPnpManager != null) UPnpManager.Dispose();
        }

        private void ShutdownConnectionLogger()
        {
            _View.ReportProgress(Strings.ShuttingDownConnectionLogger);
            Factory.ResolveSingleton<IConnectionLogger>().Dispose();
        }

        private void ShutdownWebServer()
        {
            _View.ReportProgress(Strings.ShuttingDownWebServer);
            Factory.ResolveSingleton<IAutoConfigWebServer>().Dispose();
        }

        private void ShutdownBaseStationDatabase()
        {
            _View.ReportProgress(Strings.ShuttingDownBaseStationDatabase);
            Factory.ResolveSingleton<IAutoConfigBaseStationDatabase>().Dispose();
        }

        private void ShutdownUserManager()
        {
            _View.ReportProgress(Strings.ShuttingDownUserManager);
            Factory.Resolve<IUserManager>().Singleton.Shutdown();
        }

        private void ShutdownLogDatabase()
        {
            _View.ReportProgress(Strings.ShuttingDownLogDatabase);
            Factory.ResolveSingleton<ILogDatabase>().Dispose();
        }
    }
}
