using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Owin;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Owin.Middleware;

namespace VirtualRadar.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Registers the standard pipeline with an OWIN web app.
    /// </summary>
    static class StandardPipeline
    {
        /// <summary>
        /// Registers all of the standard pipeline middleware.
        /// </summary>
        /// <param name="webAppConfiguration"></param>
        public static void Register(IWebAppConfiguration webAppConfiguration)
        {
            webAppConfiguration.AddCallback(UseAuthentication, (MiddlewarePriority)StandardPipelinePriority.Authentication);
        }

        private static void UseAuthentication(IAppBuilder app)
        {
            var filter = Factory.Singleton.Resolve<IBasicAuthenticationFilter>();
            var middleware = new Func<AppFunc, AppFunc>(filter.FilterRequest);
            app.Use(middleware);
        }
    }
}
