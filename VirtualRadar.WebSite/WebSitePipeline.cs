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
using System.Web.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Owin;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;
using System.Net.Http.Formatting;

namespace VirtualRadar.WebSite
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Registers the standard pipeline for the web site.
    /// </summary>
    public class WebSitePipeline : IPipeline
    {
        private IWebAppConfiguration _WebAppConfiguration;

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="webAppConfiguration"></param>
        public void Register(IWebAppConfiguration webAppConfiguration)
        {
            _WebAppConfiguration = webAppConfiguration;

            webAppConfiguration.AddCallback(UseExceptionHandler,            StandardPipelinePriority.Exception);
            webAppConfiguration.AddCallback(UseAccessFilter,                StandardPipelinePriority.Access);
            webAppConfiguration.AddCallback(UseBasicAuthenticationFilter,   StandardPipelinePriority.Authentication);
            webAppConfiguration.AddCallback(UseRedirectionFilter,           StandardPipelinePriority.Redirection);
            webAppConfiguration.AddCallback(UseCorsHandler,                 StandardPipelinePriority.Cors);
            webAppConfiguration.AddCallback(UseResponseStreamWrapper,       StandardPipelinePriority.ResponseStreamWrapper);

            webAppConfiguration.AddCallback(ConfigureHttpConfiguration,     StandardPipelinePriority.WebApiConfiguration);
            webAppConfiguration.AddCallback(UseWebApi,                      StandardPipelinePriority.WebApi);

            webAppConfiguration.AddCallback(UseBundlerServer,               StandardPipelinePriority.BundlerServer);
            webAppConfiguration.AddCallback(UseFileSystemServer,            StandardPipelinePriority.FileSystemServer);
            webAppConfiguration.AddCallback(UseImageServer,                 StandardPipelinePriority.ImageServer);
            webAppConfiguration.AddCallback(UseAudioServer,                 StandardPipelinePriority.AudioServer);

            webAppConfiguration.AddStreamManipulator(Factory.Resolve<IHtmlManipulator>(),       StreamManipulatorPriority.HtmlManipulator);
            webAppConfiguration.AddStreamManipulator(Factory.Resolve<IJavascriptManipulator>(), StreamManipulatorPriority.JavascriptManipulator);

            Factory.Resolve<IHtmlManipulatorConfiguration>().AddTextResponseManipulator<IBundlerHtmlManipulator>();
        }

        private void UseExceptionHandler(IAppBuilder app)
        {
            var handler = Factory.Resolve<IExceptionHandler>();
            var middleware = new Func<AppFunc, AppFunc>(handler.HandleRequest);
            app.Use(middleware);
        }

        private void UseAccessFilter(IAppBuilder app)
        {
            var filter = Factory.Resolve<IAccessFilter>();
            var middleware = new Func<AppFunc, AppFunc>(filter.FilterRequest);
            app.Use(middleware);
        }

        private void UseBasicAuthenticationFilter(IAppBuilder app)
        {
            var filter = Factory.Resolve<IBasicAuthenticationFilter>();
            var middleware = new Func<AppFunc, AppFunc>(filter.FilterRequest);
            app.Use(middleware);
        }

        private void UseRedirectionFilter(IAppBuilder app)
        {
            var filter = Factory.Resolve<IRedirectionFilter>();
            var middleware = new Func<AppFunc, AppFunc>(filter.FilterRequest);
            app.Use(middleware);
        }

        private void UseCorsHandler(IAppBuilder app)
        {
            var handler = Factory.Resolve<ICorsHandler>();
            var middleware = new Func<AppFunc, AppFunc>(handler.HandleRequest);
            app.Use(middleware);
        }

        private void UseResponseStreamWrapper(IAppBuilder app)
        {
            var wrapper = Factory.Resolve<IResponseStreamWrapper>();
            wrapper.Initialise(_WebAppConfiguration.GetStreamManipulators());

            var middleware = new Func<AppFunc, AppFunc>(wrapper.WrapResponseStream);
            app.Use(middleware);
        }

        private void ConfigureHttpConfiguration(IAppBuilder app)
        {
            var configuration = _WebAppConfiguration.GetHttpConfiguration();
            configuration.MapHttpAttributeRoutes();
            configuration.Routes.MapHttpRoute(
                name:           "DefaultApi",
                routeTemplate:  "api/{controller}/{id}",
                defaults:       new { id = RouteParameter.Optional }
            );

            var basicAuthenticationWebApiHandler = (System.Net.Http.DelegatingHandler)Factory.Resolve<IBasicAuthenticationWebApiMessageHandler>();
            configuration.MessageHandlers.Add(basicAuthenticationWebApiHandler);

            configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(
                new RequestHeaderMapping(
                    "Accept",
                    "text/html",
                    StringComparison.OrdinalIgnoreCase,
                    true,
                    "application/json"
                )
            );
        }

        private void UseWebApi(IAppBuilder app)
        {
            var configuration = _WebAppConfiguration.GetHttpConfiguration();
            app.UseWebApi(configuration);
        }

        private void UseBundlerServer(IAppBuilder app)
        {
            var server = Factory.Resolve<IBundlerServer>();
            var middleware = new Func<AppFunc, AppFunc>(server.HandleRequest);
            app.Use(middleware);
        }

        private void UseFileSystemServer(IAppBuilder app)
        {
            var server = Factory.Resolve<IFileSystemServer>();
            var middleware = new Func<AppFunc, AppFunc>(server.HandleRequest);
            app.Use(middleware);
        }

        private void UseImageServer(IAppBuilder app)
        {
            var server = Factory.Resolve<IImageServer>();
            var middleware = new Func<AppFunc, AppFunc>(server.HandleRequest);
            app.Use(middleware);
        }

        private void UseAudioServer(IAppBuilder app)
        {
            var server = Factory.Resolve<IAudioServer>();
            var middleware = new Func<AppFunc, AppFunc>(server.HandleRequest);
            app.Use(middleware);
        }
    }
}
