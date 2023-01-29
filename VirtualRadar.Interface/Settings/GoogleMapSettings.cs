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
    /// The web site configuration options (originally these were just Google Map settings but they
    /// expanded over time - unfortunately I can't change the class name without breaking backwards
    /// compatibility).
    /// </summary>
    public class GoogleMapSettings
    {
        /// <summary>
        /// Gets or sets the map provider to use.
        /// </summary>
        public MapProvider MapProvider { get; set; } = MapProvider.Leaflet;

        /// <summary>
        /// Gets or sets the initial settings to use for new visitors.
        /// </summary>
        public string InitialSettings { get; set; }

        /// <summary>
        /// Gets or sets the initial latitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLatitude { get; set; } = 51.47;

        /// <summary>
        /// Gets or sets the initial longitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLongitude { get; set; } = -0.6;

        /// <summary>
        /// Gets or sets the initial map type to use (terrain, satellite etc.). This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public string InitialMapType { get; set; } = "ROADMAP";

        /// <summary>
        /// Gets or sets the initial level of zoom to use. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public int InitialMapZoom { get; set; } = 11;

        /// <summary>
        /// Gets or sets the initial refresh period. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        /// <remarks>
        /// For historical reasons the browser always adds one second to whatever value it has been configured to use. Setting 0 here indicates a 1 second refresh period,
        /// a 1 is 2 seconds and so on.
        /// </remarks>
        public int InitialRefreshSeconds { get; set; } = 1;

        /// <summary>
        /// Gets or sets the smallest refresh period that the browser will allow the user to set.
        /// </summary>
        /// <remarks>
        /// This setting is difficult to police in the server so it should just be taken as a hint to well-behaved code rather than a guarantee that the server will reject
        /// the second and subsequent request under this threshold.
        /// </remarks>
        public int MinimumRefreshSeconds { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of seconds that short trails are to be stored for.
        /// </summary>
        /// <remarks>
        /// Short trails are lines connecting the current position of the aircraft to each coordinate it was at over the past NN seconds. This property holds the NN value.
        /// </remarks>
        public int ShortTrailLengthSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the initial unit used to display distances. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public DistanceUnit InitialDistanceUnit { get; set; } = DistanceUnit.NauticalMiles;

        /// <summary>
        /// Gets or sets the initial unit used to display heights. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public HeightUnit InitialHeightUnit { get; set; } = HeightUnit.Feet;

        /// <summary>
        /// Gets or sets the initial unit used to display speeds. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public SpeedUnit InitialSpeedUnit { get; set; } = SpeedUnit.Knots;

        /// <summary>
        /// Gets or sets a value indicating that IATA codes should be used to describe airports whenever possible.
        /// </summary>
        public bool PreferIataAirportCodes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that the server should bundle multiple CSS and JavaScript files into a single download before serving them.
        /// </summary>
        public bool EnableBundling { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that the server should minify CSS and JavaScript files before serving them.
        /// </summary>
        public bool EnableMinifying { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that the server should compress responses.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Gets or sets the receiver to show to the user when they visit the web site.
        /// </summary>
        public int WebSiteReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the key that directory entry requests must contain before the site will respond with directory entry information.
        /// </summary>
        public string DirectoryEntryKey { get; set; }

        /// <summary>
        /// Gets or sets the receiver to use when the closest aircraft desktop widget asks for details of the closest aircraft.
        /// </summary>
        public int ClosestAircraftReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the receiver to use with the Flight Simulator X ride-along feature.
        /// </summary>
        public int FlightSimulatorXReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the type of proxy that the server is sitting behind.
        /// </summary>
        public ProxyType ProxyType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the web site should respond to the CORS OPTIONS request.
        /// </summary>
        public bool EnableCorsSupport { get; set; }

        /// <summary>
        /// Gets or sets a semi-colon separated list of domains that can access the server's content via CORS.
        /// </summary>
        public string AllowCorsDomains { get; set; }

        /// <summary>
        /// Gets or sets the Google Maps API key to use with the site.
        /// </summary>
        public string GoogleMapsApiKey { get; set; }

        /// <summary>
        /// True if the Google Maps API key should be used for requests from local addresses. This should
        /// only be switched on if the installation is behind a proxy.
        /// </summary>
        public bool UseGoogleMapsAPIKeyWithLocalRequests { get; set; }

        /// <summary>
        /// Gets or sets the name of the tile server setting to use with map providers that use tile servers.
        /// </summary>
        public string TileServerSettingName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on desktop pages.
        /// </summary>
        public bool UseSvgGraphicsOnDesktop { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on mobile pages.
        /// </summary>
        public bool UseSvgGraphicsOnMobile { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on report pages.
        /// </summary>
        public bool UseSvgGraphicsOnReports { get; set; } = true;
    }
}
