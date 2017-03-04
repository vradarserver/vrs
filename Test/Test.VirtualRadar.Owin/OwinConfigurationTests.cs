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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebServer;
using InterfaceFactory;
using Owin;
using Moq;

namespace Test.VirtualRadar.Owin
{
    [TestClass]
    public class OwinConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IOwinConfiguration _OwinConfiguration;
        private Mock<IAppBuilder> _AppBuilder;

        [TestInitialize]
        public void TestInitialise()
        {
            _OwinConfiguration = Factory.Singleton.Resolve<IOwinConfiguration>();
            _AppBuilder = new Mock<IAppBuilder>();
        }

        [TestMethod]
        public void OwinConfiguration_Singleton_Is_Not_Null()
        {
            Assert.IsNotNull(_OwinConfiguration.Singleton);
        }

        [TestMethod]
        public void OwinConfiguration_Singleton_Exposes_A_Static_Object()
        {
            var other = Factory.Singleton.Resolve<IOwinConfiguration>();
            Assert.AreSame(_OwinConfiguration.Singleton, other.Singleton);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OwinConfiguration_AddConfigureCallback_Throws_If_Passed_Null()
        {
            _OwinConfiguration.AddConfigureCallback(null, MiddlewarePriority.Normal);
        }

        [TestMethod]
        public void OwinConfiguration_AddConfigureCallback_Callback_Returns_Handle()
        {
            var callback = new MockConfigureCallback();
            Assert.IsNotNull(_OwinConfiguration.AddConfigureCallback(callback.Callback, MiddlewarePriority.Normal));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OwinConfiguration_RemoveConfigureCallback_Throws_If_Passed_Null()
        {
            _OwinConfiguration.RemoveConfigureCallback(null);
        }

        [TestMethod]
        public void OwinConfiguration_RemoveConfigureCallback_Does_Nothing_If_Passed_Same_Handle_Twice()
        {
            var callback = new MockConfigureCallback();
            var handle = _OwinConfiguration.AddConfigureCallback(callback.Callback, MiddlewarePriority.Normal);

            _OwinConfiguration.RemoveConfigureCallback(handle);
            _OwinConfiguration.RemoveConfigureCallback(handle);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OwinConfiguration_Configure_Throws_If_Passed_Null()
        {
            _OwinConfiguration.Configure(null);
        }

        [TestMethod]
        public void OwinConfiguration_Configure_Calls_Registered_Callback()
        {
            var callback = new MockConfigureCallback();
            _OwinConfiguration.AddConfigureCallback(callback.Callback, MiddlewarePriority.Normal);

            _OwinConfiguration.Configure(_AppBuilder.Object);

            Assert.AreEqual(1, callback.CallCount);
            Assert.AreSame(_AppBuilder.Object, callback.AppBuilder);
        }

        [TestMethod]
        public void OwinConfiguration_Configure_Calls_Registered_Callbacks_In_Correct_Order()
        {
            var firstCallback = new MockConfigureCallback();
            var secondCallback = new MockConfigureCallback();
            var thirdCallback = new MockConfigureCallback();

            _OwinConfiguration.AddConfigureCallback(secondCallback.Callback, MiddlewarePriority.Normal);
            _OwinConfiguration.AddConfigureCallback(thirdCallback.Callback,  MiddlewarePriority.Late);
            _OwinConfiguration.AddConfigureCallback(firstCallback.Callback,  MiddlewarePriority.Early);

            firstCallback.Action = r => {
                Assert.AreEqual(0, secondCallback.CallCount, "2nd callback called before 1st");
                Assert.AreEqual(0, thirdCallback.CallCount, "3rd callback called before 1st");
            };
            secondCallback.Action = r => {
                Assert.AreEqual(1, firstCallback.CallCount, "1st callback not called before 2nd");
                Assert.AreEqual(0, thirdCallback.CallCount, "3rd callback called before 2nd");
            };
            thirdCallback.Action = r => {
                Assert.AreEqual(1, firstCallback.CallCount, "1st callback not called before 3rd");
                Assert.AreEqual(1, secondCallback.CallCount, "2nd callback not called before 3rd");
            };

            _OwinConfiguration.Configure(_AppBuilder.Object);

            Assert.AreEqual(1, firstCallback.CallCount);
            Assert.AreEqual(1, secondCallback.CallCount);
            Assert.AreEqual(1, thirdCallback.CallCount);
        }

        [TestMethod]
        public void OwinConfiguration_Configure_Does_Not_Call_Callbacks_That_Have_Been_Removed()
        {
            var callback = new MockConfigureCallback();
            var handle = _OwinConfiguration.AddConfigureCallback(callback.Callback, MiddlewarePriority.Normal);
            _OwinConfiguration.RemoveConfigureCallback(handle);

            _OwinConfiguration.Configure(_AppBuilder.Object);

            Assert.AreEqual(0, callback.CallCount);
        }
    }
}
