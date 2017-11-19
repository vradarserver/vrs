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
using InterfaceFactory;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that records the configuration of the OWIN bundler mechanism.
    /// </summary>
    [Singleton]
    public interface IBundlerConfiguration
    {
        /// <summary>
        /// Registers a bundle and returns the bundle's fully-pathed filename.
        /// </summary>
        /// <param name="htmlRequestEnvironment">
        /// The OWIN environment for the request of the HTML page that contains the bundle of JavaScript links.
        /// At a minimum the path must be specified, ideally the headers too.
        /// </param>
        /// <param name="pageBundleIndex">
        /// The index number of the bundle (i.e. 0 for the first bundle on the page, 1 for the second and so on).
        /// </param>
        /// <param name="javascriptLinkPaths">The paths for all of the JavaScript links in the bundle.</param>
        /// <returns>The link to the bundle pathed relative to the HTML page.</returns>
        /// <remarks>
        /// If this is repeatedly called for the same <paramref name="htmlRequestEnvironment"/> path then the
        /// second and subsequent calls have no effect.
        /// </remarks>
        string RegisterJavascriptBundle(IDictionary<string, object> htmlRequestEnvironment, int pageBundleIndex, IEnumerable<string> javascriptLinkPaths);

        /// <summary>
        /// Returns the content of a bundle for an absolute path of a relative path previously returned by
        /// <see cref="RegisterJavascriptBundle"/>.
        /// </summary>
        /// <param name="environment">The OWIN environment for the request for the bundle.</param>
        /// <returns>
        /// Either null if no bundle has been registered for the path passed across, otherwise the text content
        /// of the bundle in UTF-8.
        /// </returns>
        string GetJavascriptBundle(IDictionary<string, object> bundleRequestEnvironment);
    }
}
