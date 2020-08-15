// Copyright © 2020 onwards, Andrew Whewell
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
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Records an instance of a configured database and values cached from it.
    /// </summary>
    /// <remarks><para>
    /// To make life a bit easier when the configuration changes we have the concept of a
    /// database instance, which is just a reference to a known database. The state history
    /// manager creates one of these every time the configuration changes, and it takes a
    /// reference to one of these before it makes any database changes. All changes within
    /// a set use the same reference, so if configuration is changed while the instance is
    /// used it shouldn't matter, the database won't change in the middle of a set of
    /// operations.
    /// </para><para>
    /// Bear in mind that two instances can exist concurrently, one reflects the most recent
    /// configuration and the other exists until operations that are using it have finished
    /// and it goes out of scope. The repository on an instance is tied to the configuration
    /// settings on the instance.
    /// </para>
    /// </remarks>
    public interface IStateHistoryDatabaseInstance : IDisposable
    {
        /// <summary>
        /// Gets a value indicating that database writes are enabled.
        /// </summary>
        bool WritesEnabled { get; }

        /// <summary>
        /// Gets the non-standard folder that the instance is using.
        /// </summary>
        string NonStandardFolder { get; }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="writesEnabled"></param>
        /// <param name="nonStandardFolder"></param>
        void Initialise(bool writesEnabled, string nonStandardFolder);

        /// <summary>
        /// Calls the action with the repository if the repository supports reads.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True if the repository supports read actions, false otherwise.</returns>
        bool DoIfReadable(Action<IStateHistoryRepository> action);

        /// <summary>
        /// Calls the action with the repository if the repository supports writes.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True if the repository supports write actions, false otherwise.</returns>
        bool DoIfWriteable(Action<IStateHistoryRepository> action);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="countryName"></param>
        /// <returns></returns>
        CountrySnapshot Country_GetOrCreate(string countryName);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="enginePlacementName"></param>
        /// <returns></returns>
        EnginePlacementSnapshot EnginePlacement_GetOrCreate(int enumValue, string enginePlacementName);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="engineTypeName"></param>
        /// <returns></returns>
        EngineTypeSnapshot EngineType_GetOrCreate(int enumValue, string engineTypeName);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="manufacturerName"></param>
        /// <returns></returns>
        ManufacturerSnapshot Manufacturer_GetOrCreate(string manufacturerName);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="operatorName"></param>
        /// <returns></returns>
        OperatorSnapshot Operator_GetOrCreate(string icao, string operatorName);

        /// <summary>
        /// Returns a snapshot for the values passed across. Returns null if writes are disabled.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="wakeTurbulenceCategoryName"></param>
        /// <returns></returns>
        WakeTurbulenceCategorySnapshot WakeTurbulenceCategory_GetOrCreate(int enumValue, string wakeTurbulenceCategoryName);
    }
}
