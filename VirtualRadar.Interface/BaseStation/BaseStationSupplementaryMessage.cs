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

namespace VirtualRadar.Interface.BaseStation
{
    /// <summary>
    /// Extra information carried on some <see cref="BaseStationMessage"/>s that is gleaned from
    /// raw messages.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseStationMessage"/> class only conveys information from port 30003 feeds of
    /// data. Raw Mode-S and ADS-B messages can carry interesting bits of information that are not
    /// present on the port 30003 feed. The <see cref="IRawMessageTranslator"/> creates one of these
    /// objects and attaches it to the <see cref="BaseStationMessage"/> to pass that information on,
    /// or to qualify values that have been written to <see cref="BaseStationMessage"/> that are not
    /// in the same units that a port 30003 feed is expected to send.
    /// </remarks>
    public class BaseStationSupplementaryMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="BaseStationMessage.Altitude"/> is
        /// geometric (true) or barometric (false).
        /// </summary>
        /// <remarks>
        /// If there is no supplementary message then the altitude should be assumed to be barometric.
        /// </remarks>
        public bool? AltitudeIsGeometric { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="BaseStationMessage.VerticalRate"/> is
        /// from a geometric (true) or barometric (false) source.
        /// </summary>
        /// <remarks>
        /// If there is no supplementary message then the vertical rate should be assumed to be from
        /// a barometric source.
        /// </remarks>
        public bool? VerticalRateIsGeometric { get; set; }

        /// <summary>
        /// Gets or sets a value that qualifies the type of speed being reported in the
        /// <see cref="BaseStationMessage.GroundSpeed"/> property.
        /// </summary>
        /// <remarks>
        /// If there is no supplementary message then the GroundSpeed should be assumed to be the ground speed.
        /// </remarks>
        public SpeedType? SpeedType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="BaseStationMessage.Track"/> value is
        /// the aircraft's heading (true) or ground track (false).
        /// </summary>
        /// <remarks>
        /// If there is no supplementary message then the Track should be assumed to be the ground track.
        /// </remarks>
        public bool? TrackIsHeading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the callsign might be wrong.
        /// </summary>
        /// <remarks>
        /// This is set when the callsign was extracted from a Comm-B message where the first byte of the
        /// payload was 0x20, which *could* indicate that it is a BDS2,0 message. Other Comm-B messages
        /// might inadvertently set the same, so the callsign could be garbage. Once the raw message translator
        /// sees a callsign from an ADS-B message it will ignore further BDS2,0 callsigns from the same
        /// aircraft.
        /// </remarks>
        public bool? CallsignIsSuspect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the message carries some indication about the type of transponder
        /// carried by the aircraft. Note that this is not set for plain Mode-S messages or ADS-B messages which
        /// are identical for all versions of ADS-B - it is only set when the message plainly indicates a particular
        /// version of ADS-B.
        /// </summary>
        public TransponderType? TransponderType { get; set; }

        /// <summary>
        /// Gets or sets the target altitude setting from the aircraft's auto-pilot / FMS etc.
        /// </summary>
        public int? TargetAltitude { get; set; }

        /// <summary>
        /// Gets or sets the target heading from the aircraft's auto-pilot / FMS etc.
        /// </summary>
        public float? TargetHeading { get; set; }

        /// <summary>
        /// Gets or sets the barometric pressure setting in millibars. A value of zero indicates that the pressure setting
        /// was outside the bounds supported by the message type.
        /// </summary>
        public float? PressureSettingMb { get; set; }

        /// <summary>
        /// Gets the barometric pressure setting in inches of mercury.
        /// </summary>
        public float? PressureSettingInHg
        {
            get { return PressureSettingMb.GetValueOrDefault() == 0 ? (float?)null : AirPressure.MillibarsToInHg(PressureSettingMb); }
        }
    }
}
