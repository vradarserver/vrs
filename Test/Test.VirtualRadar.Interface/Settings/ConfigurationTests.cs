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
using System.Text;
using Test.VirtualRadar;
using VirtualRadar.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using Test.Framework;
using InterfaceFactory;
using Moq;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class ConfigurationTests
    {
        private IClassFactory _ClassFactorySnapshot;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Configuration _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();

            _Implementation = new Configuration();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void Configuration_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsNotNull(_Implementation.BaseStationSettings);
            Assert.IsNotNull(_Implementation.FlightRouteSettings);
            Assert.IsNotNull(_Implementation.WebServerSettings);
            Assert.IsNotNull(_Implementation.GoogleMapSettings);
            Assert.IsNotNull(_Implementation.VersionCheckSettings);
            Assert.IsNotNull(_Implementation.InternetClientSettings);
            Assert.IsNotNull(_Implementation.RawDecodingSettings);
            Assert.IsNotNull(_Implementation.MonoSettings);

            Assert.AreEqual(0, _Implementation.ReceiverLocations.Count);
            Assert.AreEqual(0, _Implementation.RebroadcastSettings.Count);
            Assert.AreEqual(0, _Implementation.Receivers.Count);

            TestUtilities.TestProperty(_Implementation, r => r.BaseStationSettings, _Implementation.BaseStationSettings, new BaseStationSettings());
            TestUtilities.TestProperty(_Implementation, r => r.FlightRouteSettings, _Implementation.FlightRouteSettings, new FlightRouteSettings());
            TestUtilities.TestProperty(_Implementation, r => r.WebServerSettings, _Implementation.WebServerSettings, new WebServerSettings());
            TestUtilities.TestProperty(_Implementation, r => r.GoogleMapSettings, _Implementation.GoogleMapSettings, new GoogleMapSettings());
            TestUtilities.TestProperty(_Implementation, r => r.VersionCheckSettings, _Implementation.VersionCheckSettings, new VersionCheckSettings());
            TestUtilities.TestProperty(_Implementation, r => r.InternetClientSettings, _Implementation.InternetClientSettings, new InternetClientSettings());
            TestUtilities.TestProperty(_Implementation, r => r.RawDecodingSettings, _Implementation.RawDecodingSettings, new RawDecodingSettings());
        }

        [TestMethod]
        public void Configuration_ReceiverLocation_Returns_ReceiverLocation_From_ReceiverLocations()
        {
            var home = new ReceiverLocation() { UniqueId = 1, Name = "HOME" };
            var away = new ReceiverLocation() { UniqueId = 2, Name = "AWAY" };
            _Implementation.ReceiverLocations.AddRange(new ReceiverLocation[] { home, away });

            var location = _Implementation.ReceiverLocation(2);
            Assert.AreSame(away, location);
        }

        [TestMethod]
        public void Configuration_ReceiverLocation_Returns_Null_If_Receiver_Cannot_Be_Found()
        {
            var home = new ReceiverLocation() { UniqueId = 1, Name = "HOME" };
            var away = new ReceiverLocation() { UniqueId = 2, Name = "AWAY" };
            _Implementation.ReceiverLocations.AddRange(new ReceiverLocation[] { home, away });

            var location = _Implementation.ReceiverLocation(3);
            Assert.IsNull(location);
        }
    }
}
