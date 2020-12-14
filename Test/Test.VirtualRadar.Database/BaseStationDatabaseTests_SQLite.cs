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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Database;
using System.IO;
using Test.Framework;
using System.Data.SQLite;
using Moq;
using System.Reflection;
using System.Threading;
using System.Globalization;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class BaseStationDatabaseTests_SQLite : BaseStationDatabaseTests
    {
        private string _CreateDatabaseFileName;
        private string _EmptyDatabaseFileName;
        private SQLiteConnectionStringBuilder _ConnectionStringBuilder;

        protected override bool EngineTruncatesMilliseconds => true;

        [TestInitialize]
        public void TestInitialise()
        {
            CommonTestInitialise<IBaseStationDatabaseSQLite>(() => {
                _CreateDatabaseFileName = Path.Combine(TestContext.TestDeploymentDir, "CreatedDatabase.sqb");
                if(File.Exists(_CreateDatabaseFileName)) {
                    RetryAction(() => File.Delete(_CreateDatabaseFileName));
                }

                _EmptyDatabaseFileName = Path.Combine(TestContext.TestDeploymentDir, "TestCopyBaseStation.sqb");
                if(File.Exists(_EmptyDatabaseFileName)) {
                    RetryAction(() => File.Delete(_EmptyDatabaseFileName));
                }
                File.Copy(Path.Combine(TestContext.TestDeploymentDir, "BaseStation.sqb"), _EmptyDatabaseFileName, true);

                _ConnectionStringBuilder = new SQLiteConnectionStringBuilder() { DataSource = _EmptyDatabaseFileName };
            },
            () => new SQLiteConnection(_ConnectionStringBuilder.ConnectionString),
            (db) => db.FileName = _EmptyDatabaseFileName,
            "SELECT last_insert_rowid();");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CommonTestCleanup();
        }

        #region Constructors and Properties
        [TestMethod]
        public void SQLite_BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_IsConnected_Indicates_State_Of_Connection()
        {
            Assert.IsFalse(_Database.IsConnected);
            _Database.GetAircraftByCode("000000");
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileName_Change_Closes_Connection_If_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.FileName = _CreateDatabaseFileName;
            Assert.IsFalse(_Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileName_Set_Does_Not_Close_Connection_If_Value_Unchanged()
        {
            _Database.GetAircraftByCode("000000");
            _Database.FileName = _Database.FileName;
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileName_Change_Can_Allow_Methods_To_Start_Returning_Data()
        {
            AddAircraft(CreateAircraft("123456", "REG"));

            _Database.FileName = "c:\\DoesNotExist\\ThisFileDoesNotExist.ifitdoesthenitwillbetremendousbadluck";

            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
            Assert.IsFalse(_Database.IsConnected);

            _Database.FileName = _EmptyDatabaseFileName;

            Assert.IsNotNull(_Database.GetAircraftByRegistration("REG"));
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabse_WriteSupportEnabled_Closes_Connection_If_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.WriteSupportEnabled = true;
            Assert.AreEqual(true, _Database.WriteSupportEnabled);
            Assert.IsFalse(_Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabse_WriteSupportEnabled_Leaves_Connection_If_Not_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.WriteSupportEnabled = false;
            Assert.AreEqual(false, _Database.WriteSupportEnabled);
            Assert.IsTrue(_Database.IsConnected);
        }
        #endregion

        #region MaxParameters
        [TestMethod]
        public void SQLite_BaseStationDatabase_MaxParameters_Returns_Minus_One_When_Disconnected()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(-1, _Database.MaxParameters);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_MaxParameters_Returns_Correct_Value_When_Connected()
        {
            // SQLite's max parameters is a compile time variable. It can be fetched via a C function call but
            // that's a little tricky to do with ADO.NET when you can't use interop, so we're hard-coding to
            // the default, which is 999. The database object lops off a few parameters for internal use,
            // leaving a result of 900. This will need adjusting for other engines.
            Assert.AreEqual(900, _Database.MaxParameters);
        }
        #endregion

        #region FileNameChanging event
        [TestMethod]
        public void SQLite_BaseStationDatabase_FileNameChanging_Raised_Before_FileName_Changes()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanging += _FileNameChangingEvent.Handler;
            _FileNameChangingEvent.EventRaised += (s, a) => {
                Assert.AreEqual("OLD", _Database.FileName);
            };

            _Database.FileName = "NEW";

            Assert.AreEqual(1, _FileNameChangingEvent.CallCount);
            Assert.AreSame(_Database, _FileNameChangingEvent.Sender);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileNameChanging_Not_Raised_If_FileName_Does_Not_Change()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanging += _FileNameChangingEvent.Handler;

            _Database.FileName = "OLD";

            Assert.AreEqual(0, _FileNameChangingEvent.CallCount);
        }
        #endregion

        #region FileNameChanged event
        [TestMethod]
        public void SQLite_BaseStationDatabase_FileNameChanged_Raised_After_FileName_Changes()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanged += _FileNameChangedEvent.Handler;
            _FileNameChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual("NEW", _Database.FileName);
            };

            _Database.FileName = "NEW";

            Assert.AreEqual(1, _FileNameChangedEvent.CallCount);
            Assert.AreSame(_Database, _FileNameChangedEvent.Sender);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileNameChanged_Not_Raised_If_FileName_Does_Not_Change()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanged += _FileNameChangedEvent.Handler;

            _Database.FileName = "OLD";

            Assert.AreEqual(0, _FileNameChangedEvent.CallCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void SQLite_BaseStationDatabaseTests_Dispose_Prevents_Reopening_Of_Connection()
        {
            var flight = CreateFlight("ABC123");
            AddAircraft(flight.Aircraft);
            AddFlight(flight);
            _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);

            _Database.Dispose();

            var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
            Assert.AreEqual(0, flights.Count);
        }
        #endregion

        #region TestConnection
        [TestMethod]
        public void SQLite_BaseStationDatabase_TestConnection_Returns_False_If_File_Could_Not_Be_Opened()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.IsFalse(_Database.TestConnection());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_TestConnection_Returns_True_If_File_Could_Be_Opened()
        {
            Assert.IsTrue(_Database.TestConnection());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_TestConnection_Leaves_Connection_Open()
        {
            Assert.IsTrue(_Database.TestConnection());
            Assert.IsTrue(_Database.IsConnected);

            bool seenConnectionOpen = false;
            try {
                File.Delete(_EmptyDatabaseFileName);
            } catch(IOException) {
                seenConnectionOpen = true;
            }
            Assert.IsTrue(seenConnectionOpen);
        }
        #endregion

        #region Connection
        [TestMethod]
        public void SQLite_BaseStationDatabase_Connection_Does_Not_Create_File_If_Missing()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            _Database.GetAircraftByRegistration("G-ABCD");
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_Connection_Does_Not_Try_To_Use_Zero_Length_Files()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            File.Create(_EmptyDatabaseFileName).Close();

            Assert.IsNull(_Database.GetAircraftByRegistration("G-ABCD"));
            Assert.AreEqual(false, _Database.IsConnected);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_Connection_Can_Work_With_ReadOnly_Access()
        {
            var fileInfo = new FileInfo(_EmptyDatabaseFileName);
            try {
                AddAircraft(CreateAircraft("ABCDEF", "G-AGWP"));
                fileInfo.IsReadOnly = true;
                Assert.IsNotNull(_Database.GetAircraftByRegistration("G-AGWP"));
            } finally {
                fileInfo.IsReadOnly = false;
            }
        }
        #endregion

        #region FileExists
        [TestMethod]
        public void SQLite_BaseStationDatabase_FileExists_Returns_True_If_The_FileName_Exists()
        {
            _FileSystem.AddFile(@"c:\tmp\file.sqb", new byte[0]);
            _Database.FileName = @"c:\tmp\file.sqb";

            Assert.IsTrue(_Database.FileExists());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileExists_Returns_False_If_The_FileName_Does_Not_Exist()
        {
            _FileSystem.AddFile(@"c:\tmp\file.sqb", new byte[0]);
            _Database.FileName = @"c:\tmp\not-file.sqb";

            Assert.IsFalse(_Database.FileExists());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileExists_Does_Not_Test_Null_FileName()
        {
            _Database.FileName = null;
            Assert.IsFalse(_Database.FileExists());
            Assert.AreEqual(0, _FileSystem.FileExists_CallCount);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileExists_Does_Not_Test_Empty_FileName()
        {
            _Database.FileName = "";
            Assert.IsFalse(_Database.FileExists());
            Assert.AreEqual(0, _FileSystem.FileExists_CallCount);
        }
        #endregion

        #region FileIsEmpty
        [TestMethod]
        public void SQLite_BaseStationDatabase_FileIsEmpty_Returns_True_If_The_File_Is_Empty()
        {
            _FileSystem.AddFile(@"c:\tmp\file.sqb", new byte[0]);
            _Database.FileName = @"c:\tmp\file.sqb";

            Assert.IsTrue(_Database.FileIsEmpty());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_FileIsEmpty_Returns_False_If_The_FileName_Is_Not_Empty()
        {
            _FileSystem.AddFile(@"c:\tmp\file.sqb", new byte[] { 0x01 });
            _Database.FileName = @"c:\tmp\file.sqb";

            Assert.IsFalse(_Database.FileIsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void SQLite_BaseStationDatabase_FileIsEmpty_Throws_Exception_If_File_Does_Not_Exist()
        {
            _FileSystem.AddFolder(@"c:\tmp");
            _Database.FileName = @"c:\tmp\no-file.sqb";
            _Database.FileIsEmpty();
        }
        #endregion

        #region GetAircraftByRegistration
        [TestMethod]
        public void SQLite_BaseStationDatatbase_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Database_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Database_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration();
        }
        #endregion

        #region GetAircraftByCode
        [TestMethod]
        public void SQLite_BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }
        #endregion

        #region GetManyAircraftByCode
        [TestMethod]
        public void SQLite_BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
        {
            BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        [TestMethod]
        public void SQLite_BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetAircraftById
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftById_Returns_Null_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetAircraftById_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
        {
            BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier();
        }
        #endregion

        #region InsertAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled();
        }


        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("123456"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertAircraft_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;

            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X", FirstCreated = time1, LastModified = time2 });
            var readBack = _Database.GetAircraftByCode("X");

            Assert.AreEqual(TruncateDate(time1), readBack.FirstCreated);
            Assert.AreEqual(TruncateDate(time2), readBack.LastModified);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region GetOrInsertAircraftByCode
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.GetOrInsertAircraftByCode("123456", out var created);
            Assert.AreEqual(null, _Database.GetAircraftByCode("123456"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.GetOrInsertAircraftByCode("123456", out var created);
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Correctly_Inserts_Record()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Looks_Up_ModeSCountry()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Looks_Up_ModeSCountry();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_Country()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_Country();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Unknown_Country()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Unknown_Country();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date()
        {
            BaseStationDatabase_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date();
        }
        #endregion

        #region UpdateAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.UpdateAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            var update = _Database.GetAircraftByCode("X");
            update.FirstCreated = time1;
            update.LastModified = time2;
            _Database.UpdateAircraft(update);

            var readBack = _Database.GetAircraftByCode("X");
            Assert.AreEqual(TruncateDate(time1), readBack.FirstCreated);
            Assert.AreEqual(TruncateDate(time2), readBack.LastModified);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated()
        {
            BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateAircraftModeSCountry
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateAircraftModeSCountry_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateAircraftModeSCountry(1, "X");
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
        {
            BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record();
        }
        #endregion

        #region RecordMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            BaseStationDatabase_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records()
        {
            BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1()
        {
            BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values()
        {
            BaseStationDatabase_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values();
        }
        #endregion

        #region RecordManyMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordManyMissingAircraft_Updates_Existing_Empty_Records()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations()
        {
            BaseStationDatabase_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations();
        }
        #endregion

        #region UpsertAircraftLookup
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Inserts_New_Lookups()
        {
            BaseStationDatabase_UpsertAircraftLookup_Inserts_New_Lookups();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Updates_Existing_Aircraft()
        {
            BaseStationDatabase_UpsertAircraftLookup_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required()
        {
            BaseStationDatabase_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required()
        {
            BaseStationDatabase_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration()
        {
            BaseStationDatabase_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update()
        {
            BaseStationDatabase_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            BaseStationDatabase_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region UpsertManyAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            BaseStationDatabase_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Inserts_New_Lookups()
        {
            BaseStationDatabase_UpsertManyAircraft_Inserts_New_Lookups();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Inserts_New_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Inserts_New_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Lookups()
        {
            BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Lookups();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup()
        {
            BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft()
        {
            BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            BaseStationDatabase_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region DeleteAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.DeleteAircraft(new BaseStationAircraft());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteAircraft(new BaseStationAircraft());
            // Nothing much we can do to assure ourselves that nothing has changed here
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void SQLite_BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record();
        }
        #endregion

        #region GetFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SQLite_BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights()
        {
            BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria()
        {
            BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields()
        {
            BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Callsigns$")]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign()
        {
            BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive()
        {
            BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive()
        {
            BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void SQLite_BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows()
        {
            BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns()
        {
            BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names()
        {
            BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly()
        {
            BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly()
        {
            BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetFlightsForAircraft
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null()
        {
            BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft()
        {
            BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects()
        {
            BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object()
        {
            BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria()
        {
            BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria()
        {
            BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive()
        {
            BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive()
        {
            BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows()
        {
            BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns()
        {
            BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names()
        {
            BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly()
        {
            BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly()
        {
            BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetCountOfFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SQLite_BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria()
        {
            BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria();
        }
        #endregion

        #region GetCountOfFlightsForAircraft
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SQLite_BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria()
        {
            BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria();
        }
        #endregion

        #region GetFlightById
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist()
        {
            BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightById_Returns_Null_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.IsNull(_Database.GetFlightById(1));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetFlightById_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetFlightById(1));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier()
        {
            BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier();
        }
        #endregion

        #region InsertFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.InsertFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertFlight(new BaseStationFlight());
            // Can't think of a good way to test that it didn't do anything, but if it tried to write to a file without knowing the
            // name of the file it'd crash so if the test passes without any exceptions being raised then that's a good sign
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertFlight_Truncates_Milliseconds_From_Date()
        {
            BaseStationDatabase_InsertFlight_Truncates_Milliseconds_From_Date();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_InsertFlight_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertFlight_Correctly_Inserts_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_InsertFlight_Works_In_Different_Cultures()
        {
            BaseStationDatabase_InsertFlight_Works_In_Different_Cultures();
        }
        #endregion

        #region UpdateFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.UpdateFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateFlight(new BaseStationFlight());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date()
        {
            BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_UpdateFlight_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateFlight_Correctly_Updates_Record();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.DeleteFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteFlight(new BaseStationFlight());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record();
        }
        #endregion

        #region UpsertManyFlights
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpsertManyFlights_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpsertManyFlights_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_UpsertManyFlights_Inserts_New_Flights()
        {
            BaseStationDatabase_UpsertManyFlights_Inserts_New_Flights();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void SQLite_BaseStationDatabase_UpsertManyFlights_Updates_Existing_Flights()
        {
            BaseStationDatabase_UpsertManyFlights_Updates_Existing_Flights();
        }
        #endregion

        #region GetDatabaseHistory
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseHistory_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseHistory_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table()
        {
            BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table();
        }
        #endregion

        #region GetDatabaseVersion
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseVersion_Returns_Null_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(null, _Database.GetDatabaseVersion());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseVersion_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(null, _Database.GetDatabaseVersion());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table()
        {
            BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table()
        {
            BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table();
        }
        #endregion

        #region GetSystemEvents
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSystemEvents_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSystemEvents_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table()
        {
            BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table();
        }
        #endregion

        #region InsertSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.UpdateSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.DeleteSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record();
        }
        #endregion

        #region GetLocations
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetLocations_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetLocations_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table()
        {
            BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table();
        }
        #endregion

        #region InsertLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertLocation_Correctly_Inserts_Record()
        {
            BaseStationDatabase_InsertLocation_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertLocation_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertLocation_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.UpdateLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateLocation_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateLocation_Correctly_Updates_Record();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures();
        }
        #endregion

        #region DeleteLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.DeleteLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record();
        }
        #endregion

        #region GetSessions
        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSessions_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSessions_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;

            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table()
        {
            BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table();
        }
        #endregion

        #region InsertSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.InsertSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSession_Inserts_Record_Correctly()
        {
            BaseStationDatabase_InsertSession_Inserts_Record_Correctly();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSession_Works_For_Different_Cultures()
        {
            BaseStationDatabase_InsertSession_Works_For_Different_Cultures();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations()
        {
            BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations();
        }
        #endregion

        #region UpdateSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.UpdateSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSession_Correctly_Updates_Record()
        {
            BaseStationDatabase_UpdateSession_Correctly_Updates_Record(timeGetsRounded: true);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_UpdateSession_Works_For_Different_Cultures()
        {
            BaseStationDatabase_UpdateSession_Works_For_Different_Cultures(timeGetsRounded: true);
        }
        #endregion

        #region DeleteSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled()
        {
            BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            RetryAction(() => File.Delete(_EmptyDatabaseFileName));

            _Database.DeleteSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_DeleteSession_Correctly_Deletes_Record()
        {
            BaseStationDatabase_DeleteSession_Correctly_Deletes_Record();
        }
        #endregion

        #region CreateDatabaseIfMissing
        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_FileName_Is_Null()
        {
            _Database.CreateDatabaseIfMissing(null);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_FileName_Is_Empty()
        {
            _Database.CreateDatabaseIfMissing("");
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Path_To_Database_File()
        {
            string folder = Path.Combine(TestContext.TestDeploymentDir, "SubFolderForCDIF");
            if(Directory.Exists(folder)) Directory.Delete(folder, true);

            _CreateDatabaseFileName = Path.Combine(folder, "TheFile.sdb");

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.IsTrue(Directory.Exists(folder));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_File_If_Missing()
        {
            if(File.Exists(_CreateDatabaseFileName)) {
                RetryAction(() => File.Delete(_CreateDatabaseFileName));
            }

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.IsTrue(File.Exists(_CreateDatabaseFileName));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_File_Empty()
        {
            File.Create(_CreateDatabaseFileName).Close();

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            Assert.IsTrue(_Database.GetDatabaseHistory().Where(h => h.IsCreationOfDatabaseByVirtualRadarServer).Any());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_File_Not_Empty()
        {
            _CreateDatabaseFileName = _EmptyDatabaseFileName;
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count());
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_Missing()
        {
            var timeStamp = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Provider.Setup(p => p.UtcNow).Returns(timeStamp);

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var allHistory = _Database.GetDatabaseHistory();
            Assert.AreEqual(1, allHistory.Count());

            var vrsCreationNote = allHistory.Where(h => h.IsCreationOfDatabaseByVirtualRadarServer).Single();
            Assert.AreEqual(TruncateDate(timeStamp), vrsCreationNote.TimeStamp);
            Assert.AreNotEqual(0, vrsCreationNote.DBHistoryID);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_Missing();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var dbInfo = _Database.GetDatabaseVersion();
            Assert.AreEqual(2, dbInfo.OriginalVersion);
            Assert.AreEqual(2, dbInfo.CurrentVersion);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Google_Map_Default_Centre_As_Default_Location()
        {
            _Configuration.GoogleMapSettings.InitialMapLatitude = 2.3456;
            _Configuration.GoogleMapSettings.InitialMapLongitude = -7.8901;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var locations = _Database.GetLocations();
            Assert.AreEqual(1, locations.Count());

            var location = locations.First();
            Assert.AreEqual("Home", location.LocationName);
            Assert.AreEqual(0.0, location.Altitude);
            Assert.AreEqual(2.3456, location.Latitude, 0.00001);
            Assert.AreEqual(-7.8901, location.Longitude, 0.00001);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Default_Location_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Writes_Google_Map_Default_Centre_As_Default_Location();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Session_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var location = _Database.GetLocations()[0];
            var session = new BaseStationSession() { LocationID = location.LocationID };

            _Database.WriteSupportEnabled = true;
            _Database.InsertSession(session);
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_SystemEvents_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.GetSystemEvents();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Aircraft_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Flights_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var aircraft = new BaseStationAircraft() { ModeS = "1" };
            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.WriteSupportEnabled = true;
            _Database.InsertSession(session);
            _Database.InsertAircraft(aircraft);

            _Database.InsertFlight(new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID });
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Session_Trigger_To_Delete_Flights()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;

            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.InsertSession(session);

            var aircraft = new BaseStationAircraft() { ModeS = "K" };
            _Database.InsertAircraft(aircraft);

            var flight = new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID };
            _Database.InsertFlight(flight);

            _Database.DeleteSession(session);

            Assert.IsNull(_Database.GetFlightById(flight.FlightID));
            Assert.IsNotNull(_Database.GetAircraftById(aircraft.AircraftID));
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_CreateDatabaseIfMissing_Creates_Aircraft_Trigger_To_Delete_Flights()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;

            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.InsertSession(session);

            var aircraft = new BaseStationAircraft() { ModeS = "K" };
            _Database.InsertAircraft(aircraft);

            var flight = new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID };
            _Database.InsertFlight(flight);

            _Database.DeleteAircraft(aircraft);

            Assert.IsNull(_Database.GetFlightById(flight.FlightID));
            Assert.AreEqual(session.SessionID, _Database.GetSessions()[0].SessionID);
        }
        #endregion

        #region Transactions
        [TestMethod]
        public void SQLite_BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database()
        {
            BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database();
        }

        [TestMethod]
        public void SQLite_BaseStationDatabase_Transactions_Can_Rollback_Inserts()
        {
            BaseStationDatabase_Transactions_Can_Rollback_Inserts();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SQLite_BaseStationDatabase_Transactions_Cannot_Be_Nested()
        {
            BaseStationDatabase_Transactions_Cannot_Be_Nested();
        }
        #endregion
    }
}
