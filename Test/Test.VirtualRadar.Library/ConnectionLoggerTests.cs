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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.WebServer;
using InterfaceFactory;
using Test.Framework;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class ConnectionLoggerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const int MinimumCacheMinutes = 1;
        private const int GapBetweenExceptionsSeconds = 30;
        private const int SessionDurationMinutes = 10;

        private IClassFactory _ClassFactorySnapshot;
        private IConnectionLogger _ConnectionLogger;
        private ClockMock _Clock;
        private Mock<IWebServer> _WebServer;
        private Mock<ILogDatabase> _LogDatabase;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<LogSession> _LogSession;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LogDatabase = new Mock<ILogDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LogSession = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _ExceptionCaughtEvent = new EventRecorder<EventArgs<Exception>>();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _ConnectionLogger = Factory.Singleton.Resolve<IConnectionLogger>();
            _ConnectionLogger.WebServer = _WebServer.Object;
            _ConnectionLogger.LogDatabase = _LogDatabase.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Helper functions - SetUtcNow, RaiseResponseEvent, RaiseHeartbeatEvent
        /// <summary>
        /// Configures the provider to return a given time and returns that time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private DateTime SetUtcNow(DateTime dateTime)
        {
            _Clock.UtcNowValue = dateTime;
            return dateTime;
        }

        /// <summary>
        /// Raises an event on the web webServer to simulate a response to a request being sent at a given time.
        /// </summary>
        /// <param name="userAddress"></param>
        /// <param name="bytesSent"></param>
        /// <param name="contentClassification"></param>
        /// <param name="timeSent"></param>
        /// <returns></returns>
        private DateTime RaiseResponseEvent(string userAddress, long bytesSent, ContentClassification contentClassification, DateTime timeSent)
        {
            SetUtcNow(timeSent);
            _WebServer.Raise(s => s.ResponseSent += null, new ResponseSentEventArgs(null, null, userAddress, bytesSent, contentClassification, null, 0, 0, null));
            return timeSent;
        }

        /// <summary>
        /// Raises an event on the heartbeat service at the time supplied.
        /// </summary>
        /// <param name="heartbeatTime"></param>
        /// <returns></returns>
        private DateTime RaiseHeartbeatEvent(DateTime heartbeatTime)
        {
            SetUtcNow(heartbeatTime);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            return heartbeatTime;
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void ConnectionLogger_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _ConnectionLogger = Factory.Singleton.Resolve<IConnectionLogger>();

            TestUtilities.TestProperty(_ConnectionLogger, "LogDatabase", null, _LogDatabase.Object);
            TestUtilities.TestProperty(_ConnectionLogger, "WebServer", null, _WebServer.Object);
        }

        [TestMethod]
        public void ConnectionLogger_Singleton_Returns_Same_Reference_For_All_Instances()
        {
            var instance1 = Factory.Singleton.Resolve<IConnectionLogger>();
            var instance2 = Factory.Singleton.Resolve<IConnectionLogger>();

            Assert.AreNotSame(instance1, instance2);
            Assert.IsNotNull(instance1.Singleton);
            Assert.AreSame(instance1.Singleton, instance2.Singleton);
        }
        #endregion

        #region Start
        #region ResponseSent
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectionLogger_Start_Throws_If_WebServer_Is_Null()
        {
            _ConnectionLogger.WebServer = null;
            _ConnectionLogger.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectionLogger_Start_Throws_If_Database_Is_Null()
        {
            _ConnectionLogger.LogDatabase = null;
            _ConnectionLogger.Start();
        }

        [TestMethod]
        public void ConnectionLogger_Start_Creates_New_Session_Information_If_IP_Address_Never_Seen_Before()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            RaiseResponseEvent("1.2.3.4", 7L, ContentClassification.Audio, now);

            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());
            Assert.AreEqual(1, _LogSession.Object.CountRequests);
            Assert.AreEqual(now, _LogSession.Object.EndTime);
            Assert.AreEqual(7L, _LogSession.Object.AudioBytesSent);
        }

        [TestMethod]
        public void ConnectionLogger_Start_Ignores_Unhandled_Requests()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            RaiseResponseEvent("1.2.3.4", 0L, ContentClassification.Other, now);

            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Never());
            Assert.AreEqual(0, _LogSession.Object.CountRequests);
        }

        [TestMethod]
        public void ConnectionLogger_Start_Ignores_Null_IPAddresses()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            RaiseResponseEvent(null, 7L, ContentClassification.Audio, now);

            _LogDatabase.Verify(d => d.EstablishSession(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Ignores_Empty_IPAddresses()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            RaiseResponseEvent("", 7L, ContentClassification.Audio, now);

            _LogDatabase.Verify(d => d.EstablishSession(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Updates_Session_Information_If_IPAddress_Seen_Before()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            RaiseResponseEvent("1.2.3.4", 7L, ContentClassification.Audio, now);
            now = RaiseResponseEvent("1.2.3.4", 4L, ContentClassification.Audio, now.AddSeconds(1));

            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());
            Assert.AreEqual(2, _LogSession.Object.CountRequests);
            Assert.AreEqual(now, _LogSession.Object.EndTime);
            Assert.AreEqual(11L, _LogSession.Object.AudioBytesSent);
        }

        [TestMethod]
        public void ConnectionLogger_Start_Establishes_New_Session_For_Each_IPAddress()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            var session1 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var session2 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(session1.Object);
            _LogDatabase.Setup(d => d.EstablishSession("9.8.7.6")).Returns(session2.Object);

            RaiseResponseEvent("1.2.3.4", 7L, ContentClassification.Audio, now);
            RaiseResponseEvent("9.8.7.6", 4L, ContentClassification.Audio, now);

            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());
            _LogDatabase.Verify(d => d.EstablishSession("9.8.7.6"), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Updates_Appropriate_BytesSent_Session_Property()
        {
            foreach(ContentClassification contentClassification in Enum.GetValues(typeof(ContentClassification))) {
                TestCleanup();
                TestInitialise();

                var now = SetUtcNow(DateTime.Now);
                _ConnectionLogger.Start();

                _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
                RaiseResponseEvent("1.2.3.4", 100L, contentClassification, now);

                switch(contentClassification) {
                    case ContentClassification.Audio:
                    case ContentClassification.Html:
                    case ContentClassification.Image:
                    case ContentClassification.Json:
                    case ContentClassification.Other:
                        Assert.AreEqual(contentClassification == ContentClassification.Audio ? 100L : 0L,   _LogSession.Object.AudioBytesSent);
                        Assert.AreEqual(contentClassification == ContentClassification.Html ? 100L : 0L,    _LogSession.Object.HtmlBytesSent);
                        Assert.AreEqual(contentClassification == ContentClassification.Image ? 100L : 0L,   _LogSession.Object.ImageBytesSent);
                        Assert.AreEqual(contentClassification == ContentClassification.Json ? 100L : 0L,    _LogSession.Object.JsonBytesSent);
                        Assert.AreEqual(contentClassification == ContentClassification.Other ? 100L : 0L,   _LogSession.Object.OtherBytesSent);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region Heartbeat tick
        [TestMethod]
        public void ConnectionLogger_Start_Writes_Sessions_To_Database()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);

            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Writes_Session_Within_Transaction()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _LogDatabase.Setup(d => d.UpdateSession(It.IsAny<LogSession>())).Callback(() => {
                _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            });
            _LogDatabase.Setup(d => d.EndTransaction()).Callback(() => {
                _LogDatabase.Verify(d => d.UpdateSession(It.IsAny<LogSession>()), Times.Once());
            });

            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.RollbackTransaction(), Times.Never());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Rolls_Back_Session_If_Exception_Raised()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _LogDatabase.Setup(d => d.UpdateSession(It.IsAny<LogSession>())).Callback(() => { throw new InvalidOperationException(); });

            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Never());
            _LogDatabase.Verify(d => d.RollbackTransaction(), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Writes_All_Sessions_To_Database()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            var session1 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var session2 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(session1.Object);
            _LogDatabase.Setup(d => d.EstablishSession("9.8.7.6")).Returns(session2.Object);

            RaiseResponseEvent("1.2.3.4", 7L, ContentClassification.Audio, now);
            RaiseResponseEvent("9.8.7.6", 4L, ContentClassification.Audio, now);

            now = RaiseHeartbeatEvent(now.AddMinutes(MinimumCacheMinutes));

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(session1.Object), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(session2.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Does_Nothing_If_No_Session_Established_By_Heartbeat_Tick()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            now = RaiseHeartbeatEvent(now.AddMinutes(30));

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Never());
            _LogDatabase.Verify(d => d.UpdateSession(It.IsAny<LogSession>()), Times.Never());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Never());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Updates_Session_On_Every_Tick()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);

            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);
            now = RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now.AddMinutes(MinimumCacheMinutes));
            RaiseHeartbeatEvent(now);

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Exactly(2));
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Exactly(2));
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Exactly(2));
        }

        [TestMethod]
        public void ConnectionLogger_Start_Does_Nothing_If_Tick_Is_Too_Soon_After_Previous_Tick()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);

            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);
            now = RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now.AddMinutes(MinimumCacheMinutes).AddSeconds(-1));
            RaiseHeartbeatEvent(now);

            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Start_Establishes_New_Session_If_IPAddress_Makes_No_Requests_For_Timeout_Minutes()
        {
            var now = SetUtcNow(DateTime.Now);
            _ConnectionLogger.Start();

            int callCount = 0;
            var session1 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var session2 = new Mock<LogSession>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(() => {
                return callCount++ == 0 ? session1.Object : session2.Object;
            });
            _LogDatabase.Setup(d => d.EstablishSession("9.8.7.6")).Returns(_LogSession.Object);

            // 1st response to 1.2.3.4
            RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);
            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(session1.Object), Times.Once());

            // next response just 1 second before timeout
            now = RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now.AddMinutes(SessionDurationMinutes).AddSeconds(-1));
            RaiseHeartbeatEvent(now);
            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(session1.Object), Times.Exactly(2));

            // this response is for a different IP address - if the previous request had not reset the timeout then the session for 1.2.3.4
            // would be removed from the cache here...
            now = RaiseResponseEvent("9.8.7.6", 100L, ContentClassification.Html, now.AddMinutes(MinimumCacheMinutes));
            RaiseHeartbeatEvent(now);

            // which means that the next response for it would establish a new session, which would be wrong
            now = RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now.AddMinutes(MinimumCacheMinutes));
            RaiseHeartbeatEvent(now);
            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Once());

            // next response is for a different IP address and this time the timeout will have expired so it should be removed from the cache,
            // but not before the last version of it is written to the database
            now = RaiseResponseEvent("9.8.7.6", 100L, ContentClassification.Html, now.AddMinutes(SessionDurationMinutes));
            RaiseHeartbeatEvent(now);
            _LogDatabase.Verify(d => d.UpdateSession(session1.Object), Times.Exactly(5));

            // which means this reponse for 1.2.3.4 will establish a new session
            now = RaiseResponseEvent("1.2.3.4", 100L, ContentClassification.Html, now.AddMinutes(MinimumCacheMinutes));
            RaiseHeartbeatEvent(now);
            _LogDatabase.Verify(d => d.EstablishSession("1.2.3.4"), Times.Exactly(2));
            _LogDatabase.Verify(d => d.UpdateSession(session2.Object), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(session1.Object), Times.Exactly(5));
        }
        #endregion
        #endregion

        #region ExceptionCaught
        [TestMethod]
        public void ConnectionLogger_ExceptionCaught_Raised_When_Database_Throws_During_ResponseSent_Handling()
        {
            _ConnectionLogger.Start();
            _ConnectionLogger.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            var exception = new InvalidOperationException();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Callback(() => { throw exception; });

            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now);

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
        }

        [TestMethod]
        public void ConnectionLogger_ExceptionCaught_Not_Raised_If_Exceptions_Throws_Too_Quickly_During_ResponseSent_Handling()
        {
            _ConnectionLogger.Start();
            _ConnectionLogger.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            var exception = new InvalidOperationException();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Callback(() => { throw exception; });

            var now = RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now); // <-- should raise exception
            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, now.AddSeconds(GapBetweenExceptionsSeconds - 1));  // <-- should not raise exception

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
        }

        [TestMethod]
        public void ConnectionLogger_ExceptionCaught_Raised_If_Second_Exception_Throws_After_Anti_GUI_Spamming_Timeout_Expires()
        {
            _ConnectionLogger.Start();
            _ConnectionLogger.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            var exception = new InvalidOperationException();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Callback(() => { throw exception; });

            var now = RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now); // <-- should raise exception
            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, now.AddSeconds(GapBetweenExceptionsSeconds));  // <-- should raise exception

            Assert.AreEqual(2, _ExceptionCaughtEvent.CallCount);
        }

        [TestMethod]
        public void ConnectionLogger_ExceptionCaught_Raised_When_Database_Throws_During_Heartbeat_Tick_Handling()
        {
            _ConnectionLogger.Start();
            _ConnectionLogger.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            var exception = new InvalidOperationException();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _LogDatabase.Setup(d => d.UpdateSession(_LogSession.Object)).Callback(() => { throw exception; });

            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now);
            RaiseHeartbeatEvent(DateTime.Now);

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
            Assert.AreSame(_ConnectionLogger, _ExceptionCaughtEvent.Sender);
        }

        [TestMethod]
        public void ConnectionLogger_ExceptionCaught_Not_Raised_If_Exception_Happens_Again_Too_Quickly_During_Heartbeat_Tick_Handling()
        {
            _ConnectionLogger.Start();
            _ConnectionLogger.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            var exception = new InvalidOperationException();
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _LogDatabase.Setup(d => d.UpdateSession(_LogSession.Object)).Callback(() => { throw exception; });

            var now = RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now);
            RaiseHeartbeatEvent(now);   // <-- should raise exception
            now = RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, now.AddSeconds(GapBetweenExceptionsSeconds - 1));
            RaiseHeartbeatEvent(now);   // <-- should not raise exception, would be spamming the UI with message boxes

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void ConnectionLogger_Dispose_Flushes_Sessions_Out_To_Database()
        {
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _ConnectionLogger.Start();

            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, DateTime.Now);

            _ConnectionLogger.Dispose();
            _LogDatabase.Verify(d => d.StartTransaction(), Times.Once());
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Once());
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Once());
        }

        [TestMethod]
        public void ConnectionLogger_Dispose_Flushes_Sessions_Even_If_Dispose_Occurs_Before_Minimum_Cache_Timer_Has_Elapsed()
        {
            var now = SetUtcNow(DateTime.Now);
            _LogDatabase.Setup(d => d.EstablishSession("1.2.3.4")).Returns(_LogSession.Object);
            _ConnectionLogger.Start();

            RaiseResponseEvent("1.2.3.4", 1, ContentClassification.Html, now);
            RaiseHeartbeatEvent(now);
            RaiseResponseEvent("1.2.3.4", 2, ContentClassification.Html, now.AddSeconds(1));

            _ConnectionLogger.Dispose();
            _LogDatabase.Verify(d => d.StartTransaction(), Times.Exactly(2));
            _LogDatabase.Verify(d => d.UpdateSession(_LogSession.Object), Times.Exactly(2));
            _LogDatabase.Verify(d => d.EndTransaction(), Times.Exactly(2));
        }
        #endregion
    }
}
