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
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

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
        /// The read buffer.
        /// </summary>
        private byte[] _Buffer = new byte[2048];

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
        /// The object that can turn <see cref="AircraftListJson"/> objects into lists of <see cref="BaseStationMessage"/> objects.
        /// </summary>
        private IAircraftListJsonMessageConverter _AircraftListJsonMessageConverter;

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
        /// Set to the time of the last message from the receiver.
        /// </summary>
        private DateTime _LastMessageUtc;

        /// <summary>
        /// Set to true if the listener has been, or is in the process of being, disposed.
        /// </summary>
        private bool _Disposed;

        /// <summary>
        /// Deserialisation settings for aircraft list JSON.
        /// </summary>
        private static JsonSerializerSettings _AircraftListJsonSerialiserSettings = new JsonSerializerSettings() {
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };
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
        public bool IsSatcomFeed { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnector Connector { get; private set; }

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
            EventHelper.Raise(ExceptionCaught, this, args);
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
            EventHelper.RaiseQuickly(RawBytesReceived, this, args);
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
            EventHelper.RaiseQuickly(ModeSBytesReceived, this, args);
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
            EventHelper.RaiseQuickly(Port30003MessageReceived, this, () => {
                args.Message.ReceiverId = ReceiverId;
                return args;
            });
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
            EventHelper.RaiseQuickly(ModeSMessageReceived, this, args);
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
            EventHelper.Raise(ConnectionStateChanged, this, args);
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
            EventHelper.Raise(SourceChanged, this, args);
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
            EventHelper.Raise(PositionReset, this, args);
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
            _AircraftListJsonMessageConverter = Factory.Singleton.Resolve<IAircraftListJsonMessageConverter>();

            _ModeSMessageTranslator.Statistics = Statistics;
            _AdsbMessageTranslator.Statistics = Statistics;

            var messageQueueName = String.Format("MessageProcessingAndDispatchQueue_{0}", ++_ListenerCounter);
            _MessageProcessingAndDispatchQueue = new BackgroundThreadQueue<MessageDispatch>(messageQueueName, 200000);
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
                    if(Connector != null) {
                        Connector.CloseConnection();
                        UnhookConnector();
                        Connector.Dispose();
                    }
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
        /// <param name="connector"></param>
        /// <param name="bytesExtractor"></param>
        /// <param name="rawMessageTranslator"></param>
        public void ChangeSource(IConnector connector, IMessageBytesExtractor bytesExtractor, IRawMessageTranslator rawMessageTranslator)
        {
            var changed = false;
            var reconnect = false;

            lock(_SyncLock) {
                reconnect = Connector != null && Connector.EstablishingConnections;
                if(connector != Connector || bytesExtractor != BytesExtractor || rawMessageTranslator != RawMessageTranslator) {
                    if(RawMessageTranslator != null && RawMessageTranslator != rawMessageTranslator) RawMessageTranslator.Dispose();

                    if(Connector != connector) {
                        if(Connector != null) {
                            UnhookConnector();
                            Connector.Dispose();
                        }
                        Connector = connector;
                        Connector.IsSingleConnection = true;
                        HookConnector();
                    }

                    BytesExtractor = bytesExtractor;
                    RawMessageTranslator = rawMessageTranslator;
                    if(RawMessageTranslator != null) RawMessageTranslator.Statistics = Statistics;

                    TotalMessages = 0;
                    TotalBadMessages = 0;

                    changed = true;
                }
            }

            if(changed) {
                OnSourceChanged(EventArgs.Empty);
                if(reconnect) Connect();
            }
        }

        private void HookConnector()
        {
            if(Connector != null) {
                Connector.ConnectionEstablished += Connector_ConnectionEstablished;
                Connector.ConnectionStateChanged += Connector_ConnectionStateChanged;
                Connector.ConnectionClosed += Connector_ConnectionClosed;
            }
        }

        private void UnhookConnector()
        {
            if(Connector != null) {
                Connector.ConnectionEstablished -= Connector_ConnectionEstablished;
                Connector.ConnectionStateChanged -= Connector_ConnectionStateChanged;
                Connector.ConnectionClosed -= Connector_ConnectionClosed;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            if(Connector == null || BytesExtractor == null || RawMessageTranslator == null) throw new InvalidOperationException("Cannot call Connect before ChangeSource has been used to set Connector, BytesExtractor and RawMessageTranslator");

            if(!_Disposed) {
                try {
                    SetConnectionStatus(ConnectionStatus.Connecting);
                    Connector.EstablishConnection();
                } catch(Exception ex) {
                    Disconnect();
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                }
            }
        }

        /// <summary>
        /// Called after the connector has established a connection to a source of data.
        /// </summary>
        private void Connected()
        {
            if(Statistics != null) Statistics.Lock(r => r.ConnectionTimeUtc = _Clock.UtcNow);
            SetConnectionStatus(ConnectionStatus.Connected);
            Connector.Read(_Buffer, BytesReceived);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            Statistics.ResetConnectionStatistics();
            Connector.CloseConnection();
            SetConnectionStatus(ConnectionStatus.Disconnected);
        }

        /// <summary>
        /// Attempts to reconnect to the data source when the connection is lost after it has been successfully established.
        /// </summary>
        private void Reconnect()
        {
            if(!_Disposed) {
                Statistics.ResetConnectionStatistics();
                Connector.RestartConnection();
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

        #region BytesReceived, Process****MessageBytes
        /// <summary>
        /// Called every time the connection sends some bytes to us.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="bytesRead"></param>
        private void BytesReceived(IConnection connection, byte[] buffer, int offset, int length, int bytesRead)
        {
            try {
                var now = _Clock.UtcNow;

                if(bytesRead > 0) {
                    if(Statistics != null) Statistics.Lock(r => r.BytesReceived += bytesRead);
                    _LastMessageUtc = now;

                    // This is a bit of a cheat - I don't want the overhead of taking a copy of the read buffer if nothing is
                    // listening to RawBytesReceived, so I take a peek at the event handler before creating the event args...
                    if(RawBytesReceived != null) {
                        var copyRawBytes = new byte[bytesRead];
                        Array.Copy(buffer, offset, copyRawBytes, 0, bytesRead);
                        _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { RawBytesEventArgs = new EventArgs<byte[]>(copyRawBytes) });
                    }

                    foreach(var extractedBytes in BytesExtractor.ExtractMessageBytes(buffer, offset, bytesRead)) {
                        if(extractedBytes.ChecksumFailed) {
                            ++TotalBadMessages;
                            if(Statistics != null) Statistics.Lock(r => ++r.FailedChecksumMessages);
                        } else {
                            // Another cheat and for the same reason as explained for the RawBytesReceived message - we don't want to
                            // incur the overhead of copying the extracted bytes if there is nothing listening to the event.
                            if(ModeSBytesReceived != null && extractedBytes.Format == ExtractedBytesFormat.ModeS) {
                                _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() {
                                    ModeSBytesEventArgs = new EventArgs<ExtractedBytes>((ExtractedBytes)extractedBytes.Clone())
                                });
                            }

                            switch(extractedBytes.Format) {
                                case ExtractedBytesFormat.Port30003:        ProcessPort30003MessageBytes(extractedBytes); break;
                                case ExtractedBytesFormat.ModeS:            ProcessModeSMessageBytes(now, extractedBytes); break;
                                case ExtractedBytesFormat.Compressed:       ProcessCompressedMessageBytes(now, extractedBytes); break;
                                case ExtractedBytesFormat.AircraftListJson: ProcessAircraftListJsonMessageBytes(now, extractedBytes); break;
                                default:                                    throw new NotImplementedException();
                            }
                        }
                    }

                    if(Statistics != null) Statistics.Lock(r => r.CurrentBufferSize = BytesExtractor.BufferSize);
                }

                Connector.Read(_Buffer, BytesReceived);
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
                    if(Statistics != null) Statistics.Lock(r => ++r.BaseStationMessagesReceived);
                    _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() {
                        Port30003MessageEventArgs = new BaseStationMessageEventArgs(translatedMessage, isOutOfBand: false, isSatcomFeed: IsSatcomFeed)
                    });
                }
            } catch(Exception) {
                ++TotalBadMessages;
                if(Statistics != null) Statistics.Lock(r => ++r.BaseStationBadFormatMessagesReceived);
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
                    if(Statistics != null) Statistics.Lock(r => ++r.BaseStationMessagesReceived);
                    _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { Port30003MessageEventArgs = new BaseStationMessageEventArgs(message) });
                }
            } catch(Exception) {
                ++TotalBadMessages;
                if(Statistics != null) Statistics.Lock(r => ++r.BaseStationBadFormatMessagesReceived);
                if(!IgnoreBadMessages) throw;
            }
        }

        /// <summary>
        /// Translates the bytes for an <see cref="AircraftListJson"/> into a list of cooked messages and raises the
        /// appropriate events.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="extractedBytes"></param>
        private void ProcessAircraftListJsonMessageBytes(DateTime now, ExtractedBytes extractedBytes)
        {
            try {
                var jsonText = Encoding.UTF8.GetString(extractedBytes.Bytes, extractedBytes.Offset, extractedBytes.Length);
                var json = JsonConvert.DeserializeObject<AircraftListJson>(jsonText, _AircraftListJsonSerialiserSettings);
                var totalMessages = 0;
                foreach(var message in _AircraftListJsonMessageConverter.ConvertIntoBaseStationMessages(json)) {
                    ++totalMessages;
                    _MessageProcessingAndDispatchQueue.Enqueue(new MessageDispatch() { Port30003MessageEventArgs = new BaseStationMessageEventArgs(message) });
                }

                if(totalMessages != 0) {
                    TotalMessages += totalMessages;
                    if(Statistics != null) Statistics.Lock(r => r.BaseStationMessagesReceived += totalMessages);
                }
            } catch(Exception) {
                ++TotalBadMessages;
                if(Statistics != null) Statistics.Lock(r => ++r.BaseStationBadFormatMessagesReceived);
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
                    var modeSMessage = _ModeSMessageTranslator.Translate(extractedBytes.Bytes, extractedBytes.Offset, extractedBytes.SignalLevel, extractedBytes.IsMlat);
                    if(modeSMessage != null) {
                        bool hasPIField = modeSMessage.ParityInterrogatorIdentifier != null;
                        bool isPIWithBadParity = hasPIField && modeSMessage.ParityInterrogatorIdentifier != 0;
                        var adsbMessage = _AdsbMessageTranslator.Translate(modeSMessage);

                        if((hasPIField || isPIWithBadParity || adsbMessage == null) && Statistics != null) {
                            Statistics.Lock(r => {
                                if(hasPIField) ++r.ModeSWithPIField;
                                if(isPIWithBadParity) ++r.ModeSWithBadParityPIField;
                                if(adsbMessage == null) ++r.ModeSNotAdsbCount;
                            });
                        }

                        if(adsbMessage != null && modeSMessage.DownlinkFormat == DownlinkFormat.ExtendedSquitterNonTransponder) {
                            if(adsbMessage.TisbIcaoModeAFlag == 0) {
                                switch(modeSMessage.ControlField) {
                                    case ControlField.CoarseFormatTisb:
                                    case ControlField.FineFormatTisb:
                                        modeSMessage.Icao24 = modeSMessage.NonIcao24Address.GetValueOrDefault();
                                        modeSMessage.NonIcao24Address = null;
                                        break;
                                }
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

        #region Other subscribed events
        /// <summary>
        /// Called when the connection is established.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connector_ConnectionEstablished(object sender, ConnectionEventArgs e)
        {
            Connected();
        }

        /// <summary>
        /// Called when the connection is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connector_ConnectionClosed(object sender, ConnectionEventArgs e)
        {
            ;
        }

        /// <summary>
        /// Called when the connection status changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connector_ConnectionStateChanged(object sender, EventArgs e)
        {
            if(Connector != null) ConnectionStatus = Connector.ConnectionStatus;
        }

        /// <summary>
        /// Called when the statistics wants a list of every exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Statistics_ConnectorExceptionsRequired(object sender, EventArgs<List<TimestampedException>> args)
        {
            if(Connector != null) args.Value.AddRange(Connector.GetExceptionHistory());
        }
        #endregion
    }
}
