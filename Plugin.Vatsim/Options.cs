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
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// Holds the plugin's options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the current version of the options object. This is incremented every time it is saved.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is downloading VATSIM states and building
        /// feeds from them.
        /// </summary>
        public bool Enabled { get; set; }

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
        public List<GeofenceFeedOption> GeofencedFeeds { get; private set; } = new List<GeofenceFeedOption>();

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public Options Clone()
        {
            var result = (Options)MemberwiseClone();
            result.GeofencedFeeds = new List<GeofenceFeedOption>();
            foreach(var original in GeofencedFeeds) {
                result.GeofencedFeeds.Add(original.Clone());
            }

            return result;
        }

        /// <summary>
        /// Fixes up any obvious garbage. Intended for use before saving.
        /// </summary>
        public void NormaliseBeforeSave()
        {
            RefreshIntervalSeconds = Math.Max(5, RefreshIntervalSeconds);
            SlowAircraftThresholdSpeedKnots = Math.Max(0, SlowAircraftThresholdSpeedKnots);

            foreach(var feed in GeofencedFeeds) {
                var id = feed.ID.ToString().ToUpperInvariant();
                if(String.IsNullOrEmpty(feed.FeedName)) {
                    feed.FeedName = id.Substring(id.Length - Math.Min(8, id.Length));
                }
                switch(feed.CentreOn) {
                    case GeofenceCentreOn.Airport:
                        feed.Latitude = feed.Longitude = null;
                        feed.PilotCid = null;
                        feed.AirportCode = (feed.AirportCode ?? "????").Trim().ToUpperInvariant();
                        if(feed.AirportCode.Length > 4) {
                            feed.AirportCode = feed.AirportCode.Substring(0, 4);
                        }
                        break;
                    case GeofenceCentreOn.Coordinate:
                        feed.Latitude = Math.Min(90.0, Math.Max(-90, feed.Latitude ?? 0.0));
                        feed.Longitude = Math.Min(180.0, Math.Max(-180, feed.Longitude ?? 0.0));
                        feed.AirportCode = null;
                        feed.PilotCid = null;
                        break;
                    case GeofenceCentreOn.PilotCid:
                        feed.AirportCode = null;
                        feed.Latitude = feed.Longitude = null;
                        feed.PilotCid = feed.PilotCid ?? 0;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                const double earthCircumferenceKilometres = 40075.0;
                feed.Width = Math.Max(0.0, Math.Min(earthCircumferenceKilometres, feed.Width));
                feed.Height = Math.Max(0.0, Math.Min(earthCircumferenceKilometres, feed.Height));
            }
        }

        /// <summary>
        /// Adds default geofeed settings to a new object.
        /// </summary>
        /// <returns></returns>
        public Options AddDefaultGeofeeds()
        {
            GeofencedFeeds.Add(new GeofenceFeedOption() {
                FeedName =      "UK and Ireland",
                CentreOn =      GeofenceCentreOn.Coordinate,
                Latitude =      54.497989,
                Longitude =     -4.555628,
                Width =         560.0,
                Height =        740.0,
                DistanceUnit =  DistanceUnit.Miles,
            });
            GeofencedFeeds.Add(new GeofenceFeedOption() {
                FeedName =      "Heathrow",
                CentreOn =      GeofenceCentreOn.Airport,
                AirportCode =   "EGLL",
                Width =         80,
                Height =        80,
                DistanceUnit =  DistanceUnit.Miles,
            });

            return this;
        }
    }
}
