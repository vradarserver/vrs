// Copyright © 2018 onwards, Andrew Whewell
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
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Manages HTML content injection for <see cref="WebSite"/>.
    /// </summary>
    /// <remarks><para>
    /// Before v3 of the site the <see cref="IWebSite"/> interface had features to help plugins
    /// inject content into the HTML that the site served. These features were moved out of the
    /// implementation of IWebSite and into <see cref="IHtmlManipulatorConfiguration"/> when the
    /// server was ported to OWIN.
    /// </para><para>
    /// This class is a bridge between the old and new worlds. The default <see cref="IWebSite"/>
    /// implementation creates one of these, hooks it into IHtmlManipulatorConfiguration and passes
    /// all of the old <see cref="HtmlContentInjector"/> configuration calls through to it. In turn
    /// this class uses the HtmlContentInjector objects to inject code into HTML in the OWIN world.
    /// </para></remarks>
    public class HtmlManipulator : ITextResponseManipulator
    {
        /// <summary>
        /// The lock object that protects <see cref="_HtmlContentInjectors"/> for writes.
        /// </summary>
        private object _HtmlContentInjectorsLock = new object();

        /// <summary>
        /// The list of content injectors.
        /// </summary>
        private List<HtmlContentInjector> _HtmlContentInjectors = new List<HtmlContentInjector>();

        /// <summary>
        /// Adds an object that can cause content to be injected into HTML files served by the site.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void AddHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            if(contentInjector == null) {
                throw new ArgumentNullException("contentInjector");
            }
            lock(_HtmlContentInjectorsLock) {
                var injectors = CollectionHelper.ShallowCopy(_HtmlContentInjectors);
                injectors.Add(contentInjector);
                _HtmlContentInjectors = injectors;
            }
        }

        /// <summary>
        /// Removes an HtmlContentInjector previously added by <see cref="AddHtmlContentInjector"/>.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void RemoveHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            lock(_HtmlContentInjectorsLock) {
                var injectors = CollectionHelper.ShallowCopy(_HtmlContentInjectors);
                injectors.Remove(contentInjector);
                _HtmlContentInjectors = injectors;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="textContent"></param>
        public void ManipulateTextResponse(IDictionary<string, object> environment, TextContent textContent)
        {
            var context = PipelineContext.GetOrCreate(environment);
            var pathAndFile = context.Request.FlattenedPath;

            var allInjectors = _HtmlContentInjectors;
            var injectors = allInjectors .Where(r =>
                !String.IsNullOrEmpty(r.Element) &&
                r.Content != null &&
                (r.PathAndFile == null || r.PathAndFile.Equals(pathAndFile, StringComparison.OrdinalIgnoreCase))
            ).ToArray();

            if(injectors.Length > 0) {
                var document = new HtmlAgilityPack.HtmlDocument() {
                    OptionCheckSyntax = false,
                    OptionDefaultStreamEncoding = textContent.Encoding,
                };
                document.LoadHtml(textContent.Content);

                var modified = false;
                foreach(var injector in injectors.OrderByDescending(r => r.Priority)) {
                    var elements = document.DocumentNode.Descendants(injector.Element);
                    var element = injector.AtStart ? elements.FirstOrDefault() : elements.LastOrDefault();
                    var content = element == null ? null : injector.Content();
                    if(element != null && !String.IsNullOrEmpty(content)) {
                        var subDocument = new HtmlAgilityPack.HtmlDocument() {
                            OptionCheckSyntax = false,
                        };
                        subDocument.LoadHtml(injector.Content());

                        if(injector.AtStart) element.PrependChild(subDocument.DocumentNode);
                        else                 element.AppendChild(subDocument.DocumentNode);
                        modified = true;
                    }
                }

                if(modified) {
                    using(var stream = new MemoryStream()) {
                        document.Save(stream);
                        stream.Position = 0;
                        using(var streamReader = new StreamReader(stream, textContent.Encoding, true)) {
                            textContent.Content = streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
