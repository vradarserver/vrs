// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Library;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class CallsignParser_Tests
    {
        public TestContext TestContext { get; set; }

        private CallsignParser _CallsignParser;
        private MockStandingDataManager _StandingDataManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _StandingDataManager = new MockStandingDataManager();
            _CallsignParser = new(_StandingDataManager.Object);
        }

        /// <summary>
        /// Adds an airline to the dictionary that the standing data mock uses when faking the "find airlines by code" call.
        /// </summary>
        /// <param name="airline"></param>
        private void AddAirlineToStandingData(Airline airline) => _StandingDataManager.AllAirlines.Add(airline);

        /// <summary>
        /// Adds airlines from a worksheet where the ICAO and IATA codes are held in numbered columns
        /// called "ICAOn" and "IATAn", where n is from 1 to count of columns.
        /// </summary>
        /// <param name="worksheet"></param>
        private void InitialiseStandingDataFromWorksheet(SpreadsheetTestDataRow row, int countColumns)
        {
            for(var i = 1;i <= countColumns;++i) {
                var icao = row.EString($"ICAO{i}");
                var iata = row.EString($"IATA{i}");
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
        public void CallsignParser_GetAllAlternateCallsigns_Returns_The_Correct_Collection_Of_Callsigns()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.CallsignParserTestData, "CallsignParserAlt");
            spreadsheet.TestEveryRow(this, row => {
                InitialiseStandingDataFromWorksheet(row, countColumns: 2);

                var callsign = row.EString("Callsign");
                var result = _CallsignParser.GetAllAlternateCallsigns(callsign);

                var expectedText = row.String("Result") ?? "";
                var expectedResult = expectedText == ""
                    ? Array.Empty<string>()
                    : row.Array<string>("Result")
                        .Select(r => r == null ? null : r.Trim()).ToArray();

                var message = $"{callsign} -> {expectedText}";
                Assert.AreEqual(expectedResult.Length, result.Count, message);

                foreach(var expected in expectedResult) {
                    Assert.IsTrue(result.Contains(expected), "Missing '{0}'. {1}", expected, message);
                }
            });
        }

        [TestMethod]
        public void CallsignParser_GetAllRouteCallsigns_Returns_The_Correct_Collection_Of_Callsigns()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.CallsignParserTestData, "CallsignParserRoute");
            spreadsheet.TestEveryRow(this, row => {
                InitialiseStandingDataFromWorksheet(row, countColumns: 3);

                var callsign = row.EString("Callsign");
                var operatorIcaoCode = row.EString("OpCode");
                var result = _CallsignParser.GetAllRouteCallsigns(callsign, operatorIcaoCode);

                var expectedText = row.String("Result") ?? "";
                var expectedResult = expectedText == ""
                    ? Array.Empty<string>()
                    : row.Array<string>("Result").Select(r => r == null ? null : r.Trim()).ToArray();

                var message = $"{callsign} -> {expectedText}";
                Assert.AreEqual(expectedResult.Length, result.Count, message);

                foreach(var expected in expectedResult) {
                    Assert.IsTrue(result.Contains(expected), "Missing '{0}'. {1}", expected, message);
                }
            });
        }
    }
}
