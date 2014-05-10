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
using System.ComponentModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.CustomContent
{
    public interface IOptionsView : IDisposable, IValidateView
    {
        /// <summary>
        /// Gets or sets the web site that the plugin has hooked into.
        /// </summary>
        IWebSite WebSite { get; set; }

        /// <summary>
        /// Gets or sets the site root being used by the plugin.
        /// </summary>
        SiteRoot SiteRoot { get; set; }

        /// <summary>
        /// See <see cref="Options.Enabled"/>.
        /// </summary>
        bool PluginEnabled { get; set; }

        /// <summary>
        /// Gets a list of every InjectSettings object on display to the user.
        /// </summary>
        List<InjectSettings> InjectSettings { get; }

        /// <summary>
        /// Gets or sets the currently selected record in <see cref="InjectSettings"/>.
        /// </summary>
        InjectSettings SelectedInjectSettings { get; set; }

        /// <summary>
        /// <see cref="InjectSettings.Enabled"/>.
        /// </summary>
        bool InjectEnabled { get; set; }

        /// <summary>
        /// <see cref="InjectSettings.File"/>
        /// </summary>
        string InjectFileName { get; set; }

        /// <summary>
        /// See <see cref="InjectSettings.Start"/>
        /// </summary>
        bool InjectAtStart { get; set; }

        /// <summary>
        /// See <see cref="InjectSettings.InjectionLocation"/>.
        /// </summary>
        InjectionLocation InjectOf { get; set; }

        /// <summary>
        /// See <see cref="InjectSettings.PathAndFile"/>.
        /// </summary>
        string InjectPathAndFile { get; set; }

        /// <summary>
        /// See <see cref="Options.SiteRootFolder"/>.
        /// </summary>
        string SiteRootFolder { get; set; }

        /// <summary>
        /// See <see cref="Options.DefaultInjectionFilesFolder"/>.
        /// </summary>
        string DefaultInjectionFilesFolder { get; set; }

        /// <summary>
        /// Raised when the user wants to delete an existing InjectSettings object.
        /// </summary>
        event EventHandler DeleteInjectSettingsClicked;

        /// <summary>
        /// Raised when the user wants to create a new InjectSettings object.
        /// </summary>
        event EventHandler NewInjectSettingsClicked;

        /// <summary>
        /// Raised when the user clicks Reset.
        /// </summary>
        event EventHandler ResetClicked;

        /// <summary>
        /// Raised when the user indicates that they've finished editing options.
        /// </summary>
        event CancelEventHandler SaveClicked;

        /// <summary>
        /// Raised when the <see cref="SelectedInjectSettings"/> value changes.
        /// </summary>
        event EventHandler SelectedInjectSettingsChanged;

        /// <summary>
        /// Raised when an InjectSettings value changes.
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Displays the view and returns a value indicating that the view's contents were saved by the user.
        /// </summary>
        /// <returns></returns>
        bool DisplayView();

        /// <summary>
        /// Sets the focus to the edit fields for the inject settings.
        /// </summary>
        void FocusOnEditFields();

        /// <summary>
        /// Refreshes the selected InjectSettings edit fields.
        /// </summary>
        void RefreshSelectedInjectSettings();

        /// <summary>
        /// Refreshes the list of InjectSettings.
        /// </summary>
        void RefreshInjectSettings();
    }
}
