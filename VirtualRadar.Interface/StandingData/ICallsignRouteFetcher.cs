// Copyright © 2014 onwards, Andrew Whewell
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
using InterfaceFactory;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The interface for a singleton object that manages the fetching of routes for callsigns
    /// across many aircraft lists (or potentially anything that needs to find callsigns from
    /// routes).
    /// </summary>
    [Singleton]
    public interface ICallsignRouteFetcher : ISingleton<ICallsignRouteFetcher>
    {
        /// <summary>
        /// Raised when callsign details have been fetched or changed, or if a fetch attempt was
        /// made and no details could be found.
        /// </summary>
        /// <remarks>
        /// This is raised on a background thread. If you allow exceptions to bubble up from your
        /// event handler then they will be logged but they could also interfere with other
        /// heartbeat event handlers - do not let exceptions bubble up.
        /// </remarks>
        event EventHandler<EventArgs<CallsignRouteDetail>> Fetched;

        /// <summary>
        /// Registers an aircraft's callsign with the fetcher. If the details are already known
        /// then they are immediately returned, otherwise they will be fetched on a background
        /// thread.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns>Null if the callsign has not been fetched, the callsign details if they have
        /// been fetched.</returns>
        /// <remarks>
        /// There is no unregister call, any given callsign is automatically deregistered after the
        /// fetcher hasn't seen a RegisterAircraft for it in a while.
        /// </remarks>
        CallsignRouteDetail RegisterAircraft(IAircraft aircraft);
    }
}
