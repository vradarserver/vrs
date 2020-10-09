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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using Microsoft.FlightSimulator.SimConnect;
using VirtualRadar.Interface;
using VirtualRadar.Interface.FlightSimulator;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.FlightSimulatorX
{
    /// <summary>
    /// The default implementation of <see cref="IFlightSimulatorX"/>.
    /// </summary>
    sealed class FlightSimulatorX : IFlightSimulatorX
    {
        #region SimConnect enums and fields
        /// <summary>
        /// The message number that we use to identify SimConnect messages.
        /// </summary>
        const uint SimConnect_UserMessageId = 0x402;

        /// <summary>
        /// Identifiers that SimConnect will be told to use when talking to us.
        /// </summary>
        enum DefinitionId
        {
            ReadAircraftInformation,
            WriteAircraftInformation,
        }

        /// <summary>
        /// Identifiers that SimConnect will be told to use when talking to us.
        /// </summary>
        enum RequestId
        {
            ReadAircraftInformation
        }

        /// <summary>
        /// Identifiers that SimConnect will be told to use when talking to us.
        /// </summary>
        enum GroupId
        {
            Slewing,
            Freezing,
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that will wrap calls to SimConnect for us so that we can be tested without FSX having to be installed.
        /// </summary>
        private ISimConnectWrapper _SimConnect;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsInstalled { get { return _SimConnect.IsInstalled; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ConnectionStatus { get; private set; }

        private bool _IsFrozen;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsFrozen
        {
            get { return _IsFrozen; }
            set
            {
                if(Connected && IsFrozen != value) {
                    var dataValue = value ? 1u : 0u;
                    _SimConnect.TransmitClientEvent((uint)SIMCONNECT_SIMOBJECT_TYPE.USER, FlightSimulatorXEventId.FreezeAltitude, dataValue, GroupId.Freezing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                    _SimConnect.TransmitClientEvent((uint)SIMCONNECT_SIMOBJECT_TYPE.USER, FlightSimulatorXEventId.FreezeAttitude, dataValue, GroupId.Freezing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                    _SimConnect.TransmitClientEvent((uint)SIMCONNECT_SIMOBJECT_TYPE.USER, FlightSimulatorXEventId.FreezeLatitudeLongitude, dataValue, GroupId.Freezing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);

                    _IsFrozen = value;
                    OnFreezeStatusChanged(EventArgs.Empty);
                }
            }
        }

        private bool _IsSlewing;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSlewing
        {
            get { return _IsSlewing; }
            set
            {
                if(Connected && IsSlewing != value) {
                    if(!value) {
                        _SimConnect.TransmitClientEvent(0, FlightSimulatorXEventId.SlewModeOff, 0, GroupId.Slewing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                    } else {
                        _SimConnect.TransmitClientEvent(0, FlightSimulatorXEventId.SlewModeOn, 0, GroupId.Slewing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                        _SimConnect.TransmitClientEvent(0, FlightSimulatorXEventId.SlewAltitudeUpSlow, 0, GroupId.Slewing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                        Thread.Sleep(250);
                        _SimConnect.TransmitClientEvent(0, FlightSimulatorXEventId.SlewFreeze, 0, GroupId.Slewing, (int)SIMCONNECT_EVENT_FLAG.DEFAULT);
                    }

                    _IsSlewing = value;
                    OnSlewStatusChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MessagesReceivedCount { get; private set; }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<ReadAircraftInformation>> AircraftInformationReceived;

        /// <summary>
        /// Raises <see cref="AircraftInformationReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnAircraftInformationReceived(EventArgs<ReadAircraftInformation> args)
        {
            EventHelper.RaiseQuickly(AircraftInformationReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectionStatusChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnConnectionStatusChanged(EventArgs args)
        {
            EventHelper.Raise(ConnectionStatusChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<FlightSimulatorXException>> FlightSimulatorXExceptionRaised;

        /// <summary>
        /// Raises <see cref="FlightSimulatorXExceptionRaised"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFlightSimulatorXExceptionRaised(EventArgs<FlightSimulatorXException> args)
        {
            EventHelper.Raise(FlightSimulatorXExceptionRaised, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FreezeStatusChanged;

        /// <summary>
        /// Raises <see cref="FreezeStatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFreezeStatusChanged(EventArgs args)
        {
            EventHelper.Raise(FreezeStatusChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler SlewStatusChanged;

        /// <summary>
        /// Raises <see cref="SlewStatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnSlewStatusChanged(EventArgs args)
        {
            EventHelper.Raise(SlewStatusChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler SlewToggled;

        /// <summary>
        /// Raises <see cref="SlewToggled"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnSlewToggled(EventArgs args)
        {
            EventHelper.Raise(SlewToggled, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightSimulatorX()
        {
            ConnectionStatus = Strings.Disconnected;
            _SimConnect = Factory.Resolve<ISimConnectWrapper>();
            _SimConnect.ExceptionRaised += SimConnect_ExceptionRaised;
            _SimConnect.UserHasQuit += SimConnect_UserHasQuit;
            _SimConnect.ObjectReceived += SimConnect_ObjectReceived;
            _SimConnect.EventObserved += SimConnect_EventObserved;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FlightSimulatorX()
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
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                IsFrozen = false;
                _SimConnect.Dispose();
            }
        }
        #endregion

        #region Connect, Disconnect
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="windowHandle"></param>
        public void Connect(IntPtr windowHandle)
        {
            if(_SimConnect.IsInstalled) {
                if(Connected) throw new InvalidOperationException("Already connected to FSX");

                try {
                    _SimConnect.CreateSimConnect("Virtual Radar Server", windowHandle, SimConnect_UserMessageId, null, 0);

                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewModeOn, "SLEW_ON");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewModeOff, "SLEW_OFF");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewAltitudeUpSlow, "SLEW_ALTIT_UP_SLOW");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewFreeze, "SLEW_FREEZE");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.SlewToggle, "SLEW_TOGGLE");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeAltitude, "FREEZE_ALTITUDE_SET");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeAttitude, "FREEZE_ATTITUDE_SET");
                    _SimConnect.MapClientEventToSimEvent(FlightSimulatorXEventId.FreezeLatitudeLongitude, "FREEZE_LATITUDE_LONGITUDE_SET");

                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Slewing, FlightSimulatorXEventId.SlewModeOn, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Slewing, FlightSimulatorXEventId.SlewModeOff, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Slewing, FlightSimulatorXEventId.SlewAltitudeUpSlow, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Slewing, FlightSimulatorXEventId.SlewFreeze, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Slewing, FlightSimulatorXEventId.SlewToggle, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Freezing, FlightSimulatorXEventId.FreezeAltitude, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Freezing, FlightSimulatorXEventId.FreezeAttitude, false);
                    _SimConnect.AddClientEventToNotificationGroup(GroupId.Freezing, FlightSimulatorXEventId.FreezeLatitudeLongitude, false);

                    _SimConnect.SubscribeToSystemEvent(FlightSimulatorXEventId.Crashed, "Crashed");
                    _SimConnect.SubscribeToSystemEvent(FlightSimulatorXEventId.MissionCompleted, "MissionCompleted");

                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "PLANE LATITUDE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "PLANE LONGITUDE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "PLANE ALTITUDE", "feet", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "AIRSPEED INDICATED", "knots", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "AIRSPEED BARBER POLE", "knots", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "PLANE HEADING DEGREES TRUE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "VERTICAL SPEED", "feet/minute", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "ATC TYPE", null, (int)SIMCONNECT_DATATYPE.STRING32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "ATC MODEL", null, (int)SIMCONNECT_DATATYPE.STRING32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "ATC ID", null, (int)SIMCONNECT_DATATYPE.STRING32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "ATC AIRLINE", null, (int)SIMCONNECT_DATATYPE.STRING64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "TRANSPONDER CODE:1", "number", (int)SIMCONNECT_DATATYPE.INT32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.ReadAircraftInformation, "SIM ON GROUND", "bool", (int)SIMCONNECT_DATATYPE.INT32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.RegisterDataDefineStruct<ReadAircraftInformation>(DefinitionId.ReadAircraftInformation);

                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE LATITUDE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE LONGITUDE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE ALTITUDE", "feet", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "AIRSPEED INDICATED", "knots", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE HEADING DEGREES TRUE", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "VERTICAL SPEED", "feet/minute", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "ATC ID", null, (int)SIMCONNECT_DATATYPE.STRING32, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "ATC AIRLINE", null, (int)SIMCONNECT_DATATYPE.STRING64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE PITCH DEGREES", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.AddToDataDefinition(DefinitionId.WriteAircraftInformation, "PLANE BANK DEGREES", "degrees", (int)SIMCONNECT_DATATYPE.FLOAT64, 0.0f, _SimConnect.UnusedValue);
                    _SimConnect.RegisterDataDefineStruct<WriteAircraftInformation>(DefinitionId.WriteAircraftInformation);

                    _SimConnect.SetSystemEventState(FlightSimulatorXEventId.Crashed, true);
                    _SimConnect.SetSystemEventState(FlightSimulatorXEventId.MissionCompleted, true);

                    Connected = true;
                    ConnectionStatus = Strings.Connected;
                } catch(COMException ex) {
                    Debug.WriteLine(String.Format("FlightSimulatorX.Connect caught exception: {0}", ex.ToString()));
                    Connected = false;
                    ConnectionStatus = String.Format("{0}... {1} - {2}", Strings.Disconnected, Strings.CouldNotConnect, ex.Message);
                }

                OnConnectionStatusChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            DoDisconnect(Strings.Disconnected);
        }

        /// <summary>
        /// Does the work for <see cref="Disconnect"/>.
        /// </summary>
        /// <param name="connectionStatus"></param>
        private void DoDisconnect(string connectionStatus)
        {
            if(Connected) {
                IsFrozen = false;

                _SimConnect.EventObserved -= SimConnect_EventObserved;
                _SimConnect.ExceptionRaised -= SimConnect_ExceptionRaised;
                _SimConnect.ObjectReceived -= SimConnect_ObjectReceived;
                _SimConnect.UserHasQuit -= SimConnect_UserHasQuit;

                _SimConnect.Dispose();

                _SimConnect = Factory.Resolve<ISimConnectWrapper>();
                _SimConnect.EventObserved += SimConnect_EventObserved;
                _SimConnect.ExceptionRaised += SimConnect_ExceptionRaised;
                _SimConnect.ObjectReceived += SimConnect_ObjectReceived;
                _SimConnect.UserHasQuit += SimConnect_UserHasQuit;

                Connected = false;
                ConnectionStatus = connectionStatus;

                OnConnectionStatusChanged(EventArgs.Empty);
            }
        }
        #endregion

        #region IsSimConnectMessage
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsSimConnectMessage(Message message)
        {
            var result = message.Msg == SimConnect_UserMessageId && _SimConnect.IsInstalled && Connected;
            if(result) _SimConnect.ReceiveMessage();

            return result;
        }
        #endregion

        #region RequestAircraftInformation
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RequestAircraftInformation()
        {
            if(_SimConnect.IsInstalled) {
                _SimConnect.RequestDataOnSimObjectType(RequestId.ReadAircraftInformation, DefinitionId.ReadAircraftInformation, 0, (int)SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
        }
        #endregion

        #region MoveAircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftInformation"></param>
        public void MoveAircraft(WriteAircraftInformation aircraftInformation)
        {
            if(Connected && _SimConnect.IsInstalled) {
                _SimConnect.SetDataOnSimObject(DefinitionId.WriteAircraftInformation, (uint)SIMCONNECT_SIMOBJECT_TYPE.USER, (int)SIMCONNECT_DATA_SET_FLAG.DEFAULT, aircraftInformation);
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when FSX notifies us of an event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SimConnect_EventObserved(object sender, SimConnectEventObservedEventArgs args)
        {
            if(args.EventId == (uint)FlightSimulatorXEventId.SlewToggle) OnSlewToggled(EventArgs.Empty);
        }

        /// <summary>
        /// Called when SimConnect receives an exception notification from FSX.
        /// </summary>
        private void SimConnect_ExceptionRaised(object sender, SimConnectExceptionRaisedEventArgs args)
        {
            OnFlightSimulatorXExceptionRaised(new EventArgs<FlightSimulatorXException>(new FlightSimulatorXException(
                args.ExceptionCode,
                args.ParameterIndex,
                args.SendId
            )));
        }

        /// <summary>
        /// Called when SimConnect sends us the results of a request for information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SimConnect_ObjectReceived(object sender, SimConnectObjectReceivedEventArgs args)
        {
            if(args.RequestId == (uint)RequestId.ReadAircraftInformation) {
                if(args.Data != null && args.Data.Length > 0 && args.Data[0] is ReadAircraftInformation) {
                    ++MessagesReceivedCount;
                    OnAircraftInformationReceived(new EventArgs<ReadAircraftInformation>((ReadAircraftInformation)args.Data[0]));
                }
            }
        }

        /// <summary>
        /// Called when SimConnect is told that the user has quit Flight Simulator X.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SimConnect_UserHasQuit(object sender, EventArgs args)
        {
            DoDisconnect(Strings.FlightSimulatorXShutdown);
        }
        #endregion
    }
}
