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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// Generates the JavaScript for an object that holds a resource's translated strings.
    /// </summary>
    class JavaScriptTranslations
    {
        /// <summary>
        /// Gets or sets the JavaScript file built from the <see cref="HtmlLocaliser"/>.
        /// </summary>
        public string JavaScript { get; set; }

        /// <summary>
        /// Gets the localiser used to build the <see cref="JavaScript"/>.
        /// </summary>
        public IHtmlLocaliser HtmlLocaliser { get; set; }

        /// <summary>
        /// Gets the namespace used in the JavaScript.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// True if the <see cref="JavaScript"/> includes a dummy Globalize object.
        /// </summary>
        public bool IncludesGlobalize { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="htmlLocaliser"></param>
        /// <param name="namespace"></param>
        /// <param name="addGlobalizeObject"></param>
        public JavaScriptTranslations(IHtmlLocaliser htmlLocaliser, string @namespace, bool addGlobalizeObject = false)
        {
            HtmlLocaliser = htmlLocaliser;
            Namespace = @namespace;
            IncludesGlobalize = addGlobalizeObject;

            GenerateJavaScript();
        }

        /// <summary>
        /// Generates the <see cref="JavaScript"/> string.
        /// </summary>
        private void GenerateJavaScript()
        {
            var hasNamespace = !String.IsNullOrEmpty(Namespace);
            var objectName = !hasNamespace ? "VRS.$$" : String.Format("VRS.{0}.$$", Namespace);
            var builder = new StringBuilder();

            using(var cultureSwitcher = new CultureSwitcher()) {
                var cultureInfo = Thread.CurrentThread.CurrentCulture;
                var localisedStrings = HtmlLocaliser.LocalisedStringsMap;

                builder.AppendLine(@"(function(VRS, undefined)");      // Note that this can be loaded before jQuery, so the usual parameter set is inappropriate
                builder.AppendLine(@"{");
                if(hasNamespace) {
                    builder.AppendLine(String.Format(@"    VRS.{0} = VRS.{0} || {{}}", Namespace));
                }
                builder.AppendLine(String.Format(@"    {0} = {0} || {{}};", objectName));
                builder.AppendLine();
                foreach(var name in localisedStrings.GetStringNames().OrderBy(r => r)) {
                    var value = localisedStrings.GetLocalisedString(name);
                    value = HtmlLocaliser.ConvertResourceStringToJavaScriptString(value, '\'');
                    builder.AppendLine(String.Format("    {0}.{1} = '{2}';",
                        objectName,
                        name,
                        value
                    ));
                }
                builder.AppendLine();

                if(IncludesGlobalize) {
                    builder.AppendLine(@"    Globalize = {");
                    builder.AppendLine(@"        culture: function() {");
                    builder.AppendLine(@"            return {");
                    builder.AppendLine(@"                numberFormat: {");
                    builder.AppendLine(String.Format(@"                    name: '{0}',", Escape(cultureInfo.Name)));
                    builder.AppendLine(String.Format(@"                    groupSizes: [{0}],", Escape(String.Join(", ", cultureInfo.NumberFormat.NumberGroupSizes.Select(r => r.ToString(CultureInfo.InvariantCulture)).ToArray()))));
                    builder.AppendLine(String.Format(@"                    ',': '{0}',", Escape(cultureInfo.NumberFormat.CurrencyGroupSeparator)));
                    builder.AppendLine(String.Format(@"                    '.': '{0}',", Escape(cultureInfo.NumberFormat.CurrencyDecimalSeparator)));
                    builder.AppendLine(String.Format(@"                    pattern: ['{0}']", Escape(NegativePattern(cultureInfo.NumberFormat.NumberNegativePattern))));
                    builder.AppendLine(@"                }");
                    builder.AppendLine(@"            };");
                    builder.AppendLine(@"        }");
                    builder.AppendLine(@"    };");
                }

                builder.AppendLine(@"}(window.VRS = window.VRS || {}));");
                JavaScript = builder.ToString();
            }
        }

        private static string NegativePattern(int patternIndex)
        {
            var patterns = new string[] { "(n)", "-n", "- n", "n-", "n -" };
            return patterns[patternIndex];
        }

        private static string Escape(string text, char quoteChar = '\'')
        {
            return String.IsNullOrEmpty(text) ? text : quoteChar == '"' ? text.Replace("\"", "\\\"") : text.Replace("'", "\\'");
        }
    }
}
