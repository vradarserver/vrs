// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.WebApi;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Default implementation of <see cref="IWebSitePipelineBuilder"/>.
    /// </summary>
    class WebSitePipelineBuilder : IWebSitePipelineBuilder
    {
        /// <summary>
        /// True if <see cref="AddStandardPipelineMiddleware"/> has already been called.
        /// </summary>
        private bool _HasRegistrationBeenDone;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPipelineBuilder PipelineBuilder { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSitePipelineBuilder()
        {
            PipelineBuilder = Factory.Resolve<IPipelineBuilder>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void AddStandardPipelineMiddleware()
        {
            if(!_HasRegistrationBeenDone) {
                _HasRegistrationBeenDone = true;

                PipelineBuilder.RegisterCallback(UseExceptionHandler,              StandardPipelinePriority.LowestVrsMiddlewarePriority);
                PipelineBuilder.RegisterCallback(UseAccessFilter,                  StandardPipelinePriority.Access);
                PipelineBuilder.RegisterCallback(UseBasicAuthenticationFilter,     StandardPipelinePriority.Authentication);
                PipelineBuilder.RegisterCallback(UseRedirectionFilter,             StandardPipelinePriority.Redirection);
                PipelineBuilder.RegisterCallback(UseCorsHandler,                   StandardPipelinePriority.Cors);
                PipelineBuilder.RegisterCallback(UseWebApi,                        StandardPipelinePriority.WebApi);
                PipelineBuilder.RegisterCallback(UseBundlerServer,                 StandardPipelinePriority.BundlerServer);
                PipelineBuilder.RegisterCallback(UseFileSystemServer,              StandardPipelinePriority.FileSystemServer);
                PipelineBuilder.RegisterCallback(UseImageServer,                   StandardPipelinePriority.ImageServer);
                PipelineBuilder.RegisterCallback(UseAudioServer,                   StandardPipelinePriority.AudioServer);

                PipelineBuilder.RegisterCallback(UseHtmlManipulator,               StreamManipulatorPriority.HtmlManipulator);
                PipelineBuilder.RegisterCallback(UseJavaScriptManipulator,         StreamManipulatorPriority.JavascriptManipulator);
                PipelineBuilder.RegisterCallback(UseCompressionManipulator,        StreamManipulatorPriority.CompressionManipulator);

                var htmlManipulatorConfiguration = Factory.ResolveSingleton<IHtmlManipulatorConfiguration>();
                htmlManipulatorConfiguration.AddTextResponseManipulator<IMapPluginHtmlManipulator>();
                htmlManipulatorConfiguration.AddTextResponseManipulator<IBundlerHtmlManipulator>();
            }
        }

        private void UseExceptionHandler(IPipelineBuilderEnvironment builderEnv)
        {
            builderEnv.UseExceptionLogger(new OwinExceptionLogger());
        }

        private void UseAccessFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IAccessFilter>();
            builderEnv.UseMiddlewareBuilder(filter.AppFuncBuilder);
        }

        private void UseBasicAuthenticationFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IBasicAuthenticationFilter>();
            builderEnv.UseMiddlewareBuilder(filter.AppFuncBuilder);
        }

        private void UseRedirectionFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IRedirectionFilter>();
            builderEnv.UseMiddlewareBuilder(filter.AppFuncBuilder);
        }

        private void UseCorsHandler(IPipelineBuilderEnvironment builderEnv)
        {
            var handler = Factory.Resolve<ICorsHandler>();
            builderEnv.UseMiddlewareBuilder(handler.AppFuncBuilder);
        }

        private void UseWebApi(IPipelineBuilderEnvironment builderEnv)
        {
            var webApi = Factory.Resolve<IWebApiMiddleware>();
            webApi.AreFormNamesCaseSensitive = false;
            webApi.AreQueryStringNamesCaseSensitive = false;

            builderEnv.UseMiddlewareBuilder(webApi.AppFuncBuilder);
        }

        private void UseBundlerServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IBundlerServer>();
            builderEnv.UseMiddlewareBuilder(server.AppFuncBuilder);
        }

        private void UseFileSystemServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IFileSystemServer>();
            builderEnv.UseMiddlewareBuilder(server.AppFuncBuilder);
        }

        private void UseImageServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IImageServer>();
            builderEnv.UseMiddlewareBuilder(server.AppFuncBuilder);
        }

        private void UseAudioServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IAudioServer>();
            builderEnv.UseMiddlewareBuilder(server.AppFuncBuilder);
        }

        private void UseHtmlManipulator(IPipelineBuilderEnvironment builderEnv)
        {
            var manipulator = Factory.Resolve<IHtmlManipulator>();
            builderEnv.UseStreamManipulatorBuilder(manipulator.AppFuncBuilder);
        }

        private void UseJavaScriptManipulator(IPipelineBuilderEnvironment builderEnv)
        {
            var manipulator = Factory.Resolve<IJavascriptManipulator>();
            builderEnv.UseStreamManipulatorBuilder(manipulator.AppFuncBuilder);
        }

        private void UseCompressionManipulator(IPipelineBuilderEnvironment builderEnv)
        {
            if(!IsLoopbackHost(builderEnv)) {
                var manipulator = Factory.Resolve<IAutoConfigCompressionManipulator>();
                builderEnv.UseStreamManipulatorBuilder(manipulator.AppFuncBuilder);
            }
        }

        private bool IsLoopbackHost(IPipelineBuilderEnvironment builderEnv)
        {
            builderEnv.Properties.TryGetValue(ApplicationStartupKey.HostType, out var host);
            return host as string == LoopbackHost.HostType;
        }
    }
}
