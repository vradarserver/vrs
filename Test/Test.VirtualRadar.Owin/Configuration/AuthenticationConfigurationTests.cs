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
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class AuthenticationConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IAuthenticationConfiguration _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.ResolveNewInstance<IAuthenticationConfiguration>();
        }

        [TestMethod]
        public void AuthenticationConfiguration_GetAdministratorPaths_Is_Initially_Empty()
        {
            Assert.AreEqual(0, _Configuration.GetAdministratorPaths().Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_GetAdministratorPaths_Shows_Paths_Added()
        {
            _Configuration.AddAdministratorPath("/path/");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(1, paths.Length);
            Assert.AreEqual("/path/", paths[0], ignoreCase: true);
        }

        [TestMethod]
        public void AuthenticationConfiguration_GetAdministratorPaths_Does_Not_Show_Paths_Removed()
        {
            _Configuration.AddAdministratorPath("/path");
            _Configuration.RemoveAdministratorPath("/path");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(0, paths.Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Treats_Null_As_Root()
        {
            _Configuration.AddAdministratorPath(null);

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(1, paths.Length);
            Assert.AreEqual("/", paths[0]);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Does_Not_Add_Same_Path_Twice()
        {
            _Configuration.AddAdministratorPath("/abc/");
            _Configuration.AddAdministratorPath("/abc/");

            Assert.AreEqual(1, _Configuration.GetAdministratorPaths().Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Is_Case_Insensitive()
        {
            _Configuration.AddAdministratorPath("/abc/");
            _Configuration.AddAdministratorPath("/ABC/");

            Assert.AreEqual(1, _Configuration.GetAdministratorPaths().Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Adds_Leading_Slash()
        {
            _Configuration.AddAdministratorPath("MyPlugin/");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(1, paths.Length);
            Assert.AreEqual("/MyPlugin/", paths[0], ignoreCase: true);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Adds_Trailing_Slash()
        {
            _Configuration.AddAdministratorPath("/MyPlugin");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(1, paths.Length);
            Assert.AreEqual("/MyPlugin/", paths[0], ignoreCase: true);
        }

        [TestMethod]
        public void AuthenticationConfiguration_AddAdministratorPath_Does_Not_Add_Slashes_To_Root()
        {
            _Configuration.AddAdministratorPath("/");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(1, paths.Length);
            Assert.AreEqual("/", paths[0], ignoreCase: true);
        }

        [TestMethod]
        public void AuthenticationConfiguration_RemoveAdministratorPath_Adds_Leading_Slash()
        {
            _Configuration.AddAdministratorPath("/MyPlugin/");
            _Configuration.RemoveAdministratorPath("MyPlugin/");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(0, paths.Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_RemoveAdministratorPath_Adds_Trailing_Slash()
        {
            _Configuration.AddAdministratorPath("/MyPlugin/");
            _Configuration.RemoveAdministratorPath("/MyPlugin");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(0, paths.Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_RemoveAdministratorPath_Does_Not_Add_Slashes_To_Root()
        {
            _Configuration.AddAdministratorPath("/");
            _Configuration.RemoveAdministratorPath("/");

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(0, paths.Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_RemoveAdministratorPath_Treats_Null_As_Root()
        {
            _Configuration.AddAdministratorPath("/");
            _Configuration.RemoveAdministratorPath(null);

            var paths = _Configuration.GetAdministratorPaths();

            Assert.AreEqual(0, paths.Length);
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Returns_False_If_Not_Registered()
        {
            Assert.IsFalse(_Configuration.IsAdministratorPath("/path/"));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Returns_True_If_Registered()
        {
            _Configuration.AddAdministratorPath("/path/");
            Assert.IsTrue(_Configuration.IsAdministratorPath("/path/"));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Is_Case_Insensitive()
        {
            _Configuration.AddAdministratorPath("/path/");
            Assert.IsTrue(_Configuration.IsAdministratorPath("/PATH/"));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Does_Not_Add_Leading_Slashes()
        {
            _Configuration.AddAdministratorPath("/path/");
            Assert.IsFalse(_Configuration.IsAdministratorPath("path/"));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Does_Not_Add_Trailing_Slashes()
        {
            _Configuration.AddAdministratorPath("/path/");
            Assert.IsFalse(_Configuration.IsAdministratorPath("/path"));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Does_Not_Convert_Null_To_Root()
        {
            _Configuration.AddAdministratorPath("/");
            Assert.IsFalse(_Configuration.IsAdministratorPath(null));
        }

        [TestMethod]
        public void AuthenticationConfiguration_IsAdministratorPath_Returns_True_If_Path_Starts_With_Administrative_Path()
        {
            _Configuration.AddAdministratorPath("/path/");
            Assert.IsTrue(_Configuration.IsAdministratorPath("/path/file"));
        }
    }
}
