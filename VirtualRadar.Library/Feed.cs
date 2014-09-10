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
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IFeed"/>.
    /// </summary>
    class Feed : IFeed
    {
        #region Fields
        private bool _Initialised;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int UniqueId { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationAircraftList AircraftList { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IListener Listener { get; private set; }
        #endregion

        #region Constructors and finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Feed()
        {
            Dispose(false);
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            if(ExceptionCaught != null) ExceptionCaught(this, args);
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
                if(AircraftList != null) {
                    AircraftList.ExceptionCaught -= AircraftList_ExceptionCaught;
                    AircraftList.Dispose();
                    AircraftList = null;
                }

                if(Listener != null) {
                    Listener.ExceptionCaught -= Listener_ExceptionCaught;
                    Listener.Dispose();
                    Listener = null;
                }
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="configuration"></param>
        public void Initialise(Receiver receiver, Configuration configuration)
        {
            if(_Initialised) throw new InvalidOperationException("A feed can only be initialised once");
            if(receiver == null) throw new ArgumentNullException("receiver");
            if(configuration == null) throw new ArgumentNullException("configuration");
            if(!receiver.Enabled) throw new InvalidOperationException(String.Format("The {0} receiver has not been enabled", receiver.Name));
            var receiverLocation = configuration.ReceiverLocation(receiver.ReceiverLocationId);

            Listener = Factory.Singleton.Resolve<IListener>();
            Listener.ExceptionCaught += Listener_ExceptionCaught;
            Listener.IgnoreBadMessages = true;
            ApplyReceiverListenerSettings(false, receiver, configuration, receiverLocation);

            DoCommonInitialise(receiver.UniqueId, receiver.Name);
            ConfigurePolarPlotter(receiverLocation);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergeFeeds"></param>
        public void Initialise(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            if(_Initialised) throw new InvalidOperationException("A feed can only be initialised once");
            if(mergedFeed == null) throw new ArgumentNullException("receiver");
            if(mergeFeeds == null) throw new ArgumentNullException("mergePathways");
            if(!mergedFeed.Enabled) throw new InvalidOperationException(String.Format("The {0} merged feed has not been enabled", mergedFeed.Name));

            var mergedListeners = GetListenersFromMergeFeeds(mergedFeed, mergeFeeds);

            var mergedFeedListener = Factory.Singleton.Resolve<IMergedFeedListener>();
            Listener = mergedFeedListener;
            Listener.ExceptionCaught += Listener_ExceptionCaught;
            Listener.IgnoreBadMessages = true;
            mergedFeedListener.IcaoTimeout = mergedFeed.IcaoTimeout;
            mergedFeedListener.IgnoreAircraftWithNoPosition = mergedFeed.IgnoreAircraftWithNoPosition;
            mergedFeedListener.SetListeners(mergedListeners);

            DoCommonInitialise(mergedFeed.UniqueId, mergedFeed.Name);
        }

        private static List<IListener> GetListenersFromMergeFeeds(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            var mergedListeners = mergeFeeds.Where(r => mergedFeed.ReceiverIds.Contains(r.UniqueId)).Select(r => r.Listener).ToList();
            return mergedListeners;
        }

        /// <summary>
        /// Performs initialisation common to all Initialise methods.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="name"></param>
        private void DoCommonInitialise(int uniqueId, string name)
        {
            _Initialised = true;
            UniqueId = uniqueId;
            Name = name;

            Listener.ReceiverId = uniqueId;
            Listener.ReceiverName = Name;

            AircraftList = Factory.Singleton.Resolve<IBaseStationAircraftList>();
            AircraftList.ExceptionCaught += AircraftList_ExceptionCaught;
            AircraftList.Listener = Listener;
            AircraftList.StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
            AircraftList.Start();
        }

        /// <summary>
        /// Applies receiver listener settings.
        /// </summary>
        /// <param name="reconnect"></param>
        /// <param name="receiver"></param>
        /// <param name="configuration"></param>
        /// <param name="receiverLocation"></param>
        private void ApplyReceiverListenerSettings(bool reconnect, Receiver receiver, Configuration configuration, ReceiverLocation receiverLocation)
        {
            var connector = DetermineConnector(receiver);
            var bytesExtractor = DetermineBytesExtractor(receiver);
            bool feedSourceHasChanged = connector != Listener.Connector || bytesExtractor != Listener.BytesExtractor;
            var rawMessageTranslator = DetermineRawMessageTranslator(receiver, receiverLocation, configuration, feedSourceHasChanged);

            if(feedSourceHasChanged) {
                Listener.ChangeSource(connector, bytesExtractor, rawMessageTranslator);
                if(Listener.Statistics != null) Listener.Statistics.ResetMessageCounters();
            }
        }
        #endregion

        #region ApplyConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="configuration"></param>
        public void ApplyConfiguration(Receiver receiver, Configuration configuration)
        {
            if(!_Initialised) throw new InvalidOperationException("ApplyConfiguration cannot be called on an uninitialised feed");
            if(receiver == null) throw new ArgumentNullException("receiver");
            if(configuration == null) throw new ArgumentNullException("configuration");
            if(receiver.UniqueId != UniqueId) throw new InvalidOperationException(String.Format("Cannot apply configuration for receiver #{0} to feed for receiver for #{1}", receiver.UniqueId, UniqueId));
            var receiverLocation = configuration.ReceiverLocation(receiver.ReceiverLocationId);

            var initialisedWithReceiver = (Listener as IMergedFeedListener) == null;
            if(!initialisedWithReceiver) throw new InvalidOperationException(String.Format("Feed {0} was initialised with a merged feed but updated with a receiver", UniqueId));

            Name = receiver.Name;
            ApplyReceiverListenerSettings(true, receiver, configuration, receiverLocation);

            ConfigurePolarPlotter(receiverLocation);
        }

        /// <summary>
        /// Configures the polar plotter.
        /// </summary>
        /// <param name="receiverLocation"></param>
        private void ConfigurePolarPlotter(ReceiverLocation receiverLocation)
        {
            if(receiverLocation == null) AircraftList.PolarPlotter = null;
            else {
                var existingPlotter = AircraftList.PolarPlotter;
                if(existingPlotter == null || existingPlotter.Latitude != receiverLocation.Latitude || existingPlotter.Longitude != receiverLocation.Longitude) {
                    var polarPlotter = existingPlotter ?? Factory.Singleton.Resolve<IPolarPlotter>();
                    polarPlotter.Initialise(receiverLocation.Latitude, receiverLocation.Longitude);
                    if(existingPlotter == null) AircraftList.PolarPlotter = polarPlotter;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergeFeeds"></param>
        public void ApplyConfiguration(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            if(!_Initialised) throw new InvalidOperationException("ApplyConfiguration cannot be called on an uninitialised feed");
            if(mergedFeed == null) throw new ArgumentNullException("mergedFeed");
            if(mergeFeeds == null) throw new ArgumentNullException("mergePathways");
            if(mergedFeed.UniqueId != UniqueId) throw new InvalidOperationException(String.Format("Cannot apply configuration for merged feed #{0} to feed for merged feed for #{1}", mergedFeed.UniqueId, UniqueId));

            var mergedFeedListener = Listener as IMergedFeedListener;
            if(mergedFeedListener == null) throw new InvalidOperationException(String.Format("ReceiverPathway {0} was initialised with a receiver but updated with a merged feed", UniqueId));

            Name = mergedFeed.Name;

            var listeners = GetListenersFromMergeFeeds(mergedFeed, mergeFeeds);
            mergedFeedListener.IcaoTimeout = mergedFeed.IcaoTimeout;
            mergedFeedListener.IgnoreAircraftWithNoPosition = mergedFeed.IgnoreAircraftWithNoPosition;
            mergedFeedListener.SetListeners(listeners);
        }
        #endregion

        #region DetermineConnector, DetermineBytesExtractor, DetermineRawMessageTranslator
        /// <summary>
        /// Creates and configures the provider to use to connect to the data feed.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private IConnector DetermineConnector(Receiver receiver)
        {
            IConnector result = Listener.Connector;

            switch(receiver.ConnectionType) {
                case ConnectionType.COM:
                    var existingSerialProvider = result as ISerialConnector;
                    if(existingSerialProvider == null || existingSerialProvider.BaudRate != receiver.BaudRate || existingSerialProvider.ComPort != receiver.ComPort ||
                       existingSerialProvider.DataBits != receiver.DataBits || existingSerialProvider.Handshake != receiver.Handshake || 
                       existingSerialProvider.Parity != receiver.Parity || existingSerialProvider.ShutdownText != receiver.ShutdownText ||
                       existingSerialProvider.StartupText != receiver.StartupText || existingSerialProvider.StopBits != receiver.StopBits) {
                        var serialConnector = Factory.Singleton.Resolve<ISerialConnector>();
                        serialConnector.Name =          receiver.Name;
                        serialConnector.BaudRate =      receiver.BaudRate;
                        serialConnector.ComPort =       receiver.ComPort;
                        serialConnector.DataBits =      receiver.DataBits;
                        serialConnector.Handshake =     receiver.Handshake;
                        serialConnector.Parity =        receiver.Parity;
                        serialConnector.ShutdownText =  receiver.ShutdownText;
                        serialConnector.StartupText =   receiver.StartupText;
                        serialConnector.StopBits =      receiver.StopBits;
                        result = serialConnector;
                    }
                    break;
                case ConnectionType.TCP:
                    var existingTcpProvider = result as INetworkConnector;
                    if(existingTcpProvider == null || existingTcpProvider.Address != receiver.Address || existingTcpProvider.Port != receiver.Port ||
                       existingTcpProvider.UseKeepAlive != receiver.UseKeepAlive || existingTcpProvider.IdleTimeout != receiver.IdleTimeoutMilliseconds ||
                       existingTcpProvider.IsPassive != receiver.IsPassive ||
                       (receiver.IsPassive && !Object.Equals(existingTcpProvider.Access, receiver.Access))) {
                        var ipActiveConnector = Factory.Singleton.Resolve<INetworkConnector>();
                        ipActiveConnector.Name =            receiver.Name;
                        ipActiveConnector.IsPassive =       receiver.IsPassive;
                        ipActiveConnector.Port =            receiver.Port;
                        ipActiveConnector.UseKeepAlive =    receiver.UseKeepAlive;
                        ipActiveConnector.IdleTimeout =     receiver.IdleTimeoutMilliseconds;

                        if(!receiver.IsPassive) {
                            ipActiveConnector.Address =     receiver.Address;
                        } else {
                            ipActiveConnector.Access =      receiver.Access;
                        }

                        result = ipActiveConnector;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Creates and configures the message bytes extractor to use to get the message bytes out of the feed.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private IMessageBytesExtractor DetermineBytesExtractor(Receiver receiver)
        {
            IMessageBytesExtractor result = Listener.BytesExtractor;

            switch(receiver.DataSource) {
                case DataSource.Beast:                  if(result == null || !(result is IBeastMessageBytesExtractor)) result = Factory.Singleton.Resolve<IBeastMessageBytesExtractor>(); break;
                case DataSource.Port30003:              if(result == null || !(result is IPort30003MessageBytesExtractor)) result = Factory.Singleton.Resolve<IPort30003MessageBytesExtractor>(); break;
                case DataSource.Sbs3:                   if(result == null || !(result is ISbs3MessageBytesExtractor)) result = Factory.Singleton.Resolve<ISbs3MessageBytesExtractor>(); break;
                case DataSource.CompressedVRS:    if(result == null || !(result is ICompressedMessageBytesExtractor)) result = Factory.Singleton.Resolve<ICompressedMessageBytesExtractor>(); break;
                default:                                throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Creates and configures the object to translate Mode-S messages into BaseStation messages.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="receiverLocation"></param>
        /// <param name="config"></param>
        /// <param name="isNewSource"></param>
        /// <returns></returns>
        private IRawMessageTranslator DetermineRawMessageTranslator(Receiver receiver, ReceiverLocation receiverLocation, Configuration config, bool isNewSource)
        {
            var result = isNewSource || Listener.RawMessageTranslator == null ? Factory.Singleton.Resolve<IRawMessageTranslator>() : Listener.RawMessageTranslator;

            // There's every chance that the translator is in use while we're changing the properties here. In practise I
            // don't think it's going to make a huge difference, and people won't be changing this stuff very often anyway,
            // but I might have to add some shared locking code here. I'd like to avoid it if I can though.

            result.ReceiverLocation = receiverLocation == null ? null : new GlobalCoordinate(receiverLocation.Latitude, receiverLocation.Longitude);

            result.AcceptIcaoInNonPICount                       = config.RawDecodingSettings.AcceptIcaoInNonPICount;
            result.AcceptIcaoInNonPIMilliseconds                = config.RawDecodingSettings.AcceptIcaoInNonPISeconds * 1000;
            result.AcceptIcaoInPI0Count                         = config.RawDecodingSettings.AcceptIcaoInPI0Count;
            result.AcceptIcaoInPI0Milliseconds                  = config.RawDecodingSettings.AcceptIcaoInPI0Seconds * 1000;
            result.GlobalDecodeAirborneThresholdMilliseconds    = config.RawDecodingSettings.AirborneGlobalPositionLimit * 1000;
            result.GlobalDecodeFastSurfaceThresholdMilliseconds = config.RawDecodingSettings.FastSurfaceGlobalPositionLimit * 1000;
            result.GlobalDecodeSlowSurfaceThresholdMilliseconds = config.RawDecodingSettings.SlowSurfaceGlobalPositionLimit * 1000;
            result.IgnoreMilitaryExtendedSquitter               = config.RawDecodingSettings.IgnoreMilitaryExtendedSquitter;
            result.LocalDecodeMaxSpeedAirborne                  = config.RawDecodingSettings.AcceptableAirborneSpeed;
            result.LocalDecodeMaxSpeedSurface                   = config.RawDecodingSettings.AcceptableSurfaceSpeed;
            result.LocalDecodeMaxSpeedTransition                = config.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed;
            result.ReceiverRangeKilometres                      = config.RawDecodingSettings.ReceiverRange;
            result.SuppressCallsignsFromBds20                   = config.RawDecodingSettings.IgnoreCallsignsInBds20;
            result.SuppressReceiverRangeCheck                   = config.RawDecodingSettings.SuppressReceiverRangeCheck;
            result.TrackingTimeoutSeconds                       = config.BaseStationSettings.TrackingTimeoutSeconds;
            result.UseLocalDecodeForInitialPosition             = config.RawDecodingSettings.UseLocalDecodeForInitialPosition;

            return result;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when the listener catches an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }

        /// <summary>
        /// Raised when the aircraft list catches an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftList_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }
        #endregion
    }
}
