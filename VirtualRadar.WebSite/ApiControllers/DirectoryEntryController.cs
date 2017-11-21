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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Handles requests for public directory information.
    /// </summary>
    public class DirectoryEntryController : PipelineApiController
    {
        /// <summary>
        /// Responds to requests from the VRS mothership for public directory information.
        /// </summary>
        /// <param name="key">The key that the user copied from their profile on the SDM site. If this is missing
        /// or it does not match the key entered in the configuration then we need to respond with a 404.</param>
        /// <returns></returns>
        /// <remarks>
        /// The mother ship only calls instances of VRS that have been added to the public directory by their owner.
        /// The owner copies a key out of their profile on the SDM site into the local VRS's configuration and
        /// periodically the SDM site calls this API entry with the key. The SDM site will assume that the VRS instance
        /// is offline if there is no response, otherwise it shows the counters returned by this call in the public
        /// directory.
        /// </remarks>
        [HttpGet]
        [Route("api/3.00/directory-entry/{key}")]
        [Route("DirectoryEntry.json")]                  // Pre-version 3 path
        [ResponseType(typeof(DirectoryEntryJson))]
        public IHttpActionResult GetDirectoryEntry(string key = null)
        {
            var configuration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton.Get();
            var keyInvalid = String.IsNullOrEmpty(key) || !String.Equals(key, configuration.GoogleMapSettings.DirectoryEntryKey, StringComparison.OrdinalIgnoreCase);

            return keyInvalid ? (IHttpActionResult)NotFound() : Content(HttpStatusCode.OK, BuildDirectoryEntry());
        }

        /// <summary>
        /// Builds the <see cref="DirectoryEntryJson"/> response to a request for directory information.
        /// </summary>
        /// <returns></returns>
        private DirectoryEntryJson BuildDirectoryEntry()
        {
            var feeds = Factory.Singleton.ResolveSingleton<IFeedManager>().VisibleFeeds.Where(r => r.AircraftList != null).ToArray();
            var maxAircraft = feeds.Select(r => {
                long unused1, unused2;
                var aircraftList = r?.AircraftList.TakeSnapshot(out unused1, out unused2);
                return aircraftList.Count;
            }).DefaultIfEmpty(0).Max();

            return new DirectoryEntryJson() {
                Version =           Factory.Singleton.Resolve<IApplicationInformation>().ShortVersion,
                NumberOfFeeds =     feeds.Length,
                NumberOfAircraft =  maxAircraft,
            };
        }
    }
}
