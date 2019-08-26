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
using InterfaceFactory;
using VirtualRadar.Library;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using System.IO;
using Moq;
using Test.Framework;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class LogTests
    {
        #region TestContext, fields, test initialise & cleanup
        public TestContext TestContext { get; set; }

        private string _FileName = "VirtualRadarLog.txt";
        private string _FullPath;
        private IClassFactory _ClassFactorySnapshot;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private ILog _Log;
        private Mock<ILogProvider> _Provider;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Object.Folder = @"ztt:\ump";
            _FullPath = Path.Combine(_ConfigurationStorage.Object.Folder, _FileName);

            _Clock = new ClockMock() { UtcNowValue = new DateTime(2001, 2, 3, 4, 5, 6, 789) };
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _Provider = new Mock<ILogProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(true);
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(true);

            _Log = Factory.ResolveNewInstance<ILog>();
            _Log.Provider = _Provider.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructors and properties
        [TestMethod]
        public void Log_FileName_Contains_Correct_String()
        {
            Assert.AreEqual(_FullPath, _Log.FileName, true);
        }

        [TestMethod]
        public void Log_FileName_Read_Does_Not_Create_Folder()
        {
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(false);
            Assert.AreEqual(_FullPath, _Log.FileName, true);
            _Provider.Verify(p => p.CreateFolder(It.IsAny<string>()), Times.Never());
        }
        #endregion

        #region WriteLine
        [TestMethod]
        public void Log_WriteLine_Text_Creates_Folder_If_Required()
        {
            _Provider.Setup(p => p.FolderExists(@"ztt:\ump")).Returns(false);

            _Log.WriteLine("Hello");

            _Provider.Verify(p => p.CreateFolder(@"ztt:\ump"), Times.Once());
            _Provider.Verify(p => p.CreateFolder(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void Log_WriteLine_Format_Creates_Folder_If_Required()
        {
            _Provider.Setup(p => p.FolderExists(@"ztt:\ump")).Returns(false);

            _Log.WriteLine("{0}", "Hello");

            _Provider.Verify(p => p.CreateFolder(@"ztt:\ump"), Times.Once());
            _Provider.Verify(p => p.CreateFolder(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void Log_WriteLine_Text_Writes_Message_To_Log_File()
        {
            _Provider.Setup(p => p.CurrentThreadId).Returns(17);
            _Log.WriteLine("Hello");

            _Provider.Verify(p => p.AppendAllText(_FullPath, "[2001-02-03 04:05:06.789 UTC] [t17] Hello\r\n"), Times.Once());
            _Provider.Verify(p => p.AppendAllText(_FullPath, It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void Log_WriteLine_Format_Writes_Message_To_Log_File()
        {
            _Provider.Setup(p => p.CurrentThreadId).Returns(17);
            _Log.WriteLine("{0}", "Hello");

            _Provider.Verify(p => p.AppendAllText(_FullPath, "[2001-02-03 04:05:06.789 UTC] [t17] Hello\r\n"), Times.Once());
            _Provider.Verify(p => p.AppendAllText(_FullPath, It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void Log_WriteLine_Message_Ignores_Null_Message()
        {
            _Log.WriteLine(null);

            _Provider.Verify(p => p.AppendAllText(_FullPath, It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void Log_WriteLine_Format_Writes_Formatted_Lines()
        {
            _Log.WriteLine("This {0} test", "is a");

            _Provider.Verify(p => p.AppendAllText(_FullPath, "[2001-02-03 04:05:06.789 UTC] [t0] This is a test\r\n"), Times.Once());
            _Provider.Verify(p => p.AppendAllText(_FullPath, It.IsAny<string>()), Times.Once());
        }
        #endregion

        #region Truncate
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Log_Truncate_Throws_Exception_When_Passed_Negative_Number()
        {
            _Log.WriteLine("X");
            _Log.Truncate(-1);
        }

        [TestMethod]
        public void Log_Truncate_Shortens_Log_File()
        {
            _Log.Truncate(2);
            _Provider.Verify(p => p.TruncateTo(_FullPath, 2048), Times.Once());
            _Provider.Verify(p => p.TruncateTo(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void Log_Truncate_Does_Nothing_If_Folder_Missing()
        {
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(false);
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(false);
            _Log.Truncate(2);

            _Provider.Verify(p => p.CreateFolder(It.IsAny<string>()), Times.Never());
            _Provider.Verify(p => p.TruncateTo(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void Log_Truncate_Does_Nothing_If_File_Missing()
        {
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(false);
            _Log.Truncate(2);

            _Provider.Verify(p => p.TruncateTo(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        }
        #endregion
    }
}
