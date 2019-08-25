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
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.Settings;
using System.Globalization;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A page that can produce (mostly) static text content in response to requests from a browser.
    /// </summary>
    class TextPage : Page
    {
        /// <summary>
        /// A private class describing the content associated with a PathAndFile.
        /// </summary>
        class Content
        {
            public string Text;
            public string MimeType;
        }

        /// <summary>
        /// A map of files to content.
        /// </summary>
        private Dictionary<string, Content> _PathAndFileToContentMap = new Dictionary<string, Content>();

        /// <summary>
        /// True when running under the Mono runtime.
        /// </summary>
        private bool _IsMono;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TextPage(WebSite webSite) : base(webSite)
        {
            _IsMono = Factory.ResolveSingleton<IRuntimeEnvironment>().IsMono;
            RegisterStaticPages();
        }

        /// <summary>
        /// Records the static content that is served for each of the addresses that we can respond to. These pages never
        /// change over the lifetime of the website.
        /// </summary>
        private void RegisterStaticPages()
        {
            var googleMapContent = Resources.GoogleMap_htm.Replace("__MAP_MODE__", "normal");
            var flightSimContent = Resources.GoogleMap_htm.Replace("__MAP_MODE__", "flightSim");

            var dateReportContent = Resources.FlightReport_htm.Replace("%%NAME%%", "Date");
            var icaoReportContent = Resources.AircraftReport_htm.Replace("%%NAME%%", "Icao");
            var regReportContent = Resources.AircraftReport_htm.Replace("%%NAME%%", "Reg");

            ServePage("/AircraftList.htm",                         Resources.AircraftList_htm,                         MimeType.Html);
            ServePage("/AircraftList.html",                        Resources.AircraftList_htm,                         MimeType.Html);
            ServePage("/Common.js",                                Resources.Common_js,                                MimeType.Javascript);
            ServePage("/DateReport.htm",                           dateReportContent,                                  MimeType.Html);
            ServePage("/DateReport.html",                          dateReportContent,                                  MimeType.Html);
            ServePage("/DateReport.js",                            Resources.DateReport_js,                            MimeType.Javascript);
            ServePage("/DateReportCriteria.js",                    Resources.DateReportCriteria_js,                    MimeType.Javascript);
            ServePage("/Events.js",                                Resources.Events_js,                                MimeType.Javascript);
            ServePage("/FlightSim.htm",                            flightSimContent,                                   MimeType.Html);
            ServePage("/FlightSim.html",                           flightSimContent,                                   MimeType.Html);
            ServePage("/GoogleMap.htm",                            googleMapContent,                                   MimeType.Html);
            ServePage("/GoogleMap.html",                           googleMapContent,                                   MimeType.Html);
            ServePage("/GoogleMap.js",                             Resources.GoogleMap_js,                             MimeType.Javascript);
            ServePage("/GoogleMapAircraftCollection.js",           Resources.GoogleMapAircraftCollection_js,           MimeType.Javascript);
            ServePage("/GoogleMapAircraftDetail.js",               Resources.GoogleMapAircraftDetail_js,               MimeType.Javascript);
            ServePage("/GoogleMapAircraftList.js",                 Resources.GoogleMapAircraftList_js,                 MimeType.Javascript);
            ServePage("/GoogleMapAircraftListColumns.js",          Resources.GoogleMapAircraftListColumns_js,          MimeType.Javascript);
            ServePage("/GoogleMapAircraftListOptions.js",          Resources.GoogleMapAircraftListOptions_js,          MimeType.Javascript);
            ServePage("/GoogleMapAudio.js",                        Resources.GoogleMapAudio_js,                        MimeType.Javascript);
            ServePage("/GoogleMapAutoSelect.js",                   Resources.GoogleMapAutoSelect_js,                   MimeType.Javascript);
            ServePage("/GoogleMapCurrentLocation.js",              Resources.GoogleMapCurrentLocation_js,              MimeType.Javascript);
            ServePage("/GoogleMapGeolocation.js",                  Resources.GoogleMapGeolocation_js,                  MimeType.Javascript);
            ServePage("/GoogleMapGotoCurrentLocationButton.js",    Resources.GoogleMapGotoCurrentLocationButton_js,    MimeType.Javascript);
            ServePage("/GoogleMapInfoButton.js",                   Resources.GoogleMapInfoButton_js,                   MimeType.Javascript);
            ServePage("/GoogleMapMarker.js",                       Resources.GoogleMapMarker_js,                       MimeType.Javascript);
            ServePage("/GoogleMapMarkerCollection.js",             Resources.GoogleMapMarkerCollection_js,             MimeType.Javascript);
            ServePage("/GoogleMapMovingMapControl.js",             Resources.GoogleMapMovingMapControl_js,             MimeType.Javascript);
            ServePage("/GoogleMapOptions.js",                      Resources.GoogleMapOptions_js,                      MimeType.Javascript);
            ServePage("/GoogleMapOptionsUI.js",                    Resources.GoogleMapOptionsUI_js,                    MimeType.Javascript);
            ServePage("/GoogleMapOutline.js",                      Resources.GoogleMapOutline_js,                      MimeType.Javascript);
            ServePage("/GoogleMapReverseGeocode.js",               Resources.GoogleMapReverseGeocode_js,               MimeType.Javascript);
            ServePage("/GoogleMapSidebar.js",                      Resources.GoogleMapSidebar_js,                      MimeType.Javascript);
            ServePage("/GoogleMapStylesheet.css",                  Resources.GoogleMapStylesheet_css,                  MimeType.Css);
            ServePage("/GoogleMapTimeout.js",                      Resources.GoogleMapTimeout_js,                      MimeType.Javascript);
            ServePage("/GoogleMapVolumeControl.js",                Resources.GoogleMapVolumeControl_js,                MimeType.Javascript);
            ServePage("/MarkerWithLabel.min.js",                   Resources.MarkerWithLabel_min_js,                   MimeType.Javascript);
            ServePage("/jQuery-1.6.4.js",                          Resources.jQuery_1_6_4_js,                          MimeType.Javascript);
            ServePage("/jQuery-1.6.4.min.js",                      Resources.jQuery_1_6_4_min_js,                      MimeType.Javascript);
            ServePage("/IcaoReport.htm",                           icaoReportContent,                                  MimeType.Html);
            ServePage("/IcaoReport.html",                          icaoReportContent,                                  MimeType.Html);
            ServePage("/IcaoReport.js",                            Resources.IcaoReport_js,                            MimeType.Javascript);
            ServePage("/IcaoReportCriteria.js",                    Resources.IcaoReportCriteria_js,                    MimeType.Javascript);
            ServePage("/iPhoneMap.htm",                            Resources.iPhoneMap_htm,                            MimeType.Html);
            ServePage("/iPhoneMap.html",                           Resources.iPhoneMap_htm,                            MimeType.Html);
            ServePage("/iPhoneMapAircraftDetail.js",               Resources.iPhoneMapAircraftDetail_js,               MimeType.Javascript);
            ServePage("/iPhoneMapInfoWindow.js",                   Resources.iPhoneMapInfoWindow_js,                   MimeType.Javascript);
            ServePage("/iPhoneMapOptionsUI.js",                    Resources.iPhoneMapOptionsUI_js,                    MimeType.Javascript);
            ServePage("/iPhoneMapPages.js",                        Resources.iPhoneMapPages_js,                        MimeType.Javascript);
            ServePage("/iPhoneMapPlaneList.js",                    Resources.iPhoneMapPlaneList_js,                    MimeType.Javascript);
            ServePage("/iPhoneMapStylesheet.css",                  Resources.iPhoneMapStylesheet_css,                  MimeType.Css);
            ServePage("/RegReport.htm",                            regReportContent,                                   MimeType.Html);
            ServePage("/RegReport.html",                           regReportContent,                                   MimeType.Html);
            ServePage("/RegReport.js",                             Resources.RegReport_js,                             MimeType.Javascript);
            ServePage("/RegReportCriteria.js",                     Resources.RegReportCriteria_js,                     MimeType.Javascript);
            ServePage("/Report.js",                                Resources.Report_js,                                MimeType.Javascript);
            ServePage("/ReportAircraftDetail.js",                  Resources.ReportAircraftDetail_js,                  MimeType.Javascript);
            ServePage("/ReportAircraftFlights.js",                 Resources.ReportAircraftFlights_js,                 MimeType.Javascript);
            ServePage("/ReportCriteria.js",                        Resources.ReportCriteria_js,                        MimeType.Javascript);
            ServePage("/ReportFlights.js",                         Resources.ReportFlights_js,                         MimeType.Javascript);
            ServePage("/ReportFlightsDetail.js",                   Resources.ReportFlightsDetail_js,                   MimeType.Javascript);
            ServePage("/ReportMap.js",                             Resources.ReportMap_js,                             MimeType.Javascript);
            ServePage("/ReportPageControl.js",                     Resources.ReportPageControl_js,                     MimeType.Javascript);
            ServePage("/ReportPrintStylesheet.css",                Resources.ReportPrintStylesheet_css,                MimeType.Css);
            ServePage("/ReportRowProvider.js",                     Resources.ReportRowProvider_js,                     MimeType.Javascript);
            ServePage("/ReportScreenStylesheet.css",               Resources.ReportScreenStylesheet_css,               MimeType.Css);
            ServePage("/XHR.js",                                   Resources.XHR_js,                                   MimeType.Javascript);
        }

        /// <summary>
        /// Registers pages that contain values which have been substituted with configuration values.
        /// </summary>
        /// <param name="configuration"></param>
        private void RegisterConfigurablePages(Configuration configuration)
        {
            ServePage("/ServerConfig.js", SubstituteConfigurationOptions(Resources.ServerConfig_js, configuration), MimeType.Javascript);
        }

        /// <summary>
        /// Registers content against the path (from root) and file portion of a URL.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        private void ServePage(string pathAndFile, string content, string mimeType)
        {
            var key = pathAndFile.ToUpper();
            Content contentDto;
            if(!_PathAndFileToContentMap.TryGetValue(key, out contentDto)) _PathAndFileToContentMap.Add(key, new Content() { Text = content, MimeType = mimeType });
            else {
                contentDto.Text = content;
                contentDto.MimeType = mimeType;
            }
        }

        /// <summary>
        /// Replaces strings in the text passed across with values read from the configuration file.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private string SubstituteConfigurationOptions(string text, Configuration configuration)
        {
            var vrsVersion = Factory.Resolve<IApplicationInformation>().ShortVersion;
            return text.Replace("__INITIAL_LATITUDE",                   configuration.GoogleMapSettings.InitialMapLatitude.ToString(CultureInfo.InvariantCulture))
                       .Replace("__INITIAL_LONGITUDE",                  configuration.GoogleMapSettings.InitialMapLongitude.ToString(CultureInfo.InvariantCulture))
                       .Replace("__INITIAL_MAPTYPE",                    configuration.GoogleMapSettings.InitialMapType)
                       .Replace("__INITIAL_ZOOM",                       configuration.GoogleMapSettings.InitialMapZoom.ToString())
                       .Replace("__INITIAL_REFRESHSECONDS",             configuration.GoogleMapSettings.InitialRefreshSeconds.ToString())
                       .Replace("__MINIMUM_REFRESHSECONDS",             configuration.GoogleMapSettings.MinimumRefreshSeconds.ToString())
                       .Replace("__INTERNET_CLIENT_CAN_RUN_REPORTS",    configuration.InternetClientSettings.CanRunReports ? "true" : "false")
                       .Replace("__INTERNET_CLIENT_CAN_SHOW_PIN_TEXT",  configuration.InternetClientSettings.CanShowPinText ? "true" : "false")
                       .Replace("__INTERNET_CLIENT_TIMEOUT_MINUTES",    configuration.InternetClientSettings.TimeoutMinutes.ToString())
                       .Replace("__INTERNET_CLIENT_CAN_PLAY_AUDIO",     configuration.InternetClientSettings.CanPlayAudio ? "true" : "false")
                       .Replace("__AUDIO_ENABLED",                      configuration.AudioSettings.Enabled ? "true" : "false")
                       .Replace("__INITIAL_DISTANCE_UNIT",              String.Format("DistanceUnit.{0}", configuration.GoogleMapSettings.InitialDistanceUnit))
                       .Replace("__INITIAL_HEIGHT_UNIT",                String.Format("HeightUnit.{0}", configuration.GoogleMapSettings.InitialHeightUnit))
                       .Replace("__INITIAL_SPEED_UNIT",                 String.Format("SpeedUnit.{0}", configuration.GoogleMapSettings.InitialSpeedUnit))
                       .Replace("__IS_MONO",                            _IsMono ? "true" : "false")
                       .Replace("__INTERNET_CLIENT_CAN_SUBMIT_ROUTES",  configuration.InternetClientSettings.CanSubmitRoutes ? "true" : "false")
                       .Replace("__VRS_VERSION",                        vrsVersion)
                       ;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            bool result = false;

            var key = args.PathAndFile.ToUpper();
            Content content;
            result = _PathAndFileToContentMap.TryGetValue(key, out content);
            if(result) {
                string text = content.Text;
                if(key == "/SERVERCONFIG.JS") {
                    text = text.Replace("__IS_LOCAL_ADDRESS", args.IsInternetRequest ? "false" : "true");
                }

                Responder.SendText(args.Request, args.Response, text, Encoding.UTF8, content.MimeType);
                args.Classification = ContentClassification.Html;
            }

            return result;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            RegisterConfigurablePages(configuration);
        }
    }
}
