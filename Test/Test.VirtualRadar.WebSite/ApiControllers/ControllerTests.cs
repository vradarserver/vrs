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
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using Test.VirtualRadar.WebSite.MockOwinMiddleware;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public abstract class ControllerTests
    {
        protected class GarbageServer
        {
            // Just a bunch of dumb crap to get the tests compiling. I will sort out the controller tests later.

            public System.Net.Http.HttpClient HttpClient { get; }
        }

        public TestContext TestContext { get; set; }

        protected IClassFactory _Snapshot;
        protected Mock<IUserManager> _UserManager;
        protected Mock<ISharedConfiguration> _SharedConfiguration;
        protected Configuration _Configuration;

        protected MockAccessFilter _AccessFilter;
        protected MockBasicAuthenticationFilter _BasicAuthenticationFilter;
        protected MockRedirectionFilter _RedirectionFilter;
        protected string _RemoteIpAddress;

        protected GarbageServer _Server;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();

            _RemoteIpAddress = "127.0.0.1";
            _AccessFilter = MockAccessFilter.CreateAndRegister();
            _BasicAuthenticationFilter = MockBasicAuthenticationFilter.CreateAndRegister();
            _RedirectionFilter = MockRedirectionFilter.CreateAndRegister();

            /*
            _WebAppConfiguration = Factory.Resolve<IWebAppConfiguration>();
            _WebAppConfiguration.AddCallback(UsetTestEnvironmentSetup,   StandardPipelinePriority.Access - 1);
            _WebAppConfiguration.AddCallback(ConfigureHttpConfiguration, StandardPipelinePriority.WebApiConfiguration);
            _WebAppConfiguration.AddCallback(UseWebApi,                  StandardPipelinePriority.WebApi);
            */

            ExtraInitialise();

            /*
            _Server = TestServer.Create(app => {
                _WebAppConfiguration.Configure(app);
            });
            */
        }

        protected virtual void ExtraInitialise()
        {
            ;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //if(_Server != null) {
            //    _Server.Dispose();
            //    _Server = null;
            //}

            ExtraCleanup();

            Factory.RestoreSnapshot(_Snapshot);
        }

        protected virtual void ExtraCleanup()
        {
            ;
        }

        //private void ConfigureHttpConfiguration(IAppBuilder app)
        //{
        //    var configuration = _WebAppConfiguration.GetHttpConfiguration();
        //    configuration.MapHttpAttributeRoutes();
        //    configuration.Routes.MapHttpRoute(
        //        name:           "DefaultApi",
        //        routeTemplate:  "api/{controller}/{id}",
        //        defaults:       new { id = RouteParameter.Optional }
        //    );
        //}

        //private void UseWebApi(IAppBuilder app)
        //{
        //    var configuration = _WebAppConfiguration.GetHttpConfiguration();
        //    app.UseWebApi(configuration);
        //}

        //void UsetTestEnvironmentSetup(IAppBuilder app)
        //{
        //    // The intention is for this to get called at the start of the pipeline
        //    Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> middleware = 
        //    (Func<IDictionary<string, object>, Task> next) => {
        //        Func<IDictionary<string, object>, Task> appFunc = async(IDictionary<string, object> environment) => {
        //            SetEnvironmentValue(environment, "server.RemoteIpAddress", _RemoteIpAddress);

        //            await next.Invoke(environment);
        //        };

        //        return appFunc;
        //    };
        //    app.Use(middleware);
        //}

        //void SetEnvironmentValue<T>(IDictionary<string, object> environment, string key, T value)
        //{
        //    if(!environment.ContainsKey(key)) {
        //        environment.Add(key, value);
        //    } else {
        //        environment[key] = value;
        //    }
        //}
    }
}
