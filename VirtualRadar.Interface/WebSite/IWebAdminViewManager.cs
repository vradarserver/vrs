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
using InterfaceFactory;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface that objects which handle collections of <see cref="WebAdminView"/>s must implement.
    /// </summary>
    [Singleton]
    public interface IWebAdminViewManager
    {
        /// <summary>
        /// Returns true if the web admin plugin has been installed.
        /// </summary>
        bool WebAdminPluginInstalled { get; }

        /// <summary>
        /// Registers a folder with web admin view HTML files on behalf of a plugin.
        /// </summary>
        /// <param name="pluginFolder"></param>
        /// <param name="subFolder"></param>
        /// <remarks>
        /// Do not put web admin views in or under the plugin's normal Web folder, keep them separate
        /// so that they are not served if the WebAdmin plugin is not installed.
        /// </remarks>
        void RegisterWebAdminViewFolder(string pluginFolder, string subFolder);

        /// <summary>
        /// Registers a template marker and the file that contains the HTML that should be subtituted in its place
        /// when seen within a web admin HTML file.
        /// </summary>
        /// <param name="templateMarker"></param>
        /// <param name="templateHtmlFullPath"></param>
        /// <remarks>
        /// The web admin plugin reserves the @head.html@ template marker. All web admin views should include
        /// this marker in their head tag.
        /// </remarks>
        void RegisterTemplateFileName(string templateMarker, string templateHtmlFullPath);

        /// <summary>
        /// Registers a string resources file that should be made available to JavaScript. It can be loaded via
        /// the URL WebAdmin/Script/Strings.&lt;namespace&gt;.js. Do not use the namespace Server, that is used by the
        /// plugin to expose the VRS's strings. Strings.js loads the web site strings and a Globalize object
        /// for the server's language.
        /// </summary>
        /// <param name="stringResourcesType"></param>
        /// <param name="namespace"></param>
        void RegisterTranslations(Type stringResourcesType, string @namespace);

        /// <summary>
        /// Registers a view with the web admin site. The view's folder must previously have been registered
        /// with a call to <see cref="RegisterWebAdminViewFolder"/>.
        /// </summary>
        /// <param name="webAdminView"></param>
        void AddWebAdminView(WebAdminView webAdminView);
    }
}
