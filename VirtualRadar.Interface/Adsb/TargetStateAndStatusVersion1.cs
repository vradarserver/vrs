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
    /// Describes the content of an ADS-B version 1 Target State and Status message.
    /// </summary>
    public class TargetStateAndStatusVersion1
    {
        /// <summary>
        /// Gets or sets the source of the vertical state data.
        /// </summary>
        public VerticalDataSource VerticalDataSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that altitudes are barometric corrected altitudes (mean sea level)
        /// or a reference barometric altitude (flight level).
        /// </summary>
        public bool AltitudesAreMeanSeaLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which target altitudes can be sent by the transponder.
        /// </summary>
        public TargetAltitudeCapability TargetAltitudeCapability { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target altitude has been reached or whether the aircraft is moving towards it.
        /// </summary>
        public VerticalModeIndicator VerticalModeIndicator { get; set; }

        /// <summary>
        /// Gets or sets the target altitude in feet.
        /// </summary>
        public int? TargetAltitude { get; set; }

        /// <summary>
        /// Gets or sets the source of the horizontal state data.
        /// </summary>
        public HorizontalDataSource HorizontalDataSource { get; set; }

        /// <summary>
        /// Gets or sets the target heading or track angle.
        /// </summary>
        public short? TargetHeading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TargetHeading"/> is the track (true) or heading (false) angle.
        /// </summary>
        public bool TargetHeadingIsTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target heading has been reached or whether the aircraft is moving towards it.
        /// </summary>
        public HorizontalModeIndicator HorizontalModeIndicator { get; set; }

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
        /// Gets or sets the surveillance integrity level.
        /// </summary>
        /// <remarks>
        /// Another value I don't really care about and haven't translated into radii.
        /// </remarks>
        public byte Sil { get; set; }

        /// <summary>
        /// Gets or sets the current state of the TCAS/ACAS system.
        /// </summary>
        public TcasCapabilityMode TcasCapabilityMode { get; set; }

        /// <summary>
        /// Gets or sets the current emergency state for the aircraft.
        /// </summary>
        public EmergencyState EmergencyState { get; set; }

        /// <summary>
        /// Returns an English description of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("V1");

            result.AppendFormat(" VDS:{0}", (int)VerticalDataSource);
            result.AppendFormat(" VMSL:{0}", AltitudesAreMeanSeaLevel ? '1' : '0');
            result.AppendFormat(" TAC:{0}", (int)TargetAltitudeCapability);
            result.AppendFormat(" VMI:{0}", (int)VerticalModeIndicator);
            if(TargetAltitude != null) result.AppendFormat(" ALT:{0}", TargetAltitude);
            result.AppendFormat(" HDS:{0}", (int)HorizontalDataSource);
            if(TargetHeading != null) result.AppendFormat(" HDG:{0}", TargetHeading);
            result.AppendFormat(" HDG-T:{0}", TargetHeadingIsTrack ? '1' : '0');
            result.AppendFormat(" HMI:{0}", (int)HorizontalModeIndicator);
            result.AppendFormat(" NACP:{0}", NacP);
            result.AppendFormat(" NICBA:{0}", NicBaro);
            result.AppendFormat(" SIL:{0}", Sil);
            result.AppendFormat(" TCC:{0}", (int)TcasCapabilityMode);
            result.AppendFormat(" ES:{0}", (int)EmergencyState);

            return result.ToString();
        }
    }
}
