using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Types;
using VirtualRadar.Library;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class Log_Tests
    {
        private MockOptions<EnvironmentOptions> _EnvironmentOptions;
        private MockClock _Clock;
        private MockFileSystem _FileSystem;
        private Mock<IThreadingEnvironmentProvider> _ThreadingEnvironment;
        private ILog _Log;

        [TestInitialize]
        public void TestInitialise()
        {
            _EnvironmentOptions = new();
            _EnvironmentOptions.Value.WorkingFolder = @"C:\Folder";
            _Clock = new();
            _FileSystem = new();
            _ThreadingEnvironment = MockHelper.CreateMock<IThreadingEnvironmentProvider>();

            _Log = new Log(_EnvironmentOptions, _Clock.Object, _FileSystem, _ThreadingEnvironment.Object);
        }

        [TestMethod]
        public void WriteLine_Adds_Log_Message()
        {
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, TimeSpan.Zero);
            _Log.WriteLine("Test");

            var messages = _Log.GetContent();

            Assert.AreEqual(1, messages.Count);
            var message = messages[0];
            Assert.AreEqual("Test", message.Text);
            Assert.AreEqual(_Clock.Object.UtcNow, message.FirstLoggedAtUtc);
            Assert.AreEqual(_Clock.Object.UtcNow, message.LastLoggedAtUtc);
            Assert.AreEqual(1, message.CountInstances);
        }

        [TestMethod]
        public void WriteLine_Multiple_Instances_Rolled_Together()
        {
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, TimeSpan.Zero);
            _Log.WriteLine("Test");
            _Clock.Now = _Clock.Now.AddSeconds(10);
            _Log.WriteLine("Test");


            var messages = _Log.GetContent();

            Assert.AreEqual(1, messages.Count);
            var message = messages[0];
            Assert.AreEqual("Test", message.Text);
            Assert.AreEqual(_Clock.Object.UtcNow.AddSeconds(-10), message.FirstLoggedAtUtc);
            Assert.AreEqual(_Clock.Object.UtcNow, message.LastLoggedAtUtc);
            Assert.AreEqual(2, message.CountInstances);
        }

        [TestMethod]
        public async Task WriteLineAsync_Adds_Log_Message()
        {
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, TimeSpan.Zero);
            await _Log.WriteLineAsync("Test");

            var lines = _Log.GetContent();

            Assert.AreEqual(1, lines.Count);
            var line = lines[0];
            Assert.AreEqual("Test", line.Text);
            Assert.AreEqual(_Clock.Object.UtcNow, line.FirstLoggedAtUtc);
            Assert.AreEqual(_Clock.Object.UtcNow, line.LastLoggedAtUtc);
            Assert.AreEqual(1, line.CountInstances);
        }

        [TestMethod]
        public void Flush_Writes_Correct_Value_File_Content_For_Single_Line()
        {
            _ThreadingEnvironment.SetupGet(r => r.CurrentThreadId).Returns(() => 99);
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, 789, TimeSpan.Zero);

            _Log.WriteLine("Test");
            _Log.Flush();

            var lines = _FileSystem
                .GetFileContentAsString(@"C:\Folder\VirtualRadarLog.txt")
                .SplitIntoLines();

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("[2001-02-03 04:05:06.789 UTC] [t99] Test", lines[0]);
        }

        [TestMethod]
        public async Task FlushAsync_Writes_Correct_Value_File_Content_For_Single_Line()
        {
            _ThreadingEnvironment.SetupGet(r => r.CurrentThreadId).Returns(() => 99);
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, 789, TimeSpan.Zero);

            _Log.WriteLine("Test");
            await _Log.FlushAsync();

            var lines = _FileSystem
                .GetFileContentAsString(@"C:\Folder\VirtualRadarLog.txt")
                .SplitIntoLines();

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("[2001-02-03 04:05:06.789 UTC] [t99] Test", lines[0]);
        }

        [TestMethod]
        [DataRow("[2021-02-03 04:05:06.789 UTC] [t99] Some text")]
        [DataRow("+ Continuation line")]
        [DataRow("+ Old continuation line")]
        public void Log_Entries_From_Previous_Sessions_Are_Not_Lost(string text)
        {
            _FileSystem.AddFileContent(@"C:\Folder\VirtualRadarLog.txt", text);

            var messages = _Log.GetContent();

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(text, messages[0].Text);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        public void Blank_Lines_In_Log_Are_Ignored(string text)
        {
            _FileSystem.AddFileContent(@"C:\Folder\VirtualRadarLog.txt", text);

            var messages = _Log.GetContent();

            Assert.AreEqual(0, messages.Count);
        }
    }
}
