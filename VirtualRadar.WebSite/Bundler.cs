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
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using System.Net;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IBundler"/>.
    /// </summary>
    sealed class Bundler : IBundler
    {
        #region Fields
        private const string StartJavaScriptBundleComment = "<!-- [[ JS BUNDLE START ]] -->";
        private const string EndBundleComment =             "<!-- [[ BUNDLE END ]] -->";

        private object _SyncLock = new object();
        private bool _Enabled;
        private bool _ConfigurationStorageHooked;
        private bool _WebServerHooked;
        private IMinifier _Minifier;
        private Dictionary<string, List<string>> _BundleToRequestFileNameMap = new Dictionary<string,List<string>>();
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebSite WebSite { get; private set; }
        
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; private set; }
        #endregion

        #region Constructor, Finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Bundler()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Minifier != null) {
                    _Minifier.Dispose();
                    _Minifier = null;
                }
                if(_ConfigurationStorageHooked) {
                    var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                    configurationStorage.ConfigurationChanged -= ConfigurationChanged;
                    _ConfigurationStorageHooked = false;
                }
                if(_WebServerHooked && WebServer != null) {
                    WebServer.RequestReceived -= WebServer_RequestReceived;
                    _WebServerHooked = false;
                }
            }
        }
        #endregion

        #region AttachToWebSite
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webSite"></param>
        public void AttachToWebSite(IWebSite webSite)
        {
            if(webSite == null) throw new ArgumentNullException("webSite");
            if(webSite.WebServer == null) throw new InvalidOperationException("The web site must be attached to a web server before the bundler can use it");

            if(_WebServerHooked && WebServer != null) {
                WebServer.RequestReceived -= WebServer_RequestReceived;
            }

            if(_Minifier == null) {
                _Minifier = Factory.Resolve<IMinifier>();
                _Minifier.Initialise();
            }

            WebSite = webSite;
            WebServer = webSite.WebServer;

            WebServer.RequestReceived += WebServer_RequestReceived;
            _WebServerHooked = true;

            if(!_ConfigurationStorageHooked) {
                var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                configurationStorage.ConfigurationChanged += ConfigurationChanged;
                _ConfigurationStorageHooked = true;
                LoadConfiguration(configurationStorage);
            }
        }

        /// <summary>
        /// Picks up the current configuration settings.
        /// </summary>
        /// <param name="configurationStorage"></param>
        private void LoadConfiguration(IConfigurationStorage configurationStorage)
        {
            var configuration = configurationStorage.Load();
            _Enabled = configuration.GoogleMapSettings.EnableBundling;
        }
        #endregion

        #region BundleHtml
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="requestPathAndFile"></param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public string BundleHtml(string requestPathAndFile, string htmlContent)
        {
            if(requestPathAndFile == null) throw new ArgumentNullException("requestPathAndFile");
            if(htmlContent == null) throw new ArgumentNullException("htmlContent");
            if(WebSite == null) throw new InvalidOperationException("BundleHtml cannot be called before AttachToWebSite");

            if(_Enabled) {
                var document = new HtmlDocument();
                document.LoadHtml(htmlContent);

                var parserErrors = new List<string>();
                var modifiedHtml = SubstituteJavaScript(requestPathAndFile, document, parserErrors);
                if(parserErrors.Count == 0) {
                    foreach(HtmlCommentNode commentNode in document.DocumentNode.Descendants("#comment")) {
                        if(IsEndNode(commentNode)) parserErrors.Add(String.Format("Bundle end at line {0} has no start", commentNode.Line));
                    }
                }

                if(parserErrors.Count > 0) {
                    document.LoadHtml(htmlContent);
                    var node = document.DocumentNode.AppendChild(document.CreateComment("\r\n<!-- BUNDLE PARSER ERROR -->\r\n"));
                    foreach(var parserError in parserErrors) {
                        node = document.DocumentNode.InsertAfter(document.CreateComment(String.Format("<!-- {0} -->\r\n", parserError)), node);
                    }
                    modifiedHtml = true;
                }

                if(modifiedHtml) {
                    var buffer = new StringBuilder();
                    using(var writer = new StringWriter(buffer)) {
                        document.Save(writer);
                    }
                    htmlContent = buffer.ToString();
                }
            }

            return htmlContent;
        }

        /// <summary>
        /// Substitutes JavaScript bundles in the document.
        /// </summary>
        /// <param name="requestPathAndFile"></param>
        /// <param name="document"></param>
        /// <param name="parserErrors"></param>
        /// <returns></returns>
        private bool SubstituteJavaScript(string requestPathAndFile, HtmlDocument document, List<string> parserErrors)
        {
            var result = false;

            var requestPath = ExtractPath(requestPathAndFile);
            var finished = false;
            while(!finished) {
                finished = true;

                var bundleStart = document.DocumentNode.Descendants().Where(r => IsStartJavaScriptNode(r)).FirstOrDefault();
                if(bundleStart != null) {
                    var pathsAndFiles = new List<string>();
                    var removeNodes = new List<HtmlNode>();
                    removeNodes.Add(bundleStart);
                    var seenEnd = false;
                    for(var node = bundleStart.NextSibling;!seenEnd && node != null;node = node.NextSibling) {
                        seenEnd = IsEndNode(node);
                        if(seenEnd) removeNodes.Add(node);
                        else {
                            if(node.NodeType == HtmlNodeType.Comment) {
                                removeNodes.Add(node);
                            } else if(node.NodeType == HtmlNodeType.Text && (node.InnerText ?? "").Trim() == "") {
                                removeNodes.Add(node);
                            } else if(IsAnyStartNode(node)) {
                                parserErrors.Add(String.Format("Bundle start at line {0} has another bundle start nested within it at line {1}", bundleStart.Line, node.Line));
                                seenEnd = finished = true;
                                break;
                            } else if(IsScriptNode(node)) {
                                var pathAndFile = ExtractScriptPathAndFile(node);
                                if(!String.IsNullOrEmpty(pathAndFile)) pathsAndFiles.Add(MakeAbsolutePathAndFile(requestPath, pathAndFile, node.Line, parserErrors));
                                if(parserErrors.Count > 0) {
                                    seenEnd = finished = true;
                                    break;
                                }
                                removeNodes.Add(node);
                            }
                        }
                    }

                    if(!seenEnd) parserErrors.Add(String.Format("Bundle start at line {0} has no end", bundleStart.Line));
                    else if(parserErrors.Count == 0) {
                        string bundlePathAndFile = null;
                        lock(_SyncLock) {
                            foreach(var kvp in _BundleToRequestFileNameMap) {
                                if(kvp.Value.SequenceEqual(pathsAndFiles)) {
                                    bundlePathAndFile = kvp.Key;
                                    break;
                                }
                            }

                            if(bundlePathAndFile == null) {
                                do {
                                    bundlePathAndFile = String.Format("bundle-{0}.js", Guid.NewGuid().ToString().ToLower());
                                } while(_BundleToRequestFileNameMap.ContainsKey(bundlePathAndFile));
                                _BundleToRequestFileNameMap.Add(String.Format("{0}", bundlePathAndFile), pathsAndFiles);
                            }
                        }

                        var newNode = HtmlNode.CreateNode(String.Format(@"<script src=""{0}"" type=""text/javascript""></script>", bundlePathAndFile));
                        bundleStart.ParentNode.InsertBefore(newNode, bundleStart);

                        foreach(var removeNode in removeNodes) {
                            removeNode.Remove();
                        }

                        result = true;
                        finished = false;
                    }
                }
            }

            return result;
        }

        private string ExtractPath(string pathAndFile)
        {
            var lastSlashPosn = pathAndFile.LastIndexOf('/');
            return lastSlashPosn == -1 ? "/" : pathAndFile.Substring(0, lastSlashPosn + 1);
        }

        private string MakeAbsolutePathAndFile(string path, string pathAndFile, int lineNumber, List<string> parserErrors)
        {
            var result = pathAndFile;
            if(pathAndFile[0] != '/') {
                result = Path.Combine(path, pathAndFile);
            }

            var pathParts = new List<string>(result.Split('/'));
            for(var i = 0;result != "" && i < pathParts.Count - 1;++i) {
                var pathComponent = pathParts[i];
                switch(pathComponent) {
                    case ".":
                        pathParts.RemoveAt(i);
                        --i;
                        break;
                    case "..":
                        if(i <= 1) {
                            result = "";
                            break;
                        }
                        pathParts.RemoveAt(i);
                        pathParts.RemoveAt(i - 1);
                        --i;
                        break;
                }
            }
            if(result != "" && pathParts.Count > 0) result = String.Join("/", pathParts.ToArray());
            else parserErrors.Add(String.Format("Cannot reach {0} from {1} at line {2}", pathAndFile, path, lineNumber));

            return result;
        }

        private bool IsStartJavaScriptNode(HtmlNode node)
        {
            return node != null && node.NodeType == HtmlNodeType.Comment && (((HtmlCommentNode)node).Comment ?? "").Trim() == StartJavaScriptBundleComment;
        }

        private bool IsAnyStartNode(HtmlNode node)
        {
            var result = node != null && node.NodeType == HtmlNodeType.Comment;
            if(result) {
                var comment = ((HtmlCommentNode)node).Comment;
                switch(comment) {
                    case StartJavaScriptBundleComment:
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            return result;
        }

        private bool IsEndNode(HtmlNode node)
        {
            return node != null && node.NodeType == HtmlNodeType.Comment && (((HtmlCommentNode)node).Comment ?? "").Trim() == EndBundleComment;
        }

        private bool IsScriptNode(HtmlNode node)
        {
            var result = node != null && node.NodeType == HtmlNodeType.Element && node.Name == "script";
            if(result) result = node.GetAttributeValue("type", "").Equals("text/javascript", StringComparison.OrdinalIgnoreCase);
            if(result) result = IsLocalAddress(node.GetAttributeValue("src", null));

            return result;
        }

        private string ExtractScriptPathAndFile(HtmlNode node)
        {
            return node.GetAttributeValue("src", "");
        }

        private bool IsLocalAddress(string address)
        {
            var result = !String.IsNullOrEmpty(address);
            if(result) {
                var colonIndex = address.IndexOf(':');
                result = colonIndex == -1;
            }

            return result;
        }
        #endregion

        #region HandleRequestForBundle
        /// <summary>
        /// Processes the request for the content of a bundle.
        /// </summary>
        /// <param name="args"></param>
        private void HandleRequestForBundle(RequestReceivedEventArgs args)
        {
            List<string> bundlePathsAndFiles;
            lock(_SyncLock) _BundleToRequestFileNameMap.TryGetValue(args.File.ToLower(), out bundlePathsAndFiles);
            if(bundlePathsAndFiles != null) {
                var bundle = GetUtf8Content(bundlePathsAndFiles);
                var content = Encoding.UTF8.GetBytes(bundle);

                var preamble = Encoding.UTF8.GetPreamble();
                args.Handled = true;
                args.Response.EnableCompression(args.Request);
                args.Response.StatusCode = HttpStatusCode.OK;
                args.Response.MimeType = MimeType.GetForExtension(Path.GetExtension(args.PathAndFile));
                args.Response.ContentEncoding = Encoding.UTF8;
                args.Classification = MimeType.GetContentClassification(args.Response.MimeType);
                args.Response.ContentLength = content.LongLength + preamble.LongLength;

                using(var memoryStream = new MemoryStream(preamble)) {
                    StreamHelper.CopyStream(memoryStream, args.Response.OutputStream, 10);
                }
                using(var memoryStream = new MemoryStream(content)) {
                    StreamHelper.CopyStream(memoryStream, args.Response.OutputStream, 4096);
                }
            }
        }

        /// <summary>
        /// Fetches the content of a bundle encoded in UTF8.
        /// </summary>
        /// <param name="bundlePathsAndFiles"></param>
        /// <returns></returns>
        private string GetUtf8Content(List<string> bundlePathsAndFiles)
        {
            var buffer = new StringBuilder();

            foreach(var pathAndFile in bundlePathsAndFiles) {
                var simpleContent = WebSite.RequestSimpleContent(pathAndFile);
                buffer.AppendLine(String.Format("/* {0} */", pathAndFile));
                if(simpleContent.HttpStatusCode != HttpStatusCode.OK) {
                    buffer.AppendLine(String.Format("/* Status {0} ({1}) */", (int)simpleContent.HttpStatusCode, simpleContent.HttpStatusCode));
                } else {
                    using(var memoryStream = new MemoryStream(simpleContent.Content)) {
                        var javaScript = "";
                        using(var streamReader = new StreamReader(memoryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true)) {
                            javaScript = streamReader.ReadToEnd();
                        }
                        if(!String.IsNullOrEmpty(javaScript)) javaScript = _Minifier.MinifyJavaScript(javaScript);
                        buffer.Append(javaScript);
                        buffer.AppendLine(";");     // <-- the minifier may strip off a semi-colon that's actually required at the end of the file if more JS is to follow
                    }
                }
            }

            return buffer.ToString();
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the user changes the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration((IConfigurationStorage)sender);
        }

        /// <summary>
        /// Called when the web server receives a request for content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebServer_RequestReceived(object sender, RequestReceivedEventArgs args)
        {
            if(!args.Handled) {
                HandleRequestForBundle(args);
            }
        }
        #endregion
    }
}
