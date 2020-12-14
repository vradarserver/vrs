using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Database;
using VirtualRadar.Plugin.SqlServer;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class BaseStationDatabaseTests_SqlServer : BaseStationDatabaseTests
    {
        protected override string SchemaPrefix => "[BaseStation].";

        private const string ConnectionString = @"Server=.\SQLExpress;Trusted_Connection=True;";
        private const string TestDatabaseName = "VRSTest";

        private string DatabaseConnectionString => $"{ConnectionString};Database={TestDatabaseName}";
        private string MasterConnectionString => $"{ConnectionString};Database=master";

        [TestInitialize]
        public void TestInitialise()
        {
            CommonTestInitialise<IBaseStationDatabaseSqlServer>(() => {
                ResetTestDatabase();
                UpdateSchema();
            },
            () => new SqlConnection(DatabaseConnectionString),
            (db) => {
                db.ConnectionString = DatabaseConnectionString;
                db.CommandTimeoutSeconds = 60;

                // Make sure that Dapper is using DateTime2 when creating test records
                SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);
            },
            "SELECT SCOPE_IDENTITY();");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CommonTestCleanup();
        }

        private void ResetTestDatabase()
        {
            using(var connection = new SqlConnection(MasterConnectionString)) {
                connection.Open();
                SqlServerHelper.RunScript(connection, Scripts.ScriptResources.SqlServerReset);
            }
        }

        private void UpdateSchema()
        {
            using(var connection = new SqlConnection(DatabaseConnectionString)) {
                connection.Open();
                SqlServerHelper.RunScript(connection, global::VirtualRadar.Plugin.SqlServer.Scripts.Resources.UpdateSchema_sql);
            }
        }

        #region Constructors and Properties
        [TestMethod]
        public void SqlServer_BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work();
        }
        #endregion

        #region GetAircraftByRegistration
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration();
        }
        #endregion

        #region GetAircraftByCode
        [TestMethod]
        public void SqlServer_BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }
        #endregion

        #region GetManyAircraftByCode
        [TestMethod]
        public void SqlServer_BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
        {
            BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        [TestMethod]
        public void SqlServer_BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetAircraftById
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
        {
            BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier();
        }
        #endregion

        #region InsertAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region GetOrInsertAircraftByCode
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Correctly_Inserts_Record()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Looks_Up_ModeSCountry()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Looks_Up_ModeSCountry();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_Country()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_Country();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Unknown_Country()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Unknown_Country();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date();
        }
        #endregion

        #region UpdateAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated()
        {
            BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateAircraftModeSCountry
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
        {
            BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record();
        }
        #endregion

        #region RecordMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            BaseStationDatabase_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records()
        {
            BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1()
        {
            BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values()
        {
            BaseStationDatabase_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values();
        }
        #endregion

        #region RecordManyMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordManyMissingAircraft_Updates_Existing_Empty_Records()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations();
        }
        #endregion

        #region UpsertAircraftLookup
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Inserts_New_Lookups()
        {
            BaseStationDatabase_UpsertAircraftLookup_Inserts_New_Lookups();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Updates_Existing_Aircraft()
        {
            BaseStationDatabase_UpsertAircraftLookup_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required()
        {
            BaseStationDatabase_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required()
        {
            BaseStationDatabase_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration()
        {
            BaseStationDatabase_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update()
        {
            BaseStationDatabase_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            BaseStationDatabase_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region UpsertManyAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Inserts_New_Lookups()
        {
            BaseStationDatabase_UpsertManyAircraft_Inserts_New_Lookups();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Inserts_New_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Inserts_New_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Lookups()
        {
            BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Lookups();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup()
        {
            BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            BaseStationDatabase_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region DeleteAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SqlServer_BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record();
        }
        #endregion

        #region GetFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights()
        {
            BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields()
        {
            BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Callsigns$")]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign()
        {
            BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive()
        {
            BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive();
        }

        // See the README.md in the SQL Server plugin source - we leave the collation sequence up to the user for SQL Server
        //[TestMethod]
        //public void SqlServer_BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive()
        //{
        //    BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive();
        //}

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void SqlServer_BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows()
        {
            BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns()
        {
            BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names()
        {
            BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly()
        {
            BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly()
        {
            BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetFlightsForAircraft
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null()
        {
            BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft()
        {
            BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects()
        {
            BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object()
        {
            BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object();
        }

        // Does not apply to the SQL Server implementation...
        //[TestMethod]
        //public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria()
        //{
        //    BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria();
        //}

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria()
        {
            BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive()
        {
            BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive()
        {
            BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows()
        {
            BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns()
        {
            BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names()
        {
            BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly()
        {
            BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly()
        {
            BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetCountOfFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria();
        }
        #endregion

        #region GetCountOfFlightsForAircraft
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlServer_BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria();
        }
        #endregion

        #region GetFlightById
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist()
        {
            BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier()
        {
            BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier();
        }
        #endregion

        #region InsertFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_InsertFlight_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertFlight_Correctly_Inserts_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_InsertFlight_Works_In_Different_Cultures()
        {
            BaseStationDatabase_InsertFlight_Works_In_Different_Cultures();
        }
        #endregion

        #region UpdateFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date()
        {
            BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_UpdateFlight_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateFlight_Correctly_Updates_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record();
        }
        #endregion

        #region UpsertManyFlights
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpsertManyFlights_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpsertManyFlights_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_UpsertManyFlights_Inserts_New_Flights()
        {
            BaseStationDatabase_UpsertManyFlights_Inserts_New_Flights();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SqlServer_BaseStationDatabase_UpsertManyFlights_Updates_Existing_Flights()
        {
            BaseStationDatabase_UpsertManyFlights_Updates_Existing_Flights();
        }
        #endregion

        #region GetDatabaseHistory
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table()
        {
            BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table();
        }
        #endregion

        #region GetDatabaseVersion
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table()
        {
            BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table()
        {
            BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table();
        }
        #endregion

        #region GetSystemEvents
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table()
        {
            BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table();
        }
        #endregion

        #region InsertSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record();
        }
        #endregion

        #region GetLocations
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table()
        {
            BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table();
        }
        #endregion

        #region InsertLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertLocation_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertLocation_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertLocation_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertLocation_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateLocation_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateLocation_Correctly_Updates_Record();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record();
        }
        #endregion

        #region GetSessions
        [TestMethod]
        public void SqlServer_BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table()
        {
            BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table();
        }
        #endregion

        #region InsertSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertSession_Inserts_Record_Correctly()
        {
            BaseStationDatabase_InsertSession_Inserts_Record_Correctly();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertSession_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertSession_Works_For_Different_Cultures();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations()
        {
            BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations();
        }
        #endregion

        #region UpdateSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateSession_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateSession_Correctly_Updates_Record(timeGetsRounded: false);
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_UpdateSession_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateSession_Works_For_Different_Cultures(timeGetsRounded: false);
        }
        #endregion

        #region DeleteSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_DeleteSession_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteSession_Correctly_Deletes_Record();
        }
        #endregion

        #region Transactions
        [TestMethod]
        public void SqlServer_BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database()
        {
            BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database();
        }

        [TestMethod]
        public void SqlServer_BaseStationDatabase_Transactions_Can_Rollback_Inserts()
        {
            BaseStationDatabase_Transactions_Can_Rollback_Inserts();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SqlServer_BaseStationDatabase_Transactions_Cannot_Be_Nested()
        {
            BaseStationDatabase_Transactions_Cannot_Be_Nested();
        }
        #endregion
    }
}
