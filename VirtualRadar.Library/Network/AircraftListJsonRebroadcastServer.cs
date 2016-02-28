// Copyright © 2016 onwards, Andrew Whewell
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
using InterfaceFactory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// A rebroadcast server that sends the feed in AircraftList JSON format.
    /// </summary>
    class AircraftListJsonRebroadcastServer : IRebroadcastFormatProvider
    {
        /// <summary>
        /// NewtonSoft JSON.NET resolver that suppresses JSON properties that don't need to be sent
        /// in a rebroadcast server.
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

        /// <summary>
        /// The hooked connector.
        /// </summary>
        private IConnector _Connector;

        /// <summary>
        /// The object that creates aircraft list JSON for us.
        /// </summary>
        private IAircraftListJsonBuilder _AircraftListJsonBuilder;

        /// <summary>
        /// The object that suppresses properties on <see cref="AircraftListJson"/> that we're not interested in.
        /// </summary>
        private AircraftListJsonContractResolver _AircraftListJsonContractResolver;

        /// <summary>
        /// The serialiser settings for <see cref="AircraftListJson"/> serialisation.
        /// </summary>
        private JsonSerializerSettings _AircraftListJsonSerialiserSettings;

        /// <summary>
        /// The list of aircraft unique identifiers taken on the previous snapshot.
        /// </summary>
        private int[] _PreviousAircraftList;

        /// <summary>
        /// The last data version that was used when taking snapshots.
        /// </summary>
        private long _PreviousDataVersion = -1;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId { get { return RebroadcastFormat.AircraftListJson; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortName { get { return Strings.AircraftListJson; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanRebroadcastMergedFeed { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UsesReceiverAircraftList { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UsesSendIntervalMilliseconds { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRebroadcastServer RebroadcastServer { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public bool CanRebroadcastReceiver(Receiver receiver)
        {
            // Disallow receivers that do not have aircraft lists
            return receiver.ReceiverUsage != ReceiverUsage.MergeOnly;
        }

        /// <summary>
        /// See interval docs.
        /// </summary>
        /// <param name="sendIntervalMilliseconds"></param>
        /// <returns></returns>
        public bool IsValidSendIntervalMilliseconds(int sendIntervalMilliseconds)
        {
            return sendIntervalMilliseconds >= 1000 && sendIntervalMilliseconds <= 30000;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IRebroadcastFormatProvider CreateNewInstance()
        {
            return new AircraftListJsonRebroadcastServer();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void HookFeed()
        {
            if(_Connector != null) {
                UnhookFeed();
            }

            if(_AircraftListJsonBuilder == null) {
                _AircraftListJsonBuilder = Factory.Singleton.Resolve<IAircraftListJsonBuilder>();
                var provider = Factory.Singleton.Resolve<IWebSiteProvider>();
                _AircraftListJsonBuilder.Initialise(provider);
            }

            if(_AircraftListJsonContractResolver == null) {
                _AircraftListJsonContractResolver = new AircraftListJsonContractResolver();

                _AircraftListJsonSerialiserSettings = new JsonSerializerSettings() {
                    ContractResolver = _AircraftListJsonContractResolver,
                };
            }

            _Connector = RebroadcastServer.Connector;
            _Connector.AddingConnection += Connector_AddingConnection;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void UnhookFeed()
        {
            if(_Connector != null) {
                _Connector.AddingConnection -= Connector_AddingConnection;
                _Connector = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SendIntervalElapsed()
        {
            SendAircraftList(null);
        }

        /// <summary>
        /// Sends the aircraft list to the connection passed across.
        /// </summary>
        /// <param name="connection">Null the established connection should be used, otherwise the new connection
        /// that the connector has created.</param>
        private void SendAircraftList(IConnection connection)
        {
            var feed = RebroadcastServer.Feed;
            var aircraftList = feed == null ? null : feed.AircraftList;
            var connector = RebroadcastServer.Connector;
            var isNewConnection = connection != null;

            if(aircraftList != null && RebroadcastServer.Online && connector != null && (connector.HasConnection || isNewConnection)) {
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
                    // send ICAOs for aircraft that aren't changing message values.

                    var bytes = Encoding.UTF8.GetBytes(jsonText);
                    if(connection != null) {
                        connection.Write(bytes);
                    } else {
                        connector.Write(bytes);
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the hooked connector establishes a connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Connector_AddingConnection(object sender, ConnectionEventArgs args)
        {
            SendAircraftList(args.Connection);
        }
    }
}
