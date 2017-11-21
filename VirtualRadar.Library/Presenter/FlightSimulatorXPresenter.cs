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
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.FlightSimulatorX;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implmentation of <see cref="IFlightSimulatorXPresenter"/>.
    /// </summary>
    class FlightSimulatorXPresenter : IFlightSimulatorXPresenter
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : IFlightSimulatorXPresenterProvider
        {
            class BackgroundState
            {
                public Action CallbackMethod;
                public int Milliseconds;
            }

            public void TimedInvokeOnBackgroundThread(Action callback, int milliseconds)
            {
                ThreadPool.QueueUserWorkItem(BackgroundThreadWorker, new BackgroundState() { CallbackMethod = callback, Milliseconds = milliseconds });
            }

            private void BackgroundThreadWorker(object state)
            {
                var backgroundState = (BackgroundState)state;
                Thread.Sleep(backgroundState.Milliseconds);
                backgroundState.CallbackMethod();
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The view that this presenter is controlling.
        /// </summary>
        IFlightSimulatorXView _View;

        /// <summary>
        /// The object that will talk to FSX for us.
        /// </summary>
        IFlightSimulatorX _FlightSimulatorX;

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        IClock _Clock;

        /// <summary>
        /// The maximum airspeed of the current FSX aircraft.
        /// </summary>
        double _MaximumAirspeed = int.MaxValue;

        /// <summary>
        /// The aircraft that was used in the last call to <see cref="MoveAircraft"/>.
        /// </summary>
        IAircraft _MovedAircraft;

        /// <summary>
        /// The bank angle last approximated by the last call to <see cref="MoveAircraft"/>.
        /// </summary>
        double _LastApproximatedBank;

        /// <summary>
        /// The number of seconds that short trails are configured to last for.
        /// </summary>
        int _ShortTrailLengthSeconds;

        /// <summary>
        /// The date and time that the last exception from Flight Simulator X was shown to the user.
        /// </summary>
        /// <remarks>
        /// The intent is to prevent a flood of FSX exceptions from making the GUI unusable by limiting the rate at which
        /// they are shown to the user.
        /// </remarks>
        DateTime _TimeLastFsxExceptionRaised;

        /// <summary>
        /// The feed that we're using for the ride-along feature.
        /// </summary>
        IFeed _Feed;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFlightSimulatorXPresenterProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorAircraftList { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; set; }
        #endregion

        #region Constructor, Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightSimulatorXPresenter()
        {
            Provider = new DefaultProvider();
            _FlightSimulatorX = Factory.Singleton.Resolve<IFlightSimulatorX>();
            _Clock = Factory.Singleton.Resolve<IClock>();

            Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            Factory.Singleton.ResolveSingleton<IFeedManager>().FeedsChanged += FeedManager_FeedsChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FlightSimulatorXPresenter()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.ConfigurationChanged -= ConfigurationStorage_ConfigurationChanged;
                Factory.Singleton.ResolveSingleton<IFeedManager>().FeedsChanged -= FeedManager_FeedsChanged;
                _Feed = null;
            }
        }
        #endregion

        #region Initialise, IsSimConnectMessage
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IFlightSimulatorXView view)
        {
            LoadConfiguration();

            if(FlightSimulatorAircraftList == null) throw new InvalidOperationException("The FSX aircraft list must be set before calling Initialise");
            if(WebServer == null) throw new InvalidOperationException("The web server must be set before calling Initialise");

            _View = view;
            _View.CloseClicked += View_CloseClicked;
            _View.ConnectClicked += View_ConnectClicked;
            _View.RefreshFlightSimulatorXInformation += View_RefreshFlightSimulatorXInformation;
            _View.RideAircraftClicked += View_RideAircraftClicked;
            _View.StopRidingAircraftClicked += View_StopRidingAircraftClicked;
            _View.ConnectionStatus = _FlightSimulatorX.IsInstalled ? Strings.Disconnected : Strings.FlightSimulatorXIsNotInstalled;
            _View.ConnectToFlightSimulatorXEnabled = _FlightSimulatorX.IsInstalled;
            _View.FlightSimulatorPageAddress = String.Format("{0}/fsx.html", WebServer.LocalAddress);
            _View.RideStatus = "-";
            _View.RideAircraftEnabled = false;
            if(_FlightSimulatorX.IsInstalled) _View.UseSlewMode = true;

            _FlightSimulatorX.AircraftInformationReceived += FlightSimulatorX_AircraftInformationReceived;
            _FlightSimulatorX.ConnectionStatusChanged += FlightSimulatorX_ConnectionStatusChanged;
            _FlightSimulatorX.FlightSimulatorXExceptionRaised += FlightSimulatorX_FlightSimulatorXExceptionRaised;
            _FlightSimulatorX.SlewToggled += FlightSimulatorX_SlewToggled;

            ApplyRealAircraftList(initialise: true);

            Provider.TimedInvokeOnBackgroundThread(BackgroundThread_UpdateRealAircraftDisplay, 1000);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsSimConnectMessage(Message message)
        {
            return _FlightSimulatorX.IsSimConnectMessage(message);
        }

        /// <summary>
        /// Records information we are interested in from the configuration settings.
        /// </summary>
        private void LoadConfiguration()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();
            _ShortTrailLengthSeconds = configuration.GoogleMapSettings.ShortTrailLengthSeconds;

            var feedManager = Factory.Singleton.ResolveSingleton<IFeedManager>();
            _Feed = feedManager.GetByUniqueId(configuration.GoogleMapSettings.FlightSimulatorXReceiverId, ignoreInvisibleFeeds: false);
        }
        #endregion

        #region UpdateFlightSimulatorAircraftList
        /// <summary>
        /// Copies the aircraft information from FSX into the flight simulator aircraft list.
        /// </summary>
        /// <param name="fsxAircraft"></param>
        private void UpdateFlightSimulatorAircraftList(ReadAircraftInformation fsxAircraft)
        {
            lock(FlightSimulatorAircraftList.ListSyncLock) {
                IAircraft aircraft;
                if(FlightSimulatorAircraftList.Aircraft.Count != 0) aircraft = FlightSimulatorAircraftList.Aircraft[0];
                else {
                    aircraft = Factory.Singleton.Resolve<IAircraft>();
                    aircraft.Icao24 = "000000";
                    aircraft.UniqueId = 1;
                    FlightSimulatorAircraftList.Aircraft.Add(aircraft);
                }

                _MaximumAirspeed = fsxAircraft.MaxAirspeedIndicated;

                lock(aircraft) {
                    var now = _Clock.UtcNow;
                    aircraft.DataVersion = now.Ticks;
                    aircraft.Latitude = (float)fsxAircraft.Latitude;
                    aircraft.Longitude = (float)fsxAircraft.Longitude;
                    aircraft.GroundSpeed = (int)fsxAircraft.AirspeedIndicated;
                    aircraft.Track = (float)fsxAircraft.TrueHeading;
                    aircraft.Type = fsxAircraft.Type;
                    aircraft.Model = fsxAircraft.Model;
                    aircraft.Operator = fsxAircraft.Operator;
                    aircraft.Registration = fsxAircraft.Registration;
                    aircraft.Squawk = fsxAircraft.Squawk;
                    if(fsxAircraft.OnGround) {
                        aircraft.Altitude = 0;
                        aircraft.VerticalRate = 0;
                    } else {
                        aircraft.Altitude = (int)fsxAircraft.Altitude;
                        aircraft.VerticalRate = (int)fsxAircraft.VerticalSpeed;
                    }

                    aircraft.UpdateCoordinates(now, _ShortTrailLengthSeconds);
                }
            }
        }
        #endregion

        #region RideAircraft, StopRidingAircraft, ApplyRealAircraftList, MoveAircraft
        /// <summary>
        /// Moves the simulated aircraft in FSX to mirror the location, speed, attitude etc. of a real aircraft.
        /// </summary>
        private void RideAircraft()
        {
            var aircraft = _View.SelectedRealAircraft;
            if(aircraft == null) {
                _View.RideStatus = Strings.SelectAnAircraftToRide;
            } else {
                _FlightSimulatorX.IsFrozen = false;
                _FlightSimulatorX.IsSlewing = false;

                // Always turn on slew mode to begin with - this causes the FSX aircraft to rise off the ground a little before it returns.
                // If you freeze / slew an aircraft that is on the ground then FSX sometimes decides that it's *hit* the ground and flags a crash.
                _FlightSimulatorX.IsSlewing = true;
                if(!_View.UseSlewMode) {
                    _FlightSimulatorX.IsSlewing = false;
                    _FlightSimulatorX.IsFrozen = true;
                }

                _View.RidingAircraft = true;
                _View.RideStatus = String.Format(Strings.RidingAircraft, aircraft.Registration ?? aircraft.Icao24);
            }
        }

        /// <summary>
        /// Stops mirroring a real-life aircraft in FSX.
        /// </summary>
        private void StopRidingAircraft()
        {
            if(_View.RidingAircraft) {
                _View.RidingAircraft = false;
                _View.RideStatus = "-";
                _FlightSimulatorX.IsFrozen = false;
                _FlightSimulatorX.IsSlewing = false;
            }
        }

        /// <summary>
        /// Initialises or updates the real aircraft list.
        /// </summary>
        /// <param name="initialise"></param>
        private void ApplyRealAircraftList(bool initialise)
        {
            long trash1, trash2;
            var aircraftList = _Feed == null ? null : _Feed.AircraftList.TakeSnapshot(out trash1, out trash2);
            if(aircraftList != null) {
                if(initialise) _View.InitialiseRealAircraftListDisplay(aircraftList);
                else           _View.UpdateRealAircraftListDisplay(aircraftList);
            }
        }

        /// <summary>
        /// Repositions the FSX aircraft to match the location, speed and attitude of a real aircraft.
        /// </summary>
        private void MoveAircraft()
        {
            var selectedAircraft = _View.SelectedRealAircraft;

            if(_View.RidingAircraft && selectedAircraft != null) {
                long trash1, trash2;
                var aircraft = _Feed == null ? null : _Feed.AircraftList.TakeSnapshot(out trash1, out trash2).Where(a => a.UniqueId == selectedAircraft.UniqueId).FirstOrDefault();
                if(aircraft != null) {
                    var speedLimit = (float)_MaximumAirspeed - 30f;

                    var writeAircraftInformation = new WriteAircraftInformation();
                    writeAircraftInformation.AirspeedIndicated = aircraft.GroundSpeed > speedLimit ? speedLimit : aircraft.GroundSpeed.GetValueOrDefault();
                    aircraft.GroundSpeed.GetValueOrDefault();
                    writeAircraftInformation.Altitude = aircraft.Altitude.GetValueOrDefault();
                    writeAircraftInformation.Operator = aircraft.Operator;
                    writeAircraftInformation.Registration = aircraft.Registration;
                    writeAircraftInformation.Latitude = aircraft.Latitude.GetValueOrDefault();
                    writeAircraftInformation.Longitude = aircraft.Longitude.GetValueOrDefault();
                    writeAircraftInformation.TrueHeading = aircraft.Track.GetValueOrDefault();
                    writeAircraftInformation.VerticalSpeed = aircraft.VerticalRate.GetValueOrDefault();
                    writeAircraftInformation.Bank = ApproximateBank(_MovedAircraft, aircraft);

                    _MovedAircraft = aircraft;

                    _FlightSimulatorX.MoveAircraft(writeAircraftInformation);
                }
            }
        }

        /// <summary>
        /// Calculates an approximate bank angle based on the change in track of an aircraft since its last update.
        /// </summary>
        /// <param name="previousAircraft"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private double ApproximateBank(IAircraft previousAircraft, IAircraft aircraft)
        {
            double result = 0.0;

            if(previousAircraft != null) {
                if(previousAircraft.UniqueId == aircraft.UniqueId) {
                    if(previousAircraft.LastUpdate == aircraft.LastUpdate) result = _LastApproximatedBank;
                    else {
                        var delta = previousAircraft.Track - aircraft.Track;
                        if(delta < 180F) delta += 360F;
                        if(delta > 180F) delta -= 360F;

                        result = ((double?)delta * 2.0).GetValueOrDefault();
                        result = Math.Max(-40.0, Math.Min(40.0, result));
                    }
                }
            }

            _LastApproximatedBank = result;
            return result;
        }
        #endregion

        #region Events
        private void BackgroundThread_UpdateRealAircraftDisplay()
        {
            if(_View.CanBeRefreshed) {
                ApplyRealAircraftList(initialise: false);
                Provider.TimedInvokeOnBackgroundThread(BackgroundThread_UpdateRealAircraftDisplay, 1000);
            }
        }

        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }

        private void FeedManager_FeedsChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }

        private void FlightSimulatorX_AircraftInformationReceived(object sender, EventArgs<ReadAircraftInformation> args)
        {
            UpdateFlightSimulatorAircraftList(args.Value);
        }

        private void FlightSimulatorX_ConnectionStatusChanged(object sender, EventArgs args)
        {
            _View.ConnectionStatus = _FlightSimulatorX.ConnectionStatus;
            _View.ConnectToFlightSimulatorXEnabled = !_FlightSimulatorX.Connected;
            _View.RideAircraftEnabled = _FlightSimulatorX.Connected;
        }

        private void FlightSimulatorX_FlightSimulatorXExceptionRaised(object sender, EventArgs<FlightSimulatorXException> args)
        {
            var now = _Clock.UtcNow;
            if(now >= _TimeLastFsxExceptionRaised.AddSeconds(20)) {
                _TimeLastFsxExceptionRaised = now;
                throw args.Value;
            } else {
                var log = Factory.Singleton.ResolveSingleton<ILog>();
                log.WriteLine("FSX exception seen within 20 seconds of the last one: {0}", args.Value.Message);
            }
        }

        private void FlightSimulatorX_SlewToggled(object sender, EventArgs args)
        {
            StopRidingAircraft();
        }

        private void View_CloseClicked(object sender, EventArgs args)
        {
            if(_FlightSimulatorX.Connected) _FlightSimulatorX.Disconnect();
        }

        private void View_ConnectClicked(object sender, EventArgs args)
        {
            _View.ConnectToFlightSimulatorXEnabled = false;
            _FlightSimulatorX.Connect(_View.WindowHandle);
        }

        private void View_RefreshFlightSimulatorXInformation(object sender, EventArgs args)
        {
            if(_FlightSimulatorX.Connected) {
                _FlightSimulatorX.RequestAircraftInformation();
                MoveAircraft();
            }
        }

        private void View_RideAircraftClicked(object sender, EventArgs args)
        {
            RideAircraft();
        }

        private void View_StopRidingAircraftClicked(object sender, EventArgs args)
        {
            StopRidingAircraft();
        }
        #endregion
    }
}
