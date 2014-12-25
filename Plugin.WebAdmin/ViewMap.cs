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
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Plugin.WebAdmin.View;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// Holds all of the information necessary to map between a view interface and the web page that
    /// represents that view.
    /// </summary>
    class ViewMap
    {
        /// <summary>
        /// Gets the filename of the view's page.
        /// </summary>
        public string ViewPage { get; private set; }

        /// <summary>
        /// Gets the path and file of the view page.
        /// </summary>
        public string ViewPathAndFile { get; private set; }

        /// <summary>
        /// Gets the filename of the JSON that carries the view's data to the site.
        /// </summary>
        public string ViewDataPage { get; private set; }

        /// <summary>
        /// Gets the path and file of the view data page.
        /// </summary>
        public string ViewDataPathAndFile { get; private set; }

        /// <summary>
        /// Gets the view that is mapped onto the web page.
        /// </summary>
        public BaseView View { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="htmlFileName"></param>
        /// <param name="view"></param>
        public ViewMap(string folder, string htmlFileName, BaseView view)
        {
            ViewPage = htmlFileName;
            ViewPathAndFile = String.Format("{0}/{1}", folder, ViewPage);

            ViewDataPage = Path.ChangeExtension(ViewPage, ".json");
            ViewDataPathAndFile = String.Format("{0}/{1}", folder, ViewDataPage);

            View = view;
        }
    }
}
