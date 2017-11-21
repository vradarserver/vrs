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
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The interface for objects that manage standing data for the server.
    /// </summary>
    [Singleton]
    public interface IStandingDataManager
    {
        /// <summary>
        /// Gets or sets the object that abstracts away the environment to make testing easier.
        /// </summary>
        IStandingDataManagerProvider Provider { get; set; }

        /// <summary>
        /// Gets the lock object.
        /// </summary>
        object Lock { get; }

        /// <summary>
        /// Returns status information about the current route files.
        /// </summary>
        string RouteStatus { get; }

        /// <summary>
        /// Returns true if the code blocks have been loaded.
        /// </summary>
        bool CodeBlocksLoaded { get; }

        /// <summary>
        /// Raised after <see cref="Load"/> has finished loading the data and <see cref="RouteStatus"/> has been updated.
        /// </summary>
        event EventHandler LoadCompleted;

        /// <summary>
        /// Forces the manager to load the data if it hasn't already been loaded. Does
        /// nothing if the data is already loaded.
        /// </summary>
        void Load();

        /// <summary>
        /// Returns the route that the aircraft using the callsign passed across is most likely following.
        /// </summary>
        /// <param name="callSign"></param>
        /// <returns></returns>
        Route FindRoute(string callSign);

        /// <summary>
        /// Returns the ICAO8643 type information for the aircraft type passed across.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        AircraftType FindAircraftType(string type);

        /// <summary>
        /// Returns the airlines that have the IATA or ICAO code passed across.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        IEnumerable<Airline> FindAirlinesForCode(string code);

        /// <summary>
        /// Returns the code block for the ICAO24 aircraft identifier passed across.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        CodeBlock FindCodeBlock(string icao24);
    }
}
