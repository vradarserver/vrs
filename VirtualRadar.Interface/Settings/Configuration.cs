// Copyright © 2010 onwards, Andrew Whewell
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
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Specialized;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// An object that carries all of the configuration settings for the application.
    /// </summary>
    public class Configuration : INotifyPropertyChanged
    {
        private BaseStationSettings _BaseStationSettings;
        /// <summary>
        /// Gets or sets the object holding settings that describe the source of data that we are listening to.
        /// </summary>
        public BaseStationSettings BaseStationSettings
        {
            get { return _BaseStationSettings; }
            set { SetField(ref _BaseStationSettings, value, () => BaseStationSettings); }
        }

        private FlightRouteSettings _FlightRouteSettings;
        /// <summary>
        /// Gets or sets the object holding settings that control how flight routes are used and stored.
        /// </summary>
        public FlightRouteSettings FlightRouteSettings
        {
            get { return _FlightRouteSettings; }
            set { SetField(ref _FlightRouteSettings, value, () => FlightRouteSettings); }
        }

        private WebServerSettings _WebServerSettings;
        /// <summary>
        /// Gets or sets the object holding the configuration of the web server.
        /// </summary>
        public WebServerSettings WebServerSettings
        {
            get { return _WebServerSettings; }
            set { SetField(ref _WebServerSettings, value, () => WebServerSettings); }
        }

        private GoogleMapSettings _GoogleMapSettings;
        /// <summary>
        /// Gets or sets the object holding settings that modify how the Google Maps pages are shown to connecting browsers.
        /// </summary>
        public GoogleMapSettings GoogleMapSettings
        {
            get { return _GoogleMapSettings; }
            set { SetField(ref _GoogleMapSettings, value, () => GoogleMapSettings); }
        }

        private VersionCheckSettings _VersionCheckSettings;
        /// <summary>
        /// Gets or sets the object holding settings that control how checks for new versions of the application are made.
        /// </summary>
        public VersionCheckSettings VersionCheckSettings
        {
            get { return _VersionCheckSettings; }
            set { SetField(ref _VersionCheckSettings, value, () => VersionCheckSettings); }
        }

        private InternetClientSettings _InternetClientSettings;
        /// <summary>
        /// Gets or sets the object holding settings that control what resources are made available to browsers connecting from public Internet addresses.
        /// </summary>
        public InternetClientSettings InternetClientSettings
        {
            get { return _InternetClientSettings; }
            set { SetField(ref _InternetClientSettings, value, () => InternetClientSettings); }
        }

        private AudioSettings _AudioSettings;
        /// <summary>
        /// Gets or sets the object that controls the audio that is sent to browsers.
        /// </summary>
        public AudioSettings AudioSettings
        {
            get { return _AudioSettings; }
            set { SetField(ref _AudioSettings, value, () => AudioSettings); }
        }

        private RawDecodingSettings _RawDecodingSettings;
        /// <summary>
        /// Gets or sets the object that configures the raw message decoding.
        /// </summary>
        public RawDecodingSettings RawDecodingSettings
        {
            get { return _RawDecodingSettings; }
            set { SetField(ref _RawDecodingSettings, value, () => RawDecodingSettings); }
        }

        private BindingList<Receiver> _Receivers = new BindingList<Receiver>();
        /// <summary>
        /// Gets a list of every receveiver that the program will listen to.
        /// </summary>
        public BindingList<Receiver> Receivers { get { return _Receivers; } }

        private BindingList<MergedFeed> _MergedFeeds = new BindingList<MergedFeed>();
        /// <summary>
        /// Gets a list of the merged feeds of receivers that the program will maintain.
        /// </summary>
        public BindingList<MergedFeed> MergedFeeds { get { return _MergedFeeds; } }

        private BindingList<ReceiverLocation> _ReceiverLocations = new BindingList<ReceiverLocation>();
        /// <summary>
        /// Gets a list of every receiver location recorded by the user.
        /// </summary>
        public BindingList<ReceiverLocation> ReceiverLocations { get { return _ReceiverLocations; } }

        private BindingList<RebroadcastSettings> _RebroadcastSettings = new BindingList<RebroadcastSettings>();
        /// <summary>
        /// Gets a list of all of the rebroadcast server settings recorded by the user.
        /// </summary>
        public BindingList<RebroadcastSettings> RebroadcastSettings { get { return _RebroadcastSettings; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        /// <summary>
        /// Changes the field's value and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if(EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if(selectorExpression == null) throw new ArgumentNullException("selectorExpression");
            MemberExpression body = selectorExpression.Body as MemberExpression;
            if(body == null) throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

            return true;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Configuration()
        {
            BaseStationSettings = new BaseStationSettings();
            FlightRouteSettings = new FlightRouteSettings();
            WebServerSettings = new WebServerSettings();
            GoogleMapSettings = new GoogleMapSettings();
            VersionCheckSettings = new VersionCheckSettings();
            InternetClientSettings = new InternetClientSettings();
            AudioSettings = new AudioSettings();
            RawDecodingSettings = new RawDecodingSettings();

            _MergedFeeds.ListChanged +=           MergedFeeds_ListChanged;
            _RebroadcastSettings.ListChanged +=   RebroadcastSettings_ListChanged;
            _ReceiverLocations.ListChanged +=     ReceiverLocations_ListChanged;
            _Receivers.ListChanged +=             Receivers_ListChanged;
        }

        /// <summary>
        /// Returns the receiver location.
        /// </summary>
        /// <param name="locationUniqueId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Originally the program didn't need to know where the receiver was located, but after ADS-B decoding
        /// was added it needed to know for local surface position decoding. Because all of this data is being
        /// serialised we unfortunately can't have direct references to <see cref="ReceiverLocation"/> objects
        /// in the classes so when a receiver location needs to be recorded only its ID is stored. This looks
        /// up the location for the ID passed across in a consistent fashion. It may return null if the receiver
        /// location with that name no longer exists.
        /// </remarks>
        public ReceiverLocation ReceiverLocation(int locationUniqueId)
        {
            return ReceiverLocations.FirstOrDefault(r => r.UniqueId == locationUniqueId);
        }

        private void MergedFeeds_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(PropertyHelper.ExtractName(this, r => r.MergedFeeds)));
            }
        }

        private void RebroadcastSettings_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(PropertyHelper.ExtractName(this, r => r.RebroadcastSettings)));
            }
        }

        private void ReceiverLocations_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(PropertyHelper.ExtractName(this, r => r.ReceiverLocations)));
            }
        }

        private void Receivers_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(PropertyHelper.ExtractName(this, r => r.Receivers)));
            }
        }
    }
}
