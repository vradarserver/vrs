// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.IO.Compression;
using System.Text;
using Moq;
using Test.Framework;
using VirtualRadar.Database.SQLite.StandingData;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.Types;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class StandingDataUpdater_Tests
    {
        private const string ConfigurationFolder = @"c:\config\";
        private const string DatabaseFileName =     ConfigurationFolder + "StandingData.sqb";
        private const string DatabaseTempFileName = ConfigurationFolder + "StandingData.sqb.tmp";
        private const string StateFileName =        ConfigurationFolder + "FlightNumberCoverage.csv";
        private const string StateTempFileName =    ConfigurationFolder + "FlightNumberCoverage.csv.tmp";
        private const string DatabaseFileUrl =      "http://www.virtualradarserver.co.uk/Files/StandingData.sqb.gz";
        private const string StateFileUrl =         "http://www.virtualradarserver.co.uk/Files/FlightNumberCoverage.csv";
        private readonly string[] AllFileNames = {
            DatabaseFileName,
            DatabaseTempFileName,
            StateFileName,
            StateTempFileName,
        };
        private readonly string[] AllUrls = {
            DatabaseFileUrl,
            StateFileUrl,
        };

        private MockOptions<EnvironmentOptions> _EnvironmentOptions;
        private MockFileSystem _FileSystem;
        private MockHttpClient _HttpClient;
        private MockLog _Log;
        private MockWebAddressManager _WebAddresses;

        private IStandingDataUpdater _Implementation;
        private Mock<IStandingDataManager> _StandingDataManager;

        [TestInitialize]
        public void TestInitialise()
        {
#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)

            _EnvironmentOptions = new(new EnvironmentOptions() {
                WorkingFolder = ConfigurationFolder,
            });
            _FileSystem = new();
            _HttpClient = new();
            _Log = new();
            _WebAddresses = new();

            _StandingDataManager = MockHelper.CreateMock<IStandingDataManager>();
            _StandingDataManager
                .Setup(r => r.Lock(It.IsAny<Action<IStandingDataManager>>()))
                .Callback((Action<IStandingDataManager> action) => action.Invoke(_StandingDataManager.Object));

            _Implementation = new StandingDataUpdater(
                _EnvironmentOptions,
                _FileSystem,
                _HttpClient,
                _WebAddresses,
                _StandingDataManager.Object
            );
        }

        private void SetupValidStateFileDownload(string contentLine = "1,2,3,4,5,6,7,8,9", string[] extraLines = null)
        {
            _HttpClient.AddUrlContent(StateFileUrl, SetupStateFileContent(
                headerInUpperCase: true,
                contentLine,
                extraLines,
                "\r\n"
            ));
        }

        private void SetupValidLocalStateFile(string contentLine = "1,2,3,4,5,6,7,8,9", string[] extraLines = null, string newLineSeparator = "\r\n")
        {
            _FileSystem.AddFileContent(StateFileName, SetupStateFileContent(
                headerInUpperCase: false,
                contentLine,
                extraLines,
                newLineSeparator
            ));
        }

        private string SetupStateFileContent(bool headerInUpperCase, string contentLine, string[] extraLines, string newLineSeparator)
        {
            var fileLines = new List<string>(new string[] {
                headerInUpperCase
                    ? "A,B,C,D,E,F,G,H,I"
                    : "a,b,c,d,e,f,g,h,i",
                contentLine,
            });
            if(extraLines?.Length > 0) {
                fileLines.AddRange(extraLines);
            }
            return String.Join(newLineSeparator, fileLines);
        }

        private void SetupValidDatabaseDownload(byte content = 0)
        {
            using(var inputStream = new MemoryStream(new byte[] { content, })) {
                using(var outputStream = new MemoryStream()) {
                    using(var gzipStream = new GZipStream(outputStream, CompressionMode.Compress)) {
                        inputStream.CopyTo(gzipStream);
                        gzipStream.Flush();
                        outputStream.Flush();

                        _HttpClient.AddUrlContent(DatabaseFileUrl, outputStream.ToArray());
                    }
                }
            }
        }

        private void SetupValidDatabaseFile(byte content = 0)
        {
            _FileSystem.AddFileContent(DatabaseFileName, new byte[] { content });
        }

        [TestMethod]
        public void DataIsOld_Returns_True_If_State_File_Is_Missing()
        {
            Assert.AreEqual(true, _Implementation.DataIsOld());
        }

        [TestMethod]
        public void DataIsOld_Downloads_Current_State_File()
        {
            SetupValidLocalStateFile();
            SetupValidStateFileDownload();

            _Implementation.DataIsOld();
            Assert.AreEqual(1, _HttpClient.DownloadedUrls.Count);
            Assert.AreEqual(StateFileUrl, _HttpClient.DownloadedUrls[0]);
        }

        [TestMethod]
        public void DataIsOld_Returns_False_If_Remote_And_Local_State_Files_Are_Same()
        {
            SetupValidLocalStateFile();
            SetupValidStateFileDownload();
            Assert.AreEqual(false, _Implementation.DataIsOld());
        }

        [TestMethod]
        public void DataIsOld_Returns_True_If_Remote_And_Local_State_Files_Differ()
        {
            SetupValidLocalStateFile("1");
            SetupValidStateFileDownload("2");
            Assert.AreEqual(true, _Implementation.DataIsOld());
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Always_Downloads_State_File()
        {
            SetupValidLocalStateFile();
            SetupValidStateFileDownload();
            SetupValidDatabaseDownload();
            SetupValidDatabaseFile();

            _Implementation.Update();

            Assert.AreEqual(1, _HttpClient.DownloadedUrls.Count(r => r == StateFileUrl));
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Writes_Local_State_File_If_Missing()
        {
            SetupValidStateFileDownload();
            SetupValidDatabaseDownload();
            SetupValidDatabaseFile();

            _Implementation.Update();

            var downloadedLines = Encoding.UTF8.GetString(_HttpClient.CaseSensitiveUrlContent[StateFileUrl]).SplitIntoLines();
            var savedLines =      Encoding.UTF8.GetString(_FileSystem.CaseSensitiveFileContent[StateFileName]).SplitIntoLines();
            Assert.IsTrue(downloadedLines.SequenceEqual(savedLines));
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Does_Not_Write_Local_State_File_If_Nothing_Has_Changed_On_Data_Line()
        {
            SetupValidLocalStateFile(extraLines: new string[] { "should not be overwritten" });
            SetupValidStateFileDownload(extraLines: new string[] { "extra text that won't be counted" });
            SetupValidDatabaseDownload();
            SetupValidDatabaseFile();

            _Implementation.Update();

            var savedStateFile = Encoding.UTF8.GetString(_FileSystem.CaseSensitiveFileContent[StateFileName]);
            Assert.IsTrue(savedStateFile.Contains("should not be overwritten"));
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_Database_File_If_Local_Database_Missing()
        {
            SetupValidLocalStateFile(extraLines: new string[] { "will be overwritten" });
            SetupValidStateFileDownload(extraLines: new string[] { "new content" });
            SetupValidDatabaseDownload();

            _Implementation.Update();

            var savedStateFile = Encoding.UTF8.GetString(_FileSystem.CaseSensitiveFileContent[StateFileName]);
            Assert.IsTrue(savedStateFile.Contains("new content"));
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Downloads_Database_File_If_Local_Database_Has_Old_Checksum()
        {
            SetupValidLocalStateFile(       contentLine: "A,B,C,D,E,F,G,H");
            SetupValidStateFileDownload(    contentLine: "A,B,C,D,E,F,G,*");
            SetupValidDatabaseDownload(content: 1);
            SetupValidDatabaseFile(content: 0);

            _Implementation.Update();

            Assert.IsTrue(_FileSystem.CaseSensitiveFileContent[DatabaseFileName].SequenceEqual(new byte[] { 1 } ));
        }

        [TestMethod]
        public void StandingDataUpdater_Update_Does_Not_Download_Database_File_If_Local_Database_Correct()
        {
            SetupValidStateFileDownload();
            SetupValidLocalStateFile();
            SetupValidDatabaseDownload(content: 1);
            SetupValidDatabaseFile(content: 0);

            _Implementation.Update();

            Assert.IsTrue(_FileSystem.CaseSensitiveFileContent[DatabaseFileName].SequenceEqual(new byte[] { 0 } ));
        }
    }
}
