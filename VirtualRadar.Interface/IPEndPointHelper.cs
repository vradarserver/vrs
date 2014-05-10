// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A static helper class that can make life easier when dealing with <see cref="IPEndPoint"/> objects.
    /// </summary>
    public static class IPEndPointHelper
    {
        /// <summary>
        /// Returns true if the endpoint looks like it is on the LAN.
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <returns></returns>
        public static bool IsLocalOrLanAddress(IPEndPoint ipEndPoint)
        {
            bool result = false;

            if(ipEndPoint.AddressFamily == AddressFamily.InterNetwork) {
                byte[] addressParts = ipEndPoint.Address.GetAddressBytes();
                if(CompareAddress(addressParts, 127, 0, 0, 1) == 0) result = true;
                if(!result) result = CompareAddress(addressParts, 10, 0, 0, 0) >= 0 && CompareAddress(addressParts, 10, 255, 255, 255) <= 0;
                if(!result) result = CompareAddress(addressParts, 169, 254, 1, 0) >= 0 && CompareAddress(addressParts, 169, 254, 254, 255) <= 0;
                if(!result) result = CompareAddress(addressParts, 172, 16, 0, 0) >= 0 && CompareAddress(addressParts, 172, 31, 255, 255) <= 0;
                if(!result) result = CompareAddress(addressParts, 192, 168, 0, 0) >= 0 && CompareAddress(addressParts, 192, 168, 255, 255) <= 0;
            }

            return result;
        }

        private static int CompareAddress(byte[] address, int p1, int p2, int p3, int p4)
        {
            int result = (int)address[0] - p1;
            if(result == 0) {
                result = (int)address[1] - p2;
                if(result == 0) {
                    result = (int)address[2] - p3;
                    if(result == 0) {
                        result = (int)address[3] - p4;
                    }
                }
            }

            return result;
        }
    }
}
