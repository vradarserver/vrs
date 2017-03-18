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
using InterfaceFactory;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using Test.VirtualRadar.WebSite.MockOwinMiddleware;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public abstract class ControllerTests
    {
        protected IClassFactory _Snapshot;
        protected Mock<IUserManager> _UserManager;
        protected Mock<ISharedConfiguration> _SharedConfiguration;
        protected Configuration _Configuration;

        protected MockAccessFilter _AccessFilter;
        protected MockBasicAuthenticationFilter _BasicAuthenticationFilter;
        protected MockRedirectionFilter _RedirectionFilter;

        protected TestServer _Server;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();

            _AccessFilter = MockAccessFilter.CreateAndRegister();
            _BasicAuthenticationFilter = MockBasicAuthenticationFilter.CreateAndRegister();
            _RedirectionFilter = MockRedirectionFilter.CreateAndRegister();

            ExtraInitialise();

            _Server = TestServer.Create(app => {
                var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>().Singleton;
                webAppConfiguration.Configure(app);
            });
        }

        protected virtual void ExtraInitialise()
        {
            ;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_Server != null) {
                _Server.Dispose();
                _Server = null;
            }

            ExtraCleanup();

            Factory.RestoreSnapshot(_Snapshot);
        }

        protected virtual void ExtraCleanup()
        {
            ;
        }
    }
}
