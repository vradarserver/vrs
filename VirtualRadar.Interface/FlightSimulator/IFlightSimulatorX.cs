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
using System.Windows.Forms;

namespace VirtualRadar.Interface.FlightSimulator
{
    /// <summary>
    /// The interface for objects that can talk to Microsoft's Flight Simulator X for us.
    /// </summary>
    /// <remarks>
    /// SimConnect sends messages to another application via Windows messages, so we need to have access to a WndProc
    /// for this to work. The .NET framework doesn't make it easy to get at the WndProc for a form directly, you need
    /// to inherit from the form to do so. We could use interop and just do it the old-fashioned way but it feels a bit
    /// naughty-naughty, so instead the caller should call <see cref="IsSimConnectMessage"/> for every message they receive
    /// and do nothing if it returns true.
    /// </remarks>
    public interface IFlightSimulatorX : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets a value indicating that Flight Simulator X appears to be installed.
        /// </summary>
        /// <remarks>
        /// If this returns false then the behaviour of the rest of the object is undefined.
        /// </remarks>
        bool IsInstalled { get; }

        /// <summary>
        /// Gets a value indicating whether we are connected to Flight Simulator X or not.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets a description of the state of the connection to FSX.
        /// </summary>
        string ConnectionStatus { get; }

        /// <summary>
        /// Gets or sets a value indicating that the simulated aircraft's position, altitude and attitude
        /// are frozen in FSX. When an aircraft is frozen it is unable to move but FSX will register a collision
        /// if it hits an object or the ground.
        /// </summary>
        bool IsFrozen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the simulator is in slew mode. In slew mode the aircraft does
        /// not fly normally but the pitch is still affected by the simulated environment. FSX will ignore
        /// collisions with objects or the ground.
        /// </summary>
        bool IsSlewing { get; set; }

        /// <summary>
        /// Gets a count of messages that have been sent from FSX.
        /// </summary>
        int MessagesReceivedCount { get; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when Flight Simulator responds to a call to <see cref="RequestAircraftInformation"/>.
        /// </summary>
        event EventHandler<EventArgs<ReadAircraftInformation>> AircraftInformationReceived;

        /// <summary>
        /// Raised whenever <see cref="ConnectionStatus"/> changes.
        /// </summary>
        event EventHandler ConnectionStatusChanged;

        /// <summary>
        /// Raised whenever Flight Simulator raises an exception.
        /// </summary>
        event EventHandler<EventArgs<FlightSimulatorXException>> FlightSimulatorXExceptionRaised;

        /// <summary>
        /// Raised whenever <see cref="IsFrozen"/> changes.
        /// </summary>
        event EventHandler FreezeStatusChanged;

        /// <summary>
        /// Raised whenever <see cref="IsSlewing"/> changes.
        /// </summary>
        event EventHandler SlewStatusChanged;

        /// <summary>
        /// Raised whenever the user manually toggles slew mode within the game.
        /// </summary>
        event EventHandler SlewToggled;
        #endregion

        #region Methods
        /// <summary>
        /// Connects to Flight Simulator X. 
        /// </summary>
        /// <param name="windowHandle"></param>
        void Connect(IntPtr windowHandle);

        /// <summary>
        /// Disconnects from Flight Simulator X.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns true if the Windows message is a SimConnect message from Flight Simulator X.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if the message is a SimConnect message, false if it is not.</returns>
        /// <remarks>
        /// You will need to have an open form for SimConnect / Flight Simulator X to send messages to.
        /// Override the DefWndProc method on the form and then pass every message that comes into
        /// the form to this method. If this method returns true then throw the message away, otherwise
        /// pass it on for normal processing.
        /// </remarks>
        /// <example>
        /// <code>
        /// protected override void DefWndProc(ref Message m)
        /// {
        ///     if(!_FlightSimulatorX.IsSimConnectMessage(m)) base.DefWndProc(ref m);
        /// }
        /// </code>
        /// </example>
        bool IsSimConnectMessage(Message message);

        /// <summary>
        /// Tells Flight Simulator to move the simulated aircraft to the position specified.
        /// </summary>
        /// <param name="aircraftInformation"></param>
        void MoveAircraft(WriteAircraftInformation aircraftInformation);

        /// <summary>
        /// Sends a request to Flight Simulator for information about the simulated aircraft.
        /// </summary>
        /// <remarks>
        /// This should be sent to FSX from the GUI thread. FSX will reply at some point and cause
        /// <see cref="AircraftInformationReceived"/> to be raised, also on the GUI thread.
        /// </remarks>
        void RequestAircraftInformation();
        #endregion
    }
}
