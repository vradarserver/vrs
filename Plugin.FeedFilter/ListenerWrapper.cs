// Copyright © 2015 onwards, Andrew Whewell
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
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// An implementation of <see cref="IListener"/> that wraps the real implementation of the class and
    /// filters out unwanted elements from the feed.
    /// </summary>
    sealed class ListenerWrapper : IListener
    {
        #region Fields
        /// <summary>
        /// An instance of the original listener that was in place before we took it over.
        /// </summary>
        private IListener _Original;
        #endregion

        #region Ctor, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ListenerWrapper()
        {
            _Original = OriginalImplementationFactory.Resolve<IListener>();

            _Original.ConnectionStateChanged +=     Original_ConnectionStateChanged;
            _Original.ExceptionCaught +=            Original_ExceptionCaught;
            _Original.ModeSBytesReceived +=         Original_ModeSBytesReceived;
            _Original.ModeSMessageReceived +=       Original_ModeSMessageReceived;
            _Original.Port30003MessageReceived +=   Original_Port30003MessageReceived;
            _Original.PositionReset +=              Original_PositionReset;
            _Original.RawBytesReceived +=           Original_RawBytesReceived;
            _Original.SourceChanged +=              Original_SourceChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ListenerWrapper()
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
        /// Disoposes or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks>
        /// Note that the object is sealed, hence this is private instead of protected virtual.
        /// </remarks>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Original != null) {
                    _Original.Dispose();
                }
            }
        }
        #endregion

        #region Wrapped Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId
        {
            get { return _Original.ReceiverId; }
            set { _Original.ReceiverId = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ReceiverName
        {
            get { return _Original.ReceiverName; }
            set { _Original.ReceiverName = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStatistics Statistics
        {
            get { return _Original.Statistics; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnector Connector
        {
            get { return _Original.Connector; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IMessageBytesExtractor BytesExtractor
        {
            get { return _Original.BytesExtractor; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRawMessageTranslator RawMessageTranslator
        {
            get { return _Original.RawMessageTranslator; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return _Original.ConnectionStatus; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalMessages
        {
            get { return _Original.TotalMessages; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalBadMessages
        {
            get { return _Original.TotalBadMessages; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreBadMessages
        {
            get { return _Original.IgnoreBadMessages; }
            set { _Original.IgnoreBadMessages = value; }
        }
        #endregion

        #region Wrapped Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<byte[]>> RawBytesReceived;

        /// <summary>
        /// Raises <see cref="RawBytesReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnRawBytesReceived(EventArgs<byte[]> args)
        {
            if(RawBytesReceived != null) RawBytesReceived(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<ExtractedBytes>> ModeSBytesReceived;

        /// <summary>
        /// Raises <see cref="ModeSBytesReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnModeSBytesReceived(EventArgs<ExtractedBytes> args)
        {
            if(ModeSBytesReceived != null) ModeSBytesReceived(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<BaseStationMessageEventArgs> Port30003MessageReceived;

        /// <summary>
        /// Raises <see cref="Port30003MessageReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnPort30003MessageReceived(BaseStationMessageEventArgs args)
        {
            if(Port30003MessageReceived != null) Port30003MessageReceived(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<ModeSMessageEventArgs> ModeSMessageReceived;

        /// <summary>
        /// Raises <see cref="ModeSMessageReceived"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnModeSMessageReceived(ModeSMessageEventArgs args)
        {
            if(ModeSMessageReceived != null) ModeSMessageReceived(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnConnectionStateChanged(EventArgs args)
        {
            if(ConnectionStateChanged != null) ConnectionStateChanged(this, args);
        }

        /// <summary>
        /// Raises <see cref="SourceChanged"/>.
        /// </summary>
        public event EventHandler SourceChanged;

        private void OnSourceChanged(EventArgs args)
        {
            if(SourceChanged != null) SourceChanged(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<string>> PositionReset;

        /// <summary>
        /// Raises <see cref="PositionReset"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnPositionReset(EventArgs<string> args)
        {
            if(PositionReset != null) PositionReset(this, args);
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
            if(ExceptionCaught != null) ExceptionCaught(this, args);
        }
        #endregion

        #region Wrapped Methods
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="bytesExtractor"></param>
        /// <param name="rawMessageTranslator"></param>
        public void ChangeSource(IConnector connector, IMessageBytesExtractor bytesExtractor, IRawMessageTranslator rawMessageTranslator)
        {
            _Original.ChangeSource(connector, bytesExtractor, rawMessageTranslator);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            _Original.Connect();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            _Original.Disconnect();
        }
        #endregion

        #region Events Subscribed
        /// <summary>
        /// Passes through events from the original <see cref="RawBytesReceived"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_RawBytesReceived(object sender, EventArgs<byte[]> args)
        {
            if(!Filter.AreUnfilterableFeedsProhibited()) {
                OnRawBytesReceived(args);
            }
        }

        /// <summary>
        /// Passes through events from the original <see cref="ModeSBytesReceived"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_ModeSBytesReceived(object sender, EventArgs<ExtractedBytes> args)
        {
            if(!Filter.AreUnfilterableFeedsProhibited()) {
                OnModeSBytesReceived(args);
            }
        }

        /// <summary>
        /// Passes through events from the original <see cref="Port30003MessageReceived"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_Port30003MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            args = Filter.FilterEvent(args);
            if(args != null) {
                OnPort30003MessageReceived(args);
            }
        }

        /// <summary>
        /// Passes through events from the original <see cref="ModeSMessageReceived"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_ModeSMessageReceived(object sender, ModeSMessageEventArgs args)
        {
            args = Filter.FilterEvent(args);
            if(args != null) {
                OnModeSMessageReceived(args);
            }
        }

        /// <summary>
        /// Passes through events from the original <see cref="ConnectionStateChanged"/>.
        /// </summary>
        private void Original_ConnectionStateChanged(object sender, EventArgs args)
        {
            OnConnectionStateChanged(args);
        }

        /// <summary>
        /// Passes through events from the original <see cref="SourceChanged"/>.
        /// </summary>
        private void Original_SourceChanged(object sender, EventArgs args)
        {
            OnSourceChanged(args);
        }

        /// <summary>
        /// Passes through events from the original <see cref="PositionReset"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_PositionReset(object sender, EventArgs<string> args)
        {
            OnPositionReset(args);
        }

        /// <summary>
        /// Passes through events from the original <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Original_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }
        #endregion
    }
}
