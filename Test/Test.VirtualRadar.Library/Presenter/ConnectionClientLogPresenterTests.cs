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
using InterfaceFactory;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Database;
using Test.Framework;
using System.Net;
using System.Net.Sockets;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class ConnectionClientLogPresenterTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IConnectionClientLogPresenter _Presenter;
        private Mock<IConnectionClientLogPresenterProvider> _Provider;
        private ClockMock _Clock;
        private Mock<IConnectionClientLogView> _View;
        private Mock<ILogDatabase> _LogDatabase;
        private Mock<ILog> _Log;
        private List<LogClient> _LogClients;
        private Dictionary<long, IList<LogSession>> _LogSessions;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _LogClients = new List<LogClient>();
            _LogSessions = new Dictionary<long, IList<LogSession>>();
            _LogDatabase = TestUtilities.CreateMockSingleton<ILogDatabase>();
            _LogDatabase.Setup(d => d.FetchAll(It.IsAny<IList<LogClient>>(), It.IsAny<IDictionary<long, IList<LogSession>>>())).Callback((IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessions) => {
                foreach(var client in _LogClients) clients.Add(client);
                foreach(var kvpSession in _LogSessions) sessions.Add(kvpSession.Key, kvpSession.Value);
            });

            _Log = TestUtilities.CreateMockSingleton<ILog>();

            _Presenter = Factory.Singleton.Resolve<IConnectionClientLogPresenter>();

            _Provider = new Mock<IConnectionClientLogPresenterProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.InvokeOnBackgroundThread(It.IsAny<Action<IList<LogClient>>>(), It.IsAny<IList<LogClient>>())).Callback((Action<IList<LogClient>> callback, IList<LogClient> clients) => {
                callback(clients);
            });
            _Presenter.Provider = _Provider.Object;

            _View = new Mock<IConnectionClientLogView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var presenter = Factory.Singleton.Resolve<IConnectionClientLogPresenter>();
            Assert.IsNotNull(presenter.Provider);
            TestUtilities.TestProperty(presenter, "Provider", presenter.Provider, _Provider.Object);
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Loads_Clients_From_Database_And_Copies_To_View()
        {
            var client1 = new LogClient();
            var client2 = new LogClient();
            var session1 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var session2 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;

            _LogClients.Add(client1);
            _LogClients.Add(client2);
            _LogSessions.Add(1L, new List<LogSession>() { session1 });
            _LogSessions.Add(2L, new List<LogSession>() { session2 });

            _View.Setup(v => v.ShowClientsAndSessions(It.IsAny<IList<LogClient>>(), It.IsAny<IDictionary<long, IList<LogSession>>>())).Callback((IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessions) => {
                Assert.AreEqual(2, clients.Count());
                Assert.IsTrue(clients.Where(c => c == client1).Any());
                Assert.IsTrue(clients.Where(c => c == client2).Any());

                Assert.AreEqual(2, sessions.Count());
                Assert.AreEqual(1, sessions[1L].Count);
                Assert.AreSame(session1, sessions[1L][0]);
                Assert.AreEqual(1, sessions[2L].Count);
                Assert.AreSame(session2, sessions[2L][0]);
            });

            _Presenter.Initialise(_View.Object);

            _LogDatabase.Verify(d => d.FetchAll(It.IsAny<IList<LogClient>>(), It.IsAny<IDictionary<long, IList<LogSession>>>()), Times.Once());
            _View.Verify(v => v.ShowClientsAndSessions(It.IsAny<IList<LogClient>>(), It.IsAny<IDictionary<long, IList<LogSession>>>()), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Starts_Reverse_DNS_Lookups_On_Background_Thread()
        {
            var client = new LogClient() { IpAddress = "2.3.2.3" };
            _LogClients.Add(client);
            _Provider.Setup(p => p.InvokeOnBackgroundThread(It.IsAny<Action<IList<LogClient>>>(), It.IsAny<IList<LogClient>>())).Callback((Action<IList<LogClient>> callback, IList<LogClient> clients) => {
                Assert.IsNotNull(callback);
                Assert.AreEqual(1, clients.Count);
                Assert.AreSame(client, clients[0]);
            });

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.InvokeOnBackgroundThread(It.IsAny<Action<IList<LogClient>>>(), It.IsAny<IList<LogClient>>()), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Does_Not_Perform_Reverse_DNS_Lookup_On_Clients_That_Already_Have_Reverse_DNS_Details()
        {
            var client1 = new LogClient() { ReverseDns = "ABC" };
            var client2 = new LogClient() { ReverseDnsDate = new DateTime(2010, 9, 8) };
            var client3 = new LogClient() { ReverseDns = "ABC", ReverseDnsDate = new DateTime(2010, 9, 8) };

            client1.IpAddress = client2.IpAddress = client3.IpAddress = "1.2.3.4";

            _LogClients.Add(client1);
            _LogClients.Add(client2);
            _LogClients.Add(client3);

            _Provider.Setup(p => p.InvokeOnBackgroundThread(It.IsAny<Action<IList<LogClient>>>(), It.IsAny<IList<LogClient>>())).Callback((Action<IList<LogClient>> callback, IList<LogClient> clients) => {
                Assert.AreEqual(2, clients.Count);
                Assert.IsTrue(clients.Where(c => c == client1).Any());
                Assert.IsTrue(clients.Where(c => c == client2).Any());
            });

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.InvokeOnBackgroundThread(It.IsAny<Action<IList<LogClient>>>(), It.IsAny<IList<LogClient>>()), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Looks_Up_ReverseDNS_Details_Via_Provider()
        {
            var client1 = new LogClient() { IpAddress = "1.2.3.4" };
            var ipAddress = IPAddress.Parse(client1.IpAddress);
            _LogClients.Add(client1);

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.LookupReverseDns(It.IsAny<IPAddress>()), Times.Once());
            _Provider.Verify(p => p.LookupReverseDns(ipAddress), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Does_Not_Lookup_ReverseDNS_Details_If_IPAddress_Is_Missing()
        {
            var ipAddress = IPAddress.Parse("1.2.3.4");
            _LogClients.Add(new LogClient() { IpAddress = "1.2.3.4" });
            _LogClients.Add(new LogClient());

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.LookupReverseDns(It.IsAny<IPAddress>()), Times.Once());
            _Provider.Verify(p => p.LookupReverseDns(ipAddress), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Stores_Reverse_DNS_Lookup_On_Database()
        {
            var client = new LogClient() { IpAddress = "1.2.3.4" };
            _LogClients.Add(client);

            _Clock.UtcNowValue = new DateTime(2011, 2, 3, 4, 5, 6, 7);
            _Provider.Setup(p => p.LookupReverseDns(It.IsAny<IPAddress>())).Returns("Reverse Result");

            _LogDatabase.Setup(db => db.UpdateClient(It.IsAny<LogClient>())).Callback((LogClient saveClient) => {
                Assert.AreSame(client, saveClient);
                Assert.AreEqual(_Clock.UtcNowValue, saveClient.ReverseDnsDate);
                Assert.AreEqual("Reverse Result", saveClient.ReverseDns);
            });

            _Presenter.Initialise(_View.Object);

            _LogDatabase.Verify(db => db.UpdateClient(It.IsAny<LogClient>()), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Updates_Display_With_Reverse_DNS_Details()
        {
            var client = new LogClient() { IpAddress = "1.2.3.4" };
            _LogClients.Add(client);

            _Provider.Setup(p => p.LookupReverseDns(It.IsAny<IPAddress>())).Returns("Reverse Result");

            _Presenter.Initialise(_View.Object);

            _View.Verify(v => v.RefreshClientReverseDnsDetails(client), Times.Once());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Uses_IP_Address_If_Reverse_Dns_Gives_Argument_Exception()
        {
            var client = new LogClient() { IpAddress = "1.2.3.4" };
            _LogClients.Add(client);

            _Provider.Setup(p => p.LookupReverseDns(It.IsAny<IPAddress>())).Callback(() => { throw new ArgumentException(); });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("1.2.3.4", client.ReverseDns);
            _LogDatabase.Verify(db => db.UpdateClient(client), Times.Once());
            _View.Verify(db => db.RefreshClientReverseDnsDetails(client), Times.Once());
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Ignores_Clients_Whose_IPAddress_Produces_A_SocketException()
        {
            var client = new LogClient() { IpAddress = "1.2.3.4" };
            _LogClients.Add(client);

            _Provider.Setup(p => p.LookupReverseDns(It.IsAny<IPAddress>())).Callback(() => { throw new SocketException(); });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(null, client.ReverseDns);
            Assert.AreEqual(null, client.ReverseDnsDate);
            _LogDatabase.Verify(db => db.UpdateClient(client), Times.Never());
            _View.Verify(db => db.RefreshClientReverseDnsDetails(client), Times.Never());
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ConnectionClientLogPresenter_Initialise_Logs_Any_Other_Exceptions_From_ReverseDNS_Lookup()
        {
            var client = new LogClient() { IpAddress = "1.2.3.4" };
            _LogClients.Add(client);

            var exception = new InvalidOperationException("Other exception");
            _Provider.Setup(p => p.LookupReverseDns(It.IsAny<IPAddress>())).Callback(() => { throw exception; });

            _Log.Setup(g => g.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string format, object[] parameters) => {
                Assert.IsTrue(parameters.Where(p => {
                    var s = p as string;
                    return s != null && s.Contains("Other exception");
                }).Any());
            });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(null, client.ReverseDns);
            Assert.AreEqual(null, client.ReverseDnsDate);
            _LogDatabase.Verify(db => db.UpdateClient(client), Times.Never());
            _View.Verify(db => db.RefreshClientReverseDnsDetails(client), Times.Never());
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once());
        }
    }
}
