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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.Host.Ram;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using Test.VirtualRadar.WebSite.MockOwinMiddleware;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public abstract class ControllerTests
    {
        public TestContext TestContext { get; set; }

        protected IClassFactory _Snapshot;
        protected Mock<IUserManager> _UserManager;
        protected Mock<ISharedConfiguration> _SharedConfiguration;
        protected Configuration _Configuration;

        protected MockAccessFilter _AccessFilter;
        protected MockBasicAuthenticationFilter _BasicAuthenticationFilter;
        protected MockRedirectionFilter _RedirectionFilter;
        protected string _RemoteIpAddress;

        protected IHostRam _RamHost;
        protected IPipelineBuilder _PipelineBuilder;
        protected IPipelineBuilderEnvironment _PipelineBuilderEnvironment;
        protected IWebApiMiddleware _WebApiMiddleware;

        protected OwinContext _Context;
        protected MemoryStream _RequestStream;
        protected MemoryStream _ResponseStream;

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

            _RamHost = Factory.Resolve<IHostRam>();
            _PipelineBuilder = Factory.Resolve<IPipelineBuilder>();
            _PipelineBuilderEnvironment = Factory.Resolve<IPipelineBuilderEnvironment>();
            _WebApiMiddleware = Factory.Resolve<IWebApiMiddleware>();

            _PipelineBuilder.RegisterCallback(UseSetupTestEnvironment, StandardPipelinePriority.Access - 1);
            _PipelineBuilder.RegisterCallback(UseWebApi,               StandardPipelinePriority.WebApi);

            _Context = new OwinContext();
            _Context.RequestHeaders = new HeadersDictionary();
            _Context.ResponseHeaders = new HeadersDictionary();

            _RequestStream = new MemoryStream();
            _ResponseStream = new MemoryStream();
            _Context.RequestBody = _RequestStream;
            _Context.ResponseBody = _ResponseStream;

            _Context.CallCancelled =            new CancellationToken();
            _Context.Version =                  "1.0.0";
            _Context.RequestHost =              "127.0.0.1:80";
            _Context.RequestProtocol =          Formatter.FormatHttpProtocol(HttpProtocol.Http1_1);
            _Context.RequestScheme =            Formatter.FormatHttpScheme(HttpScheme.Http);
            _Context.RequestPathBase =          "/VirtualRadar";
            _Context.ServerLocalIpAddress =     "1.2.3.4";
            _Context.ServerLocalPortNumber =    80;

            ExtraInitialise();

            _RamHost.Initialise(_PipelineBuilder, _PipelineBuilderEnvironment);
            _RamHost.Start();
        }

        protected virtual void ExtraInitialise()
        {
            ;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_RequestStream != null) {
                _RequestStream.Dispose();
                _RequestStream = null;
            }
            if(_ResponseStream != null) {
                _ResponseStream.Dispose();
                _ResponseStream = null;
            }
            if(_RamHost != null) {
                _RamHost.Dispose();
                _RamHost = null;
            }

            ExtraCleanup();

            Factory.RestoreSnapshot(_Snapshot);
        }

        protected virtual void ExtraCleanup()
        {
            ;
        }

        void UseSetupTestEnvironment(IPipelineBuilderEnvironment builderEnv)
        {
            // The intention is for this to get called at the start of the pipeline
            Func<AppFunc, AppFunc> appFuncBuilder = (Func<IDictionary<string, object>, Task> next) =>
            {
                return async(IDictionary<string, object> environment) => {
                    environment["server.RemoteIpAddress"] = _RemoteIpAddress;
                    await next(environment);
                };
            };

            builderEnv.UseMiddlewareBuilder(appFuncBuilder);
        }

        void UseWebApi(IPipelineBuilderEnvironment builderEnv)
        {
            _WebApiMiddleware.AreFormNamesCaseSensitive = false;
            _WebApiMiddleware.AreQueryStringNamesCaseSensitive = false;

            builderEnv.UseMiddlewareBuilder(_WebApiMiddleware.AppFuncBuilder);
        }

        public void SetPathAndQueryString(string pathAndQueryString)
        {
            if(pathAndQueryString.Length > 0 && pathAndQueryString[0] != '/') {
                pathAndQueryString = "/" + pathAndQueryString;
            }

            var separatorIdx = pathAndQueryString.IndexOf('?');
            if(separatorIdx == -1) {
                _Context.Environment[OwinConstants.RequestPath] = pathAndQueryString;
            } else {
                _Context.Environment[OwinConstants.RequestPath] =        pathAndQueryString.Substring(0, separatorIdx);
                _Context.Environment[OwinConstants.RequestQueryString] = pathAndQueryString.Substring(separatorIdx + 1);
            }
        }

        public byte[] Bytes() => _ResponseStream.ToArray();

        public string Text()
        {
            var encoding = _Context.ResponseHeadersDictionary.ContentTypeValue.Encoding;
            var bytes = _ResponseStream.ToArray();

            return encoding.GetString(bytes);
        }

        public T Json<T>() => JsonConvert.DeserializeObject<T>(Text());

        public object Json(Type jsonType) => JsonConvert.DeserializeObject(Text(), jsonType);

        public ControllerTests Get(string pathAndQueryString)
        {
            SetPathAndQueryString(pathAndQueryString);
            _ResponseStream.SetLength(0);
            _Context.RequestMethod = "GET";
            _RamHost.ProcessRequest(_Context.Environment);

            return this;
        }

        public ControllerTests Post(string pathAndQueryString)
        {
            SetPathAndQueryString(pathAndQueryString);
            _ResponseStream.SetLength(0);
            _Context.RequestMethod = "POST";
            _RamHost.ProcessRequest(_Context.Environment);

            return this;
        }

        public ControllerTests PostForm(string pathAndQueryString, string[,] keyValues)
        {
            FormBody(keyValues);

            SetPathAndQueryString(pathAndQueryString);
            _ResponseStream.SetLength(0);
            _Context.RequestMethod = "POST";
            _RamHost.ProcessRequest(_Context.Environment);

            return this;
        }

        public ControllerTests PostJson(string pathAndQueryString, object obj)
        {
            JsonBody(obj);

            SetPathAndQueryString(pathAndQueryString);
            _ResponseStream.SetLength(0);
            _Context.RequestMethod = "POST";
            _RamHost.ProcessRequest(_Context.Environment);

            return this;
        }

        public ControllerTests FormBody(string[,] keyValues)
        {
            if(keyValues == null) {
                throw new ArgumentNullException(nameof(keyValues));
            }
            if(keyValues.GetLength(1) != 2) {
                throw new ArgumentOutOfRangeException($"You must pass a two dimensional array to {nameof(FormBody)}");
            }

            _Context.RequestHeadersDictionary["Content-Type"] = new ContentTypeValue(
                Formatter.FormatMediaType(MediaType.UrlEncodedForm),
                charset: "utf-8"
            ).ToString();

            _Context.RequestBody.SetLength(0);
            for(var i = 0;i < keyValues.GetLength(0);++i) {
                var key = keyValues[i,0];
                var value = keyValues[i,1];
                var prefix = _Context.RequestBody.Length != 0 ? "&" : "";
                var formValue = $"{prefix}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                var bytes = Encoding.UTF8.GetBytes(formValue);
                _Context.RequestBody.Write(bytes, 0, bytes.Length);
            }

            return this;
        }

        public ControllerTests JsonBody(object obj)
        {
            if(obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            var jsonText = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(jsonText);
            _Context.RequestBody.SetLength(0);
            _Context.RequestBody.Write(bytes, 0, bytes.Length);

            _Context.RequestHeadersDictionary["Content-Type"] = new ContentTypeValue(
                Formatter.FormatMediaType(MediaType.Json),
                charset: "utf-8"
            ).ToString();

            return this;
        }
    }
}
