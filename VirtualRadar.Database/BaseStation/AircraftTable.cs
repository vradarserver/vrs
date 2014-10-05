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
using VirtualRadar.Interface.Database;
using System.Data;
using System.Linq;
using InterfaceFactory;
using System.IO;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// A class that deals with ADO.NET interaction for the Aircraft table.
    /// </summary>
    sealed class AircraftTable : Table
    {
        #region Fields
        private string _GetAllCommandText;
        private string _GetByRegistrationCommandText;
        private string _GetByIcaoCommandText;
        private string _GetManyByIcaoCommandText;
        private string _GetManyAircraftAndFlightsByIcaoCommandText;
        private string _GetByIdCommandText;
        private string _UpdateCommandText;
        private string _DeleteCommandText;
        private string _UpdateModeSCountryText;
        #endregion

        #region Properties
        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Aircraft"; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftTable()
        {
            _GetAllCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a", FieldList());
            _GetByRegistrationCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a WHERE a.[Registration] = ?", FieldList());
            _GetByIcaoCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a WHERE a.[ModeS] = ?", FieldList());
            _GetManyByIcaoCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a ", FieldList());
            _GetManyAircraftAndFlightsByIcaoCommandText = String.Format(
                                                      "SELECT {0}, " +
                                                      "       (SELECT COUNT(*) FROM [Flights] AS f WHERE f.[AircraftID] = a.[AircraftID]) AS FlightsCount " +
                                                      "  FROM [Aircraft] AS a ", FieldList());
            _GetByIdCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a WHERE a.[AircraftID] = ?", FieldList());
            _UpdateCommandText = "UPDATE [Aircraft] SET" +
                    "  [AircraftClass] = ?" +
                    ", [CofACategory] = ?" +
                    ", [CofAExpiry] = ?" +
                    ", [Country] = ?" +
                    ", [CurrentRegDate] = ?" +
                    ", [DeRegDate] = ?" +
                    ", [Engines] = ?" +
                    ", [FirstCreated] = ?" +
                    ", [FirstRegDate] = ?" +
                    ", [GenericName] = ?" +
                    ", [ICAOTypeCode] = ?" +
                    ", [InfoUrl] = ?" +
                    ", [Interested] = ?" +
                    ", [LastModified] = ?" +
                    ", [Manufacturer] = ?" +
                    ", [ModeS] = ?" +
                    ", [ModeSCountry] = ?" +
                    ", [MTOW] = ?" +
                    ", [OperatorFlagCode] = ?" +
                    ", [OwnershipStatus] = ?" +
                    ", [PictureUrl1] = ?" +
                    ", [PictureUrl2] = ?" +
                    ", [PictureUrl3] = ?" +
                    ", [PopularName] = ?" +
                    ", [PreviousID] = ?" +
                    ", [Registration] = ?" +
                    ", [RegisteredOwners] = ?" +
                    ", [SerialNo] = ?" +
                    ", [Status] = ?" +
                    ", [TotalHours] = ?" +
                    ", [Type] = ?" +
                    ", [UserNotes] = ?" +
                    ", [UserTag] = ?" +
                    ", [YearBuilt] = ?" +
                    " WHERE [AircraftID] = ?";
            _UpdateModeSCountryText = "UPDATE [Aircraft] SET [ModeSCountry] = ? WHERE [AircraftID] = ?";
            _DeleteCommandText = "DELETE FROM [Aircraft] WHERE [AircraftID] = ?";
        }
        #endregion

        #region CreateTable
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([AircraftID] integer primary key," +
                "   [FirstCreated] datetime not null," +
                "   [LastModified] datetime not null," +
                "   [ModeS] varchar(6) not null unique," +
                "   [ModeSCountry] varchar(24)," +
                "   [Country] varchar(24)," +
                "   [Registration] varchar(20)," +
                "   [CurrentRegDate] varchar(10)," +
                "   [PreviousID] varchar(10)," +
                "   [FirstRegDate] varchar(10)," +
                "   [Status] varchar(10)," +
                "   [DeRegDate] varchar(10)," +
                "   [Manufacturer] varchar(60)," +
                "   [ICAOTypeCode] varchar(10)," +
                "   [Type] varchar(40)," +
                "   [SerialNo] varchar(30)," +
                "   [PopularName] varchar(20)," +
                "   [GenericName] varchar(20)," +
                "   [AircraftClass] varchar(20)," +
                "   [Engines] varchar(40)," +
                "   [OwnershipStatus] varchar(10)," +
                "   [RegisteredOwners] varchar(100)," +
                "   [MTOW] varchar(10)," +
                "   [TotalHours] varchar(20)," +
                "   [YearBuilt] varchar(4)," +
                "   [CofACategory] varchar(30)," +
                "   [CofAExpiry] varchar(10)," +
                "   [UserNotes] varchar(300)," +
                "   [Interested] boolean not null default 0," +
                "   [UserTag] varchar(5)," +
                "   [InfoURL] varchar(150)," +
                "   [PictureURL1] varchar(150)," +
                "   [PictureURL2] varchar(150)," +
                "   [PictureURL3] varchar(150)," +
                "   [UserBool1] boolean not null default 0," +
                "   [UserBool2] boolean not null default 0," +
                "   [UserBool3] boolean not null default 0," +
                "   [UserBool4] boolean not null default 0," +
                "   [UserBool5] boolean not null default 0," +
                "   [UserString1] varchar(20)," +
                "   [UserString2] varchar(20)," +
                "   [UserString3] varchar(20)," +
                "   [UserString4] varchar(20)," +
                "   [UserString5] varchar(20)," +
                "   [UserInt1] integer default 0," +
                "   [UserInt2] integer default 0," +
                "   [UserInt3] integer default 0," +
                "   [UserInt4] integer default 0," +
                "   [UserInt5] integer default 0," +
                "   [OperatorFlagCode] varchar(20))",
                TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftAircraftClass] ON [Aircraft]([AircraftClass])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftCountry] ON [Aircraft]([Country])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftGenericName] ON [Aircraft]([GenericName])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftICAOTypeCode] ON [Aircraft]([ICAOTypeCode])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftInterested] ON [Aircraft]([Interested])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftManufacturer] ON [Aircraft]([Manufacturer])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftModeS] ON [Aircraft]([ModeS])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftModeSCountry] ON [Aircraft]([ModeSCountry])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftPopularName] ON [Aircraft]([PopularName])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftRegisteredOwners] ON [Aircraft]([RegisteredOwners])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftRegistration] ON [Aircraft]([Registration])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftSerialNo] ON [Aircraft]([SerialNo])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftType] ON [Aircraft]([Type])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftUserTag] ON [Aircraft]([UserTag])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [AircraftYearBuilt] ON [Aircraft]([YearBuilt])");
        }

        /// <summary>
        /// Creates triggers on the table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public void CreateTriggers(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, "CREATE TRIGGER IF NOT EXISTS [AircraftIDdeltrig] BEFORE DELETE ON [Aircraft] FOR EACH ROW BEGIN DELETE FROM [Flights] WHERE [AircraftID] = OLD.[AircraftID]; END;");
        }
        #endregion

        #region GetByRegistration, GetByIcao, GetManyByIcao, GetById
        /// <summary>
        /// Fetches an aircraft record by its registration code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public BaseStationAircraft GetByRegistration(IDbConnection connection, IDbTransaction transaction, TextWriter log, string registration)
        {
            BaseStationAircraft result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetByRegistration", _GetByRegistrationCommandText, 1);
            Sql.SetParameters(preparedCommand, registration);
            Sql.LogCommand(log, preparedCommand.Command);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullAircraft(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Fetches an aircraft record by its ICAO code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="icao"></param>
        /// <returns></returns>
        public BaseStationAircraft GetByIcao(IDbConnection connection, IDbTransaction transaction, TextWriter log, string icao)
        {
            BaseStationAircraft result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetByIcao", _GetByIcaoCommandText, 1);
            Sql.SetParameters(preparedCommand, icao);
            Sql.LogCommand(log, preparedCommand.Command);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullAircraft(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Fetches many aircraft records by their ICAO code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public List<BaseStationAircraft> GetManyByIcao(IDbConnection connection, IDbTransaction transaction, TextWriter log, IEnumerable<string> icao24s)
        {
            var result = new List<BaseStationAircraft>();

            var parameters = icao24s.ToArray();
            var parameterString = new StringBuilder();
            for(var i = 0;i < parameters.Length;++i) {
                if(i != 0) parameterString.Append(',');
                parameterString.Append('?');
            }
            using(var command = connection.CreateCommand()) {
                var whereClause = String.Format(" WHERE a.ModeS IN ({0})", parameterString);
                var commandText = String.Format("{0} {1}", _GetManyByIcaoCommandText, whereClause);
                command.Connection = connection;
                command.Transaction = transaction;
                command.CommandText = commandText;

                Sql.AddParameters(command, parameters);
                Sql.LogCommand(log, command);
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        int ordinal = 0;
                        var aircraft = DecodeFullAircraft(reader, ref ordinal);
                        result.Add(aircraft);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches many aircraft records and counts of flights by their ICAO code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public List<BaseStationAircraftAndFlightsCount> GetManyAircraftAndFlightsByIcao(IDbConnection connection, IDbTransaction transaction, TextWriter log, IEnumerable<string> icao24s)
        {
            var result = new List<BaseStationAircraftAndFlightsCount>();

            var parameters = icao24s.ToArray();
            var parameterString = new StringBuilder();
            for(var i = 0;i < parameters.Length;++i) {
                if(i != 0) parameterString.Append(',');
                parameterString.Append('?');
            }
            using(var command = connection.CreateCommand()) {
                var whereClause = String.Format(" WHERE a.ModeS IN ({0})", parameterString);
                var commandText = String.Format("{0} {1}", _GetManyAircraftAndFlightsByIcaoCommandText, whereClause);
                command.Connection = connection;
                command.Transaction = transaction;
                command.CommandText = commandText;

                Sql.AddParameters(command, parameters);
                Sql.LogCommand(log, command);
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        int ordinal = 0;
                        var aircraft = (BaseStationAircraftAndFlightsCount)DecodeFullAircraft(reader, ref ordinal, createBaseStationAircraftAndFlightsCount: true);
                        result.Add(aircraft);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches an aircraft record by its record ID.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationAircraft GetById(IDbConnection connection, IDbTransaction transaction, TextWriter log, int id)
        {
            BaseStationAircraft result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetById", _GetByIdCommandText, 1);
            Sql.SetParameters(preparedCommand, id);
            Sql.LogCommand(log, preparedCommand.Command);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullAircraft(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Fetches every aircraft in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraftList"></param>
        public void GetAll(IDbConnection connection, IDbTransaction transaction, TextWriter log, List<BaseStationAircraft> aircraftList)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "GetAll", _GetAllCommandText, 0);
            Sql.LogCommand(log, preparedCommand.Command);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                while(reader.Read()) {
                    int ordinal = 0;
                    var aircraft = DecodeFullAircraft(reader, ref ordinal);
                    aircraftList.Add(aircraft);
                }
            }
        }
        #endregion

        #region Insert, Update, Delete
        /// <summary>
        /// Inserts a record into the database and returns its ID.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "AircraftID",
                    "AircraftClass",
                    "CofACategory",
                    "CofAExpiry",
                    "Country",
                    "CurrentRegDate",
                    "DeRegDate",
                    "Engines",
                    "FirstCreated",
                    "FirstRegDate",
                    "GenericName",
                    "ICAOTypeCode",
                    "InfoUrl",
                    "Interested",
                    "LastModified",
                    "Manufacturer",
                    "ModeS",
                    "ModeSCountry",
                    "MTOW",
                    "OperatorFlagCode",
                    "OwnershipStatus",
                    "PictureUrl1",
                    "PictureUrl2",
                    "PictureUrl3",
                    "PopularName",
                    "PreviousID",
                    "Registration",
                    "RegisteredOwners",
                    "SerialNo",
                    "Status",
                    "TotalHours",
                    "Type",
                    "UserNotes",
                    "UserTag",
                    "YearBuilt");

            return (int)Sql.ExecuteInsert(preparedCommand, log,
                    aircraft.AircraftClass,
                    aircraft.CofACategory,
                    aircraft.CofAExpiry,
                    aircraft.Country,
                    aircraft.CurrentRegDate,
                    aircraft.DeRegDate,
                    aircraft.Engines,
                    aircraft.FirstCreated,
                    aircraft.FirstRegDate,
                    aircraft.GenericName,
                    aircraft.ICAOTypeCode,
                    aircraft.InfoUrl,
                    aircraft.Interested,
                    aircraft.LastModified,
                    aircraft.Manufacturer,
                    aircraft.ModeS,
                    aircraft.ModeSCountry,
                    aircraft.MTOW,
                    aircraft.OperatorFlagCode,
                    aircraft.OwnershipStatus,
                    aircraft.PictureUrl1,
                    aircraft.PictureUrl2,
                    aircraft.PictureUrl3,
                    aircraft.PopularName,
                    aircraft.PreviousID,
                    aircraft.Registration,
                    aircraft.RegisteredOwners,
                    aircraft.SerialNo,
                    aircraft.Status,
                    aircraft.TotalHours,
                    aircraft.Type,
                    aircraft.UserNotes,
                    aircraft.UserTag,
                    aircraft.YearBuilt);
        }

        /// <summary>
        /// Updates the aircraft passed across with new values.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 35);
            Sql.SetParameters(preparedCommand, 
                    aircraft.AircraftClass,
                    aircraft.CofACategory,
                    aircraft.CofAExpiry,
                    aircraft.Country,
                    aircraft.CurrentRegDate,
                    aircraft.DeRegDate,
                    aircraft.Engines,
                    aircraft.FirstCreated,
                    aircraft.FirstRegDate,
                    aircraft.GenericName,
                    aircraft.ICAOTypeCode,
                    aircraft.InfoUrl,
                    aircraft.Interested,
                    aircraft.LastModified,
                    aircraft.Manufacturer,
                    aircraft.ModeS,
                    aircraft.ModeSCountry,
                    aircraft.MTOW,
                    aircraft.OperatorFlagCode,
                    aircraft.OwnershipStatus,
                    aircraft.PictureUrl1,
                    aircraft.PictureUrl2,
                    aircraft.PictureUrl3,
                    aircraft.PopularName,
                    aircraft.PreviousID,
                    aircraft.Registration,
                    aircraft.RegisteredOwners,
                    aircraft.SerialNo,
                    aircraft.Status,
                    aircraft.TotalHours,
                    aircraft.Type,
                    aircraft.UserNotes,
                    aircraft.UserTag,
                    aircraft.YearBuilt,
                    aircraft.AircraftID);
            Sql.LogCommand(log, preparedCommand.Command);
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the Mode-S Country field for an aircraft.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraftId"></param>
        /// <param name="modeSCountry"></param>
        public void UpdateModeSCountry(IDbConnection connection, IDbTransaction transaction, TextWriter log, int aircraftId, string modeSCountry)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "UpdateModeSCountry", _UpdateModeSCountryText, 2);
            Sql.SetParameters(preparedCommand,
                    modeSCountry,
                    aircraftId);
            Sql.LogCommand(log, preparedCommand.Command);
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes the aircraft passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Delete", _DeleteCommandText, 1);
            Sql.SetParameters(preparedCommand, aircraft.AircraftID);
            Sql.LogCommand(log, preparedCommand.Command);
            preparedCommand.Command.ExecuteNonQuery();
        }
        #endregion

        #region Helpers for the Flights table join handling - FieldList, DecodeFullAircraft
        /// <summary>
        /// Returns the base list of fields that the GetBy methods read from the Aircraft table.
        /// </summary>
        /// <returns></returns>
        public static string FieldList()
        {
            return "a.[AircraftClass], " +
                   "a.[AircraftID], " +
                   "a.[Country], " +
                   "a.[DeRegDate], " +
                   "a.[Engines], " +
                   "a.[FirstCreated], " +
                   "a.[GenericName], " +
                   "a.[ICAOTypeCode], " +
                   "a.[LastModified], " +
                   "a.[Manufacturer], " +
                   "a.[ModeS], " +
                   "a.[ModeSCountry], " +
                   "a.[OperatorFlagCode], " +
                   "a.[OwnershipStatus], " +
                   "a.[PopularName], " +
                   "a.[PreviousID], " +
                   "a.[RegisteredOwners], " +
                   "a.[Registration], " +
                   "a.[SerialNo], " +
                   "a.[Status], " +
                   "a.[Type], " +
                   "a.[CofACategory], " +
                   "a.[CofAExpiry], " +
                   "a.[CurrentRegDate], " +
                   "a.[FirstRegDate], " +
                   "a.[InfoUrl], " +
                   "a.[Interested], " +
                   "a.[MTOW], " +
                   "a.[PictureUrl1], " +
                   "a.[PictureUrl2], " +
                   "a.[PictureUrl3], " +
                   "a.[TotalHours], " +
                   "a.[UserNotes], " +
                   "a.[UserTag], " +
                   "a.[YearBuilt]";
        }

        /// <summary>
        /// Copies the content of an IDataReader containing an aircraft's information into an object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <param name="createBaseStationAircraftAndFlightsCount"></param>
        /// <returns></returns>
        public static BaseStationAircraft DecodeFullAircraft(IDataReader reader, ref int ordinal, bool createBaseStationAircraftAndFlightsCount = false)
        {
            BaseStationAircraft result = createBaseStationAircraftAndFlightsCount ? new BaseStationAircraftAndFlightsCount() : new BaseStationAircraft();
            result.AircraftClass = Sql.GetString(reader, ordinal++);
            result.AircraftID = Sql.GetInt32(reader, ordinal++);
            result.Country = Sql.GetString(reader, ordinal++);
            result.DeRegDate = Sql.GetString(reader, ordinal++);
            result.Engines = Sql.GetString(reader, ordinal++);
            result.FirstCreated = Sql.GetDateTime(reader, ordinal++);
            result.GenericName = Sql.GetString(reader, ordinal++);
            result.ICAOTypeCode = Sql.GetString(reader, ordinal++);
            result.LastModified = Sql.GetDateTime(reader, ordinal++);
            result.Manufacturer = Sql.GetString(reader, ordinal++);
            result.ModeS = Sql.GetString(reader, ordinal++);
            result.ModeSCountry = Sql.GetString(reader, ordinal++);
            result.OperatorFlagCode = Sql.GetString(reader, ordinal++);
            result.OwnershipStatus = Sql.GetString(reader, ordinal++);
            result.PopularName = Sql.GetString(reader, ordinal++);
            result.PreviousID = Sql.GetString(reader, ordinal++);
            result.RegisteredOwners = Sql.GetString(reader, ordinal++);
            result.Registration = Sql.GetString(reader, ordinal++);
            result.SerialNo = Sql.GetString(reader, ordinal++);
            result.Status = Sql.GetString(reader, ordinal++);
            result.Type = Sql.GetString(reader, ordinal++);
            result.CofACategory = Sql.GetString(reader, ordinal++);
            result.CofAExpiry = Sql.GetString(reader, ordinal++);
            result.CurrentRegDate = Sql.GetString(reader, ordinal++);
            result.FirstRegDate = Sql.GetString(reader, ordinal++);
            result.InfoUrl = Sql.GetString(reader, ordinal++);
            result.Interested = Sql.GetBool(reader, ordinal++);
            result.MTOW = Sql.GetString(reader, ordinal++);
            result.PictureUrl1 = Sql.GetString(reader, ordinal++);
            result.PictureUrl2 = Sql.GetString(reader, ordinal++);
            result.PictureUrl3 = Sql.GetString(reader, ordinal++);
            result.TotalHours = Sql.GetString(reader, ordinal++);
            result.UserNotes = Sql.GetString(reader, ordinal++);
            result.UserTag = Sql.GetString(reader, ordinal++);
            result.YearBuilt = Sql.GetString(reader, ordinal++);

            if(createBaseStationAircraftAndFlightsCount) ((BaseStationAircraftAndFlightsCount)result).FlightsCount = Sql.GetInt32(reader, ordinal++);

            return result;
        }
        #endregion
    }
}
