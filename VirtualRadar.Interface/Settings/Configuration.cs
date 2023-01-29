// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// An object that carries all of the configuration settings for the application.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets a counter that is incremented every time the configuration is saved.
        /// </summary>
        public int DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the object holding settings that describe the source of data that we are listening to.
        /// </summary>
        public BaseStationSettings BaseStationSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object holding settings that control how flight routes are used and stored.
        /// </summary>
        public FlightRouteSettings FlightRouteSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object holding the configuration of the web server.
        /// </summary>
        public WebServerSettings WebServerSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object holding settings that modify how the Google Maps pages are shown to connecting browsers.
        /// </summary>
        public GoogleMapSettings GoogleMapSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object holding settings that control how checks for new versions of the application are made.
        /// </summary>
        public VersionCheckSettings VersionCheckSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object holding settings that control what resources are made available to browsers connecting from public Internet addresses.
        /// </summary>
        public InternetClientSettings InternetClientSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object that controls the audio that is sent to browsers.
        /// </summary>
        public AudioSettings AudioSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object that configures the raw message decoding.
        /// </summary>
        public RawDecodingSettings RawDecodingSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the object that carries Mono-only settings.
        /// </summary>
        public MonoSettings MonoSettings { get; set; } = new();

        /// <summary>
        /// Gets a list of every receveiver that the program will listen to.
        /// </summary>
        public IList<Receiver> Receivers { get; } = new List<Receiver>();

        /// <summary>
        /// Gets a list of the merged feeds of receivers that the program will maintain.
        /// </summary>
        public IList<MergedFeed> MergedFeeds { get; } = new List<MergedFeed>();

        /// <summary>
        /// Gets a list of every receiver location recorded by the user.
        /// </summary>
        public IList<ReceiverLocation> ReceiverLocations { get; } = new List<ReceiverLocation>();

        /// <summary>
        /// Gets a list of all of the rebroadcast server settings recorded by the user.
        /// </summary>
        public IList<RebroadcastSettings> RebroadcastSettings { get; } = new List<RebroadcastSettings>();
    }
}
