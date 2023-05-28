using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Test.Framework;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Database.SQLite
{
    public class CommonBaseStationDatabaseTests
    {
        protected virtual string _SchemaPrefix => "";
        protected virtual bool _EngineTruncatesMilliseconds => false;
        protected virtual string _SqlReturnNewIdentity => null;
        protected IBaseStationDatabase _Implementation;
        protected Func<IDbConnection> _CreateConnection;
        protected EnvironmentOptions _EnvironmentOptions;
        protected BaseStationDatabaseOptions _BaseStationDatabaseOptions;
        protected MockSharedConfiguration _SharedConfiguration;
        protected Configuration _Configuration;
        protected MockClock _MockClock;
        protected MockStandingDataManager _StandingData;

        protected void CommonTestInitialise()
        {
            _SharedConfiguration = new();
            _Configuration = _SharedConfiguration.Configuration;
            _EnvironmentOptions = new() {
                WorkingFolder = Path.GetTempPath(),
            };
            _BaseStationDatabaseOptions = new();
            _MockClock = new();
            _StandingData = new();
        }

        protected long AddAircraft(KineticAircraft aircraft)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                var dynamicParameters = new Dapper.DynamicParameters();
                var fieldNames = new StringBuilder();
                var parameters = new StringBuilder();

                foreach(var property in typeof(KineticAircraft).GetProperties()) {
                    var fieldName = property.Name;
                    if(fieldName == nameof(KineticAircraft.AircraftID)) {
                        continue;
                    }

                    if(fieldNames.Length > 0) {
                        fieldNames.Append(',');
                    }
                    if(parameters.Length > 0) {
                        parameters.Append(',');
                    }

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");

                    dynamicParameters.Add(fieldName, property.GetValue(aircraft, null));
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {_SchemaPrefix}[Aircraft] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                aircraft.AircraftID = (int)result;
            }

            return result;
        }

        protected static KineticAircraft CreateAircraft(string icao24 = "123456", string registration = "G-VRST")
        {
            return new KineticAircraft() {
                ModeS = icao24,
                Registration = registration,
            };
        }

        #region GetAircraftByRegistration
        protected void Common_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Implementation.GetAircraftByRegistration(null));
        }

        protected void Common_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Implementation.GetAircraftByRegistration("REG"));
        }
        #endregion
    }
}
