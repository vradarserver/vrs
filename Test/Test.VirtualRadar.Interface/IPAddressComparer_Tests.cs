// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class IPAddressComparer_Tests
    {
        [TestMethod]
        [DataRow(null,              null,              0)]
        [DataRow(null,              "0.0.0.0",         -1)]
        [DataRow("0.0.0.0",         null,              1)]
        [DataRow("0.0.0.0",         "0.0.0.0",         0)]
        [DataRow("0.0.0.0",         "0.0.0.1",         -1)]
        [DataRow("0.0.0.1",         "0.0.0.0",         1)]
        [DataRow("0.0.1.0",         "0.0.0.255",       1)]
        [DataRow("0.0.0.255",       "0.0.1.0",         -1)]
        [DataRow("0.1.0.0",         "0.0.255.0",       1)]
        [DataRow("0.0.255.0",       "0.1.0.0",         -1)]
        [DataRow("1.0.0.0",         "0.255.0.0",       1)]
        [DataRow("0.255.0.0",       "1.0.0.0",         -1)]
        [DataRow("255.0.0.0",       "254.255.255.255", 1)]
        [DataRow("254.255.255.255", "255.0.0.0",       -1)]
        [DataRow("0001::1",         "255.255.255.255", 1)]
        [DataRow("255.255.255.255", "0001::1",         -1)]
        public void Compare_Compares_Endpoints_Correctly(string lhsText, string rhsText, int expectedResult)
        {
            var comparer = new IPAddressComparer();
            var lhs = lhsText == null ? null : IPAddress.Parse(lhsText);
            var rhs = rhsText == null ? null : IPAddress.Parse(rhsText);

            var result = comparer.Compare(lhs, rhs);
            if(expectedResult < 0) {
                Assert.IsTrue(result < 0);
            } else if(expectedResult > 0) {
                Assert.IsTrue(result > 0);
            } else {
                Assert.AreEqual(0, result);
            }
        }
    }
}
