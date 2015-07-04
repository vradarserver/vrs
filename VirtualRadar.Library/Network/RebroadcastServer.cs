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
        #region Private Class - AircraftListJsonContractResolver
        /// <summary>
        /// See base docs. Suppresses JSON properties that don't need to be sent in a rebroadcast server but do need to
        /// be sent to web sites.
        /// </summary>
        class AircraftListJsonContractResolver : DefaultContractResolver
        {
            private List<string> _AllowAircraftListJsonNames = new List<string>();
            private List<string> _SuppressAircraftJsonNames = new List<string>();

            public AircraftListJsonContractResolver() : base()
            {
                _AllowAircraftListJsonNames.Add(PropertyHelper.ExtractName<AircraftListJson>(r => r.Aircraft));

                _SuppressAircraftJsonNames.Add(PropertyHelper.ExtractName<AircraftJson>(r => r.UniqueId));
                _SuppressAircraftJsonNames.Add(PropertyHelper.ExtractName<AircraftJson>(r => r.HasSignalLevel));
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var result = base.CreateProperty(member, memberSerialization);

                if(member.DeclaringType == typeof(AircraftListJson)) {
                    if(!_AllowAircraftListJsonNames.Contains(member.Name)) {
                        result.ShouldSerialize = (instance) => false;
                    }
                } else if(member.DeclaringType == typeof(AircraftJson)) {
                    if(_SuppressAircraftJsonNames.Contains(member.Name)) {
                        result.ShouldSerialize = (instance) => false;
                    }
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// True if the listener's port 30003 message events have been hooked.
        /// </summary>
        private bool _Hooked_Port30003_Messages;

        /// <summary>
        /// True if the listener's raw bytes events have been hooked.
        /// </summary>
        private bool _Hooked_Raw_Bytes;

        /// <summary>
        /// True if the listener's raw Mode-S bytes event has been hooked.
        /// </summary>
        private bool _Hooked_ModeS_Bytes;

        /// <summary>
        /// Gets the listener that has had its events hooked.
        /// </summary>
        private IListener _HookedListener;

        /// <summary>
        /// The object that can compress messages for us.
        /// </summary>
        private IBaseStationMessageCompressor _Compressor;

        /// <summary>
        /// The timer object that aircraft list rebroadcasts use.
        /// </summary>
        private ITimer _Timer;

        /// <summary>
        /// The object that creates aircraft list JSON for us.
        /// </summary>
        private IAircraftListJsonBuilder _AircraftListJsonBuilder;

        /// <summary>
        /// The list of aircraft unique identifiers taken on the previous snapshot.
        /// </summary>
        private int[] _PreviousAircraftList;

        /// <summary>
        /// The last data version that was used when taking snapshots.
        /// </summary>
        private long _PreviousDataVersion = -1;

        /// <summary>
        /// The connector that has been hooked.
        /// </summary>
        private IConnector _HookedConnector;

        /// <summary>
        /// The object that suppresses properties on <see cref="AircraftListJson"/> that we're not interested in.
        /// </summary>
        private AircraftListJsonContractResolver _AircraftListJsonContractResolver;

        /// <summary>
        /// The serialiser settings for <see cref="AircraftListJson"/> serialisation.
        /// </summary>
        private JsonSerializerSettings _AircraftListJsonSerialiserSettings;
        #endregion

        #region Properties
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
        public RebroadcastFormat Format { get; set; }

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
        /// Gets a value indicating that we should rebroadcast something. If the server is offline or nothing is
        /// currently connected to it then there's no point in doing any rebroadcasting work.
        /// </summary>
        private bool ShouldRebroadcast
        {
            get { return Online && Connector.HasConnection; }
        }
        #endregion

        #region Events exposed
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
            if(ExceptionCaught != null) ExceptionCaught(this, args);
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
            if(OnlineChanged != null) OnlineChanged(this, args);
        }
        #endregion

        #region Constructor and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RebroadcastServer()
        {
            _Compressor = Factory.Singleton.Resolve<IBaseStationMessageCompressor>();
            SendIntervalMilliseconds = 1000;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~RebroadcastServer()
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
                if(_Hooked_Port30003_Messages) _HookedListener.Port30003MessageReceived -= Listener_Port30003MessageReceived;
                if(_Hooked_Raw_Bytes)          _HookedListener.RawBytesReceived -= Listener_RawBytesReceived;
                if(_Hooked_ModeS_Bytes)        _HookedListener.ModeSBytesReceived -= Listener_ModeSBytesReceived;
                _Hooked_Port30003_Messages = _Hooked_Raw_Bytes = false;

                if(_Timer != null) {
                    var timer = _Timer;
                    _Timer = null;
                    timer.Elapsed -= Timer_Elapsed;
                    timer.Dispose();
                }

                if(_HookedConnector != null) {
                    _HookedConnector.AddingConnection -= Connector_AddingConnection;
                    _HookedConnector = null;
                }
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(UniqueId == 0) throw new InvalidOperationException("UniqueId must be set before calling Initialise");
            if(Feed == null) throw new InvalidOperationException("Feed must be set before calling Initialise");
            if(Connector == null) throw new InvalidOperationException("Connector must be set before calling Initialise");
            if(Format == RebroadcastFormat.None) throw new InvalidOperationException("Format must be specified before calling Initialise");
            if(_Hooked_Port30003_Messages || _Hooked_Raw_Bytes || _Hooked_ModeS_Bytes || _AircraftListJsonBuilder != null) throw new InvalidOperationException("Initialise has already been called");

            Connector.Name = Name;
            Connector.EstablishConnection();

            _HookedListener = Feed.Listener;
            switch(Format) {
                case RebroadcastFormat.Passthrough:
                    _HookedListener.RawBytesReceived += Listener_RawBytesReceived;
                    _Hooked_Raw_Bytes = true;
                    break;
                case RebroadcastFormat.CompressedVRS:
                case RebroadcastFormat.Port30003:
                    _HookedListener.Port30003MessageReceived += Listener_Port30003MessageReceived;
                    _Hooked_Port30003_Messages = true;
                    break;
                case RebroadcastFormat.Avr:
                    _HookedListener.ModeSBytesReceived += Listener_ModeSBytesReceived;
                    _Hooked_ModeS_Bytes = true;
                    break;
                case RebroadcastFormat.AircraftListJson:
                    _AircraftListJsonBuilder = Factory.Singleton.Resolve<IAircraftListJsonBuilder>();
                    var provider = Factory.Singleton.Resolve<IWebSiteProvider>();
                    _AircraftListJsonBuilder.Initialise(provider);

                    _AircraftListJsonContractResolver = new AircraftListJsonContractResolver();
                    _AircraftListJsonSerialiserSettings = new JsonSerializerSettings() {
                        ContractResolver = _AircraftListJsonContractResolver,
                        
                    };

                    _HookedConnector = Connector;
                    _HookedConnector.AddingConnection += Connector_AddingConnection;

                    _Timer = Factory.Singleton.Resolve<ITimer>();
                    _Timer.Elapsed += Timer_Elapsed;
                    _Timer.AutoReset = false;
                    _Timer.Enabled = true;
                    _Timer.Interval = SendIntervalMilliseconds;

                    _Timer.Start();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region NibbleToAscii
        /// <summary>
        /// Returns the appropriate ASCII character for a nibble value between 0 and 15 inclusive.
        /// </summary>
        /// <param name="nibble"></param>
        /// <returns></returns>
        private byte NibbleToAscii(int nibble)
        {
            return (byte)((nibble < 10 ? 0x30 : 0x37) + nibble);
        }
        #endregion

        #region GetConnections, SendToNewConnection
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
        /// Sends bytes to a new connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <remarks>
        /// Note that this will be called on a background thread.
        /// </remarks>
        private void SendToNewConnection(IConnection connection)
        {
            var format = Format;
            switch(format) {
                case RebroadcastFormat.AircraftListJson:
                    SendAircraftList(connection);
                    break;
            }
        }
        #endregion

        #region Aircraft list rebroadcasting
        /// <summary>
        /// Sends the aircraft list.
        /// </summary>
        /// <param name="connection">The connection to send to. If this is not null then it is assumed that it is a new
        /// connection and that a full aircraft list has to be sent 'out-of-bound'. The connection is assumed to not already
        /// be in the connector's list of established connections.</param>
        /// <remarks>
        /// It's possible that this could be called on the timer's thread while the manager is messing about with
        /// our object.
        /// </remarks>
        private void SendAircraftList(IConnection connection = null)
        {
            var format = Format;
            var feed = Feed;
            var aircraftList = feed == null ? null : feed.AircraftList;
            var connector = Connector;
            var isNewConnection = connection != null;

            if(aircraftList != null && Online && connector != null && (connector.HasConnection || isNewConnection)) {
                switch(format) {
                    case RebroadcastFormat.AircraftListJson:
                        long timestamp, dataVersion;
                        var snapshot = aircraftList.TakeSnapshot(out timestamp, out dataVersion);

                        var args = new AircraftListJsonBuilderArgs() {
                            AlwaysShowIcao = true,
                            FeedsNotRequired = true,
                            IgnoreUnchanged = true,
                            OnlyIncludeMessageFields = true,
                            SourceFeedId = feed.UniqueId,
                            PreviousDataVersion = isNewConnection ? -1 : _PreviousDataVersion,
                        };

                        if(!isNewConnection) {
                            if(_PreviousAircraftList != null) {
                                args.PreviousAircraft.AddRange(_PreviousAircraftList);
                            }
                            _PreviousAircraftList = snapshot.Select(r => r.UniqueId).ToArray();
                            _PreviousDataVersion = dataVersion;
                        }

                        var json = _AircraftListJsonBuilder.Build(args);
                        if(json.Aircraft.Count > 0) {
                            var jsonText = JsonConvert.SerializeObject(json, _AircraftListJsonSerialiserSettings);

                            // The json text can include some entries that have ICAO codes and nothing else. When I
                            // first wrote this I was taking them out at this point... but that is a mistake, if the
                            // aircraft is sending messages between refreshes but not changing any message values (e.g.
                            // test beacons that transmit a constant callsign, altitude etc. then by removing their
                            // entry here we make the receiving end think that they've gone off the radar. We need to
                            // send ICAOs for aircraft that aren't changing message values. This will bump the JSON
                            // size up a bit but c'est la vie.

                            var bytes = Encoding.UTF8.GetBytes(jsonText);
                            if(connection != null) {
                                connection.Write(bytes);
                            } else {
                                connector.Write(bytes);
                            }
                        }
                        break;
                    default:
                        // Do not throw on an invalid format, it might be in the middle of being switched.
                        break;
                }
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Raised when the listener picks up a Port 30003 format message (or derives one from a raw message).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_Port30003MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(ShouldRebroadcast) {
                byte[] bytes;
                switch(Format) {
                    case RebroadcastFormat.CompressedVRS:   bytes = _Compressor.Compress(args.Message); break;
                    case RebroadcastFormat.Port30003:       bytes = Encoding.ASCII.GetBytes(String.Concat(args.Message.ToBaseStationString(), "\r\n")); break;
                    default:                                throw new NotImplementedException();
                }
                if(bytes != null && bytes.Length > 0) Connector.Write(bytes);
            }
        }

        /// <summary>
        /// Raised when the listener picks up raw bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_RawBytesReceived(object sender, EventArgs<byte[]> args)
        {
            if(ShouldRebroadcast) Connector.Write(args.Value);
        }

        /// <summary>
        /// Raised when the listener picks up Mode-S bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_ModeSBytesReceived(object sender, EventArgs<ExtractedBytes> args)
        {
            if(ShouldRebroadcast) {
                var extractedBytes = args.Value;

                var bytes = new byte[(extractedBytes.Length * 2) + 4];
                var di = 0;
                bytes[di++] = extractedBytes.HasParity ? (byte)0x2a : (byte)0x3a;

                var length = extractedBytes.Offset + extractedBytes.Length;
                for(int i = extractedBytes.Offset;i < length;++i) {
                    var sourceByte = extractedBytes.Bytes[i];
                    bytes[di++] = NibbleToAscii((sourceByte & 0xf0) >> 4);
                    bytes[di++] = NibbleToAscii(sourceByte & 0x0f);
                }

                bytes[di++] = (byte)0x3b;
                bytes[di++] = (byte)0x0d;
                bytes[di] = (byte)0x0a;

                Connector.Write(bytes);
            }
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
                SendAircraftList();
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

        /// <summary>
        /// Raised when <see cref="Connector"/> establishes a connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Connector_AddingConnection(object sender, ConnectionEventArgs args)
        {
            SendToNewConnection(args.Connection);
        }
        #endregion
    }
}
