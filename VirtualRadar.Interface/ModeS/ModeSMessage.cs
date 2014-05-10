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

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// Describes the content of a downlink message from a Mode A/C/S transponder.
    /// </summary>
    /// <remarks>
    /// This object carries the information from all Mode-S transponder replies. Null properties were either not present
    /// in the reply or could not be decoded. The class is not thread-safe.
    /// </remarks>
    public class ModeSMessage
    {
        /// <summary>
        /// Gets or sets the signal level of the message, if known. Null if not known.
        /// </summary>
        public int? SignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the decoded downlink format (DF) field of the reply. This is present on all replies.
        /// </summary>
        public DownlinkFormat DownlinkFormat { get; set; }

        /// <summary>
        /// The backing field for <see cref="Icao24"/>.
        /// </summary>
        private int _Icao24;

        /// <summary>
        /// Gets or sets the ICAO24 identifier of the aircraft, either from the AP field or the AA field of the reply (as appropriate).
        /// </summary>
        /// <remarks>
        /// This can be zero for DF18 (Extended Squitter from Non-Transponder devices) replies if the <see cref="ControlField"/>
        /// indicates that the message is coming from a non-transponder device that does not broadcast ICAO24 addresses in its
        /// AA field. It can also be zero for downlink formats that are either reserved for future use or for DF19 (military
        /// extended squitter) where the aircraft address field is not defined by the spec.
        /// </remarks>
        public int Icao24
        {
            get { return _Icao24; }
            set { _Icao24 = value; _FormattedIcao24 = null; }
        }

        /// <summary>
        /// The backing field for <see cref="FormattedIcao24"/>.
        /// </summary>
        private string _FormattedIcao24;

        /// <summary>
        /// Gets the formatted ICAO24 code of the aircraft after any parity bits have been masked out (if applicable).
        /// </summary>
        public string FormattedIcao24
        {
            get
            {
                if(_FormattedIcao24 == null) _FormattedIcao24 = Icao24.ToString("X6");
                return _FormattedIcao24;
            }
        }

        /// <summary>
        /// Gets or sets the content of AA fields where the content is not an ICAO24 address.
        /// </summary>
        /// <remarks>
        /// This is filled for DF18 messages where the control field indicates that the transmitter is not using an ICAO24 address.
        /// </remarks>
        public int? NonIcao24Address { get; set; }

        /// <summary>
        /// Gets or sets the CA field of the reply.
        /// </summary>
        public Capability? Capability { get; set; }

        /// <summary>
        /// Gets or sets the 24-bit interrogator's identifier code with parity masked out (PI).
        /// </summary>
        public int? ParityInterrogatorIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the FS field from the reply.
        /// </summary>
        public FlightStatus? FlightStatus { get; set; }

        /// <summary>
        /// Gets or sets the DR field from the reply.
        /// </summary>
        public byte? DownlinkRequest { get; set; }

        /// <summary>
        /// Gets or sets the UM field from the reply.
        /// </summary>
        public byte? UtilityMessage { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet as described by the AC field from the reply.
        /// </summary>
        public int? Altitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the AC field had the M bit set, which indicates that <see cref="Altitude"/> is in metric units.
        /// </summary>
        /// <remarks>
        /// This is reserved for the formatting of altitude in metric units. However it appears that such an encoding has not yet (Jul 2012) been
        /// specified - if you see this set in a message then the message was likely corrupt and should be discarded.
        /// </remarks>
        public bool? AltitudeIsMetric { get; set; }

        /// <summary>
        /// Gets or sets the content of a 7-byte message being sent to the ground in the MB field of the reply.
        /// </summary>
        public byte[] CommBMessage { get; set; }

        /// <summary>
        /// Gets or sets the content of the ID field (the squawk code) from the reply.
        /// </summary>
        public short? Identity { get; set; }

        /// <summary>
        /// Gets or sets the content of the <see cref="CommBMessage"/> when the first byte indicates that it is a BDS2,0 reply.
        /// </summary>
        /// <remarks>
        /// Other BDS replies could potentially also decode to 0x20 in the first byte of the <see cref="CommBMessage"/> so the
        /// content of this field should be treated as suspect. If a more certain source of callsign information is available
        /// then that should be preferred over this.
        /// </remarks>
        public string PossibleCallsign { get; set; }

        /// <summary>
        /// Gets or sets the ELM control setting in the KE field of the reply.
        /// </summary>
        public ElmControl? ElmControl { get; set; }

        /// <summary>
        /// Gets or sets the content of the ND field from the reply.
        /// </summary>
        public byte? DSegmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the content of the 10-byte MD field from the reply.
        /// </summary>
        public byte[] CommDMessage { get; set; }

        /// <summary>
        /// Gets or sets the vertical status from the VS field of the reply.
        /// </summary>
        public VerticalStatus? VerticalStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the transponder supports the cross-link capability, as exposed by the CC field of the reply.
        /// </summary>
        public bool? CrossLinkCapability { get; set; }

        /// <summary>
        /// Gets or sets the ACAS sensitivity level from the SL field of the reply.
        /// </summary>
        public byte? SensitivityLevel { get; set; }

        /// <summary>
        /// Gets or sets the reply information from the RI field of the reply.
        /// </summary>
        /// <remarks>
        /// Originally I had this as an enum of the different values in 3.1.2.8.2.2 of the spec, but then when I read 3.1.2.8.4 it seemed to be
        /// saying that the RI field would change its meaning depending on values sent by the interrogator that elicited the ACAS reply. As
        /// we don't know what those values were we can't be sure what the meaning of RI is, and as the only useful information contained in
        /// 3.1.2.8.2.2 was the maximum cruise airspeed - which isn't really very interesting from a radar spotting perspective - I decided to
        /// just make it a raw number instead.
        /// </remarks>
        public byte? ReplyInformation { get; set; }

        /// <summary>
        /// Gets or sets the 7-byte information in the MV field of the reply.
        /// </summary>
        public byte[] ACASMessage { get; set; }

        /// <summary>
        /// Gets or sets the 7-byte broadcast message in the ME field of an extended squitter reply.
        /// </summary>
        public byte[] ExtendedSquitterMessage { get; set; }

        /// <summary>
        /// Gets or sets the content of the CF field for a non-transponder extended squitter reply.
        /// </summary>
        public ControlField? ControlField { get; set; }

        /// <summary>
        /// Gets or sets the 10-byte body of a DF18 reply when the control field is not 0, 1 or 6 in a DF18 reply, or the 13-byte body of a
        /// DF19 reply where the application field is not 0.
        /// </summary>
        /// <remarks><para>
        /// 3.1.2.8.7.2 states that the control field determines the content of DF18 messages. If the control field is set to 0
        /// (<see cref="ControlField"/>).AdsbDeviceTransmittingIcao24 then the <see cref="Icao24"/> and <see cref="ExtendedSquitterMessage"/>
        /// properties are set and this field is null. Otherwise <see cref="Icao24"/> is left at 0, <see cref="ExtendedSquitterMessage"/> is
        /// null and this property is filled with the ten bytes between the CF field and the PI field. I've labelled this as a MEX
        /// field in the <see cref="ToString"/>() description of the object but in reality the spec does not declare a name for
        /// the field and only describes the content for CF0 messages.
        /// </para><para>
        /// For DF19 messages the only messages that can potentially be decoded are those where the AF field is 0, although even this
        /// appears contentious. For all other message types the entire body of the message (which is likely to be encrypted) is placed in
        /// this block.
        /// </para></remarks>
        public byte[] ExtendedSquitterSupplementaryMessage { get; set; }

        /// <summary>
        /// Gets or sets the content of the AF field in a DF19 reply.
        /// </summary>
        public ApplicationField? ApplicationField { get; set; }

        /// <summary>
        /// Returns an English description of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder("DF:");
            result.Append((int)DownlinkFormat);
            result.AppendFormat(" ICAO24:{0}", FormattedIcao24);
            if(ApplicationField != null) result.AppendFormat(" AF:{0}", (int)ApplicationField);
            if(ControlField != null) result.AppendFormat(" CF:{0}", (int)ControlField);
            if(Capability != null) result.AppendFormat(" CA:{0}", (int)Capability);
            if(FlightStatus != null) result.AppendFormat(" FS:{0}", (int)FlightStatus);
            if(DownlinkRequest != null) result.AppendFormat(" DR:{0}", DownlinkRequest);
            if(UtilityMessage != null) result.AppendFormat(" UM:{0}", UtilityMessage);
            if(Altitude != null) result.AppendFormat(" AC:{0}", Altitude);
            if(AltitudeIsMetric.GetValueOrDefault()) result.Append(" AC:M");
            if(CommBMessage != null) result.AppendFormat(" MB:{0}", FormatBytes(CommBMessage));
            if(Identity != null) result.AppendFormat(" ID:{0:0000}", Identity);
            if(ElmControl != null) result.AppendFormat(" KE:{0}", (int)ElmControl);
            if(DSegmentNumber != null) result.AppendFormat(" ND:{0}", (int)DSegmentNumber);
            if(CommDMessage != null) result.AppendFormat(" MD:{0}", FormatBytes(CommDMessage));
            if(VerticalStatus != null) result.AppendFormat(" VS:{0}", (int)VerticalStatus);
            if(CrossLinkCapability != null) result.AppendFormat(" CC:{0}", CrossLinkCapability.Value ? '1' : '0');
            if(SensitivityLevel != null) result.AppendFormat(" SL:{0}", SensitivityLevel);
            if(ReplyInformation != null) result.AppendFormat(" RI:{0}", (int)ReplyInformation);
            if(ACASMessage != null) result.AppendFormat(" MV:{0}", FormatBytes(ACASMessage));
            if(ExtendedSquitterMessage != null) result.AppendFormat(" ME:{0}", FormatBytes(ExtendedSquitterMessage));
            if(ExtendedSquitterSupplementaryMessage != null) result.AppendFormat(" MEX:{0}", FormatBytes(ExtendedSquitterSupplementaryMessage));
            if(ParityInterrogatorIdentifier != null) result.AppendFormat(" PI:{0:X6}", ParityInterrogatorIdentifier);

            return result.ToString();
        }

        private string FormatBytes(byte[] bytes)
        {
            var result = new StringBuilder();
            for(var i = 0;i < bytes.Length;++i) {
                if(i != 0) result.Append('/');
                result.AppendFormat("{0:X2}", bytes[i]);
            }

            return result.ToString();
        }
    }
}
