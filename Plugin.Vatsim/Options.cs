// Copyright © 2022 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.Vatsim
{
    class Options
    {
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is downloading VATSIM states and building
        /// feeds from them.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of seconds between fetches of VATSIM data. Note that there is a rate limit
        /// at VATSIM of 15 seconds (as of time of writing).
        /// </summary>
        public int RefreshIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Gets or sets a value indicating that on-ground status is to be inferred from the speed of the
        /// aircraft. See <see cref="SlowAircraftThresholdSpeedKnots"/>.
        /// </summary>
        public bool AssumeSlowAircraftAreOnGround { get; set; } = true;

        /// <summary>
        /// Gets or sets an aircraft speed (in knots) below which aircraft are considered to be on the ground if
        /// <see cref="AssumeSlowAircraftAreOnGround"/> is true.
        /// </summary>
        public int SlowAircraftThresholdSpeedKnots { get; set; } = 40;

        /// <summary>
        /// Gets or sets a value indicating that the manufacturer and model should be inferred from the reported
        /// model type.
        /// </summary>
        public bool InferModelFromModelType { get; set; } = true;

        /// <summary>
        /// Gets a list of geofences and the feeds that can be associated with them.
        /// </summary>
        public List<GeofenceFeedOption> GeofencedFeeds { get; } = new List<GeofenceFeedOption>() {
            new GeofenceFeedOption() {
                FeedName =      "UK and Ireland",
                CentreOn =      GeofenceCentreOn.Coordinate,
                Latitude =      54.49798931601776,
                Longitude =     -4.5556287244490985,
                Width =         560.0,
                Height =        740.0,
                DistanceUnit =  DistanceUnit.Miles,
            },
        };
    }
}
