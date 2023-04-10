// Copyright © 2018 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class Callsign_Tests
    {
        [TestMethod]
        [DataRow(null,       null,       "",    "",      "",      "",         false)]
        [DataRow("",         "",         "",    "",      "",      "",         false)]
        [DataRow("VIR1234",  "VIR1234",  "VIR", "1234",  "1234",  "VIR1234",  true)]
        [DataRow("VS1234",   "VS1234",   "VS",  "1234",  "1234",  "VS1234",   true)]
        [DataRow("VIR",      "VIR",      "",    "",      "",      "",         false)]
        [DataRow("1234",     "1234",     "",    "",      "",      "",         false)]
        [DataRow("VIR01234", "VIR01234", "VIR", "01234", "1234",  "VIR1234",  true)]
        [DataRow("VIR12345", "VIR12345", "VIR", "12345", "12345", "",         false)]   // numbers can only be up to 4 characters
        [DataRow("VIR123N",  "VIR123N",  "VIR", "123N",  "123N",  "VIR123N",  true)]
        [DataRow("VIR12NN",  "VIR12NN",  "VIR", "12NN",  "12NN",  "VIR12NN",  true)]
        [DataRow("VIR1NNN",  "VIR1NNN",  "VIR", "1NNN",  "1NNN",  "",         false)]   // only last one or two digits of number can be alphabetical
        [DataRow("VIR12N3",  "VIR12N3",  "VIR", "12N3",  "12N3",  "",         false)]   // cannot have a letter followed by a digit in the number
        [DataRow("VIR0",     "VIR0",     "VIR", "0",     "0",     "VIR0",     true)]
        [DataRow("VIR000N",  "VIR000N",  "VIR", "000N",  "0N",    "VIR0N",    true)]
        [DataRow("U21234",   "U21234",   "U2",  "1234",  "1234",  "U21234",   true)]
        [DataRow("2P1234",   "2P1234",   "2P",  "1234",  "1234",  "2P1234",   true)]
        [DataRow("BA1234",   "BA1234",   "BA",  "1234",  "1234",  "BA1234",   true)]
        [DataRow("GABCD",    "GABCD",    "",    "",      "",      "",         false)]
        [DataRow("WBA2O5S",  "WBA2O5S",  "WBA", "2O5S",  "2O5S",  "",         false)]   // real-life example, the 0 has been replaced with an O
        public void Ctor_Parses_Callsigns_Correctly(string callsignText, string original, string code, string number, string trimNumber, string trimCallsign, bool isValid)
        {
            var message = $"Callsign is '{callsignText}'";
            var callsign = new Callsign(callsignText);

            Assert.AreEqual(original,       callsign.OriginalCallsign,          message);
            Assert.AreEqual(code,           callsign.Code,                      message);
            Assert.AreEqual(number,         callsign.Number,                    message);
            Assert.AreEqual(trimNumber,     callsign.TrimmedNumber,             message);
            Assert.AreEqual(trimCallsign,   callsign.TrimmedCallsign,           message);
            Assert.AreEqual(isValid,        callsign.IsOriginalCallsignValid,   message);
        }
    }
}
