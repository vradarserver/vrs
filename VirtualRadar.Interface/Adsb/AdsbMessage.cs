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
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// Describes the content of an ADS-B message as decoded from the ME field of a DF17 / DF18 CF0,1,6 / DF19 AF0 message.
    /// </summary>
    /// <remarks>
    /// Unlike Mode-S messages very few fields are transmitted by multiple message types. As some of the messages can carry a lot
    /// of unique information the body of the messages have been split into separate classes and this class just carries the
    /// common fields and a reference to one message object.
    /// </remarks>
    public class AdsbMessage
    {
        /// <summary>
        /// Gets the parent Mode-S message that carried the ADS-B message.
        /// </summary>
        public ModeSMessage ModeSMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the message as described by the first 5 bits.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Gets or sets the format of ADS-B message.
        /// </summary>
        public MessageFormat MessageFormat { get; set; }

        /// <summary>
        /// Gets or sets the value of the ICAO/Mode-A flag for TIS-B fine-format messages.
        /// </summary>
        /// <remarks>
        /// This is only used on TIS-B fine format messages. It is either zero or one for those
        /// messages. For all other types of message this value is null.
        /// </remarks>
        public byte? TisbIcaoModeAFlag { get; set; }

        /// <summary>
        /// Gets or sets the content of an airborne position message.
        /// </summary>
        /// <remarks>
        /// Note that type code 0 messages are considered by the code to be airborne positions, although in
        /// principle they can also be transmitted by vehicles on the ground.
        /// </remarks>
        public AirbornePositionMessage AirbornePosition { get; set; }

        /// <summary>
        /// Gets or sets the content of a surface position message.
        /// </summary>
        public SurfacePositionMessage SurfacePosition { get; set; }

        /// <summary>
        /// Gets or sets the content of an identifer and category message.
        /// </summary>
        public IdentifierAndCategoryMessage IdentifierAndCategory { get; set; }

        /// <summary>
        /// Gets or sets the content of an airborne velocity message.
        /// </summary>
        public AirborneVelocityMessage AirborneVelocity { get; set; }

        /// <summary>
        /// Gets or sets the content of an aircraft status message.
        /// </summary>
        public AircraftStatusMessage AircraftStatus { get; set; }

        /// <summary>
        /// Gets or sets the content of the target state and status messages (not present in ADS-B version 0).
        /// </summary>
        public TargetStateAndStatusMessage TargetStateAndStatus { get; set; }

        /// <summary>
        /// Gets or sets the content of the aircraft operational status message.
        /// </summary>
        public AircraftOperationalStatusMessage AircraftOperationalStatus { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AdsbMessage()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="modeSMessage"></param>
        public AdsbMessage(ModeSMessage modeSMessage)
        {
            ModeSMessage = modeSMessage;
        }

        /// <summary>
        /// Returns an English description of the message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            if(ModeSMessage != null) {
                result.AppendFormat("DF{0}", (int)ModeSMessage.DownlinkFormat);
                if(ModeSMessage.ApplicationField != null) result.AppendFormat("/AF{0}", (int)ModeSMessage.ApplicationField);
                if(ModeSMessage.ControlField != null) result.AppendFormat("/CF{0}", (int)ModeSMessage.ControlField);
                result.AppendFormat(" {0} ", ModeSMessage.FormattedIcao24);
            }
            result.AppendFormat("TYPE:{0} Format:{1}", Type, MessageFormat);
            if(AirbornePosition != null) result.AppendFormat(" {0}", AirbornePosition);
            if(SurfacePosition != null) result.AppendFormat(" {0}", SurfacePosition);
            if(IdentifierAndCategory != null) result.AppendFormat(" {0}", IdentifierAndCategory);
            if(AirborneVelocity != null) result.AppendFormat(" {0}", AirborneVelocity);
            if(AircraftStatus != null) result.AppendFormat(" {0}", AircraftStatus);
            if(TargetStateAndStatus != null) result.AppendFormat(" {0}", TargetStateAndStatus);
            if(AircraftOperationalStatus != null) result.AppendFormat(" {0}", AircraftOperationalStatus);

            return result.ToString();
        }
    }
}
