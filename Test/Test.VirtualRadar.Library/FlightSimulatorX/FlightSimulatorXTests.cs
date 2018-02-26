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
using VirtualRadar.Interface.FlightSimulatorX;
using VirtualRadar.Library;
using System.Threading;
using System.Globalization;
using VirtualRadar.Localisation;
using Moq;
using Test.Framework;
using InterfaceFactory;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library.FlightSimulatorX
{
    [TestClass]
    public class FlightSimulatorXTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;

        private IFlightSimulatorX _Fsx;
        private Mock<ISimConnectWrapper> _SimConnect;
        private EventRecorder<EventArgs> _ConnectionStatusChangedEvent;
        private EventRecorder<EventArgs> _FreezeStatusChangedEvent;
        private EventRecorder<EventArgs> _SlewStatusChangedEvent;
        private EventRecorder<EventArgs<FlightSimulatorXException>> _FlightSimulatorXExceptionRaisedEvent;
        private EventRecorder<EventArgs<ReadAircraftInformation>> _AircraftInformationReceivedEvent;
        private EventRecorder<EventArgs> _SlewToggledEvent;
        private Dictionary<string, Enum> _ClientEventIdMap;
        private Dictionary<string, Enum> _SystemEventIdMap;
        private Dictionary<Enum, Enum> _NotificationGroupMap;
        private ReadAircraftInformation _ReadAircraftInformation;
        private WriteAircraftInformation _WriteAircraftInformation;
        private Message _Message;
        private Enum _ReadAircraftInformationDefinitionId;
        private Enum _AircraftInformationRequestId;
        private Enum _WriteAircraftInformationDefinitionId;
        private const int _MessageNumber = 0x402;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.CreateChildFactory();

            _SimConnect = TestUtilities.CreateMockImplementation<ISimConnectWrapper>();
            _SimConnect.Setup(m => m.IsInstalled).Returns(true);

            _Fsx = Factory.Resolve<IFlightSimulatorX>();

            _ClientEventIdMap = new Dictionary<string,Enum>();
            _SimConnect.Setup(m => m.MapClientEventToSimEvent(It.IsAny<Enum>(), It.IsAny<string>())).Callback((Enum id, string name) => { _ClientEventIdMap.Add(name, id); });

            _SystemEventIdMap = new Dictionary<string,Enum>();
            _SimConnect.Setup(m => m.SubscribeToSystemEvent(It.IsAny<Enum>(), It.IsAny<string>())).Callback((Enum id, string name) => { _SystemEventIdMap.Add(name, id); });

            _NotificationGroupMap = new Dictionary<Enum,Enum>();
            _SimConnect.Setup(m => m.AddClientEventToNotificationGroup(It.IsAny<Enum>(), It.IsAny<Enum>(), false)).Callback((Enum groupId, Enum eventId, bool maskable) => { _NotificationGroupMap.Add(eventId, groupId); });

            _ReadAircraftInformationDefinitionId = default(Enum);
            _SimConnect.Setup(m => m.RegisterDataDefineStruct<ReadAircraftInformation>(It.IsAny<Enum>())).Callback((Enum definitionId) => { _ReadAircraftInformationDefinitionId = definitionId; });

            _AircraftInformationRequestId = default(Enum);
            _SimConnect.Setup(m => m.RequestDataOnSimObjectType(It.IsAny<Enum>(), It.IsAny<Enum>(), It.IsAny<uint>(), It.IsAny<int>())).Callback((Enum requestId, Enum definitionId, uint radius, int objectType) => {
                if(definitionId == _ReadAircraftInformationDefinitionId) _AircraftInformationRequestId = requestId;
            });

            _WriteAircraftInformationDefinitionId = default(Enum);
            _SimConnect.Setup(m => m.RegisterDataDefineStruct<WriteAircraftInformation>(It.IsAny<Enum>())).Callback((Enum definitionId) => { _WriteAircraftInformationDefinitionId = definitionId; });

            _ConnectionStatusChangedEvent = new EventRecorder<EventArgs>();
            _FreezeStatusChangedEvent = new EventRecorder<EventArgs>();
            _SlewStatusChangedEvent = new EventRecorder<EventArgs>();
            _FlightSimulatorXExceptionRaisedEvent = new EventRecorder<EventArgs<FlightSimulatorXException>>();
            _AircraftInformationReceivedEvent = new EventRecorder<EventArgs<ReadAircraftInformation>>();
            _SlewToggledEvent = new EventRecorder<EventArgs>();

            _ReadAircraftInformation = new ReadAircraftInformation();
            _WriteAircraftInformation = new WriteAircraftInformation();
            _Message = new Message();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);

            if(_Fsx != null) _Fsx.Dispose();
            _Fsx = null;
        }
        #endregion

        #region Helper methods - VerifyFreezeSent, VerifyUnfreezeSent, VerifySlewOnSent, VerifySlewOffSent
        /// <summary>
        /// Verifies that SimConnect was used to send commands to FSX to freeze the aircraft and prevent it from moving.
        /// </summary>
        /// <param name="times"></param>
        private void VerifyFreezeSent(Times times)
        {
            DoVerifyFreeze(true, times);
        }

        /// <summary>
        /// Verifies that SimConnect was sent commands to unfreeze the aircraft and allow it to fly normally.
        /// </summary>
        /// <param name="times"></param>
        private void VerifyUnfreezeSent(Times times)
        {
            DoVerifyFreeze(false, times);
        }

        /// <summary>
        /// Does the work for <see cref="VerifyFreezeSent"/> and <see cref="VerifyUnfreezeSent"/>.
        /// </summary>
        /// <param name="freeze"></param>
        /// <param name="times"></param>
        private void DoVerifyFreeze(bool freeze, Times times)
        {
            var expectedValue = freeze ? 1U : 0U;

            var altitudeEventId = _ClientEventIdMap["FREEZE_ALTITUDE_SET"];
            var attitudeEventId = _ClientEventIdMap["FREEZE_ATTITUDE_SET"];
            var positionEventId = _ClientEventIdMap["FREEZE_LATITUDE_LONGITUDE_SET"];

            var groupId = _NotificationGroupMap[altitudeEventId];

            _SimConnect.Verify(m => m.TransmitClientEvent(0U, altitudeEventId, expectedValue, groupId, 0), times);
            _SimConnect.Verify(m => m.TransmitClientEvent(0U, attitudeEventId, expectedValue, groupId, 0), times);
            _SimConnect.Verify(m => m.TransmitClientEvent(0U, positionEventId, expectedValue, groupId, 0), times);
        }

        /// <summary>
        /// Verifies that SimConnect was asked to put FSX into slew mode.
        /// </summary>
        /// <param name="times"></param>
        private void VerifySlewOnSent(Times times)
        {
            DoVerifySlew(true, times);
        }

        /// <summary>
        /// Verifies that SimConnect was asked to take FSX out of slew mode.
        /// </summary>
        /// <param name="times"></param>
        private void VerifySlewOffSent(Times times)
        {
            DoVerifySlew(false, times);
        }

        /// <summary>
        /// Does the work for <see cref="VerifySlewOnSent"/> and <see cref="VerifySlewOffSent"/>.
        /// </summary>
        /// <param name="on"></param>
        /// <param name="times"></param>
        private void DoVerifySlew(bool on, Times times)
        {
            var slewModeOnEventId = _ClientEventIdMap["SLEW_ON"];
            var slewModeOffEventId = _ClientEventIdMap["SLEW_OFF"];
            var slewAltitudeUpSlowEventId = _ClientEventIdMap["SLEW_ALTIT_UP_SLOW"];
            var slewFreezeEventId = _ClientEventIdMap["SLEW_FREEZE"];

            var groupId = _NotificationGroupMap[slewModeOffEventId];

            if(!on) {
                _SimConnect.Verify(m => m.TransmitClientEvent(0U, slewModeOffEventId, 0u, groupId, 0), times);
            } else {
                _SimConnect.Verify(m => m.TransmitClientEvent(0U, slewModeOnEventId, 0u, groupId, 0), times);
                _SimConnect.Verify(m => m.TransmitClientEvent(0U, slewAltitudeUpSlowEventId, 0u, groupId, 0), times);
                _SimConnect.Verify(m => m.TransmitClientEvent(0U, slewFreezeEventId, 0u, groupId, 0), times);
            }
        }

        /// <summary>
        /// Raises the event on the SimConnect wrapper that causes the <see cref="ReadAircraftInformation"/> object passed across
        /// to be picked up by the FSX object.
        /// </summary>
        /// <param name="readAircraft"></param>
        private void SendAircraftInformation(ReadAircraftInformation readAircraft)
        {
            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                DefineId = Convert.ToUInt32(_ReadAircraftInformationDefinitionId),
                Data = new object[] { readAircraft } });
        }
        #endregion

        #region Constructor and properties
        [TestMethod]
        public void FlightSimulatorX_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Fsx.Dispose();
            _Fsx = Factory.Resolve<IFlightSimulatorX>();

            Assert.AreEqual(false, _Fsx.Connected);
            Assert.AreEqual(Strings.Disconnected, _Fsx.ConnectionStatus);
            Assert.AreEqual(0, _Fsx.MessagesReceivedCount);
            Assert.AreEqual(false, _Fsx.IsFrozen);
        }

        [TestMethod]
        public void FlightSimulatorX_IsInstalled_Returns_False_If_SimConnect_Is_Not_Installed()
        {
            _SimConnect.Setup(m => m.IsInstalled).Returns(false);
            Assert.IsFalse(_Fsx.IsInstalled);
        }

        [TestMethod]
        public void FlightSimulatorX_IsInstalled_Returns_True_If_SimConnect_Is_Installed()
        {
            _SimConnect.Setup(m => m.IsInstalled).Returns(true);
            Assert.IsTrue(_Fsx.IsInstalled);
        }
        #endregion

        #region IsFrozen property
        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Does_Nothing_If_Set_Before_Connected()
        {
            _Fsx.IsFrozen = true;
            Assert.IsFalse(_Fsx.IsFrozen);
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Set_Sends_Correct_Commands_To_FSX()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;

            Assert.IsTrue(_Fsx.IsFrozen);
            VerifyFreezeSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Clear_Sends_Correct_Commands_To_FSX()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;
            _Fsx.IsFrozen = false;

            Assert.IsFalse(_Fsx.IsFrozen);
            VerifyUnfreezeSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Set_Does_Not_Send_Commands_If_Already_Frozen()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;
            _Fsx.IsFrozen = true;

            VerifyFreezeSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Clear_Does_Not_Send_Commands_If_Already_Unfrozen()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;
            _Fsx.IsFrozen = false;
            _Fsx.IsFrozen = false;

            VerifyUnfreezeSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Set_Raises_FreezeStatusChanged()
        {
            _Fsx.FreezeStatusChanged += _FreezeStatusChangedEvent.Handler;
            _FreezeStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.IsTrue(_Fsx.IsFrozen);
            };

            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;

            Assert.AreEqual(1, _FreezeStatusChangedEvent.CallCount);
            Assert.AreSame(_Fsx, _FreezeStatusChangedEvent.Sender);

            _Fsx.FreezeStatusChanged -= _FreezeStatusChangedEvent.Handler;
        }

        [TestMethod]
        public void FlightSimulatorX_IsFrozen_Clear_Raises_FreezeStatusChanged()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;

            _Fsx.FreezeStatusChanged += _FreezeStatusChangedEvent.Handler;
            _FreezeStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.IsFalse(_Fsx.IsFrozen);
            };

            _Fsx.IsFrozen = false;

            Assert.AreEqual(1, _FreezeStatusChangedEvent.CallCount);
            Assert.AreSame(_Fsx, _FreezeStatusChangedEvent.Sender);
        }
        #endregion

        #region IsSlewing property
        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Does_Nothing_If_Set_Before_Connected()
        {
            _Fsx.IsSlewing = true;
            Assert.IsFalse(_Fsx.IsSlewing);
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Set_Sends_Correct_Commands_To_FSX()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;

            Assert.IsTrue(_Fsx.IsSlewing);
            VerifySlewOnSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Clear_Sends_Correct_Commands_To_FSX()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;
            _Fsx.IsSlewing = false;

            Assert.IsFalse(_Fsx.IsSlewing);
            VerifySlewOffSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Set_Does_Not_Send_Commands_If_Already_Slewing()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;
            _Fsx.IsSlewing = true;

            VerifySlewOnSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Clear_Does_Not_Send_Commands_If_Not_Slewing()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;
            _Fsx.IsSlewing = false;
            _Fsx.IsSlewing = false;

            VerifySlewOffSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Set_Raises_SlewStatusChanged()
        {
            _Fsx.SlewStatusChanged += _SlewStatusChangedEvent.Handler;
            _SlewStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.IsTrue(_Fsx.IsSlewing);
            };

            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;

            Assert.AreEqual(1, _SlewStatusChangedEvent.CallCount);
            Assert.AreSame(_Fsx, _SlewStatusChangedEvent.Sender);
        }

        [TestMethod]
        public void FlightSimulatorX_IsSlewing_Clear_Raises_SlewStatusChanged()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsSlewing = true;

            _Fsx.SlewStatusChanged += _SlewStatusChangedEvent.Handler;
            _SlewStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.IsFalse(_Fsx.IsSlewing);
            };

            _Fsx.IsSlewing = false;

            Assert.AreEqual(1, _SlewStatusChangedEvent.CallCount);
            Assert.AreSame(_Fsx, _SlewStatusChangedEvent.Sender);
        }
        #endregion

        #region MessageCount property
        [TestMethod]
        public void FlightSimulatorX_MessagesReceivedCount_Increments_When_Aircraft_Information_Received()
        {
            _Fsx.Connect(new IntPtr(10));
            SendAircraftInformation(_ReadAircraftInformation);

            Assert.AreEqual(1, _Fsx.MessagesReceivedCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void FlightSimulatorX_Dispose_Releases_Provider()
        {
            _Fsx.Dispose();
            _SimConnect.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_Dispose_Unfreezes_Aircraft()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;
            _Fsx.Dispose();

            Assert.IsFalse(_Fsx.IsFrozen);
            VerifyUnfreezeSent(Times.Once());
        }
        #endregion

        #region Connect
        [TestMethod]
        public void FlightSimulatorX_Connect_Creates_SimConnect()
        {
            IntPtr windowHandle = new IntPtr(10289);
            _Fsx.Connect(windowHandle);
            _SimConnect.Verify(m => m.CreateSimConnect("Virtual Radar Server", windowHandle, _MessageNumber, null, 0), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Does_Nothing_If_FSX_Not_Installed()
        {
            _SimConnect.Setup(m => m.IsInstalled).Returns(false);
            _Fsx.Connect(new IntPtr(10));
            _SimConnect.Verify(m => m.CreateSimConnect(It.IsAny<string>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<WaitHandle>(), It.IsAny<uint>()), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Hooks_Correct_Client_Events()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Verify(m => m.MapClientEventToSimEvent(It.IsAny<Enum>(), It.IsAny<string>()), Times.Exactly(8));

            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewModeOn, "SLEW_ON"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewModeOff, "SLEW_OFF"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewAltitudeUpSlow, "SLEW_ALTIT_UP_SLOW"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewFreeze, "SLEW_FREEZE"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewToggle, "SLEW_TOGGLE"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeAltitude, "FREEZE_ALTITUDE_SET"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeAttitude, "FREEZE_ATTITUDE_SET"), Times.Once());
            _SimConnect.Verify(m => m.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeLatitudeLongitude, "FREEZE_LATITUDE_LONGITUDE_SET"), Times.Once());

            Assert.AreEqual(8, _ClientEventIdMap.Values.Distinct().Count());
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Adds_Client_Events_To_Notification_Group()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Verify(m => m.AddClientEventToNotificationGroup(It.IsAny<Enum>(), It.IsAny<Enum>(), false), Times.Exactly(8));

            var slewGroupId = _NotificationGroupMap[_ClientEventIdMap["SLEW_ON"]];
            Assert.AreEqual(slewGroupId, _NotificationGroupMap[_ClientEventIdMap["SLEW_OFF"]]);
            Assert.AreEqual(slewGroupId, _NotificationGroupMap[_ClientEventIdMap["SLEW_ALTIT_UP_SLOW"]]);
            Assert.AreEqual(slewGroupId, _NotificationGroupMap[_ClientEventIdMap["SLEW_FREEZE"]]);
            Assert.AreEqual(slewGroupId, _NotificationGroupMap[_ClientEventIdMap["SLEW_TOGGLE"]]);

            var freezeGroupId = _NotificationGroupMap[_ClientEventIdMap["FREEZE_ALTITUDE_SET"]];
            Assert.AreEqual(freezeGroupId, _NotificationGroupMap[_ClientEventIdMap["FREEZE_ATTITUDE_SET"]]);
            Assert.AreEqual(freezeGroupId, _NotificationGroupMap[_ClientEventIdMap["FREEZE_LATITUDE_LONGITUDE_SET"]]);
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Subscribes_To_Correct_System_Events()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Verify(m => m.SubscribeToSystemEvent(It.IsAny<Enum>(), It.IsAny<string>()), Times.Exactly(2));

            _SimConnect.Verify(m => m.SubscribeToSystemEvent(FlightSimulatorXEventId.Crashed, "Crashed"), Times.Once());
            _SimConnect.Verify(m => m.SubscribeToSystemEvent(FlightSimulatorXEventId.MissionCompleted, "MissionCompleted"), Times.Once());

            Assert.AreEqual(2, _SystemEventIdMap.Values.Distinct().Count());
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Registers_Correct_Request_Fields()
        {
            const int expectedFieldDeclarations = 23;
            List<Enum> structIds = new List<Enum>();
            List<string> fieldNames = new List<string>();

            _SimConnect.Setup(m => m.AddToDataDefinition(It.IsAny<Enum>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<uint>())).Callback((Enum structId, string fieldName, string fieldUnit, int fieldType, float epsilon, uint dataId) => {
                structIds.Add(structId);
                fieldNames.Add(fieldName);
            });

            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<uint>()), Times.Exactly(expectedFieldDeclarations));
            _SimConnect.Verify(m => m.RegisterDataDefineStruct<ReadAircraftInformation>(It.IsAny<Enum>()), Times.Once());
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE LATITUDE", "degrees", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE LONGITUDE", "degrees", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE ALTITUDE", "feet", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "AIRSPEED INDICATED", "knots", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "AIRSPEED BARBER POLE", "knots", 4, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE HEADING DEGREES TRUE", "degrees", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "VERTICAL SPEED", "feet/minute", 4, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "ATC TYPE", null, 6, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "ATC MODEL", null, 6, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "ATC ID", null, 6, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "ATC AIRLINE", null, 7, 0f, It.IsAny<uint>()), Times.Exactly(2));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "TRANSPONDER CODE:1", "number", 1, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "SIM ON GROUND", "bool", 1, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE PITCH DEGREES", "degrees", 4, 0f, It.IsAny<uint>()), Times.Exactly(1));
            _SimConnect.Verify(m => m.AddToDataDefinition(It.IsAny<Enum>(), "PLANE BANK DEGREES", "degrees", 4, 0f, It.IsAny<uint>()), Times.Exactly(1));

            Assert.AreNotEqual(_ReadAircraftInformationDefinitionId, _WriteAircraftInformationDefinitionId);

            CheckFieldOrder(structIds, fieldNames, _ReadAircraftInformationDefinitionId,
                "PLANE LATITUDE",
                "PLANE LONGITUDE",
                "PLANE ALTITUDE",
                "AIRSPEED INDICATED",
                "AIRSPEED BARBER POLE",
                "PLANE HEADING DEGREES TRUE",
                "VERTICAL SPEED",
                "ATC TYPE",
                "ATC MODEL",
                "ATC ID",
                "ATC AIRLINE",
                "TRANSPONDER CODE:1",
                "SIM ON GROUND");

            CheckFieldOrder(structIds, fieldNames, _WriteAircraftInformationDefinitionId,
                "PLANE LATITUDE",
                "PLANE LONGITUDE",
                "PLANE ALTITUDE",
                "AIRSPEED INDICATED",
                "PLANE HEADING DEGREES TRUE",
                "VERTICAL SPEED",
                "ATC ID",
                "ATC AIRLINE",
                "PLANE PITCH DEGREES",
                "PLANE BANK DEGREES");
        }

        private void CheckFieldOrder(List<Enum> structIds, List<string> fieldNames, Enum structId, params string[] expectedFieldNames)
        {
            int previousIndex = -1;
            foreach(var expectedFieldName in expectedFieldNames) {
                int index = structIds.IndexOf(structId, previousIndex + 1);
                previousIndex = index;
                Assert.AreEqual(expectedFieldName, fieldNames[index]);
            }
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Sets_System_Event_States()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Verify(m => m.SetSystemEventState(It.IsAny<Enum>(), true), Times.Exactly(2));
            _SimConnect.Verify(m => m.SetSystemEventState(_SystemEventIdMap["Crashed"], true), Times.Once());
            _SimConnect.Verify(m => m.SetSystemEventState(_SystemEventIdMap["MissionCompleted"], true), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Sets_Connected_Property()
        {
            _Fsx.Connect(new IntPtr(10));
            Assert.IsTrue(_Fsx.Connected);
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Sets_ConnectionStatus_Property()
        {
            _Fsx.Connect(new IntPtr(10));
            Assert.AreEqual(Strings.Connected, _Fsx.ConnectionStatus);
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Raises_ConnectionStatusChanged()
        {
            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _ConnectionStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(true, _Fsx.Connected);
                Assert.AreEqual(Strings.Connected, _Fsx.ConnectionStatus);
            };

            _Fsx.Connect(new IntPtr(10));

            Assert.AreEqual(1, _ConnectionStatusChangedEvent.CallCount);
            Assert.AreSame(_Fsx, _ConnectionStatusChangedEvent.Sender);
            Assert.IsNotNull(_ConnectionStatusChangedEvent.Args);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FlightSimulatorX_Connect_Throws_If_Called_Twice()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.Connect(new IntPtr(10));
        }

        [TestMethod]
        public void FlightSimulatorX_Connect_Copes_If_SimConnect_Throws_A_COM_Exception()
        {
            _SimConnect.Setup(m => m.CreateSimConnect(It.IsAny<string>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<WaitHandle>(), It.IsAny<uint>())).Callback(() => { throw new COMException(); });

            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _ConnectionStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(false, _Fsx.Connected);
                Assert.IsTrue(_Fsx.ConnectionStatus.Contains(Strings.CouldNotConnect));
            };

            _Fsx.Connect(new IntPtr(10));

            Assert.AreEqual(false, _Fsx.Connected);
            Assert.IsTrue(_Fsx.ConnectionStatus.StartsWith(Strings.Disconnected));
            Assert.IsTrue(_Fsx.ConnectionStatus.Contains(Strings.CouldNotConnect));

            Assert.AreEqual(1, _ConnectionStatusChangedEvent.CallCount);
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void FlightSimulatorX_Disconnect_Disposes_Of_SimConnect_Object()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.Disconnect();

            _SimConnect.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Clears_Connected_Flag()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.Disconnect();
            Assert.IsFalse(_Fsx.Connected);
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Unfreezes_Aircraft()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.IsFrozen = true;
            _Fsx.Disconnect();

            Assert.IsFalse(_Fsx.IsFrozen);
            VerifyUnfreezeSent(Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Raises_ConnectionStatusChanged()
        {
            _Fsx.Connect(new IntPtr(10));

            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _ConnectionStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.IsFalse(_Fsx.Connected);
                Assert.AreEqual(Strings.Disconnected, _Fsx.ConnectionStatus);
            };
            _Fsx.Disconnect();

            Assert.AreEqual(1, _ConnectionStatusChangedEvent.CallCount);
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Does_Nothing_If_Not_Connected()
        {
            _Fsx.Disconnect();
            _SimConnect.Verify(m => m.Dispose(), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Creates_New_SimConnect_Object()
        {
            var secondSimConnect = TestUtilities.CreateMockImplementation<ISimConnectWrapper>();
            secondSimConnect.Setup(m => m.IsInstalled).Returns(false);

            _Fsx.Connect(new IntPtr(10));
            Assert.AreEqual(true, _Fsx.IsInstalled);

            _Fsx.Disconnect();
            Assert.AreEqual(false, _Fsx.IsInstalled); // <-- false because it's now using the new SimConnect
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Unhooks_All_Events_On_Original_SimConnect()
        {
            var secondSimConnect = TestUtilities.CreateMockImplementation<ISimConnectWrapper>();
            secondSimConnect.Setup(m => m.IsInstalled).Returns(true);

            _Fsx.Connect(new IntPtr(10));
            _Fsx.Disconnect();

            _Fsx.AircraftInformationReceived += _AircraftInformationReceivedEvent.Handler;
            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _Fsx.FlightSimulatorXExceptionRaised += _FlightSimulatorXExceptionRaisedEvent.Handler;
            _Fsx.SlewToggled += _SlewToggledEvent.Handler;

            _SimConnect.Raise(m => m.ExceptionRaised += null, new SimConnectExceptionRaisedEventArgs());
            Assert.AreEqual(0, _FlightSimulatorXExceptionRaisedEvent.CallCount);

            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() { DefineId = Convert.ToUInt32(_AircraftInformationRequestId), Data = new object[] { _ReadAircraftInformation } } );
            Assert.AreEqual(0, _AircraftInformationReceivedEvent.CallCount);

            _SimConnect.Raise(m => m.UserHasQuit += null, EventArgs.Empty);
            Assert.AreEqual(0, _ConnectionStatusChangedEvent.CallCount);

            _SimConnect.Raise(m => m.EventObserved += null, new SimConnectEventObservedEventArgs() { EventId = (uint)FlightSimulatorXEventId.SlewToggle });
            Assert.AreEqual(0, _SlewToggledEvent.CallCount);
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Hooks_All_Events_To_New_SimConnect()
        {
            var secondSimConnect = TestUtilities.CreateMockImplementation<ISimConnectWrapper>();
            secondSimConnect.Setup(m => m.IsInstalled).Returns(true);

            _Fsx.Connect(new IntPtr(10));
            _Fsx.Disconnect();

            _ClientEventIdMap.Clear();
            _NotificationGroupMap.Clear();
            _SystemEventIdMap.Clear();

            _Fsx.Connect(new IntPtr(10));

            _Fsx.AircraftInformationReceived += _AircraftInformationReceivedEvent.Handler;
            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _Fsx.FlightSimulatorXExceptionRaised += _FlightSimulatorXExceptionRaisedEvent.Handler;
            _Fsx.SlewToggled += _SlewToggledEvent.Handler;

            secondSimConnect.Raise(m => m.ExceptionRaised += null, new SimConnectExceptionRaisedEventArgs());
            Assert.AreEqual(1, _FlightSimulatorXExceptionRaisedEvent.CallCount);

            secondSimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() { DefineId = Convert.ToUInt32(_AircraftInformationRequestId), Data = new object[] { _ReadAircraftInformation } } );
            Assert.AreEqual(1, _AircraftInformationReceivedEvent.CallCount);

            secondSimConnect.Raise(m => m.UserHasQuit += null, EventArgs.Empty);
            Assert.AreEqual(1, _ConnectionStatusChangedEvent.CallCount);

            secondSimConnect.Raise(m => m.EventObserved += null, new SimConnectEventObservedEventArgs() { EventId = (uint)FlightSimulatorXEventId.SlewToggle });
            Assert.AreEqual(1, _SlewToggledEvent.CallCount);
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Automatically_Called_When_User_Quits_FSX()
        {
            _Fsx.Connect(new IntPtr(10));

            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _ConnectionStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(false, _Fsx.Connected);
                Assert.AreEqual(Strings.FlightSimulatorXShutdown, _Fsx.ConnectionStatus);
            };

            _SimConnect.Raise(m => m.UserHasQuit += null, EventArgs.Empty);

            Assert.AreEqual(1, _ConnectionStatusChangedEvent.CallCount);
        }

        [TestMethod]
        public void FlightSimulatorX_Disconnect_Not_Automatically_Called_When_User_Quits_FSX_When_Not_Connected()
        {
            _Fsx.ConnectionStatusChanged += _ConnectionStatusChangedEvent.Handler;
            _ConnectionStatusChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(false, _Fsx.Connected);
                Assert.AreEqual(Strings.FlightSimulatorXShutdown, _Fsx.ConnectionStatus);
            };

            _SimConnect.Raise(m => m.UserHasQuit += null, EventArgs.Empty);

            Assert.AreEqual(0, _ConnectionStatusChangedEvent.CallCount);
        }
        #endregion

        #region IsSimConnectMessage
        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Returns_False_When_Passed_An_Invalid_Message()
        {
            _Fsx.Connect(new IntPtr(10));

            _Message.Msg = _MessageNumber + 1;
            Assert.IsFalse(_Fsx.IsSimConnectMessage(_Message));
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Returns_True_When_Passed_A_Valid_Message()
        {
            _Fsx.Connect(new IntPtr(10));

            _Message.Msg = _MessageNumber;
            Assert.IsTrue(_Fsx.IsSimConnectMessage(_Message));
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Passes_Valid_Messages_To_SimConnect()
        {
            _Fsx.Connect(new IntPtr(10));

            _Message.Msg = _MessageNumber;
            _Fsx.IsSimConnectMessage(_Message);

            _SimConnect.Verify(m => m.ReceiveMessage(), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Does_Not_Pass_Invalid_Messages_To_SimConnect()
        {
            _Fsx.Connect(new IntPtr(10));

            _Message.Msg = _MessageNumber - 1;
            _Fsx.IsSimConnectMessage(_Message);

            _SimConnect.Verify(m => m.ReceiveMessage(), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Does_Not_Pass_Messages_To_SimConnect_When_FSX_Is_Not_Installed()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Setup(m => m.IsInstalled).Returns(false);

            _Message.Msg = _MessageNumber;
            _Fsx.IsSimConnectMessage(_Message);

            _SimConnect.Verify(m => m.ReceiveMessage(), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Returns_False_When_FSX_Is_Not_Installed()
        {
            _Fsx.Connect(new IntPtr(10));

            _SimConnect.Setup(m => m.IsInstalled).Returns(false);

            _Message.Msg = _MessageNumber;
            Assert.IsFalse(_Fsx.IsSimConnectMessage(_Message));
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Does_Not_Pass_Messages_To_SimConnect_When_FSX_Is_Not_Connected()
        {
            _Message.Msg = _MessageNumber;
            _Fsx.IsSimConnectMessage(_Message);

            _SimConnect.Verify(m => m.ReceiveMessage(), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_IsSimConnectMessage_Returns_False_When_FSX_Is_Not_Connected()
        {
            _Message.Msg = _MessageNumber;
            Assert.IsFalse(_Fsx.IsSimConnectMessage(_Message));
        }
        #endregion

        #region RequestAircraftInformation
        [TestMethod]
        public void FlightSimulatorX_RequestAircraftInformation_Passes_Request_To_SimConnect()
        {
            _Fsx.RequestAircraftInformation();

            _SimConnect.Verify(m => m.RequestDataOnSimObjectType(It.IsAny<Enum>(), It.IsAny<Enum>(), 0, 0), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_RequestAircraftInformation_Does_Nothing_If_SimConnect_Not_Installed()
        {
            _SimConnect.Setup(m => m.IsInstalled).Returns(false);

            _Fsx.RequestAircraftInformation();

            _SimConnect.Verify(m => m.RequestDataOnSimObjectType(It.IsAny<Enum>(), It.IsAny<Enum>(), It.IsAny<uint>(), It.IsAny<int>()), Times.Never());
        }
        #endregion

        #region MoveAircraft
        [TestMethod]
        public void FlightSimulatorX_MoveAircraft_Passes_Aircraft_Information_To_SimConnect()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.MoveAircraft(_WriteAircraftInformation);

            _SimConnect.Verify(m => m.SetDataOnSimObject(_WriteAircraftInformationDefinitionId, 0U, 0, _WriteAircraftInformation), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorX_MoveAircraft_Does_Nothing_If_Not_Installed()
        {
            _Fsx.Connect(new IntPtr(10));
            _SimConnect.Setup(m => m.IsInstalled).Returns(false);

            _Fsx.MoveAircraft(_WriteAircraftInformation);
            _SimConnect.Verify(m => m.SetDataOnSimObject(It.IsAny<Enum>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorX_MoveAircraft_Does_Nothing_If_Not_Connected()
        {
            _Fsx.MoveAircraft(_WriteAircraftInformation);
            _SimConnect.Verify(m => m.SetDataOnSimObject(It.IsAny<Enum>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<object>()), Times.Never());
        }
        #endregion

        #region AircraftInformationReceived event
        [TestMethod]
        public void FlightSimulatorX_AircraftInformationReceived_Raised_When_ObjectReceived_Event_Raised_By_SimConnect()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.RequestAircraftInformation();

            _Fsx.AircraftInformationReceived += _AircraftInformationReceivedEvent.Handler;
            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                Data = new object[] { _ReadAircraftInformation },
                RequestId = Convert.ToUInt32(_AircraftInformationRequestId),
            });

            Assert.AreEqual(1, _AircraftInformationReceivedEvent.CallCount);
            Assert.AreSame(_Fsx, _AircraftInformationReceivedEvent.Sender);
            Assert.AreEqual(_ReadAircraftInformation, _AircraftInformationReceivedEvent.Args.Value);
        }

        [TestMethod]
        public void FlightSimulatorX_AircraftInformationReceived_Not_Raised_When_RequestId_Is_Incorrect()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.RequestAircraftInformation();

            _Fsx.AircraftInformationReceived += _AircraftInformationReceivedEvent.Handler;
            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                Data = new object[] { _ReadAircraftInformation },
                RequestId = Convert.ToUInt32(_AircraftInformationRequestId) + 100U,
            });

            Assert.AreEqual(0, _AircraftInformationReceivedEvent.CallCount);
        }

        [TestMethod]
        public void FlightSimulatorX_AircraftInformationReceived_Not_Raised_When_Invalid_Data_Is_Sent()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.RequestAircraftInformation();

            _Fsx.AircraftInformationReceived += _AircraftInformationReceivedEvent.Handler;
            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                Data = new object[] { },
                RequestId = Convert.ToUInt32(_AircraftInformationRequestId),
            });

            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                Data = new object[] { null },
                RequestId = Convert.ToUInt32(_AircraftInformationRequestId),
            });

            _SimConnect.Raise(m => m.ObjectReceived += null, new SimConnectObjectReceivedEventArgs() {
                Data = new object[] { "Ooops, this isn't an AircraftInformation!" },
                RequestId = Convert.ToUInt32(_AircraftInformationRequestId),
            });

            Assert.AreEqual(0, _AircraftInformationReceivedEvent.CallCount);
        }
        #endregion

        #region FlightSimulatorXExceptionRaised event
        [TestMethod]
        public void FlightSimulatorX_FlightSimulatorXExceptionRaised_Raised_When_SimConnect_Raises_Event()
        {
            _Fsx.Connect(new IntPtr(10));
            _Fsx.FlightSimulatorXExceptionRaised += _FlightSimulatorXExceptionRaisedEvent.Handler;

            _SimConnect.Raise(m => m.ExceptionRaised += null, new SimConnectExceptionRaisedEventArgs() {
                ExceptionCode = 32,
                ParameterIndex = 2,
                SendId = 3
            });

            Assert.AreEqual(1, _FlightSimulatorXExceptionRaisedEvent.CallCount);
            Assert.AreSame(_Fsx, _FlightSimulatorXExceptionRaisedEvent.Sender);

            var args = _FlightSimulatorXExceptionRaisedEvent.Args.Value;
            Assert.AreEqual(FlightSimulatorXExceptionCode.AlreadyCreated, args.ExceptionCode);
            Assert.AreEqual(32U, args.RawExceptionCode);
            Assert.AreEqual(2U, args.IndexNumber);
            Assert.AreEqual(3U, args.SendID);
        }
        #endregion

        #region SlewToggled event
        [TestMethod]
        public void FlightSimulatorX_SlewToggled_Raised_When_User_Toggles_Slew_Manually()
        {
            _Fsx.SlewToggled += _SlewToggledEvent.Handler;
            _Fsx.Connect(new IntPtr(10));

            var args = new SimConnectEventObservedEventArgs() { EventId = (uint)FlightSimulatorXEventId.SlewToggle, };
            _SimConnect.Raise(s => s.EventObserved += null, args);

            Assert.AreEqual(1, _SlewToggledEvent.CallCount);
            Assert.AreSame(_Fsx, _SlewToggledEvent.Sender);
            Assert.IsNotNull(_SlewToggledEvent.Args);
        }
        #endregion
    }
}
