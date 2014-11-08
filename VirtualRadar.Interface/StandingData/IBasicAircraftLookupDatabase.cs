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
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The interface to the basic aircraft lookup database.
    /// </summary>
    public interface IBasicAircraftLookupDatabase : ISingleton<IBasicAircraftLookupDatabase>, ITransactionable, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the filename of the database.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets or sets the full path to the database.
        /// </summary>
        /// <remarks>
        /// The property is set on initialisation, the setter is there so that you can override it
        /// if necessary.
        /// </remarks>
        string FullPath { get; set; }

        /// <summary>
        /// Gets or sets write support. Once write support is set it cannot be removed.
        /// </summary>
        bool WriteSupportEnabled { get; set; }

        /// <summary>
        /// Gets a lock object that prevents any operations on the database while it is held in a lock().
        /// </summary>
        object Lock { get; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a new version of the file has been downloaded.
        /// </summary>
        event EventHandler ContentUpdated;
        #endregion

        #region CreateDatabaseIfMissing, Compact
        /// <summary>
        /// Creates an empty database. If the database already exists it is left alone.
        /// </summary>
        void CreateDatabaseIfMissing();

        /// <summary>
        /// Removes unused space from the file.
        /// </summary>
        void Compact();

        /// <summary>
        /// Closes any open connections. Only call this within a lock on Lock.
        /// </summary>
        void PrepareForUpdate();

        /// <summary>
        /// Signals any users of the object that it has been updated and that they may want to refresh their content.
        /// </summary>
        void FinishedUpdate();
        #endregion

        #region Aircraft
        /// <summary>
        /// Inserts a new aircraft record.
        /// </summary>
        /// <param name="model"></param>
        void InsertAircraft(BasicAircraft record);

        /// <summary>
        /// Updates an existing aircraft record.
        /// </summary>
        /// <param name="aircraft"></param>
        void UpdateAircraft(Interface.StandingData.BasicAircraft aircraft);

        /// <summary>
        /// Fetches an aircraft record by its ICAO code.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        BasicAircraft GetAircraftByIcao(string icao);

        /// <summary>
        /// Fetches the full graph of aircraft objects for an aircraft's ICAO code.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        BasicAircraftAndChildren GetAircraftAndChildrenByIcao(string icao);

        /// <summary>
        /// Fetches the full graph of aircraft objects for many aircraft ICAOs.
        /// </summary>
        /// <param name="existingIcaos"></param>
        /// <returns></returns>
        Dictionary<string, BasicAircraftAndChildren> GetManyAircraftAndChildrenByCode(string[] existingIcaos);
        #endregion

        #region Model
        /// <summary>
        /// Inserts a new model record.
        /// </summary>
        /// <param name="model"></param>
        void InsertModel(BasicModel model);

        /// <summary>
        /// Updates an existing record
        /// </summary>
        /// <param name="model"></param>
        void UpdateModel(BasicModel model);

        /// <summary>
        /// Delete every model not referenced by an aircraft.
        /// </summary>
        /// <returns></returns>
        void DeleteUnusedModels();

        /// <summary>
        /// Fetches a model by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        BasicModel GetModelById(int? id);

        /// <summary>
        /// Fetches the first model with the name passed across.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<BasicModel> GetModelByName(string name);
        #endregion

        #region Operator
        /// <summary>
        /// Inserts a new operator record.
        /// </summary>
        /// <param name="model"></param>
        void InsertOperator(BasicOperator record);

        /// <summary>
        /// Updates an existing record
        /// </summary>
        /// <param name="record"></param>
        void UpdateOperator(BasicOperator record);

        /// <summary>
        /// Delete every operator not referenced by an aircraft.
        /// </summary>
        /// <returns></returns>
        void DeleteUnusedOperators();

        /// <summary>
        /// Fetches an operator by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        BasicOperator GetOperatorById(int? id);

        /// <summary>
        /// Fetches an operator by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<BasicOperator> GetOperatorByName(string name);
        #endregion
    }
}
