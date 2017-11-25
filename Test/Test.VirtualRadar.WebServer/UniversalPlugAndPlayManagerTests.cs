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
    public class UniversalPlugAndPlayManagerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        const string _Description = "Virtual Radar Server";
        private IClassFactory _OriginalFactory;
        private IUniversalPlugAndPlayManager _Manager;
        private Mock<IUniversalPlugAndPlayManagerProvider> _Provider;
        private List<IPortMapping> _PortMappings;
        private Mock<IWebServer> _WebServer;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private EventRecorder<EventArgs> _StateChangedEvent;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _PortMappings = new List<IPortMapping>();
            _Manager = Factory.Singleton.Resolve<IUniversalPlugAndPlayManager>();
            _Provider = new Mock<IUniversalPlugAndPlayManagerProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.GetPortMappings()).Returns(_PortMappings);
            _Manager.Provider = _Provider.Object;

            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Manager.WebServer = _WebServer.Object;

            _StateChangedEvent = new EventRecorder<EventArgs>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }

        private void AddPortMapping(string description, int externalPort, string internalClient, int internalPort, string protocol)
        {
            var portMapping = new Mock<IPortMapping>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            portMapping.Setup(p => p.Description).Returns(description);
            portMapping.Setup(p => p.ExternalPort).Returns(externalPort);
            portMapping.Setup(p => p.InternalClient).Returns(internalClient);
            portMapping.Setup(p => p.InternalPort).Returns(internalPort);
            portMapping.Setup(p => p.Protocol).Returns(protocol);

            _PortMappings.Add(portMapping.Object);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void UniversalPlugAndPlayManager_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var manager = Factory.Singleton.Resolve<IUniversalPlugAndPlayManager>();
            Assert.IsNotNull(manager.Provider);
            TestUtilities.TestProperty(manager, "Provider", manager.Provider, _Provider.Object);

            Assert.IsFalse(manager.IsEnabled);
            Assert.IsFalse(manager.IsRouterPresent);
            Assert.IsFalse(manager.PortForwardingPresent);

            TestUtilities.TestProperty(manager, "WebServer", null, _WebServer.Object);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_IsEnabled_Reflects_Configuration_Setting()
        {
            Assert.IsFalse(_Manager.IsEnabled);

            _Configuration.WebServerSettings.EnableUPnp = true;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            Assert.IsTrue(_Manager.IsEnabled);

            _Configuration.WebServerSettings.EnableUPnp = false;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            Assert.IsFalse(_Manager.IsEnabled);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_IsEnabled_Raises_StateChanged_If_Changed()
        {
            _Manager.StateChanged += _StateChangedEvent.Handler;

            _Configuration.WebServerSettings.EnableUPnp = true;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _StateChangedEvent.CallCount);
            Assert.AreSame(_Manager, _StateChangedEvent.Sender);
            Assert.IsNotNull(_StateChangedEvent.Args);

            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _StateChangedEvent.CallCount);

            _Configuration.WebServerSettings.EnableUPnp = false;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(2, _StateChangedEvent.CallCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void UniversalPlugAndPlayManager_Dispose_Removes_The_Port_Mapping_If_Established()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Manager.Dispose();

            _Provider.Verify(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Dispose_Does_Not_Remove_The_Port_Mapping_If_Not_Established()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();
            _Manager.Dispose();

            _Provider.Verify(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Dispose_Does_Not_Remove_The_Port_Mapping_If_Never_Initialised()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Manager.Dispose();

            _Provider.Verify(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UniversalPlugAndPlayManager_Initialise_Throws_If_WebServer_Is_Null()
        {
            _Manager.WebServer = null;
            _Manager.Initialise();
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Creates_The_UPnP_COM_Object()
        {
            _Manager.Initialise();
            _Provider.Verify(p => p.CreateUPnPComComponent(), Times.Once());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Sets_IsRouterPresent_If_UPnP_COM_Component_Does_Not_Throw()
        {
            _Manager.Initialise();
            Assert.IsTrue(_Manager.IsRouterPresent);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Clears_IsRouterPresent_If_UPnP_COM_Component_Throws()
        {
            _Provider.Setup(p => p.CreateUPnPComComponent()).Callback(() => { throw new InvalidOperationException(); });
            _Manager.Initialise();
            Assert.IsFalse(_Manager.IsRouterPresent);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Clears_IsRouterPresent_If_UPnP_COM_Component_Does_Not_Expose_Static_Port_Mappings()
        {
            _Provider.Setup(p => p.GetPortMappings()).Returns((List<IPortMapping>)null);
            _Manager.Initialise();
            Assert.IsFalse(_Manager.IsRouterPresent);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Clears_IsRouterPresent_If_UPnP_COM_ComponentThrows_When_Exposing_Static_Port_Mappings()
        {
            _Provider.Setup(p => p.GetPortMappings()).Callback(() => { throw new ApplicationException(); });
            _Manager.Initialise();
            Assert.IsFalse(_Manager.IsRouterPresent);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Raises_StateChanged()
        {
            _Manager.StateChanged += _StateChangedEvent.Handler;
            _Manager.Initialise();
            Assert.AreEqual(1, _StateChangedEvent.CallCount);
            Assert.AreSame(_Manager, _StateChangedEvent.Sender);
            Assert.AreNotEqual(null, _StateChangedEvent.Args);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Initialise_Raises_StateChanged_Even_If_Exception_Thrown()
        {
            _Provider.Setup(p => p.CreateUPnPComComponent()).Callback(() => { throw new NotImplementedException(); });
            _Manager.StateChanged += _StateChangedEvent.Handler;
            _Manager.Initialise();
            Assert.AreEqual(1, _StateChangedEvent.CallCount);
            Assert.AreSame(_Manager, _StateChangedEvent.Sender);
            Assert.AreNotEqual(null, _StateChangedEvent.Args);
        }
        #endregion

        #region PutServerOntoInternet
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UniversalPlugAndPlayManager_PutServerOntoInternet_Throws_If_Called_Before_Initialise()
        {
            _Manager.PutServerOntoInternet();
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_PutServerOntoInternet_Does_Not_Attempt_Calls_If_Initialise_Failed()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Provider.Setup(p => p.CreateUPnPComComponent()).Callback(() => { throw new InvalidOperationException(); });

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();

            _Provider.Verify(p => p.AddMapping(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never());
            Assert.IsFalse(_Manager.PortForwardingPresent);
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AddPortForwarding$")]
        public void UniversalPlugAndPlayManager_PutServerOntoInternet_Adds_Correct_Mapping_To_Router()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Configuration.WebServerSettings.EnableUPnp = worksheet.Bool("CnfEnabled");
            _Configuration.WebServerSettings.UPnpPort = worksheet.Int("CnfUPnpPort");
            _WebServer.Setup(s => s.NetworkIPAddress).Returns(worksheet.String("SvrAddress"));
            _WebServer.Setup(s => s.Port).Returns(worksheet.Int("SvrPort"));

            _Manager.Initialise();

            if(worksheet.String("RtrDescription") != null) {
                AddPortMapping(worksheet.String("RtrDescription"),
                               worksheet.Int("RtrExtPort"),
                               worksheet.String("RtrIntClient"),
                               worksheet.Int("RtrIntPort"),
                               worksheet.String("RtrProtocol"));
            }

            _Manager.StateChanged += _StateChangedEvent.Handler;
            _Manager.PutServerOntoInternet();

            _Provider.Verify(p => p.AddMapping(worksheet.Int("CnfUPnpPort"), "TCP", worksheet.Int("SvrPort"), worksheet.String("SvrAddress"), true, _Description),
                worksheet.Bool("ExpectAdd") ? Times.Once() : Times.Never());

            Assert.AreEqual(worksheet.Bool("ExpectIsPresent"), _Manager.PortForwardingPresent);
            Assert.AreEqual(worksheet.Bool("ExpectStateChanged") ? 1 : 0, _StateChangedEvent.CallCount);
            if(_StateChangedEvent.CallCount > 0) {
                Assert.AreSame(_Manager, _StateChangedEvent.Sender);
                Assert.IsNotNull(_StateChangedEvent.Args);
            }
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_PutServerOntoInternet_Copes_If_GetPortMappings_Throws()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 1234;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("10.11.12.13");
            _WebServer.Setup(s => s.Port).Returns(8080);

            _Provider.Setup(p => p.GetPortMappings()).Callback(() => { throw new NotImplementedException(); });

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();

            _Provider.Verify(p => p.AddMapping(1234, "TCP", 8080, "10.11.12.13", true, _Description), Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_PutServerOntoInternet_Copes_If_AddMapping_Throws()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 1234;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("10.11.12.13");
            _WebServer.Setup(s => s.Port).Returns(8080);

            _Provider.Setup(p => p.AddMapping(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).Callback(() => { throw new ApplicationException(); });

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();

            _Provider.Verify(p => p.AddMapping(1234, "TCP", 8080, "10.11.12.13", true, _Description), Times.Once());
            Assert.IsFalse(_Manager.PortForwardingPresent);
        }
        #endregion

        #region TakeServerOffInternet
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Throws_If_Initialse_Not_Called_First()
        {
            _Manager.TakeServerOffInternet();
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Does_Not_Attempt_Calls_If_Initialise_Failed()
        {
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Provider.Setup(p => p.CreateUPnPComComponent()).Callback(() => { throw new InvalidOperationException(); });

            _Manager.Initialise();
            _Manager.TakeServerOffInternet();

            _Provider.Verify(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Behaves_Correctly_If_GetPortMappings_Fails()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();

            _Provider.Setup(p => p.GetPortMappings()).Callback(() => { throw new InvalidOperationException(); });
            _Manager.TakeServerOffInternet();

            _Provider.Verify(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            Assert.IsTrue(_Manager.PortForwardingPresent);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Behaves_Correctly_If_RemoveMapping_Fails()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Provider.Setup(p => p.RemoveMapping(It.IsAny<int>(), It.IsAny<string>())).Callback(() => { throw new InvalidOperationException(); });
            _Manager.TakeServerOffInternet();

            Assert.IsTrue(_Manager.PortForwardingPresent);
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RemovePortForwarding$")]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Removes_Mapping_From_Router()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Configuration.WebServerSettings.EnableUPnp = worksheet.Bool("CnfEnabled");
            _Configuration.WebServerSettings.UPnpPort = worksheet.Int("CnfUPnpPort");
            _Configuration.WebServerSettings.IsOnlyInternetServerOnLan = worksheet.Bool("CnfOnlyVRSOnNetwork");
            _WebServer.Setup(s => s.NetworkIPAddress).Returns(worksheet.String("SvrAddress"));
            _WebServer.Setup(s => s.Port).Returns(worksheet.Int("SvrPort"));

            _Manager.Initialise();
            if(worksheet.String("RtrDescription") != null) {
                AddPortMapping(worksheet.String("RtrDescription"),
                               worksheet.Int("RtrExtPort"),
                               worksheet.String("RtrIntClient"),
                               worksheet.Int("RtrIntPort"),
                               worksheet.String("RtrProtocol"));
                if(_PortMappings[0].Description == _Description && _PortMappings[0].ExternalPort == _Configuration.WebServerSettings.UPnpPort &&
                   _PortMappings[0].InternalClient == _WebServer.Object.NetworkIPAddress && _PortMappings[0].InternalPort == _WebServer.Object.Port &&
                   _PortMappings[0].Protocol == "TCP") _Manager.PutServerOntoInternet();
            }

            _Manager.StateChanged += _StateChangedEvent.Handler;
            _Manager.TakeServerOffInternet();

            _Provider.Verify(p => p.RemoveMapping(worksheet.Int("CnfUPnpPort"), "TCP"),
                worksheet.Bool("ExpectRemove") ? Times.Once() : Times.Never());

            Assert.AreEqual(worksheet.Bool("ExpectIsPresent"), _Manager.PortForwardingPresent);
            Assert.AreEqual(worksheet.Bool("ExpectStateChanged") ? 1 : 0, _StateChangedEvent.CallCount);
            if(_StateChangedEvent.CallCount > 0) {
                Assert.AreSame(_Manager, _StateChangedEvent.Sender);
                Assert.IsNotNull(_StateChangedEvent.Args);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "UPnPManagerTSOTogglesWebServer$")]
        public void UniversalPlugAndPlayManager_TakeServerOffInternet_Disconnects_And_Reconnects_WebServer()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _Configuration.WebServerSettings.IsOnlyInternetServerOnLan = true;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.1");
            _WebServer.Setup(s => s.Port).Returns(80);

            var expectedStarts = 0;
            if(worksheet.Bool("StartOnline")) {
                _WebServer.Object.Online = true;
                ++expectedStarts;
            }

            _WebServer.SetupSet(s => s.Online = true).Callback(() => {
                Assert.IsFalse(_Manager.PortForwardingPresent);
            });

            var expectedStops = 0;
            if(worksheet.Bool("ExpectServerToggle")) {
                ++expectedStarts;
                ++expectedStops;
            }

            _Manager.Initialise();
            if(worksheet.Bool("StartPortForwarded")) {
                AddPortMapping(_Description, 8080, "192.168.0.1", 80, "TCP");
                _Manager.PutServerOntoInternet();
            }

            _Manager.StateChanged += _StateChangedEvent.Handler;
            _Manager.TakeServerOffInternet();

            _WebServer.VerifySet(v => v.Online = true, Times.Exactly(expectedStarts));
            _WebServer.VerifySet(v => v.Online = false, Times.Exactly(expectedStops));
        }
        #endregion

        #region Configuration Changes
        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Of_UPnP_Port_Causes_Disconnect_And_Reconnect()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Provider.Verify(p => p.RemoveMapping(8080, "TCP"), Times.Once());
            _Provider.Verify(p => p.AddMapping(9090, "TCP", 80, "192.168.0.10", true, _Description), Times.Once());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Of_UPnP_Port_Has_No_Effect_If_Not_Already_Connected()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _Manager.Initialise();

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Provider.Verify(p => p.RemoveMapping(8080, "TCP"), Times.Never());
            _Provider.Verify(p => p.AddMapping(9090, "TCP", 80, "192.168.0.10", true, _Description), Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Of_UPnP_Port_Asks_WebServer_To_Disconnect_And_Reconnect()
        {
            // Some routers leave port forwarding open for as long as a connection exists on it, so if the UPnP port changes the
            // server needs to drop all connections before removing the old mapping and restart after the new mapping is established
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _WebServer.Object.Online = true;

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _WebServer.SetupSet(s => s.Online = false).Callback(() => {
                _Provider.Verify(p => p.RemoveMapping(8080, "TCP"), Times.Never());
            });
            _WebServer.SetupSet(s => s.Online = true).Callback(() => {
                _Provider.Verify(p => p.AddMapping(9090, "TCP", 80, "192.168.0.10", true, _Description), Times.Once());
            });

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            _WebServer.VerifySet(s => s.Online = false, Times.Once());
            _WebServer.VerifySet(s => s.Online = true, Times.Exactly(2));  // once by the Online = true above and once in the code we're testing
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Of_UPnP_Port_Does_Not_Ask_WebServer_To_Cycle_If_It_Is_Not_Connected()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Manager.Initialise();

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            _WebServer.VerifySet(s => s.Online = false, Times.Never());
            _WebServer.VerifySet(s => s.Online = true, Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Before_Initialisation_Has_No_Effect()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Provider.Verify(p => p.RemoveMapping(8080, "TCP"), Times.Never());
            _Provider.Verify(p => p.AddMapping(9090, "TCP", 80, "192.168.0.10", true, _Description), Times.Never());
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Before_WebServer_Is_Set_Does_Not_Raise_Exceptions()
        {
            // All we care about here is that there's no null reference exception through - we don't have anything to assert
            _Manager.WebServer = null;

            _Configuration.WebServerSettings.UPnpPort = 9090;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
        }

        [TestMethod]
        public void UniversalPlugAndPlayManager_Configuration_Change_Does_Not_Cycle_Server_If_Nothing_Has_Changed()
        {
            _Configuration.WebServerSettings.EnableUPnp = true;
            _Configuration.WebServerSettings.UPnpPort = 8080;
            _WebServer.Setup(s => s.NetworkIPAddress).Returns("192.168.0.10");
            _WebServer.Setup(s => s.Port).Returns(80);

            _WebServer.Object.Online = true;

            _Manager.Initialise();
            _Manager.PutServerOntoInternet();
            AddPortMapping(_Description, 8080, "192.168.0.10", 80, "TCP");

            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _WebServer.VerifySet(s => s.Online = false, Times.Never());
            _WebServer.VerifySet(s => s.Online = true, Times.Once());  // once by the Online = true above
        }
        #endregion

        #region Trash mappings (those with our description but not pointing to our machine)
        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RemoveTrashPortFowards$")]
        public void UniversalPlugAndPlayManager_Trash_Mappings_Removed_During_Initialise()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("RtrDescription") != null) {
                AddPortMapping(worksheet.String("RtrDescription"),
                               worksheet.Int("RtrExtPort"),
                               worksheet.String("RtrIntClient"),
                               worksheet.Int("RtrIntPort"),
                               worksheet.String("RtrProtocol"));
            }

            _Configuration.WebServerSettings.EnableUPnp = worksheet.Bool("CnfEnabled");
            _Configuration.WebServerSettings.UPnpPort = worksheet.Int("CnfUPnpPort");
            _Configuration.WebServerSettings.IsOnlyInternetServerOnLan = worksheet.Bool("CnfOnlyVRSOnNetwork");
            _WebServer.Setup(s => s.NetworkIPAddress).Returns(worksheet.String("SvrAddress"));
            _WebServer.Setup(s => s.Port).Returns(worksheet.Int("SvrPort"));

            _Manager.Initialise();

            _Provider.Verify(p => p.RemoveMapping(worksheet.Int("RtrExtPort"), worksheet.String("RtrProtocol")),
                worksheet.Bool("ExpectRemove") ? Times.Once() : Times.Never());
        }
        #endregion
    }
}
