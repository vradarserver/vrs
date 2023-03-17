﻿// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Xml.Serialization;

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// A DTO that carries a slice of polar plots for a given receiver.
    /// </summary>
    public class PolarPlotSlice : ICloneable
    {
        /// <summary>
        /// Gets or sets the lower-bound of the altitude that the plot covers.
        /// </summary>
        public int AltitudeLower { get; set; }

        /// <summary>
        /// Gets or sets the upper-bound of the altitude that the plot covers.
        /// </summary>
        public int AltitudeHigher { get; set; }

        /// <summary>
        /// Gets a dictionary of angles clockwise from 0° north to the last-seen plot.
        /// </summary>
        [XmlIgnore]
        public IDictionary<int, PolarPlot> PolarPlots { get; } = new Dictionary<int,PolarPlot>();

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var result = new PolarPlotSlice() {
                AltitudeLower = AltitudeLower,
                AltitudeHigher = AltitudeHigher,
            };
            foreach(var kvp in PolarPlots) {
                result.PolarPlots.Add(kvp.Key, (PolarPlot)kvp.Value.Clone());
            }

            return result;
        }
    }
}
