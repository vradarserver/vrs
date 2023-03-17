// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// The listener for a receiver that will be merged into a single feed by <see cref="IMergedFeedListener"/>.
    /// </summary>
    /// <remarks>
    /// Implementations are expected to be immutable. They need to implement Equals, returning true if the other
    /// object has the same instance of <see cref="Listener"/> and same value for <see cref="IsMlatFeed"/>.
    /// </remarks>
    public interface IMergedFeedComponentListener
    {
        /// <summary>
        /// Gets the listener for the receiver.
        /// </summary>
        IListener Listener { get; }

        /// <summary>
        /// Gets or sets a value indicating that every position message on the feed should be treated as an MLAT message.
        /// </summary>
        bool IsMlatFeed { get; }

        /// <summary>
        /// Initialises the object. Objects can only be initialised once.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="isMlatFeed"></param>
        void SetListener(IListener listener, bool isMlatFeed);
    }
}
