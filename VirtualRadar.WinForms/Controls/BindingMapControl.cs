using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A control that can display a Google map and be bound to latitude
    /// and longitude values.
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    [ComVisibleAttribute(true)]
    public partial class BindingMapControl : UserControl
    {
        #region Fields
        private CurrencyManager _CurrencyManager;
        private bool _CurrencyManagerHooked;
        private bool _BrowserInitialised;
        private bool _IsMono;
        #endregion

        #region Properties
        private bool _BindMapType;
        /// <summary>
        /// Gets or sets a value indicating that the map type should be bound to the map.
        /// </summary>
        [DefaultValue(false)]
        public bool BindMapType
        {
            get { return _BindMapType; }
            set {
                if(_BindMapType != value) {
                    _BindMapType = value;
                    SetBinding();
                }
            }
        }

        private bool _BindZoomLevel;
        /// <summary>
        /// Gets or sets a value indicating that the zoom level should be bound to the map.
        /// </summary>
        [DefaultValue(false)]
        public bool BindZoomLevel
        {
            get { return _BindZoomLevel; }
            set {
                if(_BindZoomLevel != value) {
                    _BindZoomLevel = value;
                    SetBinding();
                }
            }
        }

        private object _DataSource;
        /// <summary>
        /// Gets or sets the source of the latitude and longitude values.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get { return _DataSource; }
            set {
                if(_DataSource != value) {
                    _DataSource = value;
                    SetBinding();
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        private string _LatitudeMember;
        /// <summary>
        /// Gets or sets the name of the latitude member to bind to.
        /// </summary>
        public string LatitudeMember
        {
            get { return _LatitudeMember; }
            set { 
                if(_LatitudeMember != value) {
                    _LatitudeMember = value; 
                    SetBinding();
                }
            }
        }

        /// <summary>
        /// Gets or sets the latitude that we're bound to.
        /// </summary>
        private double Latitude
        {
            get {
                var result = 0.0;
                if(_CurrencyManager != null) {
                    try {
                        var value = _CurrencyManager.GetItemProperties().Find(LatitudeMember, false).GetValue(_CurrencyManager.Current);
                        result = Convert.ToDouble(value);
                    } catch {
                        ;
                    }
                }
                return result;
            }
            set {
                if(_CurrencyManager != null) {
                    try {
                        _CurrencyManager.GetItemProperties().Find(LatitudeMember, false).SetValue(_CurrencyManager.Current, value);
                    } catch {
                        ;
                    }
                }
            }
        }

        private string _LongitudeMember;
        /// <summary>
        /// Gets or sets the name of the longitude member to bind to.
        /// </summary>
        public string LongitudeMember
        {
            get { return _LongitudeMember; }
            set {
                if(_LongitudeMember != value) {
                    _LongitudeMember = value;
                    SetBinding();
                }
            }
        }

        /// <summary>
        /// Gets or sets the longitude that we're bound to.
        /// </summary>
        private double Longitude
        {
            get {
                var result = 0.0;
                if(_CurrencyManager != null) {
                    try {
                        var value = _CurrencyManager.GetItemProperties().Find(LongitudeMember, false).GetValue(_CurrencyManager.Current);
                        result = Convert.ToDouble(value);
                    } catch {
                    }
                }
                return result;
            }
            set {
                if(_CurrencyManager != null) {
                    try {
                        _CurrencyManager.GetItemProperties().Find(LongitudeMember, false).SetValue(_CurrencyManager.Current, value);
                    } catch {
                    }
                }
            }
        }

        private string _MapTypeMember;
        /// <summary>
        /// Gets or sets the name of the map type member to bind to. Only used if <see cref="BindMapType"/> is true.
        /// </summary>
        public string MapTypeMember
        {
            get { return _MapTypeMember; }
            set {
                if(_MapTypeMember != value) {
                    _MapTypeMember = value;
                    SetBinding();
                }
            }
        }

        /// <summary>
        /// Gets or sets the map type that we're bound to.
        /// </summary>
        private string MapType
        {
            get {
                var result = "ROADMAP";
                if(_CurrencyManager != null && BindMapType) {
                    try {
                        var value = _CurrencyManager.GetItemProperties().Find(MapTypeMember, false).GetValue(_CurrencyManager.Current);
                        result = (value ?? "").ToString();
                    } catch {
                    }
                }
                return result;
            }
            set {
                if(_CurrencyManager != null && BindMapType) {
                    try {
                        _CurrencyManager.GetItemProperties().Find(MapTypeMember, false).SetValue(_CurrencyManager.Current, value);
                    } catch {
                    }
                }
            }
        }

        private string _ZoomLevelMember;
        /// <summary>
        /// Gets or sets the name of the zoom level member to bind to. Only used if <see cref="BindZoomLevel"/> is true.
        /// </summary>
        public string ZoomLevelMember
        {
            get { return _ZoomLevelMember; }
            set {
                if(_ZoomLevelMember != value) {
                    _ZoomLevelMember = value;
                    SetBinding();
                }
            }
        }

        /// <summary>
        /// Gets or sets the zoom level that we're bound to.
        /// </summary>
        private int ZoomLevel
        {
            get {
                var result = 10;
                if(_CurrencyManager != null && BindZoomLevel) {
                    try {
                        var value = _CurrencyManager.GetItemProperties().Find(ZoomLevelMember, false).GetValue(_CurrencyManager.Current);
                        result = Convert.ToInt32(value);
                    } catch {
                    }
                }
                return result;
            }
            set {
                if(_CurrencyManager != null && BindZoomLevel) {
                    try {
                        _CurrencyManager.GetItemProperties().Find(ZoomLevelMember, false).SetValue(_CurrencyManager.Current, value);
                    } catch {
                    }
                }
            }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised when the <see cref="DataSource"/> is changed.
        /// </summary>
        public event EventHandler DataSourceChanged;

        /// <summary>
        /// Raises <see cref="DataSourceChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDataSourceChanged(EventArgs args)
        {
            if(DataSourceChanged != null) DataSourceChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BindingMapControl()
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

        #region SetBinding, HookDataSource, UnhookDataSource
        /// <summary>
        /// Attaches events to the binding objects.
        /// </summary>
        private void SetBinding()
        {
            var membersPresent = LatitudeMember != null && LongitudeMember != null;
            if(BindMapType) membersPresent = membersPresent && MapTypeMember != null;
            if(BindZoomLevel) membersPresent = membersPresent && ZoomLevelMember != null;

            if(_DataSource == null || !membersPresent) {
                UnhookBindingEvents();
                _CurrencyManager = null;
            } else {
                var reloadPosition = false;
                var currencyManager = DataSource == null ? null : BindingContext[DataSource] as CurrencyManager;

                if(currencyManager != _CurrencyManager) {
                    UnhookBindingEvents();
                    _CurrencyManager = currencyManager;
                    if(_CurrencyManager != null) {
                        HookBindingEvents();
                        reloadPosition = true;
                    }
                }

                if(reloadPosition) {
                    SendBoundValuesToBrowser();
                }
            }
        }

        /// <summary>
        /// Hooks binding events.
        /// </summary>
        private void HookBindingEvents()
        {
            if(_CurrencyManager != null && !_CurrencyManagerHooked) {
                _CurrencyManagerHooked = true;
                _CurrencyManager.CurrentChanged += CurrencyManager_CurrentChanged;
                _CurrencyManager.CurrentItemChanged += CurrencyManager_CurrentItemChanged;
            }
        }

        /// <summary>
        /// Unhooks binding events.
        /// </summary>
        private void UnhookBindingEvents()
        {
            if(_CurrencyManager != null && _CurrencyManagerHooked) {
                _CurrencyManagerHooked = false;
                _CurrencyManager.CurrentChanged -= CurrencyManager_CurrentChanged;
                _CurrencyManager.CurrentItemChanged -= CurrencyManager_CurrentItemChanged;
            }
        }
        #endregion

        #region Event overrides
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                if(!_IsMono) {
                    LoadBrowser();
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            base.OnBindingContextChanged(e);
            SetBinding();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            base.OnParentBindingContextChanged(e);
            SetBinding();
        }
        #endregion

        #region LoadBrowser, SendPositionToBrowser
        /// <summary>
        /// Loads the script into the browser.
        /// </summary>
        private void LoadBrowser()
        {
            webBrowser.ObjectForScripting = this;
            webBrowser.DocumentText = ControlResources.BindingMapControl_html;
        }

        /// <summary>
        /// Sends the current position to the browser.
        /// </summary>
        private void SendBoundValuesToBrowser()
        {
            UnhookBindingEvents();
            try {
                if(_BrowserInitialised && _CurrencyManager != null) {
                    webBrowser.Document.InvokeScript("setPositionFromForm", new object[] { Latitude, Longitude });
                    if(BindMapType) {
                        webBrowser.Document.InvokeScript("setMapTypeFromForm", new object[] { MapType });
                    }
                    if(BindZoomLevel) {
                        webBrowser.Document.InvokeScript("setZoomLevelFromForm", new object[] { ZoomLevel });
                    }
                }
            } finally {
                HookBindingEvents();
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
            UnhookBindingEvents();
            try {
                Latitude = latitude;
                Longitude = longitude;
            } finally {
                HookBindingEvents();
            }
        }

        /// <summary>
        /// Sets the map type.
        /// </summary>
        /// <param name="mapType"></param>
        public void BrowserSetMapType(string mapType)
        {
            UnhookBindingEvents();
            try {
                MapType = mapType;
            } finally {
                HookBindingEvents();
            }
        }

        /// <summary>
        /// Sets the zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void BrowserSetZoomLevel(int zoomLevel)
        {
            UnhookBindingEvents();
            try {
                ZoomLevel = zoomLevel;
            } finally {
                HookBindingEvents();
            }
        }
        #endregion

        #region Events subscribed
        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _BrowserInitialised = true;
            SendBoundValuesToBrowser();
        }

        private void CurrencyManager_CurrentChanged(object sender, EventArgs args)
        {
            SendBoundValuesToBrowser();
        }

        private void CurrencyManager_CurrentItemChanged(object sender, EventArgs args)
        {
            SendBoundValuesToBrowser();
        }
        #endregion
    }
}
