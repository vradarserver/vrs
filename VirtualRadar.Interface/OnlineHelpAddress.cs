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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class exposing the addresses of online help pages.
    /// </summary>
    /// <remarks>
    /// The website is configured to redirect from the addresses here to wherever the online help is being stored nowadays.
    /// </remarks>
    public static class OnlineHelpAddress
    {
        /// <summary>
        /// The prefix for every online help address.
        /// </summary>
        private const string Prefix = "http://www.virtualradarserver.co.uk/OnlineHelp/";

        /// <summary>
        /// The address of the online help page for the About WinForms dialog.
        /// </summary>
        public static readonly string WinFormsAboutDialog = Prefix + "WinFormsAboutDialog.aspx";

        /// <summary>
        /// The address of the online help page for the Connection Client Log dialog.
        /// </summary>
        public static readonly string WinFormsConnectionClientLogDialog = Prefix + "WinFormsConnectionClientLogDialog.aspx";

        /// <summary>
        /// The address of the online help page for the Connection Session Log dialog.
        /// </summary>
        public static readonly string WinFormsConnectionSessionLogDialog = Prefix + "WinFormsConnectionSessionLogDialog.aspx";

        /// <summary>
        /// The address of the online help page for the Flight Simulator X dialog.
        /// </summary>
        public static readonly string WinFormsFlightSimulatorXDialog = Prefix + "WinFormsFlightSimulatorXDialog.aspx";

        /// <summary>
        /// The address of the online help page for the Main dialog.
        /// </summary>
        public static readonly string WinFormsMainDialog = Prefix + "WinFormsMainDialog2.aspx";

        /// <summary>
        /// The address of the online help page for the Rebroadcast Options dialog.
        /// </summary>
        public static readonly string WinFormsRebroadcastOptionsView = Prefix + "WinFormsRebroadcastServersView.aspx";

        /// <summary>
        /// The address of the online help page for the Receiver Locations dialog.
        /// </summary>
        public static readonly string WinFormsReceiverLocationsView = Prefix + "WinFormsReceiverLocationsView.aspx";

        /// <summary>
        /// The address of the online help page for the Options dialog.
        /// </summary>
        public static readonly string WinFormsOptionsDialog = Prefix + "WinFormsOptionsDialog2.aspx";

        /// <summary>
        /// The address of the online help page for the Statistics dialog.
        /// </summary>
        public static readonly string WinFormsStatisticsDialog = Prefix + "WinFormsStatisticsDialog.aspx";
    }
}
