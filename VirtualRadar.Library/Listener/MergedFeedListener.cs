// Copyright © 2013 onwards, Andrew Whewell
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using InterfaceFactory;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IMergedFeedListener"/>.
    /// </summary>
    class MergedFeedListener : IMergedFeedListener
    {
        #region Private class - Source
        class Source
        {
            /// <summary>
            /// The listener that is the only source of messages from an ICAO that the merged feed will report on.
            /// </summary>
            public IListener Listener;

            /// <summary>
            /// The last time that a message for a given ICAO was picked up.
            /// </summary>
            public DateTime LastMessageUtc;

            /// <summary>
            /// True if a position message has been seen for an aircraft.
            /// </summary>
            public bool SeenPositionMessage;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object used to protect fields from multithreaded access.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that is managing the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// A map of ICAO codes to the listener that first reported a message from that ICAO. Once this relationship is established the merged
        /// feed will only report messages for an ICAO if they are raised by the listener - other listeners can't contribute their messages.
        /// However once <see cref="IcaoTimeout"/> milliseconds elapse the source is removed from the list and the next listener that reports
        /// a message for the ICAO will become the source.
        /// </summary>
        private Dictionary<string, Source> _IcaoSourceMap = new Dictionary<string,Source>();

        /// <summary>
        /// Set if the slow tick has been hooked.
        /// </summary>
        private bool _HookedSlowTick;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ReceiverName { get; set; }

        private List<IListener> _Listeners = new List<IListener>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ReadOnlyCollection<IListener> Listeners { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int IcaoTimeout { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreAircraftWithNoPosition { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IListenerProvider Provider { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStatistics Statistics { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IMessageBytesExtractor BytesExtractor { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRawMessageTranslator RawMessageTranslator { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectionStatus ConnectionStatus { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalMessages { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalBadMessages { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreBadMessages { get; set; }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<BaseStationMessageEventArgs> Port30003MessageReceived;
        protected virtual void OnPort30003MessageReceived(BaseStationMessageEventArgs args)
        {
            if(Port30003MessageReceived != null) Port30003MessageReceived(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<string>> PositionReset;
        protected virtual void OnPositionReset(EventArgs<string> args)
        {
            if(PositionReset != null) PositionReset(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            if(ExceptionCaught != null) ExceptionCaught(this, args);
        }

        #region Inert events
        #pragma warning disable 0067    // Unused event
        /// <summary>
        /// See interface docs. Inert.
        /// </summary>
        public event EventHandler<EventArgs<byte[]>> RawBytesReceived;

        /// <summary>
        /// See interface docs. Inert.
        /// </summary>
        public event EventHandler<EventArgs<ExtractedBytes>> ModeSBytesReceived;

        /// <summary>
        /// See interface docs. Inert.
        /// </summary>
        public event EventHandler<ModeSMessageEventArgs> ModeSMessageReceived;

        /// <summary>
        /// See interface docs. Inert.
        /// </summary>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// See interface docs. Inert.
        /// </summary>
        public event EventHandler SourceChanged;
        #pragma warning restore 0067
        #endregion
        #endregion

        #region Constructor, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MergedFeedListener()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();
            Listeners = new ReadOnlyCollection<IListener>(_Listeners);
            IcaoTimeout = 5000;
            ConnectionStatus = ConnectionStatus.Connected;

            var heartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
            heartbeatService.SlowTick += HeartbeatService_SlowTick;
            _HookedSlowTick = true;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~MergedFeedListener()
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
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_HookedSlowTick) {
                    _HookedSlowTick = false;
                    var heartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
                    heartbeatService.SlowTick -= HeartbeatService_SlowTick;
                }

                lock(_SyncLock) {
                    foreach(var listener in _Listeners) {
                        listener.Port30003MessageReceived -= Listener_Port30003MessageReceived;
                        listener.PositionReset -= Listener_PositionReset;
                    }
                    _Listeners.Clear();
                }
            }
        }
        #endregion

        #region SetListeners
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="listeners"></param>
        public void SetListeners(IEnumerable<IListener> listeners)
        {
            lock(_SyncLock) {
                var newListeners = listeners.Except(_Listeners).ToArray();
                var oldListeners = _Listeners.Except(listeners).ToArray();

                foreach(var oldListener in oldListeners) {
                    oldListener.Port30003MessageReceived -= Listener_Port30003MessageReceived;
                    oldListener.PositionReset -= Listener_PositionReset;
                }

                foreach(var newListener in newListeners) {
                    newListener.Port30003MessageReceived += Listener_Port30003MessageReceived;
                    newListener.PositionReset += Listener_PositionReset;
                    _Listeners.Add(newListener);
                }
            }
        }
        #endregion

        #region ChangeSource, Connect, Disconnect
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="bytesExtractor"></param>
        /// <param name="rawMessageTranslator"></param>
        /// <param name="autoReconnect"></param>
        public void ChangeSource(IListenerProvider provider, IMessageBytesExtractor bytesExtractor, IRawMessageTranslator rawMessageTranslator, bool autoReconnect)
        {
            throw new InvalidOperationException("You cannot call ChangeSource on a merged feed listener");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="autoReconnect"></param>
        public void Connect(bool autoReconnect)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            ;
        }
        #endregion

        #region FilterMessageFromListener, CleanupOldIcaos, MessageCarriesPosition
        /// <summary>
        /// Filters BaseStation messages and raises <see cref="Port30003MessageReceived"/> with those messages that
        /// pass the filter.
        /// </summary>
        /// <param name="receivedUtc"></param>
        /// <param name="listener"></param>
        /// <param name="icao"></param>
        /// <param name="hasPosition"></param>
        private bool FilterMessageFromListener(DateTime receivedUtc, IListener listener, string icao, bool hasPosition)
        {
            icao = icao ?? "";

            Source source;
            lock(_SyncLock) {
                if(!_IcaoSourceMap.TryGetValue(icao, out source)) {
                    source = new Source() {
                        LastMessageUtc = receivedUtc,
                        Listener = listener,
                        SeenPositionMessage = hasPosition,
                    };
                    _IcaoSourceMap.Add(icao, source);
                } else {
                    if(source.Listener != listener) {
                        var threshold = receivedUtc.AddMilliseconds(-IcaoTimeout);
                        if(source.LastMessageUtc < threshold) source.Listener = listener;
                        else                                  source = null;
                    }

                    if(source != null) {
                        source.LastMessageUtc = receivedUtc;
                        if(hasPosition) source.SeenPositionMessage = true;
                    }
                }

                if(source != null && IgnoreAircraftWithNoPosition && !source.SeenPositionMessage) source = null;
            }

            return source != null;
        }

        /// <summary>
        /// Removes old entries from <see cref="_IcaoSourceMap"/>
        /// </summary>
        private void CleanupOldIcaos()
        {
            var threshold = _Clock.UtcNow.AddMinutes(-10);

            lock(_SyncLock) {
                var deleteList = _IcaoSourceMap.Where(r => r.Value.LastMessageUtc <= threshold).Select(r => r.Key).ToArray();
                foreach(var oldIcao in deleteList) {
                    _IcaoSourceMap.Remove(oldIcao);
                }
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when a listener raises a BaseStation message event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_Port30003MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            try {
                var listener = (IListener)sender;
                var hasNoPosition = args.Message.Latitude.GetValueOrDefault() == 0.0 && args.Message.Longitude.GetValueOrDefault() == 0.0;
                if(FilterMessageFromListener(_Clock.UtcNow, listener, args.Message.Icao24, !hasNoPosition)) {
                    OnPort30003MessageReceived(args);
                    ++TotalMessages;
                }
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Called when a listener raises a position reset event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_PositionReset(object sender, EventArgs<string> args)
        {
            try {
                var listener = (IListener)sender;
                if(FilterMessageFromListener(_Clock.UtcNow, listener, args.Value, false)) OnPositionReset(args);
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Called on a background thread when the heartbeat service does its slow tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            CleanupOldIcaos();
        }
        #endregion
    }
}
