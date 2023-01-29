// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Settings that control how browsers on the Internet interact with the server.
    /// </summary>
    public class InternetClientSettings
    {
        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to run reports.
        /// </summary>
        public bool CanRunReports { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to show text labels on the aircraft pins.
        /// </summary>
        public bool CanShowPinText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to play audio from the server.
        /// </summary>
        public bool CanPlayAudio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to see aircraft pictures.
        /// </summary>
        public bool CanShowPictures { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes of inactivity before the client times out and stops asking for the aircraft list.
        /// If this is zero then the timeout is disabled.
        /// </summary>
        public int TimeoutMinutes { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value indicating whether proximity gadgets can connect to this server over the Internet.
        /// </summary>
        public bool AllowInternetProximityGadgets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients can see the links to submit routes.
        /// </summary>
        public bool CanSubmitRoutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Internet clients can see polar plots.
        /// </summary>
        public bool CanShowPolarPlots { get; set; }
    }
}
