// Copyright © 2014 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that displays a map centred on a given set of coordinates and displaying
    /// a particular set of properties (satellite view, zoom level etc).
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    [ComVisibleAttribute(true)]
    public partial class MapControl : UserControl
    {
        #region Fields
        private bool _BrowserInitialised;
        private bool _IsMono;
        private bool _SuppressUpdates;
        #endregion

        #region Properties
        private double _Latitude;
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        [DefaultValue(0.0)]
        public double Latitude
        {
            get { return _Latitude; }
            set {
                if(_Latitude != value) {
                    _Latitude = value;
                    CopyPositionToBrowser();
                    OnLatitudeChanged(EventArgs.Empty);
                }
            }
        }

        private double _Longitude;
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        [DefaultValue(0.0)]
        public double Longitude
        {
            get { return _Longitude; }
            set {
                if(_Longitude != value) {
                    _Longitude = value;
                    CopyPositionToBrowser();
                    OnLongitudeChanged(EventArgs.Empty);
                }
            }
        }

        private string _MapType = "ROADMAP";
        /// <summary>
        /// Gets or sets the map type.
        /// </summary>
        [DefaultValue("ROADMAP")]
        public string MapType
        {
            get { return _MapType; }
            set {
                if(_MapType != value) {
                    _MapType = value;
                    CopyMapTypeToBrowser();
                    OnMapTypeChanged(EventArgs.Empty);
                }
            }
        }

        private int _ZoomLevel = 12;
        /// <summary>
        /// Gets or sets the zoom level.
        /// </summary>
        [DefaultValue(12)]
        public int ZoomLevel
        {
            get { return _ZoomLevel; }
            set {
                if(_ZoomLevel != value) {
                    _ZoomLevel = value;
                    CopyZoomLevelToBrowser();
                    OnZoomLevelChanged(EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised when the <see cref="Latitude"/> changes.
        /// </summary>
        public event EventHandler LatitudeChanged;

        /// <summary>
        /// Raises <see cref="LatitudeChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLatitudeChanged(EventArgs args)
        {
            if(LatitudeChanged != null) LatitudeChanged(this, args);
        }

        /// <summary>
        /// Raised when the <see cref="Longitude"/> changes.
        /// </summary>
        public event EventHandler LongitudeChanged;

        /// <summary>
        /// Raises <see cref="LongitudeChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLongitudeChanged(EventArgs args)
        {
            if(LongitudeChanged != null) LongitudeChanged(this, args);
        }

        /// <summary>
        /// Raised when the <see cref="MapType"/> changes.
        /// </summary>
        public event EventHandler MapTypeChanged;

        /// <summary>
        /// Raises <see cref="MapTypeChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnMapTypeChanged(EventArgs args)
        {
            if(MapTypeChanged != null) MapTypeChanged(this, args);
        }

        /// <summary>
        /// Raised when the <see cref="ZoomLevel"/> changes.
        /// </summary>
        public event EventHandler ZoomLevelChanged;

        /// <summary>
        /// Raises <see cref="ZoomLevelChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnZoomLevelChanged(EventArgs args)
        {
            if(ZoomLevelChanged != null) ZoomLevelChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MapControl()
        {
            // If this is running under Mono then the WebBrowser control is unavailable... there
            // is an implementation but you have to install some component to glue to Firefox and
            // even if you do install it you don't get any control over JavaScript, so it's useless
            // for our purposes. Unfortunately if we let it initialise it scrawls messages all over
            // stdout so we need to nip it in the bud before it gets started. However the test for
            // Mono won't work when this ctor is called from the Visual Studio designer so we need
            // to catch exceptions and ignore them.
            try {
                var runtime = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
                _IsMono = runtime.IsMono;
            } catch {
                _IsMono = false;
            }

            if(!_IsMono) {
                InitializeComponent();
            }
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                if(!_IsMono) {
                    webBrowser.ObjectForScripting = this;
                    webBrowser.DocumentText = ControlResources.MapControl_html;
                }
            }
        }
        #endregion

        #region CopyToBrowser
        /// <summary>
        /// Copies values from the properties to the browser.
        /// </summary>
        private void CopyToBrowser()
        {
            CopyPositionToBrowser();
            CopyMapTypeToBrowser();
            CopyZoomLevelToBrowser();
        }

        /// <summary>
        /// Copies the latitude and longitude to the browser.
        /// </summary>
        private void CopyPositionToBrowser()
        {
            if(_BrowserInitialised && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    webBrowser.Document.InvokeScript("setPositionFromForm", new object[] { Latitude, Longitude });
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }

        /// <summary>
        /// Copies the map type to the browser.
        /// </summary>
        private void CopyMapTypeToBrowser()
        {
            if(_BrowserInitialised && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    webBrowser.Document.InvokeScript("setMapTypeFromForm", new object[] { MapType });
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }

        /// <summary>
        /// Copies the zoom level from the properties to the browser.
        /// </summary>
        private void CopyZoomLevelToBrowser()
        {
            if(_BrowserInitialised && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    webBrowser.Document.InvokeScript("setZoomLevelFromForm", new object[] { ZoomLevel });
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }
        #endregion

        #region JavaScript helper methods - Browser*****
        /// <summary>
        /// Returns the globalised error message text when Google Maps can't be loaded.
        /// </summary>
        /// <returns></returns>
        public string BrowserGetFailedErrorText()
        {
            return Strings.CouldNotLoadGoogleMaps;
        }

        /// <summary>
        /// Returns the globalised name for the HIGHCONTRAST custom map type.
        /// </summary>
        /// <returns></returns>
        public string BrowserHighContrastText()
        {
            return Strings.ContrastMapTitle;
        }

        /// <summary>
        /// Returns the latitude that we're bound to.
        /// </summary>
        /// <returns></returns>
        public double BrowserGetLatitude()
        {
            return Latitude;
        }

        /// <summary>
        /// Returns the longitude that we're bound to.
        /// </summary>
        /// <returns></returns>
        public double BrowserGetLongitude()
        {
            return Longitude;
        }

        /// <summary>
        /// Returns the map type that we're bound to, or a suitable default if we're not bound.
        /// </summary>
        /// <returns></returns>
        public string BrowserGetMapType()
        {
            return MapType;
        }

        /// <summary>
        /// Returns the zoom level that we're bound to, or a suitable default if we're not bound.
        /// </summary>
        /// <returns></returns>
        public int BrowserGetZoomLevel()
        {
            return ZoomLevel;
        }

        /// <summary>
        /// Sets both the latitude and longitude in one operation.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void BrowserSetPosititon(double latitude, double longitude)
        {
            if(!_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    Latitude = latitude;
                    Longitude = longitude;
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }

        /// <summary>
        /// Sets the map type.
        /// </summary>
        /// <param name="mapType"></param>
        public void BrowserSetMapType(string mapType)
        {
            if(!_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    MapType = mapType;
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }

        /// <summary>
        /// Sets the zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void BrowserSetZoomLevel(int zoomLevel)
        {
            if(!_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    ZoomLevel = zoomLevel;
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called once the web browser has finished loading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _BrowserInitialised = true;
            CopyToBrowser();
        }
        #endregion
    }
}
