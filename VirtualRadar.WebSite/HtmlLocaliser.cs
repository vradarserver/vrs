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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IHtmlLocaliser"/>.
    /// </summary>
    public class HtmlLocaliser : IHtmlLocaliser
    {
        private CultureInfo _MainThreadCultureInfo;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public LocalisedStringsMap LocalisedStringsMap { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            Initialise(Localise.VirtualRadarStrings);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="localisedStringsMap"></param>
        public void Initialise(LocalisedStringsMap localisedStringsMap)
        {
            LocalisedStringsMap = localisedStringsMap;
            _MainThreadCultureInfo = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.MainThreadCultureInfo;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="encoding"></param>
        public string Html(string html, Encoding encoding)
        {
            var result = html;

            var document = new HtmlAgilityPack.HtmlDocument() {
                OptionCheckSyntax = false,
                OptionDefaultStreamEncoding = encoding,
            };
            document.LoadHtml(html);

            if(Html(document)) {
                using(var stream = new MemoryStream()) {
                    document.Save(stream);
                    stream.Position = 0;
                    using(var streamReader = new StreamReader(stream, encoding, true)) {
                        result = streamReader.ReadToEnd();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces all instances of ::STRING:: with the localised text.
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns>True if the document was modified, false if it was not.</returns>
        private bool Html(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            var result = false;

            using(new CultureSwitcher()) {
                foreach(var element in htmlDocument.DocumentNode.ChildNodes) {
                    if(HtmlNode(element)) result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces all instances of ::STRING:: with the localised text in an Html Agility Pack HtmlNode.
        /// </summary>
        /// <param name="htmlNode"></param>
        /// <returns></returns>
        private bool HtmlNode(HtmlAgilityPack.HtmlNode htmlNode)
        {
            var result = false;

            if(htmlNode.NodeType == HtmlAgilityPack.HtmlNodeType.Text) {
                var text = htmlNode.InnerHtml;
                var newText = GetLocalisedText(text);
                if(text != newText) {
                    result = true;
                    htmlNode.InnerHtml = newText;
                }
            }

            foreach(var attribute in htmlNode.Attributes) {
                var text = attribute.Value;
                var newText = GetLocalisedText(text);
                if(text != newText) {
                    result = true;
                    attribute.Value = newText;
                }
            }

            foreach(var childNode in htmlNode.ChildNodes) {
                if(HtmlNode(childNode)) result = true;
            }

            return result;
        }

        /// <summary>
        /// Returns the localised version of the text passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>If the text starts and ends with :: then the inner part is taken to be the name of a string in Strings
        /// and it's returned.</remarks>
        public string GetLocalisedText(String text)
        {
            string result = text;

            if(!String.IsNullOrEmpty(result) && result.Length > 4) {
                var index = result.IndexOf("::");
                while(index != -1) {
                    var endIndex = result.IndexOf("::", index + 2);
                    if(endIndex == -1) break;
                    var originalText = result.Substring(index + 2, endIndex - (index + 2));
                    var replaceWith = ConvertResourceStringToHtmlString(LocalisedStringsMap.TryGetLocalisedString(originalText));
                    if(replaceWith != null) {
                        result = String.Format("{0}{1}{2}", result.Substring(0, index), replaceWith, result.Substring(endIndex + 2));
                        endIndex -= originalText.Length + 2;        // Length of "::STRING" portion of original "::STRING::", end index is on 3rd colon
                        endIndex += replaceWith.Length;             // Should now be on 1st character after substitution
                    }
                    index = result.IndexOf("::", endIndex);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="resourceString"></param>
        /// <returns></returns>
        public string ConvertResourceStringToHtmlString(string resourceString)
        {
            var result = resourceString;
            if(result != null) {
                result = HttpUtility.HtmlEncode(resourceString);
                result = result.Replace("'", "&#39;");      // <-- not required for .NET 4, is required for .NET 3.5
                result = result.Replace("\r\n", "<br />");
            }

            return result;
        }
    }
}
