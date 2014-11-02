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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Presents the data sources values to the user.
    /// </summary>
    public partial class PageDataSources : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return Images.Notebook16x16; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageDataSources();
            }
        }
        #endregion

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageDataSources() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            var baseStationSettings = SettingsView.Configuration.BaseStationSettings;

            AddControlBinder(new FileNameStringBinder<BaseStationSettings>(baseStationSettings, fileDatabaseFileName,    r => r.DatabaseFileName,   (r,v) => r.DatabaseFileName = v));

            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderFlags,                       r => r.OperatorFlagsFolder,    (r,v) => r.OperatorFlagsFolder = v));
            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderSilhouettes,                 r => r.SilhouettesFolder,      (r,v) => r.SilhouettesFolder = v));
            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderPictures,                    r => r.PicturesFolder,         (r,v) => r.PicturesFolder = v));

            AddControlBinder(new CheckBoxBoolBinder<BaseStationSettings>(baseStationSettings, checkBoxSearchPictureSubFolders,   r => r.SearchPictureSubFolders,    (r,v) => r.SearchPictureSubFolders = v));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField, Control>() {
                { ValidationField.BaseStationDatabase,  fileDatabaseFileName },
                { ValidationField.FlagsFolder,          folderFlags },
                { ValidationField.SilhouettesFolder,    folderSilhouettes },
                { ValidationField.PicturesFolder,       folderPictures },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(fileDatabaseFileName,             Strings.DatabaseFileName,           Strings.OptionsDescribeDataSourcesDatabaseFileName);
            SetInlineHelp(folderFlags,                      Strings.FlagsFolder,                Strings.OptionsDescribeDataSourcesFlagsFolder);
            SetInlineHelp(folderSilhouettes,                Strings.SilhouettesFolder,          Strings.OptionsDescribeDataSourcesSilhouettesFolder);
            SetInlineHelp(folderPictures,                   Strings.PicturesFolder,             Strings.OptionsDescribeDataSourcesPicturesFolder);
            SetInlineHelp(checkBoxSearchPictureSubFolders,  Strings.SearchPictureSubFolders,    Strings.OptionsDescribeDataSourcesSearchPictureSubFolders);
        }
    }
}
