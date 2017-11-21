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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Owin.StreamManipulator
{
    /// <summary>
    /// Default implementation of <see cref="IBundlerHtmlManipulator"/>.
    /// </summary>
    class BundlerHtmlManipulator : IBundlerHtmlManipulator
    {
        private const string BundleStart = "<!-- [[ JS BUNDLE START ]] -->";
        private const string BundleEnd =   "<!-- [[ BUNDLE END ]] -->";

        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="textContent"></param>
        public void ManipulateTextResponse(IDictionary<string, object> environment, TextContent textContent)
        {
            var configuration = _SharedConfiguration;
            if(configuration == null) {
                configuration = Factory.Singleton.ResolveSingleton<ISharedConfiguration>();
                _SharedConfiguration = configuration;
            }

            if(configuration.Get().GoogleMapSettings.EnableBundling) {
                var context = PipelineContext.GetOrCreate(environment);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(textContent.Content);
                var parserErrors = new List<string>();

                if(ReplaceScriptLinksWithLinkToBundle(context, htmlDocument, parserErrors)) {
                    if(parserErrors.Count != 0) {
                        htmlDocument.LoadHtml(textContent.Content);

                        var node = htmlDocument.DocumentNode.AppendChild(htmlDocument.CreateComment("\r\n<!-- BUNDLE PARSER ERROR -->\r\n"));
                        foreach(var parserError in parserErrors) {
                            node = htmlDocument.DocumentNode.InsertAfter(htmlDocument.CreateComment($"<!-- {parserError} -->\r\n"), node);
                        }
                    }

                    var buffer = new StringBuilder();
                    using(var writer = new StringWriter(buffer)) {
                        htmlDocument.Save(writer);
                    }
                    textContent.Content = buffer.ToString();
                }
            }
        }

        private bool ReplaceScriptLinksWithLinkToBundle(PipelineContext context, HtmlDocument htmlDocument, List<string> parserErrors)
        {
            var htmlChanged = false;

            var bundleIndex = 0;
            var finished = false;
            while(!finished) {
                var bundleStart = htmlDocument.DocumentNode.Descendants().Where(r => IsBundleStartNode(r)).FirstOrDefault();
                finished = bundleStart == null;

                if(!finished) {
                    var pathsAndFiles = new List<string>();
                    var removeNodes = new List<HtmlNode> {
                        bundleStart
                    };
                    var seenEnd = false;
                    for(var node = bundleStart.NextSibling;!seenEnd && node != null;node = node.NextSibling) {
                        seenEnd = IsBundleEndNode(node);
                        if(seenEnd) {
                            removeNodes.Add(node);
                        } else {
                            if(node.NodeType == HtmlNodeType.Comment) {
                                removeNodes.Add(node);
                            } else if(node.NodeType == HtmlNodeType.Text && (node.InnerText ?? "").Trim() == "") {
                                removeNodes.Add(node);
                            }

                            if(IsBundleStartNode(node)) {
                                parserErrors.Add($"Bundle start at line {bundleStart.Line} has another bundle start nested within it at line {node.Line}");
                                seenEnd = finished = true;
                                break;
                            } else if(IsScriptNode(node)) {
                                var pathAndFile = ExtractScriptPathAndFile(node);
                                pathsAndFiles.Add(pathAndFile);
                                removeNodes.Add(node);
                            }
                        }
                    }

                    if(!seenEnd) {
                        parserErrors.Add($"Bundle start at line {bundleStart.Line} has no end");
                        finished = true;
                    } else {
                        var bundleConfig = Factory.Singleton.ResolveSingleton<IBundlerConfiguration>();
                        var bundlePath = bundleConfig.RegisterJavascriptBundle(context.Environment, bundleIndex++, pathsAndFiles);

                        var bundleNode = HtmlNode.CreateNode($@"<script src=""{bundlePath}"" type=""text/javascript""></script>");
                        bundleStart.ParentNode.InsertBefore(bundleNode, bundleStart);
                        foreach(var removeNode in removeNodes) {
                            removeNode.Remove();
                        }
                        htmlChanged = true;
                    }
                }
            }

            if(parserErrors.Count == 0) {
                foreach(HtmlCommentNode commentNode in htmlDocument.DocumentNode.Descendants("#comment")) {
                    if(IsBundleEndNode(commentNode)) {
                        parserErrors.Add($"Bundle end at line {commentNode.Line} has no start");
                    }
                }
            }

            if(!htmlChanged && parserErrors.Count > 0) {
                htmlChanged = true;
            }

            return htmlChanged;
        }

        private bool IsBundleStartNode(HtmlNode node)
        {
            return node?.NodeType == HtmlNodeType.Comment &&
                   (((HtmlCommentNode)node).Comment ?? "").Trim() == BundleStart;
        }

        private bool IsBundleEndNode(HtmlNode node)
        {
            return node?.NodeType == HtmlNodeType.Comment &&
                   (((HtmlCommentNode)node).Comment ?? "").Trim() == BundleEnd;
        }

        private bool IsScriptNode(HtmlNode node)
        {
            var result = node?.NodeType == HtmlNodeType.Element && node.Name == "script";
            if(result) {
                var type = node.GetAttributeValue("type", "");
                result = type == "" || type.Equals("text/javascript", StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private string ExtractScriptPathAndFile(HtmlNode node)
        {
            return node.GetAttributeValue("src", "");
        }
    }
}
