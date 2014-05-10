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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IRebroadcastServer"/>.
    /// </summary>
    sealed class RebroadcastServer : IRebroadcastServer
    {
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
        /// The object that can compress messages for us.
        /// </summary>
        private IBaseStationMessageCompressor _Compressor;
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
        public IListener Listener { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBroadcastProvider BroadcastProvider { get; set; }

        private bool _Online;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Online
        {
            get { return _Online; }
            set { if(_Online != value) { _Online = value; OnOnlineChanged(EventArgs.Empty); } }
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
                if(_Hooked_Port30003_Messages) Listener.Port30003MessageReceived -= Listener_Port30003MessageReceived;
                if(_Hooked_Raw_Bytes)          Listener.RawBytesReceived -= Listener_RawBytesReceived;
                if(_Hooked_ModeS_Bytes)        Listener.ModeSBytesReceived -= Listener_ModeSBytesReceived;
                _Hooked_Port30003_Messages = _Hooked_Raw_Bytes = false;
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
            if(Listener == null) throw new InvalidOperationException("Listener must be set before calling Initialise");
            if(BroadcastProvider == null) throw new InvalidOperationException("BroadcastProvider must be set before calling Initialise");
            if(Format == RebroadcastFormat.None) throw new InvalidOperationException("Format must be specified before calling Initialise");
            if(_Hooked_Port30003_Messages || _Hooked_Raw_Bytes || _Hooked_ModeS_Bytes) throw new InvalidOperationException("Initialise has already been called");

            BroadcastProvider.ExceptionCaught += BroadcastProvider_ExceptionCaught;
            BroadcastProvider.RebroadcastServerId = UniqueId;
            BroadcastProvider.BeginListening();

            switch(Format) {
                case RebroadcastFormat.Passthrough:
                    Listener.RawBytesReceived += Listener_RawBytesReceived;
                    _Hooked_Raw_Bytes = true;
                    break;
                case RebroadcastFormat.CompressedVRS:
                case RebroadcastFormat.Port30003:
                    Listener.Port30003MessageReceived += Listener_Port30003MessageReceived;
                    _Hooked_Port30003_Messages = true;
                    break;
                case RebroadcastFormat.Avr:
                    Listener.ModeSBytesReceived += Listener_ModeSBytesReceived;
                    _Hooked_ModeS_Bytes = true;
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

        #region GetConnections
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<RebroadcastServerConnection> GetConnections()
        {
            var result = new List<RebroadcastServerConnection>();
            if(BroadcastProvider != null) {
                BroadcastProvider.PopulateConnections(result);
                foreach(var connection in result) {
                    connection.RebroadcastServerId = UniqueId;
                    connection.Name = Name;
                }
            }

            return result;
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
            if(Online) {
                byte[] bytes;
                switch(Format) {
                    case RebroadcastFormat.CompressedVRS:   bytes = _Compressor.Compress(args.Message); break;
                    case RebroadcastFormat.Port30003:       bytes = Encoding.ASCII.GetBytes(String.Concat(args.Message.ToBaseStationString(), "\r\n")); break;
                    default:                                throw new NotImplementedException();
                }
                if(bytes != null && bytes.Length > 0) BroadcastProvider.Send(bytes);
            }
        }

        /// <summary>
        /// Raised when the listener picks up raw bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_RawBytesReceived(object sender, EventArgs<byte[]> args)
        {
            if(Online) BroadcastProvider.Send(args.Value);
        }

        /// <summary>
        /// Raised when the listener picks up Mode-S bytes from a receiver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_ModeSBytesReceived(object sender, EventArgs<ExtractedBytes> args)
        {
            if(Online) {
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

                BroadcastProvider.Send(bytes);
            }
        }

        /// <summary>
        /// Raised when the broadcast listener catches an exception on a background thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BroadcastProvider_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }
        #endregion
    }
}
