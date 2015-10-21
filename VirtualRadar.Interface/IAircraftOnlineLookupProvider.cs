// Copyright © 2015 onwards, Andrew Whewell
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
    /// The interface that online lookup provider can implement to change where <see cref="IAircraftOnlineLookup"/>
    /// goes to fetch aircraft details.
    /// </summary>
    /// <remarks>
    /// Do not do anything in the constructor that will take a long time.
    /// </remarks>
    public interface IAircraftOnlineLookupProvider
    {
        /// <summary>
        /// Gets the largest batch size that the online service can accept.
        /// </summary>
        int MaxBatchSize { get; }

        /// <summary>
        /// Gets the minimum number of seconds between requests. Do not set this to less than one. This restriction
        /// is policed by the code.
        /// </summary>
        int MinSecondsBetweenRequests { get; }

        /// <summary>
        /// Gets the maximum number of seconds to wait between failed requests. After each request the lookup waits for
        /// an extra <see cref="MinSecondsBetweenRequests"/> until this limit is reached.
        /// </summary>
        int MaxSecondsAfterFailedRequest { get; }

        /// <summary>
        /// Periodically called to fetch aircraft details from the online service.
        /// </summary>
        /// <param name="icaos">A collection of ICAOs to fetch.</param>
        /// <param name="fetchedAircraft">Add fetched aircraft records to this list.</param>
        /// <param name="missingIcaos">Add ICAOs for aircraft that definitely do not have details to this list.</param>
        /// <returns>True if the lookup was completed without errors, false if the online source appears to be offline or
        /// having some problems. Missing ICAOs are not an error.</returns>
        /// <remarks><para>
        /// This will be called on a background thread but two calls will never run simultaneously.
        /// </para><para>
        /// If the method allows web exceptions to bubble up then they will not be logged by the caller, it would spam the
        /// log when the user is offline, but any other type of exception will be logged. If the method throws an exception
        /// then it is always assumed to have failed, i.e. returned false. Try not to let exceptions bubble out after a
        /// successful fetch.
        /// </para><para>
        /// If the online source cannot be reached or it returns an error then return false. Do not fill in either of the
        /// lists, that way the ICAOs passed across will be presented later for another try. If you mark an ICAO as missing
        /// when it is not then it could have ramifications elsewhere.
        /// </para></remarks>
        bool DoLookup(string[] icaos, IList<AircraftOnlineLookupDetail> fetchedAircraft, IList<string> missingIcaos);
    }
}
