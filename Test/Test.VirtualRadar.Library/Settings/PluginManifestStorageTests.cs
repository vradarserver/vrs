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

namespace Test.VirtualRadar.Library.Settings
{
    [TestClass]
    public class PluginManifestStorageTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IPluginManifestStorage _Storage;
        private Mock<IPluginManifestStorageProvider> _Provider;
        private bool _FileExists;
        private string _FileContent;

        [TestInitialize]
        public void TestInitialise()
        {
            _Provider = new Mock<IPluginManifestStorageProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(() => { return _FileExists; });
            _Provider.Setup(p => p.ReadAllText(It.IsAny<string>())).Returns(() => { return _FileContent; });

            _FileContent = "";
            _FileExists = true;

            _Storage = Factory.Resolve<IPluginManifestStorage>();
            _Storage.Provider = _Provider.Object;
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void PluginManifestStorage_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var storage = Factory.Resolve<IPluginManifestStorage>();
            Assert.IsNotNull(storage.Provider);
            TestUtilities.TestProperty(storage, "Provider", storage.Provider, _Provider.Object);
        }
        #endregion

        #region LoadForPlugin
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginManifestStorage_LoadForPlugin_Throws_If_Passed_Null()
        {
            _Storage.LoadForPlugin(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PluginManifestStorage_LoadForPlugin_Throws_If_Passed_Empty_String()
        {
            _Storage.LoadForPlugin("");
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Throws_If_Not_Passed_A_Dll_FileName()
        {
            var names = new string[]            { @"file", @"file.dil", @"file.dll", @"file.DLL" };
            var expectException = new bool[]    { true,    true,        false,       false };

            _FileExists = false;

            for(var i = 0;i < names.Length;++i) {
                bool threwException = false;
                try {
                    _Storage.LoadForPlugin(names[i]);
                } catch(InvalidOperationException) {
                    threwException = true;
                }

                Assert.AreEqual(expectException[i], threwException, names[i]);
            }
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Returns_Null_If_File_Does_Not_Exist()
        {
            _FileExists = false;

            Assert.IsNull(_Storage.LoadForPlugin("x.dll"));
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Parses_Text_Of_PluginManifest_File_Version_1()
        {
            _FileContent = @"<?xml version=""1.0"" encoding=""utf-8""?><PluginManifest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">" +
                           @"<MinimumVersion>a</MinimumVersion>" +
                           @"<MaximumVersion>b</MaximumVersion>" +
                           @"</PluginManifest>";

            var manifest = _Storage.LoadForPlugin("x.dll");
            Assert.AreEqual("a", manifest.MinimumVersion);
            Assert.AreEqual("b", manifest.MaximumVersion);
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Does_Not_Load_Content_If_File_Does_Not_Exist()
        {
            _FileExists = false;

            _Storage.LoadForPlugin("x.dll");

            _Provider.Verify(p => p.ReadAllText(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Checks_That_Correct_File_Exists()
        {
            string fileName = null;
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Callback((string s) => { fileName = s; }).Returns(false);

            _Storage.LoadForPlugin(@"c:\tmp\file.dll");

            Assert.IsTrue(@"c:\tmp\file.xml".Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Loads_The_Content_Of_The_Correct_File()
        {
            string fileName = null;
            _Provider.Setup(p => p.ReadAllText(It.IsAny<string>())).Callback((string s) => { fileName = s; }).Returns(@"<?xml version=""1.0"" encoding=""utf-8""?><PluginManifest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""/>");

            _Storage.LoadForPlugin(@"c:\zoo\file.dll");

            Assert.IsTrue(@"c:\zoo\file.xml".Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PluginManifestStorage_LoadForPlugin_Lets_Exceptions_Bubble_Up_If_File_Cannot_Be_Parsed()
        {
            _FileContent = "this won't parse as valid XML";

            bool seenException = false;
            try {
                _Storage.LoadForPlugin("manifest.dll");
            } catch {
                seenException = true;
            }

            Assert.IsTrue(seenException);
        }
        #endregion
    }
}
