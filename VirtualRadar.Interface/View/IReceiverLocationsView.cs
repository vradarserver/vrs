// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for the view that can be used to edit the list of receiver locations.
    /// </summary>
    public interface IReceiverLocationsView : IView, IValidateView
    {
        /// <summary>
        /// Gets the full list of receiver locations.
        /// </summary>
        List<ReceiverLocation> ReceiverLocations { get; }

        /// <summary>
        /// Gets or sets the currently-selected receiver location.
        /// </summary>
        ReceiverLocation SelectedReceiverLocation { get; set; }

        /// <summary>
        /// Gets or sets the name of the location being edited by the user.
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the location being edited by the user.
        /// </summary>
        string Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the location being edited by the user.
        /// </summary>
        string Longitude { get; set; }

        /// <summary>
        /// Raised when the user changes the selection in the list of receiver locations.
        /// </summary>
        event EventHandler SelectedLocationChanged;

        /// <summary>
        /// Raised when the user indicates that they want to abandon their edits of the selected location.
        /// </summary>
        event EventHandler ResetClicked;

        /// <summary>
        /// Raised when the content of the <see cref="Location"/>, <see cref="Latitude"/> or <see cref="Longitude"/> fields change.
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Raised when the user clicks the button to add a new location.
        /// </summary>
        event EventHandler NewLocationClicked;

        /// <summary>
        /// Raised when the user clicks the button to delete an existing location.
        /// </summary>
        event EventHandler DeleteLocationClicked;

        /// <summary>
        /// Raised when the user indicates that they want to refresh the content of the list with
        /// locations from the BaseStation database.
        /// </summary>
        event EventHandler UpdateFromBaseStationDatabaseClicked;

        /// <summary>
        /// Raised when the user closes the dialog. The currently selected location should be validated and used.
        /// </summary>
        event EventHandler CloseClicked;

        /// <summary>
        /// Refreshes the display of the selected location in the list of locations.
        /// </summary>
        void RefreshSelectedLocation();

        /// <summary>
        /// Refreshes the display of all of the locations.
        /// </summary>
        void RefreshLocations();

        /// <summary>
        /// Place the focus onto the location / latitude / longitude fields.
        /// </summary>
        void FocusOnEditFields();
    }
}
