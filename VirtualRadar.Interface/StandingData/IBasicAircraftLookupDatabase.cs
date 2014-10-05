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
    public interface IBasicAircraftLookupDatabase : ITransactionable, IDisposable
    {
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
        /// Creates an empty database. If the database already exists it is left alone.
        /// </summary>
        void CreateDatabaseIfMissing();

        #region Whole-table actions
        /// <summary>
        /// Removes unused space from the file.
        /// </summary>
        void Compact();
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
