// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using InterfaceFactory;

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
        /// The address of the online help page for the About WinForms dialog.
        /// </summary>
        public static string WinFormsAboutDialog { get; }

        /// <summary>
        /// The address of the online help page for the Connection Client Log dialog.
        /// </summary>
        public static string WinFormsConnectionClientLogDialog { get; }

        /// <summary>
        /// The address of the online help page for the Connection Session Log dialog.
        /// </summary>
        public static string WinFormsConnectionSessionLogDialog { get; }

        /// <summary>
        /// The address of the online help page for the Flight Simulator X dialog.
        /// </summary>
        public static string WinFormsFlightSimulatorXDialog { get; }

        /// <summary>
        /// The address of the online help page for the Main dialog.
        /// </summary>
        public static string WinFormsMainDialog { get; }

        /// <summary>
        /// The address of the online help page for the Rebroadcast Options dialog.
        /// </summary>
        public static string WinFormsRebroadcastOptionsView { get; }

        /// <summary>
        /// The address of the online help page for the Receiver Locations dialog.
        /// </summary>
        public static string WinFormsReceiverLocationsView { get; }

        /// <summary>
        /// The address of the online help page for the Options dialog.
        /// </summary>
        public static string WinFormsOptionsDialog { get; }

        /// <summary>
        /// The address of the online help page for the Statistics dialog.
        /// </summary>
        public static string WinFormsStatisticsDialog { get; }

        /// <summary>
        /// Static ctor.
        /// </summary>
        static OnlineHelpAddress()
        {
            var webAddressManager = Factory.ResolveSingleton<IWebAddressManager>();

            WinFormsAboutDialog =                   webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-about",                  "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsAboutDialog.aspx");
            WinFormsConnectionClientLogDialog =     webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-client-log",             "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsConnectionClientLogDialog.aspx");
            WinFormsConnectionSessionLogDialog =    webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-session-log",            "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsConnectionSessionLogDialog.aspx");
            WinFormsFlightSimulatorXDialog =        webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-fsx",                    "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsFlightSimulatorXDialog.aspx");
            WinFormsMainDialog =                    webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-main-window",            "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsMainDialog2.aspx");
            WinFormsRebroadcastOptionsView =        webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-rebroadcast-servers",    "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsRebroadcastServersView.aspx");
            WinFormsReceiverLocationsView =         webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-receiver-locations",     "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsReceiverLocationsView.aspx");
            WinFormsOptionsDialog =                 webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-options",                "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsOptionsDialog2.aspx");
            WinFormsStatisticsDialog =              webAddressManager.RegisterAddress("vrs-onlinehelp-winforms-statistics",             "https://www.virtualradarserver.co.uk/OnlineHelp/WinFormsStatisticsDialog.aspx");
        }
    }
}
