// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class DescribeTests
    {
        [TestMethod]
        public void Describe_Airport_Returns_Empty_String_If_Passed_Null()
        {
            Assert.AreEqual("", Describe.Airport(null, false));
        }

        [TestMethod]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, true,  true,  true,  "EGLL Heathrow, United Kingdom")]
        [DataRow(null,   "LHR",  "Heathrow", "United Kingdom", false, true,  true,  true,  "LHR Heathrow, United Kingdom")]
        [DataRow("",     "LHR",  "Heathrow", "United Kingdom", false, true,  true,  true,  "LHR Heathrow, United Kingdom")]
        [DataRow(null,   null,   "Heathrow", "United Kingdom", false, true,  true,  true,  "Heathrow, United Kingdom")]
        [DataRow("",     "",     "Heathrow", "United Kingdom", false, true,  true,  true,  "Heathrow, United Kingdom")]
        [DataRow("EGLL", "LHR",  null,       "United Kingdom", false, true,  true,  true,  "EGLL, United Kingdom")]
        [DataRow("EGLL", "LHR",  "",         "United Kingdom", false, true,  true,  true,  "EGLL, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", null,             false, true,  true,  true,  "EGLL Heathrow")]
        [DataRow("EGLL", "LHR",  "Heathrow", "",               false, true,  true,  true,  "EGLL Heathrow")]
        [DataRow(null,   null,   "Heathrow", null,             false, true,  true,  true,  "Heathrow")]
        [DataRow("",     "",     "Heathrow", "",               false, true,  true,  true,  "Heathrow")]
        [DataRow(null,   null,   null,       "United Kingdom", false, true,  true,  true,  "United Kingdom")]
        [DataRow("",     "",     "",         "United Kingdom", false, true,  true,  true,  "United Kingdom")]
        [DataRow(null,   null,   null,       null,             false, true,  true,  true,  "")]
        [DataRow("",     "",     "",         "",               false, true,  true,  true,  "")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, false, true,  true,  "Heathrow, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, true,  false, true,  "EGLL, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, true,  true,  false, "EGLL Heathrow")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, false, false, true,  "United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, false, true,  false, "Heathrow")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", false, true,  false, false, "EGLL")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", true,  true,  true,  true,  "LHR Heathrow, United Kingdom")]
        [DataRow("EGLL", null,   "Heathrow", "United Kingdom", true,  true,  true,  true,  "EGLL Heathrow, United Kingdom")]
        [DataRow("EGLL", "",     "Heathrow", "United Kingdom", true,  true,  true,  true,  "EGLL Heathrow, United Kingdom")]
        [DataRow(null,   null,   "Heathrow", "United Kingdom", true,  true,  true,  true,  "Heathrow, United Kingdom")]
        [DataRow("",     "",     "Heathrow", "United Kingdom", true,  true,  true,  true,  "Heathrow, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", true,  false, true,  true,  "Heathrow, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", true,  true,  false, true,  "LHR, United Kingdom")]
        [DataRow("EGLL", "LHR",  "Heathrow", "United Kingdom", true,  true,  true,  false, "LHR Heathrow")]
        public void Describe_Airport_Formats_Airport_Correctly(
            string icao,
            string iata,
            string name,
            string country,
            bool preferIata,
            bool showCode,
            bool showName,
            bool showCountry,
            string expected
        )
        {
            var airport = new Airport {
                IcaoCode =  icao,
                IataCode =  iata,
                Name =      name,
                Country =   country,
            };

            var actual = Describe.Airport(
                airport,
                preferIata,
                showCode,
                showName,
                showCountry
            );

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("2",           "A,\"B\"", "2, A,\"B\"",        "Normal case")]
        [DataRow(null,          "Paris",   "Paris",             "No official name")]
        [DataRow("2",           null,      "2",                 "No city")]
        [DataRow("X Bristol",   "Bristol", "X Bristol",         "If name ends with city then don't append city")]
        [DataRow("XBristol",    "Bristol", "XBristol, Bristol", "Only ignore city if city is there as a word")]
        [DataRow("Bristol",     "Bristol", "Bristol",           "Ignore city if it's the same as the name")]
        [DataRow("Bristol X",   "Bristol", "Bristol X",         "If name starts with city then don't append city")]
        [DataRow("X Bristol X", "Bristol", "X Bristol X",       "If name contains city then don't append city")]
        public void Describe_AirportName_Formats_AirportName_Correctly(string officialName, string cityName, string expected, string rule)
        {
            var actual = Describe.AirportName(officialName, cityName);

            Assert.AreEqual(expected, actual, rule);
        }

        [TestMethod]
        [DataRow(0,                "en-GB", "0 B")]
        [DataRow(1023,             "en-GB", "1,023 B")]
        [DataRow(1024,             "en-GB", "1.00 KB")]
        [DataRow(1048569,          "en-GB", "1,023.99 KB")]
        [DataRow(1048576,          "en-GB", "1.00 MB")]
        [DataRow(1073735532,       "en-GB", "1,023.99 MB")]
        [DataRow(1073741824,       "en-GB", "1.00 GB")]
        [DataRow(1099505185325,    "en-GB", "1,023.99 GB")]
        [DataRow(1099511627776,    "en-GB", "1.00 TB")]
        [DataRow(9999999999999999, "en-GB", "9,094.95 TB")]
        [DataRow(0,                "de-DE", "0 B")]
        [DataRow(1023,             "de-DE", "1.023 B")]
        [DataRow(1024,             "de-DE", "1,00 KB")]
        [DataRow(1048569,          "de-DE", "1.023,99 KB")]
        [DataRow(1048576,          "de-DE", "1,00 MB")]
        [DataRow(1073735532,       "de-DE", "1.023,99 MB")]
        [DataRow(1073741824,       "de-DE", "1,00 GB")]
        [DataRow(1099505185325,    "de-DE", "1.023,99 GB")]
        [DataRow(1099511627776,    "de-DE", "1,00 TB")]
        [DataRow(9999999999999999, "de-DE", "9.094,95 TB")]
        public void Describe_Bytes_Formats_Bytes_Correctly(long bytes, string culture, string expected)
        {
            using(var switcher = new CultureSwitcher(culture)) {
                var actual = Describe.Bytes(bytes);
                Assert.AreEqual(expected, actual);
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
        [DataRow(null,                                    null)]
        [DataRow("",                                      "")]
        [DataRow("G-ABCD123",                             "G-ABCD123")]
        [DataRow("g-abcd123",                             "G-ABCD123")]
        [DataRow(": ~#DTE 4!2*",                          "DTE42")]
        [DataRow("!\" £$%^&*()_+=-{}[]~#@':;/?.>,<|\\¬`", "+-")]
        [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890", "ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890")]
        [DataRow("^",                                     "")]
        [DataRow("°©12ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖÙÚÛÜÝ3",     "123")]
        public void Describe_IcaoCompliantRegistration_Correctly_Transforms_Registration_To_Comply_With_ICAO_Rules(string registration, string expected)
        {
            var actual = Describe.IcaoCompliantRegistration(registration);
            Assert.AreEqual(expected, actual);
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

        [TestMethod]
        public void Describe_MapProvider_Returns_Correct_Description()
        {
            foreach(var mapProvider in Enum.GetValues<MapProvider>()) {
                var expected = "";
                switch(mapProvider) {
                    case MapProvider.GoogleMaps:    expected = "Google Maps"; break;
                    case MapProvider.Leaflet:       expected = "Leaflet"; break;
                    default:                        throw new NotImplementedException();
                }

                var actual = Describe.MapProvider(mapProvider);
                Assert.AreEqual(expected, actual, $"Expected {expected} for map provider {mapProvider}, actually got {actual}");
            }
        }
    }
}
