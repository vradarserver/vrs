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
using System.Web.Http;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class StandardPipelineTests
    {
        class MiddlewareDetail
        {
            public object Target;               // The Object property of the Mock<T> middleware
            public string AppFuncMethodName;    // The name of the AppFunc middleware method on Target

            public override string ToString()
            {
                return $"{Target?.GetType().Name}.{AppFuncMethodName}";
            }

            public bool IsSameAs(MiddlewareDetail other)
            {
                var result = Object.ReferenceEquals(this, other);
                if(!result) {
                    result = Object.ReferenceEquals(Target, other.Target) &&
                             String.Equals(AppFuncMethodName, other.AppFuncMethodName);
                }

                return result;
            }
        }

        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;
        private IStandardPipeline _StandardPipeline;

        private List<MiddlewareDetail> _ExpectedMiddleware;
        private Mock<IAccessFilter> _MockAccessFilter;
        private Mock<IBasicAuthenticationFilter> _MockBasicAuthenticationFilter;
        private Mock<IRedirectionFilter> _MockRedirectionFilter;
        private Mock<ICorsHandler> _MockCorsHandler;
        private Mock<IResponseStreamWrapper> _MockResponseStreamWrapper;
        private Mock<IFileSystemServer> _MockFileSystemServer;
        private Mock<IImageServer> _MockImageServer;
        private Mock<IAudioServer> _MockAudioServer;
        private MiddlewareDetail _LastMiddlewareBeforeWebApiInit;

        private List<IStreamManipulator> _ExpectedStreamManipulators;
        private Mock<IJavascriptManipulator> _MockJavascriptManipulator;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _ExpectedMiddleware = new List<MiddlewareDetail>();
            _MockAccessFilter =                 CreateMockMiddleware<IAccessFilter>(nameof(IAccessFilter.FilterRequest));
            _MockBasicAuthenticationFilter =    CreateMockMiddleware<IBasicAuthenticationFilter>(nameof(IBasicAuthenticationFilter.FilterRequest));
            _MockRedirectionFilter =            CreateMockMiddleware<IRedirectionFilter>(nameof(IRedirectionFilter.FilterRequest));
            _MockCorsHandler =                  CreateMockMiddleware<ICorsHandler>(nameof(ICorsHandler.HandleRequest));
            _MockResponseStreamWrapper =        CreateMockMiddleware<IResponseStreamWrapper>(nameof(IResponseStreamWrapper.WrapResponseStream));
            _LastMiddlewareBeforeWebApiInit =   _ExpectedMiddleware[_ExpectedMiddleware.Count - 1];
            _MockFileSystemServer =             CreateMockMiddleware<IFileSystemServer>(nameof(IFileSystemServer.HandleRequest));
            _MockImageServer =                  CreateMockMiddleware<IImageServer>(nameof(IImageServer.HandleRequest));
            _MockAudioServer =                  CreateMockMiddleware<IAudioServer>(nameof(IAudioServer.HandleRequest));

            _ExpectedStreamManipulators = new List<IStreamManipulator>();
            _MockJavascriptManipulator = CreateMockStreamManipulator<IJavascriptManipulator>();

            _StandardPipeline = Factory.Singleton.Resolve<IStandardPipeline>();
        }

        private Mock<T> CreateMockMiddleware<T>(string appFuncMethodName)
            where T: class
        {
            var result = TestUtilities.CreateMockImplementation<T>();
            _ExpectedMiddleware.Add(new MiddlewareDetail() {
                Target =            result.Object,
                AppFuncMethodName = appFuncMethodName,
            });

            return result;
        }

        private Mock<T> CreateMockStreamManipulator<T>()
            where T: class, IStreamManipulator
        {
            var result = TestUtilities.CreateMockImplementation<T>();
            _ExpectedStreamManipulators.Add(result.Object);

            return result;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void StandardPipeline_Adds_Standard_Middleware_With_Correct_Priorities()
        {
            var actualStreamManipulators = new List<IStreamManipulator>();
            _MockResponseStreamWrapper.Setup(r => r.Initialise(It.IsAny<IEnumerable<IStreamManipulator>>())).Callback((IEnumerable<IStreamManipulator> manipulators) => {
                actualStreamManipulators.AddRange(manipulators);
            });

            var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _StandardPipeline.Register(webAppConfiguration);

            var actualTargets = new List<MiddlewareDetail>();
            MiddlewareDetail targetBeforeWebApiAdded = null;
            var mockAppBuilder = TestUtilities.CreateMockInstance<IAppBuilder>();
            mockAppBuilder.Setup(r => r.Use(It.IsAny<object>(), It.IsAny<object[]>())).Callback((object middlewareAbstract, object[] parameters) => {
                if(middlewareAbstract is Func<AppFunc, AppFunc> middleware) {
                    actualTargets.Add(new MiddlewareDetail() {
                        Target = middleware.Target,
                        AppFuncMethodName = middleware.Method.Name,
                    });
                } else if((middlewareAbstract as Type)?.FullName.StartsWith("System.Web.Http.") ?? false) {
                    if(targetBeforeWebApiAdded == null) {
                        targetBeforeWebApiAdded = actualTargets.Count > 0 ? actualTargets[actualTargets.Count - 1] : new MiddlewareDetail();
                    }
                }
            });

            webAppConfiguration.Configure(mockAppBuilder.Object);

            // All middleware was added in correct order
            Assert.AreEqual(_ExpectedMiddleware.Count, actualTargets.Count);
            for(var i = 0;i < _ExpectedMiddleware.Count;++i) {
                var expected = _ExpectedMiddleware[i];
                var actual = actualTargets[i];
                Assert.IsTrue(expected.IsSameAs(actual), $"Expected {expected}, got {actual}");
            }

            // The web api was initialised at the right point in the chain
            Assert.IsTrue(_LastMiddlewareBeforeWebApiInit.IsSameAs(targetBeforeWebApiAdded), $"Web API should have been initialised after {_LastMiddlewareBeforeWebApiInit}, was actually after {targetBeforeWebApiAdded}");

            // The response stream wrapper was initialised correctly
            Assert.IsTrue(_ExpectedStreamManipulators.SequenceEqual(actualStreamManipulators));
        }

        [TestMethod]
        public void StandardPipeline_Configures_Microsoft_WebApi()
        {
            var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _StandardPipeline.Register(webAppConfiguration);

            var mockAppBuilder = TestUtilities.CreateMockInstance<IAppBuilder>();
            webAppConfiguration.Configure(mockAppBuilder.Object);

            var httpConfiguration = webAppConfiguration.GetHttpConfiguration();

            Assert.AreEqual(2, httpConfiguration.Routes.Count);
            Assert.AreEqual("", httpConfiguration.Routes[0].RouteTemplate);                         // Added by MapAttributeRoutes
            Assert.AreEqual("api/{controller}/{id}", httpConfiguration.Routes[1].RouteTemplate);    // Added by StandardPipeline
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StandardPipeline_Register_Can_Only_Be_Called_Once()
        {
            var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _StandardPipeline.Register(webAppConfiguration);
            _StandardPipeline.Register(webAppConfiguration);
        }

        [TestMethod]
        public void StandardPipeline_Register_Adds_Standard_StreamManipulators()
        {
            var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _StandardPipeline.Register(webAppConfiguration);

            Assert.IsTrue(_ExpectedStreamManipulators.SequenceEqual(webAppConfiguration.GetStreamManipulators()));
        }
    }
}
