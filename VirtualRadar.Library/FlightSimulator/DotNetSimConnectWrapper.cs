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
using System.Threading;
using Microsoft.FlightSimulator.SimConnect;
using VirtualRadar.Interface;
using VirtualRadar.Interface.FlightSimulator;

namespace VirtualRadar.Library.FlightSimulator
{
    /// <summary>
    /// The default implementation of <see cref="ISimConnectWrapper"/>.
    /// </summary>
    sealed class DotNetSimConnectWrapper : ISimConnectWrapper
    {
        #region Fields
        /// <summary>
        /// The SimConnect object that the provider wraps access to.
        /// </summary>
        SimConnect _SimConnect;
        #endregion

        #region Properties
        private bool? _IsInstalled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsInstalled
        {
            get
            {
                if(_IsInstalled == null) {
                    try {
                        _IsInstalled = false;
                        NullCall();
                        _IsInstalled = true;
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("SimConnectWrapper.IsInstalled caught exception {0}", ex.ToString()));
                        _IsInstalled = false;
                    }
                }

                return _IsInstalled.Value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public uint UnusedValue { get { return SimConnect.SIMCONNECT_UNUSED; } }
        #endregion

        #region Events exposed - EventObserved, ExceptionRaised, ObjectReceived, UserHasQuit
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<SimConnectEventObservedEventArgs> EventObserved;

        /// <summary>
        /// Raises <see cref="EventObserved"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnEventObserved(SimConnectEventObservedEventArgs args)
        {
            EventHelper.RaiseQuickly(EventObserved, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<SimConnectExceptionRaisedEventArgs> ExceptionRaised;

        /// <summary>
        /// Raises <see cref="ExceptionRaised"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionRaised(SimConnectExceptionRaisedEventArgs args)
        {
            EventHelper.Raise(ExceptionRaised, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<SimConnectObjectReceivedEventArgs> ObjectReceived;

        /// <summary>
        /// Raises <see cref="ObjectReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnObjectReceived(SimConnectObjectReceivedEventArgs args)
        {
            EventHelper.RaiseQuickly(ObjectReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler UserHasQuit;

        /// <summary>
        /// Raises <see cref="UserHasQuit"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnUserHasQuit(EventArgs args)
        {
            EventHelper.Raise(UserHasQuit, this, args);
        }
        #endregion

        #region Constructors / finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DotNetSimConnectWrapper()
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
        /// Disposes of / finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            // We need to check to see if SimConnect is installed before we try to use it. We can only use it when
            // it's in another method so that we don't trigger an attempt to load it if it's missing.
            if(IsInstalled) DoDispose(disposing);
        }

        /// <summary>
        /// Performs the disposal of the SimConnect object - has to be in a separate method from Dispose because we need
        /// to make sure it doesn't trigger an attempt to load SimConnect when SimConnect is missing.
        /// </summary>
        /// <param name="disposing"></param>
        private void DoDispose(bool disposing)
        {
            if(disposing) {
                if(_SimConnect != null) _SimConnect.Dispose();
                _SimConnect = null;
            }
        }
        #endregion

        #region NullCall
        /// <summary>
        /// A call that does nothing but references SimConnect, which in turn will cause the .NET framework to try to load it.
        /// </summary>
        private void NullCall()
        {
            if(_SimConnect != null) _SimConnect.GetType();
        }
        #endregion

        #region AddClientEventToNotificationGroup, AddToDataDefinition, CreateSimConnect, MapClientEventToSimEvent, RegisterDataDefineStruct, SubscribeToSystemEvent
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="eventId"></param>
        /// <param name="maskable"></param>
        public void AddClientEventToNotificationGroup(Enum groupId, Enum eventId, bool maskable)
        {
            _SimConnect.AddClientEventToNotificationGroup(groupId, eventId, maskable);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="defineId"></param>
        /// <param name="fieldName"></param>
        /// <param name="unitsName"></param>
        /// <param name="dataType"></param>
        /// <param name="epsilon"></param>
        /// <param name="dataId"></param>
        public void AddToDataDefinition(Enum defineId, string fieldName, string unitsName, int dataType, float epsilon, uint dataId)
        {
            _SimConnect.AddToDataDefinition(defineId, fieldName, unitsName, (SIMCONNECT_DATATYPE)dataType, epsilon, dataId);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windowHandle"></param>
        /// <param name="userEventWin32"></param>
        /// <param name="eventHandle"></param>
        /// <param name="configurationIndex"></param>
        public void CreateSimConnect(string name, IntPtr windowHandle, uint userEventWin32, WaitHandle eventHandle, uint configurationIndex)
        {
            _SimConnect = new SimConnect("Virtual Radar Server", windowHandle, userEventWin32, eventHandle, configurationIndex);
            _SimConnect.OnRecvEvent += SimConnect_RecvEvent;
            _SimConnect.OnRecvException += SimConnect_RecvException;
            _SimConnect.OnRecvQuit += SimConnect_RecvQuit;
            _SimConnect.OnRecvSimobjectDataBytype += SimConnect_RecvSimobjectDataBytype;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventName"></param>
        public void MapClientEventToSimEvent(Enum eventId, string eventName)
        {
            _SimConnect.MapClientEventToSimEvent(eventId, eventName);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ReceiveMessage()
        {
            _SimConnect.ReceiveMessage();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitionId"></param>
        public void RegisterDataDefineStruct<T>(Enum definitionId)
        {
            _SimConnect.RegisterDataDefineStruct<T>(definitionId);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="definitionId"></param>
        /// <param name="radius"></param>
        /// <param name="objectType"></param>
        public void RequestDataOnSimObjectType(Enum requestId, Enum definitionId, uint radius, int objectType)
        {
            _SimConnect.RequestDataOnSimObjectType(requestId, definitionId, radius, (SIMCONNECT_SIMOBJECT_TYPE)objectType);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="defineID"></param>
        /// <param name="objectID"></param>
        /// <param name="flags"></param>
        /// <param name="data"></param>
        public void SetDataOnSimObject(Enum defineID, uint objectID, int flags, object data)
        {
            _SimConnect.SetDataOnSimObject(defineID, objectID, (SIMCONNECT_DATA_SET_FLAG)flags, data);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="on"></param>
        public void SetSystemEventState(Enum eventId, bool on)
        {
            _SimConnect.SetSystemEventState(eventId, on ? SIMCONNECT_STATE.ON : SIMCONNECT_STATE.OFF);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventName"></param>
        public void SubscribeToSystemEvent(Enum eventId, string eventName)
        {
            _SimConnect.SubscribeToSystemEvent(eventId, eventName);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="eventId"></param>
        /// <param name="value"></param>
        /// <param name="groupId"></param>
        /// <param name="flags"></param>
        public void TransmitClientEvent(uint objectId, Enum eventId, uint value, Enum groupId, int flags)
        {
            _SimConnect.TransmitClientEvent(objectId, eventId, value, groupId, (SIMCONNECT_EVENT_FLAG)flags);
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when SimConnect observes an event taking place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void SimConnect_RecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            var args = new SimConnectEventObservedEventArgs() {
                EventId = data.uEventID,
                GroupId = data.uGroupID,
                GroupIdIsUnknown = data.uGroupID == SIMCONNECT_RECV_EVENT.UNKNOWN_GROUP,
                Value = data.dwData,
            };

            OnEventObserved(args);
        }

        /// <summary>
        /// Called when SimConnect throws an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void SimConnect_RecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            var args = new SimConnectExceptionRaisedEventArgs() {
                ExceptionCode = data.dwException,
                ParameterIndex = data.dwIndex,
                SendId = data.dwSendID,
                ParameterIndexIsUnknown = data.dwIndex == SIMCONNECT_RECV_EXCEPTION.UNKNOWN_INDEX,
                SendIdIsUnknown = data.dwSendID == SIMCONNECT_RECV_EXCEPTION.UNKNOWN_SENDID,
            };

            OnExceptionRaised(args);
        }

        /// <summary>
        /// Called when SimConnect sends data for an object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void SimConnect_RecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            var args = new SimConnectObjectReceivedEventArgs() {
                Data = data.dwData,
                DefineCount = data.dwDefineCount,
                DefineId = data.dwDefineID,
                EntryNumber = data.dwentrynumber,
                Flags = data.dwFlags,
                ObjectId = data.dwObjectID,
                OutOf = data.dwoutof,
                RequestId = data.dwRequestID,
            };

            OnObjectReceived(args);
        }

        /// <summary>
        /// Called when SimConnect detects that the user has quit FSX.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void SimConnect_RecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            OnUserHasQuit(EventArgs.Empty);
        }
        #endregion
    }
}
