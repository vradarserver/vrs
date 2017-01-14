// Copyright © 2016 onwards, Andrew Whewell
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
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class IPAddressHelperTests
    {
        class AddressResponse
        {
            public string Address   { get; set; }
            public bool IsLinkLocal { get; set; }
            public string Comment   { get; set; }

            public AddressResponse(string address, bool isLinkLocal, string comment)
            {
                Address = address;
                IsLinkLocal = isLinkLocal;
                Comment = comment;
            }
        }

        [TestMethod]
        public void IPAddressHelper_IsLinkLocal_Returns_Correct_Response_For_Different_Addresses()
        {
            var addressResponses = new AddressResponse[] {
                new AddressResponse("FE80::260:3EFF:FE11:6770",     true,   "IPV6 link local address"),
                new AddressResponse("2001:cdba:0:0:0:0:3257:9652",  false,  "IPV6 not link local address"),
                new AddressResponse("169.253.255.255",              false,  "IPV4 just before link local address range"),
                new AddressResponse("169.254.0.0",                  true,   "IPV4 start of link local address (first reserved) range"),
                new AddressResponse("169.254.0.255",                true,   "IPV4 end of link local address (first reserved) range"),
                new AddressResponse("169.254.1.0",                  true,   "IPV4 start of link local address (non-reserved) range"),
                new AddressResponse("169.254.254.255",              true,   "IPV4 end of link local address (non-reserved) range"),
                new AddressResponse("169.254.255.0",                true,   "IPV4 start of link local address (second reserved) range"),
                new AddressResponse("169.254.255.255",              true,   "IPV4 end of link local address (second reserved) range"),
                new AddressResponse("169.255.0.0",                  false,  "IPV4 just after link local address range"),
            };

            foreach(var addressResponse in addressResponses) {
                var address = IPAddress.Parse(addressResponse.Address);
                var response = IPAddressHelper.IsLinkLocal(address);
                Assert.AreEqual(addressResponse.IsLinkLocal, response, "Incorrect reponse for {0}: expected {1}, got {2}", addressResponse.Comment, addressResponse.IsLinkLocal, response);
            }
        }
    }
}
