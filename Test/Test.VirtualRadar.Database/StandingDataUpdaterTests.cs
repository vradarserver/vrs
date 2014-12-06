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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class StandingDataUpdaterTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const string ConfigurationFolder = @"C:\CONFIG\";
        private const string DatabaseFileName = ConfigurationFolder + "STANDINGDATA.SQB";
        private const string DatabaseTempFileName = ConfigurationFolder + "STANDINGDATA.SQB.TMP";
        private const string DatabaseFileUrl = "HTTP://WWW.VIRTUALRADARSERVER.CO.UK/FILES/STANDINGDATA.SQB.GZ";
        private const string StateFileName = ConfigurationFolder + "FLIGHTNUMBERCOVERAGE.CSV";
        private const string StateTempFileName = ConfigurationFolder + "FLIGHTNUMBERCOVERAGE.CSV.TMP";
        private const string StateFileUrl = "HTTP://WWW.VIRTUALRADARSERVER.CO.UK/FILES/FLIGHTNUMBERCOVERAGE.CSV";
        private const string BasicAircraftLookupFileName = ConfigurationFolder + "BASICAIRCRAFTLOOKUP.SQB";
        private const string BasicAircraftLookupTempFileName = ConfigurationFolder + "BASICAIRCRAFTLOOKUP.SQB.TMP";
        private const string BasicAircraftLookupFileUrl = "HTTP://WWW.VIRTUALRADARSERVER.CO.UK/FILES/BASICAIRCRAFTLOOKUP.SQB.GZ";
        private readonly string[] AllFileNames = {
            DatabaseFileName,
            DatabaseTempFileName,
            StateFileName,
            StateTempFileName,
            BasicAircraftLookupFileName,
            BasicAircraftLookupTempFileName,
        };
        private readonly string[] AllUrls = {
            DatabaseFileUrl,
            StateFileUrl,
            BasicAircraftLookupFileUrl,
        };

        private IClassFactory _OriginalClassFactory;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private IStandingDataUpdater _Implementation;
        private Mock<IStandingDataUpdaterProvider> _Provider;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IBasicAircraftLookupDatabase> _BasicAircraftLookupDatabase;
        private object _StandingDataManagerLock;
        private object _BasicAircraftLookupDatabaseLock;
        private Dictionary<string, bool> _FileExists;
        private Dictionary<string, List<string>> _ReadLines;
        private Dictionary<string, List<string>> _DownloadLines;
        private Dictionary<string, List<string>> _SavedLines;
        private Dictionary<string, string> _DownloadedCompressedFiles;
        private Dictionary<string, string> _MovedFiles;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(r => r.Folder).Returns(ConfigurationFolder);

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataManagerLock = new object();
            _StandingDataManager.Setup(r => r.Lock).Returns(_StandingDataManagerLock);

            _BasicAircraftLookupDatabase = TestUtilities.CreateMockSingleton<IBasicAircraftLookupDatabase>();
            _BasicAircraftLookupDatabaseLock = new object();
            _BasicAircraftLookupDatabase.Setup(r => r.Lock).Returns(_BasicAircraftLookupDatabaseLock);

            _Implementation = Factory.Singleton.Resolve<IStandingDataUpdater>();
            _Provider = new Mock<IStandingDataUpdaterProvider>();
            _Implementation.Provider = _Provider.Object;

            _FileExists = new Dictionary<string,bool>();
            foreach(var fileName in AllFileNames) {
                _FileExists.Add(fileName, true);
            }
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns((string fileName) => {
                return _FileExists[fileName.ToUpper()];
            });

            _ReadLines = new Dictionary<string,List<string>>();
            foreach(var fileName in AllFileNames) {
                _ReadLines.Add(fileName, new List<string>());
            }
            _Provider.Setup(p => p.ReadLines(It.IsAny<string>())).Returns((string fileName) => {
                return _ReadLines[fileName.ToUpper()].ToArray();
            });

            _DownloadLines = new Dictionary<string,List<string>>();
            foreach(var url in AllUrls) {
                _DownloadLines.Add(url, new List<string>());
            }
            _Provider.Setup(p => p.DownloadLines(It.IsAny<string>())).Returns((string url) => {
                return _DownloadLines[url.ToUpper()].ToArray();
            });

            _SavedLines = new Dictionary<string,List<string>>();
            _Provider.Setup(p => p.WriteLines(It.IsAny<string>(), It.IsAny<string[]>())).Callback((string fileName, string[] lines) => {
                var key = fileName.ToUpper();
                var value = new List<string>(lines);
                _SavedLines.Add(key, value);
            });

            _DownloadedCompressedFiles = new Dictionary<string,string>();
            _Provider.Setup(r => r.DownloadAndDecompressFile(It.IsAny<string>(), It.IsAny<string>())).Callback((string url, string fileName) => {
                _DownloadedCompressedFiles.Add(url.ToUpper(), fileName.ToUpper());
            });

            _MovedFiles = new Dictionary<string,string>();
            _Provider.Setup(r => r.MoveFile(It.IsAny<string>(), It.IsAny<string>())).Callback((string temporaryFileName, string liveFileName) => {
                _MovedFiles.Add(temporaryFileName.ToUpper(), liveFileName.ToUpper());
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);
        }
        #endregion

        #region SetupStateFileResult, AssertFileDownloaded
        private void SetupStateFileResult(Dictionary<string, List<string>> resultsMap, string key, string line)
        {
            resultsMap[key].Add("StartDate,EndDate,Count,AirlinesMD5,AirportsMD5,TypesMD5,CodeBlocksMD5,StandingDataZipMD5");
            resultsMap[key].Add(line);
        }

        private void SetupValidStateFileDownload()
        {
            _DownloadLines[StateFileUrl].Add("A,B,C,D,E,F,G,H,I");
            _DownloadLines[StateFileUrl].Add("1,2,3,4,5,6,7,8,9");
        }

        private void SetupValidLocalStateFile()
        {
            _ReadLines[StateFileName].Add("a,b,c,d,e,f,g,h,i");
            _ReadLines[StateFileName].Add("1,2,3,4,5,6,7,8,9");
        }

        private void AssertLinesDownloaded(string url, string tempFileName, string finalFileName)
        {
            _Provider.Verify(p => p.DownloadLines(It.Is<string>(u => u.ToUpper() == url)), Times.Once());
            _Provider.Verify(p => p.WriteLines(It.Is<string>(f => f.ToUpper() == tempFileName), It.IsAny<string[]>()), Times.Once());

            List<string> downloadedLines;
            if(!_DownloadLines.TryGetValue(url, out downloadedLines)) downloadedLines = new List<string>();

            _StandingDataManager.VerifyGet(r => r.Lock, Times.Once());

            List<string> savedLines;
            if(!_SavedLines.TryGetValue(tempFileName, out savedLines)) savedLines = new List<string>();

            Assert.AreEqual(finalFileName, _MovedFiles[tempFileName]);

            _StandingDataManager.Verify(r => r.Load(), Times.Once());
        }
        #endregion

        #region Constructors
        [TestMethod]
        public void StandingDataUpdater_Constructor_Initialises_Provider()
        {
            var implementation = Factory.Singleton.Resolve<IStandingDataUpdater>();
            Assert.IsNotNull(implementation.Provider);
            TestUtilities.TestProperty(implementation, "Provider", implementation.Provider, _Provider.Object);
        }
        #endregion

        #region DataIsOld
        [TestMethod]
        public void StandingDataUpdater_DataIsOld_Returns_True_If_State_File_Is_Missing()
        {
            _FileExists[StateFileName] = false;
            Assert.AreEqual(true, _Implementation.DataIsOld());
            _Provider.Verify(p => p.FileExists(It.Is<string>(f => f.ToUpper() == StateFileName)), Times.Once());
            _Provider.Verify(p => p.DownloadLines(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void StandingDataUpdater_DataIsOld_Downloads_Current_State_File()
        {
            _Implementation.DataIsOld();
            _Provider.Verify(p => p.DownloadLines(It.Is<string>(u => u.ToUpper() == StateFileUrl)), Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_DataIsOld_Returns_False_If_Remote_And_Local_State_Files_Are_Same()
        {
            SetupStateFileResult(_DownloadLines, StateFileUrl, "a");
            SetupStateFileResult(_ReadLines, StateFileName, "a");
            Assert.AreEqual(false, _Implementation.DataIsOld());
        }

        [TestMethod]
        public void StandingDataUpdater_DataIsOld_Returns_True_If_Remote_And_Local_State_Files_Differ()
        {
            SetupStateFileResult(_DownloadLines, StateFileUrl, "1");
            SetupStateFileResult(_ReadLines, StateFileName, "2");
            Assert.AreEqual(true, _Implementation.DataIsOld());
        }
        #endregion

        #region Update
        #region State file
        [TestMethod]
        public void StandingDataUpdater_Update_Always_Downloads_State_File()
        {
            SetupValidStateFileDownload();
            _Implementation.Update();
            _Provider.Verify(p => p.DownloadLines(It.Is<string>(u => u.ToUpper() == StateFileUrl)), Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Writes_Local_State_File_If_Missing()
        {
            _FileExists[StateFileName] = false;
            SetupValidStateFileDownload();
            _Implementation.Update();
            AssertLinesDownloaded(StateFileUrl, StateTempFileName, StateFileName);
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Does_Not_Write_Local_State_File_If_Nothing_Has_Changed_On_Data_Line()
        {
            _DownloadLines[StateFileUrl].Add("A,B,C,D,E,F,G,H,I");
            _DownloadLines[StateFileUrl].Add("1,2,3,4,5,6,7,8,9");
            _DownloadLines[StateFileUrl].Add("IGNORED");

            _ReadLines[StateFileName].Add("Ignored");
            _ReadLines[StateFileName].Add("1,2,3,4,5,6,7,8,9");

            _Implementation.Update();

            _Provider.Verify(p => p.WriteLines(It.Is<string>(f => f.ToUpper() == StateTempFileName), It.IsAny<string[]>()), Times.Never());
        }
        #endregion

        #region Database file
        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_Database_File_If_Local_Database_Missing()
        {
            SetupValidStateFileDownload();
            SetupValidLocalStateFile();
            _FileExists[DatabaseFileName] = false;

            _Implementation.Update();

            Assert.AreEqual(DatabaseTempFileName, _DownloadedCompressedFiles[DatabaseFileUrl]);
            Assert.AreEqual(DatabaseFileName, _MovedFiles[DatabaseTempFileName]);

            _StandingDataManager.VerifyGet(r => r.Lock, Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_Database_File_If_Local_Database_Has_Old_Checksum()
        {
            SetupStateFileResult(_DownloadLines, StateFileUrl,   "A,B,C,D,E,F,G,H");
            SetupStateFileResult(_ReadLines, StateFileName,      "A,B,C,D,E,F,G,*");
            _FileExists[DatabaseFileName] = false;

            _Implementation.Update();

            Assert.AreEqual(DatabaseTempFileName, _DownloadedCompressedFiles[DatabaseFileUrl]);
            Assert.AreEqual(DatabaseFileName, _MovedFiles[DatabaseTempFileName]);

            _StandingDataManager.VerifyGet(r => r.Lock, Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Does_Not_Download_Database_File_If_Local_Database_Correct()
        {
            SetupValidStateFileDownload();
            SetupValidLocalStateFile();

            _Implementation.Update();

            Assert.IsFalse(_DownloadedCompressedFiles.ContainsKey(DatabaseFileUrl));
            Assert.IsFalse(_MovedFiles.ContainsKey(DatabaseTempFileName));
            _StandingDataManager.VerifyGet(r => r.Lock, Times.Never());
        }
        #endregion

        #region BasicAircraftLookup file
        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_BasicAircraftLookup_File_If_Local_Database_Missing()
        {
            SetupValidStateFileDownload();
            SetupValidLocalStateFile();
            _FileExists[BasicAircraftLookupFileName] = false;

            _Implementation.Update();

            Assert.AreEqual(BasicAircraftLookupTempFileName, _DownloadedCompressedFiles[BasicAircraftLookupFileUrl]);
            Assert.AreEqual(BasicAircraftLookupFileName, _MovedFiles[BasicAircraftLookupTempFileName]);

            _BasicAircraftLookupDatabase.VerifyGet(r => r.Lock, Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_BasicAircraftLookup_File_If_Local_Database_Has_Old_Checksum()
        {
            SetupStateFileResult(_DownloadLines, StateFileUrl,   "A,B,C,D,E,F,G,H,I");
            SetupStateFileResult(_ReadLines, StateFileName,      "A,B,C,D,E,F,G,H,*");
            _FileExists[BasicAircraftLookupFileName] = false;

            _Implementation.Update();

            Assert.AreEqual(BasicAircraftLookupTempFileName, _DownloadedCompressedFiles[BasicAircraftLookupFileUrl]);
            Assert.AreEqual(BasicAircraftLookupFileName, _MovedFiles[BasicAircraftLookupTempFileName]);

            _BasicAircraftLookupDatabase.VerifyGet(r => r.Lock, Times.Once());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Does_Not_Download_BasicAircraftLookup_File_If_Local_Database_Correct()
        {
            SetupValidStateFileDownload();
            SetupValidLocalStateFile();

            _Implementation.Update();

            Assert.IsFalse(_DownloadedCompressedFiles.ContainsKey(BasicAircraftLookupFileUrl));
            Assert.IsFalse(_MovedFiles.ContainsKey(BasicAircraftLookupTempFileName));
            _BasicAircraftLookupDatabase.VerifyGet(r => r.Lock, Times.Never());
        }
        #endregion
        #endregion
    }
}
