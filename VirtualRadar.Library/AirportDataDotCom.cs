// Copyright © 2014 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using VirtualRadar.Interface;
using System.Web;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAirportDataDotCom"/>.
    /// </summary>
    class AirportDataDotCom : IAirportDataDotCom
    {
        #region Constant fields
        /// <summary>
        /// The timeout for requests for thumbnails
        /// </summary>
        const int ThumbnailTimeout = 10000;

        /// <summary>
        /// The timeout for entries in the thumbnail cache in minutes. Entries that have not
        /// been accessed within this period of time are automatically removed.
        /// </summary>
        const int ThumbnailCacheMaxMinutes = 60;
        #endregion

        #region Private classes
        /// <summary>
        /// A private class that acts as a key into the thumbnail cache.
        /// </summary>
        class ThumbnailKey
        {
            public string Icao { get; private set; }
            public int MaxThumbnails { get; private set; }

            public ThumbnailKey(string icao, int maxThumbnails)
            {
                Icao = icao;
                MaxThumbnails = maxThumbnails;
            }

            public override string ToString()
            {
                return String.Format("{0}:{1}", Icao, MaxThumbnails);
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as ThumbnailKey;
                    result = other != null && other.Icao == Icao && other.MaxThumbnails == MaxThumbnails;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return (Icao ?? "").GetHashCode();
            }
        }

        /// <summary>
        /// A private class that holds the cache result for a thumbnail.
        /// </summary>
        class CachedThumbnail
        {
            public WebRequestResult<AirportDataThumbnailsJson> Thumbnail { get; set; }

            public DateTime LastAccessTimeUtc { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// A cache of recent thumbnail requests and their responses.
        /// </summary>
        private static Dictionary<ThumbnailKey, CachedThumbnail> _ThumbnailCache = new Dictionary<ThumbnailKey,CachedThumbnail>();

        /// <summary>
        /// The date and time at UTC of the last cleanup of the cache.
        /// </summary>
        private static DateTime _ThumbnailCacheLastCleanTime;

        /// <summary>
        /// The spin lock that controls multi-threaded access to the thumbnail cache. Use carefully!
        /// </summary>
        private static SpinLock _ThumbnailCacheSpinLock = new SpinLock();
        #endregion

        #region GetThumbnails
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="registration"></param>
        /// <param name="maxThumbnails"></param>
        /// <returns></returns>
        public WebRequestResult<AirportDataThumbnailsJson> GetThumbnails(string icao, string registration, int maxThumbnails)
        {
            var thumbnailKey = new ThumbnailKey(icao, maxThumbnails);
            CachedThumbnail cachedThumbnail;
            using(var spinLock = _ThumbnailCacheSpinLock.AcquireLock()) {
                _ThumbnailCache.TryGetValue(thumbnailKey, out cachedThumbnail);
            }

            if(cachedThumbnail != null) {
                cachedThumbnail.LastAccessTimeUtc = DateTime.UtcNow;
            } else {
                cachedThumbnail = new CachedThumbnail() {
                    LastAccessTimeUtc = DateTime.UtcNow,
                    Thumbnail = RequestThumbnails(icao, registration, maxThumbnails),
                };
                using(var spinLock = _ThumbnailCacheSpinLock.AcquireLock()) {
                    if(_ThumbnailCache.ContainsKey(thumbnailKey)) _ThumbnailCache[thumbnailKey] = cachedThumbnail;
                    else                                          _ThumbnailCache.Add(thumbnailKey, cachedThumbnail);
                }
            }

            CleanOldThumbnailCacheEntries();

            return cachedThumbnail.Thumbnail;
        }

        private WebRequestResult<AirportDataThumbnailsJson> RequestThumbnails(string icao, string registration, int maxThumbnails)
        {
            var requestUrl = String.Format("http://www.airport-data.com/api/ac_thumb.json?m={0}&r={1}&n={2}", HttpUtility.UrlEncode(icao), HttpUtility.UrlEncode(registration ?? ""), maxThumbnails);
            var request = HttpWebRequest.Create(requestUrl);
            request.Timeout = ThumbnailTimeout;

            var result = new WebRequestResult<AirportDataThumbnailsJson>();
            try {
                using(var response = (HttpWebResponse)request.GetResponse()) {
                    result.HttpStatusCode = response.StatusCode;
                    if(result.HttpStatusCode == HttpStatusCode.OK) {
                        using(var responseStream = response.GetResponseStream()) {
                            var deserialiser = new DataContractJsonSerializer(typeof(AirportDataThumbnailsJson));
                            result.Result = (AirportDataThumbnailsJson)deserialiser.ReadObject(responseStream);
                        }
                    }
                }
            } catch(WebException ex) {
                var webResponse = ex.Response as HttpWebResponse;
                if(webResponse != null) result.HttpStatusCode = webResponse.StatusCode;
                else                    throw;
            }

            return result;
        }

        /// <summary>
        /// Removes out-of-date thumbnail cache entries.
        /// </summary>
        private void CleanOldThumbnailCacheEntries()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-ThumbnailCacheMaxMinutes);
            using(var spinLock = _ThumbnailCacheSpinLock.AcquireLock()) {
                if(_ThumbnailCacheLastCleanTime <= DateTime.UtcNow.AddMinutes(-1)) {
                    foreach(var kvp in _ThumbnailCache.Where(r => r.Value.LastAccessTimeUtc <= threshold).ToList()) {
                        _ThumbnailCache.Remove(kvp.Key);
                    }
                    _ThumbnailCacheLastCleanTime = DateTime.UtcNow;
                }
            }
        }
        #endregion
    }
}
