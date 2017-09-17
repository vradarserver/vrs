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
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class ResponseStreamWrapperConfigurationTests
    {
        class StreamManipulator : IStreamManipulator
        {
            public int ResponseStreamPriority { get; set; }

            public void ManipulateResponseStream(IDictionary<string, object> environment)
            {
            }
        }

        public TestContext TestContext { get; set; }

        private IResponseStreamWrapperConfiguration _Config;

        [TestInitialize]
        public void TestInitialise()
        {
            _Config = Factory.Singleton.ResolveNewInstance<IResponseStreamWrapperConfiguration>();
        }

        private void AssertRegisteredStreamManipulators(params IStreamManipulator[] expected)
        {
            var actual = _Config.GetStreamManipulators();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_AddStreamManipulator_Adds_Manipulator()
        {
            var m1 = new StreamManipulator();
            _Config.AddStreamManipulator(m1);

            AssertRegisteredStreamManipulators(m1);
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_AddStreamManipulator_Ignores_Double_Adds()
        {
            var m1 = new StreamManipulator();
            _Config.AddStreamManipulator(m1);
            _Config.AddStreamManipulator(m1);

            AssertRegisteredStreamManipulators(m1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResponseStreamWrapperConfiguration_AddStreamManipulator_Throws_If_Passed_Null()
        {
            AssertRegisteredStreamManipulators(null);
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_AddStreamManipulator_Sorts_By_Priority()
        {
            var m1 = new StreamManipulator() { ResponseStreamPriority = -1 };
            var m2 = new StreamManipulator() { ResponseStreamPriority = 0 };
            var m3 = new StreamManipulator() { ResponseStreamPriority = 1 };

            _Config.AddStreamManipulator(m2);
            _Config.AddStreamManipulator(m3);
            _Config.AddStreamManipulator(m1);

            AssertRegisteredStreamManipulators(m1, m2, m3);
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_RemoveStreamManipulator_Removes_Manipulator()
        {
            var m1 = new StreamManipulator();
            _Config.AddStreamManipulator(m1);
            _Config.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators();
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_RemoveStreamManipulator_Only_Removes_Requested_Manipulator()
        {
            var m1 = new StreamManipulator();
            var m2 = new StreamManipulator();
            _Config.AddStreamManipulator(m1);
            _Config.AddStreamManipulator(m2);
            _Config.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators(m2);
        }

        [TestMethod]
        public void ResponseStreamWrapperConfiguration_RemoveStreamManipulator_Allows_Double_Removal()
        {
            var m1 = new StreamManipulator();
            _Config.AddStreamManipulator(m1);
            _Config.RemoveStreamManipulator(m1);
            _Config.RemoveStreamManipulator(m1);

            AssertRegisteredStreamManipulators();
        }
    }
}
