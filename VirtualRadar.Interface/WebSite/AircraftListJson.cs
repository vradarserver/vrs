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
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The list of aircraft that is sent to the browser as a JSON file.
    /// </summary>
    [DataContract]
    public class AircraftListJson
    {
        /// <summary>
        /// Gets or sets the source of the aircraft list (see <see cref="AircraftListSource"/>).
        /// </summary>
        [DataMember(Name="src")]
        public int Source { get; set; }

        /// <summary>
        /// Gets a collection of feeds that VRS has been configured to listen to and are currently enabled.
        /// </summary>
        [DataMember(Name="feeds")]
        public List<FeedJson> Feeds { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the feed that this aircraft list is showing.
        /// </summary>
        /// <remarks>
        /// It's entirely possible that this ID number does not correspond with any feed in <see cref="Feeds"/> - this
        /// can happen when reporting on the Flight Simulator X feed, or when the feed requested by the website no
        /// longer exists on the server.
        /// </remarks>
        [DataMember(Name="srcFeed")]
        public int SourceFeedId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that silhouettes can be shown for aircraft.
        /// </summary>
        [DataMember(Name="showSil")]
        public bool ShowSilhouettes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that operator flags can be shown for aircraft.
        /// </summary>
        [DataMember(Name="showFlg")]
        public bool ShowFlags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that pictures can be shown for aircraft.
        /// </summary>
        [DataMember(Name="showPic")]
        public bool ShowPictures { get; set; }

        /// <summary>
        /// Gets or sets the height of the operator flags.
        /// </summary>
        [DataMember(Name="flgH")]
        public int FlagHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the operator flags.
        /// </summary>
        [DataMember(Name="flgW")]
        public int FlagWidth { get; set; }

        /// <summary>
        /// Gets the list of aircraft to show to the user.
        /// </summary>
        [DataMember(Name="acList", IsRequired=true)]
        public List<AircraftJson> Aircraft { get; private set; }

        /// <summary>
        /// Gets or sets the total number of aircraft that the server is currently tracking.
        /// </summary>
        [DataMember(Name="totalAc")]
        public int AvailableAircraft { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="IAircraft.DataVersion"/> for the aircraft in the aircraft list.
        /// </summary>
        /// <remarks>The browser sends this value back to the server when it asks for another aircraft list. In this
        /// way the server can figure out what has changed since the last time the browser asked for a list.</remarks>
        [DataMember(Name="lastDv")]
        public string LastDataVersion { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds of positions to show in short trails.
        /// </summary>
        [DataMember(Name="shtTrlSec", IsRequired=false, EmitDefaultValue=false)]
        public int ShortTrailLengthSeconds { get; set; }

        /// <summary>
        /// Gets or sets the server's current time as the number of Javascript ticks in a UTC DateTime.
        /// </summary>
        [DataMember(Name="stm")]
        public long ServerTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server configuration has changed since their last update.
        /// </summary>
        [DataMember(Name="configChanged", IsRequired=false, EmitDefaultValue=false)]
        public bool ServerConfigChanged { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftListJson()
        {
            Aircraft = new List<AircraftJson>();
            Feeds = new List<FeedJson>();
        }
    }
}
