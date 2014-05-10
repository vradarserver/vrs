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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.WebServer;
using Moq;
using System.Net;

namespace Test.Framework
{
    /// <summary>
    /// A static class that helps with setting up <see cref="RequestReceivedEventArgs"/> objects for different
    /// conditions.
    /// </summary>
    public static class RequestReceivedEventArgsHelper
    {
        /// <summary>
        /// Creates a <see cref="RequestReceivedEventArgs"/>, attaches the mock request and response objects passed across and sets a few key properties.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="pathAndFile"></param>
        /// <param name="internetClient"></param>
        /// <returns></returns>
        public static RequestReceivedEventArgs Create(Mock<IRequest> request, Mock<IResponse> response, string pathAndFile, bool internetClient = false)
        {
            var root = "/Root";

            StringBuilder requestUrl = new StringBuilder("http://127.0.0.1");
            requestUrl.AppendFormat("{0}{1}", root, pathAndFile);

            var endPoint = new IPEndPoint(IPAddress.Parse(internetClient ? "16.14.12.10" : "127.0.0.1"), 1234);

            request.Setup(r => r.RemoteEndPoint).Returns(endPoint);
            request.Setup(r => r.RawUrl).Returns(String.Format("{0}{1}", root, pathAndFile));
            request.Setup(r => r.Url).Returns(new Uri(requestUrl.ToString()));

            return new RequestReceivedEventArgs(request.Object, response.Object, root);
        }

        /// <summary>
        /// Sets a mock <see cref="IRequest"/> object up with a user agent string from an Android mobile.
        /// </summary>
        /// <param name="request"></param>
        public static void SetAndroidUserAgent(Mock<IRequest> request)
        {
            request.Setup(r => r.UserAgent).Returns("Mozilla/5.0 (Linux; U; Android 2.2; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1");
        }

        /// <summary>
        /// Sets a mock <see cref="IRequest"/> object up with a user agent string from an iPad.
        /// </summary>
        /// <param name="request"></param>
        public static void SetIPadUserAgent(Mock<IRequest> request)
        {
            request.Setup(r => r.UserAgent).Returns("Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B334b Safari/531.21.10");
        }

        /// <summary>
        /// Sets a mock <see cref="IRequest"/> object up with a user agent string from an iPhone.
        /// </summary>
        /// <param name="request"></param>
        public static void SetIPhoneUserAgent(Mock<IRequest> request)
        {
            request.Setup(r => r.UserAgent).Returns("Mozilla/5.0 (iPhone; U; CPU iOS 2_0 like Mac OS X; en-us) AppleWebKit/525.18.1 (KHTML, like Gecko) Version/3.1.1 Mobile/XXXXX Safari/525.20");
        }

        /// <summary>
        /// Sets a mock <see cref="IRequest"/> object up with a user agent string from an iPod.
        /// </summary>
        /// <param name="request"></param>
        public static void SetIPodUserAgent(Mock<IRequest> request)
        {
            request.Setup(r => r.UserAgent).Returns("Mozilla/5.0 (iPod; U; CPU like Mac OS X; en) AppleWebKit/420.1 (KHTML, like Gecko) Version/3.0 Mobile/4A93 Safari/419.3");
        }
    }
}
