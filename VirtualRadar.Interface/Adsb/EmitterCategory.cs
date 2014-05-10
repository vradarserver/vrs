// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// An enumeration of the different ADS-B Emitter Categories transmitted across all category sets.
    /// </summary>
    /// <remarks>
    /// Many of the possible categories are reserved for future use. These are encoded using the following
    /// formula: value = 100 + category + ((category set - 'A') * 10). E.G. category 6 in set C would have
    /// a value of 100 + 6 + (2 * 10) = 126. The reason why a single 'Reserved' enum is not used is because
    /// the decoder would be stripping out information carried by the message. If there is a vehicle
    /// transmitting a reserved code then I'd like to know what it sent.
    /// </remarks>
    public enum EmitterCategory : byte
    {
        /// <summary>
        /// No ADS-B emitter category transmitted.
        /// </summary>
        None,

        /// <summary>
        /// Light aircraft, usually less than 15,500 lbs maximum takeoff weight.
        /// </summary>
        LightAircraft,

        /// <summary>
        /// Small aircraft, usually between 15,500 to 75,000 lbs maximum takeoff weight.
        /// </summary>
        SmallAircraft,

        /// <summary>
        /// Large aircraft, usually between 75,000 to 300,000 lbs maximum takeoff weight.
        /// </summary>
        LargeAircraft,

        /// <summary>
        /// Large aircraft that generate high vorticies.
        /// </summary>
        HighVortexLargeAircraft,

        /// <summary>
        /// Heavy aircraft, usually with a maximum takeoff weight over 300,000 lbs.
        /// </summary>
        HeavyAircraft,

        /// <summary>
        /// Aircraft with greater than 5G acceleration and capable of travelling over 400 knots.
        /// </summary>
        HighPerformanceAircraft,

        /// <summary>
        /// Helicopter or other rotorcraft.
        /// </summary>
        Rotorcraft,

        /// <summary>
        /// Glider or sailplane.
        /// </summary>
        Glider,

        /// <summary>
        /// Hot-air balloon or other lighter-than-air craft.
        /// </summary>
        LighterThanAir,

        /// <summary>
        /// Parachutist or sky diver.
        /// </summary>
        Parachutist,

        /// <summary>
        /// Ultralight, hang-glider or paraglider.
        /// </summary>
        Ultralight,

        /// <summary>
        /// UAV.
        /// </summary>
        UnmannedAerialVehicle,

        /// <summary>
        /// Spacecraft or trans-atmospheric vehicle.
        /// </summary>
        SpaceVehicle,

        /// <summary>
        /// Emergency surface vehicle.
        /// </summary>
        SurfaceEmergencyVehicle,

        /// <summary>
        /// Service surface vehicle.
        /// </summary>
        SurfaceServiceVehicle,

        /// <summary>
        /// Point obstacle including tethered balloons.
        /// </summary>
        PointObstacle,

        /// <summary>
        /// Cluster obstacle.
        /// </summary>
        ClusterObstacle,

        /// <summary>
        /// Line obstacle.
        /// </summary>
        LineObstacle,

        /// <summary>
        /// Reserved codes are at least this value - see remarks for instructions on how to derive the category set and category.
        /// </summary>
        BaseReservedCode = 100,
    }
}
