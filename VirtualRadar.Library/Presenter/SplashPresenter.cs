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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="ISplashPresenter"/>.
    /// </summary>
    class SplashPresenter : ISplashPresenter
    {
        /// <summary>
        /// The default implementation of the provider that abstracts away the environment for us.
        /// </summary>
        class DefaultProvider : ISplashPresenterProvider
        {
            public void AbortApplication()              { Environment.Exit(1); }
            public bool FolderExists(string folder)     { return Directory.Exists(folder); }
        }

        /// <summary>
        /// The view being controlled by this presenter.
        /// </summary>
        private ISplashView _View;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISplashPresenterProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string[] CommandLineArgs { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public EventHandler<EventArgs<Exception>> BackgroundThreadExceptionHandler { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SplashPresenter()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(ISplashView view)
        {
            _View = view;

            _View.ApplicationName = Strings.VirtualRadarServer;
            _View.ApplicationVersion = Factory.Singleton.Resolve<IApplicationInformation>().ShortVersion;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartApplication()
        {
            var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;

            ParseCommandLineParameters(configurationStorage);
            InitialiseLog(configurationStorage);
            var configuration = LoadConfiguration(configurationStorage);
            Factory.Singleton.Resolve<IHeartbeatService>().Singleton.Start();
            InitialiseUserManager();
            LoadPictureFolderCache();
            TestBaseStationDatabaseConnection();
            LoadStandingData();
            StartFeedManager(configuration);
            var webSite = StartWebSite();
            StartRebroadcastServers();
            InitialiseUniversalPlugAndPlay(configuration);
            StartPlugins(webSite);
        }

        private void ParseCommandLineParameters(IConfigurationStorage configurationStorage)
        {
            _View.ReportProgress(Strings.SplashScreenParsingCommandLineParameters);

            if(CommandLineArgs != null) {
                foreach(var arg in CommandLineArgs) {
                    var caselessArg = arg.ToUpper();
                    if(caselessArg.StartsWith("-CULTURE:")) continue;
                    else if(arg == "-showConfigFolder") continue;
                    else if(caselessArg.StartsWith("-WORKINGFOLDER:")) {
                        var folder = arg.Substring(15);
                        if(!Provider.FolderExists(folder)) _View.ReportProblem(String.Format(Strings.FolderDoesNotExistFull, folder), Strings.FolderDoesNotExistTitle, true);
                        else configurationStorage.Folder = folder;
                    } else if(caselessArg.StartsWith("-LISTENERTIMEOUT:")) {
                        var timeoutText = arg.Substring(17);
                        int timeout;
                        if(!int.TryParse(timeoutText, out timeout)) {
                            _View.ReportProblem(String.Format(Strings.CoarseListenerTimeoutUnparseable, timeoutText), Strings.BadListenerTimeout, true);
                        } else if(timeout < 10) {
                            _View.ReportProblem(Strings.CoarseListenerTimeoutInvalid, Strings.BadListenerTimeout, true);
                        } else {
                            Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.CoarseListenerTimeout = timeout;
                        }
                    } else {
                        _View.ReportProblem(String.Format(Strings.UnrecognisedCommandLineParameterFull, arg), Strings.UnrecognisedCommandLineParameterTitle, true);
                    }
                }
            }
        }

        private void InitialiseLog(IConfigurationStorage configurationStorage)
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingLog);

            var log = Factory.Singleton.Resolve<ILog>().Singleton;
            log.Truncate(100);
            log.WriteLine("Program started, version {0}", Factory.Singleton.Resolve<IApplicationInformation>().FullVersion);
            log.WriteLine("Working folder {0}", configurationStorage.Folder);
        }

        private Configuration LoadConfiguration(IConfigurationStorage configurationStorage)
        {
            Configuration result = new Configuration();

            _View.ReportProgress(Strings.SplashScreenLoadingConfiguration);

            try {
                result = configurationStorage.Load();
            } catch(Exception ex) {
                string message = String.Format(Strings.InvalidConfigurationFileFull, ex.Message, configurationStorage.Folder);
                if(_View.YesNoPrompt(message, Strings.InvalidConfigurationFileTitle, true)) {
                    configurationStorage.Save(new Configuration());
                    _View.ReportProblem(Strings.DefaultSettingsSavedFull, Strings.DefaultSettingsSavedTitle, true);
                }
                Provider.AbortApplication();
            }

            return result;
        }

        private void InitialiseUserManager()
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingUserManager);
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            userManager.Initialise();
        }

        private void LoadPictureFolderCache()
        {
            _View.ReportProgress(Strings.SplashScreenStartingPictureFolderCache);
            Factory.Singleton.Resolve<IAutoConfigPictureFolderCache>().Singleton.Initialise();
        }

        private void TestBaseStationDatabaseConnection()
        {
            _View.ReportProgress(Strings.SplashScreenOpeningBaseStationDatabase);

            var autoConfigDatabase = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton;
            autoConfigDatabase.Initialise();

            var baseStationDatabase = autoConfigDatabase.Database;
            if(!String.IsNullOrEmpty(baseStationDatabase.FileName)) {
                if(!baseStationDatabase.TestConnection()) {
                    _View.ReportProblem(String.Format(Strings.CannotOpenBaseStationDatabaseFull, baseStationDatabase.FileName), Strings.CannotOpenBaseStationDatabaseTitle, false);
                }
            }
        }

        private void LoadStandingData()
        {
            _View.ReportProgress(Strings.SplashScreenLoadingStandingData);

            try {
                var standingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
                standingDataManager.Load();
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Exception caught during load of standing data: {0}", ex.ToString());
                _View.ReportProblem(String.Format(Strings.CannotLoadFlightRouteDataFull, ex.Message), Strings.CannotLoadFlightRouteDataTitle, false);
            }

            Factory.Singleton.Resolve<IBackgroundDataDownloader>().Singleton.Start();
        }

        private void StartFeedManager(Configuration configuration)
        {
            _View.ReportProgress(Strings.SplashScreenConnectingToBaseStation);

            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            if(BackgroundThreadExceptionHandler != null) {
                feedManager.ExceptionCaught += BackgroundThreadExceptionHandler;
            }

            feedManager.Initialise();
        }

        private IWebSite StartWebSite()
        {
            _View.ReportProgress(Strings.SplashScreenStartingWebServer);

            var autoConfigWebServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton;
            autoConfigWebServer.Initialise();

            var webServer = autoConfigWebServer.WebServer;
            if(BackgroundThreadExceptionHandler != null) webServer.ExceptionCaught += BackgroundThreadExceptionHandler;

            var connectionLogger = Factory.Singleton.Resolve<IConnectionLogger>().Singleton;
            connectionLogger.LogDatabase = Factory.Singleton.Resolve<ILogDatabase>().Singleton;
            connectionLogger.WebServer = webServer;
            if(BackgroundThreadExceptionHandler != null) connectionLogger.ExceptionCaught += BackgroundThreadExceptionHandler;
            connectionLogger.Start();

            var webSite = Factory.Singleton.Resolve<IWebSite>();
            webSite.BaseStationDatabase = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
            webSite.FlightSimulatorAircraftList = Factory.Singleton.Resolve<ISimpleAircraftList>();
            webSite.StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;

            webSite.AttachSiteToServer(webServer);
            try {
                webServer.Online = true;
            } catch(HttpListenerException ex) {
                // .NET throws HttpListenerException...
                ReportWebServerStartupFailure(webServer, ex);
            } catch(SocketException ex) {
                // ... while Mono throws SocketException
                ReportWebServerStartupFailure(webServer, ex);
            }

            _View.FlightSimulatorXAircraftList = webSite.FlightSimulatorAircraftList;

            return webSite;
        }

        private void ReportWebServerStartupFailure(IWebServer webServer, Exception ex)
        {
            Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception when starting web server: {0}", ex.ToString());
            _View.ReportProblem(String.Format(Strings.CannotStartWebServerFull, webServer.Port), Strings.CannotStartWebServerTitle, false);
            _View.ReportProblem(Strings.SuggestUseDifferentPortFull, Strings.SuggestUseDifferentPortTitle, false);
        }

        private void StartRebroadcastServers()
        {
            _View.ReportProgress(Strings.SplashScreenStartingRebroadcastServers);

            var manager = Factory.Singleton.Resolve<IRebroadcastServerManager>().Singleton;
            if(BackgroundThreadExceptionHandler != null) manager.ExceptionCaught += BackgroundThreadExceptionHandler;
            manager.Initialise();
            manager.Online = true;
        }

        private void InitialiseUniversalPlugAndPlay(Configuration configuration)
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingUPnPManager);

            var manager = Factory.Singleton.Resolve<IUniversalPlugAndPlayManager>();
            manager.WebServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton.WebServer;
            manager.Initialise();

            if(configuration.WebServerSettings.AutoStartUPnP) {
                _View.ReportProgress(Strings.SplashScreenStartingUPnP);
                manager.PutServerOntoInternet();
            }

            _View.UPnpManager = manager;
        }

        private void StartPlugins(IWebSite webSite)
        {
            _View.ReportProgress(Strings.SplashScreenStartingPlugins);

            foreach(var plugin in Factory.Singleton.Resolve<IPluginManager>().Singleton.LoadedPlugins) {
                try {
                    var parameters = new PluginStartupParameters(
                        _View.FlightSimulatorXAircraftList,
                        _View.UPnpManager,
                        webSite,
                        plugin.PluginFolder);

                    plugin.Startup(parameters);

                    if(BackgroundThreadExceptionHandler != null) {
                        IBackgroundThreadExceptionCatcher backgroundExceptionCatcher = plugin as IBackgroundThreadExceptionCatcher;
                        if(backgroundExceptionCatcher != null) backgroundExceptionCatcher.ExceptionCaught += BackgroundThreadExceptionHandler;
                    }
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("MainPresenter.StartPlugins caught exception: {0}", ex.ToString()));
                    Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception when starting {0}: {1}", plugin.Name, ex.ToString());
                    _View.ReportProblem(String.Format(Strings.PluginThrewExceptionFull, plugin.Name, ex.Message), Strings.PluginThrewExceptionTitle, false);
                }
            }
        }
    }
}
