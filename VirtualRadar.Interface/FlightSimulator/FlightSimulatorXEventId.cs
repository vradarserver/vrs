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

namespace VirtualRadar.Interface.FlightSimulator
{
    /// <summary>
    /// An enumeration of the values used to identify events within FSX that we are interested in. Many of
    /// these events are sent to FSX although some identify events which have taken place at the user's behest.
    /// </summary>
    public enum FlightSimulatorXEventId
    {
        /// <summary>
        /// (Sent) Turn slew mode on.
        /// </summary>
        SlewModeOn,

        /// <summary>
        /// (Sent) Turn slew mode off.
        /// </summary>
        SlewModeOff,

        /// <summary>
        /// (Sent) Increase altitude slowly.
        /// </summary>
        SlewAltitudeUpSlow,

        /// <summary>
        /// (Sent) Stop moving the aircraft.
        /// </summary>
        SlewFreeze,

        /// <summary>
        /// (Received) The user has toggled slew mode.
        /// </summary>
        SlewToggle,

        /// <summary>
        /// (Sent) Stop moving the aircraft when not in slew.
        /// </summary>
        FreezeLatitudeLongitude,

        /// <summary>
        /// (Sent) Stop the aircraft from changing altitude when not in slew.
        /// </summary>
        FreezeAltitude,

        /// <summary>
        /// (Sent) Stop the aircraft from rolling or changing pitch.
        /// </summary>
        FreezeAttitude,

        /// <summary>
        /// (Received) The aircraft has crashed.
        /// </summary>
        Crashed,

        /// <summary>
        /// (Received) The user has finished the mission they were flying.
        /// </summary>
        MissionCompleted,
    }
}
