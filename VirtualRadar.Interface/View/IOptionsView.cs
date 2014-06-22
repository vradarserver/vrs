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
using VirtualRadar.Interface.Settings;
using System.IO.Ports;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for GUI objects that can display the configuration to the user and allow them to alter it.
    /// </summary>
    public interface IOptionsView : IView, IBusyView, IValidateView
    {
        #region Properties - General Options
        /// <summary>
        /// Gets or sets a value indicating that the user wants to automatically check for new versions.
        /// </summary>
        bool CheckForNewVersions { get; set; }

        /// <summary>
        /// Gets or sets the number of days to wait between automatic checks for new versions.
        /// </summary>
        int CheckForNewVersionsPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that updates to the flight routes should be downloaded automatically.
        /// </summary>
        bool DownloadFlightRoutes { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to wait before aircraft that are no longer being picked up by the radio
        /// are taken off the browser's display.
        /// </summary>
        int DisplayTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to wait before aircraft that are no longer being picked up by the radio
        /// are removed from the aircraft list.
        /// </summary>
        int TrackingTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that a short trail should span.
        /// </summary>
        int ShortTrailLengthSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the application should be minimised to the system tray.
        /// </summary>
        bool MinimiseToSystemTray { get; set; }

        /// <summary>
        /// Gets a list of the settings for the rebroadcast servers.
        /// </summary>
        IList<RebroadcastSettings> RebroadcastSettings { get; }
        #endregion

        #region Properties - BaseStation Options
        /// <summary>
        /// Gets or sets the full path to the BaseStation database to read aircraft details from.
        /// </summary>
        string BaseStationDatabaseFileName { get; set; }

        /// <summary>
        /// Gets or sets the folder containing the operator flag images to display.
        /// </summary>
        string OperatorFlagsFolder { get; set; }

        /// <summary>
        /// Gets or sets the folder containing the aircraft silhouette images to display.
        /// </summary>
        string SilhouettesFolder { get; set; }

        /// <summary>
        /// Gets or sets the folder containing the aircraft pictures to display.
        /// </summary>
        string PicturesFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sub-folders of <see cref="PicturesFolder"/> should be searched for pictures.
        /// </summary>
        bool SearchPictureSubFolders { get; set; }

        /// <summary>
        /// Gets the list of receivers configured by the user.
        /// </summary>
        IList<Receiver> Receivers { get; }

        /// <summary>
        /// Gets the list of merged feeds configured by the user.
        /// </summary>
        IList<MergedFeed> MergedFeeds { get; }
        #endregion

        #region Properties - Raw Decoding
        /// <summary>
        /// Gets the collection of all known receiver locations defined by the user.
        /// </summary>
        IList<ReceiverLocation> RawDecodingReceiverLocations { get; }

        /// <summary>
        /// Gets or sets the radius range of the receiver in kilometres.
        /// </summary>
        int RawDecodingReceiverRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that DF19/AF0 is to be interpretted as an extended squitter message.
        /// </summary>
        bool RawDecodingIgnoreMilitaryExtendedSquitter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the range check should be disabled when a receiver location is supplied.
        /// </summary>
        bool RawDecodingSuppressReceiverRangeCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the initial fix should be made using a local decode instead of a global odd/even
        /// frame within 10/25/50 seconds.
        /// </summary>
        bool RawDecodingUseLocalDecodeForInitialPosition { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on airborne position messages.
        /// </summary>
        int RawDecodingAirborneGlobalPositionLimit { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling over 25 km/h.
        /// </summary>
        int RawDecodingFastSurfaceGlobalPositionLimit { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling at or under 25 km/h.
        /// </summary>
        int RawDecodingSlowSurfaceGlobalPositionLimit { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while airborne over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        double RawDecodingAcceptableAirborneSpeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while landing or taking off over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        double RawDecodingAcceptableAirSurfaceTransitionSpeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of kilometres an surface vehicle can travel over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        double RawDecodingAcceptableSurfaceSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that callsigns extracted from what might be a BDS2,0 message are to be ignored. Does not affect extraction of callsigns from ADS-B messages.
        /// </summary>
        bool RawDecodingIgnoreCallsignsInBds20 { get; set; }

        /// <summary>
        /// Gets or sets the number of times an ICAO has to be seen in messages that don't have parity before it gets used.
        /// </summary>
        int AcceptIcaoInNonPICount { get; set; }

        /// <summary>
        /// Gets or sets the period of time over which an ICAO has to be seen in messages that don't have parity before it will get used.
        /// </summary>
        int AcceptIcaoInNonPISeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of times an ICAO has to be seen in messages that have parity before it gets used.
        /// </summary>
        int AcceptIcaoInPI0Count { get; set; }

        /// <summary>
        /// Gets or sets the period of time over which an ICAO has to be seen in messages that have parity before it will get used.
        /// </summary>
        int AcceptIcaoInPI0Seconds { get; set; }
        #endregion

        #region Properties - WebServer Options
        /// <summary>
        /// Gets or sets a value indicating that only authenticated browsers can be served any content.
        /// </summary>
        bool WebServerUserMustAuthenticate { get; set; }

        /// <summary>
        /// Gets a collection of identifiers of users that are allowed to attach to the web server when
        /// <see cref="WebServerUserMustAuthenticate"/> is true.
        /// </summary>
        IList<string> WebServerUserIds { get; }

        /// <summary>
        /// Gets or sets a value indicating that the user wants the server to use UPnP to get the server onto and off the Internet.
        /// </summary>
        bool EnableUPnpFeatures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server can assume that no other instances of VRS are running on the LAN.
        /// </summary>
        bool IsOnlyVirtualRadarServerOnLan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should be put onto the Internet with UPnP when the program starts.
        /// </summary>
        bool AutoStartUPnp { get; set; }

        /// <summary>
        /// Gets or sets the external port number that the UPnP feature will expose the server on.
        /// </summary>
        int UPnpPort { get; set; }
        #endregion

        #region Properties - Web site / Google map values
        /// <summary>
        /// Gets or sets the latitude to centre the Google map on when a browser displays the map for the very first time.
        /// </summary>
        double InitialGoogleMapLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude to centre the Google map on when a browser displays the map for the very first time.
        /// </summary>
        double InitialGoogleMapLongitude { get; set; }

        /// <summary>
        /// Gets or sets the type of Google map to use when a browser displays the map for the very first time.
        /// </summary>
        string InitialGoogleMapType { get; set; }

        /// <summary>
        /// Gets or sets the level of zoom to use when a browser displays the map for the very first time.
        /// </summary>
        int InitialGoogleMapZoom { get; set; }

        /// <summary>
        /// Gets or sets the pause between refreshes to default to when a browser displays the map for the very first time.
        /// </summary>
        int InitialGoogleMapRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the lowest pause between refreshes that browsers should honour when displaying the Google map page.
        /// </summary>
        int MinimumGoogleMapRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the distances to use when a browser displays the map for the very first time.
        /// </summary>
        DistanceUnit InitialDistanceUnit { get; set; }

        /// <summary>
        /// Gets or sets the heights to use when a browser displays the map for the very first time.
        /// </summary>
        HeightUnit InitialHeightUnit { get; set; }

        /// <summary>
        /// Gets or sets the speeds to use when a browser displays the map for the very first time.
        /// </summary>
        SpeedUnit InitialSpeedUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that IATA airport codes should be used in preference to ICAO codes.
        /// </summary>
        bool PreferIataAirportCodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should bundle multiple CSS and JavaScript files into a single download before serving them.
        /// </summary>
        bool EnableBundling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should minify CSS and JavaScript files before serving them.
        /// </summary>
        bool EnableMinifying { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should compress responses.
        /// </summary>
        bool EnableCompression { get; set; }

        /// <summary>
        /// Gets or sets the feed whose aircraft list will initially be shown to browsers.
        /// </summary>
        int WebSiteReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the feed whose aircraft list will be used when the closest aircraft desktop widget asks for details of the closest aircraft.
        /// </summary>
        int ClosestAircraftReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the feed whose aircraft list will be used to drive the Flight Simulator X ride-along feature.
        /// </summary>
        int FlightSimulatorXReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the type of proxy that the server is behind.
        /// </summary>
        ProxyType ProxyType { get; set; }
        #endregion

        #region Properties - Internet Client Options
        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can run reports.
        /// </summary>
        bool InternetClientCanRunReports { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can be sent audio.
        /// </summary>
        bool InternetClientCanPlayAudio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can be shown aircraft pictures.
        /// </summary>
        bool InternetClientCanSeePictures { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes of inactivity that must elapse before the site stops refreshing automatically.
        /// </summary>
        int InternetClientTimeoutMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can see text labels on the aircraft markers.
        /// </summary>
        bool InternetClientCanSeeLabels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that proximity gadgets connecting from the Internet are supported.
        /// </summary>
        bool AllowInternetProximityGadgets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can see links to submit routes.
        /// </summary>
        bool InternetClientCanSubmitRoutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that clients connecting from the Internet can see polar plots.
        /// </summary>
        bool InternetClientCanShowPolarPlots { get; set; }
        #endregion

        #region Properties - Audio Options
        /// <summary>
        /// Gets or sets a value indicating that the audio options should be enabled.
        /// </summary>
        bool AudioEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the voice to use when converting text to speech. If this is null then the default
        /// voice should be used.
        /// </summary>
        string TextToSpeechVoice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the relative speed of speech playback - 0 is normal speed, -50 is 50% slower,
        /// 50 is 50% faster etc.
        /// </summary>
        int TextToSpeechSpeed { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user wants to reset all values to factory defaults.
        /// </summary>
        event EventHandler ResetToDefaultsClicked;

        /// <summary>
        /// Raised when the user wants to save their changes.
        /// </summary>
        event EventHandler SaveClicked;

        /// <summary>
        /// Raised when the user indicates that they want to see if their current data feed settings will connect to something.
        /// </summary>
        event EventHandler<EventArgs<Receiver>> TestConnectionClicked;

        /// <summary>
        /// Raised when the user wants to test the speech synthesis settings.
        /// </summary>
        event EventHandler TestTextToSpeechSettingsClicked;

        /// <summary>
        /// Raised when the user indicates that they want to refresh the content of the ReceiverLocations list with
        /// locations from the BaseStation database.
        /// </summary>
        event EventHandler UpdateReceiverLocationsFromBaseStationDatabaseClicked;

        /// <summary>
        /// Raised when the user wants to use the ICAO specification settings for the raw decoder.
        /// </summary>
        event EventHandler UseIcaoRawDecodingSettingsClicked;

        /// <summary>
        /// Raised when the user wants to use the default values for the raw decoder.
        /// </summary>
        event EventHandler UseRecommendedRawDecodingSettingsClicked;

        /// <summary>
        /// Raised when values have been changed and it may be appropriate to retry validation.
        /// </summary>
        event EventHandler ValueChanged;
        #endregion

        #region Methods
        /// <summary>
        /// The names of every voice installed on the system.
        /// </summary>
        /// <param name="voiceNames"></param>
        void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames);

        /// <summary>
        /// Forces the redisplay of the content of <see cref="RawDecodingReceiverLocations"/> collection.
        /// </summary>
        void MergeBaseStationDatabaseReceiverLocations(IEnumerable<ReceiverLocation> receiverLocations);

        /// <summary>
        /// Displays the results of a BaseStation connection test attempt to the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowTestConnectionResults(string message, string title);
        #endregion
    }
}
