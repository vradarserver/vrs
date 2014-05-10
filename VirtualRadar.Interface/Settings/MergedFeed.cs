// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The configuration settings for a merged feed.
    /// </summary>
    public class MergedFeed
    {
        /// <summary>
        /// Gets or sets a value indicating that the merged feed is to be used.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the merged feed. This value is unique across <see cref="MergedFeed"/> and <see cref="Receiver"/> records. It cannot be zero.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the name that the merged feed will be known by.
        /// </summary>
        public string Name { get; set; }

        private List<int> _ReceiverIds = new List<int>();
        /// <summary>
        /// Gets a list of the IDs for the receivers that are going to be merged into this feed.
        /// </summary>
        public List<int> ReceiverIds { get { return _ReceiverIds; } }

        /// <summary>
        /// Gets or sets the number of milliseconds that any given receiver will be considered to be the only source of messages for an ICAO.
        /// </summary>
        public int IcaoTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that aircraft are ignored until they report a position that isn't 0/0.
        /// </summary>
        public bool IgnoreAircraftWithNoPosition { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MergedFeed()
        {
            Enabled = true;
            IcaoTimeout = 3000;
        }
    }
}
