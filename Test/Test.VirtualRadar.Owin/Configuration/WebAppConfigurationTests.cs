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
using VirtualRadar.Interface.Owin;
using InterfaceFactory;
using Owin;
using Moq;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class WebAppConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IWebAppConfiguration _Configuration;
        private Mock<IAppBuilder> _AppBuilder;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _AppBuilder = new Mock<IAppBuilder>();
        }

        [TestMethod]
        public void WebAppConfiguration_Singleton_Is_Not_Null()
        {
            Assert.IsNotNull(_Configuration.Singleton);
        }

        [TestMethod]
        public void WebAppConfiguration_Singleton_Exposes_A_Static_Object()
        {
            var other = Factory.Singleton.Resolve<IWebAppConfiguration>();
            Assert.AreSame(_Configuration.Singleton, other.Singleton);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebAppConfiguration_AddCallback_Throws_If_Passed_Null()
        {
            _Configuration.AddCallback(null, MiddlewarePriority.Normal);
        }

        [TestMethod]
        public void WebAppConfiguration_AddCallback_Callback_Returns_Handle()
        {
            var callback = new MockConfigureCallback();
            Assert.IsNotNull(_Configuration.AddCallback(callback.Callback, MiddlewarePriority.Normal));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebAppConfiguration_RemoveCallback_Throws_If_Passed_Null()
        {
            _Configuration.RemoveCallback(null);
        }

        [TestMethod]
        public void WebAppConfiguration_RemoveCallback_Does_Nothing_If_Passed_Same_Handle_Twice()
        {
            var callback = new MockConfigureCallback();
            var handle = _Configuration.AddCallback(callback.Callback, MiddlewarePriority.Normal);

            _Configuration.RemoveCallback(handle);
            _Configuration.RemoveCallback(handle);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebAppConfiguration_Configure_Throws_If_Passed_Null()
        {
            _Configuration.Configure(null);
        }

        [TestMethod]
        public void WebAppConfiguration_Configure_Calls_Registered_Callback()
        {
            var callback = new MockConfigureCallback();
            _Configuration.AddCallback(callback.Callback, MiddlewarePriority.Normal);

            _Configuration.Configure(_AppBuilder.Object);

            Assert.AreEqual(1, callback.CallCount);
            Assert.AreSame(_AppBuilder.Object, callback.AppBuilder);
        }

        [TestMethod]
        public void WebAppConfiguration_Configure_Calls_Registered_Callbacks_In_Correct_Order()
        {
            var firstCallback = new MockConfigureCallback();
            var secondCallback = new MockConfigureCallback();
            var thirdCallback = new MockConfigureCallback();

            _Configuration.AddCallback(secondCallback.Callback, MiddlewarePriority.Normal);
            _Configuration.AddCallback(thirdCallback.Callback,  MiddlewarePriority.Late);
            _Configuration.AddCallback(firstCallback.Callback,  MiddlewarePriority.Early);

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

            _Configuration.Configure(_AppBuilder.Object);

            Assert.AreEqual(1, firstCallback.CallCount);
            Assert.AreEqual(1, secondCallback.CallCount);
            Assert.AreEqual(1, thirdCallback.CallCount);
        }

        [TestMethod]
        public void WebAppConfiguration_Configure_Does_Not_Call_Callbacks_That_Have_Been_Removed()
        {
            var callback = new MockConfigureCallback();
            var handle = _Configuration.AddCallback(callback.Callback, MiddlewarePriority.Normal);
            _Configuration.RemoveCallback(handle);

            _Configuration.Configure(_AppBuilder.Object);

            Assert.AreEqual(0, callback.CallCount);
        }
    }
}
