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
using VirtualRadar.Interface.Settings;
using System.Collections.ObjectModel;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The interface for objects that can track the values required for a polar plot.
    /// </summary>
    /// <remarks><para>
    /// Polar plots are recorded over a series of slices, where each slice is a range of altitudes between
    /// two values in feet. The Initialise call tells the plotter the dimensions of the slices and the
    /// location to measure distances and angles from.
    /// </para><para>
    /// The plotter calculates the distance and bearing to the aircraft and then rounds the bearing to the
    /// nearest X degrees. If X is 1 then you will get 360 bearings stored for each slice, if X is 5 then you will
    /// get (360 / 5) 72 points stored for each slice. Once the rounded bearing has been calculated the plotter
    /// looks to see whether the distance it's calculated is further out than the one that already exists for the
    /// bearing - if it is then that distance and point is recorded, otherwise it is discarded.
    /// </para><para>
    /// All altitudes and positions are passed through an implementation of a IAircraftSanityChecker and only
    /// those values that are confirmed as ProbablyRight are used for the plot.
    /// </para><para>
    /// Implementations must be thread safe.
    /// </para>
    /// </remarks>
    public interface IPolarPlotter : IDisposable
    {
        /// <summary>
        /// Gets the latitude from which distances will be calculated.
        /// </summary>
        double Latitude { get; }

        /// <summary>
        /// Gets the longitude from which distances will be calculated.
        /// </summary>
        double Longitude { get; }

        /// <summary>
        /// Gets the number of degrees that bearings are rounded to.
        /// </summary>
        int RoundToDegrees { get; }

        /// <summary>
        /// Initialises the plotter with a standard set of slices and bearing rounding.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <remarks>
        /// The standard slices are -2.1 billion to 9999', 10000 to 19999', 20000 to 29999' and 30000 to 2.1 billion feet,
        /// plus another that covers -2.1 billion to 2.1 billion feet. Bearings are rounded to the nearest degree.
        /// </remarks>
        void Initialise(double latitude, double longitude);

        /// <summary>
        /// Initialises the plotter with custom slice dimensions.
        /// </summary>
        /// <param name="latitude">The latitude to measure distances from.</param>
        /// <param name="longitude">The longitude to measure distances from.</param>
        /// <param name="lowSliceAltitude">The lowest altitude to record distances for.</param>
        /// <param name="highSliceAltitude">The highest altitude to record distances for.</param>
        /// <param name="sliceHeight">The height in feet of each slice.</param>
        /// <param name="roundToDegrees">The number of degrees to round to.</param>
        void Initialise(double latitude, double longitude, int lowSliceAltitude, int highSliceAltitude, int sliceHeight, int roundToDegrees);

        /// <summary>
        /// Adds a coordinate to all applicable slices. The coordinate will be checked by an instance of <see cref="IAircraftSanityChecker"/>.
        /// </summary>
        /// <param name="aircraftId">The unique identifier of the aircraft.</param>
        /// <param name="altitude">Altitude of the aircraft in feet.</param>
        /// <param name="latitude">The latitude of the aircraft.</param>
        /// <param name="longitude">The longitude of the aircraft.</param>
        /// <remarks>
        /// The plotter calculates the distance and bearing from <see cref="Latitude"/> and <see cref="Longitude"/>
        /// to the latitude and longitude passed across. If the distance is the furthest seen for that bearing then
        /// it is recorded in all slices that cover the altitude passed across, otherwise it is discarded.
        /// </remarks>
        void AddCoordinate(int aircraftId, int altitude, double latitude, double longitude);

        /// <summary>
        /// Adds a coordinate to all applicable slices. The coordinate is guaranteed to have been considered <see cref="Certainty.ProbablyRight"/>
        /// by an instance of <see cref="IAircraftSanityChecker"/>.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="altitude"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <remarks>
        /// Identical to <see cref="AddCoordinate"/> except that the coordinate is not checked to see whether it is
        /// reasonable before it is used. If another sanity checker has already tested the coordinate then this method
        /// is more efficient, it avoids double-checking.
        /// </remarks>
        void AddCheckedCoordinate(int aircraftId, int altitude, double latitude, double longitude);

        /// <summary>
        /// Returns a copy of all of the slices as-at the current point in time.
        /// </summary>
        /// <returns></returns>
        List<PolarPlotSlice> TakeSnapshot();

        /// <summary>
        /// Removes all existing polar plots.
        /// </summary>
        void ClearPolarPlots();

        /// <summary>
        /// Overwrites the <see cref="RoundToDegrees"/> and slices with the values from the saved polar plot passed across.
        /// </summary>
        /// <param name="savedPolarPlot"></param>
        /// <remarks>
        /// The savedPolarPlot is not checked to make sure that the latitude and longitude are correct. It is assumed that the
        /// caller has already checked that they are valid, or at least close enough for jazz.
        /// </remarks>
        void LoadFrom(SavedPolarPlot savedPolarPlot);
    }
}
