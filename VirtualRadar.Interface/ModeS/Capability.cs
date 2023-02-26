// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaiC:\Source\VirtualRadar\Source\VirtualRadar.Interface\View\mer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// An enumeration of the different communications capabilities of a transponder.
    /// </summary>
    /// <remarks>The CA field of a DF11 reply.</remarks>
    public enum Capability : byte
    {
        /// <summary>
        /// No communications capability.
        /// </summary>
        SurveillanceOnly = 0,

        /// <summary>
        /// Reserved.
        /// </summary>
        CA1 = 1,

        /// <summary>
        /// Reserved.
        /// </summary>
        CA2 = 2,

        /// <summary>
        /// Reserved.
        /// </summary>
        CA3 = 3,

        /// <summary>
        /// Has at least Comm-A and Comm-B capability and ability to set CA 7 and is on the ground.
        /// </summary>
        HasCommACommBAndOnGround = 4,

        /// <summary>
        /// Has at least Comm-A and Comm-B capability and ability to set CA 7 and is airborne.
        /// </summary>
        HasCommACommBAndAirborne = 5,

        /// <summary>
        /// Has at least Comm-A and Comm-B capability and ability to set CA 7, cannot automatically determine whether airborne or on ground.
        /// </summary>
        HasCommACommB = 6,

        /// <summary>
        /// DR is not 0 or FS is 2, 3, 4 or 5, and either airborne or on the ground :)
        /// </summary>
        DownlinkRequestOrAlertOrSpiInForce = 7,
    }
}
