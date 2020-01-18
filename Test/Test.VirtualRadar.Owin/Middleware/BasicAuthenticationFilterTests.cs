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
using System.Net;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class BasicAuthenticationFilterTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IBasicAuthenticationFilter _Filter;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private global::VirtualRadar.Interface.Settings.Configuration _Configuration;
        private Mock<IUserCache> _UserCache;
        private Mock<IUserManager> _UserManager;
        private Mock<IAuthenticationConfiguration> _AuthenticationConfiguration;
        private List<string> _AdministratorPaths;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);

            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();
            _UserCache = TestUtilities.CreateMockImplementation<IUserCache>();
            _UserCache.Setup(r => r.GetUser(It.IsAny<string>())).Returns((CachedUser)null);
            _UserCache.Setup(r => r.GetWebContentUser(It.IsAny<string>())).Returns((CachedUser)null);
            _UserCache.Setup(r => r.GetAdministrator(It.IsAny<string>())).Returns((CachedUser)null);

            _AuthenticationConfiguration = TestUtilities.CreateMockSingleton<IAuthenticationConfiguration>();
            _AdministratorPaths = new List<string>();
            _AuthenticationConfiguration.Setup(r => r.GetAdministratorPaths()).Returns(() => _AdministratorPaths.ToArray());
            _AuthenticationConfiguration.Setup(r => r.IsAdministratorPath(It.IsAny<string>())).Returns((string pathAndFile) => {
                var normalised = (pathAndFile ?? "").ToLower();
                return _AdministratorPaths.Any(r => normalised.StartsWith(r));
            });

            _Filter = Factory.Resolve<IBasicAuthenticationFilter>();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private CachedUser AddUserToCache(string loginName = "default", string password = "password", bool isWebContentUser = true, bool isAdministrator = false)
        {
            if(isAdministrator) {
                isWebContentUser = true;
            }

            var mockUser = TestUtilities.CreateMockInstance<IUser>();
            mockUser.SetupGet(r => r.LoginName).Returns(loginName);

            var cachedUser = new CachedUser(mockUser.Object, isWebContentUser, isAdministrator);
            _UserCache.Object.TagAction(cachedUser);

            _UserCache.Setup(r => r.GetUser(loginName)).Returns(cachedUser);
            if(isWebContentUser) {
                _UserCache.Setup(r => r.GetWebContentUser(loginName)).Returns(cachedUser);
            }
            if(isAdministrator) {
                _UserCache.Setup(r => r.GetAdministrator(loginName)).Returns(cachedUser);
            }

            _UserManager.Setup(r => r.PasswordMatches(mockUser.Object, It.IsAny<string>())).Returns((IUser u, string p) => p == password);

            return cachedUser;
        }

        private void SetAdministratorPath(string pathFromRoot)
        {
            pathFromRoot = (pathFromRoot ?? "").Trim().ToLower();
            if(!pathFromRoot.StartsWith("/")) pathFromRoot = String.Format("/{0}", pathFromRoot);
            if(!pathFromRoot.EndsWith("/")) pathFromRoot = String.Format("{0}/", pathFromRoot);

            _AdministratorPaths.Add(pathFromRoot);
        }

        private void SetGlobalAuthentication(bool enabled)
        {
            _Configuration.WebServerSettings.AuthenticationScheme = enabled ? AuthenticationSchemes.Basic : AuthenticationSchemes.None;
        }

        private void AssertSendCredentialsSent()
        {
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, _Environment.ResponseStatusCode);
            var authoriseHeader = _Environment.ResponseHeaders["WWW-Authenticate"];
            Assert.IsTrue((authoriseHeader ?? "").StartsWith("Basic Realm=\""));
            Assert.IsTrue(authoriseHeader.EndsWith(", charset=\"UTF-8\""));
        }

        private void AssertRequestAllowed()
        {
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            Assert.IsNull(_Environment.ResponseHeaders["WWW-Authenticate"]);
            Assert.IsTrue(
                   _Environment.Context.ResponseStatusCode == null  //  null and 200 are equivalent, if the status code remains
                || _Environment.Context.ResponseStatusCode == 200   //  at zero then eventually the runtime will set it to 200
            );
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Calls_Next_Middleware_When_Authentication_Switched_Off()
        {
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Requests_Credentials_When_Not_Supplied()
        {
            SetGlobalAuthentication(true);

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Requests_Credentials_When_User_Not_Found()
        {
            SetGlobalAuthentication(true);
            AddUserToCache(loginName: "known");
            _Environment.SetBasicCredentials("unknown", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Requests_Credentials_When_Password_Is_Bad()
        {
            SetGlobalAuthentication(true);
            AddUserToCache(loginName: "known", password: "password");
            _Environment.SetBasicCredentials("known", "not password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Continues_When_Password_Is_Good()
        {
            SetGlobalAuthentication(true);
            AddUserToCache(loginName: "known", password: "password");
            _Environment.SetBasicCredentials("known", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Sets_Principal_When_User_Authorised()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            var principal = _Environment.User;
            Assert.IsNotNull(principal);
            Assert.AreEqual("user", principal.Identity.Name);
            Assert.IsTrue(principal.Identity.IsAuthenticated);
            Assert.AreEqual("basic", principal.Identity.AuthenticationType);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Does_Not_Set_Principal_When_User_Not_Authorised()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            _Environment.SetBasicCredentials("user", "not password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
            Assert.IsNull(_Environment.User);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Does_Not_Set_Principal_When_No_Credentials_Supplied()
        {
            SetGlobalAuthentication(true);

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
            Assert.IsNull(_Environment.User);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Does_Not_Set_Principal_When_Unnecessary_Credentials_Supplied()
        {
            SetGlobalAuthentication(false);
            AddUserToCache("user", "password");
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertRequestAllowed();
            Assert.IsNull(_Environment.User);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Sets_Roles_For_WebContent_Users()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("joe", "bloggs");
            _Environment.SetBasicCredentials("joe", "bloggs");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            var principal = _Environment.User;
            Assert.IsNotNull(principal);
            Assert.IsTrue(principal.IsInRole(Roles.User));
            Assert.IsFalse(principal.IsInRole(Roles.Admin));
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Sets_Roles_For_Admin_Users()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("joe", "bloggs", isAdministrator: true);
            _Environment.SetBasicCredentials("joe", "bloggs");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            var principal = _Environment.User;
            Assert.IsNotNull(principal);
            Assert.IsTrue(principal.IsInRole(Roles.User));
            Assert.IsTrue(principal.IsInRole(Roles.Admin));
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Requests_Credentials_When_User_Is_Not_WebContent_User()
        {
            SetGlobalAuthentication(true);
            AddUserToCache(loginName: "known", password: "password", isWebContentUser: false);
            _Environment.SetBasicCredentials("known", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Blocks_Requests_For_Administrator_Paths()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Considers_Empty_Request_Path_To_Be_Root()
        {
            SetAdministratorPath("/");
            _Environment.RequestPath = "";

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Allows_Requests_For_Administrator_Paths_If_Administrator_Credentials_Supplied()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";
            AddUserToCache("admin", "adminPassword", isAdministrator: true);
            _Environment.SetBasicCredentials("admin", "adminPassword");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Sets_Principal_When_Administrator_Credentials_Supplied_For_Administrator_Path()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";
            AddUserToCache("admin", "adminPassword", isAdministrator: true);
            _Environment.SetBasicCredentials("admin", "adminPassword");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            Assert.IsNotNull(_Environment.User);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Blocks_Requests_For_Administrator_Paths_If_WebContentUser_Credentials_Supplied()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";
            AddUserToCache("user", "password", isAdministrator: false);
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Does_Not_Set_Principal_For_Administrator_Paths_If_WebContentUser_Credentials_Supplied()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";
            AddUserToCache("user", "password", isAdministrator: false);
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            Assert.IsNull(_Environment.User);
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Blocks_Requests_For_Administrator_Paths_If_Invalid_Credentials_Supplied()
        {
            SetAdministratorPath("/admin/");
            _Environment.RequestPath = "/admin/index.html";
            AddUserToCache("known", "password", isAdministrator: true);
            _Environment.SetBasicCredentials("notknown", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Only_Hashes_Passwords_Once()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            _UserManager.Verify(r => r.PasswordMatches(It.IsAny<IUser>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Retests_Password_If_Bad()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            _Environment.SetBasicCredentials("user", "password");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            _Environment.SetBasicCredentials("user", "other password");
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            _UserManager.Verify(r => r.PasswordMatches(It.IsAny<IUser>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Request_Blocked_If_Authenticated_User_Supplies_Bad_Password_In_Subsequent_Request()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");

            _Environment.SetBasicCredentials("user", "password");
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();

            _Environment.SetBasicCredentials("user", "other password");
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Expects_Password_To_Be_Encoded_Using_UTF8()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "£money");
            _Environment.SetBasicCredentials("user", "£money");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Administrator_Paths_Take_Precedence_Over_Global_Authentication()
        {
            SetGlobalAuthentication(true);
            SetAdministratorPath("/admin/");
            AddUserToCache("drone", "sector");
            AddUserToCache("admin", "boss", isAdministrator: true);

            _Environment.SetBasicCredentials("drone", "sector");
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();

            _Environment.Reset();
            _Environment.SetBasicCredentials("drone", "sector");
            _Environment.RequestPath = "/admin/index.html";
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertSendCredentialsSent();

            _Environment.Reset();
            _Environment.SetBasicCredentials("admin", "boss");
            _Environment.RequestPath = "/admin/index.html";
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Ignores_Authorization_Headers_That_Are_Not_Basic()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            var mime64Credentials = _Environment.EncodeBasicCredentials("user", "password");
            _Environment.RequestAuthorizationHeader = $"Other {mime64Credentials}";

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);

            AssertSendCredentialsSent();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Accepts_Any_Case_For_Basic_Scheme()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            var mime64Credentials = _Environment.EncodeBasicCredentials("user", "password");

            _Environment.RequestAuthorizationHeader = $"BASIC {mime64Credentials}";
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();

            _Environment.Reset();
            _Environment.RequestAuthorizationHeader = $"basic {mime64Credentials}";
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();
        }

        [TestMethod]
        public void BasicAuthenticationFilter_Accepts_Extra_Space_Between_Scheme_And_Credentials()
        {
            SetGlobalAuthentication(true);
            AddUserToCache("user", "password");
            var mime64Credentials = _Environment.EncodeBasicCredentials("user", "password");

            _Environment.RequestAuthorizationHeader = $"Basic  {mime64Credentials}";
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment);
            AssertRequestAllowed();
        }
    }
}
