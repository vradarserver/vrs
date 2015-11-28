// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using Moq;
using InterfaceFactory;
using Test.Framework;
using System.IO;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class DirectoryCacheTests
    {
        #region TestFileInfo
        class TestFileInfo : IDirectoryCacheProviderFileInfo
        {
            public string Name { get; set; }
            public DateTime LastWriteTimeUtc { get; set; }

            public TestFileInfo(string name) : this(name, _StandardDate)
            {
            }

            public TestFileInfo(string name, DateTime lastModified) : base()
            {
                Name = name;
                LastWriteTimeUtc = lastModified;
            }
        }
        #endregion

        #region TestContext, Fields, TestIntialise, TestCleanup
        public TestContext TestContext { get; set; }

        private static readonly DateTime _StandardDate = new DateTime(2001, 2, 3, 4, 5, 6, 789);
        private IClassFactory _ClassFactorySnapshot;
        private IDirectoryCache _DirectoryCache;
        private ClockMock _Clock;
        private Mock<IDirectoryCacheProvider> _Provider;
        private Mock<IBackgroundWorker> _BackgroundWorker;
        private Mock<IHeartbeatService> _HeartbeatService;
        private const int SecondsBetweenRefreshes = 60;
        private DateTime _LastModifiedUtc;
        private Dictionary<string, List<TestFileInfo>> _Files;
        private List<string> _Folders;

        private EventRecorder<EventArgs> _CacheChangedEvent;
        private Mock<ILog> _Log;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _Clock = new ClockMock() { UtcNowValue = _StandardDate };
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            CreateBackgroundWorkerMock();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _Log.Setup(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>(),  It.IsAny<string>())).Callback(() => { throw new InvalidOperationException("Log was unexpectedly written"); });

            _CacheChangedEvent = new EventRecorder<EventArgs>();

            _LastModifiedUtc = new DateTime(2009, 8, 7, 6, 5, 4, 321);
            _Files = new Dictionary<string,List<TestFileInfo>>();
            _Folders = new List<string>();

            _Provider = new Mock<IDirectoryCacheProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns((string folder) => {
                if(folder != null && folder.EndsWith(@"\")) folder = folder.Substring(0, folder.Length - 1);
                return _Folders.Contains(folder.ToLower());
            });
            _Provider.Setup(p => p.GetSubFoldersInFolder(It.IsAny<string>())).Returns((string folder) => {
                if(folder != null && !folder.EndsWith(@"\")) folder += '\\';
                var subFolders = _Folders.Where(r => 
                        r.StartsWith(folder, StringComparison.OrdinalIgnoreCase) &&
                        r.IndexOf('\\', folder.Length) == -1)
                    .Select(r => Path.GetFileName(r))
                    .ToList();
                return subFolders;
            });
            _Provider.Setup(p => p.GetFilesInFolder(It.IsAny<string>())).Returns((string folder) => {
                List<TestFileInfo> files;
                if(folder != null && folder.EndsWith(@"\")) folder = folder.Substring(0, folder.Length - 1);
                _Files.TryGetValue(folder.ToLower(), out files);
                return (IEnumerable<TestFileInfo>)files ?? new TestFileInfo[0];
            });
            _Provider.Setup(p => p.GetFileInfo(It.IsAny<string>())).Returns((string fullPath) => {
                TestFileInfo result = null;
                var folder = (Path.GetDirectoryName(fullPath) ?? "").ToLower();
                var fileName = Path.GetFileName(fullPath);
                List<TestFileInfo> files;
                if(_Files.TryGetValue(folder, out files)) result = files.FirstOrDefault(r => r.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                return result;
            });

            _DirectoryCache = Factory.Singleton.Resolve<IDirectoryCache>();
            _DirectoryCache.Provider = _Provider.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _DirectoryCache.Dispose();
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        private void CreateBackgroundWorkerMock()
        {
            _BackgroundWorker = TestUtilities.CreateMockImplementation<IBackgroundWorker>();
            _BackgroundWorker.Setup(w => w.StartWork(It.IsAny<object>())).Callback((object obj) => {
                _BackgroundWorker.Setup(w => w.State).Returns(obj);
                _BackgroundWorker.Raise(w => w.DoWork += null, EventArgs.Empty);
            });
        }

        private void AddToFolders(string folder)
        {
            if(!String.IsNullOrEmpty(folder)) {
                if(folder[folder.Length - 1] == '\\') folder = folder.Substring(0, folder.Length - 1);
                folder = folder.ToLower();

                var previousFolder = folder;
                do {
                    if(!_Folders.Contains(folder)) _Folders.Add(folder);
                    try {
                        previousFolder = folder;
                        folder = Path.GetDirectoryName(folder);
                    } catch {
                        folder = null;
                    }
                } while(folder != null && folder != previousFolder);
            }
        }

        private TestFileInfo AddToFiles(string directory, TestFileInfo fileInfo)
        {
            if(!String.IsNullOrEmpty(directory)) {
                directory = directory.ToLower();

                AddToFolders(directory);

                List<TestFileInfo> files;
                if(!_Files.TryGetValue(directory, out files)) {
                    files = new List<TestFileInfo>();
                    _Files.Add(directory, files);
                }
                files.Add(fileInfo);
            }

            return fileInfo;
        }

        private TestFileInfo AddToFiles(string fullPath)
        {
            return AddToFiles(fullPath, _StandardDate);
        }

        private TestFileInfo AddToFiles(string fullPath, DateTime lastModified)
        {
            try {
                var directory = Path.GetDirectoryName(fullPath);
                var fileName = Path.GetFileName(fullPath);

                return AddToFiles(directory, new TestFileInfo(fileName, lastModified));
            } catch {
                return null;
            }
        }

        private void RemoveFromFiles(string fullPath)
        {
            try {
                var directory = Path.GetDirectoryName(fullPath).ToLower();
                var fileName = Path.GetFileName(fullPath);
                List<TestFileInfo> files;
                if(_Files.TryGetValue(directory, out files)) {
                    var testFileInfo = files.FirstOrDefault(r => r.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                    if(testFileInfo != null) files.Remove(testFileInfo);
                    if(files.Count == 0) _Files.Remove(directory);
                }
            } catch {}
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void DirectoryCache_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var cache = Factory.Singleton.Resolve<IDirectoryCache>();
            Assert.IsNotNull(cache.Provider);
            TestUtilities.TestProperty(cache, r => r.Provider, cache.Provider, _Provider.Object);
            TestUtilities.TestProperty(cache, r => r.Folder, null, "Abc");
            TestUtilities.TestProperty(cache, r => r.CacheSubFolders, false);
        }

        [TestMethod]
        public void DirectoryCache_Folder_Change_Triggers_Background_File_Load()
        {
            AddToFiles(@"xyz\a");

            _DirectoryCache.Folder = "XyZ";

            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
            _Provider.Verify(p => p.GetFilesInFolder("XyZ"), Times.Once());
            _Provider.Verify(p => p.GetFilesInFolder(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_CacheSubFolders_Change_Triggers_Background_File_Load()
        {
            _DirectoryCache.Folder = "x";
            _DirectoryCache.CacheSubFolders = true;

            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        public void DirectoryCache_CacheSubFolders_No_Change_Does_Not_Trigger_Background_File_Load()
        {
            _DirectoryCache.CacheSubFolders = false;

            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        public void DirectoryCache_Folder_Change_Does_Not_Load_Files_If_Folder_Does_Not_Exist()
        {
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(false);
            _DirectoryCache.Folder = "XyZ";

            Assert.AreEqual("XyZ", _DirectoryCache.Folder);
            _Provider.Verify(p => p.GetFilesInFolder("XyZ"), Times.Never());
        }

        [TestMethod]
        public void DirectoryCache_CacheSubFolders_Change_Does_Not_Trigger_Background_File_Load_If_Folder_Not_Set()
        {
            _DirectoryCache.CacheSubFolders = true;

            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        public void DirectoryCache_Folder_Set_To_Null_Does_Not_Search_For_Files()
        {
            AddToFiles(@"xyz\a");
            _DirectoryCache.Folder = "xyz";
            _DirectoryCache.Folder = null;

            Assert.AreEqual(null, _DirectoryCache.Folder);
            _Provider.Verify(p => p.GetFilesInFolder("xyz"), Times.Once());
            _Provider.Verify(p => p.GetFilesInFolder(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_Folder_Set_To_Null_Does_Check_For_Existence_Of_Folder()
        {
            _DirectoryCache.Folder = "xyz";
            _DirectoryCache.Folder = null;

            _Provider.Verify(p => p.FolderExists("xyz"), Times.Once());
            _Provider.Verify(p => p.FolderExists(null), Times.Never());
        }

        [TestMethod]
        public void DirectoryCache_Folder_Set_To_Self_Does_Not_Search_For_Files_Again()
        {
            AddToFiles(@"xyz\a");
            _DirectoryCache.Folder = "xyz";
            _DirectoryCache.Folder = "xyz";

            _Provider.Verify(p => p.GetFilesInFolder("xyz"), Times.Once());
        }
        #endregion

        #region BeginRefresh
        [TestMethod]
        public void DirectoryCache_BeginRefresh_Loads_Files_On_Background_Thread()
        {
            _DirectoryCache.Folder = "XyZ";
            CreateBackgroundWorkerMock();
            _DirectoryCache.BeginRefresh();

            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_BeginRefresh_Checks_That_Folder_Exists()
        {
            _DirectoryCache.Folder = "Xyz";
            CreateBackgroundWorkerMock();
            _DirectoryCache.BeginRefresh();

            _Provider.Verify(p => p.FolderExists("Xyz"), Times.Exactly(2));
        }

        [TestMethod]
        public void DirectoryCache_BeginRefresh_Does_Not_Check_For_Folder_Existence_If_Null()
        {
            _DirectoryCache.BeginRefresh();

            _Provider.Verify(p => p.FolderExists(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void DirectoryCache_BeginRefresh_Loads_Files_If_Folder_Exists()
        {
            AddToFiles(@"xyz\a");
            _DirectoryCache.Folder = "Xyz";
            CreateBackgroundWorkerMock();
            _DirectoryCache.BeginRefresh();

            _Provider.Verify(p => p.GetFilesInFolder("Xyz"), Times.Exactly(2));
        }

        [TestMethod]
        public void DirectoryCache_BeginRefresh_Does_Not_Load_Files_If_Folder_Does_Not_Exist()
        {
            _Provider.Setup(p => p.FolderExists("Xyz")).Returns(false);

            _DirectoryCache.Folder = "Xyz";
            CreateBackgroundWorkerMock();
            _DirectoryCache.BeginRefresh();

            _Provider.Verify(p => p.GetFilesInFolder("Xyz"), Times.Never());
        }
        #endregion

        #region Heartbeat Tick
        [TestMethod]
        public void DirectoryCache_Heartbeat_Triggers_Refresh()
        {
            _DirectoryCache.Folder = "Xyz";

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _Provider.Verify(p => p.FolderExists("Xyz"), Times.AtLeast(2));
        }

        [TestMethod]
        public void DirectoryCache_Heartbeat_Does_Not_Trigger_Refresh_If_Timeout_Has_Not_Elapsed()
        {
            _DirectoryCache.Folder = "Xyz";

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes).AddMilliseconds(-1);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _Provider.Verify(p => p.FolderExists("Xyz"), Times.Exactly(1));
        }
        #endregion

        #region SetConfiguration
        [TestMethod]
        public void DirectoryCache_SetConfiguration_Change_Folder_Triggers_Single_Refresh()
        {
            var result = _DirectoryCache.SetConfiguration("x", _DirectoryCache.CacheSubFolders);

            Assert.AreEqual(true, result);
            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_SetConfiguration_Change_CacheSubFolders_Triggers_Single_Refresh()
        {
            var result = _DirectoryCache.SetConfiguration(_DirectoryCache.Folder, !_DirectoryCache.CacheSubFolders);

            Assert.AreEqual(true, result);
            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_SetConfiguration_Change_Both_Triggers_Single_Refresh()
        {
            var result = _DirectoryCache.SetConfiguration("x", !_DirectoryCache.CacheSubFolders);

            Assert.AreEqual(true, result);
            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_SetConfiguration_Change_Neither_Triggers_No_Refresh()
        {
            _DirectoryCache.SetConfiguration("x", !_DirectoryCache.CacheSubFolders);
            var result = _DirectoryCache.SetConfiguration(_DirectoryCache.Folder, _DirectoryCache.CacheSubFolders);

            Assert.AreEqual(false, result);
            _BackgroundWorker.Verify(w => w.StartWork(It.IsAny<object>()), Times.Once());
        }
        #endregion

        #region GetFullPath, FileExists, Add, Remove spreadsheet tests
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DirectoryCacheGetFullPath$")]
        public void DirectoryCache_GetFullPath_Returns_Correct_Value()
        {
            SpreadsheetTests(null);
        }

        private void SpreadsheetTests(Action<ExcelWorksheetData> afterSetFolder)
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            bool checkCacheChanged = worksheet.ColumnExists("CacheChanged");

            for(int i = 1;i <= 2;++i) {
                string nameColumn = String.Format("File{0}", i);
                string lastModifiedColumn = String.Format("Time{0}", i);

                if(!worksheet.ColumnExists(nameColumn)) continue;

                var name = worksheet.String(nameColumn);
                var time = worksheet.ColumnExists(lastModifiedColumn) ? worksheet.DateTime(lastModifiedColumn) : new DateTime(2999, 12, 31);
                if(name != null) AddToFiles(name, time);
            }

            var folder = worksheet.String("Folder");
            if(folder != null) {
                AddToFolders(folder);
                _DirectoryCache.Folder = folder;
            }

            if(worksheet.ColumnExists("CacheSubFolders")) {
                _DirectoryCache.CacheSubFolders = worksheet.Bool("CacheSubFolders");
            }

            if(worksheet.ColumnExists("LastModified")) {
                if(worksheet.String("LastModified") != null) AddToFiles(worksheet.String("FileName"), worksheet.DateTime("LastModified"));
            }

            if(checkCacheChanged) _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            if(afterSetFolder != null) afterSetFolder(worksheet);

            Assert.AreEqual(folder, _DirectoryCache.Folder);
            var result = _DirectoryCache.GetFullPath(worksheet.EString("SearchFor"));
            Assert.AreEqual(worksheet.EString("Result"), result, true);

            if(checkCacheChanged) Assert.AreEqual(worksheet.Bool("CacheChanged") ? 1 : 0, _CacheChangedEvent.CallCount);
        }
        #endregion

        #region CacheChanged
        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_When_Folder_Set()
        {
            AddToFiles(@"x\a");
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _CacheChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(@"x\a", _DirectoryCache.GetFullPath("a")); };

            _DirectoryCache.Folder = "x";

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
            Assert.AreSame(_DirectoryCache, _CacheChangedEvent.Sender);
            Assert.IsNotNull(_CacheChangedEvent.Args);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_When_BeginRefresh_Finished()
        {
            _DirectoryCache.Folder = "x";

            AddToFiles(@"x\a");
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _CacheChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(@"x\a", _DirectoryCache.GetFullPath("a"));  };

            CreateBackgroundWorkerMock();
            _DirectoryCache.BeginRefresh();

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
            Assert.AreSame(_DirectoryCache, _CacheChangedEvent.Sender);
            Assert.IsNotNull(_CacheChangedEvent.Args);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_When_Heartbeat_Tick_Raised()
        {
            _DirectoryCache.Folder = "x";

            AddToFiles(@"x\a");
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _CacheChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(@"x\a", _DirectoryCache.GetFullPath("a")); };

            CreateBackgroundWorkerMock();
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
            Assert.AreSame(_DirectoryCache, _CacheChangedEvent.Sender);
            Assert.IsNotNull(_CacheChangedEvent.Args);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_If_Modified_Date_Changes()
        {
            var testFileInfo = AddToFiles(@"x\a", new DateTime(2001, 2, 3));

            _DirectoryCache.Folder = "x";

            testFileInfo.LastWriteTimeUtc = new DateTime(2008, 7, 6);
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;

            CreateBackgroundWorkerMock();
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_Folder_Does_Not_Change()
        {
            _DirectoryCache.Folder = "x";

            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _DirectoryCache.Folder = "x";

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_CacheSubFolders_Does_Not_Change()
        {
            _DirectoryCache.Folder = "x";
            _DirectoryCache.CacheSubFolders = true;

            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _DirectoryCache.CacheSubFolders = true;

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_Content_Does_Not_Change()
        {
            AddToFiles(@"x\a");
            _DirectoryCache.Folder = "x";

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_Content_Does_Not_Change_But_Order_Does()
        {
            AddToFiles(@"x\a");
            AddToFiles(@"x\b");

            _DirectoryCache.Folder = "x";

            _Files.Clear();
            AddToFiles(@"x\b");
            AddToFiles(@"x\a");

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_Content_Does_Not_Change_But_Case_Does()
        {
            AddToFiles(@"x\a");
            AddToFiles(@"x\b");
            _DirectoryCache.Folder = "x";

            _Files.Clear();
            AddToFiles(@"x\A");
            AddToFiles(@"x\B");

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_If_BeginRefresh_Sees_No_Change()
        {
            AddToFiles(@"x\a");
            _DirectoryCache.Folder = "x";

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _DirectoryCache.BeginRefresh();

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_If_Folder_Changes_But_File_Names_Are_Same()
        {
            AddToFiles(@"x\a");
            AddToFiles(@"y\a");
            _DirectoryCache.Folder = "x";

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _DirectoryCache.Folder = "y";

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Raised_If_Cache_Cleared()
        {
            AddToFiles(@"x\a");
            _DirectoryCache.Folder = "x";

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _DirectoryCache.Folder = null;

            Assert.AreEqual(1, _CacheChangedEvent.CallCount);
        }

        [TestMethod]
        public void DirectoryCache_CacheChanged_Not_Raised_For_Heartbeats_After_Cache_Cleared()
        {
            AddToFiles(@"x\a");
            _DirectoryCache.Folder = "x";
            _DirectoryCache.Folder = null;

            CreateBackgroundWorkerMock();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(SecondsBetweenRefreshes);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _CacheChangedEvent.CallCount);
        }
        #endregion

        #region Exception Handling
        [TestMethod]
        public void DirectoryCache_Logs_Exceptions_Thrown_By_Provider_FolderExists()
        {
            var exception = new Exception();
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Throws(exception);
            _Log.Setup(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => { ; });

            _DirectoryCache.Folder = "x";

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), "x", exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_Logs_Exceptions_Thrown_By_Provider_GetFilesInFolder()
        {
            AddToFiles(@"x\a");

            var exception = new Exception();
            _Provider.Setup(p => p.GetFilesInFolder(It.IsAny<string>())).Throws(exception);
            _Log.Setup(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => { ; });

            _DirectoryCache.Folder = "x";

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), "x", exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void DirectoryCache_Logs_Exceptions_Thrown_By_CacheCleared_Event_Handlers()
        {
            var exception = new Exception();
            _DirectoryCache.CacheChanged += _CacheChangedEvent.Handler;
            _CacheChangedEvent.EventRaised += (s, a) => { throw exception; };
            _Log.Setup(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => { ; });

            AddToFiles(@"x\a");
            _DirectoryCache.Folder = "x";

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), "x", It.IsAny<string>()), Times.Once());
        }
        #endregion
    }
}
