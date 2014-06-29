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
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// Displays a map in a browser with a pin on it. When the pin moves the location is updated.
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    [ComVisibleAttribute(true)]
    public partial class LocationMapControl : UserControl
    {
        private bool _SuppressSendPositionToBrowser;
        private bool _BrowserInitialised;
        private bool _IsMono;

        [DefaultValue(0.0)]
        public double Latitude
        {
            get { return (double)numericLatitude.Value; }
            set {
                if(value != Latitude) {
                    numericLatitude.Value = (decimal)value;
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0.0)]
        public double Longitude
        {
            get { return (double)numericLongitude.Value; }
            set {
                if(value != Longitude) {
                    numericLongitude.Value = (decimal)value;
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs args)
        {
            if(ValueChanged != null) ValueChanged(this, args);
        }

        public LocationMapControl()
        {
            _IsMono = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                if(_IsMono) webBrowser.Visible = false;
                else        LoadBrowser();
            }
        }

        private void LoadBrowser()
        {
            webBrowser.ObjectForScripting = this;
            webBrowser.DocumentText = ControlResources.LocationMapControl_html;
        }

        public string BrowserGetFailedErrorText()
        {
            return Strings.CouldNotLoadGoogleMaps;
        }

        public double BrowserGetLatitude()
        {
            return Latitude;
        }

        public double BrowserGetLongitude()
        {
            return Longitude;
        }

        public void BrowserSetPosititon(double latitude, double longitude)
        {
            var suppress = _SuppressSendPositionToBrowser;
            try {
                _SuppressSendPositionToBrowser = true;
                Latitude = latitude;
                Longitude = longitude;
            } finally {
                _SuppressSendPositionToBrowser = suppress;
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _BrowserInitialised = true;
        }

        private void numeric_ValueChanged(object sender, EventArgs e)
        {
            if(!_SuppressSendPositionToBrowser && _BrowserInitialised) {
                webBrowser.Document.InvokeScript("setPositionFromForm", new object[] { Latitude, Longitude });
            }
        }
    }
}
