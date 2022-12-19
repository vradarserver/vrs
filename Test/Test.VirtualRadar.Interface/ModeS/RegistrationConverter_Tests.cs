// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.ModeS;

namespace Test.VirtualRadar.Interface.ModeS
{
    [TestClass]
    public class RegistrationConverter_Tests
    {
        [TestMethod]
        public void RegistrationToModeS_Returns_Null_For_Unknown_Registrations()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Registration = "G-ABCD", },
                new { Registration = "N", },
            },
            row => {
                var actual = RegistrationConverter.RegistrationToModeS(row.Registration);
                Assert.IsNull(actual);
            });
        }

        [TestMethod]
        public void RegistrationToModeS_Can_Convert_US_Registrations()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Registration = "N1",     Expected = "A00001", },
                new { Registration = "N1A",    Expected = "A00002", },
                new { Registration = "N1AA",   Expected = "A00003", },
                new { Registration = "N1AZ",   Expected = "A0001A", },
                new { Registration = "N1B",    Expected = "A0001B", },
                new { Registration = "N1BA",   Expected = "A0001C", },
                new { Registration = "N1BZ",   Expected = "A00033", },
                new { Registration = "N1C",    Expected = "A00034", },
                new { Registration = "N1HZ",   Expected = "A000C9", },
                new { Registration = "N1J",    Expected = "A000CA", },
                new { Registration = "N1NZ",   Expected = "A00146", },
                new { Registration = "N1P",    Expected = "A00147", },
                new { Registration = "N1ZZ",   Expected = "A00259", },
                new { Registration = "N10",    Expected = "A0025A", },
                new { Registration = "N10A",   Expected = "A0025B", },
                new { Registration = "N10AA",  Expected = "A0025C", },
                new { Registration = "N10AZ",  Expected = "A00273", },
                new { Registration = "N10B",   Expected = "A00274", },
                new { Registration = "N10ZY",  Expected = "A004B1", },
                new { Registration = "N10ZZ",  Expected = "A004B2", },
                new { Registration = "N100",   Expected = "A004B3", },
                new { Registration = "N100A",  Expected = "A004B4", },
                new { Registration = "N100AA", Expected = "A004B5", },
                new { Registration = "N100AZ", Expected = "A004CC", },
                new { Registration = "N100B",  Expected = "A004CD", },
                new { Registration = "N100ZZ", Expected = "A0070B", },
                new { Registration = "N1000",  Expected = "A0070C", },
                new { Registration = "N1000A", Expected = "A0070D", },
                new { Registration = "N1000Z", Expected = "A00724", },
                new { Registration = "N10000", Expected = "A00725", },
                new { Registration = "N10009", Expected = "A0072E", },
                new { Registration = "N1001",  Expected = "A0072F", },
                new { Registration = "N1001A", Expected = "A00730", },
                new { Registration = "N10019", Expected = "A00751", },
                new { Registration = "N1002",  Expected = "A00752", },
                new { Registration = "N10099", Expected = "A00869", },
                new { Registration = "N101",   Expected = "A0086A", },
                new { Registration = "N101A",  Expected = "A0086B", },
                new { Registration = "N101AA", Expected = "A0086C", },
                new { Registration = "N101AZ", Expected = "A00883", },
                new { Registration = "N101B",  Expected = "A00884", },
                new { Registration = "N101BA", Expected = "A00885", },
                new { Registration = "N101ZZ", Expected = "A00AC2", },
                new { Registration = "N1010",  Expected = "A00AC3", },
                new { Registration = "N1010A", Expected = "A00AC4", },
                new { Registration = "N10109", Expected = "A00AE5", },
                new { Registration = "N1011",  Expected = "A00AE6", },
                new { Registration = "N10199", Expected = "A00C20", },
                new { Registration = "N10999", Expected = "A029D8", },
                new { Registration = "N11",    Expected = "A029D9", },
                new { Registration = "N11A",   Expected = "A029DA", },
                new { Registration = "N11AA",  Expected = "A029DB", },
                new { Registration = "N18999", Expected = "A165D0", },
                new { Registration = "N19",    Expected = "A165D1", },
                new { Registration = "N19999", Expected = "A18D4F", },
                new { Registration = "N2",     Expected = "A18D50", },
                new { Registration = "N2A",    Expected = "A18D51", },
                new { Registration = "N2AA",   Expected = "A18D52", },
                new { Registration = "N201W",  Expected = "A197AE", },
                new { Registration = "N29999", Expected = "A31A9E", },
                new { Registration = "N3",     Expected = "A31A9F", },
                new { Registration = "N3A",    Expected = "A31AA0", },
                new { Registration = "N39999", Expected = "A4A7ED", },
                new { Registration = "N4",     Expected = "A4A7EE", },
                new { Registration = "N4AA",   Expected = "A4A7F0", },
                new { Registration = "N49999", Expected = "A6353C", },
                new { Registration = "N5",     Expected = "A6353D", },
                new { Registration = "N6",     Expected = "A7C28C", },
                new { Registration = "N69997", Expected = "A94FD8", },
                new { Registration = "N7A",    Expected = "A94FDC", },
                new { Registration = "N79999", Expected = "AADD29", },
                new { Registration = "N8A",    Expected = "AADD2B", },
                new { Registration = "N89999", Expected = "AC6A78", },
                new { Registration = "N9A",    Expected = "AC6A7A", },
                new { Registration = "N99997", Expected = "ADF7C5", },
                new { Registration = "N99999", Expected = "ADF7C7", },
            },
            row => {
                var actual = RegistrationConverter.RegistrationToModeS(row.Registration);
                Assert.AreEqual(row.Expected, actual);
            });
        }

        [TestMethod]
        public void ModeSToRegistration_Returns_Null_For_Unknown_Icao_Ranges()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Icao = "A00000", },
                new { Icao = "ADF7C8", },
            },
            row => {
                var actual = RegistrationConverter.ModeSToRegistration(row.Icao);
                Assert.IsNull(actual);
            });
        }

        [TestMethod]
        public void ModeSToRegistration_Can_Convert_US_ICAOs()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Icao = "A00001", Expected = "N1", },
                new { Icao = "A00002", Expected = "N1A", },
                new { Icao = "A00003", Expected = "N1AA", },
                new { Icao = "A0001A", Expected = "N1AZ", },
                new { Icao = "A0001B", Expected = "N1B", },
                new { Icao = "A0001C", Expected = "N1BA", },
                new { Icao = "A00033", Expected = "N1BZ", },
                new { Icao = "A00034", Expected = "N1C", },
                new { Icao = "A000C9", Expected = "N1HZ", },
                new { Icao = "A000CA", Expected = "N1J", },
                new { Icao = "A00146", Expected = "N1NZ", },
                new { Icao = "A00147", Expected = "N1P", },
                new { Icao = "A00259", Expected = "N1ZZ", },
                new { Icao = "A0025A", Expected = "N10", },
                new { Icao = "A0025B", Expected = "N10A", },
                new { Icao = "A0025C", Expected = "N10AA", },
                new { Icao = "A00273", Expected = "N10AZ", },
                new { Icao = "A00274", Expected = "N10B", },
                new { Icao = "A004B1", Expected = "N10ZY", },
                new { Icao = "A004B2", Expected = "N10ZZ", },
                new { Icao = "A004B3", Expected = "N100", },
                new { Icao = "A004B4", Expected = "N100A", },
                new { Icao = "A004B5", Expected = "N100AA", },
                new { Icao = "A004CC", Expected = "N100AZ", },
                new { Icao = "A004CD", Expected = "N100B", },
                new { Icao = "A0070B", Expected = "N100ZZ", },
                new { Icao = "A0070C", Expected = "N1000", },
                new { Icao = "A0070D", Expected = "N1000A", },
                new { Icao = "A0070E", Expected = "N1000B", },
                new { Icao = "A00724", Expected = "N1000Z", },
                new { Icao = "A00725", Expected = "N10000", },
                new { Icao = "A00726", Expected = "N10001", },
                new { Icao = "A0072E", Expected = "N10009", },
                new { Icao = "A0072F", Expected = "N1001", },
                new { Icao = "A00730", Expected = "N1001A", },
                new { Icao = "A00751", Expected = "N10019", },
                new { Icao = "A00752", Expected = "N1002", },
                new { Icao = "A00869", Expected = "N10099", },
                new { Icao = "A0086A", Expected = "N101", },
                new { Icao = "A0086B", Expected = "N101A", },
                new { Icao = "A0086C", Expected = "N101AA", },
                new { Icao = "A00883", Expected = "N101AZ", },
                new { Icao = "A00884", Expected = "N101B", },
                new { Icao = "A00885", Expected = "N101BA", },
                new { Icao = "A00AC2", Expected = "N101ZZ", },
                new { Icao = "A00AC3", Expected = "N1010", },
                new { Icao = "A00AC4", Expected = "N1010A", },
                new { Icao = "A00AE5", Expected = "N10109", },
                new { Icao = "A00AE6", Expected = "N1011", },
                new { Icao = "A00C20", Expected = "N10199", },
                new { Icao = "A029D8", Expected = "N10999", },
                new { Icao = "A029D9", Expected = "N11", },
                new { Icao = "A029DA", Expected = "N11A", },
                new { Icao = "A029DB", Expected = "N11AA", },
                new { Icao = "A165D0", Expected = "N18999", },
                new { Icao = "A165D1", Expected = "N19", },
                new { Icao = "A18D4F", Expected = "N19999", },
                new { Icao = "A18D50", Expected = "N2", },
                new { Icao = "A18D51", Expected = "N2A", },
                new { Icao = "A18D52", Expected = "N2AA", },
                new { Icao = "A197AE", Expected = "N201W", },
                new { Icao = "A31A9E", Expected = "N29999", },
                new { Icao = "A31A9F", Expected = "N3", },
                new { Icao = "A31AA0", Expected = "N3A", },
                new { Icao = "A4A7ED", Expected = "N39999", },
                new { Icao = "A4A7EE", Expected = "N4", },
                new { Icao = "A4A7F0", Expected = "N4AA", },
                new { Icao = "A6353C", Expected = "N49999", },
                new { Icao = "A6353D", Expected = "N5", },
                new { Icao = "A7C28C", Expected = "N6", },
                new { Icao = "A94FD8", Expected = "N69997", },
                new { Icao = "A94FDC", Expected = "N7A", },
                new { Icao = "AADD29", Expected = "N79999", },
                new { Icao = "AADD2B", Expected = "N8A", },
                new { Icao = "AC6A78", Expected = "N89999", },
                new { Icao = "AC6A7A", Expected = "N9A", },
                new { Icao = "ADF7C5", Expected = "N99997", },
                new { Icao = "ADF7C7", Expected = "N99999", },
            },
            row => {
                var actual = RegistrationConverter.ModeSToRegistration(row.Icao);
                Assert.AreEqual(row.Expected, actual, $"ICAO {row.Icao}");
            });
        }

        [TestMethod]
        public void RegistrationConverter_Can_Round_Trip_All_US_Registrations()
        {
            for(var icao = 0xA00001;icao < 0xADF7C7;++icao) {
                var icaoAsString = icao.ToString("X6");
                var registrationFromIcao = RegistrationConverter.ModeSToRegistration(icaoAsString);
                var icaoFromRegistration = RegistrationConverter.RegistrationToModeS(registrationFromIcao);

                Assert.AreEqual(icaoAsString, icaoFromRegistration);
            }
        }
    }
}
