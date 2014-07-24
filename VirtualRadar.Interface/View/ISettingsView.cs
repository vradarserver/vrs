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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for views that lets the user view and edit the configuration.
    /// </summary>
    public interface ISettingsView : IView, IBusyView, IValidateView
    {
        #region Properties
        /// <summary>
        /// Gets or sets the configuration to display and edit.
        /// </summary>
        Configuration Configuration { get; set; }

        /// <summary>
        /// Gets the list of configured users.
        /// </summary>
        BindingList<IUser> Users { get; }

        /// <summary>
        /// Gets or sets the name of the user manager.
        /// </summary>
        string UserManager { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user wants to save their changes.
        /// </summary>
        event EventHandler SaveClicked;

        /// <summary>
        /// Raised when the user indicates that they want to see if their current data feed settings will connect to something.
        /// </summary>
        event EventHandler<EventArgs<Receiver>> TestConnectionClicked;

        /// <summary>
        /// Raised when the user wants to test the speech synthesis settings.
        /// </summary>
        event EventHandler TestTextToSpeechSettingsClicked;

        /// <summary>
        /// Raised when the user indicates that they want to refresh the content of the ReceiverLocations list with
        /// locations from the BaseStation database.
        /// </summary>
        event EventHandler UpdateReceiverLocationsFromBaseStationDatabaseClicked;

        /// <summary>
        /// Raised when the user wants to use the ICAO specification settings for the raw decoder.
        /// </summary>
        event EventHandler UseIcaoRawDecodingSettingsClicked;

        /// <summary>
        /// Raised when the user wants to use the default values for the raw decoder.
        /// </summary>
        event EventHandler UseRecommendedRawDecodingSettingsClicked;

        /// <summary>
        /// Raised when the user indicates that they don't have any radio, they just want to set this thing up to
        /// show their aircraft from FSX.
        /// </summary>
        event EventHandler FlightSimulatorXOnlyClicked;
        #endregion

        #region Methods
        /// <summary>
        /// The names of every voice installed on the system.
        /// </summary>
        /// <param name="voiceNames"></param>
        void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames);

        /// <summary>
        /// Displays the results of a connection test attempt to the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowTestConnectionResults(string message, string title);
        #endregion
    }
}
