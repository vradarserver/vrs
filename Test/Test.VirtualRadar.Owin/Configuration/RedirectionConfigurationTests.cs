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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class RedirectionConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IRedirectionConfiguration _Configuration;
        private RedirectionRequestContext _Context;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.ResolveNewInstance<IRedirectionConfiguration>();
            _Context = new RedirectionRequestContext();
        }

        [TestMethod]
        public void RedirectionConfiguration_AddConfiguration_Is_Not_Case_Sensitive()
        {
            _Configuration.AddRedirection("/path/", "/foo", RedirectionContext.Any);
            Assert.AreEqual("/foo", _Configuration.RedirectToPathFromRoot("/PATH/", _Context));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_AddConfiguration_Throws_If_Path_Is_Null()
        {
            _Configuration.AddRedirection(null, "/foo", RedirectionContext.Any);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_AddConfiguration_Throws_If_Path_Is_Empty()
        {
            _Configuration.AddRedirection("", "/foo", RedirectionContext.Any);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_AddConfiguration_Throws_If_To_Path_Is_Null()
        {
            _Configuration.AddRedirection("/foo", null, RedirectionContext.Any);
        }

        [TestMethod]
        public void RedirectionConfiguration_AddConfiguration_Can_Overwrite_Entries()
        {
            _Configuration.AddRedirection("/", "/foo", RedirectionContext.Any);
            _Configuration.AddRedirection("/", "/new", RedirectionContext.Any);

            Assert.AreEqual("/new", _Configuration.RedirectToPathFromRoot("/", _Context));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_AddConfiguration_Throws_If_To_Path_Is_Empty()
        {
            _Configuration.AddRedirection("/foo", "", RedirectionContext.Any);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_RemoveConfiguration_Throws_If_Path_Is_Null()
        {
            _Configuration.RemoveRedirection(null, RedirectionContext.Any);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedirectionConfiguration_RemoveConfiguration_Throws_If_Path_Is_Empty()
        {
            _Configuration.RemoveRedirection("", RedirectionContext.Any);
        }

        [TestMethod]
        public void RedirectionConfiguration_RemoveConfiguration_Is_Not_Case_Sensitive()
        {
            _Configuration.AddRedirection("/path/", "/foo", RedirectionContext.Any);
            _Configuration.RemoveRedirection("/PATH/", RedirectionContext.Any);
            Assert.IsNull(_Configuration.RedirectToPathFromRoot("/path/", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RemoveConfiguration_Only_Removes_Matching_Redirections()
        {
            _Context.IsMobile = true;
            _Configuration.AddRedirection("/", "not", RedirectionContext.Any);
            _Configuration.AddRedirection("/", "mob", RedirectionContext.Mobile);

            _Configuration.RemoveRedirection("/", RedirectionContext.Mobile);

            Assert.AreEqual("not", _Configuration.RedirectToPathFromRoot("/", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Null_If_No_Redirection_Configured()
        {
            Assert.IsNull(_Configuration.RedirectToPathFromRoot("/", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Path_When_Redirection_Configured()
        {
            _Configuration.AddRedirection("/", "/index.html", RedirectionContext.Any);
            Assert.AreEqual("/index.html", _Configuration.RedirectToPathFromRoot("/", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Ignores_Redirections_That_Have_Been_Removed()
        {
            _Configuration.AddRedirection("/", "/index.html", RedirectionContext.Any);
            _Configuration.RemoveRedirection("/", RedirectionContext.Any);
            Assert.IsNull(_Configuration.RedirectToPathFromRoot("/", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Null_If_Request_Not_For_Same_Path()
        {
            _Configuration.AddRedirection("/", "/index.html", RedirectionContext.Any);
            Assert.IsNull(_Configuration.RedirectToPathFromRoot("/index.html", _Context));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Correct_Redirection_For_Context()
        {
            _Configuration.AddRedirection("/", "/mob.html", RedirectionContext.Mobile);
            _Configuration.AddRedirection("/", "/not.html", RedirectionContext.Any);

            Assert.AreEqual("/mob.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = true }));
            Assert.AreEqual("/not.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = false }));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Correct_Redirection_Regardless_Of_Registration_Order()
        {
            _Configuration.AddRedirection("/", "/mob.html", RedirectionContext.Mobile);
            _Configuration.AddRedirection("/", "/not.html", RedirectionContext.Any);

            Assert.AreEqual("/mob.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = true }));
            Assert.AreEqual("/not.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = false }));

            _Configuration = Factory.ResolveNewInstance<IRedirectionConfiguration>();
            _Configuration.AddRedirection("/", "/not.html", RedirectionContext.Any);
            _Configuration.AddRedirection("/", "/mob.html", RedirectionContext.Mobile);

            Assert.AreEqual("/mob.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = true }));
            Assert.AreEqual("/not.html", _Configuration.RedirectToPathFromRoot("/", new RedirectionRequestContext() { IsMobile = false }));
        }

        [TestMethod]
        public void RedirectionConfiguration_RedirectToPathFromRoot_Returns_Null_For_Configurations_Deleted_After_Overwritten()
        {
            _Configuration.AddRedirection("/", "/old", RedirectionContext.Any);
            _Configuration.AddRedirection("/", "/new", RedirectionContext.Any);
            _Configuration.RemoveRedirection("/", RedirectionContext.Any);

            Assert.IsNull(_Configuration.RedirectToPathFromRoot("/", _Context));
        }
    }
}
