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

                PipelineBuilder.RegisterMiddlewareBuilder(UseExceptionHandler,              StandardPipelinePriority.Exception);
                PipelineBuilder.RegisterMiddlewareBuilder(UseAccessFilter,                  StandardPipelinePriority.Access);
                PipelineBuilder.RegisterMiddlewareBuilder(UseBasicAuthenticationFilter,     StandardPipelinePriority.Authentication);
                PipelineBuilder.RegisterMiddlewareBuilder(UseRedirectionFilter,             StandardPipelinePriority.Redirection);
                PipelineBuilder.RegisterMiddlewareBuilder(UseCorsHandler,                   StandardPipelinePriority.Cors);
                PipelineBuilder.RegisterMiddlewareBuilder(UseWebApiMiddleware,              StandardPipelinePriority.WebApi);
                PipelineBuilder.RegisterMiddlewareBuilder(UseBundlerServer,                 StandardPipelinePriority.BundlerServer);
                PipelineBuilder.RegisterMiddlewareBuilder(UseFileSystemServer,              StandardPipelinePriority.FileSystemServer);
                PipelineBuilder.RegisterMiddlewareBuilder(UseImageServer,                   StandardPipelinePriority.ImageServer);
                PipelineBuilder.RegisterMiddlewareBuilder(UseAudioServer,                   StandardPipelinePriority.AudioServer);

                PipelineBuilder.RegisterMiddlewareBuilder(UseHtmlManipulator,               StreamManipulatorPriority.HtmlManipulator);
                PipelineBuilder.RegisterMiddlewareBuilder(UseJavaScriptManipulator,         StreamManipulatorPriority.JavascriptManipulator);

                var htmlManipulatorConfiguration = Factory.ResolveSingleton<IHtmlManipulatorConfiguration>();
                htmlManipulatorConfiguration.AddTextResponseManipulator<IMapPluginHtmlManipulator>();
                htmlManipulatorConfiguration.AddTextResponseManipulator<IBundlerHtmlManipulator>();
            }
        }

        private void UseExceptionHandler(IPipelineBuilderEnvironment builderEnv)
        {
            var handler = Factory.Resolve<IExceptionHandler>();
            builderEnv.UseMiddleware(handler.HandleRequest);
        }

        private void UseAccessFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IAccessFilter>();
            builderEnv.UseMiddleware(filter.FilterRequest);
        }

        private void UseBasicAuthenticationFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IBasicAuthenticationFilter>();
            builderEnv.UseMiddleware(filter.FilterRequest);
        }

        private void UseRedirectionFilter(IPipelineBuilderEnvironment builderEnv)
        {
            var filter = Factory.Resolve<IRedirectionFilter>();
            builderEnv.UseMiddleware(filter.FilterRequest);
        }

        private void UseCorsHandler(IPipelineBuilderEnvironment builderEnv)
        {
            var handler = Factory.Resolve<ICorsHandler>();
            builderEnv.UseMiddleware(handler.HandleRequest);
        }

        private void UseWebApiMiddleware(IPipelineBuilderEnvironment builderEnv)
        {
            var middleware = Factory.Resolve<IWebApiMiddleware>();
            middleware.AreFormNamesCaseSensitive = false;
            middleware.AreQueryStringNamesCaseSensitive = false;

            builderEnv.UseMiddleware(middleware.CreateMiddleware);
        }

        private void UseBundlerServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IBundlerServer>();
            builderEnv.UseMiddleware(server.HandleRequest);
        }

        private void UseFileSystemServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IFileSystemServer>();
            builderEnv.UseMiddleware(server.HandleRequest);
        }

        private void UseImageServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IImageServer>();
            builderEnv.UseMiddleware(server.HandleRequest);
        }

        private void UseAudioServer(IPipelineBuilderEnvironment builderEnv)
        {
            var server = Factory.Resolve<IAudioServer>();
            builderEnv.UseMiddleware(server.HandleRequest);
        }

        private void UseHtmlManipulator(IPipelineBuilderEnvironment builderEnv)
        {
            var manipulator = Factory.Resolve<IHtmlManipulator>();
            builderEnv.UseStreamManipulator(manipulator.CreateMiddleware);
        }

        private void UseJavaScriptManipulator(IPipelineBuilderEnvironment builderEnv)
        {
            var manipulator = Factory.Resolve<IJavascriptManipulator>();
            builderEnv.UseStreamManipulator(manipulator.CreateMiddleware);
        }
    }
}
