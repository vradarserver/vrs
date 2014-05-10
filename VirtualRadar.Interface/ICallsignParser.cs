// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for an object that can parse a callsign.
    /// </summary>
    public interface ICallsignParser
    {
        /// <summary>
        /// Given a callsign such as 'VRS1', where VRS is an airline whose IATA code is VR, this returns all
        /// possible alternate callsigns by introducing leading zeros - e.g. 'VRS1, VRS01, VRS001, VRS0001,
        /// VRS00001, VR1, VR01, ..., VR000001'
        /// </summary>
        /// <param name="callsign"></param>
        /// <returns></returns>
        List<string> GetAllAlternateCallsigns(string callsign);

        /// <summary>
        /// Given a callsign and an optional operator ICAO code this returns all possible callsigns that routes
        /// could be stored under, in the order that they should be searched on. This takes into account the
        /// behaviour of the VRS route compiler.
        /// </summary>
        /// <param name="callsign">The callsign transmitted by the aircraft.</param>
        /// <param name="operatorIcaoCode">The operator ICAO code associated with the aircraft (this usually comes from the database).</param>
        /// <returns>A list of callsigns. The first entry in the list is always the callsign that was passed in.</returns>
        List<string> GetAllRouteCallsigns(string callsign, string operatorIcaoCode);
    }
}
