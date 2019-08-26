// Copyright © 2016 onwards, Andrew Whewell
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
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can take a list of air pressures and from that
    /// give you the closest air pressure reading for a point on the surface of the earth.
    /// </summary>
    /// <remarks>
    /// Implementations must be thread-safe.
    /// </remarks>
    [Singleton]
    public interface IAirPressureLookup : ISingleton<IAirPressureLookup>
    {
        /// <summary>
        /// Gets the last fetch time passed to <see cref="LoadAirPressures"/>.
        /// </summary>
        DateTime FetchTimeUtc { get; }

        /// <summary>
        /// Gets the number of <see cref="AirPressure"/> records loaded.
        /// </summary>
        int CountAirPressuresLoaded { get; }

        /// <summary>
        /// Erases all previously loaded pressures and stores a new set.
        /// </summary>
        /// <param name="airPressures"></param>
        /// <param name="fetchTimeUtc"></param>
        void LoadAirPressures(IEnumerable<AirPressure> airPressures, DateTime fetchTimeUtc);

        /// <summary>
        /// Returns the closest air pressure reading to a point on the globe. Can return null if
        /// no air pressures have been loaded.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        AirPressure FindClosest(double latitude, double longitude);
    }
}
