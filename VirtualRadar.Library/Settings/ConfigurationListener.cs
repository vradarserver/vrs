// Copyright © 2014 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using VirtualRadar.Interface.PortableBinding;
using VirtualRadar.Interface;

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
        private List<IBindingList> _HookedLists = new List<IBindingList>();

        // Lists of configuration child objects that have been hooked.
        private List<Receiver> _HookedReceivers = new List<Receiver>();
        private List<MergedFeed> _HookedMergedFeeds = new List<MergedFeed>();
        private List<MergedFeedReceiver> _HookedMergedFeedReceivers = new List<MergedFeedReceiver>();
        private List<RebroadcastSettings> _HookedRebroadcastSettings = new List<RebroadcastSettings>();
        private List<ReceiverLocation> _HookedReceiverLocations = new List<ReceiverLocation>();
        private Dictionary<object, List<object>> _HookedChildren = new Dictionary<object,List<object>>();
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<ConfigurationListenerEventArgs> PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(ConfigurationListenerEventArgs args)
        {
            EventHelper.Raise(PropertyChanged, this, args);
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
            EventHelper.Raise(PropertyChanged, this, () => new ConfigurationListenerEventArgs(_Configuration, record, isListChild, group, propertyChanged.PropertyName));
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
        /// See interface docs.
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialise(Configuration configuration)
        {
            HookConfiguration(configuration);
        }

        /// <summary>
        /// See interface docs.
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
                _Configuration.MonoSettings.PropertyChanged +=              MonoSettings_PropertyChanged;
                _Configuration.RawDecodingSettings.PropertyChanged +=       RawDecodingSettings_PropertyChanged;
                _Configuration.VersionCheckSettings.PropertyChanged +=      VersionCheckSettings_PropertyChanged;
                _Configuration.WebServerSettings.PropertyChanged +=         WebServerSettings_PropertyChanged;

                HookList(_Configuration.MergedFeeds,            _HookedMergedFeeds,         MergedFeed_PropertyChanged,         MergedFeeds_ListChanged,         MergedFeed_HookChild,          MergedFeed_UnhookChild);
                HookList(_Configuration.RebroadcastSettings,    _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_ListChanged, RebroadcastSetting_HookChild,  RebroadcastSetting_UnhookChild);
                HookList(_Configuration.ReceiverLocations,      _HookedReceiverLocations,   ReceiverLocation_PropertyChanged,   ReceiverLocations_ListChanged,   null,                          null);
                HookList(_Configuration.Receivers,              _HookedReceivers,           Receiver_PropertyChanged,           Receivers_ListChanged,           Receiver_HookChild,            Receiver_UnhookChild);
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
                _Configuration.MonoSettings.PropertyChanged -=              MonoSettings_PropertyChanged;
                _Configuration.RawDecodingSettings.PropertyChanged -=       RawDecodingSettings_PropertyChanged;
                _Configuration.VersionCheckSettings.PropertyChanged -=      VersionCheckSettings_PropertyChanged;
                _Configuration.WebServerSettings.PropertyChanged -=         WebServerSettings_PropertyChanged;

                UnhookList(_Configuration.MergedFeeds,          _HookedMergedFeeds,         MergedFeed_PropertyChanged,         MergedFeeds_ListChanged,         MergedFeed_UnhookChild);
                UnhookList(_Configuration.RebroadcastSettings,  _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_ListChanged, RebroadcastSetting_UnhookChild);
                UnhookList(_Configuration.ReceiverLocations,    _HookedReceiverLocations,   ReceiverLocation_PropertyChanged,   ReceiverLocations_ListChanged,   null);
                UnhookList(_Configuration.Receivers,            _HookedReceivers,           Receiver_PropertyChanged,           Receivers_ListChanged,           Receiver_UnhookChild);

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
        /// <param name="hookChild"></param>
        /// <param name="unhookChild"></param>
        private void HookList<T>(NotifyList<T> list, List<T> hookedRecords, PropertyChangedEventHandler recordEventHandler, ListChangedEventHandler listEventHandler, Action<T> hookChild = null, Action<T> unhookChild = null)
            where T: INotifyPropertyChanged
        {
            if(_HookedLists.Contains(list)) UnhookList(list, hookedRecords, recordEventHandler, listEventHandler, unhookChild);

            foreach(var record in list) {
                hookedRecords.Add(record);
                record.PropertyChanged += recordEventHandler;
                if(hookChild != null) hookChild(record);
            }

            list.ListChanged += listEventHandler;
            _HookedLists.Add(list);
        }

        /// <summary>
        /// Unhooks a hooked list and all of its contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hookedRecords"></param>
        /// <param name="recordEventHandler"></param>
        /// <param name="listEventHandler"></param>
        /// <param name="unhookChild"></param>
        private void UnhookList<T>(NotifyList<T> list, List<T> hookedRecords, PropertyChangedEventHandler recordEventHandler, ListChangedEventHandler listEventHandler, Action<T> unhookChild = null)
            where T: INotifyPropertyChanged
        {
            if(_HookedLists.Contains(list)) {
                foreach(var record in hookedRecords) {
                    record.PropertyChanged -= recordEventHandler;
                    if(unhookChild != null) unhookChild(record);
                }
                hookedRecords.Clear();

                list.ListChanged -= listEventHandler;
                _HookedLists.Remove(list);
            }
        }

        /// <summary>
        /// Hooks the child records on a merged feed.
        /// </summary>
        /// <param name="record"></param>
        private void MergedFeed_HookChild(MergedFeed record)
        {
            HookList<MergedFeedReceiver>(record.ReceiverFlags, _HookedMergedFeedReceivers, MergedFeedReceiver_PropertyChanged, MergedFeedReceviers_ListChanged);
        }

        /// <summary>
        /// Unhooks the child records on a merged feed.
        /// </summary>
        /// <param name="record"></param>
        private void MergedFeed_UnhookChild(MergedFeed record)
        {
            UnhookList<MergedFeedReceiver>(record.ReceiverFlags, _HookedMergedFeedReceivers, MergedFeedReceiver_PropertyChanged, MergedFeedReceviers_ListChanged);
        }

        /// <summary>
        /// Hooks child objects on a rebroadcast setting.
        /// </summary>
        /// <param name="record"></param>
        private void RebroadcastSetting_HookChild(RebroadcastSettings record)
        {
            if(!HaveHookedChildren(record)) {
                record.Access.PropertyChanged += RebroadcastSetting_Access_PropertyChanged;
                RecordHookedChild(record, record.Access);
            }
        }

        /// <summary>
        /// Hooks child objects on a receiver.
        /// </summary>
        /// <param name="record"></param>
        private void Receiver_HookChild(Receiver record)
        {
            if(!HaveHookedChildren(record)) {
                record.Access.PropertyChanged += Receiver_Access_PropertyChanged;
                RecordHookedChild(record, record.Access);
            }
        }

        /// <summary>
        /// Unhooks child objects on a rebroadcast setting.
        /// </summary>
        /// <param name="record"></param>
        private void RebroadcastSetting_UnhookChild(RebroadcastSettings record)
        {
            foreach(var child in GetHookedChildren(record).OfType<Access>()) {
                child.PropertyChanged -= RebroadcastSetting_Access_PropertyChanged;
                RecordUnhookedChild(record, child);
            }
        }

        /// <summary>
        /// Unhooks child objects on a receiver.
        /// </summary>
        /// <param name="record"></param>
        private void Receiver_UnhookChild(Receiver record)
        {
            foreach(var child in GetHookedChildren(record).OfType<Access>()) {
                child.PropertyChanged -= Receiver_Access_PropertyChanged;
                RecordUnhookedChild(record, child);
            }
        }

        private bool HaveHookedChildren(object record)
        {
            return _HookedChildren.ContainsKey(record);
        }

        private object[] GetHookedChildren(object record)
        {
            List<object> result;
            _HookedChildren.TryGetValue(record, out result);

            return result.ToArray() ?? new object[0];
        }

        private void RecordHookedChild(object record, object child)
        {
            List<object> hookedChildren;
            if(!_HookedChildren.TryGetValue(record, out hookedChildren)) {
                hookedChildren = new List<object>();
                _HookedChildren.Add(record, hookedChildren);
            }

            if(!hookedChildren.Contains(child)) hookedChildren.Add(child);
        }

        private void RecordUnhookedChild(object record, object child)
        {
            List<object> hookedChildren;
            if(_HookedChildren.TryGetValue(record, out hookedChildren)) {
                if(hookedChildren.Contains(child)) hookedChildren.Remove(child);
                if(hookedChildren.Count == 0) _HookedChildren.Remove(record);
            }
        }
        #endregion

        #region Event handlers - PropertyChanged
        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Configuration, args);
        }

        private void Access_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Access, args);
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

        private void MergedFeedReceiver_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.MergedFeedReceiver, args, isListChild: true);
        }

        private void MonoSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.MonoSettings, args);
        }

        private void RawDecodingSettings_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.RawDecodingSettings, args);
        }

        private void RebroadcastSetting_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.RebroadcastSetting, args, isListChild: true);
        }

        private void RebroadcastSetting_Access_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Access, args, isListChild: true);
        }

        private void Receiver_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Receiver, args, isListChild: true);
        }

        private void Receiver_Access_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(sender, ConfigurationListenerGroup.Access, args, isListChild: true);
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
        private void MergedFeeds_ListChanged(object sender, ListChangedEventArgs args)
        {
            HookList(_Configuration.MergedFeeds, _HookedMergedFeeds, MergedFeed_PropertyChanged, MergedFeeds_ListChanged, MergedFeed_HookChild, MergedFeed_UnhookChild);
        }

        private void MergedFeedReceviers_ListChanged(object sender, ListChangedEventArgs args)
        {
            HookList((NotifyList<MergedFeedReceiver>)sender, _HookedMergedFeedReceivers, MergedFeedReceiver_PropertyChanged, MergedFeedReceviers_ListChanged);
        }

        private void RebroadcastSettings_ListChanged(object sender, ListChangedEventArgs args)
        {
            HookList(_Configuration.RebroadcastSettings, _HookedRebroadcastSettings, RebroadcastSetting_PropertyChanged, RebroadcastSettings_ListChanged, RebroadcastSetting_HookChild, RebroadcastSetting_UnhookChild);
        }

        private void Receivers_ListChanged(object sender, ListChangedEventArgs args)
        {
            HookList(_Configuration.Receivers, _HookedReceivers, Receiver_PropertyChanged, Receivers_ListChanged, Receiver_HookChild, Receiver_UnhookChild);
        }

        private void ReceiverLocations_ListChanged(object sender, ListChangedEventArgs args)
        {
            HookList(_Configuration.ReceiverLocations, _HookedReceiverLocations, ReceiverLocation_PropertyChanged, ReceiverLocations_ListChanged);
        }
        #endregion
    }
}
