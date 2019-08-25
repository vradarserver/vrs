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

        #region Public static fields
        /// <summary>
        /// The number of seconds that need to elapse before a new check for modified images will be made.
        /// </summary>
        public static readonly int SecondsBetweenCustomResourceImageManagerRefreshes = 60;
        #endregion

        #region Fields
        /// <summary>
        /// The content injectors that we've created and added to the web site.
        /// </summary>
        private List<CustomHtmlContentInjector> _ContentInjectors = new List<CustomHtmlContentInjector>();

        /// <summary>
        /// The object that manages the switching in and out of custom resources for us.
        /// </summary>
        private CustomResourceImageManager _CustomResourceImageManager = new CustomResourceImageManager();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the last initialised instance of the plugin object. At run-time only one plugin
        /// object gets created and initialised.
        /// </summary>
        public static Plugin Singleton { get; private set; }

        private IWebSite _WebSite;
        /// <summary>
        /// Gets the web site that's currently in use.
        /// </summary>
        public IWebSite WebSite
        {
            get { return _WebSite; }
        }

        private SiteRoot _SiteRoot = new SiteRoot() { Priority = -2000000000 };
        /// <summary>
        /// Gets the site root that will be added to the web site.
        /// </summary>
        public SiteRoot SiteRoot
        {
            get { return _SiteRoot; }
        }

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
        public string Name { get { return CustomContentStrings.PluginName; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.4.0"; } }

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
            EventHelper.Raise(StatusChanged, this, args);
        }

        /// <summary>
        /// Raised when <see cref="OptionsStorage"/> saves a new set of options.
        /// </summary>
        public event EventHandler<EventArgs<Options>> SettingsChanged;

        /// <summary>
        /// Raises <see cref="SettingsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        internal void RaiseSettingsChanged(EventArgs<Options> args)
        {
            ApplyOptions(args.Value);
            EventHelper.Raise(SettingsChanged, this, args);
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
            Singleton = this;
            _WebSite = parameters.WebSite;

            var options = OptionsStorage.Load(this);
            ApplyOptions(options);

            var heartbeat = Factory.ResolveSingleton<IHeartbeatService>();
            heartbeat.SlowTick += Heartbeat_SlowTick;
        }
        #endregion

        #region GuiThreadStartup
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.Resolve<IWebAdminViewManager>().Singleton;
            webAdminViewManager.RegisterTranslations(typeof(CustomContentStrings), "CustomContentPlugin");
            webAdminViewManager.AddWebAdminView(new WebAdminView("/WebAdmin/", "CustomContentPluginOptions.html", CustomContentStrings.WebAdminMenuName, () => new WebAdmin.OptionsView(), typeof(CustomContentStrings)) {
                Plugin = this,
            });
            webAdminViewManager.RegisterWebAdminViewFolder(PluginFolder, "Web");
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            _CustomResourceImageManager.Enabled = false;
        }
        #endregion

        #region ShowWinFormsOptionsUI
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = Factory.Resolve<IOptionsView>()) {
                var options = OptionsStorage.Load(this);

                view.WebSite = _WebSite;
                view.SiteRoot = _SiteRoot;
                view.InjectSettings.AddRange(options.InjectSettings.Select(r => (InjectSettings)r.Clone()));
                view.PluginEnabled = options.Enabled;
                view.SiteRootFolder = options.SiteRootFolder;
                view.ResourceImagesFolder = options.ResourceImagesFolder;
                view.DefaultInjectionFilesFolder = options.DefaultInjectionFilesFolder;

                if(view.DisplayView()) {
                    options.InjectSettings.Clear();
                    options.InjectSettings.AddRange(view.InjectSettings);
                    options.Enabled = view.PluginEnabled;
                    options.SiteRootFolder = view.SiteRootFolder;
                    options.ResourceImagesFolder = view.ResourceImagesFolder;
                    options.DefaultInjectionFilesFolder = view.DefaultInjectionFilesFolder;

                    OptionsStorage.Save(this, options);
                }
            }
        }
        #endregion

        #region ApplyOptions
        /// <summary>
        /// Applies the options.
        /// </summary>
        private void ApplyOptions(Options options)
        {
            if(_WebSite != null) {
                if(!options.Enabled) {
                    DisableSiteRoot();
                } else {
                    EnableSiteRoot(options.SiteRootFolder);
                }

                if(!options.Enabled || String.IsNullOrEmpty(options.ResourceImagesFolder)) {
                    _CustomResourceImageManager.Enabled = false;
                    _CustomResourceImageManager.ResourceImagesFolder = null;
                    _CustomResourceImageManager.UnloadCustomImages();
                } else {
                    _CustomResourceImageManager.ResourceImagesFolder = options.ResourceImagesFolder;
                    _CustomResourceImageManager.Enabled = true;
                    _CustomResourceImageManager.LoadCustomImages(blockThread: true);
                }

                foreach(var existingInjector in _ContentInjectors) {
                    _WebSite.RemoveHtmlContentInjector(existingInjector);
                }
                _ContentInjectors.Clear();
                if(options.Enabled) {
                    int priority = 1;
                    foreach(var injectSettings in options.InjectSettings.Where(r => r.Enabled)) {
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

                if(!options.Enabled) Status = CustomContentStrings.Disabled;
                else {
                    if(String.IsNullOrEmpty(options.SiteRootFolder)) Status = CustomContentStrings.EnabledNoSiteRoot;
                    else Status = String.Format(CustomContentStrings.EnabledWithSiteRoot, options.SiteRootFolder);
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
        /// <summary>
        /// Raised every 10 seconds or so.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            _CustomResourceImageManager.DisposeOfUnusedImages();

            if(_CustomResourceImageManager.Enabled) {
                var threshold = DateTime.UtcNow.AddSeconds(-SecondsBetweenCustomResourceImageManagerRefreshes);
                if(_CustomResourceImageManager.LastLoadCustomImagesUtc <= threshold) {
                    _CustomResourceImageManager.LoadCustomImages(blockThread: false);
                }
            }
        }
        #endregion
    }
}
