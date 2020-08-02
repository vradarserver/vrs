// Copyright © 2020 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.XPlane;

namespace VirtualRadar.Library.XPlane
{
    /// <summary>
    /// Default implementation of <see cref="IXPlaneUdp"/>.
    /// </summary>
    class XPlaneUdp : IXPlaneUdp
    {
        /// <summary>
        /// The UDP client that manages communications for us.
        /// </summary>
        private UdpClient _Client;

        /// <summary>
        /// The remote endpoint that messages will be sent to / received from.
        /// </summary>
        private IPEndPoint _RemoteEndPoint;

        /// <summary>
        /// The async result for the BeginReceive call.
        /// </summary>
        private IAsyncResult _ReceiveResult;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Host { get; private set; } = "127.0.0.1";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int XPlanePort { get; private set; } = 49000;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReplyPort { get; private set; } = 39000;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<XPlaneRposReply>> RposReplyReceived;

        /// <summary>
        /// Raises <see cref="RposReplyReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRposReplyReceived(EventArgs<XPlaneRposReply> args)
        {
            EventHelper.Raise<EventArgs<XPlaneRposReply>>(RposReplyReceived, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="xplanePort"></param>
        /// <param name="replyPort"></param>
        public void Initialise(string host, int xplanePort, int replyPort)
        {
            if(_Client != null) {
                throw new InvalidOperationException($"You can only initialise an XPlane UDP object once");
            }

            Host = host;
            XPlanePort = xplanePort;
            ReplyPort = replyPort;

            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(Host), XPlanePort);
            _Client = new UdpClient(ReplyPort);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="sendFramesPerSecond"></param>
        public void SendRPOS(int sendFramesPerSecond)
        {
            var requestBytes = FormatRposRequest(sendFramesPerSecond);
            _Client.Send(requestBytes, requestBytes.Length, _RemoteEndPoint);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartListener()
        {
            BeginReceiving();
        }

        private byte[] FormatRposRequest(int sendFramesPerSecond)
        {
            var result = new byte[9];
            using(var stream = new MemoryStream(result)) {
                using(var writer = new BinaryWriter(stream, Encoding.ASCII)) {
                    writer.Write('R');
                    writer.Write('P');
                    writer.Write('O');
                    writer.Write('S');
                    writer.Write('\0');
                    writer.Write(sendFramesPerSecond);
                }
            }

            return result;
        }

        private void BeginReceiving()
        {
            var client = _Client;
            if(_ReceiveResult == null && client != null) {
                try {
                    client.BeginReceive(MessageReceived, client);
                } catch(ObjectDisposedException) {
                    ;
                }
            }
        }

        private void MessageReceived(IAsyncResult asyncResult)
        {
            _ReceiveResult = null;

            var client = (UdpClient)asyncResult.AsyncState;
            if(client == _Client) {
                try {
                    var remoteEndpoint = new IPEndPoint(IPAddress.Parse(Host), 0);
                    var packet = client.EndReceive(asyncResult, ref remoteEndpoint);
                
                    var parsed = XPlaneRposReply.ParseResponse(packet);
                    if(parsed != null) {
                        OnRposReplyReceived(new EventArgs<XPlaneRposReply>(parsed));
                    }
                } catch(ThreadAbortException) {
                    ;
                } catch(ObjectDisposedException) {
                    ;
                } catch(Exception ex) {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine($"Caught exception when reading UDP packet from XPlane: {ex}");
                }
            
                BeginReceiving();
            }
        }
    }
}
