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

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// An enumeration of the type of aircraft engine in use on the aircraft.
    /// </summary>
    /// <remarks>The integer values for these are sent to browsers - try not to change the numbers between releases.</remarks>
    public enum EngineType
    {
        /// <summary>
        /// The type of engine is unknown or the aircraft has no engines.
        /// </summary>
        None = 0,

        /// <summary>
        /// Internal combustion engine.
        /// </summary>
        Piston = 1,

        /// <summary>
        /// Turboprop or turboshaft engine.
        /// </summary>
        Turboprop = 2,

        /// <summary>
        /// Jet engine.
        /// </summary>
        Jet = 3,

        /// <summary>
        /// Electric engine.
        /// </summary>
        Electric = 4,

        /// <summary>
        /// Rocket engine.
        /// </summary>
        Rocket = 5,
    }
}
