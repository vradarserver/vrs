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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class ResponseStreamWrapperTests
    {
        class StreamManipulator : IStreamManipulator
        {
            public int ResponseStreamPriority { get; set; }

            public int ManipulateCallCount { get; private set; }
            public Action ManipulateCallback { get; set; }
            public void ManipulateResponseStream(IDictionary<string, object> environment)
            {
                ++ManipulateCallCount;
                if(ManipulateCallback != null) {
                    ManipulateCallback();
                }
            }
        }

        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private StreamManipulator _Manipulator;
        private List<IStreamManipulator> _StreamManipulators;
        private IResponseStreamWrapper _Wrapper;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Manipulator = new StreamManipulator();
            _StreamManipulators = new List<IStreamManipulator>();
            _StreamManipulators.Add(_Manipulator);

            _Wrapper = Factory.Resolve<IResponseStreamWrapper>();

            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();

            _Environment.RequestPath = "/whatever.txt";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void InitialiseWrapper()
        {
            _Wrapper.Initialise(_StreamManipulators);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResponseStreamWrapper_Initialise_Throws_If_Passed_Null()
        {
            _Wrapper.Initialise(null);
        }

        [TestMethod]
        public void ResponseStreamWrapper_WrapResponseStream_Always_Calls_Next_Middleware()
        {
            InitialiseWrapper();
            _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void ResponseStreamWrapper_WrapResponseStream_Calls_StreamManipulators_After_Pipeline_Has_Finished()
        {
            InitialiseWrapper();

            using(var originalStream = new MemoryStream()) {
                _Environment.Response.Body = originalStream;

                _Manipulator.ManipulateCallback = () => {
                    var actualStream = _Environment.Response.Body;

                    Assert.IsNotNull(actualStream);
                    Assert.AreNotSame(originalStream, actualStream);
                    Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
                };

                _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);
            }

            Assert.AreEqual(1, _Manipulator.ManipulateCallCount);
        }

        [TestMethod]
        public void ResponseStreamWrapper_WrapResponseStream_Copies_Content_Of_Stream_Back_To_Original_Stream_On_Completion()
        {
            InitialiseWrapper();

            using(var originalStream = new MemoryStream()) {
                _Environment.Response.Body = originalStream;

                _Manipulator.ManipulateCallback = () => {
                    var actualStream = _Environment.Response.Body;
                    actualStream.WriteByte(123);
                };

                _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);

                Assert.AreEqual(1, originalStream.Length);
                originalStream.Position = 0;
                Assert.AreEqual(123, originalStream.ReadByte());
            }
        }

        [TestMethod]
        public void ResponseStreamWrapper_WrapResponseStream_Writes_Correct_ContentLength()
        {
            InitialiseWrapper();

            using(var originalStream = new MemoryStream()) {
                _Environment.Response.Body = originalStream;

                _Manipulator.ManipulateCallback = () => {
                    var actualStream = _Environment.Response.Body;
                    actualStream.WriteByte(123);
                };

                _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);

                Assert.AreEqual(1, _Environment.Response.ContentLength);
            }
        }

        [TestMethod]
        public void ResponseStreamWrapper_WrapResponseStream_Puts_Original_Stream_Back()
        {
            InitialiseWrapper();

            using(var originalStream = new MemoryStream()) {
                _Environment.Response.Body = originalStream;

                _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);

                Assert.AreSame(originalStream, _Environment.Response.Body);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ResponseStreamWrapper_WrapResponseStream_Disposes_Of_Wrapper_Stream()
        {
            InitialiseWrapper();

            Stream actualStream = null;
            _Manipulator.ManipulateCallback = () => {
                actualStream = _Environment.Response.Body;
            };

            _Pipeline.CallMiddleware(_Wrapper.WrapResponseStream, _Environment.Environment);

            // Naive assumption here is that all streams will throw ObjectDisposedException if you write to them after they're disposed
            actualStream.WriteByte(1);
        }
    }
}
