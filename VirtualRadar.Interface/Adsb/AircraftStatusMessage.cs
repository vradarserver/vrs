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
    /// A class describing the content of the ADS-B aircraft status message.
    /// </summary>
    public class AircraftStatusMessage
    {
        /// <summary>
        /// Gets or sets a value that indicates what type of status message was received.
        /// </summary>
        public AircraftStatusType AircraftStatusType { get; set; }

        /// <summary>
        /// Gets or sets the content of an emergency/priority status message.
        /// </summary>
        public EmergencyStatus EmergencyStatus { get; set; }

        /// <summary>
        /// Gets or sets the content of a TCAS RA broadcast message.
        /// </summary>
        public TcasResolutionAdvisory TcasResolutionAdvisory { get; set; }

        /// <summary>
        /// Returns an English description of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("ASTE");

            result.AppendFormat(" AST:{0}", (int)AircraftStatusType);
            if(EmergencyStatus != null) result.AppendFormat(" {0}", EmergencyStatus);
            if(TcasResolutionAdvisory != null) result.AppendFormat(" {0}", TcasResolutionAdvisory);

            return result.ToString();
        }
    }
}
