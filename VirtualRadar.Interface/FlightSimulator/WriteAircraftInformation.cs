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
using System.Runtime.InteropServices;

namespace VirtualRadar.Interface.FlightSimulator
{
    /// <summary>
    /// The structure that is passed to <see cref="IFlightSimulator"/> to control the location, altitude, attitude and details
    /// of the simulated aircraft.
    /// </summary>
    /// <remarks>
    /// The order in which the fields appear in this structure is important - do not change it without also changing the
    /// order of field registration in <see cref="IFlightSimulator"/>'s implementation.
    /// </remarks>
    public struct WriteAircraftInformation
    {
        /// <summary>
        /// The aircraft's latitude.
        /// </summary>
        public double Latitude;

        /// <summary>
        /// The aircraft's longitude.
        /// </summary>
        public double Longitude;

        /// <summary>
        /// The aircraft's altitude in feet.
        /// </summary>
        public double Altitude;

        /// <summary>
        /// The aircraft's current indicated airspeed in knots.
        /// </summary>
        public double AirspeedIndicated;

        /// <summary>
        /// The aircraft's heading in degrees clockwise from north.
        /// </summary>
        public double TrueHeading;

        /// <summary>
        /// The aircraft's rate of climb or descent in feet per minute.
        /// </summary>
        public double VerticalSpeed;

        /// <summary>
        /// The aircraft's registration.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Registration;

        /// <summary>
        /// The aircraft's operator.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Operator;

        /// <summary>
        /// The aircraft's pitch in degrees.
        /// </summary>
        public double Pitch;

        /// <summary>
        /// The aircraft's bank in degrees.
        /// </summary>
        public double Bank;
    }
}
