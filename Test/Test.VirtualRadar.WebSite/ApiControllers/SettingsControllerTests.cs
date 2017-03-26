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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class SettingsControllerTests : ControllerTests
    {
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<IApplicationInformation> _ApplicationInformation;

        protected override void ExtraInitialise()
        {
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
        }

        [TestMethod]
        public async Task SettingsController_GetServerConfig_Returns_Environment_For_Correct_Route_And_Local_Address()
        {
            _RemoteIpAddress = "127.0.0.1";

            var response = await _Server.HttpClient.GetAsync("/api/1.00/settings/server");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ServerConfigJson>(content);

            Assert.AreEqual(true, json.IsLocalAddress);
        }

        [TestMethod]
        public async Task SettingsController_GetServerConfig_Returns_Environment_For_Correct_Route_And_Internet_Address()
        {
            _RemoteIpAddress = "1.2.3.4";

            var response = await _Server.HttpClient.GetAsync("/api/1.00/settings/server");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ServerConfigJson>(content);

            Assert.AreEqual(false, json.IsLocalAddress);
        }

        [TestMethod]
        public async Task SettingsController_GetServerConfig_Returns_Environment_For_Legacy_Route()
        {
            var response = await _Server.HttpClient.GetAsync("/ServerConfig.json");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ServerConfigJson>(content);

            Assert.IsNotNull(json);
        }
    }
}
