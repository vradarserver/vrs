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
using System.Xml.Serialization;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Listener
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

        private Dictionary<int, PolarPlot> _PolarPlots = new Dictionary<int,PolarPlot>();
        /// <summary>
        /// Gets a dictionary of angles clockwise from 0° north to the last-seen plot.
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, PolarPlot> PolarPlots
        {
            get { return _PolarPlots; }
        }

        private List<SavedPolarPlotAngle> _Angles = new List<SavedPolarPlotAngle>();
        /// <summary>
        /// Gets or sets a serialisable version of <see cref="PolarPlots"/>.
        /// </summary>
        /// <remarks>
        /// This is used to support serialisation and is not intended for use by the application.
        /// </remarks>
        public List<SavedPolarPlotAngle> Angles
        {
            get { return _Angles; }
        }

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

        /// <summary>
        /// Copies the <see cref="PolarPlots"/> dictionary into <see cref="Angles"/>.
        /// </summary>
        /// <remarks>
        /// Not great, but it will work and I don't need anything fancier at the moment.
        /// Necessary because XmlSerializer doesn't support dictionaries.
        /// </remarks>
        public void PrepareForSerialisation()
        {
            Angles.Clear();
            Angles.AddRange(PolarPlots.Where(r => r.Value.Distance != 0).Select(r => new SavedPolarPlotAngle(r.Key, r.Value)));
        }

        /// <summary>
        /// Copies the <see cref="Angles"/> into <see cref="PolarPlots"/>.
        /// </summary>
        /// <remarks>
        /// Not great, but it will work and I don't need anything fancier at the moment.
        /// Necessary because XmlSerializer doesn't support dictionaries.
        /// </remarks>
        public void FinishDeserialisation()
        {
            PolarPlots.Clear();
            foreach(var savedPolarPlotAngle in Angles) {
                PolarPlots.Add(savedPolarPlotAngle.Angle, savedPolarPlotAngle.PolarPlot);
            }

            Angles.Clear();
        }
    }
}
