// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.FlightSimulator;

namespace VirtualRadar.Library.FlightSimulator
{
    /// <summary>
    /// The default implementation of <see cref="ISimConnectWrapper"/> when running under Mono.
    /// </summary>
    class MonoSimConnectWrapper : ISimConnectWrapper
    {
        public bool IsInstalled { get { return false; } }

        public uint UnusedValue { get; set; }

        #pragma warning disable 0067
        public event EventHandler<SimConnectEventObservedEventArgs>  EventObserved;

        public event EventHandler<SimConnectExceptionRaisedEventArgs>  ExceptionRaised;

        public event EventHandler UserHasQuit;

        public event EventHandler<SimConnectObjectReceivedEventArgs>  ObjectReceived;
        #pragma warning restore 0067

        public void AddClientEventToNotificationGroup(Enum groupId, Enum eventId, bool maskable)
        {
            ;
        }

        public void AddToDataDefinition(Enum defineId, string fieldName, string unitsName, int dataType, float epsilon, uint dataId)
        {
            ;
        }

        public void CreateSimConnect(string name, IntPtr windowHandle, uint userEventWin32, System.Threading.WaitHandle eventHandle, uint configurationIndex)
        {
            ;
        }

        public void MapClientEventToSimEvent(Enum eventId, string eventName)
        {
            ;
        }

        public void ReceiveMessage()
        {
            ;
        }

        public void RegisterDataDefineStruct<T>(Enum definitionId)
        {
            ;
        }

        public void RequestDataOnSimObjectType(Enum requestId, Enum definitionId, uint radius, int objectType)
        {
            ;
        }

        public void SetDataOnSimObject(Enum defineID, uint objectID, int flags, object data)
        {
            ;
        }

        public void SetSystemEventState(Enum eventId, bool on)
        {
            ;
        }

        public void SubscribeToSystemEvent(Enum eventId, string eventName)
        {
            ;
        }

        public void TransmitClientEvent(uint objectId, Enum eventId, uint value, Enum groupId, int flags)
        {
            ;
        }

        public void Dispose()
        {
            ;
        }
    }
}
