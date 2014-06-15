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

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The common class for all feed parent pages.
    /// </summary>
    public class FeedParentPage : ParentPage
    {
        /// <summary>
        /// See base class. Ensures that all references to deleted receiver or merged feed IDs are erased from the rest of the view.
        /// </summary>
        protected override void DoUpdateViewWithNewList()
        {
            var feedIds = FeedTypeConverter.BuildAllIds().ToList();
            var receiverIds = OptionsView.Receivers.Select(r => r.UniqueId).ToArray();

            ResetMissingSingleReceiverId(feedIds, OptionsView.WebSiteReceiverId, () => { OptionsView.WebSiteReceiverId = 0; });
            ResetMissingSingleReceiverId(feedIds, OptionsView.ClosestAircraftReceiverId, () => { OptionsView.ClosestAircraftReceiverId = 0; });
            ResetMissingSingleReceiverId(feedIds, OptionsView.FlightSimulatorXReceiverId, () => { OptionsView.FlightSimulatorXReceiverId = 0; });

            foreach(var rebroadcastServer in OptionsView.RebroadcastSettings) {
                var receiverStillExists = feedIds.Any(r => r == rebroadcastServer.ReceiverId);
                if(!receiverStillExists) rebroadcastServer.ReceiverId = 0;
            }

            foreach(var mergedFeed in OptionsView.MergedFeeds) {
                var existingIds = mergedFeed.ReceiverIds.Intersect(receiverIds).ToArray();
                mergedFeed.ReceiverIds.Clear();
                mergedFeed.ReceiverIds.AddRange(existingIds);
            }
        }

        private void ResetMissingSingleReceiverId(List<int> feedIds, int receiverId, Action resetId)
        {
            var receiverStillExists = feedIds.Any(r => r == receiverId);
            if(!receiverStillExists) resetId();
        }

        /// <summary>
        /// Generates a unique ID for the type passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="getUniqueId"></param>
        /// <returns></returns>
        protected override int GenerateUniqueId<T>(List<T> records, Func<T, int> getUniqueId)
        {
            throw new InvalidOperationException("Use the overridden GenerateUniqueId instead");
        }

        /// <summary>
        /// Generates a unique identifier.
        /// </summary>
        /// <returns></returns>
        protected int GenerateUniqueId()
        {
            return base.GenerateUniqueId(FeedTypeConverter.BuildAllIds().ToList(), r => r);
        }
    }
}
