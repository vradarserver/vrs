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
    /// A class carrying the content of an ADS-B version 2 target state and status message.
    /// </summary>
    public class TargetStateAndStatusVersion2
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reporting SIL probability is based on a per-sample (true) or per-hour (false) probability.
        /// </summary>
        public bool SilSupplement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SelectedAltitude"/> is derived from the Flight Management System (true) or the
        /// autopilot Mode Control Panel / Flight Control Unit (false).
        /// </summary>
        public bool SelectedAltitudeIsFms { get; set; }

        /// <summary>
        /// Gets or sets the autopilot / FMS selected altitude in feet.
        /// </summary>
        public int? SelectedAltitude { get; set; }

        /// <summary>
        /// Gets or sets the barometric pressure setting.
        /// </summary>
        /// <remarks>
        /// This is transmitted as the pressure setting minus 800 millibars. The 800 gets added back on before being reported here.
        /// </remarks>
        public float? BarometricPressureSetting { get; set; }

        /// <summary>
        /// Gets or sets the autopilot / FMS selected heading in degrees.
        /// </summary>
        public double? SelectedHeading { get; set; }

        /// <summary>
        /// Gets or sets the navigational accuracy category for position.
        /// </summary>
        /// <remarks>
        /// I'm not too interested in these and can't be bothered to type out the enumeration for the different values, or decode them
        /// into radii. Something to add in a later version perhaps?
        /// </remarks>
        public byte NacP { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the integrity category for barometric altitudes.
        /// </summary>
        /// <remarks><para>
        /// If this is false then the altitude reported in airborne position messages is a Gillham altitude that has not been
        /// cross-checked against other sources of pressure altitude.
        /// </para><para>
        /// If this is true then the altitude reported in airborne position messages is either a Gillham altitude that has been
        /// cross-checked against other sources and is verified as being consistent or it is not based on a Gillham source of
        /// pressure altitude.
        /// </para></remarks>
        public bool NicBaro { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the probability of exceeding the NIC containment radius.
        /// </summary>
        /// <remarks>This is different to the version 1 SIL but I still don't really care about it. I'll translate the values into
        /// probabilities in a later version of the program.</remarks>
        public byte Sil { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the autopilot is flying the aircraft.
        /// </summary>
        public bool? IsAutopilotEngaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the autopilot's vertical navigation mode is active.
        /// </summary>
        public bool? IsVnavEngaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the autopilot's altitude hold mode is active.
        /// </summary>
        public bool? IsAltitudeHoldActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the message has been rebroadcast by a ground link.
        /// </summary>
        public bool IsRebroadcast { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the autopilot's approach mode is active.
        /// </summary>
        public bool? IsApproachModeActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the TCAS/ACAS system is operational.
        /// </summary>
        public bool IsTcasOperational { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the autopilot's lateral navigation mode is active.
        /// </summary>
        public bool? IsLnavEngaged { get; set; }

        /// <summary>
        /// Returns an English description of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("V2");

            result.AppendFormat(" SILP:{0}", SilSupplement ? '1' : '0');
            result.AppendFormat(" ALTF:{0}", SelectedAltitudeIsFms ? '1' : '0');
            if(SelectedAltitude != null) result.AppendFormat(" ALT:{0}", SelectedAltitude);
            if(BarometricPressureSetting != null) result.AppendFormat(" QNH:{0}", BarometricPressureSetting);
            if(SelectedHeading != null) result.AppendFormat(" HDG:{0}", SelectedHeading);
            result.AppendFormat(" NACP:{0}", NacP);
            result.AppendFormat(" NICBA:{0}", NicBaro);
            result.AppendFormat(" SIL:{0}", Sil);
            if(IsAutopilotEngaged != null) result.AppendFormat(" APE:{0}", IsAutopilotEngaged.Value ? '1' : '0');
            if(IsVnavEngaged != null) result.AppendFormat(" VNAV:{0}", IsVnavEngaged.Value ? '1' : '0');
            if(IsAltitudeHoldActive != null) result.AppendFormat(" ALTH:{0}", IsAltitudeHoldActive.Value ? '1' : '0');
            result.AppendFormat(" ADSR:{0}", IsRebroadcast ? '1' : '0');
            if(IsApproachModeActive != null) result.AppendFormat(" APP:{0}", IsApproachModeActive.Value ? '1' : '0');
            result.AppendFormat(" TCOP:{0}", IsTcasOperational ? '1' : '0');
            if(IsLnavEngaged != null) result.AppendFormat(" LNAV:{0}", IsLnavEngaged.Value ? '1' : '0');

            return result.ToString();
        }
    }
}
