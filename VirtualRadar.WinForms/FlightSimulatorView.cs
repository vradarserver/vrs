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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using System.Diagnostics;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Implements <see cref="IFlightSimulatorView"/> using a WinForms dialog box.
    /// </summary>
    public partial class FlightSimulatorView : BaseForm, IFlightSimulatorView
    {
        #region Fields
        /// <summary>
        /// The presenter that's controlling this view.
        /// </summary>
        private IFlightSimulatorPresenter _Presenter;

        /// <summary>
        /// The object that handles the display of online help for us.
        /// </summary>
        private OnlineHelpHelper _OnlineHelp;

        // All of the objects that need to be passed on to the presenter when we finally get to create it.
        private IBaseStationAircraftList _BaseStationAircraftList;
        private ISimpleAircraftList _FlightSimulatorAircraftList;
        private IWebServer _WebServer;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanBeRefreshed { get { return !IsDisposed; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ConnectionStatus
        {
            get { return labelStatus.Text; }
            set { labelStatus.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ConnectToFlightSimulatorXEnabled
        {
            get { return buttonConnect.Enabled; }
            set { buttonConnect.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FlightSimulatorPageAddress
        {
            get { return linkLabelAddress.Text; }
            set { linkLabelAddress.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool RideAircraftEnabled
        {
            get { return buttonRideAircraft.Enabled; }
            set { buttonRideAircraft.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RideStatus
        {
            get { return labelRideStatus.Text; }
            set { labelRideStatus.Text = value; }
        }

        private bool _RidingAircraft;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool RidingAircraft
        {
            get { return _RidingAircraft; }
            set
            {
                _RidingAircraft = value;
                buttonRideAircraft.Text = value ? Strings.StopRidingAircraft : Strings.RideAircraft;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraft SelectedRealAircraft
        {
            get { return aircraftListControl.SelectedAircraft; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseSlewMode
        {
            get { return radioButtonSlewMethod.Checked; }
            set { radioButtonSlewMethod.Checked = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IntPtr WindowHandle
        {
            get { return Handle; }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CloseClicked;

        /// <summary>
        /// Raises <see cref="CloseClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCloseClicked(EventArgs args)
        {
            EventHelper.Raise(CloseClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectClicked;

        /// <summary>
        /// Raises <see cref="ConnectClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectClicked(EventArgs args)
        {
            EventHelper.Raise(ConnectClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler RefreshFlightSimulatorXInformation;

        /// <summary>
        /// Raises <see cref="RefreshFlightSimulatorXInformation"/>
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRefreshFlightSimulatorXAircraftListTicked(EventArgs args)
        {
            EventHelper.Raise(RefreshFlightSimulatorXInformation, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler RideAircraftClicked;

        /// <summary>
        /// Raises <see cref="RideAircraftClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRideAircraftClicked(EventArgs args)
        {
            EventHelper.Raise(RideAircraftClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StopRidingAircraftClicked;

        /// <summary>
        /// Raises <see cref="StopRidingAircraftClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStopRidingAircraftClicked(EventArgs args)
        {
            EventHelper.Raise(StopRidingAircraftClicked, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightSimulatorView() : base()
        {
            InitializeComponent();

            // This is a little tricky - the French translation for "Ride Aircraft" is quite long and to make the button
            // large enough to accommodate it would mean having "Ride Aircraft" swimming in a sea of empty button for
            // English users. So I'm going to measure the width of the text, compare it to the width of the button and
            // if it's not going to fit I'll make the button double-height so that it'll fit.
            using(var graphics = CreateGraphics()) {
                var textSize = graphics.MeasureString(Strings.RideAircraft, buttonRideAircraft.Font);
                if(textSize.Width > buttonRideAircraft.Width) buttonRideAircraft.Height = radioButtonSlewMethod.Bottom - buttonRideAircraft.Top;
            }
        }
        #endregion

        #region Initialise, InitialiseRealAircraftListDisplay, UpdateRealAircraftListDisplay
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="baseStationAircraftList"></param>
        /// <param name="flightSimulatorAircraftList"></param>
        /// <param name="webServer"></param>
        public void Initialise(IBaseStationAircraftList baseStationAircraftList, ISimpleAircraftList flightSimulatorAircraftList, IWebServer webServer)
        {
            _BaseStationAircraftList = baseStationAircraftList;
            _FlightSimulatorAircraftList = flightSimulatorAircraftList;
            _WebServer = webServer;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftList"></param>
        public void InitialiseRealAircraftListDisplay(IList<IAircraft> aircraftList)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { InitialiseRealAircraftListDisplay(aircraftList); }));
            else               aircraftListControl.InitialiseList(aircraftList);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftList"></param>
        public void UpdateRealAircraftListDisplay(IList<IAircraft> aircraftList)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { UpdateRealAircraftListDisplay(aircraftList); }));
            else               aircraftListControl.UpdateList(aircraftList);
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called once the form has been initialised but before it's on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsFlightSimulatorXDialog);

                _Presenter = Factory.Resolve<IFlightSimulatorPresenter>();
                _Presenter.FlightSimulatorAircraftList = _FlightSimulatorAircraftList;
                _Presenter.WebServer = _WebServer;
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// Called when the form is closing but before it has closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            OnCloseClicked(e);
        }

        /// <summary>
        /// Called for every message received by the window. The FSX messages are mixed in with these, if we have an FSX message then
        /// we should leave it to the presenter to handle and not do anything with it.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if(_Presenter == null || !_Presenter.IsSimConnectMessage(m)) base.WndProc(ref m);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            OnConnectClicked(e);
        }

        private void buttonRideAircraft_Click(object sender, EventArgs e)
        {
            if(RidingAircraft) OnStopRidingAircraftClicked(e);
            else               OnRideAircraftClicked(e);
        }

        private void linkLabelAddress_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabelAddress.Text);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            OnRefreshFlightSimulatorXAircraftListTicked(e);
        }
        #endregion
    }
}
