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
using VirtualRadar.Interface;
using VirtualRadar.Library;
using Moq;
using Test.Framework;
using InterfaceFactory;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class NewVersionCheckerTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private INewVersionChecker _NewVersionChecker;
        private Mock<INewVersionCheckerProvider> _Provider;
        private EventRecorder<EventArgs> _NewVersionAvailable;
        private Mock<IApplicationInformation> _ApplicationInformation;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _NewVersionChecker = Factory.ResolveNewInstance<INewVersionChecker>();
            _Provider = new Mock<INewVersionCheckerProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _NewVersionChecker.Provider = _Provider.Object;
            _NewVersionAvailable = new EventRecorder<EventArgs>();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void NewVersionChecker_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _NewVersionChecker = Factory.ResolveNewInstance<INewVersionChecker>();

            Assert.IsNotNull(_NewVersionChecker.Provider);
            TestUtilities.TestProperty(_NewVersionChecker, "Provider", _NewVersionChecker.Provider, _Provider.Object);
            Assert.IsFalse(_NewVersionChecker.IsNewVersionAvailable);
            Assert.AreEqual("http://www.virtualradarserver.co.uk", _NewVersionChecker.DownloadUrl, true);
        }

        [TestMethod]
        public void NewVersionChecker_CheckForNewVersion_Downloads_New_Version_Details_From_Correct_Location()
        {
            string url = null;
            _Provider.Setup(p => p.DownloadFileContent(It.IsAny<string>())).Callback((string downloadUrl) => url = downloadUrl);
            _NewVersionChecker.CheckForNewVersion();
            Assert.AreEqual("http://www.virtualradarserver.co.uk/latestversion.txt", url, true);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "NewVersionChecker$")]
        public void NewVersionChecker_CheckForNewVersion_Returns_Correct_Value()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            if(!String.IsNullOrEmpty(worksheet.String("ApplicationVersion"))) {
                _ApplicationInformation.Setup(p => p.Version).Returns(new Version(worksheet.String("ApplicationVersion")));
                _ApplicationInformation.Setup(p => p.BetaBasedOnFullVersion).Returns(worksheet.EString("BetaBasedOnFullVersion"));
                _ApplicationInformation.Setup(p => p.IsBeta).Returns(!String.IsNullOrEmpty(worksheet.EString("BetaBasedOnFullVersion")));
                _Provider.Setup(p => p.DownloadFileContent(It.IsAny<string>())).Returns(worksheet.String("WebsiteVersion").Replace(@"\r", "\r").Replace(@"\n", "\n"));

                var expected = worksheet.Bool("IsNewer");
                Assert.AreEqual(expected, _NewVersionChecker.CheckForNewVersion());
                Assert.AreEqual(expected, _NewVersionChecker.IsNewVersionAvailable);
            }
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "NewVersionChecker$")]
        public void NewVersionChecker_CheckForNewVersion_Raises_NewVersionAvailable_When_Appropriate()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            if(!String.IsNullOrEmpty(worksheet.String("ApplicationVersion"))) {
                _NewVersionChecker.NewVersionAvailable += _NewVersionAvailable.Handler;
                _NewVersionAvailable.EventRaised += (object sender, EventArgs args) => {
                    Assert.AreEqual(true, _NewVersionChecker.IsNewVersionAvailable);
                };

                _ApplicationInformation.Setup(p => p.Version).Returns(new Version(worksheet.String("ApplicationVersion")));
                _ApplicationInformation.Setup(p => p.BetaBasedOnFullVersion).Returns(worksheet.EString("BetaBasedOnFullVersion"));
                _ApplicationInformation.Setup(p => p.IsBeta).Returns(!String.IsNullOrEmpty(worksheet.EString("BetaBasedOnFullVersion")));
                _Provider.Setup(p => p.DownloadFileContent(It.IsAny<string>())).Returns(worksheet.String("WebsiteVersion").Replace(@"\r", "\r").Replace(@"\n", "\n"));

                _NewVersionChecker.CheckForNewVersion();

                if(!worksheet.Bool("IsNewer")) Assert.AreEqual(0, _NewVersionAvailable.CallCount);
                else {
                    Assert.AreEqual(1, _NewVersionAvailable.CallCount);
                    Assert.AreSame(_NewVersionChecker, _NewVersionAvailable.Sender);
                    Assert.IsNotNull(_NewVersionAvailable.Args);
                }
            }
        }
    }
}
