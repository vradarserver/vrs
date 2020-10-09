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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.BaseStation;
using System.Windows.Forms;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that can control <see cref="IFlightSimulatorView"/> views.
    /// </summary>
    public interface IFlightSimulatorPresenter : IPresenter<IFlightSimulatorView>, IDisposable
    {
        /// <summary>
        /// Gets or sets the provider that will abstract away the environment for the provider to make it easier to test.
        /// </summary>
        IFlightSimulatorPresenterProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the object that will be modified by the presenter to record the simulated aircraft in FSX.
        /// </summary>
        ISimpleAircraftList FlightSimulatorAircraftList { get; set; }

        /// <summary>
        /// Gets or sets the web server that the view will show a link to.
        /// </summary>
        IWebServer WebServer { get; set; }

        /// <summary>
        /// Returns true if the message passed across is intended for SimConnect (i.e. it's a Flight Simulator X message).
        /// If it is then the view should not process it.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IsSimConnectMessage(Message message);
    }
}
