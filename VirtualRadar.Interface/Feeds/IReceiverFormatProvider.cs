// Copyright © 2016 onwards, Andrew Whewell
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
    /// The interface that all receiver format providers must implement.
    /// </summary>
    public interface IReceiverFormatProvider
    {
        /// <summary>
        /// Gets the unique identifier of the format provider.
        /// </summary>
        /// <remarks>
        /// These values are saved in the user's configuration, choose your ID carefully and don't change it
        /// between releases. Do not re-use identifiers. Plugins should ensure that their unique ID cannot
        /// clash with other plugins.
        /// </remarks>
        string UniqueId { get; }

        /// <summary>
        /// Gets the short name for the format.
        /// </summary>
        /// <returns></returns>
        string ShortName { get; }

        /// <summary>
        /// Gets a value indicating that the feed format holds raw messages from the aircraft that need to be decoded.
        /// </summary>
        bool IsRawFormat { get; }

        /// <summary>
        /// Creates and returns a new message bytes extractor for the format.
        /// </summary>
        /// <returns></returns>
        IMessageBytesExtractor CreateMessageBytesExtractor();

        /// <summary>
        /// Returns true if the message bytes extractor passed across is appropriate for this receiver format.
        /// </summary>
        /// <param name="extractor"></param>
        /// <returns></returns>
        bool IsUsableBytesExtractor(IMessageBytesExtractor extractor);
    }
}
