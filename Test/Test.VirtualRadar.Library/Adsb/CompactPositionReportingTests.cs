// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace Test.VirtualRadar.Library.Adsb
{
    [TestClass]
    public class CompactPositionReportingTests
    {
        #region TestContext, Fields, TestInitialise
        public TestContext TestContext { get; set; }
        private ICompactPositionReporting _Cpr;

        [TestInitialize]
        public void TestInitialise()
        {
            _Cpr = Factory.Resolve<ICompactPositionReporting>();
        }
        #endregion

        #region Encode
        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CPREncodeLat$")]
        public void CompactPositionReporting_Encode_Produces_Correct_Results_For_CPR101_Tables()
        {
            // The values for this test are all taken directly from the transition latitude test tables in 1090-WP30-12 Proposed New Appendix CPR101.
            var worksheet = new ExcelWorksheetData(TestContext);

            var numberOfBits = worksheet.Byte("Bits");
            var oddFormat = worksheet.Bool("OddFormat");
            var latitude = worksheet.Double("Latitude");
            var longitude = worksheet.Double("Longitude");
            var globalCoordinate = new GlobalCoordinate(latitude, longitude);

            var expectedLatitude = Convert.ToInt32(worksheet.String("ExpectedLatitude"), 16);
            var expectedLongitude = Convert.ToInt32(worksheet.String("ExpectedLongitude"), 16);
            var dataRow = worksheet.Int("DataRow"); // helps set conditional breakpoints, VSTS doesn't always process rows in ascending order as they appear in the worksheet

            // In testing some of the input latitudes and longitudes couldn't produce the expected results from table 6-1 etc. of 1090-WP30-12 because of small
            // rounding errors in the handling of doubles. Switching to decimals didn't help and it would make the code slower because the FPU doesn't work with
            // decimals. So the "RELatitude" and "RELongitude" columns were added - if they are empty then the code is expected to produce the values in
            // the Expected columns, which corresponds with the test results from 1090-WP30-12, but if they contain values then these are the actual results after
            // the rounding error has had its wicked way. In most cases they are 1 out for latitude but that can move the resolved latitude into a different NL and produce
            // a large difference in longitude. There are very few of these anomalies, they represent errors of a few feet and as this isn't going into an aircraft I can't
            // say I'm too bothered about them. However I do want them to be obvious in the test data, hence the reason for adding new columns rather than just changing
            // the expected results.
            int? reLatitude = null;
            int? reLongitude = null;
            if(worksheet.String("RELatitude") != null) {
                reLatitude = Convert.ToInt32(worksheet.String("RELatitude"), 16);
                reLongitude = Convert.ToInt32(worksheet.String("RELongitude"), 16);
            }

            var coordinate = _Cpr.Encode(globalCoordinate, oddFormat, numberOfBits);
            Assert.AreEqual(reLatitude ?? expectedLatitude, coordinate.Latitude);
            Assert.AreEqual(reLongitude ?? expectedLongitude, coordinate.Longitude);
            Assert.AreEqual(numberOfBits, coordinate.NumberOfBits);
            Assert.AreEqual(oddFormat, coordinate.OddFormat);
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_Calculates_1090_WP30_17_Test_Surface_Positions()
        {
            var encoded = _Cpr.Encode(new GlobalCoordinate(38.998357, -74.0), true, 19);
            Assert.AreEqual(74133, encoded.Latitude);
            Assert.AreEqual(0, encoded.Longitude);

            encoded = _Cpr.Encode(new GlobalCoordinate(39.0, -73.999995), false, 19);
            Assert.AreEqual(0, encoded.Latitude);
            Assert.AreEqual(23302, encoded.Longitude);
        }
        #endregion

        #region LocalDecode
        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CPREncodeLat$")]
        public void CompactPositionReporting_LocalDecode_Produces_Correct_Results_For_CPR101_Tables()
        {
            // The values for this test are all taken directly from the transition latitude test tables in 1090-WP30-12 Proposed New Appendix CPR101
            var worksheet = new ExcelWorksheetData(TestContext);

            var numberOfBits = worksheet.Byte("Bits");
            var oddFormat = worksheet.Bool("OddFormat");
            var encodedLatitude = Convert.ToInt32(worksheet.String("ExpectedLatitude"), 16);
            var encodedLongitude = Convert.ToInt32(worksheet.String("ExpectedLongitude"), 16);
            var expectedLatitude = worksheet.Double("Latitude");
            var expectedLongitude = worksheet.Double("Longitude");
            var cprCoordinate = new CompactPositionReportingCoordinate(encodedLatitude, encodedLongitude, oddFormat, numberOfBits);

            // The reference latitude and longitude is set to roughly 50km of the expected latitude and longitude
            double? referenceLatitude, referenceLongitude;
            GreatCircleMaths.Destination(expectedLatitude, expectedLongitude, 45, 50, out referenceLatitude, out referenceLongitude);
            var referenceCoordinate = new GlobalCoordinate(referenceLatitude.Value, referenceLongitude.Value);

            var dataRow = worksheet.Int("DataRow"); // helps set conditional breakpoints, VSTS doesn't always process rows in ascending order as they appear in the worksheet

            var decodedCoordinate = _Cpr.LocalDecode(cprCoordinate, referenceCoordinate);

            // We need to accept 180 and -180 as being the same longitude, taking into account rounding errors
            if(expectedLongitude == -180.0 && decodedCoordinate.Longitude > 179.9999999999) expectedLongitude = 180.0;
            else if(expectedLongitude == 180.0 && decodedCoordinate.Longitude < -179.9999999999) expectedLongitude = -180.0;

            Assert.AreEqual(expectedLatitude, decodedCoordinate.Latitude, 0.0008);    // The CPR tables cover all latitudes, sometimes the rounding introduced by selecting the midpoint of a zone can be quite large
            Assert.AreEqual(expectedLongitude, decodedCoordinate.Longitude, 0.000000000001);  // This can have a lower tolerance as the CPR101 tables aren't testing longitude zone boundaries so much
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_LocalDecode_RoundTrip_Example_Produces_Correct_Results()
        {
            // This test was added in an investigation as to why the LocalDecode test using the CPR101 tables was producing wrong longitudes on the decode. It turned out that
            // the longitude decode was selecting the wrong m value, it was out by one.
            var location = new GlobalCoordinate(29.9113597534596, 45.0);
            var cprCoordinate = _Cpr.Encode(location, false, 19);
            var globalCoordinate = _Cpr.LocalDecode(cprCoordinate, location);

            Assert.AreEqual(location.Latitude, globalCoordinate.Latitude, 0.00001);
            Assert.AreEqual(location.Longitude, globalCoordinate.Longitude, 0.000000000001);
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_LocalDecode_Full_Globe_Round_Trip()
        {
            foreach(var oddFormat in new bool[] { true, false }) {
                foreach(var numberOfBits in new byte[] { 12, 14, 17, 19 }) {
                    var expectedAccuracy = 0.0;
                    switch(numberOfBits) {
                        case 12:    expectedAccuracy = 0.05; break;
                        case 14:    expectedAccuracy = 0.02; break;
                        case 17:    expectedAccuracy = 0.002; break;
                        case 19:    expectedAccuracy = 0.0004; break;
                    }

                    var latitudeResolution = 0.25;
                    var longitudeResolution = 0.25;
                    var location = new GlobalCoordinate();
                    for(var latitude = -90.0;latitude <= 90.0;latitude += latitudeResolution) {
                        for(var longitude = -180.0;longitude <= 180.0;longitude += longitudeResolution) {
                            location.Latitude = latitude;
                            location.Longitude = longitude;
                            var cprCoordinate = _Cpr.Encode(location, oddFormat, numberOfBits);
                            var globalCoordinate = _Cpr.LocalDecode(cprCoordinate, location);

                            Assert.AreEqual(latitude, globalCoordinate.Latitude, expectedAccuracy, "Lat/Lon/Format/Bits {0}/{1}/{2}/{3}", latitude, longitude, oddFormat ? "Odd" : "Even", numberOfBits);
                            Assert.AreEqual(longitude, globalCoordinate.Longitude, expectedAccuracy, "Lat/Lon/Format/Bits {0}/{1}/{2}/{3}", latitude, longitude, oddFormat ? "Odd" : "Even", numberOfBits);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_LocalDecode_Random_Fuzz_Test()
        {
            // This isn't a true fuzz test, the code will blow up if you pass truely out-of-range values. It's
            // just a check with some random values to see if I can trip over a set of coordinates that cause problems.
            // If this test blows up after a change to the class it doesn't necessarily mean the change caused the
            // problem, it could always have been there.
            var locations = new List<GlobalCoordinate>();
            var numberOfIterations = 100000;

            var random = new Random();
            for(var i = 0;i < numberOfIterations;++i) {
                locations.Add(new GlobalCoordinate() {
                    Latitude = (random.NextDouble() * 180.0) - 90.0,
                    Longitude = (random.NextDouble() * 360.0) - 180.0,
                });
            }

            foreach(var oddFormat in new bool[] { true, false }) {
                foreach(var numberOfBits in new byte[] { 12, 14, 17, 19 }) {
                    var expectedAccuracy = 0.0;
                    switch(numberOfBits) {
                        case 12:    expectedAccuracy = 0.05; break;
                        case 14:    expectedAccuracy = 0.02; break;
                        case 17:    expectedAccuracy = 0.002; break;
                        case 19:    expectedAccuracy = 0.0004; break;
                    }

                    foreach(var location in locations) {
                        var cprCoordinate = _Cpr.Encode(location, oddFormat, numberOfBits);
                        var globalCoordinate = _Cpr.LocalDecode(cprCoordinate, location);

                        Assert.AreEqual(location.Latitude, globalCoordinate.Latitude, expectedAccuracy, "Lat/Lon/Format/Bits {0}/{1}/{2}", location, oddFormat ? "Odd" : "Even", numberOfBits);
                        Assert.AreEqual(location.Longitude, globalCoordinate.Longitude, expectedAccuracy, "Lat/Lon/Format/Bits {0}/{1}/{2}", location, oddFormat ? "Odd" : "Even", numberOfBits);
                    }
                }
            }
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_LocalDecode_Produces_Expected_Results_For_CPR_ANOMALIES_AU_Investigation_Case()
        {
            // Coordinates etc. taken from the CPR_ANOMALIES_AU 02/03/2006 paper published on the Internet
            var location = new GlobalCoordinate(-36.850285934, 146.77314);
            var encoded = _Cpr.Encode(location, true, 17);
            Assert.AreEqual(125914, encoded.Latitude);
            Assert.AreEqual(98874, encoded.Longitude);

            var decoded = _Cpr.LocalDecode(encoded, new GlobalCoordinate(-36.850285934, 146.77395435));
            Assert.AreEqual(location.Latitude, decoded.Latitude, 0.0000000009);
            Assert.AreEqual(location.Longitude, decoded.Longitude, 0.00009);

            // This reproduces the "erroneous" decode. The paper's discussion on the merits of relying on single-decodes is not important here,
            // what is important is that the CPR object reproduces the decoding accurately.
            decoded = _Cpr.LocalDecode(new CompactPositionReportingCoordinate(125914, 21240, true, 17), new GlobalCoordinate(-36.850285934, 146.77395435));
            Assert.AreEqual(-36.850285934, decoded.Latitude, 0.0000000009);
            Assert.AreEqual(149.963856573, decoded.Longitude, 0.0000000009);

            // This reproduces the decode in the table showing that incrementing the encoded latitude produces the correct longitude, showing
            // that single-decodes for a vehicle transitioning between NLs can be dodgy.
            decoded = _Cpr.LocalDecode(new CompactPositionReportingCoordinate(125915, 21240, true, 17), new GlobalCoordinate(-36.850285934, 146.77395435));
            Assert.AreEqual(-36.8502393819518, decoded.Latitude, 0.0000000000009);
            Assert.AreEqual(146.7731362200798, decoded.Longitude, 0.0000000000009);
        }
        #endregion

        #region GlobalDecode
        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Returns_Null_If_Either_Coordinate_Is_Null()
        {
            var coordinate = new CompactPositionReportingCoordinate(0x10000, 0, true, 17);
            Assert.IsNull(_Cpr.GlobalDecode(null, coordinate, null));
            Assert.IsNull(_Cpr.GlobalDecode(coordinate, null, null));
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Returns_Null_If_Coordinate_Formats_Are_Not_Different()
        {
            var earlier = new CompactPositionReportingCoordinate(1234, 0, true, 17);
            var later   = new CompactPositionReportingCoordinate(1235, 0, true, 17);
            Assert.IsNull(_Cpr.GlobalDecode(earlier, later, null));
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Returns_Null_If_Coordinate_Bits_Are_Different()
        {
            var earlier = new CompactPositionReportingCoordinate(1234, 0, true, 17);
            var later   = new CompactPositionReportingCoordinate(1235, 0, false, 19);
            Assert.IsNull(_Cpr.GlobalDecode(earlier, later, null));
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Calculates_Correct_Position_From_Two_Coordinates()
        {
            var firstLocation = new GlobalCoordinate(54.12345, -0.61234);
            var secondLocation = new GlobalCoordinate(54.17, -0.5801);
            var distance = GreatCircleMaths.Distance(firstLocation.Latitude, firstLocation.Longitude, secondLocation.Latitude, secondLocation.Longitude);

            var earlyCpr = _Cpr.Encode(firstLocation, false, 17);
            var laterCpr = _Cpr.Encode(secondLocation, true, 17);

            var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, null);

            Assert.AreEqual(secondLocation.Latitude, decoded.Latitude, 0.0001);
            Assert.AreEqual(secondLocation.Longitude, decoded.Longitude, 0.0001);
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Returns_Null_If_The_Latitudes_Straddle_An_NL_Boundary()
        {
            var radiansToDegrees = 180.0 / Math.PI;
            var numerator = 1.0 - Math.Cos(Math.PI / (2.0 * 15.0));
            for(int nl = 59, i = 0;i < 58;--nl, ++i) {
                var denominator = 1.0 - Math.Cos((2.0 * Math.PI) / nl);
                var fraction = numerator / denominator;
                var sqrootOfFraction = Math.Sqrt(fraction);
                var latitude = radiansToDegrees * Math.Acos(sqrootOfFraction);

                var startLocation = new GlobalCoordinate(latitude - 0.0001, 80);
                var endLocation = new GlobalCoordinate(latitude + 0.0001, 80);

                foreach(var firstFormat in new bool[] { true, false }) {
                    foreach(var bits in new byte[] { 17, 19 }) {
                        var earlyCpr = _Cpr.Encode(startLocation, firstFormat, bits);
                        var laterCpr = _Cpr.Encode(endLocation, !firstFormat, bits);

                        Assert.IsNull(_Cpr.GlobalDecode(earlyCpr, laterCpr, endLocation), "{0}/{1} {2}-bit messages straddling NL{3} did not fail to decode", firstFormat, !firstFormat, bits, nl);
                    }
                }
            }
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Calculates_Correct_Position_For_Internet_Example_1()
        {
            var earlyCpr = new CompactPositionReportingCoordinate(92095, 39846, false, 17);
            var laterCpr = new CompactPositionReportingCoordinate(88385, 125818, true, 17);

            var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, null);

            Assert.AreEqual(10.2162144547802, decoded.Latitude, 0.00001);
            Assert.AreEqual(123.889128586342 , decoded.Longitude, 0.00001);
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Calculates_Correct_Surface_Position()
        {
            foreach(var startLatitude in new double[] { 17.12345, -17.12345 }) {
                foreach(var startLongitude in new double[] { 145.12345, 55.12345, -35.12345, -125.12345 }) {
                    var startLocation = new GlobalCoordinate(startLatitude, startLongitude);

                    double? receiverLatitude, receiverLongitude;
                    GreatCircleMaths.Destination(startLatitude, startLongitude, 70, 80, out receiverLatitude, out receiverLongitude);
                    var receiverLocation = new GlobalCoordinate(receiverLatitude.GetValueOrDefault(), receiverLongitude.GetValueOrDefault());

                    double? endLatitude, endLongitude;
                    GreatCircleMaths.Destination(startLatitude, startLongitude, 90, 1.4, out endLatitude, out endLongitude);
                    var endLocation = new GlobalCoordinate(endLatitude.GetValueOrDefault(), endLongitude.GetValueOrDefault());

                    var earlyCpr = _Cpr.Encode(startLocation, false, 19);
                    var laterCpr = _Cpr.Encode(endLocation, true, 19);

                    var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, endLocation);

                    var errorMessage = String.Format("Start: {0}, end (expected): {1}, receiver: {2}, decoded: {3}, earlyCpr: {4}, laterCpr: {5}", startLocation, endLocation, receiverLocation, decoded, earlyCpr, laterCpr);
                    Assert.AreEqual(endLocation.Latitude, decoded.Latitude, 0.00001, errorMessage);
                    Assert.AreEqual(endLocation.Longitude, decoded.Longitude, 0.00001, errorMessage);
                }
            }
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Calculates_1090_WP30_17_Test_Surface_Positions()
        {
            var message1 = new CompactPositionReportingCoordinate(130929, 23302, false, 19);  // {38.998363494873/-73.9999953560207}
            var message2 = new CompactPositionReportingCoordinate(74133, 0, true, 19);        // {38.998357, -74.0}

            var decoded = _Cpr.GlobalDecode(message1, message2, new GlobalCoordinate(38.99836, -74));
            Assert.AreEqual(38.998357, decoded.Latitude, 0.000001);
            Assert.AreEqual(-74.0, decoded.Longitude, 0.000001);

            var message3 = new CompactPositionReportingCoordinate(0, 23302, false, 19);
            decoded = _Cpr.GlobalDecode(message2, message3, new GlobalCoordinate(39.0, -74.0));
            Assert.AreEqual(39.0, decoded.Latitude, 0.000001);
            Assert.AreEqual(-73.999995, decoded.Longitude, 0.000001);
        }

        [TestMethod]
        public void CompactPositionReporting_GlobalDecode_Returns_Null_If_No_Receiver_Location_Is_Supplied_For_19Bit_Format()
        {
            var message1 = new CompactPositionReportingCoordinate(130929, 23302, false, 19);  // {38.998363494873/-73.9999953560207}
            var message2 = new CompactPositionReportingCoordinate(74133, 0, true, 19);        // {38.998357, -74.0}

            Assert.IsNull(_Cpr.GlobalDecode(message1, message2, null));
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_GlobalDecode_Full_Globe_Round_Trip()
        {
            foreach(var numberOfBits in new byte[] { 17, 19 }) {
                var expectedAccuracy = 0.0;
                switch(numberOfBits) {
                    case 17:    expectedAccuracy = 0.002; break;
                    case 19:    expectedAccuracy = 0.0004; break;
                }

                var latitudeResolution = 0.25;
                var longitudeResolution = 0.25;
                var endLocation = new GlobalCoordinate();
                for(var latitude = -89.75;latitude < 90.0;latitude += latitudeResolution) {  // global decoding at the poles can produce odd results
                    for(var longitude = -180.0;longitude <= 180.0;longitude += longitudeResolution) {
                        endLocation.Latitude = latitude;
                        endLocation.Longitude = longitude;

                        var startIsNorth = true;
                        if(latitude == 87.0 || latitude == -80.25 || latitude == 85.75 || latitude == 90.0) startIsNorth = false; // a start position north of the end position would be in a different NL for these
                        var bearing = startIsNorth ? 315 : 225;

                        double? startLatitude, startLongitude;
                        GreatCircleMaths.Destination(latitude, longitude, bearing, 1, out startLatitude, out startLongitude);
                        var startLocation = new GlobalCoordinate(startLatitude.Value, startLongitude.Value);

                        var earlyCpr = _Cpr.Encode(startLocation, true, numberOfBits);
                        var laterCpr = _Cpr.Encode(endLocation, false, numberOfBits);

                        var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, startLocation);
                        Assert.IsNotNull(decoded, "Returned null for start {0}, end {1} (early CPR {2} later CPR {3})", startLocation, endLocation, earlyCpr, laterCpr);

                        // Longitudes 180 and -180 are the same - if the decoded longitude has one sign and the expected longitude has the other then that's fine
                        if(longitude == -180.0 && decoded.Longitude == 180.0) decoded.Longitude = -180.0;
                        else if(longitude == 180.0 && decoded.Longitude == -180.0) decoded.Longitude = 180.0;

                        Assert.AreEqual(latitude, decoded.Latitude, expectedAccuracy, "Latitude incorrect for start {0}, end {1} (early CPR {2} later CPR {3})", startLocation, endLocation, earlyCpr, laterCpr);
                        Assert.AreEqual(longitude, decoded.Longitude, expectedAccuracy, "Longitude incorrect for start {0}, end {1} (early CPR {2} later CPR {3})", startLocation, endLocation, earlyCpr, laterCpr);
                    }
                }
            }
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_GlobalDecode_Surface_Position_At_North_Pole_Can_Return_Null()
        {
            // I saw a slightly odd case in the global round-trip test where a movement from 89.991006/90 to 90/-180 (about 1km) returned
            // null. Null should be returned if the NL zones for the start and end are different, but in this case they should both be 1
            // so I couldn't quite see why it would return null. This test just splits out the case to make it quicker to debug.
            //
            // The reason it happens is because the latitude encodes to 0 for 90° with even encoding which decodes to a latitude of 0°,
            // i.e. the equator. The other CPR coordinate decoded to the correct latitude, so the comparison between the NL of 89.991°
            // and 0° produces different results, hence the null being returned.
            //
            // As far as I can see the even encoding of 90° is correct:
            //    yz = floor(2^19 * (mod(90, 6) / 2^19) + 0.5)
            //    yz = floor(2^19 * (0 / 2^19) + 0.5)
            //    yz = floor(2^19 * 0 + 0.5)
            //    yz = floor(0.5)
            //    yz = 0
            // So I guess global decoding can fail with an even encoding that is directly on the north pole. I will adjust the global
            // round trip test to ignore the poles, it all breaks down a bit there.

            var startLocation = new GlobalCoordinate(89.9910067839303, 90);
            var endLocation = new GlobalCoordinate(90.0, -180.0);

            var earlyCpr = _Cpr.Encode(startLocation, true, 19);
            var laterCpr = _Cpr.Encode(endLocation, false, 19);

            var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, startLocation);
            Assert.IsNull(decoded);
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_GlobalDecode_Over_Antimeridian_Produces_Correct_Longitude()
        {
            var startLocation = new GlobalCoordinate(45.0, -179.9999); // on the western side of the antimeridian
            var endLocation = new GlobalCoordinate(45.0, 179.9999); // a movement of a few metres onto the eastern side of the antimeridian

            var earlyCpr = _Cpr.Encode(startLocation, true, 19);
            var laterCpr = _Cpr.Encode(endLocation, false, 19);

            var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, startLocation);

            Assert.AreEqual(45.0, decoded.Latitude, 0.0001);
            Assert.AreEqual(179.9999, decoded.Longitude, 0.0001);
        }

        [TestMethod]
        public void CompactPositionReporting_Encode_GlobalDecode_Over_Meridian_Produces_Correct_Longitude()
        {
            var startLocation = new GlobalCoordinate(45.0, -0.0001); // on the western side of the meridian
            var endLocation = new GlobalCoordinate(45.0, 0.0001); // a movement of a few metres onto the eastern side of the meridian

            var earlyCpr = _Cpr.Encode(startLocation, true, 19);
            var laterCpr = _Cpr.Encode(endLocation, false, 19);

            var decoded = _Cpr.GlobalDecode(earlyCpr, laterCpr, startLocation);

            Assert.AreEqual(45.0, decoded.Latitude, 0.0001);
            Assert.AreEqual(0.0001, decoded.Longitude, 0.0001);
        }
        #endregion
    }
}
