// Copyright © 2018 onwards, Andrew Whewell
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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Database;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class TrackHistoryDatabaseTests_SQLite : TrackHistoryDatabaseTests<ITrackHistoryDatabaseSQLite>
    {
        private string _DatabaseFileName;
        private SQLiteConnectionStringBuilder _ConnectionStringBuilder;

        [TestInitialize]
        public void TestInitialise()
        {
            CommonTestInitialise(() => {
                    _DatabaseFileName = Path.Combine(TestContext.TestDeploymentDir, "TrackHistoryDatabaseTests_SQLite.sqb");
                    if(File.Exists(_DatabaseFileName)) {
                        TestUtilities.RetryFileIOAction(() => File.Delete(_DatabaseFileName));
                    }
                    var createDatabase = Factory.Resolve<ITrackHistoryDatabaseSQLite>();
                    createDatabase.Create(_DatabaseFileName);

                    _ConnectionStringBuilder = new SQLiteConnectionStringBuilder() { DataSource = _DatabaseFileName, };
                },
                () => new SQLiteConnection(_ConnectionStringBuilder.ConnectionString),
                (db) => db.FileName = _DatabaseFileName,
                "SELECT last_insert_rowid();"
            );
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CommonTestCleanup();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Accepts_Configuration()
        {
            Assert.IsFalse(_Database.IsDataSourceReadOnly);
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Requires_A_FileName()
        {
            Assert.IsTrue(_Database.FileNameRequired);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TrackHistoryDatabase_SQLite_Create_Throws_If_Passed_Null()
        {
            _Database.Create(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TrackHistoryDatabase_SQLite_Create_Throws_If_Passed_Empty_String()
        {
            _Database.Create("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TrackHistoryDatabase_SQLite_Create_Throws_If_File_Already_Exists()
        {
            var fileName = Path.Combine(TestContext.DeploymentDirectory, "TrackHistory-Create-Test-1.sqb");
            File.WriteAllText(fileName, "");

            _Database.Create(fileName);
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Create_Creates_New_File()
        {
            // Originally I deleted the file after use but I took that out... Visual Studio leaves all the other
            // files hanging around and if something goes wrong it's handy to be able to examine it
            var fileName = Path.Combine(TestContext.DeploymentDirectory, "TrackHistory-Create-Test-2.sqb");
            _Database.Create(fileName);
            using(var connection = new SQLiteConnection(new SQLiteConnectionStringBuilder() { DataSource = fileName }.ConnectionString)) {
                connection.Open();
                var version = connection.Query<long>("SELECT [Version] FROM [DatabaseVersion] WHERE 1 = 1").Single();
                Assert.IsTrue(version > 0);
            }
        }

        #region Aircraft
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_Save_Creates_New_Aircraft_Correctly()
        {
            Aircraft_Save_Creates_New_Aircraft_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_Save_Updates_Existing_Records_Correctly()
        {
            Aircraft_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_GetByIcao_Returns_Aircraft_For_Icao()
        {
            Aircraft_GetByIcao_Returns_Aircraft_For_Icao();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_GetByIcao_Is_Case_Insensitive()
        {
            Aircraft_GetByIcao_Is_Case_Insensitive();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_Delete_Deletes_Aircraft()
        {
            Aircraft_Delete_Deletes_Aircraft();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Aircraft_Delete_Removes_Child_TrackHistories()
        {
            Aircraft_Delete_Removes_Child_TrackHistories();
        }
        #endregion

        #region AircraftType
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_AircraftType_Save_Creates_New_Records_Correctly()
        {
            AircraftType_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_AircraftType_Save_Updates_Existing_Records_Correctly()
        {
            AircraftType_Save_Updates_Existing_Records_Correctly();
        }
        #endregion

        #region Country
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_Save_Creates_New_Records_Correctly()
        {
            Country_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_Save_Updates_Existing_Records_Correctly()
        {
            Country_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            Country_GetByName_Fetches_By_Case_Insensitive_Name();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            Country_GetOrCreateByName_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            Country_GetOrCreateByName_Fetches_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_Delete_Deletes_Countries()
        {
            Country_Delete_Deletes_Countries();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_Delete_Nulls_Out_References_To_Country()
        {
            Country_Delete_Nulls_Out_References_To_Country();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Country_Delete_Ignores_Deleted_Countries()
        {
            Country_Delete_Ignores_Deleted_Countries();
        }
        #endregion

        #region EnginePlacement
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_EnginePlacement_GetByID_Returns_Correct_EnginePlacement()
        {
            EnginePlacement_GetByID_Returns_Correct_EnginePlacement();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_EnginePlacement_GetAll_Returns_All_EnginePlacement()
        {
            EnginePlacement_GetAll_Returns_All_EnginePlacement();
        }
        #endregion

        #region EngineType
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_EngineType_GetByID_Returns_Correct_EngineType()
        {
            EngineType_GetByID_Returns_Correct_EngineType();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_EngineType_GetAll_Returns_All_EngineType()
        {
            EngineType_GetAll_Returns_All_EngineType();
        }
        #endregion

        #region Manufacturer
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_Save_Creates_New_Records_Correctly()
        {
            Manufacturer_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_Save_Updates_Existing_Records_Correctly()
        {
            Manufacturer_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            Manufacturer_GetByName_Fetches_By_Case_Insensitive_Name();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            Manufacturer_GetOrCreateByName_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            Manufacturer_GetOrCreateByName_Fetches_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_Delete_Deletes_Manufacturers()
        {
            Manufacturer_Delete_Deletes_Manufacturers();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_Delete_Nulls_Out_References_To_Manufacturer()
        {
            Manufacturer_Delete_Nulls_Out_References_To_Manufacturer();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Manufacturer_Delete_Ignores_Deleted_Manufacturers()
        {
            Manufacturer_Delete_Ignores_Deleted_Manufacturers();
        }
        #endregion

        #region Model
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_Save_Creates_New_Records_Correctly()
        {
            Model_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_Save_Updates_Existing_Records_Correctly()
        {
            Model_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            Model_GetByName_Fetches_By_Case_Insensitive_Name();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            Model_GetOrCreateByName_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            Model_GetOrCreateByName_Fetches_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_Delete_Deletes_Models()
        {
            Model_Delete_Deletes_Models();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_Delete_Nulls_Out_References_To_Model()
        {
            Model_Delete_Nulls_Out_References_To_Model();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Model_Delete_Ignores_Deleted_Models()
        {
            Model_Delete_Ignores_Deleted_Models();
        }
        #endregion

        #region Receiver
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_Save_Creates_New_Records_Correctly()
        {
            Receiver_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_Save_Updates_Existing_Records_Correctly()
        {
            Receiver_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            Receiver_GetByName_Fetches_By_Case_Insensitive_Name();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            Receiver_GetOrCreateByName_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            Receiver_GetOrCreateByName_Fetches_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_Delete_Deletes_Receivers()
        {
            Receiver_Delete_Deletes_Receivers();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_Delete_Nulls_Out_References_To_Receiver()
        {
            Receiver_Delete_Nulls_Out_References_To_Receiver();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Receiver_Delete_Ignores_Deleted_Receivers()
        {
            Receiver_Delete_Ignores_Deleted_Receivers();
        }
        #endregion

        #region Species
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Species_GetByID_Returns_Correct_Species()
        {
            Species_GetByID_Returns_Correct_Species();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_Species_GetAll_Returns_All_Species()
        {
            Species_GetAll_Returns_All_Species();
        }
        #endregion

        #region TrackHistory
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Save_Creates_New_Records_Correctly()
        {
            TrackHistory_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Save_Updates_Existing_Records_Correctly()
        {
            TrackHistory_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_GetByAircraftID_With_No_Criteria_Returns_Correct_Records()
        {
            TrackHistory_GetByAircraftID_With_No_Criteria_Returns_Correct_Records();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_GetByAircraftID_With_Criteria_Returns_Correct_Records()
        {
            TrackHistory_GetByAircraftID_With_Criteria_Returns_Correct_Records();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_GetByDateRange_Returns_Correct_Records()
        {
            TrackHistory_GetByDateRange_Returns_Correct_Records();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Delete_Removes_TrackHistory_Records()
        {
            TrackHistory_Delete_Removes_TrackHistory_Records();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Delete_Can_Be_Called_Within_A_Transaction()
        {
            TrackHistory_Delete_Can_Be_Called_Within_A_Transaction();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_DeleteExpired_Deletes_Appropriate_Transactions()
        {
            TrackHistory_DeleteExpired_Deletes_Appropriate_Transactions();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Truncate_Removes_All_But_First_And_Last_States()
        {
            TrackHistory_Truncate_Removes_All_But_First_And_Last_States();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_Truncate_Ignores_Preserved_Histories()
        {
            TrackHistory_Truncate_Ignores_Preserved_Histories();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistory_TruncateExpired_Truncates_Histories()
        {
            TrackHistory_TruncateExpired_Truncates_Histories();
        }
        #endregion

        #region TrackHistoryState
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Creates_New_Records_Correctly()
        {
            TrackHistoryState_Save_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Updates_Existing_Records_Correctly()
        {
            TrackHistoryState_Save_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Can_Move_To_Another_TrackHistory()
        {
            TrackHistoryState_Save_Can_Move_To_Another_TrackHistory();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Throws_If_TrackHistoryID_Is_Zero()
        {
            TrackHistoryState_Save_Throws_If_TrackHistoryID_Is_Zero();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Throws_If_SequenceNumber_Is_Zero()
        {
            TrackHistoryState_Save_Throws_If_SequenceNumber_Is_Zero();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_Save_Throws_If_Timestamp_Is_Default()
        {
            TrackHistoryState_Save_Throws_If_Timestamp_Is_Default();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_SaveMany_Creates_New_Records_Correctly()
        {
            TrackHistoryState_SaveMany_Creates_New_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_SaveMany_Updates_Existing_Records_Correctly()
        {
            TrackHistoryState_SaveMany_Updates_Existing_Records_Correctly();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_TrackHistoryState_GetByTrackHistory_Returns_Saved_IDs_In_Correct_Order()
        {
            TrackHistoryState_GetByTrackHistory_Returns_Saved_IDs_In_Correct_Order();
        }
        #endregion

        #region WakeTurbulenceCategory
        [TestMethod]
        public void TrackHistoryDatabase_SQLite_WakeTurbulenceCategory_GetByID_Returns_Correct_WakeTurbulenceCategory()
        {
            WakeTurbulenceCategory_GetByID_Returns_Correct_WakeTurbulenceCategory();
        }

        [TestMethod]
        public void TrackHistoryDatabase_SQLite_WakeTurbulenceCategory_GetAll_Returns_All_WakeTurbulenceCategory()
        {
            WakeTurbulenceCategory_GetAll_Returns_All_WakeTurbulenceCategory();
        }
        #endregion
    }
}
