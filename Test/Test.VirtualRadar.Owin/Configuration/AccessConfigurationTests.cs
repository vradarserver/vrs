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
    public class AccessConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IAccessConfiguration _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.Singleton.ResolveNewInstance<IAccessConfiguration>();
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Records_Access_To_Path()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("/MyPlugin/", map.Single().Key);
            Assert.AreEqual(access, map.Single().Value);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Allows_Overwrites()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            access = new Access() { DefaultAccess = DefaultAccess.Allow };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("/MyPlugin/", map.Single().Key);
            Assert.AreEqual(access, map.Single().Value);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Allows_Removal_By_Passing_UnrestrictAccess_Access_Object()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            access = new Access() { DefaultAccess = DefaultAccess.Unrestricted };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Allows_Removal_By_Passing_Null_Access_Object()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MyPlugin/", access);

            access = new Access() { DefaultAccess = DefaultAccess.Unrestricted };
            _Configuration.SetRestrictedPath("/MyPlugin/", null);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Prefixes_Path_With_Leading_Slash_If_Required()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("MyPlugin/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual("/MyPlugin/", map.First().Key);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Suffixes_Path_With_Trailing_Slash_If_Required()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MyPlugin", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual("/MyPlugin/", map.First().Key);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Treats_Null_Path_As_Root()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath(null, access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual("/", map.First().Key);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Does_Not_Add_Extra_Slash_To_Root()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            Assert.AreEqual("/", map.First().Key);
        }

        [TestMethod]
        public void AccessConfiguration_SetRestrictedPath_Returns_Dictionary_With_Case_Insensitive_Keys()
        {
            var access = new Access() { DefaultAccess = DefaultAccess.Deny };
            _Configuration.SetRestrictedPath("/MYPLUGIN/", access);

            var map = _Configuration.GetRestrictedPathsMap();

            // This will throw an exception if the dictionary isn't case insensitive
            map["/myplugin/"].Equals(null);
        }

        [TestMethod]
        [DataSource("Data Source='OwinTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AccessConfiguration$")]
        public void AccessConfiguration_IsPathAccessible_StringVersion_Rejects_Or_Allows_Requests()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            RunDataDrivenTest(worksheet, (requestPathAndFile, requestAddress) => {
                return _Configuration.IsPathAccessible(requestPathAndFile, requestAddress);
            });
        }

        [TestMethod]
        [DataSource("Data Source='OwinTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AccessConfiguration$")]
        public void AccessConfiguration_IsPathAccessible_IPAddressVersion_Rejects_Or_Allows_Requests()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            RunDataDrivenTest(worksheet, (requestPathAndFile, requestAddress) => {
                IPAddress ipAddress;
                if(!IPAddress.TryParse(requestAddress, out ipAddress)) {
                    ipAddress = IPAddress.None;
                }
                return _Configuration.IsPathAccessible(requestPathAndFile, ipAddress);
            });
        }

        private void RunDataDrivenTest(ExcelWorksheetData worksheet, Func<string, string, bool> isPathAccessible)
        {
            var defaultAccess = worksheet.ParseEnum<DefaultAccess>("DefaultAccess");
            var exceptCidr1 = worksheet.String("ExceptCIDR1");
            var pathFromRoot = worksheet.String("PathFromRoot");
            var requestAddress = worksheet.String("RequestAddress");
            var requestPathAndFile = worksheet.String("RequestPathAndFile");
            var isAccessAllowed = worksheet.Bool("IsAccessAllowed");
            var comments = worksheet.String("Comments");

            var access = new Access() {
                DefaultAccess = defaultAccess,
            };
            access.Addresses.Add(exceptCidr1);
            _Configuration.SetRestrictedPath(pathFromRoot, access);

            var result = isPathAccessible(requestPathAndFile, requestAddress);

            Assert.AreEqual(isAccessAllowed, result, comments);
        }

        [TestMethod]
        public void AccessConfiguration_IsPathAccessible_StringVersion_Uses_Default_Access_When_IPAddress_Is_Null_Or_Empty()
        {
            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Deny });
            Assert.AreEqual(false, _Configuration.IsPathAccessible("/", (string)null));
            Assert.AreEqual(false, _Configuration.IsPathAccessible("/", ""));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Allow });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", (string)null));
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", ""));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Unrestricted });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", (string)null));
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", ""));
        }

        [TestMethod]
        public void AccessConfiguration_IsPathAccessible_IPAddressVersion_Uses_Default_Access_When_IPAddress_Is_None()
        {
            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Deny });
            Assert.AreEqual(false, _Configuration.IsPathAccessible("/", IPAddress.None));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Allow });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", IPAddress.None));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Unrestricted });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", IPAddress.None));
        }

        [TestMethod]
        public void AccessConfiguration_IsPathAccessible_IPAddressVersion_Uses_Default_Access_When_IPAddress_Is_IPV6()
        {
            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Deny });
            Assert.AreEqual(false, _Configuration.IsPathAccessible("/", IPAddress.IPv6Loopback));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Allow });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", IPAddress.IPv6Loopback));

            _Configuration.SetRestrictedPath("/", new Access() { DefaultAccess = DefaultAccess.Unrestricted });
            Assert.AreEqual(true, _Configuration.IsPathAccessible("/", IPAddress.IPv6Loopback));
        }
    }
}
