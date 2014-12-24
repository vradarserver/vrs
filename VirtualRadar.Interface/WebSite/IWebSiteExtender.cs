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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface for a wrapper around the extension methods exposed in IWebSite. Intended for
    /// use by plugins to make life a bit easier for those that want to extend the website.
    /// </summary>
    public interface IWebSiteExtender : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the extensions are enabled or disabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the priority of your plugin relative to the standard Virtual Radar Server
        /// web site and other plugins.
        /// </summary>
        /// <remarks><para>
        /// Under normal use you can leave this at the default value, in which case if there is a clash
        /// between a file you are serving and a file that Virtual Radar Server is serving then the
        /// standard Virtual Radar Server file is served. If you make sure that your files are in a well
        /// named sub-folder then clashes should not occur and priority should not be an issue.
        /// </para><para>
        /// If you want to override a standard VRS file then set this to a value that is below zero.
        /// However you need to be careful with this as your plugin may end up breaking with a future
        /// version of VRS.
        /// </para>
        /// </remarks>
        int Priority { get; set; }

        /// <summary>
        /// Gets or sets the folder under the plugin's installation folder that contains the files to
        /// add to the root of the web site.
        /// </summary>
        /// <remarks><para>
        /// If this property is null then no files will be added to the root of the site. To avoid accidentally
        /// interferring with future updates of VRS it is recommended that you create a sub-folder under this
        /// folder that is named after your plugin and put all of your files there. So, if your plugin is
        /// called 'MyPlugin' then you might:
        /// </para><list type=">">
        /// <item>Create a folder in your plugin installation folder called Web.</item>
        /// <item>Set this property to &quot;Web&quot;.</item>
        /// <item>Create a sub-folder under Web called MyPlugin.</item>
        /// <item>Put all of your HTML, JavaScript and CSS into the MyPlugin folder.</item>
        /// </list>
        /// </remarks>
        string WebRootSubFolder { get; set; }

        /// <summary>
        /// Gets or sets a string that will be injected at the end of the HEAD tag for all of the pages in <see cref="InjectPages"/>.
        /// </summary>
        string InjectContent { get; set; }

        /// <summary>
        /// Gets a collection of pages that <see cref="InjectContent"/> will be injected into. If this is empty then the content
        /// will be injected into all pages.
        /// </summary>
        IList<string> InjectPages { get; }

        /// <summary>
        /// Gets a map of page handler methods keyed by the address that they will handle.
        /// </summary>
        /// <remarks><para>
        /// If your plugin needs to handle requests in code - for example it needs to create JSON responses on the fly - then you
        /// can register your handlers for those requests here. The key to the dictionary is the address from the VRS root of the
        /// address that you want your code called for (e.g. '/MyPlugin/GiveMeData.json') and the value is a delegate, anonymous
        /// delegate or function that takes a single parameter, a <see cref="RequestReceivedEventArgs"/>, that passes information
        /// about the request and can be used to send a response to the browser.
        /// </para><para>
        /// Addresses are not case sensitive.
        /// </para></remarks>
        IDictionary<string, Action<RequestReceivedEventArgs>> PageHandlers { get; }

        /// <summary>
        /// Initialises the web extender.
        /// </summary>
        /// <param name="pluginStartupParameters">The startup parameters passed to the plugin's Startup method.</param>
        /// <remarks>
        /// Once you call this method any changes to the properties, aside from <see cref="Enabled"/>, are ignored. You can use
        /// the <see cref="Enabled"/> property to turn your plugins extensions on and off on the fly or you can dispose of the
        /// extender entirely, but you cannot change anything else.
        /// </remarks>
        void Initialise(PluginStartupParameters pluginStartupParameters);

        /// <summary>
        /// Adds the map page addresses to <see cref="InjectPages"/>. Has no effect if called after <see cref="Initialise"/>.
        /// </summary>
        /// <returns>The web site extender object that was called.</returns>
        IWebSiteExtender InjectMapPages();

        /// <summary>
        /// Adds the report page addresses to <see cref="InjectPages"/>. Has no effect if called after <see cref="Initialise"/>.
        /// </summary>
        /// <returns>The web site extender object that was called.</returns>
        IWebSiteExtender InjectReportPages();

        /// <summary>
        /// Marks a URL folder as protected. Only administrators may access protected folders.
        /// </summary>
        /// <param name="folder"></param>
        /// <remarks><para>
        /// For example, if you were to protect the folder &quot;MyPlugin/Admin&quot; then access to
        /// http://127.0.0.1/VirtualRadar/MyPlugin/index.html would not be protected, but access to
        /// http://127.0.0.1/VirtualRadar/MyPlugin/Admin/index.html would only be allowed once the user had supplied credentials,
        /// and only if the credentials could be authenticated against the list of administrator users.
        /// </para><para>
        /// Once a folder is marked as protected you cannot unprotect it, it remains protected for the
        /// remainder of the session.
        /// </para></remarks>
        void ProtectFolder(string folder);
    }
}
