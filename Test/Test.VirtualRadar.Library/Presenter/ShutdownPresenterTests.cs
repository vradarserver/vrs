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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class ShutdownPresenterTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IShutdownPresenter _Presenter;
        private Mock<IShutdownView> _View;
        private Mock<IUniversalPlugAndPlayManager> _UPnpManager;
        private Mock<IPluginManager> _PluginManager;
        private List<IPlugin> _Plugins;
        private Mock<IConnectionLogger> _ConnectionLogger;
        private Mock<IAutoConfigWebServer> _AutoConfigWebServer;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<ILogDatabase> _LogDatabase;
        private Mock<IRebroadcastServerManager> _RebroadcastServerManager;
        private Mock<IFeedManager> _FeedManager;
        private Mock<IUserManager> _UserManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _UPnpManager = new Mock<IUniversalPlugAndPlayManager>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();
            _Plugins = new List<IPlugin>();
            _PluginManager.Setup(m => m.LoadedPlugins).Returns(_Plugins);
            _ConnectionLogger = TestUtilities.CreateMockSingleton<IConnectionLogger>();
            _AutoConfigWebServer = TestUtilities.CreateMockSingleton<IAutoConfigWebServer>();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _LogDatabase = TestUtilities.CreateMockSingleton<ILogDatabase>();
            _RebroadcastServerManager = TestUtilities.CreateMockSingleton<IRebroadcastServerManager>();
            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();

            // This version of ILog should cause code that catches & logs exceptions to throw on the log write. Without this
            // the Asserts can go unnoticed.
            var uncallableLog = new Mock<ILog>(MockBehavior.Strict);
            Factory.Singleton.RegisterInstance<ILog>(uncallableLog.Object);

            _Presenter = Factory.Singleton.Resolve<IShutdownPresenter>();
            _View = new Mock<IShutdownView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void ShutdownPresenter_Initialises_To_Known_State_And_Properties_Work()
        {
            TestUtilities.TestProperty(_Presenter, r => r.UPnpManager, null, _UPnpManager.Object);
        }
        #endregion

        #region ShutdownApplication
        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Closes_Plugins()
        {
            var plugin1 = new Mock<IPlugin>();
            var plugin2 = new Mock<IPlugin>();

            plugin1.Setup(p => p.Name).Returns("X");
            plugin2.Setup(p => p.Name).Returns("B");

            plugin1.Setup(p => p.Shutdown()).Callback(() => {
                _View.Verify(v => v.ReportProgress(String.Format(Strings.ShuttingDownPlugin, "X")), Times.Once());
            });

            plugin2.Setup(p => p.Shutdown()).Callback(() => {
                _View.Verify(v => v.ReportProgress(String.Format(Strings.ShuttingDownPlugin, "B")), Times.Once());
            });

            _Plugins.Add(plugin1.Object);
            _Plugins.Add(plugin2.Object);

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            plugin1.Verify(p => p.Shutdown(), Times.Once());
            plugin2.Verify(p => p.Shutdown(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Logs_Exceptions_Raised_By_Plugins()
        {
            var log = TestUtilities.CreateMockSingleton<ILog>();
            var exception = new InvalidOperationException();
            var plugin = new Mock<IPlugin>();
            plugin.Setup(p => p.Shutdown()).Callback(() => { throw exception; });
            _Plugins.Add(plugin.Object);

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>(), exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Prevents_Exception_In_One_Plugin_Shutdown_From_Stopping_Other_Plugins_Shutting_Down()
        {
            var log = TestUtilities.CreateMockSingleton<ILog>();
            var plugin1 = new Mock<IPlugin>();
            var plugin2 = new Mock<IPlugin>();

            _Plugins.Add(plugin1.Object);
            _Plugins.Add(plugin2.Object);

            plugin1.Setup(p => p.Shutdown()).Callback(() => { throw new InvalidOperationException(); });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            plugin1.Verify(p => p.Shutdown(), Times.Once());
            plugin2.Verify(p => p.Shutdown(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_UPnP_Manager()
        {
            _UPnpManager.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownUPnpManager), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.UPnpManager = _UPnpManager.Object;
            _Presenter.ShutdownApplication();

            _UPnpManager.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_ConnectionLogger()
        {
            _ConnectionLogger.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownConnectionLogger), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _ConnectionLogger.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_WebServer()
        {
            _AutoConfigWebServer.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownWebServer), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _AutoConfigWebServer.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_BaseStationDatabase()
        {
            _AutoConfigBaseStationDatabase.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownBaseStationDatabase), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _AutoConfigBaseStationDatabase.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_LogDatabase()
        {
            _LogDatabase.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownLogDatabase), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _LogDatabase.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_RebroadcastServerManager()
        {
            _RebroadcastServerManager.Setup(m => m.Dispose()).Callback(() => {
                _View.Verify(v => v.ReportProgress(Strings.ShuttingDownRebroadcastServer), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _RebroadcastServerManager.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Disposes_Of_FeedManager()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _FeedManager.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ShutdownPresenter_ShutdownApplication_Shuts_Down_UserManager()
        {
            _Presenter.Initialise(_View.Object);
            _Presenter.ShutdownApplication();

            _UserManager.Verify(r => r.Shutdown(), Times.Once());
        }
        #endregion
    }
}
