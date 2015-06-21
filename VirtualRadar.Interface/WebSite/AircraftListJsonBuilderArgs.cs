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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The object that passes arguments to the <see cref="IAircraftListJsonBuilder.Build"/> method.
    /// </summary>
    public class AircraftListJsonBuilderArgs
    {
        /// <summary>
        /// Gets or sets the ID of the source receiver.
        /// </summary>
        public int SourceFeedId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the selected aircraft or -1 if no aircraft was selected.
        /// </summary>
        public int SelectedAircraftId { get; set; }

        /// <summary>
        /// Gets or sets the aircraft list that provides details of the aircraft being tracked.
        /// </summary>
        public IAircraftList AircraftList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that this is being used to build a list of flight simulator aircraft.
        /// </summary>
        public bool IsFlightSimulatorList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft JSON list is going to be sent to an Internet client.
        /// </summary>
        public bool IsInternetClient { get; set; }

        /// <summary>
        /// Gets or sets the latitude that the browser is located on.
        /// </summary>
        public double? BrowserLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude that the browser is located on.
        /// </summary>
        public double? BrowserLongitude { get; set; }

        /// <summary>
        /// Gets the list of aircraft that the browser was told about the last time it asked for a list.
        /// </summary>
        public List<int> PreviousAircraft { get; private set; }

        /// <summary>
        /// Gets or sets the DataVersion of the data that was last sent to the browser. Use -1 if the browser does not
        /// report the previous data version.
        /// </summary>
        public long PreviousDataVersion { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the entire trail for each aircraft should be sent regardless of whether the
        /// browser has been previously sent the trail or not.
        /// </summary>
        /// <remarks>
        /// Browsers are allowed to ask for the full trail to be sent - this happens when the user switches between short and
        /// full trails.
        /// </remarks>
        public bool ResendTrails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what type of trail is required.
        /// </summary>
        public TrailType TrailType { get; set; }

        /// <summary>
        /// Gets or sets the filters used to suppress aircraft from the list.
        /// </summary>
        public AircraftListJsonBuilderFilter Filter { get; set; }

        /// <summary>
        /// Gets a list of <see cref="AircraftComparerColumn"/> columns to sort by and a bool to indicate whether the sort direction
        /// is ascending (true) or descending (false).
        /// </summary>
        public List<KeyValuePair<string, bool>> SortBy { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that the Feeds list does not need to be filled in.
        /// </summary>
        public bool FeedsNotRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that fields derived from aircraft messages are to be suppressed,
        /// only fields taken directly from an aircraft are to be included in the aircraft JSON.
        /// </summary>
        public bool OnlyIncludeMessageFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that aircraft that have not changed are not to be included in the
        /// aircraft list.
        /// </summary>
        public bool IgnoreUnchanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the ICAO should always be shown for each aircraft, even if it has not changed.
        /// </summary>
        public bool AlwaysShowIcao { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftListJsonBuilderArgs()
        {
            PreviousAircraft = new List<int>();
            PreviousDataVersion = -1L;
            SelectedAircraftId = -1;
            SourceFeedId = -1;
            SortBy = new List<KeyValuePair<string,bool>>();
        }
    }
}
