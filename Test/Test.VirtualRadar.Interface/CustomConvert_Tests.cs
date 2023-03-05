﻿// Copyright © 2016 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CustomConvert_Tests
    {
        [TestMethod]
        public void Icao24_Null_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(null));
        }

        [TestMethod]
        public void Icao24_Empty_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(""));
        }

        [TestMethod]
        public void Icao24_Whitespace_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(" "));
        }

        [TestMethod]
        public void Icao24_Six_Digit_Hex_Codes_Are_Valid()
        {
            Assert.AreEqual(0, CustomConvert.Icao24("000000"));
            Assert.AreEqual(1193046, CustomConvert.Icao24("123456"));
            Assert.AreEqual(11259375, CustomConvert.Icao24("ABCDEF"));
            Assert.AreEqual(16777215, CustomConvert.Icao24("FFFFFF"));
        }

        [TestMethod]
        public void Icao24_Seven_Digit_Hex_Codes_Are_Invalid()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24("1000000"));
        }

        [TestMethod]
        public void Icao24_Short_Hex_Codes_Are_Valid()
        {
            Assert.AreEqual(10, CustomConvert.Icao24("A"));
            Assert.AreEqual(170, CustomConvert.Icao24("AA"));
            Assert.AreEqual(2730, CustomConvert.Icao24("AAA"));
            Assert.AreEqual(43690, CustomConvert.Icao24("AAAA"));
            Assert.AreEqual(699050, CustomConvert.Icao24("AAAAA"));
        }

        [TestMethod]
        public void Icao24_Is_Case_Insensitive()
        {
            Assert.AreEqual(1223476, CustomConvert.Icao24("12AB34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12ab34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12Ab34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12aB34"));
        }

        [TestMethod]
        public void Icao24_Tilde_Codes_Are_Invalid()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24("~45A570"));
        }

        [TestMethod]
        public void Icao24_Non_Hex_Digits_Are_Invalid()
        {
            for(var ch = 32;ch < 256;++ch) {
                if((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')) {
                    continue;
                }
                var text = new String((char)ch, 6);
                Assert.AreEqual(-1, CustomConvert.Icao24(text), $"ICAO24 was {text}");
            }
        }

        [TestMethod]
        public void Icao24_All_Valid_Hex_Digits_Are_Valid()
        {
            foreach(var ch in new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F' }) {
                var text = new String(ch, 6);
                var expected = Convert.ToInt32(text, 16);
                Assert.AreEqual(expected, CustomConvert.Icao24(text), $"Failed on {ch}");
            }
        }

        [TestMethod]
        public void HexInt_Valid_Values_Decode_Correctly()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { Text = (string)null,  Expected = -1 },
                new { Text = "",            Expected = -1 },
                new { Text = " ",           Expected = -1 },
                new { Text = "H",           Expected = -1 },
                new { Text = "0",           Expected = 0 },
                new { Text = "123456",      Expected = 0x123456 },
                new { Text = "ABCDEF",      Expected = 0xABCDEF },
                new { Text = "abcdef",      Expected = 0xABCDEF },
                new { Text = "7fffffFF",    Expected = 0x7FFFFFFF },
                new { Text = "80000000",    Expected = Convert.ToInt32("0x80000000", 16) },
                new { Text = "90000000",    Expected = Convert.ToInt32("0x90000000", 16) },
                new { Text = "a0000000",    Expected = Convert.ToInt32("0XA0000000", 16) },
                new { Text = "B0000000",    Expected = Convert.ToInt32("0XB0000000", 16) },
                new { Text = "c0000000",    Expected = Convert.ToInt32("0XC0000000", 16) },
                new { Text = "D0000000",    Expected = Convert.ToInt32("0XD0000000", 16) },
                new { Text = "e0000000",    Expected = Convert.ToInt32("0XE0000000", 16) },
                new { Text = "FFFFfffE",    Expected = Convert.ToInt32("0xFFFFFFFE", 16) },
                new { Text = "FFFFFFFF",    Expected = -1 },
                new { Text = "100000000",   Expected = -1 },
            }, (dynamic row) => {
                var actual = CustomConvert.HexInt(row.Text);
                Assert.AreEqual(row.Expected, actual);
            });
        }

        [TestMethod]
        public void DistanceUnits_Calculates_Correct_Distances()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { Distance = 12.34, FromUnit = DistanceUnit.Kilometres,     ToUnit  = DistanceUnit.Kilometres,      Expected = 12.34 },
                new { Distance = 12.34, FromUnit = DistanceUnit.Kilometres,     ToUnit  = DistanceUnit.Miles,           Expected =  7.67 },
                new { Distance = 12.34, FromUnit = DistanceUnit.Kilometres,     ToUnit  = DistanceUnit.NauticalMiles,   Expected =  6.66 },
                new { Distance = 12.34, FromUnit = DistanceUnit.Miles,          ToUnit  = DistanceUnit.Kilometres,      Expected = 19.86 },
                new { Distance = 12.34, FromUnit = DistanceUnit.Miles,          ToUnit  = DistanceUnit.Miles,           Expected = 12.34 },
                new { Distance = 12.34, FromUnit = DistanceUnit.Miles,          ToUnit  = DistanceUnit.NauticalMiles,   Expected = 10.72 },
                new { Distance = 12.34, FromUnit = DistanceUnit.NauticalMiles,  ToUnit  = DistanceUnit.Kilometres,      Expected = 22.85 },
                new { Distance = 12.34, FromUnit = DistanceUnit.NauticalMiles,  ToUnit  = DistanceUnit.Miles,           Expected = 14.20 },
                new { Distance = 12.34, FromUnit = DistanceUnit.NauticalMiles,  ToUnit  = DistanceUnit.NauticalMiles,   Expected = 12.34 },
            }, (dynamic row) => {
                var actual = CustomConvert.DistanceUnits(row.Distance, row.FromUnit, row.ToUnit);
                Assert.AreEqual(row.Expected, actual, 0.01);
            });
        }
    }
}
