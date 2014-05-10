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
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for views that can display information about the connection to Flight Simulator X.
    /// </summary>
    /// <remarks>
    /// Note that SimConnect requires that this have a window handle to which messages can be sent which may limit the
    /// circumstances under which this can be fully implemented.
    /// </remarks>
    public interface IFlightSimulatorXView : IView
    {
        /// <summary>
        /// Gets a value indicating to the presenter that the view is on show and should be refreshed.
        /// </summary>
        bool CanBeRefreshed { get; }

        /// <summary>
        /// Gets or sets a value indicating that the user should be allowed to elect to connect to Flight Simulator X.
        /// </summary>
        bool ConnectToFlightSimulatorXEnabled { get; set; }

        /// <summary>
        /// Gets or sets a description of the state of the connection to FSX.
        /// </summary>
        string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the address of the flight simulator web page that the server can display.
        /// </summary>
        string FlightSimulatorPageAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user should be allowed to elect to ride an aircraft.
        /// </summary>
        bool RideAircraftEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user is mirroring a real aircraft's position etc. in FSX.
        /// </summary>
        bool RidingAircraft { get; set; }

        /// <summary>
        /// Gets or sets a description of the state of the FSX 'ride' feature.
        /// </summary>
        string RideStatus { get; set; }

        /// <summary>
        /// Gets the real aircraft selected by the user for riding.
        /// </summary>
        IAircraft SelectedRealAircraft { get; }

        /// <summary>
        /// Gets or sets a value indicating then the ride feature should use slew mode to move the simulated
        /// aircraft instead of freeze mode.
        /// </summary>
        bool UseSlewMode { get; set; }

        /// <summary>
        /// Gets the window handle that will be passed to <see cref="FlightSimulatorX.IFlightSimulatorX"/>.
        /// </summary>
        /// <remarks>
        /// The SimConnect object that FlightSimulatorX uses needs a window handle to pass messages to, so
        /// non-Win32 implementations of this might be in some trouble!
        /// </remarks>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Raised when the view is shutting down.
        /// </summary>
        event EventHandler CloseClicked;

        /// <summary>
        /// Raised when the user has indicated that they want to connect to Flight Simulator X.
        /// </summary>
        event EventHandler ConnectClicked;

        /// <summary>
        /// Raised when the user has indicated that they want to duplicate the position, airspeed, attitude etc. of a real
        /// aircraft in Flight Simulator X.
        /// </summary>
        event EventHandler RideAircraftClicked;

        /// <summary>
        /// Raised when the user has indicated that they don't want to ride the aircraft any more.
        /// </summary>
        event EventHandler StopRidingAircraftClicked;

        /// <summary>
        /// Raised by a timer running on the GUI thread. Indicates that the presenter should ask FSX for an update of the
        /// simulated aircraft's position or send updates of real-world aircraft positions to FSX.
        /// </summary>
        /// <remarks>
        /// This should be done on the GUI thread because SimConnect doesn't respond well when sent messages on a non-GUI thread.
        /// </remarks>
        event EventHandler RefreshFlightSimulatorXInformation;

        /// <summary>
        /// Records objects that will be passed on to <see cref="Presenter.IFlightSimulatorXPresenter"/> when it is created.
        /// </summary>
        /// <param name="baseStationAircraftList"></param>
        /// <param name="flightSimulatorAircraftList"></param>
        /// <param name="webServer"></param>
        void Initialise(IBaseStationAircraftList baseStationAircraftList, ISimpleAircraftList flightSimulatorAircraftList, IWebServer webServer);

        /// <summary>
        /// Displays the list of real aircraft for the first time. Any existing display of real aircraft will be cleared by this.
        /// </summary>
        /// <param name="aircraftList"></param>
        void InitialiseRealAircraftListDisplay(IList<IAircraft> aircraftList);

        /// <summary>
        /// Updates the display of real aircraft. This could be called from a non-GUI thread.
        /// </summary>
        /// <param name="aircraftList"></param>
        void UpdateRealAircraftListDisplay(IList<IAircraft> aircraftList);
    }
}
