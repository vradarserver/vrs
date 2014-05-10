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
    /// A class that describes the content of ADS-B Aircraft Operational Status messages.
    /// </summary>
    public class AircraftOperationalStatusMessage
    {
        /// <summary>
        /// Gets or sets the ADS-B version that the transmitter is using.
        /// </summary>
        public byte AdsbVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of information carried by the message.
        /// </summary>
        public AircraftOperationalStatusType AircraftOperationalStatusType { get; set; }

        /// <summary>
        /// Gets or sets the airborne capability flags. The meaning of the flags depends upon the <see cref="AdsbVersion"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="AdsbVersion"/> is 0 then the flags are as per the <see cref="AirborneCapabilityVersion0"/> flags enumeration.
        /// If <see cref="AdsbVersion"/> is 1 then the flags are as per the <see cref="AirborneCapabilityVersion1"/> flags enumeration.
        /// If <see cref="AdsbVersion"/> is 2 then the flags are as per the <see cref="AirborneCapabilityVersion2"/> flags enumeration.
        /// Otherwise the property will be set to null.
        /// </remarks>
        public ushort? AirborneCapability { get; set; }

        /// <summary>
        /// Gets or sets the surface capability flags.
        /// </summary>
        public SurfaceCapability? SurfaceCapability { get; set; }

        /// <summary>
        /// Gets or sets the upper-bound length of the vehicle in metres.
        /// </summary>
        /// <remarks>
        /// The length of the vehicle is, at most, this many metres - except when it is over 85 metres long or
        /// 90 metres wide, in which case this value is set to 85.
        /// </remarks>
        public float? MaximumLength { get; set; }

        /// <summary>
        /// Gets or sets the upper-bound width of the vehicle in metres.
        /// </summary>
        /// <remarks>
        /// The width of the vehicle is, at most, this many metres - except when it is over 85 metres long or
        /// 90 metres wide, in which case this value is set to 90.
        /// </remarks>
        public float? MaximumWidth { get; set; }

        /// <summary>
        /// Gets or sets the operational modes that are active on board the aircraft.
        /// </summary>
        public OperationalMode? OperationalMode { get; set; }

        /// <summary>
        /// Gets or sets the SDA value extracted from the <see cref="OperationalMode"/> value in version 2 messages.
        /// </summary>
        public SystemDesignAssurance? SystemDesignAssurance { get; set; }

        /// <summary>
        /// Gets or sets the offset, in metres, of the GPS antenna from the longitudinal centre line of the aircraft. Positive
        /// values indicate an offset towards the left wingtip, negative values are an offset towards the right wingtip.
        /// </summary>
        /// <remarks>The largest offset that the message can represent is six metres. Offsets over 6m are shown as 6.</remarks>
        public short? LateralAxisGpsOffset { get; set; }

        /// <summary>
        /// Gets or sets the offset, in metres, of the GPS antenna from the nose of the aircraft.
        /// </summary>
        /// <remarks>The largest offset that the message can represent is sixty metres. Offsets over 60m are shown as 60.</remarks>
        public byte? LongitudinalAxisGpsOffset { get; set; }

        /// <summary>
        /// Gets or sets the value of the NIC Supplement A field.
        /// </summary>
        public byte? NicA { get; set; }

        /// <summary>
        /// Gets or sets the value of the NIC Supplement C field.
        /// </summary>
        public byte? NicC { get; set; }

        /// <summary>
        /// Gets or sets the navigational accuracy category for positions.
        /// </summary>
        /// <remarks>I don't bother decoding this, it is just the raw value from the message.</remarks>
        public byte? NacP { get; set; }

        /// <summary>
        /// Gets or sets the geometric vertical accuracy code.
        /// </summary>
        /// <remarks>I don't bother decoding this, it is just the raw value from the message.</remarks>
        public byte? Gva { get; set; }

        /// <summary>
        /// Gets or sets the source integrity level code.
        /// </summary>
        /// <remarks>I don't bother decoding this, it is just the raw value from the message.</remarks>
        public byte? Sil { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reporting SIL probability is based on a per-sample (true) or per-hour (false) probability.
        /// </summary>
        public bool? SilSupplement { get; set; }

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
        public bool? NicBaro { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the track angle in surface position messages is the target heading angle (false) or the track angle (true).
        /// </summary>
        public bool? SurfacePositionAngleIsTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether horizontal angles are references against true north (false) or magnetic north (true).
        /// </summary>
        public bool? HorizontalReferenceIsMagneticNorth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the message has been rebroadcast by a ground link. It's unlikely that we shall ever see this set.
        /// </summary>
        public bool? IsRebroadcast { get; set; }

        /// <summary>
        /// Returns an English description of the message content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("AOS");

            result.AppendFormat(" V:{0}", AdsbVersion);
            result.AppendFormat(" AST:{0}", (int)AircraftOperationalStatusType);
            if(AirborneCapability != null) result.AppendFormat(" AC:{0}", (int)AirborneCapability);
            if(SurfaceCapability != null) result.AppendFormat(" SC:{0}", (int)SurfaceCapability);
            if(MaximumLength != null) result.AppendFormat(" MLN:{0}", MaximumLength);
            if(MaximumWidth != null) result.AppendFormat(" MWD:{0}", MaximumWidth);
            if(OperationalMode != null) result.AppendFormat(" OM:{0}", (int)OperationalMode);
            if(SystemDesignAssurance != null) result.AppendFormat(" SDA:{0}", (int)SystemDesignAssurance);
            if(LateralAxisGpsOffset != null) result.AppendFormat(" GLAT:{0}", LateralAxisGpsOffset);
            if(LongitudinalAxisGpsOffset != null) result.AppendFormat(" GLNG:{0}", LongitudinalAxisGpsOffset);
            if(NicA != null) result.AppendFormat(" NICA:{0}", NicA);
            if(NicC != null) result.AppendFormat(" NICC:{0}", NicC);
            if(NacP != null) result.AppendFormat(" NACP:{0}", NacP);
            if(Gva != null) result.AppendFormat(" GVA:{0}", Gva);
            if(Sil != null) result.AppendFormat(" SIL:{0}", Sil);
            if(SilSupplement != null) result.AppendFormat(" SILP:{0}", SilSupplement.Value ? '1' : '0');
            if(NicBaro != null) result.AppendFormat(" NICB:{0}", NicBaro);
            if(SurfacePositionAngleIsTrack != null) result.AppendFormat(" SPT:{0}", SurfacePositionAngleIsTrack.Value ? '1' : '0');
            if(HorizontalReferenceIsMagneticNorth != null) result.AppendFormat(" HRD:{0}", HorizontalReferenceIsMagneticNorth.Value ? '1' : '0');
            if(IsRebroadcast != null) result.AppendFormat(" ADSR:{0}", IsRebroadcast.Value ? '1' : '0');

            return result.ToString();
        }
    }
}
