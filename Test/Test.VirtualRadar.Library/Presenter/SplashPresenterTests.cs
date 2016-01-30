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
using System.Net;
using System.Net.Sockets;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
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

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class SplashPresenterTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private ISplashPresenter _Presenter;
        private Mock<ISplashPresenterProvider> _Provider;
        private Mock<ISplashView> _View;

        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<ILog> _Log;
        private Mock<IHeartbeatService> _HearbeatService;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IWebServer> _WebServer;
        private Mock<IAutoConfigWebServer> _AutoConfigWebServer;
        private Mock<IWebSite> _WebSite;
        private Mock<ISimpleAircraftList> _FlightSimulatorXAircraftList;
        private Mock<IUniversalPlugAndPlayManager> _UniversalPlugAndPlayManager;
        private Mock<IConnectionLogger> _ConnectionLogger;
        private Mock<ILogDatabase> _LogDatabase;
        private Mock<IBackgroundDataDownloader> _BackgroundDataDownloader;
        private Mock<IPluginManager> _PluginManager;
        private Mock<IApplicationInformation> _ApplicationInformation;
        private Mock<IAutoConfigPictureFolderCache> _AutoConfigPictureFolderCache;
        private Mock<IRebroadcastServerManager> _RebroadcastServerManager;
        private Mock<IFeedManager> _FeedManager;
        private Mock<IUserManager> _UserManager;
        private Mock<IAircraftOnlineLookupManager> _AircraftOnlineLookupManager;
        private Mock<IStandaloneAircraftOnlineLookupCache> _StandaloneAircraftOnlineLookupCache;
        private Mock<IAircraftOnlineLookupLog> _AircraftOnlineLookupLog;
        private Mock<IUser> _User;

        private EventRecorder<EventArgs<Exception>> _BackgroundExceptionEvent;

        public interface IPluginBackgroundThreadCatcher : IPlugin, IBackgroundThreadExceptionCatcher
        {
        }

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _HearbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _AutoConfigWebServer = TestUtilities.CreateMockSingleton<IAutoConfigWebServer>();
            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigWebServer.Setup(s => s.WebServer).Returns(_WebServer.Object);
            _WebSite = TestUtilities.CreateMockImplementation<IWebSite>();
            _FlightSimulatorXAircraftList = TestUtilities.CreateMockImplementation<ISimpleAircraftList>();
            _UniversalPlugAndPlayManager = TestUtilities.CreateMockImplementation<IUniversalPlugAndPlayManager>();
            _ConnectionLogger = TestUtilities.CreateMockSingleton<IConnectionLogger>();
            _LogDatabase = TestUtilities.CreateMockSingleton<ILogDatabase>();
            _BackgroundDataDownloader = TestUtilities.CreateMockSingleton<IBackgroundDataDownloader>();
            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
            _AutoConfigPictureFolderCache = TestUtilities.CreateMockSingleton<IAutoConfigPictureFolderCache>();
            _RebroadcastServerManager = TestUtilities.CreateMockSingleton<IRebroadcastServerManager>();
            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();
            _UserManager.Setup(r => r.GetUserByLoginName(It.IsAny<string>())).Returns((string name) => null);
            _AircraftOnlineLookupManager = TestUtilities.CreateMockSingleton<IAircraftOnlineLookupManager>();
            _StandaloneAircraftOnlineLookupCache = TestUtilities.CreateMockImplementation<IStandaloneAircraftOnlineLookupCache>();
            _AircraftOnlineLookupLog = TestUtilities.CreateMockSingleton<IAircraftOnlineLookupLog>();
            _User = TestUtilities.CreateMockImplementation<IUser>();

            _BackgroundExceptionEvent = new EventRecorder<EventArgs<Exception>>();

            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);
            _BaseStationDatabase.Setup(d => d.FileName).Returns("x");
            _BaseStationDatabase.Setup(d => d.TestConnection()).Returns(true);

            _Provider = new Mock<ISplashPresenterProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(true);

            _Presenter = Factory.Singleton.Resolve<ISplashPresenter>();
            _Presenter.Provider = _Provider.Object;

            _View = new Mock<ISplashView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void SplashPresenter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Presenter = Factory.Singleton.Resolve<ISplashPresenter>();

            Assert.IsNotNull(_Presenter.Provider);
            TestUtilities.TestProperty(_Presenter, "Provider", _Presenter.Provider, _Provider.Object);
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void SplashPresenter_Initialise_Sets_Application_Title()
        {
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(Strings.VirtualRadarServer, _View.Object.ApplicationName);
        }

        [TestMethod]
        public void SplashPresenter_Initialise_Sets_Application_Version()
        {
            _ApplicationInformation.Setup(p => p.ShortVersion).Returns("1.2.3");
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("1.2.3", _View.Object.ApplicationVersion);
        }
        #endregion

        #region StartApplication
        #region Parsing command-line arguments
        [TestMethod]
        public void SplashPresenter_StartApplication_Reports_Parsing_Command_Line_Parameters()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProgress(Strings.SplashScreenParsingCommandLineParameters), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Reports_Problems_With_Unknown_Command_Line_Parameters()
        {
            foreach(string parameter in new string[] { "culture", "-culture", "workingFolder", "-workingFolder", "createAdmin", "-createAdmin" }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);
                _Presenter.CommandLineArgs = new string[] { parameter };
                _Presenter.StartApplication();

                _View.Verify(v => v.ReportProblem(String.Format(Strings.UnrecognisedCommandLineParameterFull, parameter), Strings.UnrecognisedCommandLineParameterTitle, true), Times.Once());
            }
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Stop_On_Acceptable_Command_Line_Parameters()
        {
            foreach(string parameter in new string[] { "-culture:", "-culture:X", "-culture:de-DE", "-CULTURE:en-US", "-WORKINGFOLDER:X", "-showConfigFolder" }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);
                _Presenter.CommandLineArgs = new string[] { parameter };
                _Presenter.StartApplication();

                _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            }
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Stops_Application_Working_Folder_Command_Line_Argument_Specifies_Invalid_Folder()
        {
            _Presenter.CommandLineArgs = new string[] { "-workingfolder:x" };
            _Provider.Setup(p => p.FolderExists("x")).Returns(false);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(String.Format(Strings.FolderDoesNotExistFull, "x"), Strings.FolderDoesNotExistTitle, true), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Overrides_Configuration_Folder_If_Command_Line_Argument_Requests_It()
        {
            _Presenter.CommandLineArgs = new string[] { "-workingfolder:x" };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            Assert.AreEqual("x", _ConfigurationStorage.Object.Folder);
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_CoarseListenerTimeout_Command_Line_Switch_Is_Benign()
        {
            _Presenter.CommandLineArgs = new string[] { "-ListenerTimeout:60" };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Report_Invalid_CoarseListenerTimeout()
        {
            foreach(var isValid in new bool[] { false, true }) {
                TestCleanup();
                TestInitialise();

                _Presenter.CommandLineArgs = new string[] { String.Format("-ListenerTimeout:{0}", isValid ? 10 : 9) };
                _Presenter.Initialise(_View.Object);
                _Presenter.StartApplication();

                _View.Verify(v => v.ReportProblem(Strings.CoarseListenerTimeoutInvalid, Strings.BadListenerTimeout, true), Times.Never());
            }
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Creates_Admin_User_If_Requested()
        {
            _User.Object.UniqueId = "ID";
            var expectedHash = new Hash("the-password");
            _UserManager.Setup(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>())).Callback((IUser user, Hash hash) => {
                Assert.IsTrue(expectedHash.Buffer.SequenceEqual(hash.Buffer));
            });

            _Presenter.CommandLineArgs = new string[] { "-createAdmin:the-user-name", "-password:the-password" };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            Assert.AreEqual(true, _User.Object.Enabled);
            Assert.AreEqual("the-user-name", _User.Object.LoginName);
            Assert.AreEqual("the-user-name", _User.Object.Name);
            _UserManager.Verify(r => r.CreateUserWithHash(_User.Object, It.IsAny<Hash>()), Times.Once());

            Assert.AreEqual(1, _Configuration.WebServerSettings.AdministratorUserIds.Count);
            Assert.AreEqual("ID", _Configuration.WebServerSettings.AdministratorUserIds[0]);
            _ConfigurationStorage.Verify(r => r.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Create_Admin_User_If_Name_Missing()
        {
            _Presenter.CommandLineArgs = new string[] { "-createAdmin:", "-password:the-password" };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Create_Admin_User_If_Password_Missing()
        {
            _Presenter.CommandLineArgs = new string[] { "-createAdmin:the-user-name", };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Create_Admin_User_If_User_Already_Exists()
        {
            _UserManager.Setup(r => r.GetUserByLoginName("the-user-name")).Returns(_User.Object);
            _Presenter.CommandLineArgs = new string[] { "-createAdmin:the-user-name", "-password:the-password" };
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once());
        }
        #endregion

        #region Initialising the log
        [TestMethod]
        public void SplashPresenter_StartApplication_Reports_Initialising_The_Log()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProgress(Strings.SplashScreenInitialisingLog), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Truncates_The_Log()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _Log.Verify(g => g.Truncate(100), Times.Once());
            _Log.Verify(g => g.Truncate(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Records_Startup_In_Log()
        {
            var buildDate = new DateTime(2015, 6, 9, 10, 11, 12);
            _ApplicationInformation.Setup(p => p.FullVersion).Returns("5.4.3.2");
            _ApplicationInformation.Setup(p => p.BuildDate).Returns(buildDate);
            _ConfigurationStorage.Setup(c => c.Folder).Returns(@"c:\abc");

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _Log.Verify(g => g.WriteLine("Program started, version {0}, build date {1} UTC", "5.4.3.2", buildDate), Times.Once());
            _Log.Verify(g => g.WriteLine("Working folder {0}", @"c:\abc"), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Records_Custom_Working_Folder_In_Log()
        {
            _Presenter.CommandLineArgs = new string[] { "-workingFolder:xyz" };

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _Log.Verify(g => g.WriteLine("Working folder {0}", @"xyz"), Times.Once());
            _Log.Verify(g => g.WriteLine("Working folder {0}", It.IsAny<string>()), Times.Once());
        }
        #endregion

        #region Loading the configuration for the first time
        [TestMethod]
        public void SplashPresenter_StartApplication_Does_First_Load_Of_Configuration()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });

            bool firstLoad = true;
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration).Callback(() => {
                if(firstLoad) Assert.AreEqual(Strings.SplashScreenLoadingConfiguration, currentSection);
                firstLoad = false;
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _ConfigurationStorage.Verify(c => c.Load(), Times.AtLeastOnce());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Offers_User_Chance_To_Reset_Configuration_If_It_Cannot_Be_Loaded()
        {
            // A bug in an early version of VRS lead to configuration files that would throw an exception on load which, if left
            // unhandled, could prevent the application from loading at all
            _ConfigurationStorage.Setup(c => c.Load()).Returns(() => { throw new InvalidOperationException("Blah"); });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            string message = String.Format(Strings.InvalidConfigurationFileFull, "Blah", _ConfigurationStorage.Object.Folder);
            _View.Verify(v => v.YesNoPrompt(message, Strings.InvalidConfigurationFileTitle, true), Times.Once());
            _View.Verify(v => v.YesNoPrompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Will_Reset_Configuration_If_User_Requests_It_After_Load_Throws()
        {
            _ConfigurationStorage.Setup(c => c.Load()).Returns(() => { throw new InvalidOperationException("Blah"); });
            _View.Setup(v => v.YesNoPrompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
            _View.Setup(v => v.ReportProblem(Strings.DefaultSettingsSavedFull, Strings.DefaultSettingsSavedTitle, true)).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Save(It.IsAny<Configuration>()), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(Strings.DefaultSettingsSavedFull, Strings.DefaultSettingsSavedTitle, true), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Will_Just_Quit_If_User_Requests_It_After_Load_Throws()
        {
            _ConfigurationStorage.Setup(c => c.Load()).Returns(() => { throw new InvalidOperationException("Blah"); });
            _View.Setup(v => v.YesNoPrompt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(false);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _ConfigurationStorage.Verify(c => c.Save(It.IsAny<Configuration>()), Times.Never());
            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            _Provider.Verify(p => p.AbortApplication(), Times.Once());
        }
        #endregion

        #region Heartbeat timer
        [TestMethod]
        public void SplashPresenter_StartApplication_Starts_The_HeartbeatService()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _HearbeatService.Verify(h => h.Start(), Times.Once());
        }
        #endregion

        #region UserManager
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_UserManager()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });

            _UserManager.Setup(r => r.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenInitialisingUserManager, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _UserManager.Verify(a => a.Initialise(), Times.Once());
        }
        #endregion

        #region BaseStation database connection
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_AutoConfigBaseStationDatabase()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });

            _AutoConfigBaseStationDatabase.Setup(a => a.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenOpeningBaseStationDatabase, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AutoConfigBaseStationDatabase.Verify(a => a.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Tests_The_BaseStation_Database_Connection()
        {
            _BaseStationDatabase.Setup(d => d.TestConnection()).Callback(() => {
                _AutoConfigBaseStationDatabase.Verify(a => a.Initialise(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _BaseStationDatabase.Verify(d => d.TestConnection(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Open_BaseStation_Database_Until_After_Configuration_Has_Been_Checked()
        {
            _BaseStationDatabase.Setup(v => v.TestConnection()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _BaseStationDatabase.Verify(d => d.TestConnection(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Try_Opening_BaseStation_Database_If_FileName_Is_Null()
        {
            _BaseStationDatabase.Setup(d => d.FileName).Returns((string)null);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _BaseStationDatabase.Verify(d => d.TestConnection(), Times.Never());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Report_Problem_Opening_BaseStation_Database()
        {
            _BaseStationDatabase.Setup(d => d.FileName).Returns("xyz");
            _BaseStationDatabase.Setup(d => d.TestConnection()).Returns(false);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(String.Format(Strings.CannotOpenBaseStationDatabaseFull, "xyz"), Strings.CannotOpenBaseStationDatabaseTitle, false), Times.Never());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Report_Problem_Opening_BaseStation_Database_If_FileName_Is_Null()
        {
            _BaseStationDatabase.Setup(d => d.FileName).Returns((string)null);
            _BaseStationDatabase.Setup(d => d.TestConnection()).Returns(false);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Report_Problem_If_BaseStation_Database_Can_Be_Opened()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Prompts_For_AutoFix_If_TestConnection_Throws_Exception()
        {
            var exception = new Exception();
            _BaseStationDatabase.Setup(d => d.FileName).Returns("xyz");
            _BaseStationDatabase.Setup(d => d.TestConnection()).Callback(() => { throw exception; });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(v => v.YesNoPrompt(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, false), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Calls_AutoFix_If_User_Replies_Yes_After_TestConnection_Throws_Exception()
        {
            var exception = new Exception();
            _BaseStationDatabase.Setup(d => d.FileName).Returns("xyz");
            _BaseStationDatabase.Setup(d => d.TestConnection()).Callback(() => { throw exception; });
            _View.Setup(r => r.YesNoPrompt(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, It.IsAny<bool>())).Returns(true);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _BaseStationDatabase.Verify(r => r.AttemptAutoFix(exception), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Stops_Program_If_User_Replies_No_After_TestConnection_Throws_Exception()
        {
            var exception = new Exception();
            _BaseStationDatabase.Setup(d => d.FileName).Returns("xyz");
            _BaseStationDatabase.Setup(d => d.TestConnection()).Callback(() => { throw exception; });
            _View.Setup(r => r.YesNoPrompt(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, It.IsAny<bool>())).Returns(false);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _View.Verify(r => r.ReportProblem(It.IsAny<string>(), Strings.CannotOpenBaseStationDatabaseTitle, true), Times.Once());
        }
        #endregion

        #region Picture folder cache
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_Picture_Folder_Cache()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _AutoConfigPictureFolderCache.Setup(a => a.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenStartingPictureFolderCache, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AutoConfigPictureFolderCache.Verify(a => a.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_Picture_Folder_Cache_After_Loading_Configuration()
        {
            _AutoConfigPictureFolderCache.Setup(a => a.Initialise()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AutoConfigPictureFolderCache.Verify(a => a.Initialise(), Times.Once());
        }
        #endregion

        #region Standing data
        [TestMethod]
        public void SplashPresenter_StartApplication_Loads_Standing_Data()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _StandingDataManager.Setup(m => m.Load()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenLoadingStandingData, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _StandingDataManager.Verify(m => m.Load(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Loads_Standing_Data_After_Loading_Configuration()
        {
            _StandingDataManager.Setup(m => m.Load()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _StandingDataManager.Verify(m => m.Load(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Starts_Background_Downloader()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _BackgroundDataDownloader.Verify(b => b.Start(), Times.Once());
        }
        #endregion

        #region FeedManager
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_FeedManager()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _FeedManager.Setup(m => m.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenConnectingToBaseStation, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _FeedManager.Verify(b => b.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_FeedManager_After_Loading_Configuration()
        {
            _FeedManager.Setup(m => m.Initialise()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _FeedManager.Verify(m => m.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Hooks_FeedManager_Background_Exception_Event()
        {
            _Presenter.BackgroundThreadExceptionHandler = _BackgroundExceptionEvent.Handler;
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _FeedManager.Raise(b => b.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(1, _BackgroundExceptionEvent.CallCount);
        }
        #endregion

        #region Web WebServer and Web Site
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_AutoConfigWebServer()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AutoConfigWebServer.Verify(a => a.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Hooks_Web_Server_Background_Exception_Event()
        {
            _Presenter.BackgroundThreadExceptionHandler = _BackgroundExceptionEvent.Handler;
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            var exception = new InvalidOperationException();
            _WebServer.Raise(b => b.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(1, _BackgroundExceptionEvent.CallCount);
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Attches_ConnectionLogger_To_Server()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            Assert.AreSame(_WebServer.Object, _ConnectionLogger.Object.WebServer);
            Assert.AreSame(_LogDatabase.Object, _ConnectionLogger.Object.LogDatabase);
            _ConnectionLogger.Verify(c => c.Start(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Attches_ConnectionLogger_ExceptionCaught_Handler()
        {
            _Presenter.BackgroundThreadExceptionHandler = _BackgroundExceptionEvent.Handler;
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            var exception = new InvalidOperationException();
            _ConnectionLogger.Raise(b => b.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(1, _BackgroundExceptionEvent.CallCount);
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Attaches_Web_Site_To_Web_Server()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _WebSite.Setup(s => s.AttachSiteToServer(_WebServer.Object)).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenStartingWebServer, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _WebSite.Verify(s => s.AttachSiteToServer(_WebServer.Object), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Attaches_Web_Site_After_Configuration_Has_Loaded()
        {
            _WebSite.Setup(s => s.AttachSiteToServer(_WebServer.Object)).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Sets_Properties_On_Web_Site()
        {
            _WebSite.Setup(s => s.AttachSiteToServer(_WebServer.Object)).Callback(() => {
                Assert.AreSame(_BaseStationDatabase.Object, _WebSite.Object.BaseStationDatabase);
                Assert.AreSame(_FlightSimulatorXAircraftList.Object, _WebSite.Object.FlightSimulatorAircraftList);
                Assert.AreSame(_StandingDataManager.Object, _WebSite.Object.StandingDataManager);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Starts_Web_Server_Once_Site_And_Server_Are_Initialised()
        {
            _WebServer.SetupSet(s => s.Online = true).Callback(() => {
                _AutoConfigWebServer.Verify(a => a.Initialise(), Times.Once());
                _WebSite.Verify(s => s.AttachSiteToServer(_WebServer.Object), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _WebServer.VerifySet(s => s.Online = true, Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Picks_Up_HttpListenerExceptions_When_Starting_WebServer()
        {
            var exception = new HttpListenerException();
            _WebServer.SetupSet(s => s.Online = true).Callback(() => {
                throw exception;
            });
            _WebServer.Setup(a => a.Port).Returns(123);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _Log.Verify(g => g.WriteLine("Caught exception when starting web server: {0}", exception.ToString()), Times.Once());
            _View.Verify(v => v.ReportProblem(String.Format(Strings.CannotStartWebServerFull, 123), Strings.CannotStartWebServerTitle, false), Times.Once());
            _View.Verify(v => v.ReportProblem(Strings.SuggestUseDifferentPortFull, Strings.SuggestUseDifferentPortTitle, false), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Picks_Up_SocketExceptions_When_Starting_WebServer()
        {
            var exception = new SocketException();
            _WebServer.SetupSet(s => s.Online = true).Callback(() => {
                throw exception;
            });
            _WebServer.Setup(a => a.Port).Returns(123);

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _Log.Verify(g => g.WriteLine("Caught exception when starting web server: {0}", exception.ToString()), Times.Once());
            _View.Verify(v => v.ReportProblem(String.Format(Strings.CannotStartWebServerFull, 123), Strings.CannotStartWebServerTitle, false), Times.Once());
            _View.Verify(v => v.ReportProblem(Strings.SuggestUseDifferentPortFull, Strings.SuggestUseDifferentPortTitle, false), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Copies_Web_Site_Flight_Simulator_Aircraft_List_To_View()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            Assert.AreEqual(_FlightSimulatorXAircraftList.Object, _View.Object.FlightSimulatorXAircraftList);
        }
        #endregion

        #region RebroadcastManager
        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_RebroadcastManager()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _RebroadcastServerManager.Setup(r => r.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenStartingRebroadcastServers, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _RebroadcastServerManager.Verify(r => r.Initialise(), Times.Once());
            Assert.AreEqual(true, _RebroadcastServerManager.Object.Online);
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_RebroadcastManager_After_Loading_Configuration()
        {
            _RebroadcastServerManager.Setup(m => m.Initialise()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _StandingDataManager.Verify(m => m.Load(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Hooks_RebroadcastManager_Background_Exception_Event()
        {
            _Presenter.BackgroundThreadExceptionHandler = _BackgroundExceptionEvent.Handler;
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            var exception = new InvalidOperationException();
            _RebroadcastServerManager.Raise(b => b.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(1, _BackgroundExceptionEvent.CallCount);
        }
        #endregion

        #region UPnP Manager
        [TestMethod]
        public void SplashPresenter_StartApplication_Starts_UPnP_Manager()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _UniversalPlugAndPlayManager.Setup(s => s.Initialise()).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenInitialisingUPnPManager, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _UniversalPlugAndPlayManager.Verify(s => s.Initialise(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Starts_UPnP_Manager_After_Loading_Configuration()
        {
            _UniversalPlugAndPlayManager.Setup(s => s.Initialise()).Callback(() => {
                _ConfigurationStorage.Verify(c => c.Load(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Sets_Properties_On_UPnP_Manager()
        {
            _UniversalPlugAndPlayManager.Setup(s => s.Initialise()).Callback(() => {
                Assert.AreSame(_WebServer.Object, _UniversalPlugAndPlayManager.Object.WebServer);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Puts_Server_Onto_Internet_If_Configuration_Allows()
        {
            _Configuration.WebServerSettings.AutoStartUPnP = true;

            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _UniversalPlugAndPlayManager.Setup(s => s.PutServerOntoInternet()).Callback(() => {
                _UniversalPlugAndPlayManager.Verify(m => m.Initialise(), Times.Once());
                Assert.AreEqual(Strings.SplashScreenStartingUPnP, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _UniversalPlugAndPlayManager.Verify(s => s.PutServerOntoInternet(), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Does_Not_Put_Server_Onto_Internet_If_Configuration_Forbids()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _UniversalPlugAndPlayManager.Verify(s => s.PutServerOntoInternet(), Times.Never());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Copies_UPnP_Manager_To_View()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            Assert.AreEqual(_UniversalPlugAndPlayManager.Object, _View.Object.UPnpManager);
        }
        #endregion

        #region OnlineLookupManager
        [TestMethod]
        public void SplashPresenter_StartApplication_Registers_Standalone_Online_Cache()
        {
            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            _AircraftOnlineLookupManager.Setup(r => r.RegisterCache(It.IsAny<IAircraftOnlineLookupCache>(), It.IsAny<int>(), It.IsAny<bool>())).Callback(() => {
                Assert.AreEqual(Strings.SplashScreenStartingOnlineLookupManager, currentSection);
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AircraftOnlineLookupManager.Verify(s => s.RegisterCache(_StandaloneAircraftOnlineLookupCache.Object, 0, true), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Initialises_AircraftDetailOnlineLookupLog()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            _AircraftOnlineLookupLog.Verify(r => r.Initialise(), Times.Once());
        }
        #endregion

        #region Plugins
        [TestMethod]
        public void SplashPresenter_StartApplication_Calls_Startup_On_All_Loaded_Plugins()
        {
            var plugin1 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var plugin2 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new IPlugin[] { plugin1.Object, plugin2.Object });

            string currentSection = null;
            _View.Setup(v => v.ReportProgress(It.IsAny<string>())).Callback((string section) => { currentSection = section; });
            plugin1.Setup(p => p.Startup(It.IsAny<PluginStartupParameters>())).Callback(() => { Assert.AreEqual(Strings.SplashScreenStartingPlugins, currentSection); });
            plugin2.Setup(p => p.Startup(It.IsAny<PluginStartupParameters>())).Callback(() => { Assert.AreEqual(Strings.SplashScreenStartingPlugins, currentSection); });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            plugin1.Verify(p => p.Startup(It.IsAny<PluginStartupParameters>()), Times.Once());
            plugin2.Verify(p => p.Startup(It.IsAny<PluginStartupParameters>()), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Reports_Problems_Starting_A_Plugin_To_User()
        {
            var plugin1 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var plugin2 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            plugin1.Setup(p => p.Name).Returns("P1");
            plugin2.Setup(p => p.Name).Returns("P2");
            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new IPlugin[] { plugin1.Object, plugin2.Object });

            var exception = new InvalidOperationException();
            plugin1.Setup(p => p.Startup(It.IsAny<PluginStartupParameters>())).Callback(() => { throw exception; });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            plugin1.Verify(p => p.Startup(It.IsAny<PluginStartupParameters>()), Times.Once());
            plugin2.Verify(p => p.Startup(It.IsAny<PluginStartupParameters>()), Times.Once());

            _View.Verify(v => v.ReportProblem(String.Format(Strings.PluginThrewExceptionFull, "P1", exception.Message), Strings.PluginThrewExceptionTitle, false), Times.Once());
            _View.Verify(v => v.ReportProblem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());

            _Log.Verify(g => g.WriteLine("Caught exception when starting {0}: {1}", new object[] { "P1", exception.ToString() }), Times.Once());
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Sends_Correct_Parameters_To_Plugin_Startup()
        {
            var pluginFolder = @"c:\Program Files\VirtualRadar\Plugin\MyPluginFolder";
            var plugin = TestUtilities.CreateMockInstance<IPlugin>();
            plugin.Setup(r => r.PluginFolder).Returns(pluginFolder);

            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new IPlugin[] { plugin.Object });

            PluginStartupParameters parameters = null;  // we can't just test within Startup.Callback because exceptions from there are caught by design, they won't stop the test
            plugin.Setup(p => p.Startup(It.IsAny<PluginStartupParameters>())).Callback((PluginStartupParameters p) => { parameters = p; });

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            plugin.Verify(p => p.Startup(It.IsAny<PluginStartupParameters>()), Times.Once());
            Assert.AreSame(_FlightSimulatorXAircraftList.Object, parameters.FlightSimulatorAircraftList);
            Assert.AreSame(_UniversalPlugAndPlayManager.Object, parameters.UPnpManager);
            Assert.AreSame(_WebSite.Object, parameters.WebSite);
            Assert.AreEqual(pluginFolder, parameters.PluginFolder);
        }

        [TestMethod]
        public void SplashPresenter_StartApplication_Hooks_ExceptionCaught_For_Plugins_That_Need_To_Raise_Background_Exceptions_On_GUI_Thread()
        {
            var plugin = new Mock<IPluginBackgroundThreadCatcher>()  { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new IPlugin[] { plugin.Object });

            _Presenter.BackgroundThreadExceptionHandler += _BackgroundExceptionEvent.Handler;

            _Presenter.Initialise(_View.Object);
            _Presenter.StartApplication();

            var exception = new InvalidOperationException();
            plugin.Raise(p => p.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _BackgroundExceptionEvent.CallCount);
            Assert.AreSame(exception, _BackgroundExceptionEvent.Args.Value);
            Assert.AreSame(plugin.Object, _BackgroundExceptionEvent.Sender);
        }
        #endregion
        #endregion
    }
}
