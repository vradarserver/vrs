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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class BackgroundDataDownloaderTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IBackgroundDataDownloader _BackgroundDataDownloader;
        private Mock<IBackgroundDataDownloaderProvider> _Provider;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<IStandingDataUpdater> _StandingDataUpdater;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<ILog> _Log;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _StandingDataUpdater = TestUtilities.CreateMockImplementation<IStandingDataUpdater>();
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _Configuration.FlightRouteSettings.AutoUpdateEnabled = true;
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _BackgroundDataDownloader = Factory.Singleton.ResolveNewInstance<IBackgroundDataDownloader>();
            _Provider = new Mock<IBackgroundDataDownloaderProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _BackgroundDataDownloader.Provider = _Provider.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        private DateTime SetUtcNow(DateTime dateTime)
        {
            _Provider.Setup(p => p.UtcNow).Returns(dateTime);
            return dateTime;
        }

        [TestMethod]
        public void BackgroundDataDownloader_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            var downloader = Factory.Singleton.ResolveNewInstance<IBackgroundDataDownloader>();

            Assert.IsNotNull(downloader.Provider);
            TestUtilities.TestProperty(downloader, "Provider", downloader.Provider, _Provider.Object);
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Runs_Updater()
        {
            SetUtcNow(DateTime.Now);
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Does_Not_Reload_Data_After_Updater_Finishes()
        {
            SetUtcNow(DateTime.Now);
            _StandingDataManager.Setup(m => m.Load()).Callback(() => {
                _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
            });
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(m => m.Load(), Times.Never());
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Does_Not_Run_Updater_Or_Loader_If_Configuration_Prohibits_Automated_Check()
        {
            SetUtcNow(DateTime.Now);
            _Configuration.FlightRouteSettings.AutoUpdateEnabled = false;
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _StandingDataUpdater.Verify(u => u.Update(), Times.Never());
            _StandingDataManager.Verify(m => m.Load(), Times.Never());
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Picks_Up_Changes_In_Configuration()
        {
            SetUtcNow(DateTime.Now);
            _Configuration.FlightRouteSettings.AutoUpdateEnabled = false;
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _Configuration.FlightRouteSettings.AutoUpdateEnabled = true;
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Does_Not_Run_Updater_Or_Loader_If_Under_An_Hour_Since_Last_Run()
        {
            var now = SetUtcNow(DateTime.Now);
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            now = SetUtcNow(now.AddHours(1).AddSeconds(-1));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _StandingDataUpdater.Verify(u => u.Update(), Times.Once());

            now = SetUtcNow(now.AddSeconds(1));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _StandingDataUpdater.Verify(u => u.Update(), Times.Exactly(2));
        }

        [TestMethod]
        public void BackgroundDataDownloader_Heartbeat_Tick_Logs_Exceptions_From_Updater()
        {
            SetUtcNow(DateTime.Now);
            var exception = new InvalidOperationException();
            _StandingDataUpdater.Setup(u => u.Update()).Callback(() => { throw exception; });
            _BackgroundDataDownloader.Start();

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _Log.Verify(g => g.WriteLine("Exception caught during data download: {0}", exception.ToString()), Times.Once());
        }
    }
}
