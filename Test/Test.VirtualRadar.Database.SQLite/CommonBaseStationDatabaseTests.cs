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

        protected KineticAircraft LoadAircraftFromSpreadsheetRow(SpreadsheetTestDataRow row, int firstOrdinal = 0, KineticAircraft copyIntoAircraft = null)
        {
            var result = copyIntoAircraft ?? new();

            int ordinal = firstOrdinal;
            result.AircraftClass =      row.EString(ordinal++);
            result.Country =            row.EString(ordinal++);
            result.DeRegDate =          row.EString(ordinal++);
            result.Engines =            row.EString(ordinal++);
            result.FirstCreated =       row.DateTime(ordinal++);
            result.GenericName =        row.EString(ordinal++);
            result.ICAOTypeCode =       row.EString(ordinal++);
            result.LastModified =       row.DateTime(ordinal++);
            result.Manufacturer =       row.EString(ordinal++);
            result.ModeS =              row.EString(ordinal++);
            result.ModeSCountry =       row.EString(ordinal++);
            result.OperatorFlagCode =   row.EString(ordinal++);
            result.OwnershipStatus =    row.EString(ordinal++);
            result.PopularName =        row.EString(ordinal++);
            result.PreviousID =         row.EString(ordinal++);
            result.RegisteredOwners =   row.EString(ordinal++);
            result.Registration =       row.EString(ordinal++);
            result.SerialNo =           row.EString(ordinal++);
            result.Status =             row.EString(ordinal++);
            result.Type =               row.EString(ordinal++);
            result.CofACategory =       row.EString(ordinal++);
            result.CofAExpiry =         row.EString(ordinal++);
            result.CurrentRegDate =     row.EString(ordinal++);
            result.FirstRegDate =       row.EString(ordinal++);
            result.InfoUrl =            row.EString(ordinal++);
            result.Interested =         row.Bool(ordinal++);
            result.MTOW =               row.EString(ordinal++);
            result.PictureUrl1 =        row.EString(ordinal++);
            result.PictureUrl2 =        row.EString(ordinal++);
            result.PictureUrl3 =        row.EString(ordinal++);
            result.TotalHours =         row.EString(ordinal++);
            result.UserNotes =          row.EString(ordinal++);
            result.UserString1 =        row.EString(ordinal++);
            result.UserString2 =        row.EString(ordinal++);
            result.UserString3 =        row.EString(ordinal++);
            result.UserString4 =        row.EString(ordinal++);
            result.UserString5 =        row.EString(ordinal++);
            result.UserBool1 =          row.Bool(ordinal++);
            result.UserBool2 =          row.Bool(ordinal++);
            result.UserBool3 =          row.Bool(ordinal++);
            result.UserBool4 =          row.Bool(ordinal++);
            result.UserBool5 =          row.Bool(ordinal++);
            result.UserInt1 =           row.Long(ordinal++);
            result.UserInt2 =           row.Long(ordinal++);
            result.UserInt3 =           row.Long(ordinal++);
            result.UserInt4 =           row.Long(ordinal++);
            result.UserInt5 =           row.Long(ordinal++);
            result.UserTag =            row.EString(ordinal++);
            result.YearBuilt =          row.EString(ordinal++);

            return result;
        }

        protected void AssertAircraftAreEqual(KineticAircraft expected, KineticAircraft actual, long id = -1L)
        {
            Assert.AreEqual(id == -1L ? expected.AircraftID : (int)id,  actual.AircraftID);
            Assert.AreEqual(expected.AircraftClass,                     actual.AircraftClass);
            Assert.AreEqual(expected.Country,                           actual.Country);
            Assert.AreEqual(expected.DeRegDate,                         actual.DeRegDate);
            Assert.AreEqual(expected.Engines,                           actual.Engines);
            Assert.AreEqual(expected.FirstCreated,                      actual.FirstCreated);
            Assert.AreEqual(expected.GenericName,                       actual.GenericName);
            Assert.AreEqual(expected.ICAOTypeCode,                      actual.ICAOTypeCode);
            Assert.AreEqual(expected.LastModified,                      actual.LastModified);
            Assert.AreEqual(expected.Manufacturer,                      actual.Manufacturer);
            Assert.AreEqual(expected.ModeS,                             actual.ModeS);
            Assert.AreEqual(expected.ModeSCountry,                      actual.ModeSCountry);
            Assert.AreEqual(expected.OperatorFlagCode,                  actual.OperatorFlagCode);
            Assert.AreEqual(expected.OwnershipStatus,                   actual.OwnershipStatus);
            Assert.AreEqual(expected.PopularName,                       actual.PopularName);
            Assert.AreEqual(expected.PreviousID,                        actual.PreviousID);
            Assert.AreEqual(expected.RegisteredOwners,                  actual.RegisteredOwners);
            Assert.AreEqual(expected.Registration,                      actual.Registration);
            Assert.AreEqual(expected.SerialNo,                          actual.SerialNo);
            Assert.AreEqual(expected.Status,                            actual.Status);
            Assert.AreEqual(expected.Type,                              actual.Type);
            Assert.AreEqual(expected.CofACategory,                      actual.CofACategory);
            Assert.AreEqual(expected.CofAExpiry,                        actual.CofAExpiry);
            Assert.AreEqual(expected.CurrentRegDate,                    actual.CurrentRegDate);
            Assert.AreEqual(expected.FirstRegDate,                      actual.FirstRegDate);
            Assert.AreEqual(expected.InfoUrl,                           actual.InfoUrl);
            Assert.AreEqual(expected.Interested,                        actual.Interested);
            Assert.AreEqual(expected.MTOW,                              actual.MTOW);
            Assert.AreEqual(expected.PictureUrl1,                       actual.PictureUrl1);
            Assert.AreEqual(expected.PictureUrl2,                       actual.PictureUrl2);
            Assert.AreEqual(expected.PictureUrl3,                       actual.PictureUrl3);
            Assert.AreEqual(expected.TotalHours,                        actual.TotalHours);
            Assert.AreEqual(expected.UserNotes,                         actual.UserNotes);
            Assert.AreEqual(expected.UserString1,                       actual.UserString1);
            Assert.AreEqual(expected.UserString2,                       actual.UserString2);
            Assert.AreEqual(expected.UserString3,                       actual.UserString3);
            Assert.AreEqual(expected.UserString4,                       actual.UserString4);
            Assert.AreEqual(expected.UserString5,                       actual.UserString5);
            Assert.AreEqual(expected.UserBool1,                         actual.UserBool1);
            Assert.AreEqual(expected.UserBool2,                         actual.UserBool2);
            Assert.AreEqual(expected.UserBool3,                         actual.UserBool3);
            Assert.AreEqual(expected.UserBool4,                         actual.UserBool4);
            Assert.AreEqual(expected.UserBool5,                         actual.UserBool5);
            Assert.AreEqual(expected.UserInt1,                          actual.UserInt1);
            Assert.AreEqual(expected.UserInt2,                          actual.UserInt2);
            Assert.AreEqual(expected.UserInt3,                          actual.UserInt3);
            Assert.AreEqual(expected.UserInt4,                          actual.UserInt4);
            Assert.AreEqual(expected.UserInt5,                          actual.UserInt5);
            Assert.AreEqual(expected.UserTag,                           actual.UserTag);
            Assert.AreEqual(expected.YearBuilt,                         actual.YearBuilt);
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

        protected void Common_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);

                var id = AddAircraft(mockAircraft);

                var aircraft = _Implementation.GetAircraftByRegistration(mockAircraft.Registration);
                Assert.AreNotSame(aircraft, mockAircraft);

                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }
        #endregion

        #region GetAircraftByCode
        protected void Common_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Implementation.GetAircraftByCode(null));
        }

        protected void Common_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Implementation.GetAircraftByCode("ABC123"));
        }

        protected void Common_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);
                var id = AddAircraft(mockAircraft);

                var aircraft = _Implementation.GetAircraftByCode(mockAircraft.ModeS);

                Assert.AreNotSame(aircraft, mockAircraft);
                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }
        #endregion
    }
}
