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
using VirtualRadar.Library;
using System.IO;
using InterfaceFactory;

namespace Test.VirtualRadar.Library.Settings
{
    [TestClass]
    public class InstallerSettingsStorageTests
    {
        class TestProvider : IInstallerSettingsStorageProvider
        {
            public string Folder { get; set; }
        }

        public TestContext TestContext { get; set; }
        private IInstallerSettingsStorage _Implementation;
        private TestProvider _Provider;
        private const string _FileName = "InstallerConfiguration.xml";
        private string _FullPath;

        [TestInitialize]
        public void TestInitialise()
        {
            _Provider = new TestProvider() { Folder = TestContext.TestDeploymentDir };
            _Implementation = Factory.Resolve<IInstallerSettingsStorage>();
            _Implementation.Provider = _Provider;
            _FullPath = Path.Combine(_Provider.Folder, _FileName);
            if(File.Exists(_FullPath)) File.Delete(_FullPath);
        }

        [TestMethod]
        public void InstallerSettingsStorage_Initialises_To_Known_State_And_Properties_Work()
        {
            _Implementation = Factory.Resolve<IInstallerSettingsStorage>();
            Assert.IsNotNull(_Implementation.Provider);
            Assert.AreNotSame(_Provider, _Implementation.Provider);
            _Implementation.Provider = _Provider;
            Assert.AreSame(_Provider, _Implementation.Provider);
        }

        [TestMethod]
        public void InstallerSettingsStorage_Returns_Default_Object_If_Configuration_File_Missing()
        {
            InstallerSettings settings = _Implementation.Load();
            Assert.AreEqual(80, settings.WebServerPort);
        }

        [TestMethod]
        public void InstallerSettingsStorage_Loads_Settings_From_Configuration_File()
        {
            File.WriteAllLines(_FullPath, new string[] {
                @"<?xml version=""1.0"" encoding=""utf-8""?>",
                @"<InstallerSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">",
                @"  <WebServerPort>65500</WebServerPort>",
                @"</InstallerSettings>",
            });

            InstallerSettings settings = _Implementation.Load();
            Assert.AreEqual(65500, settings.WebServerPort);
        }
    }
}
