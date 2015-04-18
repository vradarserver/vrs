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
using System.IO;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.CustomContent
{
    class OptionsPresenter : IOptionsPresenter
    {
        #region Fields
        IOptionsView _View;
        private bool _SuppressValueChangedEventHandler;
        #endregion

        #region Initialise
        public void Initialise(IOptionsView view)
        {
            _View = view;

            _View.SaveClicked += View_SaveClicked;
            _View.ResetClicked += View_ResetClicked;
            _View.SelectedInjectSettingsChanged += View_SelectedInjectSettingsChanged;
            _View.ValueChanged += View_ValueChanged;
            _View.NewInjectSettingsClicked += View_NewInjectSettingsClicked;
            _View.DeleteInjectSettingsClicked += View_DeleteInjectSettingsClicked;

            if(_View.InjectSettings.Count > 0) _View.SelectedInjectSettings = _View.InjectSettings[0];
        }
        #endregion

        #region CopySelectedInjectSettingsToFields
        private void CopySelectedInjectSettingsToFields()
        {
            var currentSuppressSetting = _SuppressValueChangedEventHandler;
            try {
                _SuppressValueChangedEventHandler = true;

                var settings = _View.SelectedInjectSettings;
                _View.InjectEnabled = settings != null ? settings.Enabled : false;
                _View.InjectFileName = settings != null ? settings.File : "";
                _View.InjectAtStart = settings != null ? settings.Start : true;
                _View.InjectOf = settings != null ? settings.InjectionLocation : InjectionLocation.Head;
                _View.InjectPathAndFile = settings != null ? settings.PathAndFile : "";

                _View.ShowValidationResults(new ValidationResults(isPartialValidation: false));
            } finally {
                _SuppressValueChangedEventHandler = currentSuppressSetting;
            }
        }
        #endregion

        #region 
        private bool DoValidation()
        {
            var results = new ValidationResults(isPartialValidation: false);

            if(_View.PluginEnabled) {
                var settings = _View.SelectedInjectSettings;
                if(settings != null) {
                    try {
                        if(String.IsNullOrEmpty(_View.InjectFileName)) results.Results.Add(new ValidationResult(ValidationField.Name, CustomContentStrings.FileNameRequired));
                        else if(!File.Exists(_View.InjectFileName))    results.Results.Add(new ValidationResult(ValidationField.Name, String.Format(CustomContentStrings.FileDoesNotExist, _View.InjectFileName)));
                    } catch(Exception ex) {
                        Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking injection file: {0}", ex.ToString());
                        results.Results.Add(new ValidationResult(ValidationField.Name, String.Format(CustomContentStrings.ErrorCheckingFileName, ex.Message)));
                    }

                    if(String.IsNullOrEmpty(_View.InjectPathAndFile)) results.Results.Add(new ValidationResult(ValidationField.PathAndFile, CustomContentStrings.PathAndFileRequired));
                    else if(_View.InjectPathAndFile != "*") {
                        if(_View.InjectPathAndFile[0] != '/') results.Results.Add(new ValidationResult(ValidationField.PathAndFile, CustomContentStrings.PathAndFileMissingRoot));
                        else if(!_View.InjectPathAndFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase) && !_View.InjectPathAndFile.EndsWith(".htm")) {
                            results.Results.Add(new ValidationResult(ValidationField.PathAndFile, CustomContentStrings.PathAndFileMissingExtension));
                        }
                    }
                }

                if(!String.IsNullOrEmpty(_View.SiteRootFolder)) {
                    string message = null;
                    try {
                        if(!Directory.Exists(_View.SiteRootFolder)) message = String.Format(CustomContentStrings.DirectoryDoesNotExist, _View.SiteRootFolder);
                        if(IsDuplicateSiteRootFolder(_View.SiteRootFolder)) message = String.Format(CustomContentStrings.DirectoryAlreadyInUse, _View.SiteRootFolder);
                    } catch(Exception ex) {
                        Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking custom content site root folder: {0}", ex.ToString());
                        message = String.Format(CustomContentStrings.ErrorCheckingFolder, ex.Message);
                    }
                    if(!String.IsNullOrEmpty(message)) results.Results.Add(new ValidationResult(ValidationField.SiteRootFolder, message));
                }

                if(!String.IsNullOrEmpty(_View.ResourceImagesFolder)) {
                    string message = null;
                    try {
                        if(!Directory.Exists(_View.ResourceImagesFolder)) message = String.Format(CustomContentStrings.DirectoryDoesNotExist, _View.ResourceImagesFolder);
                    } catch(Exception ex) {
                        Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception while checking custom content resource images folder: {0}", ex.ToString());
                        message = String.Format(CustomContentStrings.ErrorCheckingFolder, ex.Message);
                    }
                    if(!String.IsNullOrEmpty(message)) results.Results.Add(new ValidationResult(ValidationField.ResourceImagesFolder, message));
                }
            }

            _View.ShowValidationResults(results);

            return !results.HasErrors;
        }

        private bool IsDuplicateSiteRootFolder(string folder)
        {
            var result = !String.IsNullOrEmpty(folder);

            if(result) {
                var originalFolder = _View.SiteRoot.Folder;
                try {
                    _View.SiteRoot.Folder = folder;

                    var isOurs = _View.WebSite.IsSiteRootActive(_View.SiteRoot, folderMustMatch: true);
                    result = !isOurs;
                    if(result) {
                        var fullPath = Path.GetFullPath(folder);
                        if(fullPath[fullPath.Length - 1] != Path.DirectorySeparatorChar) fullPath += Path.DirectorySeparatorChar;
                        result = _View.WebSite.GetSiteRootFolders().Any(r => r.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
                    }
                } finally {
                    _View.SiteRoot.Folder = originalFolder;
                }
            }

            return result;
        }
        #endregion

        #region Events subscribed
        private void View_DeleteInjectSettingsClicked(object sender, EventArgs args)
        {
            var deleteSettings = _View.SelectedInjectSettings;
            if(deleteSettings != null) {
                _View.InjectSettings.Remove(deleteSettings);
                _View.RefreshInjectSettings();
                _View.SelectedInjectSettings = null;

                CopySelectedInjectSettingsToFields();
            }
        }

        private void View_NewInjectSettingsClicked(object sender, EventArgs args)
        {
            var settings = new InjectSettings();

            _View.InjectSettings.Add(settings);
            _View.RefreshInjectSettings();
            _View.SelectedInjectSettings = settings;

            CopySelectedInjectSettingsToFields();
            _View.FocusOnEditFields();
        }

        private void View_ResetClicked(object sender, EventArgs args)
        {
            CopySelectedInjectSettingsToFields();
        }

        private void View_SaveClicked(object sender, CancelEventArgs args)
        {
            var view = (IOptionsView)sender;

            if(!DoValidation()) args.Cancel = true;
        }

        private void View_SelectedInjectSettingsChanged(object sender, EventArgs args)
        {
            CopySelectedInjectSettingsToFields();
        }

        private void View_ValueChanged(object sender, EventArgs args)
        {
            if(!_SuppressValueChangedEventHandler) {
                var settings = _View.SelectedInjectSettings;
                if(settings != null) {
                    DoValidation();
                    settings.Enabled = _View.InjectEnabled;
                    settings.File = _View.InjectFileName;
                    settings.Start = _View.InjectAtStart;
                    settings.InjectionLocation = _View.InjectOf;
                    settings.PathAndFile = _View.InjectPathAndFile;
                    _View.DefaultInjectionFilesFolder = Path.GetDirectoryName(settings.File);

                    _View.RefreshSelectedInjectSettings();
                }
            }
        }
        #endregion
    }
}
