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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The class that describes a web admin view.
    /// </summary>
    public class WebAdminView
    {
        /// <summary>
        /// A delegate that creates a view for the path and file.
        /// </summary>
        private Func<IView> _CreateView;

        /// <summary>
        /// Gets the path and file to the HTML file from the site root.
        /// </summary>
        public string PathAndFile { get; private set; }

        /// <summary>
        /// Gets the HTML filename without the path.
        /// </summary>
        public string HtmlFileName { get; private set; }

        /// <summary>
        /// Gets the name to use for the view in the web admin's site menu. Pass a null or empty string
        /// to prevent the view from appearing in the menu.
        /// </summary>
        public string MenuName { get; private set; }

        /// <summary>
        /// Gets or sets the plugin that this view is the options screen for.
        /// </summary>
        public IPlugin Plugin { get; set; }

        /// <summary>
        /// Gets the resources that holds the strings to substitute into the view.
        /// </summary>
        public Type StringResources { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="htmlFileName"></param>
        /// <param name="menuName"></param>
        /// <param name="createView"></param>
        /// <param name="stringResources"></param>
        public WebAdminView(string pathFromRoot, string htmlFileName, string menuName, Func<IView> createView, Type stringResources)
        {
            if(String.IsNullOrEmpty(pathFromRoot) || pathFromRoot == "/") throw new InvalidOperationException("You must supply the path from root");
            if(String.IsNullOrEmpty(htmlFileName) || htmlFileName.Contains('/')) throw new InvalidOperationException("You must supply the HTML file name and it cannot contain a path");

            PathAndFile = String.Format("{0}{1}{2}",
                pathFromRoot,
                pathFromRoot.EndsWith("/") ? "" : "/",
                htmlFileName
            );
            HtmlFileName = htmlFileName;
            MenuName = menuName;
            _CreateView = createView;
            StringResources = stringResources;
        }

        /// <summary>
        /// Creates the object that will handle the requests from the HTML page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The view is expected to create an instance of its presenter and dispose of the presenter
        /// if necessary.
        /// </remarks>
        public IView CreateView()
        {
            return _CreateView();
        }
    }
}
