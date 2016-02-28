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
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// A rebroadcast server that sends the feed in BaseStation format.
    /// </summary>
    class Port30003RebroadcastServer : IRebroadcastFormatProvider
    {
        /// <summary>
        /// True if the provider is to emit extended BaseStation format messages.
        /// </summary>
        private bool _ExtendedBasestationFormat = false;

        /// <summary>
        /// The hooked listener.
        /// </summary>
        private IListener _Listener;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId
        {
            get {
                return _ExtendedBasestationFormat ? RebroadcastFormat.ExtendedBaseStation : RebroadcastFormat.Port30003;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortName
        {
            get {
                return _ExtendedBasestationFormat ? Strings.ExtendedBaseStation : Strings.RebroadcastFormatPort30003;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanRebroadcastMergedFeed { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UsesReceiverAircraftList { get { return false; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UsesSendIntervalMilliseconds { get { return false; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRebroadcastServer RebroadcastServer { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="extendedBaseStationFormat"></param>
        public Port30003RebroadcastServer(bool extendedBaseStationFormat)
        {
            _ExtendedBasestationFormat = extendedBaseStationFormat;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public bool CanRebroadcastReceiver(Receiver receiver)
        {
            return true;
        }

        /// <summary>
        /// See interval docs.
        /// </summary>
        /// <param name="sendIntervalMilliseconds"></param>
        /// <returns></returns>
        public bool IsValidSendIntervalMilliseconds(int sendIntervalMilliseconds)
        {
            return true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IRebroadcastFormatProvider CreateNewInstance()
        {
            return new Port30003RebroadcastServer(_ExtendedBasestationFormat);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void HookFeed()
        {
            if(_Listener != null) {
                UnhookFeed();
            }

            _Listener = RebroadcastServer.Feed.Listener;
            _Listener.Port30003MessageReceived += Listener_Port30003MessageReceived;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void UnhookFeed()
        {
            if(_Listener != null) {
                _Listener.Port30003MessageReceived -= Listener_Port30003MessageReceived;
                _Listener = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SendIntervalElapsed()
        {
            ;
        }

        /// <summary>
        /// Raised when the listener picks up a Port 30003 format message (or derives one from a raw message).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_Port30003MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(RebroadcastServer.ConnectionEstablished) {
                byte[] bytes = Encoding.ASCII.GetBytes(String.Concat(args.Message.ToBaseStationString(_ExtendedBasestationFormat), "\r\n"));
                if(bytes != null && bytes.Length > 0) {
                    RebroadcastServer.Connector.Write(bytes);
                }
            }
        }
    }
}
