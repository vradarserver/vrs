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
using VirtualRadar.Interface.Network;
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
        /// The username read off the -createAdmin:user command-line option.
        /// </summary>
        private string _CreateAdminUser;

        /// <summary>
        /// The password read off the command line.
        /// </summary>
        private string _Password = "";

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
            _View.ApplicationVersion = Factory.Resolve<IApplicationInformation>().ShortVersion;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartApplication()
        {
            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();

            ParseCommandLineParameters(configurationStorage);
            InitialiseLog(configurationStorage);

            // The user manager needs to be initialised before the configuration is loaded, the Load
            // method can make calls on the user manager.
            InitialiseUserManager();

            var configuration = LoadConfiguration(configurationStorage);
            FixConfigurationErrors(configurationStorage, configuration);
            Factory.ResolveSingleton<IHeartbeatService>().Start();
            LoadPictureFolderCache();
            TestBaseStationDatabaseConnection();
            LoadStandingData();
            StartTileServerSettingsManager();
            StartAirPressureManager();
            StartFeedManager(configuration);
            var webSite = StartWebSite();
            StartRebroadcastServers();
            InitialiseUniversalPlugAndPlay(configuration);
            InitialiseAircraftOnlineLookupManager();
            StartPlugins(webSite);

            if(!String.IsNullOrEmpty(_CreateAdminUser)) {
                CreateAdminUser(configurationStorage, _CreateAdminUser, _Password);
            }
        }

        private void ParseCommandLineParameters(IConfigurationStorage configurationStorage)
        {
            _View.ReportProgress(Strings.SplashScreenParsingCommandLineParameters);

            if(CommandLineArgs != null) {
                foreach(var arg in CommandLineArgs) {
                    var caselessArg = arg.ToUpper();
                    if(caselessArg.StartsWith("-CULTURE:"))         continue;
                    else if(caselessArg == "-SHOWCONFIGFOLDER")     continue;
                    else if(caselessArg == "-DEFAULTFONTS")         continue;
                    else if(caselessArg == "-NOGUI")                continue;
                    else if(caselessArg.StartsWith("-CREATEADMIN:")) {
                        _CreateAdminUser = arg.Substring(13);
                        if(String.IsNullOrEmpty(_CreateAdminUser)) {
                            _View.ReportProblem(Strings.CreateAdminUserMissing, Strings.UserNameMissing, true);
                        }
                    } else if(caselessArg.StartsWith("-PASSWORD:")) {
                        _Password = arg.Substring(10);
                    } else if(caselessArg.StartsWith("-WORKINGFOLDER:")) {
                        var folder = arg.Substring(15);
                        if(!Provider.FolderExists(folder)) _View.ReportProblem(String.Format(Strings.FolderDoesNotExistFull, folder), Strings.FolderDoesNotExistTitle, true);
                        else configurationStorage.Folder = folder;
                    } else if(caselessArg.StartsWith("-LISTENERTIMEOUT:")) {
                        // This was removed in 2.0.3 - coarse timeouts are now a per-receiver configuration property
                    } else if(caselessArg == "/?" || caselessArg == "-?" || caselessArg == "--HELP") {
                        _View.ReportProblem(Strings.CommandLineHelp, Strings.CommandLineHelpTitle, true);
                    } else {
                        _View.ReportProblem(String.Format(Strings.UnrecognisedCommandLineParameterFull, arg), Strings.UnrecognisedCommandLineParameterTitle, true);
                    }
                }
            }
        }

        private void CreateAdminUser(IConfigurationStorage configurationStorage, string createAdminName, string password)
        {
            var userManager = Factory.Resolve<IUserManager>().Singleton;

            if(String.IsNullOrEmpty(password)) {
                _View.ReportProblem(Strings.PasswordMissingOnCommandLine, Strings.PasswordMissingTitle, true);
            }
            if(userManager.GetUserByLoginName(createAdminName) != null) {
                _View.ReportProblem(Strings.CreateAdminUserAlreadyExists, Strings.UserAlreadyExists, true);
            }

            var hash = new Hash(password);
            var user = Factory.Resolve<IUser>();
            user.Enabled = true;
            user.LoginName = createAdminName;
            user.Name = createAdminName;

            userManager.CreateUserWithHash(user, hash);

            var configuration = configurationStorage.Load();
            configuration.WebServerSettings.AdministratorUserIds.Add(user.UniqueId);
            configurationStorage.Save(configuration);
        }

        private void InitialiseLog(IConfigurationStorage configurationStorage)
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingLog);

            var log = Factory.ResolveSingleton<ILog>();
            var applicationInformation = Factory.Resolve<IApplicationInformation>();
            log.Truncate(100);
            log.WriteLine("Program started, version {0}, build date {1} UTC", applicationInformation.FullVersion, applicationInformation.BuildDate);
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

        private void FixConfigurationErrors(IConfigurationStorage configurationStorage, Configuration configuration)
        {
            var saveConfiguration = false;

            var receiverFormatManager = Factory.Resolve<IReceiverFormatManager>().Singleton;
            foreach(var receiver in configuration.Receivers.Where(r => r.Enabled)) {
                if(receiverFormatManager.GetProvider(receiver.DataSource) == null) {
                    receiver.Enabled = false;
                    saveConfiguration = true;
                }
            }

            var rebroadcastFormatManager = Factory.Resolve<IRebroadcastFormatManager>().Singleton;
            foreach(var server in configuration.RebroadcastSettings.Where(r => r.Enabled)) {
                if(rebroadcastFormatManager.GetProvider(server.Format) == null) {
                    server.Enabled = false;
                    saveConfiguration = true;
                }
            }

            if(saveConfiguration) {
                configurationStorage.Save(configuration);
            }
        }

        private void InitialiseUserManager()
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingUserManager);
            var userManager = Factory.Resolve<IUserManager>().Singleton;
            userManager.Initialise();
        }

        private void LoadPictureFolderCache()
        {
            _View.ReportProgress(Strings.SplashScreenStartingPictureFolderCache);
            Factory.ResolveSingleton<IAutoConfigPictureFolderCache>().Initialise();
        }

        private void TestBaseStationDatabaseConnection()
        {
            _View.ReportProgress(Strings.SplashScreenOpeningBaseStationDatabase);

            var autoConfigDatabase = Factory.ResolveSingleton<IAutoConfigBaseStationDatabase>();
            autoConfigDatabase.Initialise();

            var baseStationDatabase = autoConfigDatabase.Database;
            Exception autoFixException = null;
            if(!String.IsNullOrEmpty(baseStationDatabase.FileName)) {
                try {
                    baseStationDatabase.TestConnection();
                } catch(Exception ex) {
                    // Ideally I would catch an SQLite exception here - however because I'm using different SQLite implementations
                    // (Mono's under Mono, SQLite's under .NET), and because I want to be database engine agnostic, I'm just
                    // assuming that any exception is a database problem and offering to turn it over to the database implementation
                    // for correction.
                    var message = String.Format(Strings.CannotOpenDatabaseWantToAutoFix.Replace(@"\r", "\r").Replace(@"\n", "\n"), baseStationDatabase.FileName, ex.Message);
                    if(_View.YesNoPrompt(message, Strings.CannotOpenBaseStationDatabaseTitle, false)) {
                        autoFixException = ex;
                    } else {
                        _View.ReportProblem(String.Format(Strings.CannotOpenBaseStationDatabaseFull, baseStationDatabase.FileName), Strings.CannotOpenBaseStationDatabaseTitle, quitApplication: true);
                    }
                }
            }

            if(autoFixException != null) {
                var mightHaveWorked = baseStationDatabase.AttemptAutoFix(autoFixException);
                if(mightHaveWorked && !baseStationDatabase.TestConnection()) {
                    _View.ReportProblem(String.Format(Strings.CannotOpenBaseStationDatabaseFull, baseStationDatabase.FileName), Strings.CannotOpenBaseStationDatabaseTitle, true);
                }
            }
        }

        private void LoadStandingData()
        {
            _View.ReportProgress(Strings.SplashScreenLoadingStandingData);

            try {
                var standingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
                standingDataManager.Load();
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Exception caught during load of standing data: {0}", ex.ToString());
            }

            Factory.Resolve<IBackgroundDataDownloader>().Singleton.Start();
        }

        private void StartTileServerSettingsManager()
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingTileServerSettingsManager);

            var manager = Factory.Resolve<ITileServerSettingsManager>().Singleton;
            manager.Initialise();
        }

        private void StartAirPressureManager()
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingAirPressureManager);

            var airPressureManager = Factory.ResolveSingleton<IAirPressureManager>();
            airPressureManager.Start();
        }

        private void StartFeedManager(Configuration configuration)
        {
            _View.ReportProgress(Strings.SplashScreenConnectingToBaseStation);

            var feedManager = Factory.ResolveSingleton<IFeedManager>();
            if(BackgroundThreadExceptionHandler != null) {
                feedManager.ExceptionCaught += BackgroundThreadExceptionHandler;
            }

            feedManager.Initialise();
        }

        private IWebSite StartWebSite()
        {
            _View.ReportProgress(Strings.SplashScreenStartingWebServer);

            var autoConfigWebServer = Factory.ResolveSingleton<IAutoConfigWebServer>();
            autoConfigWebServer.Initialise();

            var webServer = autoConfigWebServer.WebServer;
            if(BackgroundThreadExceptionHandler != null) webServer.ExceptionCaught += BackgroundThreadExceptionHandler;

            var connectionLogger = Factory.Resolve<IConnectionLogger>().Singleton;
            connectionLogger.LogDatabase = Factory.Resolve<ILogDatabase>().Singleton;
            connectionLogger.WebServer = webServer;
            if(BackgroundThreadExceptionHandler != null) connectionLogger.ExceptionCaught += BackgroundThreadExceptionHandler;
            connectionLogger.Start();

            var webSite = Factory.Resolve<IWebSite>();
            webSite.BaseStationDatabase = Factory.ResolveSingleton<IAutoConfigBaseStationDatabase>().Database;
            webSite.FlightSimulatorAircraftList = Factory.Resolve<ISimpleAircraftList>();
            webSite.StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();

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
            Factory.ResolveSingleton<ILog>().WriteLine("Caught exception when starting web server: {0}", ex.ToString());
            _View.ReportProblem(String.Format(Strings.CannotStartWebServerFull, webServer.Port), Strings.CannotStartWebServerTitle, false);
            _View.ReportProblem(Strings.SuggestUseDifferentPortFull, Strings.SuggestUseDifferentPortTitle, false);
        }

        private void StartRebroadcastServers()
        {
            _View.ReportProgress(Strings.SplashScreenStartingRebroadcastServers);

            var manager = Factory.Resolve<IRebroadcastServerManager>().Singleton;
            if(BackgroundThreadExceptionHandler != null) manager.ExceptionCaught += BackgroundThreadExceptionHandler;
            manager.Initialise();
            manager.Online = true;
        }

        private void InitialiseUniversalPlugAndPlay(Configuration configuration)
        {
            _View.ReportProgress(Strings.SplashScreenInitialisingUPnPManager);

            var manager = Factory.Resolve<IUniversalPlugAndPlayManager>();
            manager.WebServer = Factory.ResolveSingleton<IAutoConfigWebServer>().WebServer;
            manager.Initialise();

            if(configuration.WebServerSettings.AutoStartUPnP) {
                _View.ReportProgress(Strings.SplashScreenStartingUPnP);
                manager.PutServerOntoInternet();
            }

            _View.UPnpManager = manager;
        }

        private void InitialiseAircraftOnlineLookupManager()
        {
            _View.ReportProgress(Strings.SplashScreenStartingOnlineLookupManager);

            var manager = Factory.ResolveSingleton<IAircraftOnlineLookupManager>();
            var standaloneCache = Factory.Resolve<IStandaloneAircraftOnlineLookupCache>();

            manager.RegisterCache(standaloneCache, 0, true);

            var log = Factory.ResolveSingleton<IAircraftOnlineLookupLog>();
            log.Initialise();
        }

        private void StartPlugins(IWebSite webSite)
        {
            _View.ReportProgress(Strings.SplashScreenStartingPlugins);

            foreach(var plugin in Factory.Resolve<IPluginManager>().Singleton.LoadedPlugins) {
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
                    Factory.ResolveSingleton<ILog>().WriteLine("Caught exception when starting {0}: {1}", plugin.Name, ex.ToString());
                    _View.ReportProblem(String.Format(Strings.PluginThrewExceptionFull, plugin.Name, ex.Message), Strings.PluginThrewExceptionTitle, false);
                }
            }
        }
    }
}
