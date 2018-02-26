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
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class PipelineConfigurationTests
    {
        class Pipeline : IPipeline
        {
            public int RegisterCallCount;

            public void Register(IWebAppConfiguration webAppConfiguration)
            {
                ++RegisterCallCount;
            }
        }

        class AnotherPipeline : IPipeline
        {
            public void Register(IWebAppConfiguration webAppConfiguration)
            {
            }
        }

        class NotPipeline
        {
            public void Register(IWebAppConfiguration webAppConfiguration)
            {
            }
        }

        public TestContext TestContext { get; set; }

        private IPipelineConfiguration _Config;

        [TestInitialize]
        public void TestInitialise()
        {
            _Config = Factory.ResolveNewInstance<IPipelineConfiguration>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PipelineConfiguration_AddPipeline_Type_Throws_If_Passed_Null()
        {
            _Config.AddPipeline(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PipelineConfiguration_AddPipeline_Type_Throws_If_Passed_Non_Pipeline_Type()
        {
            _Config.AddPipeline(typeof(NotPipeline));
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Type_Records_Pipeline_Type()
        {
            _Config.AddPipeline(typeof(Pipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Type_Records_Many_Pipeline_Types()
        {
            _Config.AddPipeline(typeof(Pipeline));
            _Config.AddPipeline(typeof(AnotherPipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(2, types.Length);
            Assert.IsTrue(types.Any(r => r == typeof(Pipeline)));
            Assert.IsTrue(types.Any(r => r == typeof(AnotherPipeline)));
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Type_Ignores_Double_Adds()
        {
            _Config.AddPipeline(typeof(Pipeline));
            _Config.AddPipeline(typeof(Pipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Generic_Records_Pipeline_Type()
        {
            _Config.AddPipeline<Pipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Generic_Records_Many_Pipeline_Types()
        {
            _Config.AddPipeline<Pipeline>();
            _Config.AddPipeline<AnotherPipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(2, types.Length);
            Assert.IsTrue(types.Any(r => r == typeof(Pipeline)));
            Assert.IsTrue(types.Any(r => r == typeof(AnotherPipeline)));
        }

        [TestMethod]
        public void PipelineConfiguration_AddPipeline_Generic_Ignores_Double_Adds()
        {
            _Config.AddPipeline<Pipeline>();
            _Config.AddPipeline<Pipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PipelineConfiguration_RemovePipeline_Type_Throws_If_Passed_Null()
        {
            _Config.RemovePipeline(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PipelineConfiguration_RemovePipeline_Type_Throws_If_Passed_Non_Pipeline_Type()
        {
            _Config.RemovePipeline(typeof(NotPipeline));
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Type_Removes_Pipeline_Type()
        {
            _Config.AddPipeline(typeof(Pipeline));
            _Config.RemovePipeline(typeof(Pipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(0, types.Length);
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Type_Only_Removes_Specified_Type()
        {
            _Config.AddPipeline(typeof(Pipeline));
            _Config.AddPipeline(typeof(AnotherPipeline));

            _Config.RemovePipeline(typeof(AnotherPipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Type_Ignores_Double_Removes()
        {
            _Config.RemovePipeline(typeof(Pipeline));

            var types = _Config.GetPipelines();
            Assert.AreEqual(0, types.Length);
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Generic_Removes_Pipeline_Type()
        {
            _Config.AddPipeline<Pipeline>();
            _Config.RemovePipeline<Pipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(0, types.Length);
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Generic_Only_Removes_Specified_Type()
        {
            _Config.AddPipeline<Pipeline>();
            _Config.AddPipeline<AnotherPipeline>();

            _Config.RemovePipeline<AnotherPipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(1, types.Length);
            Assert.AreEqual(typeof(Pipeline), types[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_RemovePipeline_Generic_Ignores_Double_Removes()
        {
            _Config.RemovePipeline<Pipeline>();

            var types = _Config.GetPipelines();
            Assert.AreEqual(0, types.Length);
        }

        [TestMethod]
        public void PipelineConfiguration_CreatePipelines_Returns_Empty_Array_If_Nothing_Registered()
        {
            Assert.AreEqual(0, _Config.CreatePipelines().Length);
        }

        [TestMethod]
        public void PipelineConfiguration_CreatePipelines_Returns_Array_Of_Pipelines()
        {
            _Config.AddPipeline<Pipeline>();

            var pipelines = _Config.CreatePipelines();
            Assert.AreEqual(1, pipelines.Length);
            Assert.IsTrue(pipelines[0] is Pipeline);
        }

        [TestMethod]
        public void PipelineConfiguration_CreatePipelines_Creates_New_Instances_On_Every_Call()
        {
            _Config.AddPipeline<Pipeline>();

            var firstCall = _Config.CreatePipelines();
            var secondCall = _Config.CreatePipelines();
            Assert.AreNotSame(firstCall[0], secondCall[0]);
        }

        [TestMethod]
        public void PipelineConfiguration_CreatePipelines_Does_Not_Call_Register()
        {
            _Config.AddPipeline<Pipeline>();

            var pipelines = _Config.CreatePipelines();
            Assert.AreEqual(0, ((Pipeline)pipelines[0]).RegisterCallCount);
        }
    }
}
