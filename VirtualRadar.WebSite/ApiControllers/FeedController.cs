// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using System.Web.Http;
using InterfaceFactory;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// API methods that deal with aircraft data feeds.
    /// </summary>
    public class FeedController : PipelineApiController
    {
        /// <summary>
        /// Returns a list of every public facing feed.
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/1.00/feeds")]
        public FeedJson[] GetFeeds()
        {
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            return feedManager.VisibleFeeds.Select(r => FeedJson.ToModel(r)).Where(r => r != null).ToArray();
        }

        /// <summary>
        /// Returns details for a single feed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("api/1.00/feed/{id}")]
        public FeedJson GetFeed(int id)
        {
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            return FeedJson.ToModel(feedManager.GetByUniqueId(id, ignoreInvisibleFeeds: true));
        }

        /// <summary>
        /// Returns the polar plot for a feed.
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/1.00/feed/{feedId}/polar-plot")]
        [Route("PolarPlot.json")]                       // pre-version 3 route
        public PolarPlotsJson GetPolarPlot(int feedId = -1)
        {
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            var feed = feedManager.GetByUniqueId(feedId, ignoreInvisibleFeeds: true);
            var plotter = feed?.AircraftList?.PolarPlotter;

            if(plotter != null && PipelineRequest.IsInternet) {
                var configuration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton.Get();
                if(!configuration.InternetClientSettings.CanShowPolarPlots) {
                    plotter = null;
                }
            }

            var result = plotter != null ? PolarPlotsJson.ToModel(feed.UniqueId, plotter) : new PolarPlotsJson() { FeedId = feedId, };

            return result;
        }
    }
}
