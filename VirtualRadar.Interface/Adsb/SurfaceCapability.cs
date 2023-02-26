// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// An enumeration of the surface capability codes transmitted in version 1 and 2
    /// ADS-B aircraft operational status messages.
    /// </summary>
    [Flags]
    public enum SurfaceCapability : short
    {
        /// <summary>
        /// The NIC Supplement C bit (version 2 only).
        /// </summary>
        NicSupplementC                      = 0x0001,   // message bit 20

        /// <summary>
        /// The LSB of the Navigational Accuracy Category for Velocity value (version 2 only).
        /// </summary>
        NacVelocity0                        = 0x0002,   // message bit 19

        /// <summary>
        /// The middle bit of the Navigational Accuracy Category for Velocity value (version 2 only).
        /// </summary>
        NacVelocity1                        = 0x0004,   // message bit 18

        /// <summary>
        /// The MSB of the Navigational Accuracy Category for Velocity value (version 2 only).
        /// </summary>
        NacVelocity2                        = 0x0008,   // message bit 17

        /// <summary>
        /// The aircraft has UAT receive capability (version 2 only).
        /// </summary>
        HasUniversalAccessTransceiver       = 0x0010,   // message bit 16

        /// <summary>
        /// The surface vehicle transmit power is below 70 watts.
        /// </summary>
        ClassB2TransmitterIsLowPower        = 0x0020,   // message bit 15

        /// <summary>
        /// The aircraft has CDTI.
        /// </summary>
        HasCockpitDisplayTrafficInformation = 0x0100,   // message bit 12

        /// <summary>
        /// The position transmitted in surface messages is that of the reference point of the aircraft rather than the antenna.
        /// </summary>
        PositionOffsetApplied               = 0x0200,   // message bit 11
    }
}
