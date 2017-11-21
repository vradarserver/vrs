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
using VirtualRadar.Interface.Database;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using Moq;
using System.IO;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class LogDatabaseTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const string _FileName = "ConnectionLog.sqb";
        private string _FullPath;
        private ILogDatabase _LogDatabase;
        private Mock<ILogDatabaseProvider> _Provider;
        private IClassFactory _OriginalFactory;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private readonly string[] _Cultures = new string[] { "en-GB", "de-DE", "fr-FR", "it-IT", "el-GR", "ru-RU" };

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(m => m.Folder).Returns(TestContext.TestDeploymentDir);

            _FullPath = Path.Combine(_ConfigurationStorage.Object.Folder, _FileName);

            _LogDatabase = Factory.Singleton.ResolveNewInstance<ILogDatabase>();

            _Provider = new Mock<ILogDatabaseProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(m => m.UtcNow).Returns(() => { return DateTime.UtcNow; });
            _LogDatabase.Provider = _Provider.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);

            if(_LogDatabase != null) _LogDatabase.Dispose();
            _LogDatabase = null;

            if(File.Exists(_FullPath)) File.Delete(_FullPath);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void LogDatabase_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _LogDatabase.Dispose();
            _LogDatabase = Factory.Singleton.ResolveNewInstance<ILogDatabase>();
            Assert.IsNotNull(_LogDatabase.Provider);
            TestUtilities.TestProperty(_LogDatabase, "Provider", _LogDatabase.Provider, _Provider.Object);
        }
        #endregion

        #region EstablishSession
        [TestMethod]
        public void LogDatabase_EstablishSession_Creates_Database_File_If_It_Does_Not_Exist()
        {
            Assert.IsFalse(File.Exists(_FullPath));
            _LogDatabase.EstablishSession("1.2.3.4", null);
            Assert.IsTrue(File.Exists(_FullPath));
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Client_Record()
        {
            _LogDatabase.EstablishSession("88.77.66.55", null);

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, clients.Count);
            var client = clients[0];
            Assert.AreEqual("88.77.66.55", client.IpAddress);
            Assert.AreNotEqual(0, client.Id);
            Assert.IsNull(client.ReverseDns);
            Assert.IsNull(client.ReverseDnsDate);
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Client_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        LogDatabase_EstablishSession_Writes_Client_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Different_Client_Records_For_Different_IP_Addresses()
        {
            _LogDatabase.EstablishSession("88.77.66.55", null);
            _LogDatabase.EstablishSession("55.66.77.88", null);

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(2, clients.Count);

            var client1 = clients.Where(c => c.IpAddress == "88.77.66.55").Single();
            var client2 = clients.Where(c => c.IpAddress == "55.66.77.88").Single();
            Assert.AreNotEqual(0, client1.Id);
            Assert.AreNotEqual(0, client2.Id);
            Assert.AreNotEqual(client1.Id, client2.Id);
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Does_Not_Write_Multiple_Client_Records_For_The_Same_Session()
        {
            _LogDatabase.EstablishSession("88.77.66.55", null);
            _LogDatabase.EstablishSession("88.77.66.55", null);

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, clients.Count);
            Assert.AreEqual("88.77.66.55", clients[0].IpAddress);
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Session_Record()
        {
            var now = new DateTime(2011, 7, 6, 5, 4, 3);
            _Provider.Setup(m => m.UtcNow).Returns(now);

            _LogDatabase.EstablishSession("88.77.66.55", "user");

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, sessionsMap.Count);
            var client = clients[0];
            Assert.AreEqual(1, sessionsMap[client.Id].Count);
            var session = sessionsMap[clients[0].Id][0];

            Assert.AreNotEqual(0, session.Id);
            Assert.AreEqual(client.Id, session.Id);
            Assert.AreEqual("user", session.UserName);
            Assert.AreEqual(0L, session.AudioBytesSent);
            Assert.AreEqual(0L, session.CountRequests);
            Assert.AreEqual(new TimeSpan(0), session.Duration);
            Assert.AreEqual(now, session.EndTime);
            Assert.AreEqual(0L, session.HtmlBytesSent);
            Assert.AreEqual(0L, session.ImageBytesSent);
            Assert.AreEqual(0L, session.JsonBytesSent);
            Assert.AreEqual(0L, session.OtherBytesSent);
            Assert.AreEqual(now, session.StartTime);
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Session_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        LogDatabase_EstablishSession_Writes_Session_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Writes_Multiple_Sessions_For_Same_IP_Address()
        {
            _LogDatabase.EstablishSession("88.77.66.55", null);
            _LogDatabase.EstablishSession("88.77.66.55", null);

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, sessionsMap.Count);
            var sessions = sessionsMap[clients[0].Id];

            Assert.AreEqual(2, sessions.Count);
            Assert.AreNotEqual(0, sessions[0].Id);
            Assert.AreNotEqual(0, sessions[1].Id);
            Assert.AreNotEqual(sessions[0].Id, sessions[1].Id);
        }

        [TestMethod]
        public void LogDatabase_EstablishSession_Returns_Session_Record()
        {
            var now = new DateTime(2011, 7, 6, 5, 4, 3);
            _Provider.Setup(m => m.UtcNow).Returns(now);

            var session = _LogDatabase.EstablishSession("88.77.66.55", "user");

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, sessionsMap.Count);
            var client = clients[0];
            Assert.AreEqual(1, sessionsMap[client.Id].Count);
            var fetchedSession = sessionsMap[clients[0].Id][0];

            Assert.AreEqual(session.Id, fetchedSession.Id);
            Assert.AreEqual(client.Id, session.Id);

            Assert.AreEqual(fetchedSession.Id, session.Id);
            Assert.AreEqual(fetchedSession.AudioBytesSent, session.AudioBytesSent);
            Assert.AreEqual(fetchedSession.CountRequests, session.CountRequests);
            Assert.AreEqual(fetchedSession.Duration, session.Duration);
            Assert.AreEqual(fetchedSession.EndTime, session.EndTime);
            Assert.AreEqual(fetchedSession.HtmlBytesSent, session.HtmlBytesSent);
            Assert.AreEqual(fetchedSession.ImageBytesSent, session.ImageBytesSent);
            Assert.AreEqual(fetchedSession.JsonBytesSent, session.JsonBytesSent);
            Assert.AreEqual(fetchedSession.OtherBytesSent, session.OtherBytesSent);
            Assert.AreEqual(fetchedSession.StartTime, session.StartTime);
            Assert.AreEqual(fetchedSession.UserName, session.UserName);
        }
        #endregion

        #region UpdateSession
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogDatabase_UpdateSession_Throws_If_Passed_Null()
        {
            var session = _LogDatabase.EstablishSession("7.6.5.4", null);
            _LogDatabase.UpdateSession(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LogDatabase_UpdateSession_Throws_If_Database_Not_Open()
        {
            LogSession session;
            using(var otherConnection = Factory.Singleton.ResolveNewInstance<ILogDatabase>()) {
                session = otherConnection.EstablishSession("1.2.3.4", null);
            }

            _LogDatabase.UpdateSession(session);
        }

        [TestMethod]
        public void LogDatabase_UpdateSession_Updates_Existing_Session_Record()
        {
            DateTime then = new DateTime(2010, 9, 8, 7, 6, 5);
            DateTime now = then.AddSeconds(1);
            DateTime nowPlusOne = now.AddSeconds(1);
            _Provider.Setup(m => m.UtcNow).Returns(then);

            var session = _LogDatabase.EstablishSession("7.6.5.4", null);
            var originalClientId = session.ClientId;
            var otherClientId = _LogDatabase.EstablishSession("8.8.4.4", null).ClientId;

            session.AudioBytesSent = 1L;
            session.ClientId = otherClientId;
            session.UserName = "user";
            session.CountRequests = 2L;
            session.EndTime = nowPlusOne;
            session.HtmlBytesSent = 3L;
            session.ImageBytesSent = 4L;
            session.JsonBytesSent = 5L;
            session.OtherBytesSent = 6L;
            session.StartTime = now;

            var id = session.Id;

            _LogDatabase.UpdateSession(session);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);

            Assert.AreEqual(2, sessionMap[otherClientId].Count);  // <-- would be 1 if we didn't allow a change of client ID
            Assert.AreEqual(false, sessionMap.ContainsKey(originalClientId));  // <-- would be true if we didn't allow a change of client ID

            var sessions = sessionMap[otherClientId];
            Assert.AreEqual(2, sessions.Count);
            var updatedSession = sessions.Where(s => s.Id == id).Single();

            Assert.AreEqual(1L, updatedSession.AudioBytesSent);
            Assert.AreEqual(otherClientId, updatedSession.ClientId); // <-- would be originalClientId if we didn't allow a change of client ID
            Assert.AreEqual(2L, updatedSession.CountRequests);
            Assert.AreEqual(nowPlusOne - now, updatedSession.Duration);
            Assert.AreEqual(nowPlusOne, updatedSession.EndTime);
            Assert.AreEqual(3L, updatedSession.HtmlBytesSent);
            Assert.AreEqual(id, updatedSession.Id);
            Assert.AreEqual(4L, updatedSession.ImageBytesSent);
            Assert.AreEqual(5L, updatedSession.JsonBytesSent);
            Assert.AreEqual(6L, updatedSession.OtherBytesSent);
            Assert.AreEqual(now, updatedSession.StartTime);
            Assert.AreEqual("user", updatedSession.UserName);
        }

        [TestMethod]
        public void LogDatabase_UpdateSession_Does_Not_Overwrite_Existing_UserName_With_Null()
        {
            var session = _LogDatabase.EstablishSession("1.2.3.4", "user");
            session.UserName = null;

            _LogDatabase.UpdateSession(session);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);
            var loadedSession = sessionMap[clients[0].Id][0];

            Assert.AreEqual("user", loadedSession.UserName);
        }

        [TestMethod]
        public void LogDatabase_UpdateSession_Will_Overwrite_Existing_UserName_With_NonNull()
        {
            var session = _LogDatabase.EstablishSession("1.2.3.4", "user");
            session.UserName = "newUser";

            _LogDatabase.UpdateSession(session);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);
            var loadedSession = sessionMap[clients[0].Id][0];

            Assert.AreEqual("newUser", loadedSession.UserName);
        }

        [TestMethod]
        public void LogDatabase_UpdateSession_Updates_Existing_Session_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        LogDatabase_UpdateSession_Updates_Existing_Session_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateClient
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogDatabase_UpdateClient_Throws_If_Client_Is_Null()
        {
            _LogDatabase.EstablishSession("1.2.3.4", null);
            _LogDatabase.UpdateClient(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LogDatabase_UpdateClient_Throws_If_Database_Is_Not_Open()
        {
            LogClient client;
            using(var otherConnection = Factory.Singleton.ResolveNewInstance<ILogDatabase>()) {
                otherConnection.EstablishSession("1.2.3.4", null);
                var clients = new List<LogClient>();
                var sessionMap = new Dictionary<long, IList<LogSession>>();
                otherConnection.FetchAll(clients, sessionMap);
                client = clients[0];
            }

            _LogDatabase.UpdateClient(client);
        }

        [TestMethod]
        public void LogDatabase_UpdateClient_Updates_Existing_Client_Records()
        {
            _LogDatabase.EstablishSession("1.2.3.4", null);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);

            var now = new DateTime(2010, 9, 8, 7, 6, 5);

            var client = clients[0];
            var id = client.Id;
            client.IpAddress = "8.7.6.5";
            client.ReverseDns = "http://www.youtube.com/watch?v=QH2-TGUlwu4";
            client.ReverseDnsDate = now;

            _LogDatabase.UpdateClient(client);

            clients.Clear();
            sessionMap.Clear();

            _LogDatabase.FetchAll(clients, sessionMap);
            Assert.AreEqual(1, clients.Count);
            var updatedClient = clients[0];

            Assert.AreEqual(id, updatedClient.Id);
            Assert.AreEqual("8.7.6.5", updatedClient.IpAddress);
            Assert.AreEqual("http://www.youtube.com/watch?v=QH2-TGUlwu4", updatedClient.ReverseDns);
            Assert.AreEqual(now, updatedClient.ReverseDnsDate);
        }

        [TestMethod]
        public void LogDatabase_UpdateClient_Updates_Existing_Client_Records_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        LogDatabase_UpdateClient_Updates_Existing_Client_Records();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region FetchAll
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogDatabase_FetchAll_Throws_If_Client_List_Is_Null()
        {
            _LogDatabase.FetchAll(null, new Dictionary<long, IList<LogSession>>());
        }

        [TestMethod]
        public void LogDatabase_FetchAll_Creates_Database_File_If_Missing()
        {
            Assert.IsFalse(File.Exists(_FullPath));
            _LogDatabase.FetchAll(new List<LogClient>(), null);
            Assert.IsTrue(File.Exists(_FullPath));
        }

        [TestMethod]
        public void LogDatabase_FetchAll_Does_Nothing_If_Database_Is_Empty()
        {
            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();

            _LogDatabase.FetchAll(clients, sessionMap);

            Assert.AreEqual(0, clients.Count);
            Assert.AreEqual(0, sessionMap.Count);
        }

        [TestMethod]
        public void LogDatabase_FetchAll_Can_Return_List_Of_Every_Client()
        {
            _LogDatabase.EstablishSession("1.2.3.4", null);
            _LogDatabase.EstablishSession("1.0.0.1", null);

            var clients = new List<LogClient>();
            _LogDatabase.FetchAll(clients, null);

            Assert.AreEqual(2, clients.Count);
            Assert.IsTrue(clients.Where(c => c.IpAddress == "1.2.3.4").Any());
            Assert.IsTrue(clients.Where(c => c.IpAddress == "1.0.0.1").Any());
        }

        [TestMethod]
        public void LogDatabase_FetchAll_Clears_Collections_Before_Use()
        {
            _LogDatabase.EstablishSession("1.2.3.4", null);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);
            _LogDatabase.FetchAll(clients, sessionMap);

            Assert.AreEqual(1, clients.Count);
            Assert.AreEqual(1, sessionMap.Count);
            Assert.AreEqual(1, sessionMap[clients[0].Id].Count);
        }

        [TestMethod]
        public void LogDatabase_FetchAll_Can_Return_List_Of_Every_Client_And_Session()
        {
            var now = new DateTime(2007, 6, 5, 4, 3, 2);

            _Provider.Setup(m => m.UtcNow).Returns(now);
            _LogDatabase.EstablishSession("1.2.3.4", null);

            _Provider.Setup(m => m.UtcNow).Returns(now.AddSeconds(1));
            _LogDatabase.EstablishSession("1.2.3.4", null);

            _Provider.Setup(m => m.UtcNow).Returns(now.AddSeconds(2));
            _LogDatabase.EstablishSession("8.7.6.5", null);

            var clients = new List<LogClient>();
            var sessionMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionMap);

            Assert.AreEqual(2, clients.Count);
            var client1 = clients.Where(c => c.IpAddress == "1.2.3.4").Single();
            var client2 = clients.Where(c => c.IpAddress == "8.7.6.5").Single();
            Assert.AreNotEqual(client1.Id, client2.Id);

            var sessions1 = sessionMap[client1.Id];
            var sessions2 = sessionMap[client2.Id];

            Assert.AreEqual(2, sessions1.Count);
            Assert.AreEqual(1, sessions2.Count);

            Assert.AreEqual(2, sessions1.Where(s => s.ClientId == client1.Id).Count());
            Assert.AreNotEqual(sessions1[0].Id, sessions1[1].Id);
            Assert.IsTrue(sessions1.Where(s => s.StartTime == now).Any());
            Assert.IsTrue(sessions1.Where(s => s.StartTime == now.AddSeconds(1)).Any());
        }
        #endregion

        #region FetchSessions
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogDatabase_FetchSessions_Throws_If_Sessions_List_Is_Null()
        {
            _LogDatabase.FetchSessions(new List<LogClient>(), null, DateTime.Now, DateTime.Now);
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Creates_Database_File_If_Missing()
        {
            Assert.IsFalse(File.Exists(_FullPath));
            _LogDatabase.FetchSessions(new List<LogClient>(), new List<LogSession>(), DateTime.Now, DateTime.Now);
            Assert.IsTrue(File.Exists(_FullPath));
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Can_Fetch_List_Of_Sessions_Within_Date_Range()
        {
            DateTime now = new DateTime(2001, 2, 3, 4, 5, 6);
            _Provider.Setup(m => m.UtcNow).Returns(now);

            _LogDatabase.EstablishSession("4.5.6.7", null);

            var sessions = new List<LogSession>();
            _LogDatabase.FetchSessions(null, sessions, now.ToLocalTime(), now.ToLocalTime());

            Assert.AreEqual(1, sessions.Count);
            Assert.AreEqual(now, sessions[0].StartTime);
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Can_Fetch_List_Of_Clients_And_Sessions_Within_Date_Range()
        {
            var now = new DateTime(2007, 6, 5, 4, 3, 2);

            _Provider.Setup(m => m.UtcNow).Returns(now);
            _LogDatabase.EstablishSession("1.2.3.4", null);

            _Provider.Setup(m => m.UtcNow).Returns(now.AddSeconds(1));
            _LogDatabase.EstablishSession("1.2.3.4", null);

            _Provider.Setup(m => m.UtcNow).Returns(now.AddSeconds(2));
            _LogDatabase.EstablishSession("8.7.6.5", null);

            var clients = new List<LogClient>();
            var sessions = new List<LogSession>();
            _LogDatabase.FetchSessions(clients, sessions, now.ToLocalTime(), now.ToLocalTime().AddSeconds(2));

            Assert.AreEqual(2, clients.Count);
            var client1 = clients.Where(c => c.IpAddress == "1.2.3.4").Single();
            var client2 = clients.Where(c => c.IpAddress == "8.7.6.5").Single();
            Assert.AreNotEqual(client1.Id, client2.Id);

            Assert.AreEqual(3, sessions.Count);
            Assert.AreEqual(2, sessions.Where(s => s.ClientId == client1.Id).Count());
            Assert.AreEqual(1, sessions.Where(s => s.ClientId == client2.Id).Count());

            Assert.IsTrue(sessions.Where(s => s.StartTime == now).Any());
            Assert.IsTrue(sessions.Where(s => s.StartTime == now.AddSeconds(1)).Any());
            Assert.IsTrue(sessions.Where(s => s.StartTime == now.AddSeconds(2)).Any());
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Uses_Local_Dates()
        {
            // It's a bit odd that it uses local dates, really it should be storing UTC - but the code was originally
            // written this way and I can't go changing it now without breaking existing installations...

            var dateTime = new DateTime(2008, 1, 1, 0, 0, 0);
            for(int day = 0;day < 366;++day) {
                dateTime = dateTime.AddDays(1);
                if(dateTime.ToLocalTime().Hour != dateTime.Hour) break;
            }

            _Provider.Setup(m => m.UtcNow).Returns(dateTime);

            _LogDatabase.EstablishSession("1.2.3.4", null);

            var sessions = new List<LogSession>();

            _LogDatabase.FetchSessions(null, sessions, dateTime, dateTime);
            Assert.AreEqual(0, sessions.Count);

            _LogDatabase.FetchSessions(null, sessions, dateTime.ToLocalTime(), dateTime.ToLocalTime());
            Assert.AreEqual(1, sessions.Count);
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Uses_Inclusive_Date_Ranges()
        {
            var before = new DateTime(2001, 2, 3, 10, 9, 8);
            var start = before.AddSeconds(1);
            var between = start.AddSeconds(1);
            var end = between.AddSeconds(1);
            var after = end.AddSeconds(1);

            _Provider.Setup(m => m.UtcNow).Returns(before);
            _LogDatabase.EstablishSession("1.1.1.1", null);
            _Provider.Setup(m => m.UtcNow).Returns(start);
            _LogDatabase.EstablishSession("2.2.2.2", null);
            _Provider.Setup(m => m.UtcNow).Returns(between);
            _LogDatabase.EstablishSession("3.3.3.3", null);
            _Provider.Setup(m => m.UtcNow).Returns(end);
            _LogDatabase.EstablishSession("4.4.4.4", null);
            _Provider.Setup(m => m.UtcNow).Returns(after);
            _LogDatabase.EstablishSession("5.5.5.5", null);

            var sessions = new List<LogSession>();
            _LogDatabase.FetchSessions(null, sessions, start.ToLocalTime(), end.ToLocalTime());

            Assert.AreEqual(3, sessions.Count);
            Assert.IsTrue(sessions.Where(s => s.StartTime.Second == start.Second).Any());
            Assert.IsTrue(sessions.Where(s => s.StartTime.Second == between.Second).Any());
            Assert.IsTrue(sessions.Where(s => s.StartTime.Second == end.Second).Any());
        }

        [TestMethod]
        public void LogDatabase_FetchSessions_Compares_Start_And_End_Times()
        {
            var session1 = _LogDatabase.EstablishSession("1.1.1.1", null);
            var session2 = _LogDatabase.EstablishSession("1.1.1.1", null);
            var session3 = _LogDatabase.EstablishSession("1.1.1.1", null);
            var session4 = _LogDatabase.EstablishSession("1.1.1.1", null);

            var time1 = new DateTime(2001, 1, 1, 0, 0, 0);
            var time2 = time1.AddSeconds(1);
            var time3 = time2.AddSeconds(1);
            var time4 = time3.AddSeconds(1);
            var time5 = time4.AddSeconds(1);

            session1.StartTime = time1;
            session1.EndTime = session2.StartTime = time2;
            session2.EndTime = session3.StartTime = time3;
            session3.EndTime = session4.StartTime = time4;
            session4.EndTime = time5;

            _LogDatabase.UpdateSession(session1);
            _LogDatabase.UpdateSession(session2);
            _LogDatabase.UpdateSession(session3);
            _LogDatabase.UpdateSession(session4);

            var sessions = new List<LogSession>();
            _LogDatabase.FetchSessions(null, sessions, time3.ToLocalTime(), time3.ToLocalTime());

            Assert.AreEqual(2, sessions.Count);
            Assert.IsTrue(sessions.Where(s => s.StartTime == time3).Any());
            Assert.IsTrue(sessions.Where(s => s.EndTime == time3).Any());
        }
        #endregion

        #region Transactions
        [TestMethod]
        public void LogDatabase_Transactions_Can_Commit_Operations_To_Database()
        {
            _LogDatabase.StartTransaction();
            _LogDatabase.EstablishSession("88.77.66.55", null);
            _LogDatabase.EndTransaction();

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(1, clients.Count);
            var client = clients[0];
            Assert.AreEqual("88.77.66.55", client.IpAddress);
            Assert.AreNotEqual(0, client.Id);
            Assert.AreEqual(1, sessionsMap.Values.Count);
        }

        [TestMethod]
        public void LogDatabase_Transactions_Can_Rollback_Inserts()
        {
            _LogDatabase.StartTransaction();
            _LogDatabase.EstablishSession("88.77.66.55", null);
            _LogDatabase.RollbackTransaction();

            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);

            Assert.AreEqual(0, clients.Count);
            Assert.AreEqual(0, sessionsMap.Count);
        }

        [TestMethod]
        public void LogDatabase_Transactions_Can_Be_Nested()
        {
            _LogDatabase.StartTransaction();
            _LogDatabase.EstablishSession("88.77.66.55", null);

            _LogDatabase.StartTransaction();
            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);
            clients[0].ReverseDns = "hello";
            _LogDatabase.UpdateClient(clients[0]);
            _LogDatabase.EndTransaction();

            _LogDatabase.EndTransaction();

            _LogDatabase.FetchAll(clients, sessionsMap);
            Assert.AreEqual(1, clients.Count);
            Assert.AreEqual("hello", clients[0].ReverseDns);
        }

        [TestMethod]
        public void LogDatabase_Transactions_Can_Rollback_Outer_Level_When_Inner_Level_Rollsback()
        {
            _LogDatabase.StartTransaction();
            _LogDatabase.EstablishSession("88.77.66.55", null);

            _LogDatabase.StartTransaction();
            var clients = new List<LogClient>();
            var sessionsMap = new Dictionary<long, IList<LogSession>>();
            _LogDatabase.FetchAll(clients, sessionsMap);
            clients[0].ReverseDns = "hello";
            _LogDatabase.UpdateClient(clients[0]);
            _LogDatabase.RollbackTransaction();

            _LogDatabase.EndTransaction();

            _LogDatabase.FetchAll(clients, sessionsMap);
            Assert.AreEqual(0, clients.Count);
            Assert.AreEqual(0, sessionsMap.Count);
        }
        #endregion
    }
}
