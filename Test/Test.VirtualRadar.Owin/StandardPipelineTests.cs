// Copyright © 2018 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin
{
    [TestClass]
    public class StandardPipelineTests
    {
        class Pipeline : IPipeline
        {
            public int RegisterCallCount;
            public IWebAppConfiguration RegisterWebApp;

            public void Register(IWebAppConfiguration webAppConfiguration)
            {
                RegisterWebApp = webAppConfiguration;
                ++RegisterCallCount;
            }
        }

        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private List<Pipeline> _RegisteredPipelines;
        private Mock<IPipelineConfiguration> _Config;
        private Mock<IWebAppConfiguration> _WebAppConfiguration;
        private IStandardPipeline _StandardPipeline;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Config = TestUtilities.CreateMockSingleton<IPipelineConfiguration>();
            _RegisteredPipelines = new List<Pipeline>();
            _Config.Setup(r => r.CreatePipelines()).Returns(() => _RegisteredPipelines.ToArray());

            _WebAppConfiguration = TestUtilities.CreateMockInstance<IWebAppConfiguration>();

            _StandardPipeline = Factory.Resolve<IStandardPipeline>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void StandardPipeline_Register_Passes_WebAppConfiguration_To_All_Registered_Pipelines()
        {
            var pipeline = new Pipeline();
            _RegisteredPipelines.Add(pipeline);

            _StandardPipeline.Register(_WebAppConfiguration.Object);

            Assert.AreEqual(1, pipeline.RegisterCallCount);
            Assert.AreSame(_WebAppConfiguration.Object, pipeline.RegisterWebApp);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StandardPipeline_Register_Can_Only_Be_Called_Once()
        {
            _StandardPipeline.Register(_WebAppConfiguration.Object);
            _StandardPipeline.Register(_WebAppConfiguration.Object);
        }
    }
}
