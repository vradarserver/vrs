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
using System.Linq;
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
        class StreamManipulator : IStreamManipulator
        {
            public int ResponseStreamPriority { get; set; }

            public void ManipulateResponseStream(IDictionary<string, object> environment)
            {
            }
        }

        public TestContext TestContext { get; set; }
        private IWebAppConfiguration _Configuration;
        private Mock<IAppBuilder> _AppBuilder;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.Resolve<IWebAppConfiguration>();
            _AppBuilder = new Mock<IAppBuilder>();
        }

        private void AssertRegisteredStreamManipulators(params IStreamManipulator[] expected)
        {
            var actual = _Configuration.GetStreamManipulators();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebAppConfiguration_AddCallback_Throws_If_Passed_Null()
        {
            _Configuration.AddCallback(null, 0);
        }

        [TestMethod]
        public void WebAppConfiguration_AddCallback_Callback_Returns_Handle()
        {
            var callback = new MockConfigureCallback();
            Assert.IsNotNull(_Configuration.AddCallback(callback.Callback, 0));
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
            var handle = _Configuration.AddCallback(callback.Callback, 0);

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
            _Configuration.AddCallback(callback.Callback, 0);

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

            _Configuration.AddCallback(secondCallback.Callback, 0);
            _Configuration.AddCallback(thirdCallback.Callback,  1);
            _Configuration.AddCallback(firstCallback.Callback,  -1);

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
            var handle = _Configuration.AddCallback(callback.Callback, 0);
            _Configuration.RemoveCallback(handle);

            _Configuration.Configure(_AppBuilder.Object);

            Assert.AreEqual(0, callback.CallCount);
        }

        [TestMethod]
        public void WebAppConfiguration_GetHttpConfiguration_Returns_Null_If_Configure_Has_Never_Been_Called()
        {
            Assert.IsNull(_Configuration.GetHttpConfiguration());
        }

        [TestMethod]
        public void WebAppConfiguration_GetHttpConfiguration_Returns_Object_If_Configure_Has_Been_Called()
        {
            _Configuration.Configure(_AppBuilder.Object);
            Assert.IsNotNull(_Configuration.GetHttpConfiguration());
        }

        [TestMethod]
        public void WebAppConfiguration_GetHttpConfiguration_Returns_Same_Object_On_Every_Call()
        {
            _Configuration.Configure(_AppBuilder.Object);

            var instance = _Configuration.GetHttpConfiguration();
            Assert.AreSame(instance, _Configuration.GetHttpConfiguration());
        }

        [TestMethod]
        public void WebAppConfiguration_GetHttpConfiguration_Returns_New_Object_After_Every_Configure_Call()
        {
            _Configuration.Configure(_AppBuilder.Object);
            var instance = _Configuration.GetHttpConfiguration();

            _Configuration.Configure(_AppBuilder.Object);
            Assert.AreNotSame(instance, _Configuration.GetHttpConfiguration());
        }

        [TestMethod]
        public void WebAppConfiguration_AddStreamManipulator_Adds_Manipulator()
        {
            var m1 = new StreamManipulator();
            _Configuration.AddStreamManipulator(m1, 0);

            AssertRegisteredStreamManipulators(m1);
        }

        [TestMethod]
        public void WebAppConfiguration_AddStreamManipulator_Ignores_Double_Adds()
        {
            var m1 = new StreamManipulator();
            _Configuration.AddStreamManipulator(m1, 0);
            _Configuration.AddStreamManipulator(m1, 1);

            AssertRegisteredStreamManipulators(m1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebAppConfiguration_AddStreamManipulator_Throws_If_Passed_Null()
        {
            _Configuration.AddStreamManipulator(null, 0);
        }

        [TestMethod]
        public void WebAppConfiguration_AddStreamManipulator_Sorts_By_Priority()
        {
            var m1 = new StreamManipulator() { ResponseStreamPriority = -1 };
            var m2 = new StreamManipulator() { ResponseStreamPriority = 0 };
            var m3 = new StreamManipulator() { ResponseStreamPriority = 1 };

            _Configuration.AddStreamManipulator(m2, 0);
            _Configuration.AddStreamManipulator(m3, 1);
            _Configuration.AddStreamManipulator(m1, -1);

            AssertRegisteredStreamManipulators(m1, m2, m3);
        }

        [TestMethod]
        public void WebAppConfiguration_RemoveStreamManipulator_Removes_Manipulator()
        {
            var m1 = new StreamManipulator();
            _Configuration.AddStreamManipulator(m1, 0);
            _Configuration.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators();
        }

        [TestMethod]
        public void WebAppConfiguration_RemoveStreamManipulator_Only_Removes_Requested_Manipulator()
        {
            var m1 = new StreamManipulator();
            var m2 = new StreamManipulator();
            _Configuration.AddStreamManipulator(m1, 0);
            _Configuration.AddStreamManipulator(m2, 1);
            _Configuration.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators(m2);
        }

        [TestMethod]
        public void WebAppConfiguration_RemoveStreamManipulator_Allows_Double_Removal()
        {
            var m1 = new StreamManipulator();
            _Configuration.AddStreamManipulator(m1, 0);
            _Configuration.RemoveStreamManipulator(m1);
            _Configuration.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators();
        }
    }
}
