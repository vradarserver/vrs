using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using VirtualRadar.Interface;
using VirtualRadar.Library;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class Log_Tests
    {
        private Mocks.Clock _Clock;
        private Mock<IFileSystemProvider> _FileSystem;
        private Mock<IThreadingEnvironmentProvider> _ThreadingEnvironment;
        private ILog _Log;

        [TestInitialize]
        public void TestInitialise()
        {
            _Clock = new Mocks.Clock();
            _FileSystem = MockHelper.CreateMock<IFileSystemProvider>();
            _FileSystem.SetupGet(r => r.LogFolder).Returns(() => @"C:\Folder");
            _ThreadingEnvironment = MockHelper.CreateMock<IThreadingEnvironmentProvider>();

            _Log = new Log(_Clock.Object, _FileSystem.Object, _ThreadingEnvironment.Object);
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
            var fileName = "";
            var lines = Array.Empty<string>();
            _FileSystem
                .Setup(r => r.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Callback((string fn, IEnumerable<string> l) => {
                    fileName = fn;
                    lines = l.ToArray();
                });
            _ThreadingEnvironment.SetupGet(r => r.CurrentThreadId).Returns(() => 99);
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, 789, TimeSpan.Zero);

            _Log.WriteLine("Test");
            _Log.Flush();

            Assert.AreEqual(@"C:\Folder\VirtualRadarLog.txt", fileName);
            Assert.AreEqual(1, lines.Length);
            var line = lines[0];
            Assert.AreEqual("[2001-02-03 04:05:06.789 UTC] [t99] Test", line);
        }

        [TestMethod]
        public async Task FlushAsync_Writes_Correct_Value_File_Content_For_Single_Line()
        {
            var fileName = "";
            var lines = Array.Empty<string>();
            _FileSystem
                .Setup(r => r.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Callback((string fn, IEnumerable<string> l) => {
                    fileName = fn;
                    lines = l.ToArray();
                });
            _ThreadingEnvironment.SetupGet(r => r.CurrentThreadId).Returns(() => 99);
            _Clock.Now = new DateTimeOffset(2001, 2, 3, 4, 5, 6, 789, TimeSpan.Zero);

            _Log.WriteLine("Test");
            await _Log.FlushAsync();

            Assert.AreEqual(@"C:\Folder\VirtualRadarLog.txt", fileName);
            Assert.AreEqual(1, lines.Length);
            var line = lines[0];
            Assert.AreEqual("[2001-02-03 04:05:06.789 UTC] [t99] Test", line);
        }

        [TestMethod]
        [DataRow("[2021-02-03 04:05:06.789 UTC] [t99] Some text")]
        [DataRow("+ Continuation line")]
        [DataRow("+ Old continuation line")]
        public void Log_Entries_From_Previous_Sessions_Are_Not_Lost(string text)
        {
            _FileSystem
                .Setup(r => r.FileExists(It.IsAny<string>()))
                .Returns((string fn) => true);
            _FileSystem
                .Setup(r => r.ReadAllLines(It.IsAny<string>()))
                .Returns((string fn) => new string[] { text });

            var messages = _Log.GetContent();

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(text, messages[0].Text);
        }
    }
}
