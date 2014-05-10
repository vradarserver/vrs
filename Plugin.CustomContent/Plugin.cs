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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using System.ComponentModel;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.CustomContent
{
    /// <summary>
    /// Implements <see cref="IPlugin"/> to tell VRS about our plugin.
    /// </summary>
    public class Plugin : IPlugin
    {
        #region Private Class - CustomHtmlContentInjector
        class CustomHtmlContentInjector : HtmlContentInjector
        {
            public string FileName { get; set; }

            public override Func<string> Content
            {
                get { return GetContent; }
                set { ; }
            }

            private string GetContent()
            {
                string result = null;
                if(!String.IsNullOrEmpty(FileName)) {
                    if(File.Exists(FileName)) {
                        result = File.ReadAllText(FileName);
                    }
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The options that govern the plugin's behaviour.
        /// </summary>
        private Options _Options = new Options();

        /// <summary>
        /// The content injectors that we've created and added to the web site.
        /// </summary>
        private List<CustomHtmlContentInjector> _ContentInjectors = new List<CustomHtmlContentInjector>();

        /// <summary>
        /// The web site that's currently in use.
        /// </summary>
        private IWebSite _WebSite;

        /// <summary>
        /// The site root that will be added to the web site.
        /// </summary>
        private SiteRoot _SiteRoot = new SiteRoot() { Priority = -2000000000 };
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id { get { return "VirtualRadar.Plugin.CustomContent"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return "Custom Content"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.0.0"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Raises <see cref="StatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStatusChanged(EventArgs args)
        {
            if(StatusChanged != null) StatusChanged(this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Plugin()
        {
            Status = CustomContentStrings.Disabled;
        }
        #endregion

        #region RegisterImplementations
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            classFactory.Register<IOptionsView, WinForms.OptionsView>();
            classFactory.Register<IOptionsPresenter, OptionsPresenter>();
        }
        #endregion

        #region Startup
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            _WebSite = parameters.WebSite;

            _Options = OptionsStorage.Load(this);
            ApplyOptions();
        }
        #endregion

        #region GuiThreadStartup
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
        }
        #endregion

        #region ShowWinFormsOptionsUI
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = Factory.Singleton.Resolve<IOptionsView>()) {
                view.WebSite = _WebSite;
                view.SiteRoot = _SiteRoot;
                view.InjectSettings.AddRange(_Options.InjectSettings.Select(r => (InjectSettings)r.Clone()));
                view.PluginEnabled = _Options.Enabled;
                view.SiteRootFolder = _Options.SiteRootFolder;
                view.DefaultInjectionFilesFolder = _Options.DefaultInjectionFilesFolder;

                if(view.DisplayView()) {
                    _Options.InjectSettings.Clear();
                    _Options.InjectSettings.AddRange(view.InjectSettings);
                    _Options.Enabled = view.PluginEnabled;
                    _Options.SiteRootFolder = view.SiteRootFolder;
                    _Options.DefaultInjectionFilesFolder = view.DefaultInjectionFilesFolder;

                    OptionsStorage.Save(this, _Options);

                    ApplyOptions();
                }
            }
        }
        #endregion

        #region ApplyOptions
        /// <summary>
        /// Applies the options.
        /// </summary>
        private void ApplyOptions()
        {
            if(_WebSite != null) {
                if(!_Options.Enabled) DisableSiteRoot();
                else EnableSiteRoot(_Options.SiteRootFolder);

                foreach(var existingInjector in _ContentInjectors) {
                    _WebSite.RemoveHtmlContentInjector(existingInjector);
                }
                _ContentInjectors.Clear();
                if(_Options.Enabled) {
                    int priority = 1;
                    foreach(var injectSettings in _Options.InjectSettings.Where(r => r.Enabled)) {
                        var injector = new CustomHtmlContentInjector() {
                            AtStart = injectSettings.Start,
                            Element = injectSettings.InjectionLocation.ToString().ToLower(),
                            FileName = injectSettings.File,
                            PathAndFile = injectSettings.PathAndFile == "*" ? null : injectSettings.PathAndFile,
                            Priority = priority++,
                        };
                        _WebSite.AddHtmlContentInjector(injector);
                        _ContentInjectors.Add(injector);
                    }
                }

                if(!_Options.Enabled) Status = CustomContentStrings.Disabled;
                else {
                    if(String.IsNullOrEmpty(_Options.SiteRootFolder)) Status = CustomContentStrings.EnabledNoSiteRoot;
                    else Status = String.Format(CustomContentStrings.EnabledWithSiteRoot, _Options.SiteRootFolder);
                }
            }

            OnStatusChanged(EventArgs.Empty);
        }
        #endregion

        #region EnableSiteRoot, DisableSiteRoot, IsDuplicateSiteRootFolder
        /// <summary>
        /// Enables the site root folder.
        /// </summary>
        /// <param name="folder"></param>
        private void EnableSiteRoot(string folder)
        {
            if(_WebSite != null) {
                _SiteRoot.Folder = folder;
                if(String.IsNullOrEmpty(_SiteRoot.Folder)) _WebSite.RemoveSiteRoot(_SiteRoot);
                else {
                    var siteRootActive = _WebSite.IsSiteRootActive(_SiteRoot, false);

                    var folderChanged = siteRootActive && !_WebSite.IsSiteRootActive(_SiteRoot, true);
                    if(folderChanged) {
                        _WebSite.RemoveSiteRoot(_SiteRoot);
                        siteRootActive = false;
                    }

                    if(!siteRootActive) _WebSite.AddSiteRoot(_SiteRoot);
                }
            }
        }

        /// <summary>
        /// Disables the site root folder.
        /// </summary>
        private void DisableSiteRoot()
        {
            _WebSite.RemoveSiteRoot(_SiteRoot);
        }
        #endregion

        #region Events subscribed
        #endregion
    }
}
