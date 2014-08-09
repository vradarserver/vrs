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
using System.ComponentModel;
using System.Linq.Expressions;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The web site configuration options (originally these were just Google Map settings but they
    /// expanded over time - unfortunately I can't change the class name without breaking backwards
    /// compatibility).
    /// </summary>
    public class GoogleMapSettings : INotifyPropertyChanged
    {
        private double _InitialMapLatitude;
        /// <summary>
        /// Gets or sets the initial latitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLatitude
        {
            get { return _InitialMapLatitude; }
            set { SetField(ref _InitialMapLatitude, value, () => InitialMapLatitude); }
        }

        private double _InitialMapLongitude;
        /// <summary>
        /// Gets or sets the initial longitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLongitude
        {
            get { return _InitialMapLongitude; }
            set { SetField(ref _InitialMapLongitude, value, () => InitialMapLongitude); }
        }

        private string _InitialMapType;
        /// <summary>
        /// Gets or sets the initial map type to use (terrain, satellite etc.). This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public string InitialMapType
        {
            get { return _InitialMapType; }
            set { SetField(ref _InitialMapType, value, () => InitialMapType); }
        }

        private int _InitialMapZoom;
        /// <summary>
        /// Gets or sets the initial level of zoom to use. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public int InitialMapZoom
        {
            get { return _InitialMapZoom; }
            set { SetField(ref _InitialMapZoom, value, () => InitialMapZoom); }
        }

        private int _InitialRefreshSeconds;
        /// <summary>
        /// Gets or sets the initial refresh period. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        /// <remarks>
        /// For historical reasons the browser always adds one second to whatever value it has been configured to use. Setting 0 here indicates a 1 second refresh period,
        /// a 1 is 2 seconds and so on.
        /// </remarks>
        public int InitialRefreshSeconds
        {
            get { return _InitialRefreshSeconds; }
            set { SetField(ref _InitialRefreshSeconds, value, () => InitialRefreshSeconds); }
        }

        private int _MinimumRefreshSeconds;
        /// <summary>
        /// Gets or sets the smallest refresh period that the browser will allow the user to set.
        /// </summary>
        /// <remarks>
        /// This setting is difficult to police in the server so it should just be taken as a hint to well-behaved code rather than a guarantee that the server will reject
        /// the second and subsequent request under this threshold.
        /// </remarks>
        public int MinimumRefreshSeconds
        {
            get { return _MinimumRefreshSeconds; }
            set { SetField(ref _MinimumRefreshSeconds, value, () => MinimumRefreshSeconds); }
        }

        private int _ShortTrailLengthSeconds;
        /// <summary>
        /// Gets or sets the number of seconds that short trails are to be stored for.
        /// </summary>
        /// <remarks>
        /// Short trails are lines connecting the current position of the aircraft to each coordinate it was at over the past NN seconds. This property holds the NN value.
        /// </remarks>
        public int ShortTrailLengthSeconds
        {
            get { return _ShortTrailLengthSeconds; }
            set { SetField(ref _ShortTrailLengthSeconds, value, () => ShortTrailLengthSeconds); }
        }

        private DistanceUnit _InitialDistanceUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display distances. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public DistanceUnit InitialDistanceUnit
        {
            get { return _InitialDistanceUnit; }
            set { SetField(ref _InitialDistanceUnit, value, () => InitialDistanceUnit); }
        }

        private HeightUnit _InitialHeightUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display heights. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public HeightUnit InitialHeightUnit
        {
            get { return _InitialHeightUnit; }
            set { SetField(ref _InitialHeightUnit, value, () => InitialHeightUnit); }
        }

        private SpeedUnit _InitialSpeedUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display speeds. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public SpeedUnit InitialSpeedUnit
        {
            get { return _InitialSpeedUnit; }
            set { SetField(ref _InitialSpeedUnit, value, () => InitialSpeedUnit); }
        }

        private bool _PreferIataAirportCodes;
        /// <summary>
        /// Gets or sets a value indicating that IATA codes should be used to describe airports whenever possible.
        /// </summary>
        public bool PreferIataAirportCodes
        {
            get { return _PreferIataAirportCodes; }
            set { SetField(ref _PreferIataAirportCodes, value, () => PreferIataAirportCodes); }
        }

        private bool _EnableBundling;
        /// <summary>
        /// Gets or sets a value indicating that the server should bundle multiple CSS and JavaScript files into a single download before serving them.
        /// </summary>
        public bool EnableBundling
        {
            get { return _EnableBundling; }
            set { SetField(ref _EnableBundling, value, () => EnableBundling); }
        }

        private bool _EnableMinifying;
        /// <summary>
        /// Gets or sets a value indicating that the server should minify CSS and JavaScript files before serving them.
        /// </summary>
        public bool EnableMinifying
        {
            get { return _EnableMinifying; }
            set { SetField(ref _EnableMinifying, value, () => EnableMinifying); }
        }

        private bool _EnableCompression;
        /// <summary>
        /// Gets or sets a value indicating that the server should compress responses.
        /// </summary>
        public bool EnableCompression
        {
            get { return _EnableCompression; }
            set { SetField(ref _EnableCompression, value, () => EnableCompression); }
        }

        private int _WebSiteReceiverId;
        /// <summary>
        /// Gets or sets the receiver to show to the user when they visit the web site.
        /// </summary>
        public int WebSiteReceiverId
        {
            get { return _WebSiteReceiverId; }
            set { SetField(ref _WebSiteReceiverId, value, () => WebSiteReceiverId); }
        }

        private string _DirectoryEntryKey;
        /// <summary>
        /// Gets or sets the key that directory entry requests must contain before the site will respond with directory entry information.
        /// </summary>
        public string DirectoryEntryKey
        {
            get { return _DirectoryEntryKey; }
            set { SetField(ref _DirectoryEntryKey, value, () => DirectoryEntryKey); }
        }

        private int _ClosestAircraftReceiverId;
        /// <summary>
        /// Gets or sets the receiver to use when the closest aircraft desktop widget asks for details of the closest aircraft.
        /// </summary>
        public int ClosestAircraftReceiverId
        {
            get { return _ClosestAircraftReceiverId; }
            set { SetField(ref _ClosestAircraftReceiverId, value, () => ClosestAircraftReceiverId); }
        }

        private int _FlightSimulatorXReceiverId;
        /// <summary>
        /// Gets or sets the receiver to use with the Flight Simulator X ride-along feature.
        /// </summary>
        public int FlightSimulatorXReceiverId
        {
            get { return _FlightSimulatorXReceiverId; }
            set { SetField(ref _FlightSimulatorXReceiverId, value, () => FlightSimulatorXReceiverId); }
        }

        private ProxyType _ProxyType;
        /// <summary>
        /// Gets or sets the type of proxy that the server is sitting behind.
        /// </summary>
        public ProxyType ProxyType
        {
            get { return _ProxyType; }
            set { SetField(ref _ProxyType, value, () => ProxyType); }
        }

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
        /// Sets the field's value and raises <see cref="PropertyChanged"/>.
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
