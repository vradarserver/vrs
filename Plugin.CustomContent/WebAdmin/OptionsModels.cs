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

namespace VirtualRadar.Plugin.CustomContent.WebAdmin
{
    /// <summary>
    /// Carries the option view's properties to the web page.
    /// </summary>
    public class ViewModel
    {
        public long DataVersion { get; set; }

        public bool Enabled { get; set; }

        public List<InjectSettingsModel> InjectSettings { get; private set; }

        public string DefaultInjectionFilesFolder { get; set; }

        public string SiteRootFolder { get; set; }

        [ValidationModelField(ValidationField.SiteRootFolder)]
        public ValidationModelField SiteRootFolderValidation { get; set; }

        public string ResourceImagesFolder { get; set; }

        [ValidationModelField(ValidationField.ResourceImagesFolder)]
        public ValidationModelField ResourceImagesFolderValidation { get; set; }

        public EnumModel[] InjectionLocations { get; private set; }

        public ViewModel()
        {
            InjectSettings = new List<InjectSettingsModel>();
            InjectionLocations = EnumModel.CreateFromEnum<InjectionLocation>(r => Describe.InjectionLocation(r));
        }

        public ViewModel(Options options) : this()
        {
            RefreshFromSettings(options);
        }

        public void RefreshFromSettings(Options settings)
        {
            DataVersion =                   settings.DataVersion;
            Enabled =                       settings.Enabled;
            DefaultInjectionFilesFolder =   settings.DefaultInjectionFilesFolder;
            SiteRootFolder =                settings.SiteRootFolder;
            ResourceImagesFolder =          settings.ResourceImagesFolder;

            InjectSettings.Clear();
            InjectSettings.AddRange(settings.InjectSettings.Select(r => new InjectSettingsModel(r)));
        }

        public Options CopyToSettings(Options settings)
        {
            settings.DataVersion =                  DataVersion;
            settings.Enabled =                      Enabled;
            settings.DefaultInjectionFilesFolder =  DefaultInjectionFilesFolder;
            settings.SiteRootFolder =               SiteRootFolder;
            settings.ResourceImagesFolder =         ResourceImagesFolder;

            settings.InjectSettings.Clear();
            settings.InjectSettings.AddRange(InjectSettings.Select(r => {
                var injectSettings = new InjectSettings();
                return r.CopyToSettings(injectSettings);
            }));

            return settings;
        }
    }

    public class InjectSettingsModel
    {
        public bool Enabled { get; set; }

        public string PathAndFile { get; set; }

        [ValidationModelField(ValidationField.PathAndFile)]
        public ValidationModelField PathAndFileValidation { get; set; }

        public int InjectionLocation { get; set; }

        public bool Start { get; set; }

        public string File { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField FileValidation { get; set; }

        public InjectSettingsModel()
        {
        }

        public InjectSettingsModel(InjectSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(InjectSettings settings)
        {
            Enabled =           settings.Enabled;
            PathAndFile =       settings.PathAndFile;
            InjectionLocation = (int)settings.InjectionLocation;
            Start =             settings.Start;
            File =              settings.File;
        }

        public InjectSettings CopyToSettings(InjectSettings settings)
        {
            settings.Enabled =              Enabled;
            settings.PathAndFile =          PathAndFile;
            settings.InjectionLocation =    (InjectionLocation)InjectionLocation;
            settings.Start =                Start;
            settings.File =                 File;

            return settings;
        }
    }

    public class SaveOutcomeModel
    {
        public string Outcome { get; set; }

        public ViewModel ViewModel { get; set; }

        public SaveOutcomeModel(string outcome, ViewModel viewModel)
        {
            Outcome = outcome;
            ViewModel = viewModel;
        }
    }
}
