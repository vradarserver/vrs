// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// Describes an aircraft type as read from ICAO8643
    /// </summary>
    public class AircraftType
    {
        /// <summary>
        /// Gets or sets the aircraft type code.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets a list of every manufacturer that has an aircraft that uses this type code.
        /// </summary>
        public IList<string> Manufacturers { get; } = new List<string>();

        /// <summary>
        /// Gets a list of every model that is covered by this type code. There will be one entry in this
        /// list for every entry in <see cref="Manufacturers"/>.
        /// </summary>
        public IList<string> Models { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the heaviest wake turbulence category that applies to the aircraft.
        /// </summary>
        public WakeTurbulenceCategory WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets or sets the type of aircraft this is (fixed-wing, seaplane, helicopter etc.).
        /// </summary>
        public Species Species { get; set; }

        /// <summary>
        /// Gets or sets the number of engines - 1, 2, 3 etc. or C for two engines coupled to drive a single propeller.
        /// This will be null if the number of engines is not known or not applicable.
        /// </summary>
        public string Engines { get; set; }

        /// <summary>
        /// Gets or sets the type of engine predominantly used to propel the aircraft.
        /// </summary>
        public EngineType EngineType { get; set; }

        /// <summary>
        /// Gets or sets an indication of how the engines are mounted on the aircraft.
        /// </summary>
        public EnginePlacement EnginePlacement { get; set; }

        /// <summary>
        /// See Object.ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Type ?? "";
    }
}
