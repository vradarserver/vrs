// Copyright © 2013 onwards, Andrew Whewell
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualRadar.Interface.StandingData;
using InterfaceFactory;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class CallsignParserTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private ICallsignParser _CallsignParser;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Dictionary<string, List<Airline>> _Airlines;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Airlines = new Dictionary<string,List<Airline>>();
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataManager.Setup(r => r.FindAirlinesForCode(It.IsAny<string>())).Returns((string code) => {
                List<Airline> codeResult;
                if(code == null || !_Airlines.TryGetValue(code, out codeResult)) codeResult = new List<Airline>();
                return codeResult;
            });

            _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }

        /// <summary>
        /// Adds an airline to the dictionary that the standing data mock uses when faking the "find airlines by code" call.
        /// </summary>
        /// <param name="airline"></param>
        private void AddAirlineToStandingData(Airline airline)
        {
            foreach(var code in new string[] { airline.IataCode, airline.IcaoCode }) {
                if(!String.IsNullOrEmpty(code)) {
                    List<Airline> list;
                    if(!_Airlines.TryGetValue(code, out list)) {
                        list = new List<Airline>();
                        _Airlines.Add(code, list);
                    }
                    list.Add(airline);
                }
            }
        }

        /// <summary>
        /// Adds airlines from a worksheet where the ICAO and IATA codes are held in numbered columns
        /// called "ICAOn" and "IATAn", where n is from 1 to count of columns.
        /// </summary>
        /// <param name="worksheet"></param>
        private void InitialiseStandingDataFromWorksheet(ExcelWorksheetData worksheet, int countColumns)
        {
            for(var i = 1;i <= countColumns;++i) {
                var icao = worksheet.EString(String.Format("ICAO{0}", i));
                var iata = worksheet.EString(String.Format("IATA{0}", i));
                if(!String.IsNullOrEmpty(icao) || !String.IsNullOrEmpty(iata)) {
                    var airline = new Airline() {
                        IcaoCode = icao,
                        IataCode = iata,
                        Name = "Airline" + i.ToString(),
                    };
                    AddAirlineToStandingData(airline);
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CallsignParserAlt$")]
        public void CallsignParser_GetAllAlternateCallsigns_Returns_The_Correct_Collection_Of_Callsigns()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            InitialiseStandingDataFromWorksheet(worksheet, countColumns: 2);

            var callsign = worksheet.EString("Callsign");
            var result = _CallsignParser.GetAllAlternateCallsigns(callsign);

            var expectedText = worksheet.String("Result") ?? "";
            var expectedResult = expectedText == "" ? new string[] {} : worksheet.Array<string>("Result").Select(r => r == null ? null : r.Trim()).ToArray();

            var message = String.Format("{0} -> {1}", callsign, expectedText);
            Assert.AreEqual(expectedResult.Length, result.Count, message);
            foreach(var expected in expectedResult) {
                Assert.IsTrue(result.Contains(expected), "Missing '{0}'. {1}", expected, message);
            }
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CallsignParserRoute$")]
        public void CallsignParser_GetAllRouteCallsigns_Returns_The_Correct_Collection_Of_Callsigns()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            InitialiseStandingDataFromWorksheet(worksheet, countColumns: 3);

            var callsign = worksheet.EString("Callsign");
            var operatorIcaoCode = worksheet.EString("OpCode");
            var result = _CallsignParser.GetAllRouteCallsigns(callsign, operatorIcaoCode);

            var expectedText = worksheet.String("Result") ?? "";
            var expectedResult = expectedText == "" ? new string[] {} : worksheet.Array<string>("Result").Select(r => r == null ? null : r.Trim()).ToArray();

            var message = String.Format("{0} -> {1}", callsign, expectedText);
            Assert.AreEqual(expectedResult.Length, result.Count, message);
            foreach(var expected in expectedResult) {
                Assert.IsTrue(result.Contains(expected), "Missing '{0}'. {1}", expected, message);
            }
        }
    }
}
