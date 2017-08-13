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
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.BaseStation;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for the splash screen that is displayed while the application is firing up.
    /// </summary>
    public interface ISplashView : IView
    {
        /// <summary>
        /// Gets or sets the name of the application to show to the user.
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the version information to show to the user.
        /// </summary>
        string ApplicationVersion { get; set; }

        /// <summary>
        /// Gets a value indicating that the load succeeded without any obvious problems.
        /// </summary>
        bool LoadSucceeded { get; }

        /// <summary>
        /// Gets or sets the aircraft list tracking real aircraft that the presenter will create in the background.
        /// </summary>
        IBaseStationAircraftList BaseStationAircraftList { get; set; }

        /// <summary>
        /// Gets or sets the aircraft list tracking simulated aircraft in FSX that the presenter will create in the background.
        /// </summary>
        /// <remarks>
        /// Note that this is here for historical reasons and will go away in a code cleanup. The interface is now
        /// tagged as singleton and can be accessed from anywhere.
        /// </remarks>
        IFlightSimulatorAircraftList FlightSimulatorXAircraftList { get; set; }

        /// <summary>
        /// Gets or sets the universal plug and play manager that the presenter creates for the application.
        /// </summary>
        IUniversalPlugAndPlayManager UPnpManager { get; set; }

        /// <summary>
        /// Records references to objects that will be passed on to the presenter when the time comes to create it.
        /// </summary>
        /// <param name="commandLineArgs"></param>
        /// <param name="backgroundThreadExceptionHandler"></param>
        void Initialise(string[] commandLineArgs, EventHandler<EventArgs<Exception>> backgroundThreadExceptionHandler);

        /// <summary>
        /// Non-blocking method to display text to the user, intended to show the current state of the presenter's build up of the application.
        /// </summary>
        /// <param name="text"></param>
        void ReportProgress(string text);

        /// <summary>
        /// Blocking method that displays details of a problem to the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="quitApplication"></param>
        void ReportProblem(string message, string title, bool quitApplication);

        /// <summary>
        /// Blocking method that displays a prompt and asks the user to choose one of a yes/no response. Returns true if the user answers yes.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="defaultYes"></param>
        /// <returns></returns>
        bool YesNoPrompt(string message, string title, bool defaultYes);
    }
}
