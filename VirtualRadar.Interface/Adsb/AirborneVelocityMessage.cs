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
    /// Gets or sets the content of an airborne velocity ADS-B message.
    /// </summary>
    public class AirborneVelocityMessage
    {
        /// <summary>
        /// Gets or sets the type of velocity encoded within the message, specifically whether it is airspeed or ground speed.
        /// </summary>
        public VelocityType VelocityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the Intent Change flag in a velocity message was set.
        /// </summary>
        public bool ChangeOfIntent { get; set; }

        /// <summary>
        /// Gets or sets the margin for error for the horizontal velocity values.
        /// </summary>
        /// <remarks>
        /// Positive values indicate an error that is greater than or equal to the value in metres/second. Negative values
        /// indicate a margin for error that is less than the absolute value in metres/second. So a value of 10.0 indicates
        /// a margin for error greater than or equal to 10 m/s whereas a value of -10 indicates a margin for error that
        /// is less than 10 m/s. Null values indicate that the value was undefined by the documentation.
        /// </remarks>
        public float? HorizontalVelocityError { get; set; }

        /// <summary>
        /// Gets or sets the velocity and bearing when expressed in an ADS-B message as a pair of speeds along the X/Y axes.
        /// </summary>
        /// <remarks><para>
        /// At the time of writing this is used when the speed is the ground speed and the bearing the ground track. However you
        /// should consult the <see cref="VelocityType"/> property to confirm that this is not the airspeed and heading.
        /// </para><para>If this value is supplied then <see cref="Heading"/> and <see cref="Airspeed"/> will be null.</para>
        /// </remarks>
        public VectorVelocity VectorVelocity { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's heading (not its ground track).
        /// </summary>
        /// <remarks>
        /// If this value is supplied then <see cref="Airspeed"/> will also indicate the airspeed (although it can be null if
        /// the airspeed is unknown) and <see cref="VectorVelocity"/> will be null.
        /// </remarks>
        public double? Heading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Airspeed"/> property indicates true airspeed or indicated airspeed.
        /// </summary>
        public bool AirspeedIsTrueAirspeed { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's airspeed in knots.
        /// </summary>
        /// <remarks>
        /// If this value is supplied then <see cref="Heading"/> will also indicate the heading (although it can be null if
        /// the heading is unknown) and <see cref="VectorVelocity"/> will be null.
        /// </remarks>
        public double? Airspeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft's airspeed is higher than the maximum value that ADS-B messages can carry.
        /// </summary>
        /// <remarks>
        /// If this true then the aircraft's actual airspeed is higher than the value held in <see cref="Airspeed"/>.
        /// </remarks>
        public bool AirspeedExceeded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the source for the vertical rate is barometric as opposed to geometric.
        /// </summary>
        public bool VerticalRateIsBarometric { get; set; }

        /// <summary>
        /// Gets or sets the vertical rate in feet per minute. Positive values indicate that the vehicle is ascending, negative values
        /// indicate the vehicle is descending.
        /// </summary>
        public int? VerticalRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the vertical rate exceeds the highest value that the ADS-B message can convey.
        /// </summary>
        /// <remarks>
        /// When this is true the actual vertical rate is higher than <see cref="VerticalRate"/>.
        /// </remarks>
        public bool VerticalRateExceeded { get; set; }

        /// <summary>
        /// Gets or sets the difference between geometric and barometric altitude. Positive values indicate geometric altitude is above
        /// barometric, negative values indicate geometric altitude is below barometric.
        /// </summary>
        public short? GeometricAltitudeDelta { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the difference between the geometric and barometric altitude sources exceeds the maximum
        /// value that ADS-B can convey.
        /// </summary>
        /// <remarks>
        /// If this is true then the difference between the geometric altitude and barometric altitude sources is larger than the value
        /// in <see cref="GeometricAltitudeDelta"/>.
        /// </remarks>
        public bool GeometricAltitudeDeltaExceeded { get; set; }

        /// <summary>
        /// Returns an English description of the message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("VEL");
            result.AppendFormat(" VT:{0}", (int)VelocityType);
            result.AppendFormat(" IC:{0}", ChangeOfIntent ? "1" : "0");
            result.AppendFormat(" NAC:{0}", HorizontalVelocityError);
            if(VectorVelocity != null) result.AppendFormat(" VV:{0}", VectorVelocity);
            result.AppendFormat(" SBV:{0}", VerticalRateIsBarometric ? "1" : "0");
            if(VerticalRate != null) result.AppendFormat(" VSI:{0}{1}", VerticalRate, VerticalRateExceeded ? "*" : "");
            if(GeometricAltitudeDelta != null) result.AppendFormat(" DBA:{0}{1}", GeometricAltitudeDelta, GeometricAltitudeDeltaExceeded ? "*" : "0");
            if(Heading != null) result.AppendFormat(" HDG:{0}", Heading);
            if(Airspeed != null) result.AppendFormat(" AS:{1}{2}{3}", AirspeedIsTrueAirspeed ? "T" : "I", Airspeed, AirspeedExceeded ? "*" : "0");

            return result.ToString();
        }
    }
}
