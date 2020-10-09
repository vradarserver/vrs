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
using System.Threading;

namespace VirtualRadar.Interface.FlightSimulator
{
    /// <summary>
    /// The interface for objects that allows tests to abstract away the SimConnect object that we have to use to connect to FSX.
    /// </summary>
    /// <remarks>
    /// Part of the public behaviour of the <see cref="IFlightSimulator"/> object is that it must talk to SimConnect and say the right
    /// things. This provider wraps SimConnect so that we can check that IFSX is using it properly without having to have FSX installed.
    /// It is a very lightweight wrapper, you will need to read the SimConnect documentation in conjunction with the code.
    /// </remarks>
    public interface ISimConnectWrapper : IDisposable
    {
        /// <summary>
        /// Gets a value that indicates that SimConnect is installed and we were able to successfully make a null call on it without getting an exception.
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Gets the value to pass for parameters that are not used - wraps SimConnect.SIMCONNECT_UNUSED.
        /// </summary>
        uint UnusedValue { get; }

        /// <summary>
        /// Raised when SimConnect detects that an event has taken place within Flight Simulator.
        /// </summary>
        event EventHandler<SimConnectEventObservedEventArgs> EventObserved;

        /// <summary>
        /// Raised when SimConnect throws an exception.
        /// </summary>
        event EventHandler<SimConnectExceptionRaisedEventArgs> ExceptionRaised;

        /// <summary>
        /// Raised when the user quits Flight Simulator.
        /// </summary>
        event EventHandler UserHasQuit;

        /// <summary>
        /// Raised when SimConnect returns information about an object that was previously requested.
        /// </summary>
        event EventHandler<SimConnectObjectReceivedEventArgs> ObjectReceived;

        /// <summary>
        /// Wraps a call to SimConnect.AddClientEventToNotificationGroup.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="eventId"></param>
        /// <param name="maskable"></param>
        void AddClientEventToNotificationGroup(Enum groupId, Enum eventId, bool maskable);

        /// <summary>
        /// Wraps a call to SimConnect.AddToDataDefinition.
        /// </summary>
        /// <param name="defineId"></param>
        /// <param name="fieldName"></param>
        /// <param name="unitsName"></param>
        /// <param name="dataType"></param>
        /// <param name="epsilon"></param>
        /// <param name="dataId"></param>
        void AddToDataDefinition(Enum defineId, string fieldName, string unitsName, int dataType, float epsilon, uint dataId);

        /// <summary>
        /// Wraps a call to create the SimConnect object.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windowHandle"></param>
        /// <param name="userEventWin32"></param>
        /// <param name="eventHandle"></param>
        /// <param name="configurationIndex"></param>
        void CreateSimConnect(string name, IntPtr windowHandle, uint userEventWin32, WaitHandle eventHandle, uint configurationIndex);

        /// <summary>
        /// Wraps a call to SimConnect.MapClientEventToSimEvent.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventName"></param>
        void MapClientEventToSimEvent(Enum eventId, string eventName);

        /// <summary>
        /// Wraps a call to SimConnect.ReceiveMessage.
        /// </summary>
        void ReceiveMessage();

        /// <summary>
        /// Wraps a call to SimConnect.RegisterDataDefineStruct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitionId"></param>
        void RegisterDataDefineStruct<T>(Enum definitionId);

        /// <summary>
        /// Wraps a call to SimConnect.RequestDataOnSimObjectType.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="definitionId"></param>
        /// <param name="radius"></param>
        /// <param name="objectType"></param>
        void RequestDataOnSimObjectType(Enum requestId, Enum definitionId, uint radius, int objectType);

        /// <summary>
        /// Wraps a call to SetDataOnSimObject.
        /// </summary>
        /// <param name="defineID"></param>
        /// <param name="objectID"></param>
        /// <param name="flags"></param>
        /// <param name="data"></param>
        void SetDataOnSimObject(Enum defineID, uint objectID, int flags, object data);

        /// <summary>
        /// Wraps a call to SimConnect.SetSystemEventState.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="on"></param>
        void SetSystemEventState(Enum eventId, bool on);

        /// <summary>
        /// Wraps a call to SimConnect.SubscribeToSystemEvent.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventName"></param>
        void SubscribeToSystemEvent(Enum eventId, string eventName);

        /// <summary>
        /// Wraps a call to SimConnect.TransmitClientEvent.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="eventId"></param>
        /// <param name="value"></param>
        /// <param name="groupId"></param>
        /// <param name="flags"></param>
        void TransmitClientEvent(uint objectId, Enum eventId, uint value, Enum groupId, int flags);
    }

    /// <summary>
    /// The event args for the <see cref="ISimConnectWrapper.EventObserved"/> event.
    /// </summary>
    #pragma warning disable 1591 // Missing XML comment - this is a wrapper around a SimConnect event
    public class SimConnectEventObservedEventArgs : EventArgs
    {
        public uint Value { get; set; }
        public uint EventId { get; set; }
        public uint GroupId { get; set; }
        public bool GroupIdIsUnknown { get; set; }
    }
    #pragma warning restore 1591

    /// <summary>
    /// The event args for <see cref="ISimConnectWrapper.ExceptionRaised"/>
    /// </summary>
    #pragma warning disable 1591 // Missing XML comment - this is a wrapper around a SimConnect event
    public class SimConnectExceptionRaisedEventArgs : EventArgs
    {
        public uint ExceptionCode { get; set; }
        public uint ParameterIndex { get; set; }
        public uint SendId { get; set; }
        public bool ParameterIndexIsUnknown { get; set; }
        public bool SendIdIsUnknown { get; set; }
    }
    #pragma warning restore 1591

    /// <summary>
    /// The event args for <see cref="ISimConnectWrapper.ObjectReceived"/>.
    /// </summary>
    #pragma warning disable 1591 // Missing XML comment - this is a wrapper around a SimConnect event
    public class SimConnectObjectReceivedEventArgs : EventArgs
    {
        public object[] Data { get; set; }
        public uint DefineCount { get; set; }
        public uint DefineId { get; set; }
        public uint EntryNumber { get; set; }
        public uint Flags { get; set; }
        public uint ObjectId { get; set; }
        public uint OutOf { get; set; }
        public uint RequestId { get; set; }
    }
    #pragma warning restore 1591
}
