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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Interface.Owin
{
    [TestClass]
    public class PipelineContextTests
    {
        public TestContext TestContext { get; set; }

        private Dictionary<string, object> _Environment;
        private PipelineContext _Context;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _Context = new PipelineContext(_Environment);
        }

        [TestMethod]
        public void PipelineContext_Constructor_Initialises_To_Known_State()
        {
            Assert.AreSame(_Environment, _Context.Request.Environment);
            Assert.AreSame(_Environment, _Context.Response.Environment);
        }

        [TestMethod]
        public void PipelineContext_GetOrCreate_Adds_Itself_To_Environment()
        {
            var environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            PipelineContext.GetOrCreate(environment);

            var entry = environment["vrs.PipelineContext"];
            Assert.IsTrue(entry is PipelineContext);
        }

        [TestMethod]
        public void PipelineContext_GetOrCreate_Returns_Existing_Entry()
        {
            var environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            PipelineContext.GetOrCreate(environment);
            var entry = environment["vrs.PipelineContext"];

            PipelineContext.GetOrCreate(environment);
            Assert.AreEqual(1, environment.Count);
            Assert.AreSame(entry, environment["vrs.PipelineContext"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSet_Adds_New_Values_To_Environment()
        {
            var value = new object();
            var result = PipelineContext.GetOrSet(_Environment, "key", () => value);

            Assert.AreSame(value, result);
            Assert.AreSame(value, _Environment["key"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSet_Returns_Existing_Values_From_Environment()
        {
            var value = new object();
            _Environment["key"] = value;

            var result = PipelineContext.GetOrSet<object>(_Environment, "key", () => {
                Assert.Fail("This delegate should not be called");
                return new object();
            });

            Assert.AreSame(value, result);
            Assert.AreSame(value, _Environment["key"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Returns_New_Value_If_Translation_Does_Not_Exist()
        {
            var value = new object();
            var result = PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "1", () => value);

            Assert.AreSame(value, result);
            Assert.AreEqual("1", _Environment["original"]);
            Assert.AreSame(value, _Environment["translation"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Does_Not_Call_Build_Method_If_Current_Value_Is_Unchanged()
        {
            var value = new object();
            PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "1", () => value);
            var result = PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "1", () => {
                Assert.Fail("This should not be called because the current value has not changed");
                return new object();
            });

            Assert.AreSame(value, result);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Calls_Build_Method_If_Current_Value_Changes()
        {
            var value = new object();
            PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "1", () => value);

            var newValue = new object();
            var result = PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "2", () => newValue);

            Assert.AreSame(newValue, result);
            Assert.AreEqual("2", _Environment["original"]);
            Assert.AreSame(newValue, _Environment["translation"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Can_Cope_If_Current_Value_Is_Null()
        {
            var value = new object();
            var result = PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", null, () => value);

            Assert.AreSame(value, result);
            Assert.AreEqual(null, _Environment["original"]);
            Assert.AreSame(value, _Environment["translation"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Can_Cope_If_Translation_Is_Null()
        {
            var result = PipelineContext.GetOrSetTranslation<string, object>(_Environment, "original", "translation", "1", () => null);

            Assert.AreSame(null, result);
            Assert.AreEqual("1", _Environment["original"]);
            Assert.AreSame(null, _Environment["translation"]);
        }

        [TestMethod]
        public void PipelineContext_GetOrSetTranslation_Adds_StandardSuffix_If_Original_Key_Is_Null()
        {
            PipelineContext.GetOrSetTranslation<string, string>(_Environment, null, "translation", "1", () => "2");

            Assert.AreEqual("1", _Environment["original.translation"]);
            Assert.AreSame("2", _Environment["translation"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PipelineContext_GetOrSetTranslation_Throws_If_Translation_Key_Is_Null()
        {
            PipelineContext.GetOrSetTranslation<string, string>(_Environment, "original", null, "1", () => "2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PipelineContext_GetOrSetTranslation_Throws_If_Original_And_Translation_Keys_Are_The_Same()
        {
            PipelineContext.GetOrSetTranslation<string, string>(_Environment, "key", "key", "1", () => "2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PipelineContext_GetOrSetTranslation_Is_Case_Insensitive_When_Comparing_Keys()
        {
            PipelineContext.GetOrSetTranslation<string, string>(_Environment, "key", "KEY", "1", () => "2");
        }

        [TestMethod]
        [DataSource("Data Source='OwinTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ConstructUrl$")]
        public void PipelineContext_ConstructUrl_Returns_Correct_Values()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var scheme =        worksheet.EString("Scheme");
            var host =          worksheet.EString("Host");
            var pathBase =      worksheet.EString("PathBase");
            var path =          worksheet.EString("Path");
            var queryString =   worksheet.EString("QueryString");
            var expected =      worksheet.EString("URL");

            var url = PipelineContext.ConstructUrl(scheme, host, pathBase, path, queryString);

            Assert.AreEqual(expected, url);
        }
    }
}
