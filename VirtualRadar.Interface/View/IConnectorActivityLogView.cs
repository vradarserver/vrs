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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for views that can display connector activity logs.
    /// </summary>
    public interface IConnectorActivityLogView : IView
    {
        /// <summary>
        /// Gets or sets the connector to show activities for. If no connector is supplied then activities
        /// across all connectors are shown.
        /// </summary>
        IConnector Connector { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the connector name column should be hidden from view.
        /// Only has an effect if set before the control is loaded.
        /// </summary>
        bool HideConnectorName { get; set; }

        /// <summary>
        /// Gets or sets the selected connector activity events.
        /// </summary>
        ConnectorActivityEvent[] SelectedConnectorActivityEvents { get; set; }

        /// <summary>
        /// Gets all of the connector activity events on display.
        /// </summary>
        ConnectorActivityEvent[] ConnectorActivityEvents { get; }

        /// <summary>
        /// Shows the connector activity events passed across to the user.
        /// </summary>
        /// <param name="connectorActivityEvents"></param>
        void Populate(IEnumerable<ConnectorActivityEvent> connectorActivityEvents);

        /// <summary>
        /// Raised when the refresh button is clicked.
        /// </summary>
        event EventHandler RefreshClicked;

        /// <summary>
        /// Raised when the copy selected items to clipboard button is clicked.
        /// </summary>
        event EventHandler CopySelectedItemsToClipboardClicked;
    }
}
