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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A collection of strings describing the UniqueId values for the receiver formats
    /// that ship with VRS.
    /// </summary>
    public static class DataSource
    {
        /// <summary>
        /// Any source of port 30003 data such as BaseStation, PlanePlotter etc.
        /// </summary>
        public static readonly string Port30003 = "Port30003";

        /// <summary>
        /// Raw Mode-S messages from the Kinetics Avionics SBS-3.
        /// </summary>
        public static readonly string Sbs3 = "Sbs3";

        /// <summary>
        /// Raw messages from the Mode-S Beast.
        /// </summary>
        public static readonly string Beast = "Beast";

        /// <summary>
        /// Compressed messages in VRS format.
        /// </summary>
        public static readonly string CompressedVRS = "CompressedVRS";

        /// <summary>
        /// The feed is sending changes to an aircraft list in JSON format.
        /// </summary>
        public static readonly string AircraftListJson = "AircraftListJson";

        /// <summary>
        /// The receiver is sending its feed in PlaneFinder format.
        /// </summary>
        public static readonly string PlaneFinder = "PlaneFinder";

        /// <summary>
        /// The receiver is sending its feed in AirnavXRange format.
        /// </summary>
        public static readonly string AirnavXRange = "AirnavXRange";

        static string[] _AllInternalDataSources = new string[] {
            DataSource.Port30003,
            DataSource.Sbs3,
            DataSource.Beast,
            DataSource.CompressedVRS,
            DataSource.AircraftListJson,
            DataSource.PlaneFinder,
            DataSource.AirnavXRange,
        };
        /// <summary>
        /// Gets an array of all internal data sources. This is not used by the server, it's just to
        /// make life easier for the unit tests.
        /// </summary>
        public static string[] AllInternalDataSources
        {
            get { return _AllInternalDataSources; }
        }
    }
}
