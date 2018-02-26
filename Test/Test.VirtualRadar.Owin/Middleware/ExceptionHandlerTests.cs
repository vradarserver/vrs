// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class ExceptionHandlerTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private Mock<ILog> _Log;
        private IExceptionHandler _ExceptionHandler;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _ExceptionHandler = Factory.Resolve<IExceptionHandler>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void ExceptionHandler_Calls_Next_Middleware()
        {
            _Pipeline.CallMiddleware(_ExceptionHandler.HandleRequest, _Environment.Environment);
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void ExceptionHandler_Records_Exception_Thrown_In_Pipeline()
        {
            var exception = new InvalidOperationException("Hello");
            _Pipeline.NextMiddlewareCallback += r => throw exception;

            _Pipeline.CallMiddleware(_ExceptionHandler.HandleRequest, _Environment.Environment);

            _Log.Verify(r => r.WriteLine(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void ExceptionHandler_Responds_With_Internal_Server_Error_When_Exception_Thrown()
        {
            var exception = new InvalidOperationException("Hello");
            _Pipeline.NextMiddlewareCallback += r => throw exception;

            _Pipeline.CallMiddleware(_ExceptionHandler.HandleRequest, _Environment.Environment);

            Assert.AreEqual(500, _Environment.Response.StatusCode);
        }
    }
}
