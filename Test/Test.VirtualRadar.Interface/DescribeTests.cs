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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class DescribeTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Describe_Airport_Returns_Empty_String_If_Passed_Null()
        {
            Assert.AreEqual("", Describe.Airport(null, false));
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DescribeAirport$")]
        public void Describe_Airport_Formats_Airport_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var airport = new Airport();
            airport.IcaoCode = worksheet.EString("Icao");
            airport.IataCode = worksheet.EString("Iata");
            airport.Name = worksheet.EString("Name");
            airport.Country = worksheet.EString("Country");

            var result = Describe.Airport(airport, worksheet.Bool("PreferIata"), worksheet.Bool("ShowCode"), worksheet.Bool("ShowName"), worksheet.Bool("ShowCountry"));

            Assert.AreEqual(worksheet.EString("Result"), result);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DescribeAirportName$")]
        public void Describe_AirportName_Formats_AirportName_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var officialName = worksheet.EString("OfficialName");
            var cityName = worksheet.EString("City");
            var expected = worksheet.EString("Result");

            var result = Describe.AirportName(officialName, cityName);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DescribeBytes$")]
        public void Describe_Bytes_Formats_Bytes_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            using(var switcher = new CultureSwitcher(worksheet.String("Region"))) {
                Assert.AreEqual(worksheet.String("Description"), Describe.Bytes(worksheet.Long("Bytes")));
            }
        }

        [TestMethod]
        public void Describe_TimeSpan_Formats_TimeSpans_Correctly()
        {
            Assert.AreEqual("00:00:00", Describe.TimeSpan(new TimeSpan()));
            Assert.AreEqual("00:00:01", Describe.TimeSpan(new TimeSpan(0, 0, 1)));
            Assert.AreEqual("00:01:59", Describe.TimeSpan(new TimeSpan(0, 1, 59)));
            Assert.AreEqual("01:59:59", Describe.TimeSpan(new TimeSpan(1, 59, 59)));
            Assert.AreEqual("100:59:59", Describe.TimeSpan(new TimeSpan(100, 59, 59)));
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "IcaoCompliantRegistration$")]
        public void Describe_IcaoCompliantRegistration_Correctly_Transforms_Registration_To_Comply_With_ICAO_Rules()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            Assert.AreEqual(worksheet.EString("IcaoCompliantRegistration"), Describe.IcaoCompliantRegistration(worksheet.EString("Registration")));
        }

        [TestMethod]
        public void Describe_RebroadcastSettingsCollection_Correctly_Describes_Collection()
        {
            foreach(var culture in new string[] { "en-GB", "fr-FR" })  {
                using(var cultureSwitcher = new CultureSwitcher(culture)) {
                    Assert.AreEqual(Strings.RebroadcastServersNoneConfigured, Describe.RebroadcastSettingsCollection(null));

                    var settings = new List<RebroadcastSettings>();
                    Assert.AreEqual(Strings.RebroadcastServersNoneConfigured, Describe.RebroadcastSettingsCollection(settings));

                    settings.Add(new RebroadcastSettings() { Enabled = false });
                    Assert.AreEqual(String.Format(Strings.RebroadcastServersDescribeSingle, 0), Describe.RebroadcastSettingsCollection(settings));

                    settings[0].Enabled = true;
                    Assert.AreEqual(String.Format(Strings.RebroadcastServersDescribeSingle, 1), Describe.RebroadcastSettingsCollection(settings));

                    settings.Add(new RebroadcastSettings() { Enabled = false });
                    Assert.AreEqual(String.Format(Strings.RebroadcastServersDescribeMany, 2, 1), Describe.RebroadcastSettingsCollection(settings));

                    settings[1].Enabled = true;
                    Assert.AreEqual(String.Format(Strings.RebroadcastServersDescribeMany, 2, 2), Describe.RebroadcastSettingsCollection(settings));
                }
            }
        }
    }
}
