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
using System.Text;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The configuration options for the Google Maps map on the website.
    /// </summary>
    public class GoogleMapSettings
    {
        /// <summary>
        /// Gets or sets the initial latitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLatitude { get; set; }

        /// <summary>
        /// Gets or sets the initial longitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLongitude { get; set; }

        /// <summary>
        /// Gets or sets the initial map type to use (terrain, satellite etc.). This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public string InitialMapType { get; set; }

        /// <summary>
        /// Gets or sets the initial level of zoom to use. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public int InitialMapZoom { get; set; }

        /// <summary>
        /// Gets or sets the initial refresh period. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        /// <remarks>
        /// For historical reasons the browser always adds one second to whatever value it has been configured to use. Setting 0 here indicates a 1 second refresh period,
        /// a 1 is 2 seconds and so on.
        /// </remarks>
        public int InitialRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the smallest refresh period that the browser will allow the user to set.
        /// </summary>
        /// <remarks>
        /// This setting is difficult to police in the server so it should just be taken as a hint to well-behaved code rather than a guarantee that the server will reject
        /// the second and subsequent request under this threshold.
        /// </remarks>
        public int MinimumRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds that short trails are to be stored for.
        /// </summary>
        /// <remarks>
        /// Short trails are lines connecting the current position of the aircraft to each coordinate it was at over the past NN seconds. This property holds the NN value.
        /// </remarks>
        public int ShortTrailLengthSeconds { get; set; }

        /// <summary>
        /// Gets or sets the initial unit used to display distances. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public DistanceUnit InitialDistanceUnit { get; set; }

        /// <summary>
        /// Gets or sets the initial unit used to display heights. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public HeightUnit InitialHeightUnit { get; set; }

        /// <summary>
        /// Gets or sets the initial unit used to display speeds. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public SpeedUnit InitialSpeedUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that IATA codes should be used to describe airports whenever possible.
        /// </summary>
        public bool PreferIataAirportCodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should bundle multiple CSS and JavaScript files into a single download before serving them.
        /// </summary>
        public bool EnableBundling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should minify CSS and JavaScript files before serving them.
        /// </summary>
        public bool EnableMinifying { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server should compress responses.
        /// </summary>
        public bool EnableCompression { get; set; }

        /// <summary>
        /// Gets or sets the receiver to show to the user when they visit the web site.
        /// </summary>
        public int WebSiteReceiverId { get; set; }

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
        /// Creates a new object.
        /// </summary>
        public GoogleMapSettings()
        {
            InitialMapLatitude = 51.47;
            InitialMapLongitude = -0.6;
            InitialMapType = "ROADMAP";
            InitialMapZoom = 11;
            InitialRefreshSeconds = MinimumRefreshSeconds = 1;
            ShortTrailLengthSeconds = 30;
            InitialDistanceUnit = DistanceUnit.NauticalMiles;
            InitialHeightUnit = HeightUnit.Feet;
            InitialSpeedUnit = SpeedUnit.Knots;
            EnableBundling = true;
            EnableMinifying = true;
            EnableCompression = true;
        }
    }
}
