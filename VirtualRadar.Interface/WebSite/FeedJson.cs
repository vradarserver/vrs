﻿// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Runtime.Serialization;
using VirtualRadar.Interface.Feeds;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON object that describes a data feed from a receiver.
    /// </summary>
    [DataContract]
    public class FeedJson
    {
        /// <summary>
        /// Gets or sets the unique ID of the feed.
        /// </summary>
        [DataMember(Name="id", IsRequired=true)]
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the current name of the feed.
        /// </summary>
        [DataMember(Name="name", IsRequired=true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that a polar plot exists for the feed.
        /// </summary>
        [DataMember(Name="polarPlot", IsRequired=true)]
        public bool HasPolarPlot { get; set; }

        /// <summary>
        /// Constructs a <see cref="FeedJson"/> object from a feed interface.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public static FeedJson ToModel(IFeed feed)
        {
            FeedJson result = null;

            if(feed?.IsVisible == true) {
                var polarPlottingAircraftList = feed.AircraftList as IPolarPlottingAircraftList;

                result = new FeedJson() {
                    UniqueId =      feed.UniqueId,
                    Name =          feed.Name,
                    HasPolarPlot =  polarPlottingAircraftList?.PolarPlotter != null,
                };
            }

            return result;
        }
    }
}
