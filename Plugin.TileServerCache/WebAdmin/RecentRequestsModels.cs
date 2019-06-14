// Copyright © 2019 onwards, Andrew Whewell
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
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.TileServerCache.WebAdmin
{
    /// <summary>
    /// Describes a recent request.
    /// </summary>
    public class RecentRequestModel
    {
        public long ID { get; set; }

        public string ReceivedUtc { get; set; }

        public string DurationMs { get; set; }

        public string TileServerName { get; set; }

        public string Zoom { get; set; }

        public string X { get; set; }

        public string Y { get; set; }

        public string Retina { get; set; }

        public string Outcome { get; set; }

        internal static RecentRequestModel FromRequestOutcome(RequestOutcome requestOutcome)
        {
            return new RecentRequestModel() {
                ID =                requestOutcome.ID,
                ReceivedUtc =       requestOutcome.ReceivedUtc.ToString("HH:mm:ss.fff"),
                DurationMs =        requestOutcome.CompletedUtc == null ? "" : requestOutcome.DurationMs.Value.ToString("N0"),
                TileServerName =    requestOutcome.TileServerName ?? "",
                Zoom =              requestOutcome.Zoom ?? "",
                X =                 requestOutcome.X ?? "",
                Y =                 requestOutcome.Y ?? "",
                Retina =            requestOutcome.Retina ?? "",
                Outcome =           requestOutcome.Outcome ?? "",
            };
        }
    }

    /// <summary>
    /// Carries the recent requests to the HTML page.
    /// </summary>
    public class RecentRequestsViewModel
    {
        public List<RecentRequestModel> RecentRequests { get; set; } = new List<RecentRequestModel>();

        public RecentRequestsViewModel()
        {
        }

        internal RecentRequestsViewModel(IEnumerable<RequestOutcome> requestOutcomes) : this()
        {
            RefreshFromModel(requestOutcomes);
        }

        internal void RefreshFromModel(IEnumerable<RequestOutcome> requestOutcomes)
        {
            RecentRequests.Clear();
            foreach(var requestOutcome in requestOutcomes) {
                RecentRequests.Add(
                    RecentRequestModel.FromRequestOutcome(requestOutcome)
                );
            }
        }
    }
}
