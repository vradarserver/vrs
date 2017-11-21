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
using InterfaceFactory;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON object that carries server settings to the site.
    /// </summary>
    [DataContract]
    public class ServerConfigJson
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
        /// Gets or sets a value indicating that text should be drawn onto the markers using marker labels
        /// instead of asking the server to draw it directly onto the marker image.
        /// </summary>
        /// <remarks>
        /// This is always false when running under .NET. It defaults to true when running under Mono but
        /// can be modified via Tools | Options | General | Mono.
        /// </remarks>
        [DataMember]
        public bool UseMarkerLabels { get; set; }

        /// <summary>
        /// Gets a collection of receiver names.
        /// </summary>
        [DataMember]
        public List<ServerReceiverJson> Receivers { get; private set; } = new List<ServerReceiverJson>();

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
        /// Gets or sets the API key to use when working with Google Maps.
        /// </summary>
        [DataMember]
        public string GoogleMapsApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that SVG graphics should be used wherever possible.
        /// </summary>
        [DataMember]
        public bool UseSvgGraphics { get; set; }

        /// <summary>
        /// Returns a new model.
        /// </summary>
        /// <param name="isLocalAddress"></param>
        /// <returns></returns>
        public static ServerConfigJson ToModel(bool isLocalAddress)
        {
            var applicationInformation = Factory.Singleton.Resolve<IApplicationInformation>();
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            var configuration = Factory.Singleton.ResolveSingleton<ISharedConfiguration>().Get();

            var isMono = runtimeEnvironment.IsMono;

            var result = new ServerConfigJson() {
                InitialDistanceUnit =                   GetDistanceUnit(configuration.GoogleMapSettings.InitialDistanceUnit),
                InitialHeightUnit =                     GetHeightUnit(configuration.GoogleMapSettings.InitialHeightUnit),
                InitialLatitude =                       configuration.GoogleMapSettings.InitialMapLatitude,
                InitialLongitude =                      configuration.GoogleMapSettings.InitialMapLongitude,
                InitialMapType =                        GetMapType(configuration.GoogleMapSettings.InitialMapType),
                InitialSettings =                       configuration.GoogleMapSettings.InitialSettings,
                InitialSpeedUnit =                      GetSpeedUnit(configuration.GoogleMapSettings.InitialSpeedUnit),
                InitialZoom =                           configuration.GoogleMapSettings.InitialMapZoom,
                InternetClientCanRunReports =           configuration.InternetClientSettings.CanRunReports,
                InternetClientCanShowPinText =          configuration.InternetClientSettings.CanShowPinText,
                InternetClientsCanPlayAudio =           configuration.InternetClientSettings.CanPlayAudio,
                InternetClientsCanSubmitRoutes =        configuration.InternetClientSettings.CanSubmitRoutes,
                InternetClientsCanSeeAircraftPictures = configuration.InternetClientSettings.CanShowPictures,
                InternetClientsCanSeePolarPlots =       configuration.InternetClientSettings.CanShowPolarPlots,
                InternetClientTimeoutMinutes =          configuration.InternetClientSettings.TimeoutMinutes,
                IsAudioEnabled =                        configuration.AudioSettings.Enabled,
                IsLocalAddress =                        isLocalAddress,
                IsMono =                                isMono,
                UseMarkerLabels =                       isMono ? configuration.MonoSettings.UseMarkerLabels : false,
                UseSvgGraphics =                        configuration.GoogleMapSettings.UseSvgGraphics,
                MinimumRefreshSeconds =                 configuration.GoogleMapSettings.MinimumRefreshSeconds,
                RefreshSeconds =                        configuration.GoogleMapSettings.InitialRefreshSeconds,
                VrsVersion =                            applicationInformation.ShortVersion,
            };
            result.Receivers.AddRange(configuration.Receivers.Select(r => ServerReceiverJson.ToModel(r)).Where(r => r != null));
            result.Receivers.AddRange(configuration.MergedFeeds.Select(r => ServerReceiverJson.ToModel(r)).Where(r => r != null));

            if(!isLocalAddress || configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests) {
                result.GoogleMapsApiKey = configuration.GoogleMapSettings.GoogleMapsApiKey;
                if(result.GoogleMapsApiKey == "") {
                    result.GoogleMapsApiKey = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Translates from a Google map type to a JavaScript map type.
        /// </summary>
        /// <param name="mapType"></param>
        /// <returns></returns>
        private static string GetMapType(string mapType)
        {
            switch(mapType ?? "") {
                case "HYBRID":          return "h";
                case "ROADMAP":         return "m";
                case "TERRAIN":         return "t";
                case "SATELLITE":       return "s";
                case "HIGHCONTRAST":    return "o";
                default:                return null;
            }
        }

        /// <summary>
        /// Translates from an internal distance unit to a JavaScript distance unit.
        /// </summary>
        /// <param name="distanceUnit"></param>
        /// <returns></returns>
        private static string GetDistanceUnit(DistanceUnit distanceUnit)
        {
            switch(distanceUnit) {
                case DistanceUnit.Kilometres:       return "km";
                case DistanceUnit.Miles:            return "sm";
                case DistanceUnit.NauticalMiles:    return "nm";
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Translates from an internal height unit to a JavaScript height unit.
        /// </summary>
        /// <param name="heightUnit"></param>
        /// <returns></returns>
        private static string GetHeightUnit(HeightUnit heightUnit)
        {
            switch(heightUnit) {
                case HeightUnit.Feet:               return "f";
                case HeightUnit.Metres:             return "m";
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Translates from an internal speed unit to a JavaScript speed unit.
        /// </summary>
        /// <param name="speedUnit"></param>
        /// <returns></returns>
        private static string GetSpeedUnit(SpeedUnit speedUnit)
        {
            switch(speedUnit) {
                case SpeedUnit.KilometresPerHour:   return "km";
                case SpeedUnit.Knots:               return "kt";
                case SpeedUnit.MilesPerHour:        return "ml";
                default:                            throw new NotImplementedException();
            }
        }
    }
}
