// Copyright © 2014 onwards, Andrew Whewell
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
using System.Runtime.Serialization;
using System.Text;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// A DTO class that carries information about a web server request.
    /// </summary>
    [DataContract]
    public class ServerRequest : ICloneable
    {
        /// <summary>
        /// Gets or sets the remote endpoint IP address.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// Gets or sets a value that is incremented any time there is a change of value for the feed.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the user that is making the request, if any. Null if anonymous.
        /// </summary>
        [DataMember(Name="User")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets the address of the machine that made the request.
        /// </summary>
        [DataMember(Name="RemoteAddr")]
        public string RemoteAddress { get { return RemoteEndPoint == null ? null : RemoteEndPoint.Address.ToString(); } }

        /// <summary>
        /// Gets the port that the response is to go to.
        /// </summary>
        [DataMember(Name="RemotePort")]
        public int RemotePort { get { return RemoteEndPoint == null ? 0 : RemoteEndPoint.Port; } }

        /// <summary>
        /// Gets or sets the date and time of the last request made from the <see cref="RemoteEndPoint"/>.
        /// </summary>
        [DataMember]
        public DateTime LastRequest;

        /// <summary>
        /// Gets or sets the number of bytes sent to the <see cref="RemoteEndPoint"/>.
        /// </summary>
        [DataMember(Name="Bytes")]
        public long BytesSent;

        /// <summary>
        /// Gets or sets the last URL served to the <see cref="RemoteEndPoint"/>.
        /// </summary>
        [DataMember]
        public string LastUrl;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}:{1} ({2:N0})", RemoteEndPoint, LastUrl, BytesSent);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var result = Activator.CreateInstance(GetType()) as ServerRequest;

            result.BytesSent = BytesSent;
            result.DataVersion = DataVersion;
            result.LastRequest = LastRequest;
            result.LastUrl = LastUrl;
            result.RemoteEndPoint = RemoteEndPoint;
            result.UserName = UserName;

            return result;
        }
    }
}
