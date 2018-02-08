// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Interface.Data
{
    [TestClass]
    public class AirlineTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Airline_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var airline = new Airline();

            TestUtilities.TestProperty(airline, r => r.IataCode, null, "Aa");
            TestUtilities.TestProperty(airline, r => r.IcaoCode, null, "An");
            TestUtilities.TestProperty(airline, r => r.Name, null, "Bb");
            TestUtilities.TestProperty(airline, r => r.PositioningFlightPattern, null, "Ab");
            TestUtilities.TestProperty(airline, r => r.CharterFlightPattern, null, "Ab");
        }

        [TestMethod]
        public void Airline_ToString_Returns_Standardised_Text()
        {
            var airline = new Airline();

            airline.IataCode = "IATA";
            airline.IcaoCode = "ICAO";
            airline.Name = "NAME";
            Assert.AreEqual("ICAO NAME", airline.ToString());

            airline.IcaoCode = null;
            Assert.AreEqual("IATA NAME", airline.ToString());

            airline.IcaoCode = "";
            Assert.AreEqual("IATA NAME", airline.ToString());
        }

        [TestMethod]
        [DataSource("Data Source='DataTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AirlineFlightNumberRegex$")]
        public void Airline_IsPositioningFlightNumber_Returns_Correct_Value()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var regex = worksheet.EString("Regex");
            var flightNumber = worksheet.EString("FlightNumber");

            var airline = new Airline() { PositioningFlightPattern = regex };
            var actual = airline.IsPositioningFlightNumber(flightNumber);
            var expected = worksheet.Bool("Matches");

            Assert.AreEqual(expected, actual, $"FlightNumber '{flightNumber}' and Regex '{regex}'");
        }

        [TestMethod]
        [DataSource("Data Source='DataTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AirlineFlightNumberRegex$")]
        public void Airline_IsCharterFlightNumber_Returns_Correct_Value()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var regex = worksheet.EString("Regex");
            var flightNumber = worksheet.EString("FlightNumber");

            var airline = new Airline() { CharterFlightPattern = regex };
            var actual = airline.IsCharterFlightNumber(flightNumber);
            var expected = worksheet.Bool("Matches");

            Assert.AreEqual(expected, actual, $"FlightNumber '{flightNumber}' and Regex '{regex}'");
        }
    }
}
