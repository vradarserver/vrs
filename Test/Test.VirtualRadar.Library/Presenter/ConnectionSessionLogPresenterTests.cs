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
using VirtualRadar.Interface.Presenter;
using Moq;
using VirtualRadar.Interface.View;
using InterfaceFactory;
using VirtualRadar.Interface.Database;
using Test.Framework;
using System.Threading;
using System.Globalization;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class ConnectionSessionLogPresenterTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IConnectionSessionLogPresenter _Presenter;
        private Mock<IConnectionSessionLogView> _View;
        private Mock<ILogDatabase> _LogDatabase;
        private List<LogClient> _LogClients;
        private List<LogSession> _LogSessions;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _LogClients = new List<LogClient>();
            _LogSessions = new List<LogSession>();
            _LogDatabase = TestUtilities.CreateMockSingleton<ILogDatabase>();
            _LogDatabase.Setup(d => d.FetchSessions(It.IsAny<IList<LogClient>>(), It.IsAny<IList<LogSession>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Callback((IList<LogClient> clients, IList<LogSession> sessions, DateTime startDate, DateTime endDate) => {
                foreach(var client in _LogClients) clients.Add(client);
                foreach(var session in _LogSessions) sessions.Add(session);
            });

            _Presenter = Factory.Singleton.Resolve<IConnectionSessionLogPresenter>();
            _View = new Mock<IConnectionSessionLogView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void ConnectionSessionLogPresenter_Initialise_Sets_Initial_Values_For_View_Properties()
        {
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(DateTime.Today, _View.Object.StartDate);
            Assert.AreEqual(DateTime.Today, _View.Object.EndDate);
        }

        [TestMethod]
        public void ConnectionSessionLogPresenter_Clicking_ShowSessions_Triggers_Display_Of_Sessions()
        {
            _Presenter.Initialise(_View.Object);
            _View.Object.StartDate = new DateTime(2010, 3, 4);
            _View.Object.EndDate = new DateTime(2011, 7, 8);

            var client1 = new Mock<LogClient>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var client2 = new Mock<LogClient>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var session1 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var session2 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;

            _LogClients.Add(client1);
            _LogClients.Add(client2);
            _LogSessions.Add(session1);
            _LogSessions.Add(session2);

            _View.Setup(v => v.ShowSessions(It.IsAny<IEnumerable<LogClient>>(), It.IsAny<IEnumerable<LogSession>>())).Callback((IEnumerable<LogClient> clients, IEnumerable<LogSession> sessions) => {
                Assert.AreEqual(2, clients.Count());
                Assert.IsTrue(clients.Where(c => c == client1).Any());
                Assert.IsTrue(clients.Where(c => c == client2).Any());

                Assert.AreEqual(2, sessions.Count());
                Assert.IsTrue(sessions.Where(s => s == session1).Any());
                Assert.IsTrue(sessions.Where(s => s == session2).Any());
            });

            _View.Raise(v => v.ShowSessionsClicked += null, EventArgs.Empty);

            _LogDatabase.Verify(d => d.FetchSessions(It.IsAny<IList<LogClient>>(), It.IsAny<IList<LogSession>>(), new DateTime(2010, 3, 4, 0, 0, 0, 0), new DateTime(2011, 7, 8, 23, 59, 59, 999)), Times.Once());
            _View.Verify(v => v.ShowSessions(It.IsAny<IEnumerable<LogClient>>(), It.IsAny<IEnumerable<LogSession>>()), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateSessionLogView$")]
        public void ConnectionSessionLogPresenter_Clicking_ShowSessions_Triggers_Validation()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);
            _View.Object.StartDate = worksheet.DateTime("StartDate");
            _View.Object.EndDate = worksheet.DateTime("EndDate");

            IEnumerable<ValidationResult> validationResults = null;
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> results) => {
                validationResults = results;
            });

            _View.Raise(v => v.ShowSessionsClicked += null, EventArgs.Empty);

            Assert.IsNotNull(validationResults);
            Assert.AreEqual(worksheet.Int("CountErrors"), validationResults.Count());
            if(validationResults.Count() > 0) {
                Assert.IsTrue(validationResults.Where(r => r.Field == worksheet.ParseEnum<ValidationField>("Field") &&
                                                           r.Message == worksheet.String("Message") &&
                                                           r.IsWarning == worksheet.Bool("IsWarning")).Any());
            }
        }

        [TestMethod]
        public void ConnectionSessionLogPresenter_Clicking_ShowSessions_Does_Not_Display_Sessions_If_Validation_Failed()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.StartDate = DateTime.Today;
            _View.Object.EndDate = _View.Object.StartDate.AddDays(-1);

            _View.Raise(v => v.ShowSessionsClicked += null, EventArgs.Empty);

            _LogDatabase.Verify(db => db.FetchSessions(It.IsAny<IList<LogClient>>(), It.IsAny<IList<LogSession>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
            _View.Verify(v => v.ShowSessions(It.IsAny<IEnumerable<LogClient>>(), It.IsAny<IEnumerable<LogSession>>()), Times.Never());
        }
    }
}
