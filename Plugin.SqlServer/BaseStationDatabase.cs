// Copyright © 2017 onwards, Andrew Whewell
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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using InterfaceFactory;
using VirtualRadar.Database;
using VirtualRadar.Database.BaseStation;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Plugin.SqlServer.Models;

namespace VirtualRadar.Plugin.SqlServer
{
    /// <summary>
    /// The SQL Server implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    class BaseStationDatabase : IBaseStationDatabaseSqlServer
    {
        /// <summary>
        /// The helper object for <see cref="ITransactionable"/> implementations.
        /// </summary>
        private TransactionHelper _TransactionHelper = new TransactionHelper();

        /// <summary>
        /// The standing data manager that does code block lookups for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Engine => "SQL Server";

        private string _ConnectionString;
        /// <summary>
        /// Gets or sets the connection string to use.
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString ?? Plugin.Singleton.Options.ConnectionString; }
            set { _ConnectionString = value; }
        }

        private int? _CommandTimeoutSeconds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int CommandTimeoutSeconds
        {
            get { return _CommandTimeoutSeconds ?? Plugin.Singleton.Options.CommandTimeoutSeconds; }
            set { _CommandTimeoutSeconds = value; }
        }

        /// <summary>
        /// True if the connection string is not the standard one from options.
        /// </summary>
        public bool IsCustomConnectionString
        {
            get { return _ConnectionString != null; }
        }

        private bool? _CanUpdateSchema;
        /// <summary>
        /// Gets or sets a value indicating whether we are allowed to update the schema.
        /// </summary>
        public bool CanUpdateSchema
        {
            get { return _CanUpdateSchema ?? false; }
            set { _CanUpdateSchema = value; }
        }

        /// <summary>
        /// True if the connection string appears to be a good one.
        /// </summary>
        public static bool ConnectionStringIsGood { get; set; }

        /// <summary>
        /// The connection and transaction to use while a transaction is in force on the current thread.
        /// </summary>
        /// <remarks>
        /// Note that multiple instances of the class on the same thread all share the same connection.
        /// If one instance calls into another instance during a transaction then things will get messy!
        /// </remarks>
        [ThreadStatic]
        private static ConnectionWrapper _TransactionConnection;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabaseProvider Provider { get; set; } = new BaseStationDatabaseProvider();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName
        {
            get => SqlServerStrings.FakeDatabaseFileName;
            set {;}
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LogFileName
        {
            get => SqlServerStrings.FakeDatabaseFileName;
            set {;}
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsConnected
        {
            get {
                var result = false;
                PerformInConnection(connection => result = true);
                return result;
            }
        }

        /// <summary>
        /// See interface docs. Meaningless in SQL Server, there are no read-only connections. It's controlled
        /// by grants at the database level.
        /// </summary>
        public bool WriteSupportEnabled { get; set; }

        /// <summary>
        /// See interface docs. Actual max parameters is 2100, we are lowballing a bit. Nowadays code should be
        /// using Dapper rather than constructing parameter lists.
        /// </summary>
        public int MaxParameters => 2000;

        #pragma warning disable 0067
        /// <summary>
        /// Unused.
        /// </summary>
        public event EventHandler FileNameChanging;
        #pragma warning restore

        #pragma warning disable 0067
        /// <summary>
        /// Unused.
        /// </summary>
        public event EventHandler FileNameChanged;
        #pragma warning restore

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<BaseStationAircraft>> AircraftUpdated;

        /// <summary>
        /// Raises <see cref="AircraftUpdated"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnAircraftUpdated(EventArgs<BaseStationAircraft> args)
        {
            EventHelper.Raise(AircraftUpdated, this, () => args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationDatabase()
        {
            _StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            ;
        }

        /// <summary>
        /// Returns a wrapper around the connection and transaction to use.
        /// </summary>
        /// <returns></returns>
        private ConnectionWrapper CreateOpenConnection()
        {
            return _TransactionConnection ?? new ConnectionWrapper(CreateOpenDatabaseConnection(), null);
        }

        /// <summary>
        /// Performs some action but only if the connection configuration appears to be good.
        /// </summary>
        /// <param name="action"></param>
        private void PerformInConnection(Action<ConnectionWrapper> action)
        {
            using(var wrapper = CreateOpenConnection()) {
                if(wrapper.HasConnection) {
                    action(wrapper);
                }
            }
        }

        /// <summary>
        /// Returns an open connection or null if the configuration is bad.
        /// </summary>
        /// <returns></returns>
        private IDbConnection CreateOpenDatabaseConnection()
        {
            var result = IsCustomConnectionString || ConnectionStringIsGood ? new SqlConnection(ConnectionString) : null;
            result?.Open();

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool AttemptAutoFix(Exception ex) => false;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateDatabaseIfMissing(string fileName)
        {
            UpdateSchema();
        }

        /// <summary>
        /// Updates the schema and returns the output of the update script.
        /// </summary>
        /// <returns></returns>
        public string[] UpdateSchema()
        {
            string[] result = null;

            if(CanUpdateSchema) {
                PerformInConnection(wrapper => {
                    result = SqlServerHelper.RunScript(wrapper.Connection, Scripts.Resources.UpdateSchema_sql, CommandTimeoutSeconds);
                });
            }

            return result ?? new string[0];
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            return IsConnected;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool FileExists()
        {
            return true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool FileIsEmpty()
        {
            return false;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void DeleteAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete aircraft when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.Execute("[BaseStation].[Aircraft_Delete]", new {
                    @AircraftID = aircraft?.AircraftID,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void DeleteFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete flights when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.Execute("[BaseStation].[Flights_Delete]", new {
                    @FlightID = flight?.FlightID,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void DeleteLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete locations when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.Execute("[BaseStation].[Locations_Delete]", new {
                    @LocationID = location?.LocationID,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void DeleteSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete sessions when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.Execute("[BaseStation].[Sessions_Delete]", new {
                    @SessionID = session?.SessionID,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void DeleteSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete system events when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.Execute("[BaseStation].[SystemEvents_Delete]", new {
                    @SystemEventsID = systemEvent?.SystemEventsID,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftByCode(string icao24)
        {
            BaseStationAircraft result = null;

            PerformInConnection(wrapper => {
                result = wrapper.Connection.Query<BaseStationAircraft>("[BaseStation].[Aircraft_GetByModeS]", new {
                    @ModeS = icao24,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                .FirstOrDefault();
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftById(int id)
        {
            BaseStationAircraft result = null;

            PerformInConnection(wrapper => {
                result = wrapper.Connection.Query<BaseStationAircraft>("[BaseStation].[Aircraft_GetByID]", new {
                    @AircraftID = id,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                .FirstOrDefault();
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftByRegistration(string registration)
        {
            BaseStationAircraft result = null;

            PerformInConnection(wrapper => {
                result = wrapper.Connection.Query<BaseStationAircraft>("[BaseStation].[Aircraft_GetByRegistration]", new {
                    @Registration = registration,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                .FirstOrDefault();
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<BaseStationAircraft> GetAllAircraft()
        {
            var result = new List<BaseStationAircraft>();

            PerformInConnection(wrapper => {
                result.AddRange(wrapper.Connection.Query<BaseStationAircraft>("[BaseStation].[Aircraft_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                ));
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCountOfFlights(SearchBaseStationCriteria criteria)
        {
            if(criteria == null) {
                throw new ArgumentNullException(nameof(criteria));
            }
            NormaliseCriteria(criteria);

            var result = 0;
            PerformInConnection(wrapper => {
                result = Flights_GetCountByCriteria(wrapper, null, criteria);
            });

            return result;
        }

        private void NormaliseCriteria(SearchBaseStationCriteria criteria)
        {
            if(criteria?.Callsign != null)      criteria.Callsign.ToUpper();
            if(criteria?.Icao != null)          criteria.Icao.ToUpper();
            if(criteria?.Registration != null)  criteria.Registration.ToUpper();
            if(criteria?.Type != null)          criteria.Type.ToUpper();
        }

        private int Flights_GetCountByCriteria(ConnectionWrapper wrapper, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            var commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: true));
            var criteriaAndProperties = DynamicSql.GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) {
                commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);
            }

            return wrapper.Connection.ExecuteScalar<int>(
                commandText.ToString(),
                criteriaAndProperties.Parameters,
                transaction: wrapper.Transaction,
                commandTimeout: CommandTimeoutSeconds
            );
        }

        /// <summary>
        /// Builds a select statement from criteria.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="justCount"></param>
        /// <returns></returns>
        private string CreateSearchBaseStationCriteriaSql(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, bool justCount)
        {
            var result = new StringBuilder();

            if(aircraft != null) {
                result.AppendFormat("SELECT {0} FROM [BaseStation].[Flights]", justCount ? "COUNT(*)" : "[Flights].*");
            } else {
                result.AppendFormat("SELECT {0}{1}{2} FROM ",
                        justCount ? "COUNT(*)" : "[Flights].*",
                        justCount ? "" : ", ",
                        justCount ? "" : "[Aircraft].*");

                if(criteria.FilterByAircraftFirst()) result.Append("[BaseStation].[Aircraft] LEFT JOIN [BaseStation].[Flights]");
                else                                 result.Append("[BaseStation].[Flights] LEFT JOIN [BaseStation].[Aircraft]");

                result.Append(" ON [Aircraft].[AircraftID] = [Flights].[AircraftID]");
            }

            return result.ToString();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCountOfFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            if(criteria == null) {
                throw new ArgumentNullException(nameof(criteria));
            }
            NormaliseCriteria(criteria);

            var result = 0;
            if(aircraft != null) {
                PerformInConnection(wrapper => {
                    result = Flights_GetCountByCriteria(wrapper, aircraft, criteria);
                });
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationDBHistory> GetDatabaseHistory()
        {
            var result = new List<BaseStationDBHistory>();

            PerformInConnection(wrapper => {
                result.AddRange(wrapper.Connection.Query<BaseStationDBHistory>("[BaseStation].[DBHistory_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                ));
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public BaseStationDBInfo GetDatabaseVersion()
        {
            BaseStationDBInfo result = null;

            PerformInConnection(wrapper => {
                result = wrapper.Connection.Query<BaseStationDBInfo>("[BaseStation].[DBInfo_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                )
                .OrderByDescending(r => r.CurrentVersion)
                .FirstOrDefault();
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationFlight GetFlightById(int id)
        {
            BaseStationFlight result = null;

            PerformInConnection(wrapper => {
                result = wrapper.Connection.Query<BaseStationFlight>("[BaseStation].[Flights_GetByID]", new {
                    @FlightID = id,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                .FirstOrDefault();
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sortField1"></param>
        /// <param name="sortField1Ascending"></param>
        /// <param name="sortField2"></param>
        /// <param name="sortField2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetFlights(SearchBaseStationCriteria criteria, int fromRow, int toRow, string sortField1, bool sortField1Ascending, string sortField2, bool sortField2Ascending)
        {
            if(criteria == null) {
                throw new ArgumentNullException(nameof(criteria));
            }
            NormaliseCriteria(criteria);

            var result = new List<BaseStationFlight>();

            PerformInConnection(wrapper => {
                result.AddRange(Flights_GetByCriteria(
                    wrapper, null, criteria, fromRow, toRow,
                    sortField1, sortField1Ascending, sortField2, sortField2Ascending
                ));
            });

            return result;
        }

        private List<BaseStationFlight> Flights_GetByCriteria(ConnectionWrapper wrapper, BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            var result = new List<BaseStationFlight>();

            sort1 = DynamicSql.CriteriaSortFieldToColumnName(sort1);
            sort2 = DynamicSql.CriteriaSortFieldToColumnName(sort2);

            if(sort1 == null && sort2 == null) {
                sort1 = "[Flights].[FlightID]";
                sort1Ascending = false;
            }

            var commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: false));
            var criteriaAndProperties = DynamicSql.GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) {
                commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);
            }
            commandText.Append(" ORDER BY ");
            if(sort1 != null) {
                commandText.AppendFormat("{0} {1}", sort1, sort1Ascending ? "ASC" : "DESC");
            }
            if(sort2 != null) {
                commandText.AppendFormat("{0}{1} {2}", sort1 == null ? "" : ", ", sort2, sort2Ascending ? "ASC" : "DESC");
            }

            commandText.Append(" OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY");
            var limit = toRow == -1 || toRow < fromRow ? int.MaxValue : (toRow - Math.Max(0, fromRow)) + 1;
            var offset = fromRow < 0 ? 0 : fromRow;
            criteriaAndProperties.Parameters.Add("limit", limit);
            criteriaAndProperties.Parameters.Add("offset", offset);

            if(aircraft != null) {
                result.AddRange(wrapper.Connection.Query<BaseStationFlight>(
                    commandText.ToString(),
                    criteriaAndProperties.Parameters,
                    transaction: wrapper.Transaction,
                    commandTimeout: CommandTimeoutSeconds
                ));
                foreach(var flight in result) {
                    flight.Aircraft = aircraft;
                }
            } else {
                var aircraftInstances = new Dictionary<int, BaseStationAircraft>();
                Func<BaseStationAircraft, BaseStationAircraft> getAircraftInstance = (a) => {
                    BaseStationAircraft instance = null;
                    if(a != null) {
                        if(!aircraftInstances.TryGetValue(a.AircraftID, out instance)) {
                            instance = a;
                            aircraftInstances.Add(a.AircraftID, instance);
                        }
                    }
                    return instance;
                };

                // The results are always declared as flights then aircraft
                result.AddRange(wrapper.Connection.Query<BaseStationFlight, BaseStationAircraft, BaseStationFlight>(
                    commandText.ToString(),
                    (f, a) => {
                        f.Aircraft = getAircraftInstance(a);
                        return f;
                    },
                    criteriaAndProperties.Parameters,
                    transaction: wrapper.Transaction,
                    commandTimeout: CommandTimeoutSeconds,
                    splitOn: "AircraftID"
                ));
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sort1"></param>
        /// <param name="sort1Ascending"></param>
        /// <param name="sort2"></param>
        /// <param name="sort2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            if(criteria == null) {
                throw new ArgumentNullException(nameof(criteria));
            }
            NormaliseCriteria(criteria);

            var result = new List<BaseStationFlight>();

            if(aircraft != null) {
                PerformInConnection(wrapper => {
                    result.AddRange(Flights_GetByCriteria(wrapper, aircraft, criteria, fromRow, toRow, sort1, sort1Ascending, sort2, sort2Ascending));
                });
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationLocation> GetLocations()
        {
            var result = new List<BaseStationLocation>();

            PerformInConnection(wrapper => {
                result.AddRange(wrapper.Connection.Query<BaseStationLocation>("[BaseStation].[Locations_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                ));
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public Dictionary<string, BaseStationAircraftAndFlightsCount> GetManyAircraftAndFlightsCountByCode(IEnumerable<string> icao24s)
        {
            Dictionary<string, BaseStationAircraftAndFlightsCount> result = null;

            PerformInConnection(wrapper => {
                using(var dataTable = BuildIcao24Udtt(icao24s)) {
                    result = wrapper.Connection.Query<BaseStationAircraftAndFlightsCount>("[BaseStation].[AircraftAndFlightsCount_GetByCodes]", new {
                        Codes = dataTable,
                    }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                    .ToDictionary(r => r.ModeS, r => r);
                }
            });

            return result ?? new Dictionary<string, BaseStationAircraftAndFlightsCount>();
        }

        private DataTable BuildIcao24Udtt(IEnumerable<string> icao24s)
        {
            return SqlServerHelper.UdttParameter<Icao24Udtt>(
                Icao24Udtt.UdttProperties,
                icao24s.Select(r => (r ?? "").ToUpper().Trim()).Where(r => r != "").Distinct().Select(r => new Icao24Udtt(r))
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public Dictionary<string, BaseStationAircraft> GetManyAircraftByCode(IEnumerable<string> icao24s)
        {
            Dictionary<string, BaseStationAircraft> result = null;

            PerformInConnection(wrapper => {
                using(var dataTable = BuildIcao24Udtt(icao24s)) {
                    result = wrapper.Connection.Query<BaseStationAircraft>("[BaseStation].[Aircraft_GetByCodes]", new {
                        Codes = dataTable,
                    }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds)
                    .ToDictionary(r => r.ModeS, r => r);
                }
            });

            return result ?? new Dictionary<string, BaseStationAircraft>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <param name="created"></param>
        /// <returns></returns>
        public BaseStationAircraft GetOrInsertAircraftByCode(string icao24, out bool created)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");
            }

            BaseStationAircraft result = null;

            var parameters = new DynamicParameters();
            parameters.Add("@ModeS", icao24);
            parameters.Add("@Created", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@LocalNow", DateTime.Now);
            parameters.Add("@ModeSCountry", _StandingDataManager.FindCodeBlock(icao24)?.ModeSCountry);

            PerformInConnection(wrapper => {
                result = wrapper.Connection.QuerySingleOrDefault<BaseStationAircraft>(
                    "[BaseStation].[Aircraft_GetOrCreate]",
                    parameters,
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
            created = parameters.Get<bool>("@Created");

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationSession> GetSessions()
        {
            var result = new List<BaseStationSession>();

            PerformInConnection(wrapper => {
                result.AddRange(wrapper.Connection.Query<BaseStationSession>("[BaseStation].[Sessions_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                ));
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationSystemEvents> GetSystemEvents()
        {
            var result = new List<BaseStationSystemEvents>();

            PerformInConnection(wrapper => {
                result.AddRange(wrapper.Connection.Query<BaseStationSystemEvents>("[BaseStation].[SystemEvents_GetAll]",
                    transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                ));
            });

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void InsertAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");
            }

            PerformInConnection(wrapper => {
                aircraft.AircraftID = wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Aircraft_Insert]",
                    ParameterBuilder.FromAircraft(aircraft, includeAircraftID: false),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void InsertFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert flights when write support is disabled");
            }

            PerformInConnection(wrapper => {
                flight.FlightID = wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Flights_Insert]",
                    ParameterBuilder.FromFlight(flight, includeFlightID: false),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void InsertLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert locations when write support is disabled");
            }

            PerformInConnection(wrapper => {
                location.LocationID = wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Locations_Insert]",
                    ParameterBuilder.FromLocation(location, includeLocationID: false),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs,.
        /// </summary>
        /// <param name="session"></param>
        public void InsertSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert sessions when write support is disabled");
            }

            PerformInConnection(wrapper => {
                session.SessionID = wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Sessions_Insert]",
                    ParameterBuilder.FromSession(session, includeSessionID: false),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void InsertSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert system events when write support is disabled");
            }

            PerformInConnection(wrapper => {
                systemEvent.SystemEventsID = wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[SystemEvents_Insert]",
                    ParameterBuilder.FromSystemEvent(systemEvent, includeSystemEventID: false),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        public void RecordManyMissingAircraft(IEnumerable<string> icaos)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot mark missing aircraft when write support is disabled");
            }

            PerformInConnection(wrapper => {
                using(var dataTable = BuildIcao24Udtt(icaos)) {
                    wrapper.Connection.ExecuteScalar<int>("[BaseStation].[Aircraft_MarkManyMissing]", new {
                        @Codes = dataTable,
                        @LocalNow = DateTime.Now,
                    }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
                }
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        public void RecordMissingAircraft(string icao)
        {
            RecordManyMissingAircraft(new string[] { icao });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void UpdateAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update aircraft when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Aircraft_Update]",
                    ParameterBuilder.FromAircraft(aircraft),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );

                OnAircraftUpdated(new EventArgs<BaseStationAircraft>(aircraft));
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="modeSCountry"></param>
        public void UpdateAircraftModeSCountry(int aircraftId, string modeSCountry)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update aircraft when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>("[BaseStation].[Aircraft_UpdateModeSCountry]", new {
                    @AircraftID =   aircraftId,
                    @ModeSCountry = modeSCountry,
                    @LastModified = DateTime.Now,
                }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void UpdateFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update flights when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Flights_Update]",
                    ParameterBuilder.FromFlight(flight),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void UpdateLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update locations when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Locations_Update]",
                    ParameterBuilder.FromLocation(location),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void UpdateSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update sessions when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[Sessions_Update]",
                    ParameterBuilder.FromSession(session),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void UpdateSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update system events when write support is disabled");
            }

            PerformInConnection(wrapper => {
                wrapper.Connection.ExecuteScalar<int>(
                    "[BaseStation].[SystemEvents_Update]",
                    ParameterBuilder.FromSystemEvent(systemEvent),
                    transaction: wrapper.Transaction,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: CommandTimeoutSeconds
                );
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="upsertLookup"></param>
        /// <returns></returns>
        public BaseStationAircraft UpsertAircraft(BaseStationAircraftUpsertLookup upsertLookup)
        {
            return UpsertManyAircraft(new BaseStationAircraftUpsertLookup[] { upsertLookup }).FirstOrDefault();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="upsertLookups"></param>
        /// <returns></returns>
        public BaseStationAircraft[] UpsertManyAircraft(IEnumerable<BaseStationAircraftUpsertLookup> upsertLookups)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            BaseStationAircraft[] result = null;
            AircraftActionResult[] actions = null;
            using(var dataTable = GenerateBaseStationAircraftUpsertLookupDataTable(upsertLookups)) {
                PerformInConnection(wrapper => {
                    var resultSets = wrapper.Connection.QueryMultiple(
                        "[BaseStation].[Aircraft_UpsertLookups]", new {
                            @Lookups = dataTable
                        }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                    );

                    result = resultSets.Read<BaseStationAircraft>(buffered: false).ToArray();
                    actions = resultSets.Read<AircraftActionResult>(buffered: false).ToArray();
                });
            }

            if(actions != null) {
                foreach(var action in actions.Where(r => r.IsUpdated)) {
                    var aircraft = result.FirstOrDefault(r => r.AircraftID == action.AircraftID);
                    if(aircraft != null) {
                        OnAircraftUpdated(new EventArgs<BaseStationAircraft>(aircraft));
                    }
                }
            }

            return result ?? new BaseStationAircraft[0];
        }

        private DataTable GenerateBaseStationAircraftUpsertLookupDataTable(IEnumerable<BaseStationAircraftUpsertLookup> upsertLookups)
        {
            var udttModels = upsertLookups.Select(r => new BaseStationAircraftUpsertLookupUdtt(r));
            return SqlServerHelper.UdttParameter(BaseStationAircraftUpsertLookupUdtt.UdttProperties, udttModels);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="upsertAircraft"></param>
        /// <returns></returns>
        public BaseStationAircraft[] UpsertManyAircraft(IEnumerable<BaseStationAircraftUpsert> upsertAircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            BaseStationAircraft[] result = null;
            AircraftActionResult[] actions = null;
            using(var dataTable = GenerateBaseStationAircraftUpsertDataTable(upsertAircraft)) {
                PerformInConnection(wrapper => {
                    var resultSets = wrapper.Connection.QueryMultiple(
                        "[BaseStation].[Aircraft_Upsert]", new {
                            @BulkAircraft = dataTable
                        }, transaction: wrapper.Transaction, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeoutSeconds
                    );

                    result = resultSets.Read<BaseStationAircraft>(buffered: false).ToArray();
                    actions = resultSets.Read<AircraftActionResult>(buffered: false).ToArray();
                });
            }

            if(actions != null) {
                foreach(var action in actions.Where(r => r.IsUpdated)) {
                    var aircraft = result.FirstOrDefault(r => r.AircraftID == action.AircraftID);
                    if(aircraft != null) {
                        OnAircraftUpdated(new EventArgs<BaseStationAircraft>(aircraft));
                    }
                }
            }

            return result ?? new BaseStationAircraft[0];
        }

        private DataTable GenerateBaseStationAircraftUpsertDataTable(IEnumerable<BaseStationAircraftUpsert> upsertAircraft)
        {
            var udttModels = upsertAircraft.Select(r => new BaseStationAircraftUpsertUdtt(r));
            return SqlServerHelper.UdttParameter(BaseStationAircraftUpsertUdtt.UdttProperties, udttModels);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="upsertFlights"></param>
        /// <returns></returns>
        public BaseStationFlight[] UpsertManyFlights(IEnumerable<BaseStationFlightUpsert> flights)
        {
            BaseStationFlight[] result = null;

            PerformInConnection(wrapper => {
                using(var dataTable = GenerateBaseStationFlightDataTable(flights)) {
                    var resultSets = wrapper.Connection.QueryMultiple(
                        "[BaseStation].[Flights_Upsert]", new {
                            @BulkFlights = dataTable,
                        },
                        transaction: wrapper.Transaction,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: CommandTimeoutSeconds
                    );

                    result = resultSets.Read<BaseStationFlight>(buffered: false).ToArray();
                    resultSets.Read<FlightActionResult>(buffered: false);
                }
            });

            return result ?? new BaseStationFlight[0];
        }

        private DataTable GenerateBaseStationFlightDataTable(IEnumerable<BaseStationFlightUpsert> upsertFlights)
        {
            var udttModels = upsertFlights.Select(r => new BaseStationFlightUpsertUdtt(r));
            return SqlServerHelper.UdttParameter(BaseStationFlightUpsertUdtt.UdttProperties, udttModels);
        }


        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(Func<bool> action)
        {
            var result = false;

            PerformInConnection(wrapper => {
                result = _TransactionHelper.PerformInTransaction(
                    wrapper.Connection,
                    wrapper.Transaction != null,
                    false,
                    transaction => {
                        _TransactionConnection = transaction == null ? null : new ConnectionWrapper(wrapper.Connection, transaction);
                    },
                    action
                );
            });

            return result;
        }
    }
}
