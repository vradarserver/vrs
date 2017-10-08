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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Localisation;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Injects strings out of a RESX file into the web site strings JavaScript file requested
    /// by the browser.
    /// </summary>
    public class WebSiteStringsManipulator : ITextResponseManipulator
    {
        /// <summary>
        /// Map of culture names to localisers.
        /// </summary>
        private Dictionary<string, ResourceStrings> _ResourceStringsMap = new Dictionary<string, ResourceStrings>();

        /// <summary>
        /// The object that locks <see cref="_ResourceStringsMap"/> to a single thread.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="textContent"></param>
        public void ManipulateTextResponse(IDictionary<string, object> environment, Interface.TextContent textContent)
        {
            var javaScriptLanguage = ExtractJavaScriptLanguage(environment);
            if(javaScriptLanguage != null) {
                switch(javaScriptLanguage) {
                    case "en":      InjectResxProperties(textContent, "en-GB"); break;
                    case "de":      InjectResxProperties(textContent, "de-DE"); break;
                    case "fr":      InjectResxProperties(textContent, "fr-FR"); break;
                    case "pt-br":   InjectResxProperties(textContent, "pt-BR"); break;
                    case "ru":      InjectResxProperties(textContent, "ru-RU"); break;
                    case "tr":      InjectResxProperties(textContent, "tr-TR"); break;
                    case "zh":      InjectResxProperties(textContent, "zh-CN"); break;
                    // Do not throw an exception if not known, someone might be doing their own strings file
                }
            }
        }

        /// <summary>
        /// Returns the language from the JavaScript language filename or null if this isn't a JavaScript language file.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        private string ExtractJavaScriptLanguage(IDictionary<string, object> environment)
        {
            string result = null;
            const string pathAndFilePrefix = "/script/i18n/strings.";

            if(environment.TryGetValue("owin.RequestPath", out object owinPath)) {
                if(owinPath is string pathAndFile && pathAndFile.StartsWith(pathAndFilePrefix, StringComparison.OrdinalIgnoreCase)) {
                    var javaScriptLanguage = pathAndFile.Substring(pathAndFilePrefix.Length);
                    var dotIndex = javaScriptLanguage.IndexOf('.');
                    if(dotIndex != -1) {
                        var extension = javaScriptLanguage.Substring(dotIndex);
                        if(String.Equals(".js", extension, StringComparison.OrdinalIgnoreCase) ||
                           extension.StartsWith(".js?", StringComparison.OrdinalIgnoreCase)) {
                            result = javaScriptLanguage.Substring(0, dotIndex).ToLower();
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Injects VRS.$$.xyz strings built from the appropriate RESX file.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cultureName"></param>
        private void InjectResxProperties(Interface.TextContent textContent, string cultureName)
        {
            var resourceStrings = FetchResourceStrings(cultureName);
            if(resourceStrings != null) {
                var injectPoint = textContent.Content.IndexOf("    // [[ MARKER END SIMPLE STRINGS ]]");
                if(injectPoint != -1) {
                    textContent.Content = String.Format("{0}{1}{2}",
                        textContent.Content.Substring(0, injectPoint),
                        BuildLanguageStrings(resourceStrings),
                        textContent.Content.Substring(injectPoint)
                    );
                }
            }
        }

        /// <summary>
        /// Gets the resource strings for the culture passed across.
        /// </summary>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        private ResourceStrings FetchResourceStrings(string cultureName)
        {
            var resourceMap = _ResourceStringsMap;
            if(!resourceMap.TryGetValue(cultureName, out ResourceStrings result)) {
                lock(_SyncLock) {
                    if(!_ResourceStringsMap.TryGetValue(cultureName, out result)) {
                        resourceMap = CollectionHelper.ShallowCopy(_ResourceStringsMap);
                        result = new ResourceStrings(typeof(WebSiteStrings), cultureName);
                        resourceMap.Add(cultureName, result);
                        _ResourceStringsMap = resourceMap;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the JavaScript for all of the strings in the localised strings map passed across.
        /// </summary>
        /// <param name="stringsMap"></param>
        /// <returns></returns>
        private string BuildLanguageStrings(ResourceStrings stringsMap)
        {
            var result = new StringBuilder();

            foreach(var stringName in stringsMap.GetStringNames().OrderBy(r => r)) {
                result.AppendFormat("    VRS.$$.{0} = {1};\r\n", stringName, JavascriptHelper.FormatStringLiteral(stringsMap.GetLocalisedString(stringName)));
            }

            return result.ToString();
        }
    }
}
