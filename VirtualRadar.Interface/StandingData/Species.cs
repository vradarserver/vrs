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

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// An enumeration of the different species of flying machine described by ICAO8643.
    /// </summary>
    /// <remarks>
    /// The numbers associated with these entries are sent to the browsers. Try to keep them unchanged across releases.
    /// </remarks>
    public enum Species
    {
        /// <summary>
        /// Species is not known or not applicable.
        /// </summary>
        None = 0,

        /// <summary>
        /// A fixed-wing aircraft that can only land or take off from terra firma.
        /// </summary>
        Landplane = 1,

        /// <summary>
        /// A fixed-wing aircraft that can only land or take off from a body of water.
        /// </summary>
        Seaplane = 2,

        /// <summary>
        /// A fixed-wing aircraft that can land or take off either from land or water.
        /// </summary>
        Amphibian = 3,

        /// <summary>
        /// A rotary-wing aircraft whose rotors are driven by a motor.
        /// </summary>
        Helicopter = 4,

        /// <summary>
        /// A rotary-wing aircraft whose rotors are not driven by a motor.
        /// </summary>
        Gyrocopter = 5,

        /// <summary>
        /// An aircraft whose wing (and the engines / propellers attached) are horizontal
        /// during forward flight but can be tilted 90° upwards to allow it to take off
        /// and land vertically.
        /// </summary>
        TiltWing = 6,

        /// <summary>
        /// A ground vehicle.
        /// </summary>
        GroundVehicle = 7,

        /// <summary>
        /// A tower or radio beacon of some kind.
        /// </summary>
        Tower = 8,
    }
}
