﻿// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the unique identifier of the merged feed. This value is unique across <see cref="MergedFeed"/> and <see cref="Receiver"/> records. It cannot be zero.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the name that the merged feed will be known by.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets a list of the IDs for the receivers that are going to be merged into this feed.
        /// </summary>
        /// <remarks><para>
        /// Originally this was the only list of receivers on a merged feed. Unfortunately because it's
        /// just a list of integers and not a list of objects I couldn't store any information that was
        /// unique to a receiver in a merged feed. That caused problems, so the <see cref="ReceiverFlags"/>
        /// list was added.
        /// </para><para>
        /// I could not just abandon ReceiverIds without causing backwards compatability issues, so this
        /// property continues to be sole source of receiver IDs for the merged feed. The user is responsible
        /// for adding and removing entries in this list. The <see cref="ReceiverFlags"/> entries are
        /// maintained by the configuration GUI, it tries to ensure that each receiver in ReceiverIds gets
        /// a mirror entry in ReceiverFlags but it may not be a perfect mirror.
        /// </para><para>
        /// Always use ReceiverIds when building the list of receivers to merge together.
        /// </para></remarks>
        public IList<int> ReceiverIds { get; } = new List<int>();

        /// <summary>
        /// Gets a list of settings stored against each receiver that is merged into this feed.
        /// </summary>
        /// <remarks>
        /// See the comments against <see cref="ReceiverIds"/>. The configuration GUI is reponsible for adding
        /// and removing entries in this list. There may be entries in this list that are not in <see cref="ReceiverIds"/>,
        /// in which case those entries should be ignored. This list is subordinate to <see cref="ReceiverIds"/>,
        /// it only carries extra information that could not be held by <see cref="ReceiverIds"/> without
        /// breaking backwards compatibility.
        /// </remarks>
        public IList<MergedFeedReceiver> ReceiverFlags { get; } = new List<MergedFeedReceiver>();

        /// <summary>
        /// Gets or sets the number of milliseconds that any given receiver will be considered to be the only source of messages for an ICAO.
        /// </summary>
        public int IcaoTimeout { get; set; } = 3000;

        /// <summary>
        /// Gets or sets a value indicating that aircraft are ignored until they report a position that isn't 0/0.
        /// </summary>
        public bool IgnoreAircraftWithNoPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how the merged feed is going to be used by the system.
        /// </summary>
        public ReceiverUsage ReceiverUsage { get; set; } = ReceiverUsage.Normal;
    }
}
