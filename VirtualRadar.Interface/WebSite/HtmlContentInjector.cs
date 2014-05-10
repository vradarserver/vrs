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

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// A class that describes content to inject into HTML.
    /// </summary>
    /// <remarks>
    /// This differs from <see cref="SiteRoot"/> in that you are free to change the values after you have added it
    /// to the <see cref="IWebSite"/>. Be aware that the multi-threaded nature of the server and site can mean that
    /// your Content method could be called while you are changing the injector's properties - if your Content
    /// method refers to the properties of the injector then you should remove the existing injector from the web
    /// site, change its values and then add it back to the web site rather than change them while it's still attached.
    /// </remarks>
    public class HtmlContentInjector
    {
        /// <summary>
        /// Gets and sets the path and file of the HTML file to inject content into. A null string injects into every HTML
        /// file served by the site. Case insensitive.
        /// </summary>
        /// <remarks>
        /// The path must start from the root of the site - so for http://127.0.0.1/VirtualRadar/index.html the PathAndFile
        /// would be &quot;/index.html&quot;. Content injectors cannot add new pages to the site - if the page you specify
        /// is not served by the site then requests for it will still fail, a new page will not be added to satisfy the
        /// content injector.
        /// </remarks>
        public string PathAndFile { get; set; }

        /// <summary>
        /// Gets and sets a value indicating whether the content should be injected at the start or end of an HTML element.
        /// </summary>
        /// <remarks>
        /// True injects the content directly after the open tag, false injects the content directly before the end tag.
        /// </remarks>
        public bool AtStart { get; set; }

        /// <summary>
        /// Gets and sets the HTML element to inject the content into - e.g. HEAD, BODY, HTML etc. Case insensitive.
        /// </summary>
        /// <remarks>
        /// If there are more than one of these elements in the HTML then only the first or last has the content injected
        /// into it, depending on whether AtStart is true or false. If the HTML does not contain this element then nothing
        /// is injected into the file.
        /// </remarks>
        public string Element { get; set; }

        /// <summary>
        /// Gets or sets the order in which the injector will be called. Lower values have priority over higher values.
        /// </summary>
        /// <remarks>
        /// The relative order of two injectors with the same priority is undefined.
        /// </remarks>
        public int Priority { get; set; }

        /// <summary>
        /// Gets and sets a delegate that returns the content to inject into the HTML.
        /// </summary>
        /// <remarks>
        /// If this is missing or if the function returns a null or empty string then nothing is injected into the file.
        /// </remarks>
        public virtual Func<string> Content { get; set; }
    }
}
