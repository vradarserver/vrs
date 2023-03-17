// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.Feeds;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Carries the details for an individual receiver's polar plot.
    /// </summary>
    public class SavedPolarPlot
    {
        /// <summary>
        /// Gets or sets the unique identifier of the feed that the polar plot is for.
        /// </summary>
        public int FeedId { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the receiver.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the receiver.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the rounding applied to the polar plotter.
        /// </summary>
        public int RoundToDegrees { get; set; }

        private List<PolarPlotSlice> _PolarPlotSlices = new List<PolarPlotSlice>();
        /// <summary>
        /// Gets a list of saved polar plot slices.
        /// </summary>
        public List<PolarPlotSlice> PolarPlotSlices
        {
            get { return _PolarPlotSlices; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SavedPolarPlot()
        {
        }

        /// <summary>
        /// Creates a saved polar plot from a feed.
        /// </summary>
        /// <param name="feed"></param>
        public SavedPolarPlot(IFeed feed)
        {
            var aircraftList = feed?.AircraftList as IPolarPlottingAircraftList;
            var polarPlotter = aircraftList?.PolarPlotter;
            var slices = polarPlotter?.TakeSnapshot();

            if(slices != null) {
                FeedId = feed.UniqueId;
                Latitude = polarPlotter.Latitude;
                Longitude = polarPlotter.Longitude;
                RoundToDegrees = polarPlotter.RoundToDegrees;
                PolarPlotSlices.AddRange(slices);
            }
        }

        /// <summary>
        /// Returns true if the feed passed across looks to be a good match for this saved polar plot.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public bool IsForSameFeed(IFeed feed)
        {
            var result = false;

            var aircraftList = feed?.AircraftList as IPolarPlottingAircraftList;
            var polarPlotter = aircraftList?.PolarPlotter;
            var slices = polarPlotter?.TakeSnapshot();

            if(slices != null) {
                const double roundMultiplier = 10000.0;        // Latitude & Longitude need to be the same to 4 decimal places
                result =  FeedId == feed.UniqueId
                       && (long)(Latitude * roundMultiplier) == (long)(polarPlotter.Latitude * roundMultiplier)
                       && (long)(Longitude * roundMultiplier) == (long)(polarPlotter.Longitude * roundMultiplier);
            }

            return result;
        }
    }
}
