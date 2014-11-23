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
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON object that carries server settings to the site.
    /// </summary>
    [DataContract]
    public class ServerConfigJson : ICloneable
    {
        /// <summary>
        /// Gets or sets the version number of the server.
        /// </summary>
        [DataMember]
        public string VrsVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server is running under Mono.
        /// </summary>
        [DataMember]
        public bool IsMono { get; set; }

        /// <summary>
        /// Gets a collection of receiver names.
        /// </summary>
        [DataMember]
        public List<ServerReceiverJson> Receivers { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that the browser address is probably a local address.
        /// </summary>
        [DataMember]
        public bool IsLocalAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user has enabled the site's audio features.
        /// </summary>
        [DataMember]
        public bool IsAudioEnabled { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of seconds between refreshes configured that the site should allow.
        /// </summary>
        [DataMember]
        public int MinimumRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the seconds between refreshes if the user hasn't already set a value.
        /// </summary>
        [DataMember]
        public int RefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the initial settings, if any, to apply to new users.
        /// </summary>
        [DataMember]
        public string InitialSettings { get; set; }

        /// <summary>
        /// Gets or sets the initial latitude for maps.
        /// </summary>
        [DataMember]
        public double InitialLatitude { get; set; }

        /// <summary>
        /// Gets or sets the initial longitude for maps.
        /// </summary>
        [DataMember]
        public double InitialLongitude { get; set; }

        /// <summary>
        /// Gets or sets the map type to use if the user hasn't already configured one.
        /// </summary>
        [DataMember]
        public string InitialMapType { get; set; }

        /// <summary>
        /// Gets or sets the initial level of zoom to use.
        /// </summary>
        [DataMember]
        public int InitialZoom { get; set; }

        /// <summary>
        /// Gets or sets the initial distance unit to use.
        /// </summary>
        [DataMember]
        public string InitialDistanceUnit { get; set; }

        /// <summary>
        /// Gets or sets the initial height unit to use.
        /// </summary>
        [DataMember]
        public string InitialHeightUnit { get; set; }

        /// <summary>
        /// Gets or sets the initial speed unit.
        /// </summary>
        [DataMember]
        public string InitialSpeedUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that an Internet client can run reports.
        /// </summary>
        [DataMember]
        public bool InternetClientCanRunReports { get; set; }

        /// <summary>
        /// Gets or set a value indicating that Internet clients are allowed to set pin text on aircraft markers.
        /// </summary>
        [DataMember]
        public bool InternetClientCanShowPinText { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes that Internet clients can remain idle before the site times out.
        /// </summary>
        [DataMember]
        public int InternetClientTimeoutMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients can make use of the audio features of the site.
        /// </summary>
        [DataMember]
        public bool InternetClientsCanPlayAudio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients can submit routes and route corrections.
        /// </summary>
        [DataMember]
        public bool InternetClientsCanSubmitRoutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to view the local aircraft pictures.
        /// </summary>
        [DataMember]
        public bool InternetClientsCanSeeAircraftPictures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to see receiver ranges.
        /// </summary>
        [DataMember]
        public bool InternetClientsCanSeePolarPlots { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ServerConfigJson()
        {
            Receivers = new List<ServerReceiverJson>();
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var result = (ServerConfigJson)Activator.CreateInstance(GetType());

            result.InitialDistanceUnit = InitialDistanceUnit;
            result.InitialHeightUnit = InitialHeightUnit;
            result.InitialLatitude = InitialLatitude;
            result.InitialLongitude = InitialLongitude;
            result.InitialMapType = InitialMapType;
            result.InitialSettings = InitialSettings;
            result.InitialSpeedUnit = InitialSpeedUnit;
            result.InitialZoom = InitialZoom;
            result.InternetClientCanRunReports = InternetClientCanRunReports;
            result.InternetClientCanShowPinText = InternetClientCanShowPinText;
            result.InternetClientsCanPlayAudio = InternetClientsCanPlayAudio;
            result.InternetClientsCanSubmitRoutes = InternetClientsCanSubmitRoutes;
            result.InternetClientTimeoutMinutes = InternetClientTimeoutMinutes;
            result.InternetClientsCanSeeAircraftPictures = InternetClientsCanSeeAircraftPictures;
            result.InternetClientsCanSeePolarPlots = InternetClientsCanSeePolarPlots;
            result.IsAudioEnabled = IsAudioEnabled;
            result.IsLocalAddress = IsLocalAddress;
            result.IsMono = IsMono;
            result.MinimumRefreshSeconds = MinimumRefreshSeconds;
            result.RefreshSeconds = RefreshSeconds;
            result.Receivers.AddRange(Receivers.Select(r => (ServerReceiverJson)r.Clone()));
            result.VrsVersion = VrsVersion;

            return result;
        }
    }
}
