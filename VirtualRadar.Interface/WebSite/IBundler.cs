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
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface for objects that can bundle multiple JavaScript declarations in an HTML
    /// file into a single download.
    /// </summary>
    public interface IBundler : IDisposable
    {
        /// <summary>
        /// Gets the web site that the bundler is attached to.
        /// </summary>
        IWebSite WebSite { get; }

        /// <summary>
        /// Gets the web server that the bundler is attached to.
        /// </summary>
        IWebServer WebServer { get; }

        /// <summary>
        /// Attaches the bundler to the web site passed across.
        /// </summary>
        /// <param name="webSite"></param>
        /// <remarks>
        /// This gives the bundler the opportunity to register itself as a provider of content with the web server
        /// that the web site is using. When files are bundled they are fetched directly though the web site.
        /// </remarks>
        void AttachToWebSite(IWebSite webSite);

        /// <summary>
        /// Replaces multiple statements in the HTML that drag in JavaScript files with a single statement that
        /// downloads all of the files in one go.
        /// </summary>
        /// <param name="requestPathAndFile">The path and file part of the request URL for the HTML file being served.</param>
        /// <param name="htmlContent">The original content of the HTML file.</param>
        /// <returns>The (possibly modified) content of the HTML file.</returns>
        /// <remarks><para>
        /// Bundles are delineated in the HTML with two HTML comments. The opening comment, which comes before the first of
        /// a set of files to bundle together, is of the form:</para>
        /// <code>!&lt;-- [[ JS BUNDLE START ]] --&gt;</code>
        /// <para>The end tag is:</para>
        /// <code>!&lt;-- [[ BUNDLE END ]] --&gt;</code>
        /// <para>
        /// All script statements of type=&quot;text/javascript&quot; between the BUNDLE START / END lines are removed, unless
        /// they point to remote files, and are replaced with a single statement. The URL for the single statement has a random
        /// unique filename. When the browser requests that file the bundler causes the site to serve a file that has the content
        /// of all of the JS files concatenated together into a single file.
        /// </para><para>
        /// The JavaScript files that are concatenated together are fetched via the web site.
        /// </para></remarks>
        string BundleHtml(string requestPathAndFile, string htmlContent);
    }
}
