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

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// A record from the log database that describes a single session from an IP address.
    /// </summary>
    public class LogSession
    {
        /// <summary>
        /// Gets or sets the unique ID of the session.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the client that made the session.
        /// </summary>
        public long ClientId { get; set; }

        /// <summary>
        /// Gets or sets the first non-null user name used in the session. Will be null if the user never logged in.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the time that the session was first established.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end of the session. While the session is in progress
        /// this holds the last time the database record was updated with the session
        /// details.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the number of requests the client made during the session.
        /// </summary>
        public long CountRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes sent in response to requests for miscellaneous files.
        /// </summary>
        public long OtherBytesSent { get; set; }

        /// <summary>
        /// Gets or sets the overall size of all of the web pages sent in the session.
        /// </summary>
        public long HtmlBytesSent { get; set; }

        /// <summary>
        /// Gets or sets the overall size of all of the JSON files sent in the session.
        /// </summary>
        public long JsonBytesSent { get; set; }

        /// <summary>
        /// Gets or sets the overall size of all of the images sent in the session.
        /// </summary>
        public long ImageBytesSent { get; set; }

        /// <summary>
        /// Gets or sets the overall size of all of the audio files sent in the session.
        /// </summary>
        public long AudioBytesSent { get; set; }

        /// <summary>
        /// Gets the total number of bytes sent.
        /// </summary>
        public long TotalBytesSent { get { return OtherBytesSent + HtmlBytesSent + JsonBytesSent + ImageBytesSent + AudioBytesSent; } }

        /// <summary>
        /// Gets the duration of the session.
        /// </summary>
        public TimeSpan Duration { get { return EndTime - StartTime; } }
    }
}
