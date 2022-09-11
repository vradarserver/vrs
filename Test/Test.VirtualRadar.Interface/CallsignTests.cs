// Copyright © 2018 onwards, Andrew Whewell
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
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CallsignTests
    {
        [TestMethod]
        public void Ctor_Parses_Callsigns_Correctly()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Callsign = (string)null, Original = (string)null, Code = "",    Number = "",      TrimNumber = "",      TrimCallsign = "",         IsValid = false, },
                new { Callsign = "",           Original = "",           Code = "",    Number = "",      TrimNumber = "",      TrimCallsign = "",         IsValid = false, },
                new { Callsign = "VIR1234",    Original = "VIR1234",    Code = "VIR", Number = "1234",  TrimNumber = "1234",  TrimCallsign = "VIR1234",  IsValid = true, },
                new { Callsign = "VS1234",     Original = "VS1234",     Code = "VS",  Number = "1234",  TrimNumber = "1234",  TrimCallsign = "VS1234",   IsValid = true, },
                new { Callsign = "VIR",        Original = "VIR",        Code = "",    Number = "",      TrimNumber = "",      TrimCallsign = "",         IsValid = false, },
                new { Callsign = "1234",       Original = "1234",       Code = "",    Number = "",      TrimNumber = "",      TrimCallsign = "",         IsValid = false, },
                new { Callsign = "VIR01234",   Original = "VIR01234",   Code = "VIR", Number = "01234", TrimNumber = "1234",  TrimCallsign = "VIR1234",  IsValid = true, },
                new { Callsign = "VIR12345",   Original = "VIR12345",   Code = "VIR", Number = "12345", TrimNumber = "12345", TrimCallsign = "",         IsValid = false, }, // numbers can only be up to 4 characters
                new { Callsign = "VIR123N",    Original = "VIR123N",    Code = "VIR", Number = "123N",  TrimNumber = "123N",  TrimCallsign = "VIR123N",  IsValid = true, },
                new { Callsign = "VIR12NN",    Original = "VIR12NN",    Code = "VIR", Number = "12NN",  TrimNumber = "12NN",  TrimCallsign = "VIR12NN",  IsValid = true, },
                new { Callsign = "VIR1NNN",    Original = "VIR1NNN",    Code = "VIR", Number = "1NNN",  TrimNumber = "1NNN",  TrimCallsign = "",         IsValid = false, }, // only last one or two digits of number can be alphabetical
                new { Callsign = "VIR12N3",    Original = "VIR12N3",    Code = "VIR", Number = "12N3",  TrimNumber = "12N3",  TrimCallsign = "",         IsValid = false, }, // cannot have a letter followed by a digit in the number
                new { Callsign = "VIR0",       Original = "VIR0",       Code = "VIR", Number = "0",     TrimNumber = "0",     TrimCallsign = "VIR0",     IsValid = true, },
                new { Callsign = "VIR000N",    Original = "VIR000N",    Code = "VIR", Number = "000N",  TrimNumber = "0N",    TrimCallsign = "VIR0N",    IsValid = true, },
                new { Callsign = "U21234",     Original = "U21234",     Code = "U2",  Number = "1234",  TrimNumber = "1234",  TrimCallsign = "U21234",   IsValid = true, },
                new { Callsign = "2P1234",     Original = "2P1234",     Code = "2P",  Number = "1234",  TrimNumber = "1234",  TrimCallsign = "2P1234",   IsValid = true, },
                new { Callsign = "BA1234",     Original = "BA1234",     Code = "BA",  Number = "1234",  TrimNumber = "1234",  TrimCallsign = "BA1234",   IsValid = true, },
                new { Callsign = "GABCD",      Original = "GABCD",      Code = "",    Number = "",      TrimNumber = "",      TrimCallsign = "",         IsValid = false, },
                new { Callsign = "WBA2O5S",    Original = "WBA2O5S",    Code = "WBA", Number = "2O5S",  TrimNumber = "2O5S",  TrimCallsign = "",         IsValid = false, }, // real-life example, the 0 has been replaced with an O
            }, (row) => {
                var message = $"Callsign is '{row.Callsign}'";
                var callsign = new Callsign(row.Callsign);

                Assert.AreEqual(row.Original,       callsign.OriginalCallsign,          message);
                Assert.AreEqual(row.Code,           callsign.Code,                      message);
                Assert.AreEqual(row.Number,         callsign.Number,                    message);
                Assert.AreEqual(row.TrimNumber,     callsign.TrimmedNumber,             message);
                Assert.AreEqual(row.TrimCallsign,   callsign.TrimmedCallsign,           message);
                Assert.AreEqual(row.IsValid,        callsign.IsOriginalCallsignValid,   message);
            });
        }
    }
}
