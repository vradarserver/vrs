using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// A class that can listen to a <see cref="Configuration"/> object and raise
    /// events when its properties are changed.
    /// </summary>
    class ConfigurationListener : IConfigurationListener
    {
        #region Fields
        /// <summary>
        /// The configuration that we're listening to.
        /// </summary>
        private Configuration _Configuration;

        /// <summary>
        /// A collection of hooked lists.
        /// </summary>
        private List<INotifyCollectionChanged> _HookedLists = new List<INotifyCollectionChanged>();

        // Lists of child objects that have been hooked.
        private List<Receiver> _HookedReceivers = new List<Receiver>();
        private List<MergedFeed> _HookedMergedFeeds = new List<MergedFeed>();
        private List<RebroadcastSettings> _HookedRebroadcastSettings = new List<RebroadcastSettings>();
        private List<ReceiverLocation> _HookedReceiverLocations = new List<ReceiverLocation>();
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised when a configuration property changes.
        /// </summary>
        public event EventHandler<ConfigurationListenerEventArgs> PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(ConfigurationListenerEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        /// <summary>
        /// Shortcut method to call <see cref="OnPropertyChanged"/>.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="group"></param>
        /// <param name="propertyChanged"></param>
        /// <param name="isListChild"></param>
        private void RaisePropertyChanged(object record, ConfigurationListenerGroup group, PropertyChangedEventArgs propertyChanged, bool isListChild = false)
        {
            var args = new ConfigurationListenerEventArgs(_Configuration, record, isListChild, group, propertyChanged.PropertyName);
            OnPropertyChanged(args);
        }
        #endregion

        #region Ctors and finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ConfigurationListener()
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
        /// Disposes or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                UnhookConfiguration();
            }
        }
        #endregion

        #region Initialise, Release
        /// <summary>
        /// Initialises the object.
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialise(Configuration configuration)
        {
            HookConfiguration(configuration);
        }

        /// <summary>
        /// Unhooks the configuration hooked by <see cref="Initialise"/>.
        /// </summary>
        public void Release()
        {
            UnhookConfiguration();
        }
        #endregion

        #region HookConfiguration, UnhookConfiguration
        /// <summary>
        /// Hooks the configuration passed across.
        /// </summary>
        /// <param name="configuration"></param>
        private void HookConfiguration(Configuration configuration)
        {
            if(_Configuration != configuration) {
                if(_Configuration != null) UnhookConfiguration();
                _Configuration = configuration;

                _Configuration.PropertyChanged +=                           Configuration_PropertyChanged;
                _Configuration.AudioSettings.PropertyChanged +=             AudioSettings_PropertyChanged;
                _Configuration.BaseStationSettings.PropertyChanged +=       BaseStationSettings_PropertyChanged;
                _Configuration.FlightRouteSettings.PropertyChanged +=       FlightRouteSettings_PropertyChanged;
                _Configuration.GoogleMapSettings.PropertyChanged +=         GoogleMapSettings_PropertyChanged;
                _Configuration.InternetClientSettings.PropertyChanged +=    InternetClientSettings_PropertyChanged;
                _Configuration.RawDecodingSettings.PropertyChanged +=       RawDecodingSettings_PropertyChanged;
                _Configuration.VersionCheckSettings.PropertyChanged +=      VersionCheckSettings_PropertyChanged;
                _Configuration.WebServerSettings.PropertyChanged +=         WebServerSettings_PropertyChanged;

                HookList(_Configuration.MergedFeeds,            _HookedMergedFeeds,         MergedFeed_PropertyChanged,         MergedFeeds_CollectionChanged);
                HookList(_Configuration.RebroadcastSettings,    _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_CollectionChanged);
                HookList(_Configuration.ReceiverLocations,      _HookedReceiverLocations,   ReceiverLocation_PropertyChanged,   ReceiverLocations_CollectionChanged);
                HookList(_Configuration.Receivers,              _HookedReceivers,           Receiver_PropertyChanged,           Receivers_CollectionChanged);
            }
        }

        /// <summary>
        /// Unhooks the current configuration.
        /// </summary>
        private void UnhookConfiguration()
        {
            if(_Configuration != null) {
                _Configuration.PropertyChanged -=                           Configuration_PropertyChanged;
                _Configuration.AudioSettings.PropertyChanged -=             AudioSettings_PropertyChanged;
                _Configuration.BaseStationSettings.PropertyChanged -=       BaseStationSettings_PropertyChanged;
                _Configuration.FlightRouteSettings.PropertyChanged -=       FlightRouteSettings_PropertyChanged;
                _Configuration.GoogleMapSettings.PropertyChanged -=         GoogleMapSettings_PropertyChanged;
                _Configuration.InternetClientSettings.PropertyChanged -=    InternetClientSettings_PropertyChanged;
                _Configuration.RawDecodingSettings.PropertyChanged -=       RawDecodingSettings_PropertyChanged;
                _Configuration.VersionCheckSettings.PropertyChanged -=      VersionCheckSettings_PropertyChanged;
                _Configuration.WebServerSettings.PropertyChanged -=         WebServerSettings_PropertyChanged;

                UnhookList(_Configuration.MergedFeeds,          _HookedMergedFeeds,         MergedFeed_PropertyChanged,         MergedFeeds_CollectionChanged);
                UnhookList(_Configuration.RebroadcastSettings,  _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_CollectionChanged);
                UnhookList(_Configuration.ReceiverLocations,    _HookedReceiverLocations,   ReceiverLocation_PropertyChanged,   ReceiverLocations_CollectionChanged);
                UnhookList(_Configuration.Receivers,            _HookedReceivers,           Receiver_PropertyChanged,           Receivers_CollectionChanged);

                _Configuration = null;
            }
        }

        /// <summary>
        /// Hooks an observable list on the configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hookedRecords"></param>
        /// <param name="recordEventHandler"></param>
        /// <param name="listEventHandler"></param>
        private void HookList<T>(ObservableCollection<T> list, List<T> hookedRecords, PropertyChangedEventHandler recordEventHandler, NotifyCollectionChangedEventHandler listEventHandler)
            where T: INotifyPropertyChanged
        {
            if(_HookedLists.Contains(list)) UnhookList(list, hookedRecords, recordEventHandler, listEventHandler);

            list.CollectionChanged += listEventHandler;
            foreach(var record in list) {
                hookedRecords.Add(record);
                record.PropertyChanged += recordEventHandler;
            }
        }

        /// <summary>
        /// Unhooks a hooked list and all of its contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hookedRecords"></param>
        /// <param name="recordEventHandler"></param>
        /// <param name="listEventHandler"></param>
        private void UnhookList<T>(ObservableCollection<T> list, List<T> hookedRecords, PropertyChangedEventHandler recordEventHandler, NotifyCollectionChangedEventHandler listEventHandler)
            where T: INotifyPropertyChanged
        {
            if(_HookedLists.Contains(list)) {
                foreach(var record in hookedRecords) {
                    record.PropertyChanged -= recordEventHandler;
                }
                hookedRecords.Clear();

                list.CollectionChanged -= listEventHandler;
                _HookedLists.Remove(list);
            }
        }
        #endregion

        #region Event handlers - PropertyChanged
        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Configuration, args);
        }

        private void AudioSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Audio, args);
        }

        private void BaseStationSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.BaseStation, args);
        }

        private void FlightRouteSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.FlightRoute, args);
        }

        private void GoogleMapSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.GoogleMapSettings, args);
        }

        private void InternetClientSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.InternetClientSettings, args);
        }

        private void MergedFeed_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.MergedFeed, args, isListChild: true);
        }

        private void RawDecodingSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.RawDecodingSettings, args);
        }

        private void RebroadcastSetting_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.RebroadcastSetting, args, isListChild: true);
        }

        private void Receiver_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Receiver, args, isListChild: true);
        }

        private void ReceiverLocation_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.ReceiverLocation, args, isListChild: true);
        }

        private void VersionCheckSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.VersionCheckSettings, args);
        }

        private void WebServerSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.WebServerSettings, args);
        }
        #endregion

        #region Event handlers - CollectionChanged
        private void MergedFeeds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HookList(_Configuration.MergedFeeds, _HookedMergedFeeds, MergedFeed_PropertyChanged, MergedFeeds_CollectionChanged);
        }

        private void RebroadcastSettings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HookList(_Configuration.RebroadcastSettings, _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_CollectionChanged);
        }

        private void Receivers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HookList(_Configuration.Receivers, _HookedReceivers, Receiver_PropertyChanged, Receivers_CollectionChanged);
        }

        private void ReceiverLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HookList(_Configuration.ReceiverLocations, _HookedReceiverLocations, ReceiverLocation_PropertyChanged, ReceiverLocations_CollectionChanged);
        }
        #endregion
    }
}
