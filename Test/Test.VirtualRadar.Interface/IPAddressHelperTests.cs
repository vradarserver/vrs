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
