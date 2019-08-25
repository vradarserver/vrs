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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using InterfaceFactory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of <see cref="IRebroadcastServer"/>.
    /// </summary>
    sealed class RebroadcastServer : IRebroadcastServer
    {
        /// <summary>
        /// The timer object that aircraft list rebroadcasts use.
        /// </summary>
        private ITimer _Timer;

        /// <summary>
        /// The provider that does most of the work.
        /// </summary>
        private IRebroadcastFormatProvider _Provider;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int SendIntervalMilliseconds { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFeed Feed { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public INetworkConnector Connector { get; set; }

        private bool _Online;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Online
        {
            get { return _Online; }
            set { if(_Online != value) { _Online = value; OnOnlineChanged(EventArgs.Empty); } }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ConnectionEstablished
        {
            get { return Online && Connector.HasConnection; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler OnlineChanged;

        /// <summary>
        /// Raises <see cref="OnlineChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnOnlineChanged(EventArgs args)
        {
            EventHelper.Raise(OnlineChanged, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RebroadcastServer()
        {
            SendIntervalMilliseconds = 1000;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~RebroadcastServer()
        {
            Dispose(false);
        }

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
                if(_Timer != null) {
                    var timer = _Timer;
                    _Timer = null;
                    timer.Elapsed -= Timer_Elapsed;
                    timer.Dispose();
                }

                if(_Provider != null) {
                    _Provider.UnhookFeed();
                    _Provider = null;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(UniqueId == 0) throw new InvalidOperationException("UniqueId must be set before calling Initialise");
            if(Feed == null) throw new InvalidOperationException("Feed must be set before calling Initialise");
            if(Connector == null) throw new InvalidOperationException("Connector must be set before calling Initialise");
            if(String.IsNullOrEmpty(Format)) throw new InvalidOperationException("Format must be specified before calling Initialise");
            if(_Provider != null) throw new InvalidOperationException("Initialise has already been called");

            Connector.Name = Name;
            Connector.EstablishConnection();

            var providerManager = Factory.Resolve<IRebroadcastFormatManager>().Singleton;
            _Provider = providerManager.CreateProvider(Format);
            if(_Provider == null) throw new InvalidOperationException(String.Format("There is no rebroadcast format registered with a unique ID of {0}", Format));

            _Provider.RebroadcastServer = this;
            _Provider.HookFeed();

            if(_Provider.UsesSendIntervalMilliseconds) {
                _Timer = Factory.Resolve<ITimer>();
                _Timer.Elapsed += Timer_Elapsed;
                _Timer.AutoReset = false;
                _Timer.Enabled = true;
                _Timer.Interval = SendIntervalMilliseconds;

                _Timer.Start();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<RebroadcastServerConnection> GetConnections()
        {
            var result = new List<RebroadcastServerConnection>();
            if(Connector != null) {
                foreach(var connection in Connector.GetConnections().OfType<INetworkConnection>()) {
                    var localEndPoint = connection.LocalEndPoint;
                    var remoteEndPoint = connection.RemoteEndPoint;
                    if(localEndPoint != null && remoteEndPoint != null) {
                        var cookedConnection = new RebroadcastServerConnection() {
                            BytesBuffered =         connection.WriteQueueBytes,
                            BytesWritten =          connection.BytesWritten,
                            EndpointIPAddress =     remoteEndPoint == null ? null : remoteEndPoint.Address,
                            EndpointPort =          remoteEndPoint == null ? 0 : remoteEndPoint.Port,
                            LocalPort =             localEndPoint == null ? 0 : localEndPoint.Port,
                            Name =                  Name,
                            RebroadcastServerId =   UniqueId,
                            StaleBytesDiscarded =   connection.StaleBytesDiscarded,
                        };
                        result.Add(cookedConnection);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Raised when <see cref="_Timer"/>'s timer elapses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Timer_Elapsed(object sender, EventArgs args)
        {
            var abortingThread = false;

            try {
                var provider = _Provider;
                if(provider != null && provider.UsesSendIntervalMilliseconds) {
                    provider.SendIntervalElapsed();
                }
            } catch(ThreadAbortException) {
                abortingThread = true;
                // rethrow is automatic
            } finally {
                if(!abortingThread) {
                    var timer = _Timer;
                    if(timer != null) {
                        try {
                            timer.Interval = SendIntervalMilliseconds;
                            timer.Start();
                        } catch(ObjectDisposedException) {
                        }
                    }
                }
            }
        }
    }
}
