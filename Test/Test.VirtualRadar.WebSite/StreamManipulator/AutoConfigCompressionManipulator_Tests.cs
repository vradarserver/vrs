// Copyright © 2020 onwards, Andrew Whewell
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
using AWhewell.Owin.Interface;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.StreamManipulator
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class AutoConfigCompressionManipulator_Tests
    {
        private IClassFactory                       _Snapshot;
        private IAutoConfigCompressionManipulator   _Manipulator;
        private MockSharedConfiguration             _SharedConfiguration;
        private Configuration                       _Configuration;
        private Mock<ICompressResponseManipulator>  _WrappedManipulator;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = new Configuration();
            _SharedConfiguration = MockSharedConfiguration.TestInitialise(_Configuration);

            _WrappedManipulator = TestUtilities.CreateMockImplementation<ICompressResponseManipulator>();

            _Manipulator = Factory.Resolve<IAutoConfigCompressionManipulator>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void AppFuncBuilder_Passes_Through_To_Owin_Manipulator()
        {
            #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AppFunc next = async(IDictionary<string, object> env) => { ; };
            AppFunc response = async(IDictionary<string, object> env) => { ; };

            _WrappedManipulator
                .Setup(r => r.AppFuncBuilder(next))
                .Returns(response);

            var actual = _Manipulator.AppFuncBuilder(next);

            Assert.AreSame(response, actual);
            _WrappedManipulator.Verify(r => r.AppFuncBuilder(next), Times.Once());
        }

        [TestMethod]
        public void Wrapped_Enabled_Defaults_To_Current_Configuration_Setting()
        {
            foreach(var enabled in new bool[] { true, false }) {
                _Configuration.GoogleMapSettings.EnableCompression = enabled;

                _WrappedManipulator = TestUtilities.CreateMockImplementation<ICompressResponseManipulator>();
                _Manipulator = Factory.Resolve<IAutoConfigCompressionManipulator>();

                Assert.AreEqual(enabled, _WrappedManipulator.Object.Enabled);
            }
        }

        [TestMethod]
        public void Wrapped_Enabled_Updated_When_Configuration_Changes()
        {
            Assert.AreEqual(_Configuration.GoogleMapSettings.EnableCompression, _WrappedManipulator.Object.Enabled);

            _Configuration.GoogleMapSettings.EnableCompression = !_Configuration.GoogleMapSettings.EnableCompression;
            _SharedConfiguration.RaiseConfigurationChanged();

            Assert.AreEqual(_Configuration.GoogleMapSettings.EnableCompression, _WrappedManipulator.Object.Enabled);
        }
    }
}
