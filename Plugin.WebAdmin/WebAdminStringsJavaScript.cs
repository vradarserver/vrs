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
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// A static class that sends the strings from WebAdminStrings.resx to the site as a JavaScript file.
    /// </summary>
    static class WebAdminStringsJavaScript
    {
        /// <summary>
        /// The JavaScript to send to the site.
        /// </summary>
        private static string _JavaScriptContent;

        /// <summary>
        /// A spin lock that protects <see cref="_JavaScriptContent"/> while it is being formed.
        /// </summary>
        private static SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The object that handles the sending of responses for us.
        /// </summary>
        private static IResponder _Responder;

        /// <summary>
        /// The localised strings map for the plugin's strings.
        /// </summary>
        public static readonly LocalisedStringsMap LocalisedWebAdminStrings = new LocalisedStringsMap(typeof(WebAdminStrings));

        /// <summary>
        /// Sends a JavaScript file that carries the translations to the site in a form that is usable from JavaScript.
        /// </summary>
        /// <param name="requestArgs"></param>
        public static void SendJavaScript(RequestReceivedEventArgs requestArgs)
        {
            CreateJavaScriptContent();
            _Responder.SendText(requestArgs.Request, requestArgs.Response, _JavaScriptContent, Encoding.UTF8, MimeType.Javascript);
            requestArgs.Handled = true;
        }

        /// <summary>
        /// Creates the content for <see cref="_JavaScriptContent"/>.
        /// </summary>
        private static void CreateJavaScriptContent()
        {
            using(_SpinLock.AcquireLock()) {
                if(_JavaScriptContent == null) {
                    _Responder = Factory.Singleton.Resolve<IResponder>();
                    var htmlLocaliser = Factory.Singleton.Resolve<IHtmlLocaliser>();
                    var builder = new StringBuilder();

                    var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
                    var currentCultureInfo = Thread.CurrentThread.CurrentUICulture;
                    var cultureInfo = runtimeEnvironment.MainThreadCultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    try {
                        builder.AppendLine(@"(function(VRS, undefined)");      // Note that this gets loaded before jQuery, so the usual parameter set is inappropriate
                        builder.AppendLine(@"{");
                        builder.AppendLine(@"    VRS.$$ = VRS.$$ || {};");
                        builder.AppendLine();
                        foreach(var name in LocalisedWebAdminStrings.GetStringNames().Where(r => r.StartsWith("WA_")).OrderBy(r => r)) {
                            var value = LocalisedWebAdminStrings.GetLocalisedString(name);
                            value = htmlLocaliser.ConvertResourceStringToHtmlString(value);
                            builder.AppendLine(String.Format("    VRS.$$.{0} = '{1}';", name, value));
                        }
                        builder.AppendLine();

                        // Add a bare-bones Globalise object to support the string utility's number formatting
                        builder.AppendLine(@"    Globalize = {");
                        builder.AppendLine(@"        culture: function() {");
                        builder.AppendLine(@"            return {");
                        builder.AppendLine(@"                numberFormat: {");
                        builder.AppendLine(String.Format(@"                    name: '{0}',", cultureInfo.Name));
                        builder.AppendLine(String.Format(@"                    groupSizes: [{0}],", String.Join(", ", cultureInfo.NumberFormat.NumberGroupSizes.Select(r => r.ToString(CultureInfo.InvariantCulture)).ToArray())));
                        builder.AppendLine(String.Format(@"                    ',': '{0}',", cultureInfo.NumberFormat.CurrencyGroupSeparator));
                        builder.AppendLine(String.Format(@"                    '.': '{0}',", cultureInfo.NumberFormat.CurrencyDecimalSeparator));
                        builder.AppendLine(String.Format(@"                    pattern: ['{0}']", NegativePattern(cultureInfo.NumberFormat.NumberNegativePattern)));
                        builder.AppendLine(@"                }");
                        builder.AppendLine(@"            };");
                        builder.AppendLine(@"        }");
                        builder.AppendLine(@"    };");
                        builder.AppendLine(@"}(window.VRS = window.VRS || {}));");

                        _JavaScriptContent = builder.ToString();
                    } finally {
                        Thread.CurrentThread.CurrentUICulture = currentCultureInfo;
                    }
                }
            }
        }

        private static string NegativePattern(int patternIndex)
        {
            var patterns = new string[] { "(n)", "-n", "- n", "n-", "n -" };
            return patterns[patternIndex];
        }
    }
}
