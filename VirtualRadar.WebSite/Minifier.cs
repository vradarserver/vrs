// Copyright © 2013 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IMinifier"/>.
    /// </summary>
    sealed class Minifier : IMinifier
    {
        /// <summary>
        /// Gets or sets the shared configuration singleton.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// Returns true if minification is enabled.
        /// </summary>
        private bool IsEnabled()
        {
            if(_SharedConfiguration == null) {
                _SharedConfiguration = Factory.Singleton.ResolveSingleton<ISharedConfiguration>();
            }
            return _SharedConfiguration.Get().GoogleMapSettings.EnableMinifying;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="javaScriptContent"></param>
        /// <returns></returns>
        public string MinifyJavaScript(string javaScriptContent)
        {
            var result = javaScriptContent;
            if(IsEnabled() && !String.IsNullOrEmpty(result)) {
                var minifier = new Microsoft.Ajax.Utilities.Minifier(); // A fresh instance every time hopefully ensures thread safety
                result = minifier.MinifyJavaScript(javaScriptContent);
                result = AppendErrors(minifier, result);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cssContent"></param>
        /// <returns></returns>
        public string MinifyCss(string cssContent)
        {
            var result = cssContent;
            if(IsEnabled() && !String.IsNullOrEmpty(result)) {
                var minifier = new Microsoft.Ajax.Utilities.Minifier(); // A fresh instance every time hopefully ensures thread safety
                result = minifier.MinifyStyleSheet(cssContent);
                result = AppendErrors(minifier, result);
            }

            return result;
        }

        /// <summary>
        /// Appends any errors found during minification to the result.
        /// </summary>
        /// <param name="minifier"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private string AppendErrors(Microsoft.Ajax.Utilities.Minifier minifier, string result)
        {
            var errors = minifier.Errors.Where(r => !r.Contains("MinifyJavaScript(1,1-2): error JS1014:")).ToList();
            if(errors.Count() > 0) {
                var buffer = new StringBuilder(result);
                buffer.AppendLine();
                buffer.AppendLine("/*");
                buffer.AppendLine("  Minifier errors:");
                foreach(var error in errors) {
                    buffer.AppendLine(String.Format("    {0}", error.Replace("/*", "/ *").Replace("*/", "* /")));
                }
                buffer.AppendLine("*/");
                result = buffer.ToString();
            }

            return result;
        }
    }
}
