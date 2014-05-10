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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// An object that carries all of the configuration settings for the application.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the object holding settings that describe the source of data that we are listening to.
        /// </summary>
        public BaseStationSettings BaseStationSettings { get; set; }

        /// <summary>
        /// Gets or sets the object holding settings that control how flight routes are used and stored.
        /// </summary>
        public FlightRouteSettings FlightRouteSettings { get; set; }

        /// <summary>
        /// Gets or sets the object holding the configuration of the web server.
        /// </summary>
        public WebServerSettings WebServerSettings { get; set; }

        /// <summary>
        /// Gets or sets the object holding settings that modify how the Google Maps pages are shown to connecting browsers.
        /// </summary>
        public GoogleMapSettings GoogleMapSettings { get; set; }

        /// <summary>
        /// Gets or sets the object holding settings that control how checks for new versions of the application are made.
        /// </summary>
        public VersionCheckSettings VersionCheckSettings { get; set; }

        /// <summary>
        /// Gets or sets the object holding settings that control what resources are made available to browsers connecting from public Internet addresses.
        /// </summary>
        public InternetClientSettings InternetClientSettings { get; set; }

        /// <summary>
        /// Gets or sets the object that controls the audio that is sent to browsers.
        /// </summary>
        public AudioSettings AudioSettings { get; set; }

        /// <summary>
        /// Gets or sets the object that configures the raw message decoding.
        /// </summary>
        public RawDecodingSettings RawDecodingSettings { get; set; }

        private List<Receiver> _Receivers = new List<Receiver>();
        /// <summary>
        /// Gets a list of every receveiver that the program will listen to.
        /// </summary>
        public List<Receiver> Receivers { get { return _Receivers; } }

        private List<MergedFeed> _MergedFeeds = new List<MergedFeed>();
        /// <summary>
        /// Gets a list of the merged feeds of receivers that the program will maintain.
        /// </summary>
        public List<MergedFeed> MergedFeeds { get { return _MergedFeeds; } }

        private List<ReceiverLocation> _ReceiverLocations = new List<ReceiverLocation>();
        /// <summary>
        /// Gets a list of every receiver location recorded by the user.
        /// </summary>
        public List<ReceiverLocation> ReceiverLocations { get { return _ReceiverLocations; } }

        private List<RebroadcastSettings> _RebroadcastSettings = new List<RebroadcastSettings>();
        /// <summary>
        /// Gets a list of all of the rebroadcast server settings recorded by the user.
        /// </summary>
        public List<RebroadcastSettings> RebroadcastSettings { get { return _RebroadcastSettings; } }

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
    }
}
