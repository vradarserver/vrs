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
using VirtualRadar.Interface.WebServer;
using Moq;
using Test.VirtualRadar;
using System.Net;
using VirtualRadar.Interface;
using System.IO;
using Test.Framework;
using System.Reflection;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using System.Collections.Specialized;

namespace Test.VirtualRadar.WebServer
{
    [TestClass]
    public class WebServerTests
    {
        #region TestContext, TestInitialise, TestCleanup, Fields
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IWebServer _WebServer;
        private Mock<IWebServerProvider> _Provider;
        private Mock<IContext> _Context;
        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;
        private MemoryStream _OutputStream;
        private EventRecorder<EventArgs> _OnlineChangedEvent;
        private EventRecorder<AuthenticationRequiredEventArgs> _AuthenticationRequiredEvent;
        private EventRecorder<RequestReceivedEventArgs> _BeforeRequestReceivedEvent;
        private EventRecorder<RequestReceivedEventArgs> _RequestReceivedEvent;
        private EventRecorder<RequestReceivedEventArgs> _AfterRequestReceivedEvent;
        private EventRecorder<ResponseSentEventArgs> _ResponseSentEvent;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;
        private EventRecorder<EventArgs> _ExternalAddressChangedEvent;
        private DateTime _Now;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _WebServer = Factory.Singleton.Resolve<IWebServer>();

            _Now = DateTime.UtcNow;
            _Provider = new Mock<IWebServerProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(m => m.IsListening).Returns(true);
            _Provider.Setup(m => m.UtcNow).Returns(() => { return _Now; });
            _Provider.Object.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _WebServer.Provider = _Provider.Object;

            _Context = new Mock<IContext>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Request.Setup(m => m.RemoteEndPoint).Returns(new IPEndPoint(0x0100007fL, 12000));

            _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _OutputStream = new MemoryStream();
            _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

            _Context.Setup(m => m.Request).Returns(_Request.Object);
            _Context.Setup(m => m.Response).Returns(_Response.Object);

            _OnlineChangedEvent = new EventRecorder<EventArgs>();
            _AuthenticationRequiredEvent = new EventRecorder<AuthenticationRequiredEventArgs>();
            _BeforeRequestReceivedEvent = new EventRecorder<RequestReceivedEventArgs>();
            _RequestReceivedEvent = new EventRecorder<RequestReceivedEventArgs>();
            _AfterRequestReceivedEvent = new EventRecorder<RequestReceivedEventArgs>();
            _ResponseSentEvent = new EventRecorder<ResponseSentEventArgs>();
            _ExceptionCaughtEvent = new EventRecorder<EventArgs<Exception>>();
            _ExternalAddressChangedEvent = new EventRecorder<EventArgs>();

            _WebServer.ExceptionCaught += DefaultExceptionCaughtHandler;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);

            if(_WebServer != null) _WebServer.Dispose();
            _WebServer = null;

