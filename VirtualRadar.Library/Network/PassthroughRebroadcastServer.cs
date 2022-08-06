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
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// A rebroadcast server that just copies incoming feeds without alteration.
    /// </summary>
    class PassthroughRebroadcastServer : IRebroadcastFormatProvider
    {
        /// <summary>
        /// The hooked listener.
        /// </summary>
        private IListener _Listener;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId { get { return RebroadcastFormat.Passthrough; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortName { get { return Strings.RebroadcastFormatPassthrough; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanRebroadcastMergedFeed { get { return false; } }

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
            return new PassthroughRebroadcastServer();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void HookFeed()
        {
            if(_Listener != null) {
                UnhookFeed();
            }

            if(RebroadcastServer.Feed is INetworkFeed networkFeed) {
                _Listener = networkFeed.Listener;
                _Listener.RawBytesReceived += Listener_RawBytesReceived;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void UnhookFeed()
        {
            if(_Listener != null) {
                _Listener.RawBytesReceived -= Listener_RawBytesReceived;
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
        /// Raised when the listener picks up raw bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_RawBytesReceived(object sender, EventArgs<byte[]> args)
        {
            if(RebroadcastServer.ConnectionEstablished) {
                RebroadcastServer.Connector.Write(args.Value);
            }
        }
    }
}
