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
    }
}
