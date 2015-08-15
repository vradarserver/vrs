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
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// A class that handles ADO.NET access to the Flights table in a BaseStation database.
    /// </summary>
    /// <remarks>
    /// I had problems with using Linq to read records from this table - it was possible but the queries that were
    /// being constructed were slow. I already had ADO.NET code to read these records and as IQueryable Toolkit can
    /// work with an existing connection to a database I've decided to leave the methods that read records for
    /// reports as the original ADO.NET code. Otherwise access via Linq is preferred.
    /// </remarks>
    sealed class FlightsTable : Table
    {
        #region Fields
        private string _GetByIdCommandText;
        private string _UpdateCommandText;
        private string _DeleteCommandText;

        /// <summary>
        /// The object that can parse callsigns out into alternates for us.
        /// </summary>
        private ICallsignParser _CallsignParser;
        #endregion

        #region Properties
        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Flights"; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightsTable()
        {
            _GetByIdCommandText = String.Format("SELECT {0} FROM [Flights] AS f WHERE [FlightID] = ?", FieldList());
            _UpdateCommandText = "UPDATE [Flights] SET" +
                " [AircraftID] = ?" +
                ",[Callsign] = ?" +
                ",[EndTime] = ?" +
                ",[FirstAltitude] = ?" +
                ",[FirstGroundSpeed] = ?" +
                ",[FirstIsOnGround] = ?" +
                ",[FirstLat] = ?" +
                ",[FirstLon] = ?" +
                ",[FirstSquawk] = ?" +
                ",[FirstTrack] = ?" +
                ",[FirstVerticalRate] = ?" +
                ",[HadAlert] = ?" +
                ",[HadEmergency] = ?" +
                ",[HadSpi] = ?" +
                ",[LastAltitude] = ?" +
                ",[LastGroundSpeed] = ?" +
                ",[LastIsOnGround] = ?" +
                ",[LastLat] = ?" +
                ",[LastLon] = ?" +
                ",[LastSquawk] = ?" +
                ",[LastTrack] = ?" +
                ",[LastVerticalRate] = ?" +
                ",[NumADSBMsgRec] = ?" +
                ",[NumModeSMsgRec] = ?" +
                ",[NumIDMsgRec] = ?" +
                ",[NumSurPosMsgRec] = ?" +
                ",[NumAirPosMsgRec] = ?" +
                ",[NumAirVelMsgRec] = ?" +
                ",[NumSurAltMsgRec] = ?" +
                ",[NumSurIDMsgRec] = ?" +
                ",[NumAirToAirMsgRec] = ?" +
                ",[NumAirCallRepMsgRec] = ?" +
                ",[NumPosMsgRec] = ?" +
                ",[SessionID] = ?" +
                ",[StartTime] = ?" +
                " WHERE [FlightID] = ?";
            _DeleteCommandText = "DELETE FROM [Flights] WHERE [FlightID] = ?";
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
                "  ([FlightID] integer primary key," +
                "   [SessionID] integer not null," +
                "   [AircraftID] integer not null," +
                "   [StartTime] datetime not null," +
                "   [EndTime] datetime," +
                "   [Callsign] varchar(20)," +
                "   [NumPosMsgRec] integer," +
                "   [NumADSBMsgRec] integer," +
                "   [NumModeSMsgRec] integer," +
                "   [NumIDMsgRec] integer," +
                "   [NumSurPosMsgRec] integer," +
                "   [NumAirPosMsgRec] integer," +
                "   [NumAirVelMsgRec] integer," +
                "   [NumSurAltMsgRec] integer," +
                "   [NumSurIDMsgRec] integer," +
                "   [NumAirToAirMsgRec] integer," +
                "   [NumAirCallRepMsgRec] integer," +
                "   [FirstIsOnGround] boolean not null default 0," +
                "   [LastIsOnGround] boolean not null default 0," +
                "   [FirstLat] real," +
                "   [LastLat] real," +
                "   [FirstLon] real," +
                "   [LastLon] real," +
                "   [FirstGroundSpeed] real," +
                "   [LastGroundSpeed] real," +
                "   [FirstAltitude] integer," +
                "   [LastAltitude] integer," +
                "   [FirstVerticalRate] integer," +
                "   [LastVerticalRate] integer," +
                "   [FirstTrack] real," +
                "   [LastTrack] real," +
                "   [FirstSquawk] integer," +
                "   [LastSquawk] integer," +
                "   [HadAlert] boolean not null default 0," +
                "   [HadEmergency] boolean not null default 0," +
                "   [HadSPI] boolean not null default 0," +
                "   [UserNotes] varchar(300)," +
                "   CONSTRAINT [SessionIDfk] FOREIGN KEY ([SessionID]) REFERENCES [Sessions]," +
                "   CONSTRAINT [AircraftIDfk] FOREIGN KEY ([AircraftID]) REFERENCES [Aircraft])",
                TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [FlightsAircraftID] ON [Flights]([AircraftID])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [FlightsCallsign] ON [Flights]([Callsign])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [FlightsEndTime] ON [Flights]([EndTime])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [FlightsSessionID] ON [Flights]([SessionID])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [FlightsStartTime] ON [Flights]([StartTime])");
        }
        #endregion

        #region GetCount, GetCountForAircraft, GetFlights, GetForAircraft, GetById, DecodeFullFlight
        /// <summary>
        /// Normalises strings in the criteria object.
        /// </summary>
        /// <remarks>
        /// Previous versions of VRS would implement case insensitivity in searches by using COLLATE NOCASE
        /// statements in the SQL. This had the unfortunate side-effect of producing very slow searches, so
        /// it has been removed. All searches are consequently case-sensitive but it is assumed that some
        /// fields are always written in upper case. This method converts the criteria for those fields to
        /// upper-case.
        /// </remarks>
        /// <param name="criteria"></param>
        public static void NormaliseCriteria(SearchBaseStationCriteria criteria)
        {
            if(criteria.Callsign != null)       criteria.Callsign.ToUpper();
            if(criteria.Icao != null)           criteria.Icao.ToUpper();
            if(criteria.Registration != null)   criteria.Registration.ToUpper();
            if(criteria.Type != null)           criteria.Type.ToUpper();
        }

        /// <summary>
        /// Returns the count of flight records that match the criteria passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCount(IDbConnection connection, IDbTransaction transaction, TextWriter log, SearchBaseStationCriteria criteria)
        {
            return DoGetCount(connection, transaction, log, null, criteria);
        }

        /// <summary>
        /// Returns a count of flight records for a single aircraft that match the criteria passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCountForAircraft(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            return DoGetCount(connection, transaction, log, aircraft, criteria);
        }

        /// <summary>
        /// Returns an ordered subset of flight records that match the criteria passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sort1"></param>
        /// <param name="sort1Ascending"></param>
        /// <param name="sort2"></param>
        /// <param name="sort2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetFlights(IDbConnection connection, IDbTransaction transaction, TextWriter log, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            return DoGetFlights(connection, transaction, log, null, criteria, fromRow, toRow, sort1, sort1Ascending, sort2, sort2Ascending);
        }

        /// <summary>
        /// Returns an ordered subset of flight records for a single aircraft that match the criteria passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sort1"></param>
        /// <param name="sort1Ascending"></param>
        /// <param name="sort2"></param>
        /// <param name="sort2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetForAircraft(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            return DoGetFlights(connection, transaction, log, aircraft, criteria, fromRow, toRow, sort1, sort1Ascending, sort2, sort2Ascending);
        }

        /// <summary>
        /// Returns the flight corresponding to the ID passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationFlight GetById(IDbConnection connection, IDbTransaction transaction, TextWriter log, int id)
        {
            BaseStationFlight result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetById", _GetByIdCommandText, 1);
            Sql.SetParameters(preparedCommand, id);
            Sql.LogCommand(log, preparedCommand.Command);
            using(IDataReader reader = Sql.Exec.ExecuteReader(preparedCommand.Command)) {
                int ordinal = 0;
                if(Sql.Exec.Read(reader)) result = DecodeFullFlight(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Performs the work for the Get***Count methods.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private int DoGetCount(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            int result = 0;

            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSelectFrom(aircraft, criteria, true));
            var criteriaAndProperties = GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);

            using(IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = commandText.ToString();
                foreach(var property in criteriaAndProperties.PropertyValues) {
                    Sql.AddParameter(command, property);
                }

                Sql.LogCommand(log, command);
                result = (int)(long)Sql.Exec.ExecuteScalar(command);
            }

            return result;
        }

        /// <summary>
        /// Performs the work for the Get***Flights methods.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sort1"></param>
        /// <param name="sort1Ascending"></param>
        /// <param name="sort2"></param>
        /// <param name="sort2Ascending"></param>
        /// <returns></returns>
        private List<BaseStationFlight> DoGetFlights(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            List<BaseStationFlight> result = new List<BaseStationFlight>();

            sort1 = ConvertSortFieldToColumnName(sort1);
            sort2 = ConvertSortFieldToColumnName(sort2);

            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSelectFrom(aircraft, criteria, false));
            var criteriaAndProperties = GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);
            if(sort1 != null || sort2 != null) {
                commandText.Append(" ORDER BY ");
                if(sort1 != null) commandText.AppendFormat("{0} {1}", sort1, sort1Ascending ? "ASC" : "DESC");
                if(sort2 != null) commandText.AppendFormat("{0}{1} {2}", sort1 == null ? "" : ", ", sort2, sort2Ascending ? "ASC" : "DESC");
            }
            commandText.Append(" LIMIT ? OFFSET ?");

            bool decodeFlightsFirst = aircraft != null || !FilterByAircraftFirst(criteria);

            using(IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = commandText.ToString();

                int limit = toRow == -1 || toRow < fromRow ? int.MaxValue : (toRow - Math.Max(0, fromRow)) + 1;
                int offset = fromRow < 0 ? 0 : fromRow;

                foreach(var property in criteriaAndProperties.PropertyValues) {
                    Sql.AddParameter(command, property);
                }
                Sql.AddParameter(command, limit);
                Sql.AddParameter(command, offset);

                Sql.LogCommand(log, command);
                using(IDataReader reader = Sql.Exec.ExecuteReader(command)) {
                    Dictionary<int, BaseStationAircraft> aircraftMap = new Dictionary<int,BaseStationAircraft>();
                    while(Sql.Exec.Read(reader)) {
                        int ordinal = 0;

                        BaseStationFlight flight = DecodeFullFlight(reader, ref ordinal);
                        if(aircraft != null) flight.Aircraft = aircraft;
                        else {
                            if(aircraftMap.ContainsKey(flight.AircraftID)) flight.Aircraft = aircraftMap[flight.AircraftID];
                            else {
                                flight.Aircraft = AircraftTable.DecodeFullAircraft(reader, ref ordinal);
                                aircraftMap.Add(flight.AircraftID, flight.Aircraft);
                            }
                        }
                        result.Add(flight);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a select statement from criteria.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="justCount"></param>
        /// <returns></returns>
        private string CreateSelectFrom(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, bool justCount)
        {
            StringBuilder result = new StringBuilder();

            if(aircraft != null) {
                result.AppendFormat("SELECT {0} FROM [Flights] AS f", justCount ? "COUNT(*)" : FieldList());
            } else {
                result.AppendFormat("SELECT {0}{1}{2} FROM ",
                        justCount ? "COUNT(*)" : FieldList(),
                        justCount ? "" : ", ",
                        justCount ? "" : AircraftTable.FieldList());

                if(FilterByAircraftFirst(criteria)) result.Append("[Aircraft] AS a LEFT JOIN [Flights] AS f");
                else                                         result.Append("[Flights] AS f LEFT JOIN [Aircraft] AS a");

                result.Append(" ON (a.[AircraftID] = f.[AircraftID])");
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns true if the criteria attempts to restrict the search to a single aircraft.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool FilterByAircraftFirst(SearchBaseStationCriteria criteria)
        {
            return (criteria.Icao != null && !String.IsNullOrEmpty(criteria.Icao.Value)) ||
                   (criteria.Registration != null && !String.IsNullOrEmpty(criteria.Registration.Value));
        }

        /// <summary>
        /// Returns the WHERE portion of an SQL statement contains the fields describing the criteria passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private CriteriaAndProperties GetFlightsCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            var result = new CriteriaAndProperties();
            StringBuilder command = new StringBuilder();

            if(aircraft != null) {
                AddCriteria(command, "f.[AircraftID]", " = ?");
                result.PropertyValues.Add(aircraft.AircraftID);
            }

            if(criteria.UseAlternateCallsigns && criteria.Callsign != null && criteria.Callsign.Condition == FilterCondition.Equals && !String.IsNullOrEmpty(criteria.Callsign.Value)) {
                GetAlternateCallsignCriteria(command, result.PropertyValues, criteria.Callsign, "f.[Callsign]");
            } else {
                AddCriteria(command, criteria.Callsign, "f.[Callsign]");
                AddParameter(result.PropertyValues, criteria.Callsign);
            }

            AddCriteria(command, criteria.Date, "f.[StartTime]");
            AddParameter(result.PropertyValues, criteria.Date);

            AddCriteria(command, criteria.Operator, "a.[RegisteredOwners]");
            AddParameter(result.PropertyValues, criteria.Operator);

            AddCriteria(command, criteria.Registration, "a.[Registration]");
            AddParameter(result.PropertyValues, criteria.Registration);

            AddCriteria(command, criteria.Icao, "a.[ModeS]");
            AddParameter(result.PropertyValues, criteria.Icao);

            AddCriteria(command, criteria.Country, "a.[ModeSCountry]");
            AddParameter(result.PropertyValues, criteria.Country);

            AddCriteria(command, criteria.IsEmergency, "f.[HadEmergency]");
            AddParameter(result.PropertyValues, criteria.IsEmergency);

            AddCriteria(command, criteria.Type, "a.[ICAOTypeCode]");
            AddParameter(result.PropertyValues, criteria.Type);

            AddCriteria(command, criteria.FirstAltitude, "f.[FirstAltitude]");
            AddParameter(result.PropertyValues, criteria.FirstAltitude);

            AddCriteria(command, criteria.LastAltitude, "f.[LastAltitude]");
            AddParameter(result.PropertyValues, criteria.LastAltitude);

            result.SqlChunk = command.ToString();
            return result;
        }

        /// <summary>
        /// Builds up the criteria and properties for all alternate callsigns.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <param name="criteria"></param>
        /// <param name="callsignField"></param>
        private void GetAlternateCallsignCriteria(StringBuilder command, List<object> parameters, FilterString criteria, string callsignField)
        {
            if(criteria != null && !String.IsNullOrEmpty(criteria.Value)) {
                if(_CallsignParser == null) _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
                var alternates = _CallsignParser.GetAllAlternateCallsigns(criteria.Value);
                for(var i = 0;i < alternates.Count;++i) {
                    var isFirst = i == 0;
                    var isLast = i + 1 == alternates.Count;
                    var callsign = alternates[i];
                    AddCriteria(command, callsignField,
                        String.Format(" {0} ?", !criteria.ReverseCondition ? "=" : "<>"),
                        useOR: !isFirst && !criteria.ReverseCondition,
                        openParenthesis: isFirst,
                        closeParenthesis: isLast);
                    parameters.Add(callsign);
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across.
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="fieldName"></param>
        /// <param name="condition"></param>
        /// <param name="useOR"></param>
        /// <param name="openParenthesis"></param>
        /// <param name="closeParenthesis"></param>
        private void AddCriteria(StringBuilder criteriaText, string fieldName, string condition, bool useOR = false, bool openParenthesis = false, bool closeParenthesis = false)
        {
            if(criteriaText.Length > 0) criteriaText.Append(useOR ? " OR " : " AND ");
            if(openParenthesis) criteriaText.Append('(');
            criteriaText.Append(fieldName);
            criteriaText.Append(condition);
            if(closeParenthesis) criteriaText.Append(')');
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="fieldName"></param>
        private void AddCriteria(StringBuilder criteriaText, FilterRange<DateTime> filter, string fieldName)
        {
            HandleDateRangeFilter(filter,
                (bothUsed) => { AddCriteria(criteriaText, fieldName, String.Format(" {0} ?", !filter.ReverseCondition ? ">=" : "<"), openParenthesis: bothUsed); },
                (bothUsed) => { AddCriteria(criteriaText, fieldName, String.Format(" {0} ?", !filter.ReverseCondition ? "<=" : ">"), useOR: bothUsed && filter.ReverseCondition, closeParenthesis: bothUsed); }
            );
        }

        /// <summary>
        /// Adds a parameter to the command for the filter value.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filter"></param>
        private void AddParameter(List<object> parameters, FilterRange<DateTime> filter)
        {
            HandleDateRangeFilter(filter,
                (bothUsed) => { parameters.Add(filter.LowerValue); },
                (bothUsed) => { parameters.Add(filter.UpperValue); }
            );
        }

        private void HandleDateRangeFilter(FilterRange<DateTime> filter, Action<bool> onLowerValue, Action<bool> onUpperValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Between:
                        var lowerPresent = filter.LowerValue != null && filter.LowerValue.Value.Year != DateTime.MinValue.Year;
                        var upperPresent = filter.UpperValue != null && filter.UpperValue.Value.Year != DateTime.MaxValue.Year;
                        if(lowerPresent) onLowerValue(lowerPresent && upperPresent);
                        if(upperPresent) onUpperValue(lowerPresent && upperPresent);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="fieldName"></param>
        private void AddCriteria(StringBuilder criteriaText, FilterRange<int> filter, string fieldName)
        {
            HandleIntRangeFilter(filter,
                (bothUsed) => { AddCriteria(criteriaText, fieldName, String.Format(" {0} ?", !filter.ReverseCondition ? ">=" : "<"), openParenthesis: bothUsed); },
                (bothUsed) => { AddCriteria(criteriaText, fieldName, String.Format(" {0} ?", !filter.ReverseCondition ? "<=" : ">"), useOR: bothUsed && filter.ReverseCondition, closeParenthesis: bothUsed); }
            );
        }

        /// <summary>
        /// Adds a parameter to the command for the filter value.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filter"></param>
        private void AddParameter(List<object> parameters, FilterRange<int> filter)
        {
            HandleIntRangeFilter(filter,
                (bothUsed) => { parameters.Add(filter.LowerValue); },
                (bothUsed) => { parameters.Add(filter.UpperValue); }
            );
        }

        private void HandleIntRangeFilter(FilterRange<int> filter, Action<bool> onLowerValue, Action<bool> onUpperValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Between:
                        var lowerPresent = filter.LowerValue != null && filter.LowerValue != int.MinValue;
                        var upperPresent = filter.UpperValue != null && filter.UpperValue != int.MaxValue;
                        if(lowerPresent) onLowerValue(lowerPresent && upperPresent);
                        if(upperPresent) onUpperValue(lowerPresent && upperPresent);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="fieldName"></param>
        private void AddCriteria(StringBuilder criteriaText, FilterString filter, string fieldName)
        {
            HandleStringFilter(filter, (condition, matchNull) => {
                if(matchNull) {
                    if(condition == FilterCondition.Equals) {
                        AddCriteria(criteriaText, fieldName, String.Format(" IS{0} NULL", filter.ReverseCondition ? " NOT" : ""), openParenthesis: true);
                        AddCriteria(criteriaText, fieldName, String.Format(" {0} ''", filter.ReverseCondition ? "<>" : "="), useOR: !filter.ReverseCondition, closeParenthesis: true);
                    }
                } else {
                    var conditionText = "";
                    switch(condition) {
                        case FilterCondition.Contains:
                        case FilterCondition.EndsWith:
                        case FilterCondition.StartsWith:    conditionText = String.Format(" {0}LIKE ? ESCAPE '/'", filter.ReverseCondition ? "NOT " : ""); break;
                        case FilterCondition.Equals:        conditionText = String.Format(" {0} ?", filter.ReverseCondition ? "<>" : "="); break;
                    }
                    AddCriteria(criteriaText, fieldName, conditionText);
                }
            });
        }

        /// <summary>
        /// Adds a parameter to the command for the filter value.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filter"></param>
        private void AddParameter(List<object> parameters, FilterString filter)
        {
            HandleStringFilter(filter, (condition, matchNull) => {
                if(!matchNull) {
                    var value = filter.Value;
                    var escapedValue = (value ?? "").Replace("%", "/%").Replace("_", "%_");
                    switch(condition) {
                        case FilterCondition.Contains:      value = String.Format("%{0}%", escapedValue); break;
                        case FilterCondition.EndsWith:      value = String.Format("%{0}", escapedValue); break;
                        case FilterCondition.StartsWith:    value = String.Format("{0}%", escapedValue); break;
                    }
                    parameters.Add(value);
                }
            });
        }

        private void HandleStringFilter(FilterString filter, Action<FilterCondition, bool> onValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Contains:
                    case FilterCondition.EndsWith:
                    case FilterCondition.Equals:
                    case FilterCondition.StartsWith:    onValue(filter.Condition, String.IsNullOrEmpty(filter.Value)); break;
                    default:                            throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across.
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="fieldName"></param>
        private void AddCriteria(StringBuilder criteriaText, FilterBool filter, string fieldName)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Equals:    AddCriteria(criteriaText, fieldName, String.Format(" {0} 0", filter.Value && !filter.ReverseCondition ? "<>" : "=")); break;
                    default:                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a parameter to the command for the filter value.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filter"></param>
        private void AddParameter(List<object> parameters, FilterBool filter)
        {
            ; // The FilterBool version of AddCriteria doesn't add any parameters to the SQL. This is just a stub.
        }

        /// <summary>
        /// Returns the list of base fields that all SQL statements will fetch from the table.
        /// </summary>
        /// <returns></returns>
        public static string FieldList()
        {
            return "f.[AircraftID], " +
                   "f.[SessionID], " +
                   "f.[Callsign], " +
                   "f.[EndTime], " +
                   "f.[FirstAltitude], " +
                   "f.[FirstGroundSpeed], " +
                   "f.[FirstIsOnGround], " +
                   "f.[FirstLat], " +
                   "f.[FirstLon], " +
                   "f.[FirstSquawk], " +
                   "f.[FirstTrack], " +
                   "f.[FirstVerticalRate], " +
                   "f.[FlightID], " +
                   "f.[HadAlert], " +
                   "f.[HadEmergency], " +
                   "f.[HadSpi], " +
                   "f.[LastAltitude], " +
                   "f.[LastGroundSpeed], " +
                   "f.[LastIsOnGround], " +
                   "f.[LastLat], " +
                   "f.[LastLon], " +
                   "f.[LastSquawk], " +
                   "f.[LastTrack], " +
                   "f.[LastVerticalRate], " +
                   "f.[NumADSBMsgRec], " +
                   "f.[NumModeSMsgRec], " +
                   "f.[NumIDMsgRec], " +
                   "f.[NumSurPosMsgRec], " +
                   "f.[NumAirPosMsgRec], " +
                   "f.[NumAirVelMsgRec], " +
                   "f.[NumSurAltMsgRec], " +
                   "f.[NumSurIDMsgRec], " +
                   "f.[NumAirToAirMsgRec], " +
                   "f.[NumAirCallRepMsgRec], " +
                   "f.[NumPosMsgRec], " +
                   "f.[StartTime]";
        }

        /// <summary>
        /// Translates from the sort field to an SQL field name.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private string ConvertSortFieldToColumnName(string sortField)
        {
            string result = null;
            if(sortField != null) {
                switch(sortField.ToUpperInvariant()) {
                    case "CALLSIGN":        result = "f.[Callsign]"; break;
                    case "COUNTRY":         result = "a.[ModeSCountry]"; break;
                    case "DATE":            result = "f.[StartTime]"; break;
                    case "MODEL":           result = "a.[Type]"; break;
                    case "TYPE":            result = "a.[ICAOTypeCode]"; break;
                    case "OPERATOR":        result = "a.[RegisteredOwners]"; break;
                    case "REG":             result = "a.[Registration]"; break;
                    case "ICAO":            result = "a.[ModeS]"; break;
                    case "FIRSTALTITUDE":   result = "f.[FirstAltitude]"; break;
                    case "LASTALTITUDE":    result = "f.[LastAltitude]"; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an object describing a single field and copies content of the IDataReader into it.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static BaseStationFlight DecodeFullFlight(IDataReader reader, ref int ordinal)
        {
            BaseStationFlight result = new BaseStationFlight();

            result.AircraftID = Sql.GetInt32(reader, ordinal++);
            result.SessionID = Sql.GetInt32(reader, ordinal++);
            result.Callsign = Sql.GetString(reader, ordinal++);
            result.EndTime = Sql.GetDateTime(reader, ordinal++);
            result.FirstAltitude = Sql.GetInt32(reader, ordinal++);
            result.FirstGroundSpeed = Sql.GetFloat(reader, ordinal++);
            result.FirstIsOnGround = Sql.GetBool(reader, ordinal++);
            result.FirstLat = Sql.GetFloat(reader, ordinal++);
            result.FirstLon = Sql.GetFloat(reader, ordinal++);
            result.FirstSquawk = Sql.GetInt32(reader, ordinal++);
            result.FirstTrack = Sql.GetFloat(reader, ordinal++);
            result.FirstVerticalRate = Sql.GetInt32(reader, ordinal++);
            result.FlightID = Sql.GetInt32(reader, ordinal++);
            result.HadAlert = Sql.GetBool(reader, ordinal++);
            result.HadEmergency = Sql.GetBool(reader, ordinal++);
            result.HadSpi = Sql.GetBool(reader, ordinal++);
            result.LastAltitude = Sql.GetInt32(reader, ordinal++);
            result.LastGroundSpeed = Sql.GetFloat(reader, ordinal++);
            result.LastIsOnGround = Sql.GetBool(reader, ordinal++);
            result.LastLat = Sql.GetFloat(reader, ordinal++);
            result.LastLon = Sql.GetFloat(reader, ordinal++);
            result.LastSquawk = Sql.GetInt32(reader, ordinal++);
            result.LastTrack = Sql.GetFloat(reader, ordinal++);
            result.LastVerticalRate = Sql.GetInt32(reader, ordinal++);
            result.NumADSBMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumModeSMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumIDMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumSurPosMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumAirPosMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumAirVelMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumSurAltMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumSurIDMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumAirToAirMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumAirCallRepMsgRec = Sql.GetInt32(reader, ordinal++);
            result.NumPosMsgRec = Sql.GetInt32(reader, ordinal++);
            result.StartTime = Sql.GetDateTime(reader, ordinal++);

            return result;
        }
        #endregion

        #region Insert, Update, Delete
        /// <summary>
        /// Inserts a new record and returns its ID.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="flight"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationFlight flight)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "FlightID",
                "AircraftID",
                "Callsign",
                "EndTime",
                "FirstAltitude",
                "FirstGroundSpeed",
                "FirstIsOnGround",
                "FirstLat",
                "FirstLon",
                "FirstSquawk",
                "FirstTrack",
                "FirstVerticalRate",
                "HadAlert",
                "HadEmergency",
                "HadSpi",
                "LastAltitude",
                "LastGroundSpeed",
                "LastIsOnGround",
                "LastLat",
                "LastLon",
                "LastSquawk",
                "LastTrack",
                "LastVerticalRate",
                "NumADSBMsgRec",
                "NumModeSMsgRec",
                "NumIDMsgRec",
                "NumSurPosMsgRec",
                "NumAirPosMsgRec",
                "NumAirVelMsgRec",
                "NumSurAltMsgRec",
                "NumSurIDMsgRec",
                "NumAirToAirMsgRec",
                "NumAirCallRepMsgRec",
                "NumPosMsgRec",
                "SessionID",
                "StartTime");
            return (int)Sql.ExecuteInsert(preparedCommand, log,
                flight.AircraftID,
                flight.Callsign,
                flight.EndTime,
                flight.FirstAltitude,
                flight.FirstGroundSpeed,
                flight.FirstIsOnGround,
                flight.FirstLat,
                flight.FirstLon,
                flight.FirstSquawk,
                flight.FirstTrack,
                flight.FirstVerticalRate,
                flight.HadAlert,
                flight.HadEmergency,
                flight.HadSpi,
                flight.LastAltitude,
                flight.LastGroundSpeed,
                flight.LastIsOnGround,
                flight.LastLat,
                flight.LastLon,
                flight.LastSquawk,
                flight.LastTrack,
                flight.LastVerticalRate,
                flight.NumADSBMsgRec,
                flight.NumModeSMsgRec,
                flight.NumIDMsgRec,
                flight.NumSurPosMsgRec,
                flight.NumAirPosMsgRec,
                flight.NumAirVelMsgRec,
                flight.NumSurAltMsgRec,
                flight.NumSurIDMsgRec,
                flight.NumAirToAirMsgRec,
                flight.NumAirCallRepMsgRec,
                flight.NumPosMsgRec,
                flight.SessionID,
                flight.StartTime);
        }

        /// <summary>
        /// Writes the flight record back to the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="flight"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationFlight flight)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 36);
            Sql.SetParameters(preparedCommand,
                flight.AircraftID,
                flight.Callsign,
                flight.EndTime,
                flight.FirstAltitude,
                flight.FirstGroundSpeed,
                flight.FirstIsOnGround,
                flight.FirstLat,
                flight.FirstLon,
                flight.FirstSquawk,
                flight.FirstTrack,
                flight.FirstVerticalRate,
                flight.HadAlert,
                flight.HadEmergency,
                flight.HadSpi,
                flight.LastAltitude,
                flight.LastGroundSpeed,
                flight.LastIsOnGround,
                flight.LastLat,
                flight.LastLon,
                flight.LastSquawk,
                flight.LastTrack,
                flight.LastVerticalRate,
                flight.NumADSBMsgRec,
                flight.NumModeSMsgRec,
                flight.NumIDMsgRec,
                flight.NumSurPosMsgRec,
                flight.NumAirPosMsgRec,
                flight.NumAirVelMsgRec,
                flight.NumSurAltMsgRec,
                flight.NumSurIDMsgRec,
                flight.NumAirToAirMsgRec,
                flight.NumAirCallRepMsgRec,
                flight.NumPosMsgRec,
                flight.SessionID,
                flight.StartTime,
                flight.FlightID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }

        /// <summary>
        /// Deletes the record passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="flight"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationFlight flight)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Delete", _DeleteCommandText, 1);
            Sql.SetParameters(preparedCommand, flight.FlightID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }
        #endregion
    }
}
