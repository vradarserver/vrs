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
using System.Text;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface that all rebroadcast server format providers must implement.
    /// </summary>
    public interface IRebroadcastFormatProvider
    {
        /// <summary>
        /// The unique identifier for the rebroadcast format. These are written to the configuration file
        /// so avoid changing them. Do not reuse IDs. Plugins should ensure that their IDs cannot clash
        /// with other plugins.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// Gets a short localised description of the format.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets a value indicating that the server can rebroadcast a merged feed.
        /// </summary>
        bool CanRebroadcastMergedFeed { get; }

        /// <summary>
        /// Gets a value indicating that the server uses the receiver's aircraft list.
        /// </summary>
        bool UsesReceiverAircraftList { get; }

        /// <summary>
        /// Gets a value indicating that the server sends in bursts with pauses of
        /// SendIntervalMilliseconds milliseconds between each send.
        /// </summary>
        bool UsesSendIntervalMilliseconds { get; }

        /// <summary>
        /// Gets or sets the rebroadcast server that has created this instance of the provider.
        /// </summary>
        IRebroadcastServer RebroadcastServer { get; set; }

        /// <summary>
        /// Returns true if the send interval passed across is acceptable.
        /// </summary>
        /// <param name="sendIntervalMilliseconds"></param>
        /// <returns></returns>
        bool IsValidSendIntervalMilliseconds(int sendIntervalMilliseconds);

        /// <summary>
        /// Returns true if the server can rebroadcast a receiver with the settings passed across.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        bool CanRebroadcastReceiver(Receiver receiver);

        /// <summary>
        /// Creates a new instance of the provider.
        /// </summary>
        /// <returns></returns>
        IRebroadcastFormatProvider CreateNewInstance();

        /// <summary>
        /// Hook the events that the rebroadcast server needs to listen to in order to rebroadcast
        /// a feed.
        /// </summary>
        /// <remarks>
        /// Implementations should record the objects that they hook and use those references
        /// in <see cref="UnhookFeed"/>. The feed's properties may change between the hook
        /// and unhook calls, do not just record a reference to the feed.
        /// </remarks>
        void HookFeed();

        /// <summary>
        /// Unhooks the events that were hooked by <see cref="HookFeed"/>.
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="connector"></param>
        void UnhookFeed();

        /// <summary>
        /// Called when the send interval has elapsed.
        /// </summary>
        void SendIntervalElapsed();
    }
}
