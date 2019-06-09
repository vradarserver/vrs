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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// Translates between real tile server URLs, as downloaded from the SDM site,
    /// and the fake URLs that we give to the web site.
    /// </summary>
    /// <remarks>
    /// The real URLs have information substituted into them by Leaflet, e.g. the x
    /// and y parameters. Our fake ones need the same substitution parameters but
    /// represented in a way that makes it relatively easy to extract them when we
    /// get requests for the fake URLs.
    /// </remarks>
    class TileServerUrlTranslator
    {
        /// <summary>
        /// Identifies URL parameters in a Leaflet URL.
        /// </summary>
        private static Regex _MatchSubstitutionMarker = new Regex(@"(\{([^}]*)\})");

        // URL path part names
        internal const string PageName = "TileServerCache";
        internal const string MapProviderName = "mp";
        internal const string TileServerName = "name";
        internal const string VariablePrefix = "var_";

        /// <summary>
        /// Returns the fake URL that corresponds to the tile server settings passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="tileServerSettings"></param>
        /// <returns></returns>
        public string ToFakeUrl(MapProvider mapProvider, TileServerSettings tileServerSettings)
        {
            var result = new StringBuilder();

            if(tileServerSettings != null) {
                result.Append($"{PageName}/");

                result.Append($"{MapProviderName}-{((int)mapProvider).ToString(CultureInfo.InvariantCulture)}/");
                result.Append($"{TileServerName}-{HttpUtility.UrlEncode(tileServerSettings.Name)}/");

                foreach(var substitutionMarker in ExtractSubstitutionMarkerNames(tileServerSettings.Url)) {
                    // Ignore subdomain substitutions, they're meaningless here
                    if(substitutionMarker == "s") {
                        continue;
                    }

                    result.Append($"{VariablePrefix}{HttpUtility.UrlEncode(substitutionMarker)}-{{{HttpUtility.UrlEncode(substitutionMarker)}}}/");
                }

                result.Append("Tile");
                result.Append(ExtractImageExtension(tileServerSettings.Url));
            }

            return result.ToString();
        }

        /// <summary>
        /// Extracts information out of the path parts for a fake URL that has had its substitution markers replaced with
        /// real values by Leaflet.
        /// </summary>
        /// <param name="pathParts">A collection of strings that represent the folders in a URL's path. The filename
        /// and scheme are missing. An empty collection indicates that the request was for root.</param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public FakeUrlEncodedValues ExtractEncodedValuesFromUrlParts(IEnumerable<string> pathParts, string fileName)
        {
            FakeUrlEncodedValues result = null;

            if(pathParts != null && String.Equals(pathParts.FirstOrDefault(), PageName, StringComparison.OrdinalIgnoreCase)) {
                result = new FakeUrlEncodedValues();

                foreach(var pathPart in pathParts) {
                    var hyphenIdx = pathPart.IndexOf('-');
                    if(hyphenIdx > 0) {
                        var name = pathPart.Substring(0, hyphenIdx);
                        var value = pathPart.Substring(hyphenIdx + 1);

                        if(name.StartsWith(VariablePrefix)) {
                            var substitutionMarker = name.Substring(VariablePrefix.Length);
                            switch(substitutionMarker) {
                                case "r":   result.Retina = value; break;
                                case "x":   result.X = value; break;
                                case "y":   result.Y = value; break;
                                case "z":   result.Zoom = value; break;
                                default:    result.OtherValues.Add(substitutionMarker, value); break;
                            }
                        } else {
                            switch(name) {
                                case MapProviderName:
                                    if(int.TryParse(value, out var mp) && Enum.IsDefined(typeof(MapProvider), mp)) {
                                        result.MapProvider = (MapProvider)mp;
                                    }
                                    break;
                                case TileServerName:
                                    result.Name = value.Trim();
                                    break;
                            }
                        }
                    }
                }

                result.TileImageExtension = String.IsNullOrEmpty(fileName) ? ".png" : Path.GetExtension(fileName);
            }

            return result;
        }

        /// <summary>
        /// Subtitutes the values from a fake encoded URL into a real tile server URL.
        /// </summary>
        /// <param name="urlWithMarkers"></param>
        /// <param name="values"></param>
        /// <param name="subdomains">Leaflet-style list of subdomains to substitute into the {s} parameter (if present). Defaults to abc if null / empty.</param>
        /// <returns></returns>
        public string ExpandUrlParameters(string urlWithMarkers, FakeUrlEncodedValues values, string subdomains)
        {
            var result = new StringBuilder(urlWithMarkers ?? "");

            if(String.IsNullOrEmpty(subdomains)) {
                subdomains = "abc";
            }

            foreach(var match in _MatchSubstitutionMarker.Matches(result.ToString()).OfType<Match>().OrderByDescending(r => r.Index)) {
                var value = "";
                var start = match.Groups[1].Index;
                var length = match.Groups[1].Length;
                var marker = match.Groups[2].Value;

                switch(marker) {
                    case "r":   value = values.Retina; break;
                    case "x":   value = values.X; break;
                    case "y":   value = values.Y; break;
                    case "z":   value = values.Zoom; break;
                    case "s":
                        var random = new Random();
                        var idx = random.Next(subdomains.Length);
                        value = new String(subdomains[idx], 1);
                        break;
                    default:
                        if(values.OtherValues.TryGetValue(marker, out var otherValue)) {
                            value = otherValue;
                        }
                        break;
                }

                result.Remove(start, length);
                result.Insert(start, value ?? "");
            }

            return result.ToString();
        }

        /// <summary>
        /// Extracts names of substitution markers out of the URL passed across.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the URL is 'http://abc.com/{x}/{y}/tile.png' then the result is a collection whose
        /// elements are 'x' and 'y'.
        /// </remarks>
        private List<string> ExtractSubstitutionMarkerNames(string url)
        {
            var result = new List<string>();

            foreach(Match match in _MatchSubstitutionMarker.Matches(url ?? "")) {
                result.Add(match.Groups[2].Value);
            }

            return result;
        }

        /// <summary>
        /// Extracts the extension of the image from the URL passed across.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ExtractImageExtension(string url)
        {
            var result = "";

            if(!String.IsNullOrEmpty(url)) {
                var queryIdx = url.IndexOf('?');
                var hostAndPath = queryIdx == -1 ? url : url.Substring(0, queryIdx);

                var dotIdx = hostAndPath.LastIndexOf('.');
                if(dotIdx != -1) {
                    result = hostAndPath.Substring(dotIdx);
                }
            }

            return result;
        }
    }
}
