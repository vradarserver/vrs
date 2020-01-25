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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.WebApi;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.WebSite;

namespace Test.VirtualRadar.WebSite
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class WebSitePipelineBuilder_Tests
    {
        class RegisteredType
        {
            public Type MiddlewareType { get; set; }
            public int? Priority       { get; set; }
        }

        private IClassFactory                           _Snapshot;
        private IWebSitePipelineBuilder                 _Builder;

        private Mock<IPipelineBuilderEnvironment>       _PipelineBuilderEnvironment;
        private Mock<IPipelineBuilder>                  _PipelineBuilder;
        private int?                                    _Priority;
        private Type                                    _MiddlewareType;
        private List<RegisteredType>                    _RegisteredMiddlewareTypes;
        private List<RegisteredType>                    _RegisteredStreamManipulatorTypes;

        private Mock<IAccessFilter>                     _AccessFilter;
        private Mock<IAutoConfigCompressionManipulator> _AutoConfigCompressionManipulator;
        private Mock<IBasicAuthenticationFilter>        _BasicAuthenticationFilter;
        private Mock<IRedirectionFilter>                _RedirectionFilter;
        private Mock<ICorsHandler>                      _CorsHandler;
        private Mock<IWebApiMiddleware>                 _WebApiMiddleware;
        private Mock<IBundlerServer>                    _BundlerServer;
        private Mock<IFileSystemServer>                 _FileSystemServer;
        private Mock<IImageServer>                      _ImageServer;
        private Mock<IAudioServer>                      _AudioServer;
        private Mock<IHtmlManipulator>                  _HtmlManipulator;
        private Mock<IJavascriptManipulator>            _JavascriptManipulator;

        private Mock<IHtmlManipulatorConfiguration>     _HtmlManipulatorConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _AccessFilter =                     CreateMockMiddleware<IAccessFilter>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _AutoConfigCompressionManipulator = CreateMockMiddleware<IAutoConfigCompressionManipulator>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _BasicAuthenticationFilter =        CreateMockMiddleware<IBasicAuthenticationFilter>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _RedirectionFilter =                CreateMockMiddleware<IRedirectionFilter>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _CorsHandler =                      CreateMockMiddleware<ICorsHandler>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _BundlerServer =                    CreateMockMiddleware<IBundlerServer>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _WebApiMiddleware =                 CreateMockMiddleware<IWebApiMiddleware>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _FileSystemServer =                 CreateMockMiddleware<IFileSystemServer>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _ImageServer =                      CreateMockMiddleware<IImageServer>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _AudioServer =                      CreateMockMiddleware<IAudioServer>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));

            _HtmlManipulator =                  CreateMockMiddleware<IHtmlManipulator>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));
            _JavascriptManipulator =            CreateMockMiddleware<IJavascriptManipulator>(r => r.AppFuncBuilder(It.IsAny<AppFunc>()));

            _HtmlManipulatorConfiguration =     TestUtilities.CreateMockImplementation<IHtmlManipulatorConfiguration>();

            _RegisteredMiddlewareTypes = new List<RegisteredType>();
            _RegisteredStreamManipulatorTypes = new List<RegisteredType>();
            _MiddlewareType = null;
            _Priority = null;

            _PipelineBuilder = TestUtilities.CreateMockImplementation<IPipelineBuilder>();
            _PipelineBuilder.Setup(r => r.RegisterCallback(It.IsAny<Action<IPipelineBuilderEnvironment>>(), It.IsAny<int>()))
                .Callback((Action<IPipelineBuilderEnvironment> action, int priority) => {
                    // The action would normally be called when the pipeline is created. We call it
                    // here so that we can figure out which bit of middleware is being created and
                    // the priority associated with it.
                    _Priority = priority;
                    action(_PipelineBuilderEnvironment.Object);
                }
            );

            _PipelineBuilderEnvironment = TestUtilities.CreateMockImplementation<IPipelineBuilderEnvironment>();
            _PipelineBuilderEnvironment.Setup(r => r.UseMiddlewareBuilder(It.IsAny<Func<AppFunc, AppFunc>>()))
                .Callback((Func<AppFunc, AppFunc> appFuncBuilder) => {
                    appFuncBuilder(null);
                    if(_MiddlewareType != null && _Priority != null) {
                        _RegisteredMiddlewareTypes.Add(new RegisteredType() {
                            MiddlewareType = _MiddlewareType,
                            Priority =       _Priority.Value,
                        });
                        _MiddlewareType = null;
                        _Priority = null;
                    }
                }
            );
            _PipelineBuilderEnvironment.Setup(r => r.UseStreamManipulatorBuilder(It.IsAny<Func<AppFunc, AppFunc>>()))
                .Callback((Func<AppFunc, AppFunc> appFuncBuilder) => {
                    appFuncBuilder(null);
                    if(_MiddlewareType != null && _Priority != null) {
                        _RegisteredStreamManipulatorTypes.Add(new RegisteredType() {
                            MiddlewareType = _MiddlewareType,
                            Priority =       _Priority.Value,
                        });
                        _MiddlewareType = null;
                        _Priority = null;
                    }
                }
            );

            _Builder = Factory.ResolveNewInstance<IWebSitePipelineBuilder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private Mock<T> CreateMockMiddleware<T>(Expression<Action<T>> createAppFuncMethod)
            where T: class
        {
            var result = TestUtilities.CreateMockImplementation<T>();

            result
                .Setup(createAppFuncMethod)
                .Callback(() => _MiddlewareType = typeof(T));

            return result;
        }

        [TestMethod]
        public void Ctor_Fills_PipelineBuilder()
        {
            Assert.IsNotNull(_Builder.PipelineBuilder);
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Adds_Standard_Middleware_With_Correct_Priorities()
        {
            _Builder.AddStandardPipelineMiddleware();

            AssertMiddlewareRegistered(typeof(IAccessFilter),               StandardPipelinePriority.Access);
            AssertMiddlewareRegistered(typeof(IBasicAuthenticationFilter),  StandardPipelinePriority.Authentication);
            AssertMiddlewareRegistered(typeof(IRedirectionFilter),          StandardPipelinePriority.Redirection);
            AssertMiddlewareRegistered(typeof(ICorsHandler),                StandardPipelinePriority.Cors);
            AssertMiddlewareRegistered(typeof(IWebApiMiddleware),           StandardPipelinePriority.WebApi);
            AssertMiddlewareRegistered(typeof(IBundlerServer),              StandardPipelinePriority.BundlerServer);
            AssertMiddlewareRegistered(typeof(IFileSystemServer),           StandardPipelinePriority.FileSystemServer);
            AssertMiddlewareRegistered(typeof(IImageServer),                StandardPipelinePriority.ImageServer);
            AssertMiddlewareRegistered(typeof(IAudioServer),                StandardPipelinePriority.AudioServer);
        }

        private void AssertMiddlewareRegistered(Type middlewareType, int expectedPriority)
        {
            var entry = _RegisteredMiddlewareTypes.SingleOrDefault(r => r.MiddlewareType == middlewareType);

            Assert.IsNotNull(entry, $"{middlewareType.Name} has no builder registered for it");
            Assert.AreEqual(expectedPriority, entry.Priority, $"The {middlewareType.Name} builder has the wrong priority");
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Adds_Standard_Stream_Manipulators_With_Correct_Priorities()
        {
            _Builder.AddStandardPipelineMiddleware();

            AssertStreamManipulatorRegistered(typeof(IAutoConfigCompressionManipulator),    StreamManipulatorPriority.CompressionManipulator);
            AssertStreamManipulatorRegistered(typeof(IHtmlManipulator),                     StreamManipulatorPriority.HtmlManipulator);
            AssertStreamManipulatorRegistered(typeof(IJavascriptManipulator),               StreamManipulatorPriority.JavascriptManipulator);
        }

        private void AssertStreamManipulatorRegistered(Type streamManipulatorType, int expectedPriority)
        {
            var entry = _RegisteredStreamManipulatorTypes.SingleOrDefault(r => r.MiddlewareType == streamManipulatorType);

            Assert.IsNotNull(entry, $"{streamManipulatorType.Name} has no builder registered for it");
            Assert.AreEqual(expectedPriority, entry.Priority, $"The {streamManipulatorType.Name} builder has the wrong priority");
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Configures_Middleware_Correctly()
        {
            _Builder.AddStandardPipelineMiddleware();

            Assert.AreEqual(false, _WebApiMiddleware.Object.AreFormNamesCaseSensitive);
            Assert.AreEqual(false, _WebApiMiddleware.Object.AreQueryStringNamesCaseSensitive);
            Assert.AreEqual(0,     _WebApiMiddleware.Object.DefaultFormatters.Count);
            Assert.AreEqual(0,     _WebApiMiddleware.Object.DefaultParsers.Count);
        }

        [TestMethod]
        public void AddStandardPiplineMiddleware_Configures_HtmlManipulatorConfiguration()
        {
            _Builder.AddStandardPipelineMiddleware();

            _HtmlManipulatorConfiguration.Verify(r => r.AddTextResponseManipulator<IMapPluginHtmlManipulator>(), Times.Once());
            _HtmlManipulatorConfiguration.Verify(r => r.AddTextResponseManipulator<IBundlerHtmlManipulator>(),   Times.Once());
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Ignores_Second_And_Subsequent_Calls()
        {
            // The Assert() methods check that the middleware and stream manipulators have only been added once, so calling
            // the tests twice will throw an exception if the function is erroneously registering on the 2nd and subsequent
            // calls
            AddStandardPipelineMiddleware_Adds_Standard_Middleware_With_Correct_Priorities();
            AddStandardPipelineMiddleware_Adds_Standard_Stream_Manipulators_With_Correct_Priorities();
            AddStandardPiplineMiddleware_Configures_HtmlManipulatorConfiguration();

            // These will fail if the function isn't guarding against double calls
            AddStandardPipelineMiddleware_Adds_Standard_Middleware_With_Correct_Priorities();
            AddStandardPipelineMiddleware_Adds_Standard_Stream_Manipulators_With_Correct_Priorities();
            AddStandardPiplineMiddleware_Configures_HtmlManipulatorConfiguration();
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Registers_An_OwinExceptionHandler()
        {
            _Builder.AddStandardPipelineMiddleware();

            _PipelineBuilderEnvironment.Verify(r => r.UseExceptionLogger(It.IsAny<OwinExceptionLogger>()), Times.Once());
        }

        [TestMethod]
        public void AddStandardPipelineMiddleware_Does_Not_Add_Compression_To_The_VRS_LoopbackHost()
        {
            _PipelineBuilderEnvironment.Object.Properties["server.HostType"] = "VRS.LoopbackHost";

            _Builder.AddStandardPipelineMiddleware();

            var entry = _RegisteredStreamManipulatorTypes.SingleOrDefault(r => r.MiddlewareType == typeof(IAutoConfigCompressionManipulator));
            Assert.IsNull(entry);
        }
    }
}
