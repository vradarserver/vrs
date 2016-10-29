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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Serves the ServerConfig.json page.
    /// </summary>
    class ServerConfigJsonPage : Page
    {
        /// <summary>
        /// The object that locks <see cref="_ServerConfigJson"/> while we either change it or serve it.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The JSON object that the page will serve.
        /// </summary>
        private ServerConfigJson _ServerConfigJson = new ServerConfigJson();

        /// <summary>
        /// A copy of the UseGoogleMapsAPIKeyWithLocalRequests flag from the last configuration loaded.
        /// </summary>
        private bool _UseGoogleMapsAPIKeyWithLocalRequests;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public ServerConfigJsonPage(WebSite webSite) : base(webSite)
        {
        }

        /// <summary>
        /// Handles the request for the server configuration JSON.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            if("/ServerConfig.json".Equals(args.PathAndFile, StringComparison.OrdinalIgnoreCase)) {
                ServerConfigJson json;
                lock(_ServerConfigJson) json = (ServerConfigJson)_ServerConfigJson.Clone();
                json.IsLocalAddress = !args.IsInternetRequest;
                if(json.IsLocalAddress && !_UseGoogleMapsAPIKeyWithLocalRequests) {
                    json.GoogleMapsApiKey = null;
                }
                Responder.SendJson(args.Request, args.Response, json, null, null);
                args.Handled = true;
            }

            return args.Handled;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            base.DoLoadConfiguration(configuration);

            var applicationInformation = Factory.Singleton.Resolve<IApplicationInformation>();
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            var isMono = runtimeEnvironment.IsMono;

            lock(_SyncLock) {
                _UseGoogleMapsAPIKeyWithLocalRequests = configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests;
                _ServerConfigJson = new ServerConfigJson() {
                    GoogleMapsApiKey = configuration.GoogleMapSettings.GoogleMapsApiKey,
                    InitialDistanceUnit = GetDistanceUnit(configuration.GoogleMapSettings.InitialDistanceUnit),
                    InitialHeightUnit = GetHeightUnit(configuration.GoogleMapSettings.InitialHeightUnit),
                    InitialLatitude = configuration.GoogleMapSettings.InitialMapLatitude,
                    InitialLongitude = configuration.GoogleMapSettings.InitialMapLongitude,
                    InitialMapType = GetMapType(configuration.GoogleMapSettings.InitialMapType),
                    InitialSettings = configuration.GoogleMapSettings.InitialSettings,
                    InitialSpeedUnit = GetSpeedUnit(configuration.GoogleMapSettings.InitialSpeedUnit),
                    InitialZoom = configuration.GoogleMapSettings.InitialMapZoom,
                    InternetClientCanRunReports = configuration.InternetClientSettings.CanRunReports,
                    InternetClientCanShowPinText = configuration.InternetClientSettings.CanShowPinText,
                    InternetClientsCanPlayAudio = configuration.InternetClientSettings.CanPlayAudio,
                    InternetClientsCanSubmitRoutes = configuration.InternetClientSettings.CanSubmitRoutes,
                    InternetClientsCanSeeAircraftPictures = configuration.InternetClientSettings.CanShowPictures,
                    InternetClientsCanSeePolarPlots = configuration.InternetClientSettings.CanShowPolarPlots,
                    InternetClientTimeoutMinutes = configuration.InternetClientSettings.TimeoutMinutes,
                    IsAudioEnabled = configuration.AudioSettings.Enabled,
                    IsMono = isMono,
                    UseMarkerLabels = isMono ? configuration.MonoSettings.UseMarkerLabels : false,
                    UseSvgGraphics = configuration.GoogleMapSettings.UseSvgGraphics,
                    MinimumRefreshSeconds = configuration.GoogleMapSettings.MinimumRefreshSeconds,
                    RefreshSeconds = configuration.GoogleMapSettings.InitialRefreshSeconds,
                    VrsVersion = applicationInformation.ShortVersion,
                };
                if(_ServerConfigJson.GoogleMapsApiKey == "") {
                    _ServerConfigJson.GoogleMapsApiKey = null;
                }
                foreach(var receiver in configuration.Receivers) {
                    _ServerConfigJson.Receivers.Add(new ServerReceiverJson() {
                        UniqueId = receiver.UniqueId,
                        Name = receiver.Name,
                        HasPolarPlot = configuration.ReceiverLocations.Any(r => r.UniqueId == receiver.ReceiverLocationId),
                    });
                }
                foreach(var mergedFeed in configuration.MergedFeeds) {
                    _ServerConfigJson.Receivers.Add(new ServerReceiverJson() {
                        UniqueId = mergedFeed.UniqueId,
                        Name = mergedFeed.Name,
                        HasPolarPlot = false,
                    });
                }
            }
        }

        /// <summary>
        /// Translates from a Google map type to a JavaScript map type.
        /// </summary>
        /// <param name="mapType"></param>
        /// <returns></returns>
        private string GetMapType(string mapType)
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
        private string GetDistanceUnit(DistanceUnit distanceUnit)
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
        private string GetHeightUnit(HeightUnit heightUnit)
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
        private string GetSpeedUnit(SpeedUnit speedUnit)
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
