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
using System.Net.Sockets;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IListener"/>.
    /// </summary>
    sealed class Listener : IListener
    {
        #region Private class - MessageDispatch
        /// <summary>
        /// The class that carries information to the background message processing and dispatch queue.
        /// </summary>
        /// <remarks>
        /// All fields are mutually exclusive.
        /// </remarks>
        class MessageDispatch
        {
            /// <summary>
            /// The raw bytes received from the transmitter.
            /// </summary>
            public EventArgs<byte[]> RawBytesEventArgs;

            /// <summary>
            /// The raw Mode-S message bytes received from the transmitter.
            /// </summary>
            public EventArgs<ExtractedBytes> ModeSBytesEventArgs;

            /// <summary>
            /// The Port30003 message event args.
            /// </summary>
            public BaseStationMessageEventArgs Port30003MessageEventArgs;

            /// <summary>
            /// The ModeS message event args.
            /// </summary>
            public ModeSMessageEventArgs ModeSMessageEventArgs;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The object that can translate port 30003 messages for us.
        /// </summary>
        private IBaseStationMessageTranslator _Port30003MessageTranslator;

        /// <summary>
        /// The object that can translate Mode-S messages for us.
        /// </summary>
        private IModeSTranslator _ModeSMessageTranslator;

        /// <summary>
        /// The object that can translate ADS-B messages for us.
        /// </summary>
        private IAdsbTranslator _AdsbMessageTranslator;

        /// <summary>
        /// The object that can strip Mode-S parity bits for us.
        /// </summary>
        private IModeSParity _ModeSParity;

        /// <summary>
        /// The object that can decompress messages for us.
        /// </summary>
        private IBaseStationMessageCompressor _Compressor;

        /// <summary>
        /// The background thread that will dispatch Port30003 messages for us.
        /// </summary>
        private BackgroundThreadQueue<MessageDispatch> _MessageProcessingAndDispatchQueue;

        /// <summary>
        /// The number of listeners that have been created. We want to ensure that we can have as many listeners as we like
        /// so this counter is appended to the name of the queue thread to avoid clashes with other listeners.
        /// </summary>
        private static int _ListenerCounter;

        /// <summary>
        /// The object that locks access to values that can be changed by <see cref="ChangeSource"/> to prevent their use while the
        /// configuration is being updated.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// True if the fast heartbeat timer has been hooked.
        /// </summary>
        private bool _HookedFastHeartbeat;

        /// <summary>
        /// A copy of the invariant CoarseTimeout setting from <see cref="IConfigurationStorage"/>.
        /// </summary>
        private int _CoarseTimeout = -1;

        /// <summary>
        /// Set to the time of the last message from the receiver.
        /// </summary>
        private DateTime _LastMessageUtc;

        /// <summary>
        /// Set to true if the listener has been, or is in the process of being, disposed.
        /// </summary>
        private bool _Disposed;

        /// <summary>
        /// The log that connect / disconnect messages will be written to.
        /// </summary>
        private ILog _Log;
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
        public ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TotalBadMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreBadMessages { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            if(ExceptionCaught != null) ExceptionCaught(this, args);
        }

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
        /// Raises <see cref="Port30003MessageReceived"/>
        /// </summary>
        /// <param name="args"></param>
        private void OnPort30003MessageReceived(BaseStationMessageEventArgs args)
        {
            if(Port30003MessageReceived != null) {
                args.Message.ReceiverId = ReceiverId;
                Port30003MessageReceived(this, args);
            }
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
        /// See interface docs.
        /// </summary>
        public event EventHandler SourceChanged;

        /// <summary>
        /// Raises <see cref="SourceChanged"/>.
        /// </summary>
        /// <param name="args"></param>
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
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Listener()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();
            Statistics = Factory.Singleton.Resolve<IStatistics>();
            Statistics.Initialise();

            _Port30003MessageTranslator = Factory.Singleton.Resolve<IBaseStationMessageTranslator>();
            _ModeSMessageTranslator = Factory.Singleton.Resolve<IModeSTranslator>();
            _AdsbMessageTranslator = Factory.Singleton.Resolve<IAdsbTranslator>();
            _ModeSParity = Factory.Singleton.Resolve<IModeSParity>();
            _Compressor = Factory.Singleton.Resolve<IBaseStationMessageCompressor>();

            _ModeSMessageTranslator.Statistics = Statistics;
            _AdsbMessageTranslator.Statistics = Statistics;

            var messageQueueName = String.Format("MessageProcessingAndDispatchQueue_{0}", ++_ListenerCounter);
            _MessageProcessingAndDispatchQueue = new BackgroundThreadQueue<MessageDispatch>(messageQueueName);
            _MessageProcessingAndDispatchQueue.StartBackgroundThread(ProcessAndDispatchMessageQueueItem, HandleMessageDispatchException);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Listener()
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
            _Disposed = true;
            if(disposing) {
                lock(_SyncLock) {
                    if(_HookedFastHeartbeat) {
                        _HookedFastHeartbeat = false;
                        Factory.Singleton.Resolve<IHeartbeatService>().Singleton.FastTick -= HeartbeatService_FastTick;
                    }
                    if(Provider != null) Provider.Close();
                    if(_MessageProcessingAndDispatchQueue != null) {
                        _MessageProcessingAndDispatchQueue.Dispose();
                    }
                    if(RawMessageTranslator != null) RawMessageTranslator.Dispose();
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
            lock(_SyncLock) {
                bool changed = false;

                if(_Log == null) _Log = Factory.Singleton.Resolve<ILog>().Singleton;

                if(_CoarseTimeout <= 0) _CoarseTimeout = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.CoarseListenerTimeout;
                if(_CoarseTimeout > 0 && !_HookedFastHeartbeat) {
                    _HookedFastHeartbeat = true;
                    Factory.Singleton.Resolve<IHeartbeatService>().Singleton.FastTick += HeartbeatService_FastTick;
                }

                if(provider != Provider || bytesExtractor != BytesExtractor || rawMessageTranslator != RawMessageTranslator) {
                    if(Provider != null) Disconnect();
                    if(RawMessageTranslator != null && RawMessageTranslator != rawMessageTranslator) RawMessageTranslator.Dispose();

                    Provider = provider;
                    BytesExtractor = bytesExtractor;
                    RawMessageTranslator = rawMessageTranslator;
                    if(RawMessageTranslator != null) RawMessageTranslator.Statistics = Statistics;

                    TotalMessages = 0;
                    TotalBadMessages = 0;

                    changed = true;
                }

                if(changed) {
                    OnSourceChanged(EventArgs.Empty);
                    if(autoReconnect) Connect(false);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="autoReconnect"></param>
        public void Connect(bool autoReconnect)
        {
            if(Provider == null || BytesExtractor == null || RawMessageTranslator == null) throw new InvalidOperationException("Cannot call Connect before ChangeSource has been used to set Provider, BytesExtractor and RawMessageTranslator");

            if(!_Disposed) {
                try {
                    SetConnectionStatus(autoReconnect ? ConnectionStatus.Reconnecting : ConnectionStatus.Connecting);
                    if(_Log != null) _Log.WriteLine("Connecting to {0}", ReceiverName);
                    Provider.BeginConnect(Connected);
                } catch(Exception ex) {
                    Disconnect();
                    if(_Log != null) _Log.WriteLine("Failed to connect to {0}, exception {1}", ReceiverName, ex.Message);
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                }
            }
        }

        /// <summary>
        /// Called after the provider has connected to the source of data.
        /// </summary>
        /// <param name="ar"></param>
        private void Connected(IAsyncResult ar)
        {
            try {
                if(Provider.EndConnect(ar)) {
                    if(Statistics.Lock != null) { lock(Statistics.Lock) Statistics.ConnectionTimeUtc = _Clock.UtcNow; }
                    SetConnectionStatus(ConnectionStatus.Connected);
                    if(_Log != null) _Log.WriteLine("Connected to {0}", ReceiverName);
                    Provider.BeginRead(BytesReceived);
                }
            } catch(SocketException ex) {
                switch(ConnectionStatus) {
                    case ConnectionStatus.Connecting:
                        SetConnectionStatus(ConnectionStatus.CannotConnect);
                        if(_Log != null) _Log.WriteLine("Failed to connect to {0}, socket exception {1}. Cannot connect.", ReceiverName, ex.Message);
                        break;
                    case ConnectionStatus.Reconnecting:
                        Reconnect();
                        break;
                    default:
                        OnExceptionCaught(new EventArgs<Exception>(ex)); break;
                }
            } catch(ObjectDisposedException) {
                // These are just exception spam, we'll get these if the connection is disposed of while the read is running on another thread
            } catch(InvalidOperationException) {
                // More exception spam, this happens if the connection is closed but not (yet) disposed of while the read is running
            } catch(Exception ex) {
                Disconnect();
                if(_Log != null) _Log.WriteLine("Failed to connect to {0}, unexpected exception {1}", ReceiverName, ex.Message);
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            Statistics.ResetConnectionStatistics();
            Provider.Close();
            SetConnectionStatus(ConnectionStatus.Disconnected);
            if(_Log != null) _Log.WriteLine("Disconnected from {0}", ReceiverName);
        }

        /// <summary>
        /// Attempts to reconnect to the data source when the connection is lost after it has been successfully established.
        /// </summary>
        private void Reconnect()
        {
            if(!_Disposed) {
                try {
                    Statistics.ResetConnectionStatistics();
                    SetConnectionStatus(ConnectionStatus.Reconnecting);
                    Provider.Sleep(1000);
                    if(ConnectionStatus == ConnectionStatus.Reconnecting) {
                        Provider.BeginConnect(Connected);
                    }
                } catch(Exception ex) {
                    Disconnect();
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                }
            }
        }

        /// <summary>
        /// Sets the connection status and then raises an event to let subscribers know.
        /// </summary>
        /// <param name="connectionStatus"></param>
        private void SetConnectionStatus(ConnectionStatus connectionStatus)
        {
            if(connectionStatus == ConnectionStatus.Connected) _LastMessageUtc = _Clock.UtcNow;
            ConnectionStatus = connectionStatus;
            OnConnectionStateChanged(EventArgs.Empty);
        }
        #endregion

        #region BytesReceived, ProcessPort30003MessageBytes, ProcessModeSMessageBytes
        /// <summary>
        /// Called every time the provider sends some bytes to us.
        /// </summary>
        /// <param name="ar"></param>
        private void BytesReceived(IAsyncResult ar)
        {
            try {
                var now = _Clock.UtcNow;

                int bytesRead = -1;
                bool fetchNext = true;
                try {
                    bytesRead = Provider.EndRead(ar);
                } catch(ObjectDisposedException) {
                    fetchNext = false;
                } catch(SocketException ex) {
                    fetchNext = false;
                    if(_Log != null) _Log.WriteLine("Caught exception ({0}) reading from {1}, bytes read {2}", ex.Message, ReceiverName, bytesRead);
                    Reconnect();
                }

                if(bytesRead == 0 || _Disposed) {
                    fetchNext = false;
                    if(!_Disposed) {
                        if(_Log != null) _Log.WriteLine("Reconnecting {0}", ReceiverName);
                        Reconnect();
                    }
                } else if(bytesRead > 0) {
                    if(Statistics.Lock != null) { lock(Statistics.Lock) Statistics.BytesReceived += bytesRead; }
                    _LastMessageUtc = now;

                    // This is a bit of a cheat - I don't want the overhead of taking a copy of the read buffer if nothing is
                    // listening to RawBytesReceived, so I take a peek at the event handler before creating the event args...
                    if(RawBytesReceived != null) {
                        var copyRawBytes = new byte[bytesRead];
                        Array.Copy(Provider.ReadBuffer, 0, copyRawBytes, 0, bytesRead);
                        _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { RawBytesEventArgs = new EventArgs<byte[]>(copyRawBytes) });
                    }

                    foreach(var extractedBytes in BytesExtractor.ExtractMessageBytes(Provider.ReadBuffer, 0, bytesRead)) {
                        if(extractedBytes.ChecksumFailed) {
                            ++TotalBadMessages;
                            if(Statistics.Lock != null) { lock(Statistics.Lock) ++Statistics.FailedChecksumMessages; }
                        } else {
                            // Another cheat and for the same reason as explained for the RawBytesReceived message - we don't want to
                            // incur the overhead of copying the extracted bytes if there is nothing listening to the event.
                            if(ModeSBytesReceived != null && extractedBytes.Format == ExtractedBytesFormat.ModeS) {
                                _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { ModeSBytesEventArgs = new EventArgs<ExtractedBytes>((ExtractedBytes)extractedBytes.Clone()) });
                            }

                            switch(extractedBytes.Format) {
                                case ExtractedBytesFormat.Port30003:    ProcessPort30003MessageBytes(extractedBytes); break;
                                case ExtractedBytesFormat.ModeS:        ProcessModeSMessageBytes(now, extractedBytes); break;
                                case ExtractedBytesFormat.Compressed:   ProcessCompressedMessageBytes(now, extractedBytes); break;
                                default:                                throw new NotImplementedException();
                            }
                        }
                    }
                }

                if(fetchNext) {
                    try {
                        Provider.BeginRead(BytesReceived);
                    } catch(SocketException ex) {
                        if(_Log != null) _Log.WriteLine("Caught exception ({0}) starting next read from {1}", ex.Message, ReceiverName);
                        Reconnect();
                    }
                }
            } catch(Exception ex) {
                Disconnect();
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Translates the bytes for a Port30003 message into a cooked message object and causes a message received event to be raised on the background thread.
        /// </summary>
        /// <param name="extractedBytes"></param>
        private void ProcessPort30003MessageBytes(ExtractedBytes extractedBytes)
        {
            try {
                var port30003Message = Encoding.ASCII.GetString(extractedBytes.Bytes, extractedBytes.Offset, extractedBytes.Length);
                var translatedMessage = _Port30003MessageTranslator.Translate(port30003Message, extractedBytes.SignalLevel);
                if(translatedMessage != null) {
                    ++TotalMessages;
                    if(Statistics.Lock != null) { lock(Statistics.Lock) ++Statistics.BaseStationMessagesReceived; }
                    _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { Port30003MessageEventArgs = new BaseStationMessageEventArgs(translatedMessage) });
                }
            } catch(Exception) {
                ++TotalBadMessages;
                if(Statistics.Lock != null) { lock(Statistics.Lock) ++Statistics.BaseStationBadFormatMessagesReceived; }
                if(!IgnoreBadMessages) throw;
            }
        }

        /// <summary>
        /// Translates the bytes for a compressed message into a cooked message object and raises the appropriate events.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="extractedBytes"></param>
        private void ProcessCompressedMessageBytes(DateTime now, ExtractedBytes extractedBytes)
        {
            try {
                var bytes = extractedBytes.Bytes;
                if(extractedBytes.Offset != 0) {
                    bytes = new byte[extractedBytes.Length];
                    Array.ConstrainedCopy(extractedBytes.Bytes, extractedBytes.Offset, bytes, 0, extractedBytes.Length);
                }

                var message = _Compressor.Decompress(bytes);
                if(message != null) {
                    ++TotalMessages;
                    if(Statistics.Lock != null) { lock(Statistics.Lock) ++Statistics.BaseStationMessagesReceived; }
                    _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { Port30003MessageEventArgs = new BaseStationMessageEventArgs(message) });
                }
            } catch(Exception) {
                ++TotalBadMessages;
                if(Statistics.Lock != null) { lock(Statistics.Lock) ++Statistics.BaseStationBadFormatMessagesReceived; }
                if(!IgnoreBadMessages) throw;
            }
        }

        /// <summary>
        /// Translates the bytes for a Mode-S message into a cooked message object and causes a message received event to be raised on the background thread.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="extractedBytes"></param>
        private void ProcessModeSMessageBytes(DateTime now, ExtractedBytes extractedBytes)
        {
            if(extractedBytes.Length == 7 || extractedBytes.Length == 14) {
                if(extractedBytes.HasParity) _ModeSParity.StripParity(extractedBytes.Bytes, extractedBytes.Offset, extractedBytes.Length);

                try {
                    var modeSMessage = _ModeSMessageTranslator.Translate(extractedBytes.Bytes, extractedBytes.Offset, extractedBytes.SignalLevel);
                    if(modeSMessage != null) {
                        bool hasPIField = modeSMessage.ParityInterrogatorIdentifier != null;
                        bool isPIWithBadParity = hasPIField && modeSMessage.ParityInterrogatorIdentifier != 0;
                        var adsbMessage = _AdsbMessageTranslator.Translate(modeSMessage);

                        if((hasPIField || isPIWithBadParity || adsbMessage == null) && Statistics.Lock != null) {
                            lock(Statistics.Lock) {
                                if(hasPIField) ++Statistics.ModeSWithPIField;
                                if(isPIWithBadParity) ++Statistics.ModeSWithBadParityPIField;
                                if(adsbMessage == null) ++Statistics.ModeSNotAdsbCount;
                            }
                        }

                        ++TotalMessages;
                        _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { ModeSMessageEventArgs = new ModeSMessageEventArgs(now, modeSMessage, adsbMessage) });
                    }
                } catch(Exception) {
                    ++TotalBadMessages;
                    if(!IgnoreBadMessages) throw;
                }
            }
        }
        #endregion

        #region ProcessAndDispatchMessageQueueItem, HandleMessageDispatchException
        /// <summary>
        /// Called on a background thread to raise an event indicating that a message has been picked up.
        /// </summary>
        /// <param name="message"></param>
        private void ProcessAndDispatchMessageQueueItem(MessageDispatch message)
        {
            if(message.Port30003MessageEventArgs != null) {
                OnPort30003MessageReceived(message.Port30003MessageEventArgs);
            } else if(message.RawBytesEventArgs != null) {
                OnRawBytesReceived(message.RawBytesEventArgs);
            } else if(message.ModeSBytesEventArgs != null) {
                OnModeSBytesReceived(message.ModeSBytesEventArgs);
            } else {
                var modeSMessageArgs = message.ModeSMessageEventArgs;
                OnModeSMessageReceived(modeSMessageArgs);

                BaseStationMessage cookedMessage;
                lock(_SyncLock) {
                    cookedMessage = RawMessageTranslator == null ? null : RawMessageTranslator.Translate(modeSMessageArgs.ReceivedUtc, modeSMessageArgs.ModeSMessage, modeSMessageArgs.AdsbMessage);
                }
                if(cookedMessage != null) OnPort30003MessageReceived(new BaseStationMessageEventArgs(cookedMessage));
            }
        }

        /// <summary>
        /// Called when an unhandled exception bubbles up out of <see cref="ProcessAndDispatchMessageQueueItem"/>.
        /// </summary>
        /// <param name="ex"></param>
        private void HandleMessageDispatchException(Exception ex)
        {
            Disconnect();
            OnExceptionCaught(new EventArgs<Exception>(ex));
        }
        #endregion

        #region CheckCoarseListenerTimeout
        /// <summary>
        /// Checks to see whether the coarse listener timeout has expired and, if it has, force a reconnect.
        /// </summary>
        private void CheckCoarseListenerTimeout()
        {
            if(_CoarseTimeout > 0 && ConnectionStatus == ConnectionStatus.Connected) {
                var threshold = _Clock.UtcNow.AddSeconds(-_CoarseTimeout);
                if(_LastMessageUtc < threshold) {
                    if(_Log != null) _Log.WriteLine("Receiver {0} has had no messages for {1} seconds", ReceiverName, _CoarseTimeout);
                    Disconnect();
                    Reconnect();
                }
            }
        }
        #endregion

        #region Other subscribed events
        /// <summary>
        /// Called when the fast tick occurs on the heartbeat service. This is called on a background thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_FastTick(object sender, EventArgs args)
        {
            CheckCoarseListenerTimeout();
        }
        #endregion
    }
}
