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
    /// The interface for all views that allow the user to enter the details of rebroadcast servers.
    /// </summary>
    public interface IRebroadcastOptionsView : IView, IValidateView
    {
        /// <summary>
        /// Gets the list of rebroadcast server settings.
        /// </summary>
        List<RebroadcastSettings> RebroadcastSettings { get; }

        /// <summary>
        /// Gets or sets the currently selected server settings.
        /// </summary>
        RebroadcastSettings SelectedRebroadcastSettings { get; set; }

        /// <summary>
        /// Gets or sets the Enabled value of the currently selected server.
        /// </summary>
        bool ServerEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Name value of the currently selected server.
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the Format value of the currently selected server.
        /// </summary>
        RebroadcastFormat ServerFormat { get; set; }

        /// <summary>
        /// Gets or sets the Port value of the currently selected server.
        /// </summary>
        int ServerPort { get; set; }

        /// <summary>
        /// Raised when the user changes the selection in the list of rebroadcast servers.
        /// </summary>
        event EventHandler SelectedServerChanged;

        /// <summary>
        /// Raised when the user indicates that they want to abandon their edits of the selected server.
        /// </summary>
        event EventHandler ResetClicked;

        /// <summary>
        /// Raised when the content of the <see cref="ServerName"/> field changes.
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Raised when the user clicks the button to add a new server.
        /// </summary>
        event EventHandler NewServerClicked;

        /// <summary>
        /// Raised when the user clicks the button to delete an existing server.
        /// </summary>
        event EventHandler DeleteServerClicked;

        /// <summary>
        /// Refreshes the display of the selected server in the list of servers.
        /// </summary>
        void RefreshSelectedServer();

        /// <summary>
        /// Refreshes the display of all of the servers.
        /// </summary>
        void RefreshServers();

        /// <summary>
        /// Place the focus onto the data entry fields.
        /// </summary>
        void FocusOnEditFields();
    }
}
