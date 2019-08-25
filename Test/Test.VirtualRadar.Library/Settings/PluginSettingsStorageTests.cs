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
using VirtualRadar.Interface.Settings;
using Moq;
using InterfaceFactory;
using Test.Framework;
using System.IO;

namespace Test.VirtualRadar.Library.Settings
{
    [TestClass]
    public class PluginSettingsStorageTests
    {
        #region Properties, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const string PluginConfigFileName = "PLUGINSCONFIGURATION.TXT";
        private string _ExpectedFileName { get { return Path.Combine(_ConfigurationStorage.Object.Folder, PluginConfigFileName); } }

        private IClassFactory _ClassFactorySnapshot;
        private IPluginSettingsStorage _Storage;
        private Mock<IPluginSettingsStorageProvider> _Provider;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private EventRecorder<EventArgs> _ConfigurationChangedEvent;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(c => c.Folder).Returns("configFolder");

            _Storage = Factory.Resolve<IPluginSettingsStorage>();
            _Provider = new Mock<IPluginSettingsStorageProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(true);
            _Storage.Provider = _Provider.Object;

            _ConfigurationChangedEvent = new EventRecorder<EventArgs>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void PluginSettingsStorage_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var storage = Factory.Resolve<IPluginSettingsStorage>();

            Assert.IsNotNull(storage.Provider);
            TestUtilities.TestProperty(storage, "Provider", storage.Provider, _Provider.Object);
        }

        [TestMethod]
        public void PluginSettingsStorage_Singleton_Returns_Same_Reference_For_All_Instances()
        {
            var obj1 = Factory.Resolve<IPluginSettingsStorage>();
            var obj2 = Factory.Resolve<IPluginSettingsStorage>();

            Assert.AreNotSame(obj1, obj2);
            Assert.IsNotNull(obj1.Singleton);
            Assert.AreSame(obj1.Singleton, obj2.Singleton);
        }
        #endregion

        #region Load
        [TestMethod]
        public void PluginSettingsStorage_Load_Returns_Default_Object_If_File_Not_Present()
        {
            _Provider.Setup(p => p.FileReadAllLines(It.Is<string>(n => n.Equals(_ExpectedFileName, StringComparison.OrdinalIgnoreCase)))).Returns(new string[] { "Key=Value" });
            _Provider.Setup(p => p.FileExists(It.Is<string>(n => n.Equals(_ExpectedFileName, StringComparison.OrdinalIgnoreCase)))).Returns(false);

            var settings = _Storage.Load();

            Assert.AreEqual(0, settings.Values.Count);
            _Provider.Verify(p => p.FileExists(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PluginSettingsStorageLoad$")]
        public void PluginSettingsStorage_Load_Returns_Parsed_File_Content_If_File_Is_Present()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var lines = new List<string>();
            foreach(var line in worksheet.EString("Lines").Split('\n')) {
                lines.Add(line);
            }
            _Provider.Setup(p => p.FileReadAllLines(It.Is<string>(n => n.Equals(_ExpectedFileName, StringComparison.OrdinalIgnoreCase)))).Returns(lines.ToArray());

            var settings = _Storage.Load();

            Assert.AreEqual(worksheet.Int("Count"), settings.Values.Count);
            for(int i = 1;i <= settings.Values.Count;++i) {
                Assert.AreEqual(worksheet.EString(String.Format("Key{0}", i)), settings.Values.Keys[i-1]);
                Assert.AreEqual(worksheet.EString(String.Format("Value{0}", i)), settings.Values[i-1]);
            }
        }

        [TestMethod]
        public void PluginSettingsStorage_Load_Parses_Escaped_Characters_Correctly()
        {
            // These can be partially covered by the spreadsheet test but that's hampered by the inability to express some literal characters,
            // so they're also covered here
            var lines = new List<string>();
            _Provider.Setup(p => p.FileReadAllLines(It.IsAny<string>())).Returns((string fileName) => { return lines.ToArray(); });

            TestLoadUnescapes(lines, @"%0a", "\n");
            TestLoadUnescapes(lines, @"%0d", "\r");
            TestLoadUnescapes(lines, @"%25", "%");
            TestLoadUnescapes(lines, @"%0a%250a%25%0a", "\n%0a%\n");
        }

        private void TestLoadUnescapes(List<string> lines, string lineValue, string parsedValue)
        {
            lines.Clear();
            var line = String.Format("key={0}", lineValue);
            lines.Add(line);

            var settings = _Storage.Load();

            Assert.AreEqual(parsedValue, settings.Values["key"], line);
        }
        #endregion

        #region Save
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginSettingsStorage_Save_Throws_If_PluginSettings_Is_Null()
        {
            _Storage.Save(null);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PluginSettingsStorageSave$")]
        public void PluginSettingsStorage_Save_Translates_Key_Values_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var settings = new PluginSettings();
            for(int i = 1;i <= worksheet.Int("Count");++i) {
                var key = worksheet.EString(String.Format("Key{0}", i));
                var value = worksheet.EString(String.Format("Value{0}", i));
                if(value != null) value = value.Replace(@"\n", "\n").Replace(@"\r", "\r");
                settings.Values.Add(key, value);
            }

            _Provider.Setup(p => p.FileWriteAllLines(It.IsAny<string>(), It.IsAny<string[]>())).Callback((string fileName, string[] lines) => {
                Assert.AreEqual(worksheet.Int("LineCount"), lines.Length);
                for(int i = 1;i <= worksheet.Int("LineCount");++i) {
                    Assert.AreEqual(worksheet.EString(String.Format("Line{0}", i)), lines[i - 1]);
                }
            });

            _Storage.Save(settings);
            _Provider.Verify(p => p.FileWriteAllLines(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once());
        }

        [TestMethod]
        public void PluginSettingsStorage_Save_Raises_ConfigurationChanged()
        {
            _Storage.ConfigurationChanged += _ConfigurationChangedEvent.Handler;

            _Storage.Save(new PluginSettings());

            Assert.AreEqual(1, _ConfigurationChangedEvent.CallCount);
            Assert.AreSame(_Storage, _ConfigurationChangedEvent.Sender);
            Assert.IsNotNull(_ConfigurationChangedEvent.Args);
        }
        #endregion
    }
}
