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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// A rebroadcast server that sends the feed in AVR format.
    /// </summary>
    class AvrRebroadcastServer : IRebroadcastFormatProvider
    {
        /// <summary>
        /// The hooked listener.
        /// </summary>
        private IListener _Listener;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId { get { return RebroadcastFormat.Avr; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortName { get { return Strings.RebroadcastFormatAvr; } }

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
            var receiverFormatManager = Factory.Singleton.Resolve<IReceiverFormatManager>().Singleton;
            var receiverFormatProvider = receiverFormatManager.GetProvider(receiver.DataSource);
            return receiverFormatProvider.IsRawFormat;
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
            return new AvrRebroadcastServer();
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
            _Listener.ModeSBytesReceived += Listener_ModeSBytesReceived;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void UnhookFeed()
        {
            if(_Listener != null) {
                _Listener.ModeSBytesReceived -= Listener_ModeSBytesReceived;
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
        /// Raised when the listener picks up Mode-S bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_ModeSBytesReceived(object sender, EventArgs<ExtractedBytes> args)
        {
            if(RebroadcastServer.ConnectionEstablished) {
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

                RebroadcastServer.Connector.Write(bytes);
            }
        }

        /// <summary>
        /// Returns the appropriate ASCII character for a nibble value between 0 and 15 inclusive.
        /// </summary>
        /// <param name="nibble"></param>
        /// <returns></returns>
        private byte NibbleToAscii(int nibble)
        {
            return (byte)((nibble < 10 ? 0x30 : 0x37) + nibble);
        }
    }
}