            if(_OutputStream != null) _OutputStream.Dispose();
            _OutputStream = null;
        }
        #endregion

        #region DefaultExceptionCaughtHandler, ConfigureProviderForGetContext, ConfigureResponse
        /// <summary>
        /// Called by default when an exception is caught during processing on the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DefaultExceptionCaughtHandler(object sender, EventArgs<Exception> args)
        {
            Assert.Fail("Exception raised during test - {0}", args.Value.ToString());
        }

        /// <summary>
        /// Configures the provider for a Begin/End GetContext set of calls that supply a request context to the server.
        /// </summary>
        /// <param name="runs"></param>
        /// <param name="beginGetContextCallback"></param>
        /// <returns></returns>
        private IAsyncResult ConfigureProviderForGetContext(int runs = 1, Action beginGetContextCallback = null)
        {
            IAsyncResult asyncResult = new Mock<IAsyncResult>().Object;

            int beginGetContextCallCount = 0;
            _Provider.Setup(m => m.BeginGetContext(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(beginGetContextCallback != null)     beginGetContextCallback();
                    if(beginGetContextCallCount++ < runs)   callback(asyncResult);
                    return asyncResult;
                });

            _Provider.Setup(m => m.EndGetContext(asyncResult)).Returns(_Context.Object);

            return asyncResult;
        }

        /// <summary>
        /// Configures the request event handler to respond to a request.
        /// </summary>
        /// <param name="handleRequest"></param>
        /// <param name="mimeType"></param>
        /// <param name="content"></param>
        /// <param name="contentLength"></param>
        /// <param name="statusCode"></param>
        /// <param name="contentClassification"></param>
        /// <param name="milliseconds"></param>
        private void ConfigureResponse(bool handleRequest, string mimeType, string content, long contentLength = -1, HttpStatusCode statusCode = HttpStatusCode.OK, ContentClassification contentClassification = ContentClassification.Other, int milliseconds = 0)
        {
            ConfigureResponse(handleRequest, mimeType, Encoding.UTF8.GetBytes(content), contentLength, statusCode, contentClassification, milliseconds);
        }

        /// <summary>
        /// Configures the request event handler to respond to a request.
        /// </summary>
        /// <param name="handleRequest"></param>
        /// <param name="mimeType"></param>
        /// <param name="content"></param>
        /// <param name="contentLength"></param>
        /// <param name="statusCode"></param>
        /// <param name="contentClassification"></param>
        /// <param name="milliseconds"></param>
        private void ConfigureResponse(bool handleRequest, string mimeType, byte[] content, long contentLength = -1, HttpStatusCode statusCode = HttpStatusCode.OK, ContentClassification contentClassification = ContentClassification.Other, int milliseconds = 0)
        {
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => {
                args.Handled = handleRequest;
                if(handleRequest) {
                    args.Classification = contentClassification;
                    args.Response.ContentLength = contentLength >= 0L ? contentLength : content.LongLength;
                    args.Response.MimeType = mimeType;
                    args.Response.StatusCode = statusCode;
                    _OutputStream.Write(content, 0, content.Length);
                }
                _Now = _Now.AddMilliseconds(milliseconds);
            };
        }
        #endregion

        #region Simple Properties
        [TestMethod]
        public void WebServer_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _WebServer = Factory.Singleton.Resolve<IWebServer>();
            Assert.IsNotNull(_WebServer.Provider);
            Assert.AreNotSame(_Provider.Object, _WebServer.Provider);

            // Note that there is a separate test for the correct getting and setting of AuthenticationScheme in the authentication region
            TestUtilities.TestProperty(_WebServer, "CacheCredentials", false);
            TestUtilities.TestProperty(_WebServer, "ExternalIPAddress", null, "Ab");
            TestUtilities.TestProperty(_WebServer, "ExternalPort", 80, 123);
            TestUtilities.TestProperty(_WebServer, "Port", 80, 120);
            TestUtilities.TestProperty(_WebServer, "Root", "/", "/PoohBear");
            TestUtilities.TestProperty(_WebServer, "Provider", _WebServer.Provider, _Provider.Object);
        }

        [TestMethod]
        public void WebServer_Prefix_Reports_Initial_State_Correctly()
        {
            Assert.AreEqual("http://*:80/", _WebServer.Prefix);
        }

        [TestMethod]
        public void WebServer_Root_Enforces_Slashes()
        {
            _WebServer.Root = "Andrew";
            Assert.AreEqual("/Andrew", _WebServer.Root);

            _WebServer.Root = "/Andrew";
            Assert.AreEqual("/Andrew", _WebServer.Root);

            _WebServer.Root = "/Andrew/";
            Assert.AreEqual("/Andrew", _WebServer.Root);

            _WebServer.Root = "";
            Assert.AreEqual("/", _WebServer.Root);

            _WebServer.Root = null;
            Assert.AreEqual("/", _WebServer.Root);
        }

        [TestMethod]
        public void WebServer_PortText_Reflects_Value_Of_Port()
        {
            Assert.AreEqual("", _WebServer.PortText);
            _WebServer.Port = 12345;
            Assert.AreEqual(":12345", _WebServer.PortText);
        }

        [TestMethod]
        public void WebServer_Root_SetValue_Does_Not_Change_Listener_Prefix()
        {
            _Provider.SetupAllProperties();
            _WebServer.Root = "Hello";
            Assert.IsNull(_Provider.Object.ListenerPrefix);
        }

        [TestMethod]
        public void WebServer_Port_SetValue_Does_Not_Change_Listener_Prefix()
        {
            _Provider.SetupAllProperties();
            _WebServer.Port = 9001;
            Assert.IsNull(_Provider.Object.ListenerPrefix);
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Address$")]
        public void WebServer_Addresses_Are_Reported_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _WebServer.Root = worksheet.String("Root");
            _WebServer.Port = worksheet.NInt("Port") ?? 80;
            if(worksheet.String("IPAddresses") == null) _Provider.Setup(p => p.GetHostAddresses()).Returns((IPAddress[])null);
            else {
                var ipAddresses = new List<IPAddress>();
                foreach(var chunk in worksheet.EString("IPAddresses").Split(',')) {
                    if(chunk.Length > 0) ipAddresses.Add(IPAddress.Parse(chunk));
                }
                _Provider.Setup(p => p.GetHostAddresses()).Returns(ipAddresses.ToArray());
            }
            _WebServer.ExternalIPAddress = worksheet.EString("ExternalIPAddress");
            _WebServer.ExternalPort = worksheet.NInt("UPnPExternalPort") ?? 80;

            Assert.AreEqual(worksheet.String("LocalAddress"), _WebServer.LocalAddress);
            Assert.AreEqual(worksheet.String("NetworkAddress"), _WebServer.NetworkAddress);
            Assert.AreEqual(worksheet.String("NetworkIPAddress"), _WebServer.NetworkIPAddress);
            Assert.AreEqual(worksheet.String("ExternalAddress"), _WebServer.ExternalAddress);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void WebServer_Dispose_Disposes_Of_Provider()
        {
            _WebServer.Dispose();
            _Provider.Verify(m => m.Dispose(), Times.Once());
        }
        #endregion

        #region Online Property
        [TestMethod]
        public void WebServer_Online_Defaults_To_False()
        {
            Assert.AreEqual(false, _WebServer.Online);
        }

        [TestMethod]
        public void WebServer_Online_SetValue_True_Writes_Correct_Prefix_To_Listener()
        {
            object[][] testValues = new object[][] {
                new object[] { null, 80, "http://*:80/" },
                new object[] { "/A", 91, "http://*:91/A/" },
                new object[] { "/A/B", 8080, "http://*:8080/A/B/" },
            };

            foreach(var row in testValues) {
                TestCleanup();
                TestInitialise();

                string root = (string)row[0];
                int port = (int)row[1];
                string prefix = (string)row[2];

                _WebServer.Root = root;
                _WebServer.Port = port;
                Assert.AreEqual(prefix, _WebServer.Prefix);

                _WebServer.Online = true;
                _Provider.VerifySet(m => m.ListenerPrefix = prefix);
            }
        }

        [TestMethod]
        public void WebServer_Online_SetValue_True_Starts_Listener()
        {
            _WebServer.Online = true;
            Assert.AreEqual(true, _WebServer.Online);
            _Provider.Verify(m => m.StartListener(), Times.Once());
            _Provider.Verify(m => m.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Once());
            _Provider.Verify(m => m.StopListener(), Times.Never());
        }

        [TestMethod]
        public void WebServer_Online_SetValue_True_Does_Nothing_If_Server_Already_Running()
        {
            _WebServer.Online = true;
            _WebServer.Online = true;
            _Provider.Verify(m => m.StartListener(), Times.Once());
        }

        [TestMethod]
        public void WebServer_Online_SetValue_False_Stops_Listener()
        {
            _WebServer.Online = true;
            _Provider = new Mock<IWebServerProvider>();
            _WebServer.Provider = _Provider.Object;

            _WebServer.Online = false;
            Assert.AreEqual(false, _WebServer.Online);
            _Provider.Verify(m => m.StartListener(), Times.Never());
            _Provider.Verify(m => m.StopListener());
        }

        [TestMethod]
        public void WebServer_Online_SetValue_False_Does_Nothing_If_Already_Stopped()
        {
            _WebServer.Online = false;
            _Provider.Verify(m => m.StopListener(), Times.Never());
        }
        #endregion

        #region OnlineChanged Event
        [TestMethod]
        public void WebServer_OnlineChanged_Raised_When_Online_Set()
        {
            _WebServer.OnlineChanged += _OnlineChangedEvent.Handler;
            _OnlineChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual(true, _WebServer.Online); };

            _WebServer.Online = true;
            Assert.AreEqual(1, _OnlineChangedEvent.CallCount);
            Assert.AreSame(_WebServer, _OnlineChangedEvent.Sender);
        }

        [TestMethod]
        public void WebServer_OnlineChanged_Raised_When_Online_Cleared()
        {
            _WebServer.Online = true;
            _WebServer.OnlineChanged += _OnlineChangedEvent.Handler;
            _OnlineChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual(false, _WebServer.Online); };

            _WebServer.Online = false;
            Assert.AreEqual(1, _OnlineChangedEvent.CallCount);
            Assert.AreSame(_WebServer, _OnlineChangedEvent.Sender);
        }

        [TestMethod]
        public void WebServer_OnlineChanged_Only_Called_When_Online_Changes()
        {
            _WebServer.OnlineChanged += _OnlineChangedEvent.Handler;

            _WebServer.Online = true;
            _WebServer.Online = true;
            Assert.AreEqual(1, _OnlineChangedEvent.CallCount);

            _WebServer.Online = false;
            _WebServer.Online = false;
            Assert.AreEqual(2, _OnlineChangedEvent.CallCount);
        }
        #endregion

        #region RequestReceived Events
        [TestMethod]
        public void WebServer_RequestReceived_Raised_When_Provider_Gets_Context()
        {
            ConfigureProviderForGetContext();

            _WebServer.Root = "/ThisIsRoot";

            _WebServer.BeforeRequestReceived += _BeforeRequestReceivedEvent.Handler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.AfterRequestReceived += _AfterRequestReceivedEvent.Handler;
            _WebServer.Online = true;

            Assert.AreEqual(1, _BeforeRequestReceivedEvent.CallCount);
            Assert.AreEqual(1, _RequestReceivedEvent.CallCount);
            Assert.AreEqual(1, _AfterRequestReceivedEvent.CallCount);

            Assert.AreSame(_WebServer, _BeforeRequestReceivedEvent.Sender);
            Assert.AreSame(_WebServer, _RequestReceivedEvent.Sender);
            Assert.AreSame(_WebServer, _AfterRequestReceivedEvent.Sender);

            Assert.AreSame(_Request.Object, _BeforeRequestReceivedEvent.Args.Request);
            Assert.AreSame(_Request.Object, _RequestReceivedEvent.Args.Request);
            Assert.AreSame(_Request.Object, _AfterRequestReceivedEvent.Args.Request);

            Assert.AreSame(_Response.Object, _BeforeRequestReceivedEvent.Args.Response);
            Assert.AreSame(_Response.Object, _RequestReceivedEvent.Args.Response);
            Assert.AreSame(_Response.Object, _AfterRequestReceivedEvent.Args.Response);

            Assert.AreEqual("/ThisIsRoot", _BeforeRequestReceivedEvent.Args.Root);
            Assert.AreEqual("/ThisIsRoot", _RequestReceivedEvent.Args.Root);
            Assert.AreEqual("/ThisIsRoot", _AfterRequestReceivedEvent.Args.Root);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Events_Raised_In_Correct_Order()
        {
            ConfigureProviderForGetContext();

            _WebServer.Root = "/ThisIsRoot";

            _WebServer.BeforeRequestReceived += _BeforeRequestReceivedEvent.Handler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.AfterRequestReceived += _AfterRequestReceivedEvent.Handler;

            _BeforeRequestReceivedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
                Assert.AreEqual(0, _AfterRequestReceivedEvent.CallCount);
            };

            _RequestReceivedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(1, _BeforeRequestReceivedEvent.CallCount);
                Assert.AreEqual(0, _AfterRequestReceivedEvent.CallCount);
            };

            _AfterRequestReceivedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(1, _BeforeRequestReceivedEvent.CallCount);
                Assert.AreEqual(1, _RequestReceivedEvent.CallCount);
            };

            _WebServer.Online = true;
        }

        [TestMethod]
        public void WebServer_AfterRequestReceived_Still_Raised_If_RequestReceived_Handles_Request()
        {
            ConfigureProviderForGetContext();

            _WebServer.Root = "/ThisIsRoot";

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.AfterRequestReceived += _AfterRequestReceivedEvent.Handler;

            _RequestReceivedEvent.EventRaised += (s, a) => { a.Handled = true; };

            _WebServer.Online = true;

            Assert.AreEqual(1, _AfterRequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_And_AfterRequestReceived_Still_Raised_If_BeforeRequestReceived_Handles_Request()
        {
            ConfigureProviderForGetContext();

            _WebServer.Root = "/ThisIsRoot";

            _WebServer.BeforeRequestReceived += _BeforeRequestReceivedEvent.Handler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.AfterRequestReceived += _AfterRequestReceivedEvent.Handler;

            _BeforeRequestReceivedEvent.EventRaised += (s, a) => { a.Handled = true; };

            _WebServer.Online = true;

            Assert.AreEqual(1, _RequestReceivedEvent.CallCount);
            Assert.AreEqual(1, _AfterRequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Not_Raised_If_Provider_Is_Not_Listening()
        {
            // When the listener is stopped it runs the callback but the listener is in a trashed
            // state and throws exceptions, so the callback shouldn't do anything
            ConfigureProviderForGetContext();
            _Provider.Setup(m => m.IsListening).Returns(false);

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
            _Provider.Verify(m => m.EndGetContext(It.IsAny<IAsyncResult>()), Times.Never());
            _Provider.Verify(m => m.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void WebServer_RequestReceived_Gets_Next_Context()
        {
            ConfigureProviderForGetContext();

            _WebServer.Online = true;

            _Provider.Verify(m => m.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_EndGetContext_Throws_HttpListenerException()
        {
            // EndGetContext can throw HttpListenerException in the default implementation when the user disconnects before the request can be processed
            var asyncResult = ConfigureProviderForGetContext();
            _Provider.Setup(m => m.EndGetContext(asyncResult)).Returns((IAsyncResult result) => { throw new HttpListenerException(); });

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            // The request received event should not be raised because we have nothing to give it, but we should still ask for the
            // next context from the provider - if we didn't the server would stop responding
            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
            _Provider.Verify(m => m.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_EndGetContext_Throws_ObjectDisposedException()
        {
            // This can happen during shutdown, the provider can be disposed of by the time the callback is run
            var asyncResult = ConfigureProviderForGetContext();
            _Provider.Setup(m => m.EndGetContext(asyncResult)).Returns((IAsyncResult result) => { throw new ObjectDisposedException(""); });

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            // The request shouldn't be raised and we shouldn't try to get another context either
            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
            _Provider.Verify(m => m.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_BeginGetContext_Throws_ObjectDisposedException()
        {
            // This can happen during shutdown for the same reasons that EndGetContext throws ObjectDisposedException
            var asyncResult = ConfigureProviderForGetContext();
            int callCount = 0;
            _Provider.Setup(m => m.BeginGetContext(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(++callCount == 1) callback(asyncResult);
                    else throw new ObjectDisposedException("");
                    return asyncResult;
                });

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            // It's debatable as to whether we want to process the request - it'll crash when we try to send the response
            // to the user as the server has probably been disposed of.
            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_BeginGetContext_Throws_HttpListenerException()
        {
            // This can happen if the server is taken offline after an EndGetContext and before the corresponding BeginGetContext is called
            var asyncResult = ConfigureProviderForGetContext();
            int callCount = 0;
            _Provider.Setup(m => m.BeginGetContext(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(++callCount == 1) callback(asyncResult);
                    else throw new HttpListenerException();
                    return asyncResult;
                });

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_BeginGetContext_Throws_InvalidOperationException()
        {
            // This can happen if the program is very quickly disconnectng and then reconnecting the server, which can happen
            // during a change of options. The UPnP manager can do this to remove any connections to the server when taking
            // the server off the Internet.
            var asyncResult = ConfigureProviderForGetContext();
            int callCount = 0;
            _Provider.Setup(m => m.BeginGetContext(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(++callCount == 1) callback(asyncResult);
                    else throw new InvalidOperationException("");
                    return asyncResult;
                });

            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Copes_When_Event_Handler_Throws_HttpListenerException()
        {
            // The event handler can end up throwing one of these if the user disconnects while it is trying to stream
            // data to it via the Response object. This should not be reported as an exception, it can happen quite often
            // and it's just exception spam.
            ConfigureProviderForGetContext();

            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => { throw new HttpListenerException(); };
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Closes_OutputStream_When_Event_Handler_Throws_HttpListenerException()
        {
            ConfigureProviderForGetContext();

            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => { throw new HttpListenerException(); };
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;

            _WebServer.Online = true;

            _Response.Verify(r => r.Close(), Times.Once());
        }

        [TestMethod]
        public void WebServer_RequestReceived_Sets_Correct_Status_If_No_Handler_Can_Handle_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(HttpStatusCode.NotFound, _Response.Object.StatusCode);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Closes_Output_Stream_If_No_Handler_Can_Handle_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;

            _WebServer.Online = true;

            _Response.Verify(r => r.Close(), Times.Once());
        }

        [TestMethod]
        public void WebServer_RequestReceived_Leaves_Status_Alone_If_Event_Handler_Handled_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => {
                _Response.Object.StatusCode = HttpStatusCode.ServiceUnavailable;
                args.Handled = true;
            };

            _WebServer.Online = true;

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, _Response.Object.StatusCode);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Leaves_Status_Alone_If_Event_Handler_Redirected_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => {
                _Response.Object.StatusCode = HttpStatusCode.Found;
                args.Handled = true;
            };

            _WebServer.Online = true;

            Assert.AreEqual(HttpStatusCode.Found, _Response.Object.StatusCode);
        }

        [TestMethod]
        public void WebServer_RequestReceived_Closes_OutputStream_If_Handler_Handled_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => {
                _Response.Object.StatusCode = HttpStatusCode.ServiceUnavailable;
                args.Handled = true;
            };

            _WebServer.Online = true;

            _Response.Verify(r => r.Close(), Times.Once());
        }

        [TestMethod]
        public void WebServer_RequestReceived_Throws_RequestException_If_Request_Handler_Throws()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            Exception ex = new InvalidOperationException();
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => { throw ex; };

            _WebServer.Online = true;

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_WebServer, _ExceptionCaughtEvent.Sender);
            Assert.IsInstanceOfType(_ExceptionCaughtEvent.Args.Value, typeof(RequestException));
            var requestException = (RequestException)_ExceptionCaughtEvent.Args.Value;
            Assert.AreSame(ex, requestException.InnerException);
        }
        #endregion

        #region ResponseSent Event
        [TestMethod]
        public void WebServer_ResponseSent_Raised_When_Response_Sent()
        {
            ConfigureProviderForGetContext();
            ConfigureResponse(true, MimeType.Html, "<html></html>", 98767, HttpStatusCode.PartialContent, ContentClassification.Audio, 42);
            _Request.Setup(r => r.RawUrl).Returns("/Root/Folder/Page.html");
            _Request.Setup(r => r.Url).Returns(new Uri("http://Root/Folder/Page.html"));
            _Request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("192.168.0.100"), 54321));
            _WebServer.Root = "/Root";
            _WebServer.ResponseSent += _ResponseSentEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(1, _ResponseSentEvent.CallCount);
            Assert.AreSame(_WebServer, _ResponseSentEvent.Sender);
            Assert.AreEqual(98767, _ResponseSentEvent.Args.BytesSent);
            Assert.AreEqual(ContentClassification.Audio, _ResponseSentEvent.Args.Classification);
            Assert.AreEqual("/Folder/Page.html", _ResponseSentEvent.Args.UrlRequested);
            Assert.AreEqual("192.168.0.100", _ResponseSentEvent.Args.UserAddress);
            Assert.AreEqual("192.168.0.100:54321", _ResponseSentEvent.Args.UserAddressAndPort);
            Assert.AreSame(_Request.Object, _ResponseSentEvent.Args.Request);
            Assert.AreEqual((int)HttpStatusCode.PartialContent, _ResponseSentEvent.Args.HttpStatus);
            Assert.AreEqual(42, _ResponseSentEvent.Args.Milliseconds);
        }

        [TestMethod]
        public void WebServer_ResponseSent_Uses_ReverseProxy_Client_Address_If_Available()
        {
            ConfigureProviderForGetContext();
            ConfigureResponse(true, MimeType.Html, "<html></html>", 98767, HttpStatusCode.PartialContent, ContentClassification.Audio);
            _Request.Setup(r => r.RawUrl).Returns("/Root/Folder/Page.html");
            _Request.Setup(r => r.Url).Returns(new Uri("http://Root/Folder/Page.html"));
            _Request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("192.168.0.100"), 54321));
            var headers = new NameValueCollection();
            _Request.Setup(r => r.Headers).Returns(headers);
            headers.Add("X-Forwarded-For", "1.2.3.4");
            _WebServer.Root = "/Root";
            _WebServer.ResponseSent += _ResponseSentEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual("1.2.3.4", _ResponseSentEvent.Args.UserAddress);
            Assert.AreEqual("1.2.3.4:54321", _ResponseSentEvent.Args.UserAddressAndPort);
        }

        [TestMethod]
        public void WebServer_ResponseSent_Raised_If_Request_Not_Handled()
        {
            ConfigureProviderForGetContext();
            ConfigureResponse(false, null, "");
            _Request.Setup(r => r.RawUrl).Returns("/Root/Folder/Page.html");
            _Request.Setup(r => r.Url).Returns(new Uri("http://Root/Folder/Page.html"));
            _Request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("192.168.0.100"), 54321));
            _WebServer.Root = "/Root";
            _WebServer.ResponseSent += _ResponseSentEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(1, _ResponseSentEvent.CallCount);
            Assert.AreSame(_WebServer, _ResponseSentEvent.Sender);
            Assert.AreEqual(0, _ResponseSentEvent.Args.BytesSent);
            Assert.AreEqual(ContentClassification.Other, _ResponseSentEvent.Args.Classification);
            Assert.AreEqual("/Folder/Page.html", _ResponseSentEvent.Args.UrlRequested);
            Assert.AreEqual("192.168.0.100", _ResponseSentEvent.Args.UserAddress);
            Assert.AreEqual("192.168.0.100:54321", _ResponseSentEvent.Args.UserAddressAndPort);
            Assert.AreSame(_Request.Object, _ResponseSentEvent.Args.Request);
            Assert.AreEqual((int)HttpStatusCode.NotFound, _ResponseSentEvent.Args.HttpStatus);
        }

        [TestMethod]
        public void WebServer_ResponseSent_Raises_ExceptionCaught_If_Handler_Throws()
        {
            ConfigureProviderForGetContext();
            ConfigureResponse(true, MimeType.Html, "<html></html>");
            _WebServer.Root = "/Root";
            _WebServer.ResponseSent += _ResponseSentEvent.Handler;
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;

            var exception = new InvalidOperationException();
            _ResponseSentEvent.EventRaised += (object sender, ResponseSentEventArgs args) => { throw exception; };

            _WebServer.Online = true;

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
        }
        #endregion

        #region ExceptionCaught Event
        [TestMethod]
        public void WebServer_ExceptionCaught_Raised_If_EndGetContext_Throws()
        {
            ConfigureProviderForGetContext();
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            Exception ex = new ApplicationException();
            _Provider.Setup(p => p.EndGetContext(It.IsAny<IAsyncResult>())).Callback(() => { throw ex; });

            _WebServer.Online = true;

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_WebServer, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(ex, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void WebServer_Does_Not_Try_BeginGetContext_Again_If_EndGetContext_Throws()
        {
            ConfigureProviderForGetContext();
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            Exception ex = new ApplicationException();
            _Provider.Setup(p => p.EndGetContext(It.IsAny<IAsyncResult>())).Callback(() => { throw ex; });

            _WebServer.Online = true;

            _Provider.Verify(p => p.BeginGetContext(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void WebServer_ExceptionCaught_Raised_If_Second_BeginGetContext_Throws()
        {
            Exception ex = new ApplicationException();
            int callCount = 0;
            ConfigureProviderForGetContext(2, () => { if(++callCount == 2) throw ex; });
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_WebServer, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(ex, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void WebServer_Does_Not_Raise_RequestReceived_If_Second_BeginGetContext_Throws()
        {
            Exception ex = new ApplicationException();
            int callCount = 0;
            ConfigureProviderForGetContext(2, () => { if(++callCount == 2) throw ex; });
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_OutputStream_Closed_If_Request_Handler_Throws()
        {
            ConfigureProviderForGetContext();
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
            _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            Exception ex = new InvalidOperationException();
            _RequestReceivedEvent.EventRaised += (object sender, RequestReceivedEventArgs args) => { throw ex; };

            _WebServer.Online = true;

            _Response.Verify(r => r.Close(), Times.Once());
        }
        #endregion

        #region AuthenticationRequired Event
        [TestMethod]
        public void WebServer_AuthenticationScheme_Mirrors_Provider_Property()
        {
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Digest;
            Assert.AreEqual(AuthenticationSchemes.Digest, _Provider.Object.AuthenticationSchemes);

            _Provider.Object.AuthenticationSchemes = AuthenticationSchemes.Negotiate;
            Assert.AreEqual(AuthenticationSchemes.Negotiate, _WebServer.AuthenticationScheme);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Raised_When_AuthenticationScheme_Set_To_Basic()
        {
            _Context.Setup(m => m.BasicUserName).Returns("The username");
            _Context.Setup(m => m.BasicPassword).Returns("The password");
            ConfigureProviderForGetContext();

            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(1, _AuthenticationRequiredEvent.CallCount);
            Assert.AreSame(_WebServer, _AuthenticationRequiredEvent.Sender);
            Assert.AreEqual("The username", _AuthenticationRequiredEvent.Args.User);
            Assert.AreEqual("The password", _AuthenticationRequiredEvent.Args.Password);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Not_Raised_When_AuthenticationScheme_Set_To_None()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(0, _AuthenticationRequiredEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Not_Raised_When_AuthenticationScheme_Set_To_Anonymous()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;

            _WebServer.Online = true;

            Assert.AreEqual(0, _AuthenticationRequiredEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Throws_For_Any_Unsupported_AuthenticationScheme()
        {
            foreach(AuthenticationSchemes scheme in Enum.GetValues(typeof(AuthenticationSchemes))) {
                if(scheme == AuthenticationSchemes.None || scheme == AuthenticationSchemes.Anonymous || scheme == AuthenticationSchemes.Basic) continue;

                TestCleanup();
                TestInitialise();
                ConfigureProviderForGetContext();
                _WebServer.AuthenticationScheme = scheme;
                _WebServer.ExceptionCaught -= DefaultExceptionCaughtHandler;
                _WebServer.ExceptionCaught += _ExceptionCaughtEvent.Handler;

                _WebServer.Online = true;

                Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
                Assert.AreSame(_WebServer, _ExceptionCaughtEvent.Sender);
                Assert.IsInstanceOfType(_ExceptionCaughtEvent.Args.Value, typeof(RequestException));
            }
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Prevents_RequestReceived_From_Being_Raised_If_Nothing_Can_Authenticate_Request()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Prevents_RequestReceived_From_Being_Raised_If_Credentials_Are_Wrong()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = false; };

            _WebServer.Online = true;

            Assert.AreEqual(0, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Allows_Raising_Of_RequestReceived_If_Credentials_Are_Right()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _WebServer.RequestReceived += _RequestReceivedEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = true; };

            _WebServer.Online = true;

            Assert.AreEqual(1, _RequestReceivedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Returns_Correct_Response_If_Basic_Authentication_Fails()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Provider.Setup(m => m.ListenerRealm).Returns("TestRealm");

            _WebServer.Online = true;

            _Response.VerifySet(m => m.StatusCode = HttpStatusCode.Unauthorized);
            _Response.Verify(m => m.AddHeader("WWW-Authenticate", @"Basic Realm=""TestRealm"""), Times.Once());
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Closes_OutputStream_If_Basic_Authentication_Fails()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;

            _WebServer.Online = true;

            _Response.Verify(r => r.Close(), Times.Once());
        }

        [TestMethod]
        public void WebServer_AuthenticationRequired_Returns_Correct_Response_If_Basic_Authentication_Passes()
        {
            ConfigureProviderForGetContext();
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Provider.Setup(m => m.ListenerRealm).Returns("TestRealm");
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = true; };

            _WebServer.Online = true;

            // In this case there is nothing responding to the request received event so we should get a 'page not found' status code
            _Response.VerifySet(m => m.StatusCode = HttpStatusCode.NotFound);
            _Response.Verify(m => m.AddHeader("WWW-Authenticate", It.IsAny<string>()), Times.Never());
        }
        #endregion

        #region ExternalAddressChanged Event
        [TestMethod]
        public void WebServer_ExternalAddressChanged_Raised_When_ExternalIPAddress_Changed()
        {
            _WebServer.ExternalAddressChanged += _ExternalAddressChangedEvent.Handler;
            _ExternalAddressChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual("http://1.2.3.4/", _WebServer.ExternalAddress); };

            _WebServer.ExternalIPAddress = "1.2.3.4";

            Assert.AreEqual(1, _ExternalAddressChangedEvent.CallCount);
            Assert.AreSame(_WebServer, _ExternalAddressChangedEvent.Sender);
            Assert.IsNotNull(_ExternalAddressChangedEvent.Args);
        }

        [TestMethod]
        public void WebServer_ExternalAddressChanged_Not_Raised_When_ExternalIPAddress_Not_Changed()
        {
            _WebServer.ExternalIPAddress = "1.2.3.4";
            _WebServer.ExternalAddressChanged += _ExternalAddressChangedEvent.Handler;
            _WebServer.ExternalIPAddress = _WebServer.ExternalIPAddress;

            Assert.AreEqual(0, _ExternalAddressChangedEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_ExternalAddressChanged_Raised_When_ExternalPort_Changed()
        {
            _WebServer.ExternalPort = 77;
            _WebServer.ExternalIPAddress = "1.2.3.4";

            _WebServer.ExternalAddressChanged += _ExternalAddressChangedEvent.Handler;
            _ExternalAddressChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual("http://1.2.3.4:88/", _WebServer.ExternalAddress); };

            _WebServer.ExternalPort = 88;

            Assert.AreEqual(1, _ExternalAddressChangedEvent.CallCount);
            Assert.AreSame(_WebServer, _ExternalAddressChangedEvent.Sender);
            Assert.IsNotNull(_ExternalAddressChangedEvent.Args);
        }

        [TestMethod]
        public void WebServer_ExternalAddressChanged_Not_Raised_When_ExternalPort_Not_Changed()
        {
            _WebServer.ExternalPort = 192;
            _WebServer.ExternalAddressChanged += _ExternalAddressChangedEvent.Handler;
            _WebServer.ExternalPort = _WebServer.ExternalPort;

            Assert.AreEqual(0, _ExternalAddressChangedEvent.CallCount);
        }
        #endregion

        #region CacheCredentials Property and ResetCredentialsCache method
        [TestMethod]
        public void WebServer_CacheCredentials_Set_To_False_Does_Not_Automatically_Allow_Previously_Validated_Credentials()
        {
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = true; };
            _Context.Setup(m => m.BasicUserName).Returns("X");
            _Context.Setup(m => m.BasicPassword).Returns("Y");

            ConfigureProviderForGetContext();
            _WebServer.Online = true;

            ConfigureProviderForGetContext();
            _WebServer.Online = false;
            _WebServer.Online = true;

            Assert.AreEqual(2, _AuthenticationRequiredEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_CacheCredentials_Set_To_True_Automatically_Allows_Previously_Validated_Credentials()
        {
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.CacheCredentials = true;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = true; };
            _Context.Setup(m => m.BasicUserName).Returns("X");
            _Context.Setup(m => m.BasicPassword).Returns("Y");

            ConfigureProviderForGetContext();
            _WebServer.Online = true;

            ConfigureProviderForGetContext();
            _WebServer.Online = false;
            _WebServer.Online = true;

            Assert.AreEqual(1, _AuthenticationRequiredEvent.CallCount);
        }

        [TestMethod]
        public void WebServer_ResetCredentialsCache_Resets_Cache()
        {
            _WebServer.AuthenticationScheme = AuthenticationSchemes.Basic;
            _WebServer.CacheCredentials = true;
            _WebServer.AuthenticationRequired += _AuthenticationRequiredEvent.Handler;
            _AuthenticationRequiredEvent.EventRaised += (object sender, AuthenticationRequiredEventArgs args) => { args.IsAuthenticated = true; };
            _Context.Setup(m => m.BasicUserName).Returns("X");
            _Context.Setup(m => m.BasicPassword).Returns("Y");

            ConfigureProviderForGetContext();
            _WebServer.Online = true;

            _WebServer.ResetCredentialCache();

            ConfigureProviderForGetContext();
            _WebServer.Online = false;
            _WebServer.Online = true;

            Assert.AreEqual(2, _AuthenticationRequiredEvent.CallCount);
        }
        #endregion
    }
}
