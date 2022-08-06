using System;
using InterfaceFactory;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Listener
{
    class ReceiverFeed : NetworkFeed, IReceiverFeed
    {
        /// <summary>
        /// The singleton receiver format manager.
        /// </summary>
        private IReceiverFormatManager _ReceiverFormatManager;

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
            if(!receiver.Enabled) throw new InvalidOperationException($"The {receiver.Name} receiver has not been enabled");
            var receiverLocation = configuration.ReceiverLocation(receiver.ReceiverLocationId);
            _ReceiverFormatManager = Factory.ResolveSingleton<IReceiverFormatManager>();

            Listener = Factory.Resolve<IListener>();
            Listener.ConnectionStateChanged += Listener_ConnectionStateChanged;
            Listener.ExceptionCaught += Listener_ExceptionCaught;
            Listener.IgnoreBadMessages = true;
            ApplyReceiverListenerSettings(false, receiver, configuration, receiverLocation);

            var startAircraftList = receiver.ReceiverUsage != ReceiverUsage.MergeOnly;

            DoCommonInitialise(receiver.UniqueId, receiver.Name, receiver.IsSatcomFeed, receiver.ReceiverUsage, startAircraftList: startAircraftList);
            ConfigurePolarPlotter(receiverLocation, nameChanged: false);
        }

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
            if(receiver.UniqueId != UniqueId) throw new InvalidOperationException($"Cannot apply configuration for receiver #{receiver.UniqueId} to feed for receiver for #{UniqueId}");
            var receiverLocation = configuration.ReceiverLocation(receiver.ReceiverLocationId);

            var initialisedWithReceiver = (Listener as IMergedFeedListener) == null;
            if(!initialisedWithReceiver) throw new InvalidOperationException("Feed {UniqueId} was initialised with a merged feed but updated with a receiver");

            var nameChanged = !String.IsNullOrEmpty(Name) && Name != receiver.Name;
            if(nameChanged) SaveCurrentPolarPlot();

            Name = receiver.Name;
            ApplyReceiverListenerSettings(true, receiver, configuration, receiverLocation);

            if(receiver.ReceiverUsage == ReceiverUsage.MergeOnly) {
                AircraftList.Stop();
            } else {
                AircraftList.Start();
            }

            ConfigurePolarPlotter(receiverLocation, nameChanged);
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
            var feedSourceHasChanged = connector != Listener.Connector || bytesExtractor != Listener.BytesExtractor;
            var rawMessageTranslator = DetermineRawMessageTranslator(receiver, receiverLocation, configuration, feedSourceHasChanged);

            var existingAuthentication = (Listener.Connector == null ? null : Listener.Connector.Authentication) as IPassphraseAuthentication;
            var passphrase = receiver.Passphrase ?? "";

            if((existingAuthentication != null && existingAuthentication.Passphrase != passphrase) ||
               (existingAuthentication == null && passphrase != "")) {
                IPassphraseAuthentication authentication = null;
                if(passphrase != "") {
                    authentication = Factory.Resolve<IPassphraseAuthentication>();
                    authentication.Passphrase = receiver.Passphrase;
                }
                if(!feedSourceHasChanged) Listener.Connector.Authentication = authentication;
                else                      connector.Authentication = authentication;
            }

            if(feedSourceHasChanged) {
                Listener.ChangeSource(connector, bytesExtractor, rawMessageTranslator);
                if(Listener.Statistics != null) Listener.Statistics.ResetMessageCounters();
            }

            Listener.ReceiverName = receiver.Name;
            Listener.IsSatcomFeed = receiver.IsSatcomFeed;
            Listener.AssumeDF18CF1IsIcao = configuration.RawDecodingSettings.AssumeDF18CF1IsIcao;

            SetIsVisible(receiver.ReceiverUsage);
        }

        /// <summary>
        /// Creates and configures the provider to use to connect to the data feed.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IConnector DetermineConnector(Receiver settings)
        {
            var result = Listener.Connector;

            switch(settings.ConnectionType) {
                case ConnectionType.COM:
                    var existingSerialProvider = result as ISerialConnector;
                    if(existingSerialProvider == null || existingSerialProvider.BaudRate != settings.BaudRate || existingSerialProvider.ComPort != settings.ComPort ||
                       existingSerialProvider.DataBits != settings.DataBits || existingSerialProvider.Handshake != settings.Handshake || 
                       existingSerialProvider.Parity != settings.Parity || existingSerialProvider.ShutdownText != settings.ShutdownText ||
                       existingSerialProvider.StartupText != settings.StartupText || existingSerialProvider.StopBits != settings.StopBits) {
                        var serialConnector = Factory.Resolve<ISerialConnector>();
                        serialConnector.Name =          settings.Name;
                        serialConnector.BaudRate =      settings.BaudRate;
                        serialConnector.ComPort =       settings.ComPort;
                        serialConnector.DataBits =      settings.DataBits;
                        serialConnector.Handshake =     settings.Handshake;
                        serialConnector.Parity =        settings.Parity;
                        serialConnector.ShutdownText =  settings.ShutdownText;
                        serialConnector.StartupText =   settings.StartupText;
                        serialConnector.StopBits =      settings.StopBits;
                        result = serialConnector;
                    }
                    break;
                case ConnectionType.TCP:
                    var existingConnector = result as INetworkConnector;
                    if(existingConnector == null ||
                       existingConnector.Port != settings.Port ||
                       existingConnector.UseKeepAlive != settings.UseKeepAlive ||
                       existingConnector.IdleTimeout != settings.IdleTimeoutMilliseconds ||
                       existingConnector.IsPassive != settings.IsPassive ||
                       (!settings.IsPassive && existingConnector.Address != settings.Address) ||
                       (settings.IsPassive && !Object.Equals(existingConnector.Access, settings.Access))) {
                        var ipConnector = Factory.Resolve<INetworkConnector>();
                        ipConnector.Name =            settings.Name;
                        ipConnector.IsPassive =       settings.IsPassive;
                        ipConnector.Port =            settings.Port;
                        ipConnector.UseKeepAlive =    settings.UseKeepAlive;
                        ipConnector.IdleTimeout =     settings.IdleTimeoutMilliseconds;

                        if(!settings.IsPassive) {
                            ipConnector.Address =     settings.Address;
                        } else {
                            ipConnector.Access =      settings.Access;
                        }

                        result = ipConnector;
                    }
                    break;
                case ConnectionType.HTTP:
                    var existingHttpConnector = result as IHttpConnector;
                    if(   existingHttpConnector == null
                       || existingHttpConnector.WebAddress != settings.WebAddress
                       || existingHttpConnector.FetchIntervalMilliseconds != settings.FetchIntervalMilliseconds
                    ) {
                        var httpConnector = Factory.Resolve<IHttpConnector>();
                        httpConnector.Name =                        settings.Name;
                        httpConnector.WebAddress =                  settings.WebAddress;
                        httpConnector.FetchIntervalMilliseconds =   settings.FetchIntervalMilliseconds;
                        result = httpConnector;
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
            var result = Listener.BytesExtractor;

            var provider = _ReceiverFormatManager.GetProvider(receiver.DataSource);
            if(provider == null) {
                throw new InvalidOperationException($"There is no receiver format provider registered with a unique ID of {receiver.DataSource}");
            }
            if(result == null || !provider.IsUsableBytesExtractor(result)) {
                result = provider.CreateMessageBytesExtractor();
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
            var result = isNewSource || Listener.RawMessageTranslator == null ? Factory.Resolve<IRawMessageTranslator>() : Listener.RawMessageTranslator;

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
            result.SuppressTisbDecoding                         = config.RawDecodingSettings.SuppressTisbDecoding;
            result.TrackingTimeoutSeconds                       = config.BaseStationSettings.TrackingTimeoutSeconds;
            result.UseLocalDecodeForInitialPosition             = config.RawDecodingSettings.UseLocalDecodeForInitialPosition;
            result.IgnoreInvalidCodeBlockInOtherMessages        = config.RawDecodingSettings.IgnoreInvalidCodeBlockInOtherMessages;
            result.IgnoreInvalidCodeBlockInParityMessages       = config.RawDecodingSettings.IgnoreInvalidCodeBlockInParityMessages;

            return result;
        }
    }
}
