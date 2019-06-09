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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// An object that can handle requests for cached tiles.
    /// </summary>
    class WebRequestHandler
    {
        /// <summary>
        /// The next ID to assign to a request outcome object.
        /// </summary>
        private static long _NextID = 0;

        /// <summary>
        /// Controls multi-threaded access to <see cref="_TileServerNameToCookieCollection" /> and the cookies contained therein.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A map of cookies indexed by tile server
        /// </summary>
        private Dictionary<string, CookieCollection> _TileServerNameToCookieCollection = new Dictionary<string, CookieCollection>();

        /// <summary>
        /// Translates URLs for us.
        /// </summary>
        private TileServerUrlTranslator _TileServerUrlTranslator = new TileServerUrlTranslator();

        /// <summary>
        /// Gets a collection of recent requests and their outcomes.
        /// </summary>
        public ExpiringList<RequestOutcome> RecentRequestOutcomes { get; } = new ExpiringList<RequestOutcome>(
            expireMilliseconds:         10 * 60 * 1000,
            millisecondsBetweenChecks:  5 * 1000
        );

        /// <summary>
        /// Returns true if the path parts indicate a request for a cached tile.
        /// </summary>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public bool IsTileServerCacheRequest(IEnumerable<string> pathParts)
        {
            return pathParts != null
                && pathParts.Any()
                && String.Equals(pathParts.First(), TileServerUrlTranslator.PageName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles the request for a cached tile.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="pathParts">The path parts from the request.</param>
        /// <param name="fileName">The filename from the request.</param>
        /// <param name="clientEndpoint"></param>
        /// <param name="headers">The request headers.</param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public WebRequestOutcome ProcessRequest(Options options, IEnumerable<string> pathParts, string fileName, IPAddress clientEndpoint, NameValueCollection headers, string userAgent)
        {
            var result = new WebRequestOutcome();
            var displayOutcome = new RequestOutcome() {
                ID = Interlocked.Increment(ref _NextID),
                ReceivedUtc = DateTime.UtcNow,
            };
            RecentRequestOutcomes.Add(displayOutcome);

            try {
                var urlValues = _TileServerUrlTranslator.ExtractEncodedValuesFromUrlParts(pathParts, fileName);
                displayOutcome.CopyUrlValues(urlValues);

                if(urlValues != null) {
                    var cachedContent = TileCache.GetCachedTile(urlValues);
            
                    if(cachedContent != null) {
                        result.ImageBytes = cachedContent.Content;
                        displayOutcome.ServedFromCache = true;
                    } else {
                        if(options.IsOfflineModeEnabled) {
                            displayOutcome.MissingFromCache = true;
                            result.StatusCode = HttpStatusCode.NotFound;
                        } else {
                            FetchTileServerImage(options, urlValues, clientEndpoint, headers, userAgent, result, displayOutcome);

                            if(result.ImageBytes?.Length > 0) {
                                TileCache.SaveTile(urlValues, result.ImageBytes);
                            }
                        }
                    }
            
                    if(result.ImageBytes != null) {
                        result.StatusCode =     HttpStatusCode.OK;
                        result.ImageExtension = urlValues.TileImageExtension;
                    }
                }
            } catch(ThreadAbortException) {
                displayOutcome.Abandoned = true;
                ; // .NET will rethrow this automatically
            } catch {
                displayOutcome.ExceptionEncountered = true;
                throw;
            } finally {
                try {
                    displayOutcome.CompletedUtc = DateTime.UtcNow;
                } catch {
                    ; // Don't want to confuse the issue with exceptions when trying to update the outcome display
                }
            }

            return result;
        }

        /// <summary>
        /// Downloads the tile image using the URL values passed across.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="urlValues"></param>
        /// <param name="clientEndpoint"></param>
        /// <param name="headers"></param>
        /// <param name="userAgent"></param>
        /// <param name="outcome"></param>
        /// <param name="displayOutcome"></param>
        /// <returns></returns>
        private void FetchTileServerImage(Options options, FakeUrlEncodedValues urlValues, IPAddress clientEndpoint, NameValueCollection headers, string userAgent, WebRequestOutcome outcome, RequestOutcome displayOutcome)
        {
            var tileServerSettings = Plugin.TileServerSettingsManagerWrapper.GetRealTileServerSettings(
                urlValues.MapProvider,
                urlValues.Name
            );

            if(tileServerSettings != null) {
                var tileImageUrl = _TileServerUrlTranslator.ExpandUrlParameters(
                    tileServerSettings.Url,
                    urlValues,
                    tileServerSettings.Subdomains
                );

                var request = (HttpWebRequest)HttpWebRequest.Create(tileImageUrl);
                request.Method = "GET";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                foreach(var headerKey in headers.AllKeys) {
                    var value = headers[headerKey];
                    switch(headerKey.ToLower()) {
                        case "accept":              request.Accept = value; break;
                        case "accept-encoding":     break;
                        case "connection":          break;
                        case "content-length":      break;
                        case "content-type":        request.ContentType = value; break;
                        case "date":                break;
                        case "expect":              request.Expect = value; break;
                        case "host":                break;
                        case "if-modified-since":   break;
                        case "proxy-connection":    break;
                        case "range":               break;
                        case "referer":             request.Referer = value; break;
                        case "transfer-encoding":   request.TransferEncoding = value; break;
                        case "user-agent":          request.UserAgent = value; break;
                        default:
                            request.Headers[headerKey] = value;
                            break;
                    }
                }
                request.AuthenticationLevel =   AuthenticationLevel.None;
                request.Credentials =           null;
                request.UseDefaultCredentials = false;
                request.KeepAlive =             true;
                request.Timeout =               1000 * options.TileServerTimeoutSeconds;

                lock(_SyncLock) {
                    if(!_TileServerNameToCookieCollection.TryGetValue(urlValues.Name, out var cookies)) {
                        cookies = new CookieCollection();
                        _TileServerNameToCookieCollection.Add(urlValues.Name, cookies);
                    }
                    foreach(Cookie cookie in cookies) {
                        request.CookieContainer.Add(cookie);
                    }
                }

                if(!String.IsNullOrEmpty(userAgent)) {
                    request.UserAgent = userAgent;
                }

                var forwarded = request.Headers["Forwarded"] ?? "";
                if(forwarded.Length > 0) {
                    forwarded = $"{forwarded}, ";
                }
                forwarded = $"{forwarded}for={clientEndpoint}";
                request.Headers["Forwarded"] = forwarded;

                try {
                    using(var response = (HttpWebResponse)request.GetResponse()) {
                        using(var memoryStream = new MemoryStream()) {
                            using(var responseStream = response.GetResponseStream()) {
                                var buffer = new byte[1024];
                                var bytesRead = 0;
                                do {
                                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                                    if(bytesRead > 0) {
                                        memoryStream.Write(buffer, 0, bytesRead);
                                    }
                                } while(bytesRead > 0);
                            }

                            outcome.ImageBytes = memoryStream.ToArray();
                            displayOutcome.FetchedFromTileServer = true;
                            displayOutcome.TileServerResponseStatusCode = (int)response.StatusCode;
                        }

                        var cookies = new CookieCollection();
                        foreach(Cookie cookie in response.Cookies) {
                            cookies.Add(cookie);
                        }
                        lock(_SyncLock) {
                            _TileServerNameToCookieCollection[urlValues.Name] = cookies;
                        }
                    }
                } catch(WebException ex) {
                    if(ex.Status == WebExceptionStatus.Timeout) {
                        displayOutcome.TimedOut = true;
                    } else {
                        displayOutcome.WebExceptionErrorMessage = ex.Message;

                        outcome.StatusCode = HttpStatusCode.InternalServerError;
                        if(ex.Response is HttpWebResponse httpResponse) {
                            if(httpResponse.StatusCode != HttpStatusCode.OK) {
                                outcome.StatusCode = httpResponse.StatusCode;
                                displayOutcome.TileServerResponseStatusCode = (int)httpResponse.StatusCode;
                            }
                        }
                    }
                }
            }
        }
    }
}
