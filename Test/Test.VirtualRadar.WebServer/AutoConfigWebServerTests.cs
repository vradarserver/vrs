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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebServer;
using InterfaceFactory;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.WebServer
{
    [TestClass]
    public class AutoConfigWebServerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IAutoConfigWebServer _AutoConfigWebServer;
        private Mock<IWebServer> _WebServer;
        private Mock<IWebServerProvider> _WebServerProvider;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private InstallerSettings _InstallerSettings;
        private Mock<IInstallerSettingsStorage> _InstallerSettingsStorage;
        private Mock<IExternalIPAddressService> _ExternalIPAddressService;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<ILog> _Log;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _WebServer = TestUtilities.CreateMockImplementation<IWebServer>();
            _WebServerProvider = TestUtilities.CreateMockImplementation<IWebServerProvider>();
            _WebServer.Setup(r => r.Provider).Returns(_WebServerProvider.Object);
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _ExternalIPAddressService = TestUtilities.CreateMockSingleton<IExternalIPAddressService>();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _Configuration = new Configuration();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(s => s.Load()).Returns(_Configuration);
            _InstallerSettingsStorage = TestUtilities.CreateMockImplementation<IInstallerSettingsStorage>();
            _InstallerSettings = new InstallerSettings();
            _InstallerSettingsStorage.Setup(s => s.Load()).Returns(_InstallerSettings);

            _AutoConfigWebServer = Factory.ResolveNewInstance<IAutoConfigWebServer>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and properties
        [TestMethod]
        public void AutoConfigWebServer_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsNull(_AutoConfigWebServer.WebServer);
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void AutoConfigWebServer_Initialise_Sets_Properties()
        {
            _AutoConfigWebServer.Initialise();
            Assert.AreSame(_WebServer.Object, _AutoConfigWebServer.WebServer);
        }

        [TestMethod]
        public void AutoConfigWebServer_Initialise_Applies_Configuration_Settings_To_WebServer()
        {
            _Configuration.WebServerSettings.UPnpPort = 9988;
            _AutoConfigWebServer.Initialise();

            Assert.AreEqual(9988, _WebServer.Object.ExternalPort);
        }

        [TestMethod]
        public void AutoConfigWebServer_Initialise_Applies_Installer_Configuration_Settings_To_WebServer()
        {
            _InstallerSettings.WebServerPort = 4321;
            _AutoConfigWebServer.Initialise();

            Assert.AreEqual(4321, _WebServer.Object.Port);
        }

        [TestMethod]
        public void AutoConfigWebServer_Initialise_Applies_ExternalIPAddressService_IPAddress_To_WebServer()
        {
            _ExternalIPAddressService.Setup(s => s.Address).Returns("1.2.3.4");
            _AutoConfigWebServer.Initialise();

            Assert.AreEqual("1.2.3.4", _WebServer.Object.ExternalIPAddress);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void AutoConfigWebServer_Dispose_Disposes_Of_Web_Server()
        {
            _AutoConfigWebServer.Initialise();
            _AutoConfigWebServer.Dispose();

            _WebServer.Verify(s => s.Dispose(), Times.Once());
        }

        [TestMethod]
        public void AutoConfigWebServer_Dispose_Copes_When_Uninitialised()
        {
            _AutoConfigWebServer.Dispose();  // <-- just needs to not throw an exception
        }
        #endregion

        #region Configuration changes
        [TestMethod]
        public void AutoConfigWebServer_Configuration_Settings_Applied_When_New_Configuration_Saved()
        {
            _AutoConfigWebServer.Initialise();

            _Configuration.WebServerSettings.UPnpPort = 1234;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1234, _WebServer.Object.ExternalPort);
        }
        #endregion

        #region ExternalAddressService changes
        [TestMethod]
        public void AutoConfigWebServer_ExternalIPAddressService_IPAddress_Applied_When_External_IP_Address_Determined()
        {
            _AutoConfigWebServer.Initialise();

            _ExternalIPAddressService.Setup(s => s.Address).Returns("a.b.c.d");
            _ExternalIPAddressService.Raise(s => s.AddressUpdated += null, new EventArgs<string>("a.b.c.d"));

            Assert.AreEqual("a.b.c.d", _WebServer.Object.ExternalIPAddress);
        }
        #endregion

        #region Heartbeat
        [TestMethod]
        public void AutoConfigWebServer_Heartbeat_Triggered_On_Initialise()
        {
            _AutoConfigWebServer.Initialise();
            _HeartbeatService.Verify(s => s.SlowTickNow(), Times.Once());
        }

        [TestMethod]
        public void AutoConfigWebServer_Heartbeat_First_Invocation_Triggers_Fetch_Of_External_IP_Address()
        {
            _AutoConfigWebServer.Initialise();
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _ExternalIPAddressService.Verify(s => s.GetExternalIPAddress(), Times.Once());
        }

        [TestMethod]
        public void AutoConfigWebServer_Heartbeat_Only_Fetches_External_IP_Address_Once()
        {
            _AutoConfigWebServer.Initialise();
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _ExternalIPAddressService.Verify(s => s.GetExternalIPAddress(), Times.Once());
        }

        [TestMethod]
        public void AutoConfigWebServer_Heartbeat_Logs_Exceptions_Raised_During_Background_Fetch_Of_External_IP_Address()
        {
            _AutoConfigWebServer.Initialise();
            _ExternalIPAddressService.Setup(s => s.GetExternalIPAddress()).Callback(() => { throw new NotImplementedException(); });
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _Log.Verify(l => l.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void AutoConfigWebServer_Heartbeat_Only_Tries_To_Fetch_External_IP_Address_After_Exception_Once_Timeout_Has_Elapsed()
        {
            _AutoConfigWebServer.Initialise();
            _ExternalIPAddressService.Setup(s => s.GetExternalIPAddress()).Callback(() => { throw new NotImplementedException(); });

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _ExternalIPAddressService.Verify(s => s.GetExternalIPAddress(), Times.Exactly(1));

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMinutes(5);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _ExternalIPAddressService.Verify(s => s.GetExternalIPAddress(), Times.Exactly(2));
        }
        #endregion

        #region EnableCompression
        [TestMethod]
        public void AutoConfigWebServer_EnableCompression_Copied_To_WebServer_Provider_On_Initialisation()
        {
            foreach(var enableCompression in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Configuration.GoogleMapSettings.EnableCompression = enableCompression;
                _AutoConfigWebServer.Initialise();

                Assert.AreEqual(enableCompression, _WebServerProvider.Object.EnableCompression);
            }
        }

        [TestMethod]
        public void AutoConfigWebServer_EnableCompression_Copied_To_WebServer_On_Configuration_Update()
        {
            _AutoConfigWebServer.Initialise();
            foreach(var enableCompression in new bool[] { true, false, true }) {
                _Configuration.GoogleMapSettings.EnableCompression = enableCompression;
                _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

                Assert.AreEqual(enableCompression, _WebServerProvider.Object.EnableCompression);
            }
        }
        #endregion
    }
}
