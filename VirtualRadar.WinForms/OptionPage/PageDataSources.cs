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
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// Lets the user view and modify the data source options.
    /// </summary>
    public partial class PageDataSources : Page
    {
        public override string PageTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

        public override Image PageIcon { get { return Images.Notebook16x16; } }

        private string _DatabaseFileName;
        [LocalisedDisplayName("DatabaseFileName")]
        [LocalisedDescription("OptionsDescribeDataSourcesDatabaseFileName")]
        [ValidationField(ValidationField.BaseStationDatabase)]
        public string DatabaseFileName
        {
            get { return _DatabaseFileName; }
            set { SetField(ref _DatabaseFileName, value, () => DatabaseFileName); }
        }

        private string _FlagsFolder;
        [LocalisedDisplayName("FlagsFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesFlagsFolder")]
        [ValidationField(ValidationField.FlagsFolder)]
        public string FlagsFolder
        {
            get { return _FlagsFolder; }
            set { SetField(ref _FlagsFolder, value, () => FlagsFolder); }
        }

        private string _SilhouettesFolder;
        [LocalisedDisplayName("SilhouettesFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesSilhouettesFolder")]
        [ValidationField(ValidationField.SilhouettesFolder)]
        public string SilhouettesFolder
        {
            get { return _SilhouettesFolder; }
            set { SetField(ref _SilhouettesFolder, value, () => SilhouettesFolder); }
        }

        private string _PicturesFolder;
        [LocalisedDisplayName("PicturesFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesPicturesFolder")]
        [ValidationField(ValidationField.PicturesFolder)]
        public string PicturesFolder
        {
            get { return _PicturesFolder; }
            set { SetField(ref _PicturesFolder, value, () => PicturesFolder); }
        }

        private bool _SearchPictureSubFolders;
        [LocalisedDisplayName("SearchPictureSubFolders")]
        [LocalisedDescription("OptionsDescribeDataSourcesSearchPictureSubFolders")]
        public bool SearchPictureSubFolders
        {
            get { return _SearchPictureSubFolders; }
            set { SetField(ref _SearchPictureSubFolders, value, () => SearchPictureSubFolders); }
        }

        public PageDataSources() : base()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            AddBinding(this, fileDatabaseFileName,              r => r.DatabaseFileName,        r => r.FileName);
            AddBinding(this, folderFlags,                       r => r.FlagsFolder,             r => r.Folder);
            AddBinding(this, folderSilhouettes,                 r => r.SilhouettesFolder,       r => r.Folder);
            AddBinding(this, folderPictures,                    r => r.PicturesFolder,          r => r.Folder);
            AddBinding(this, checkBoxSearchPictureSubFolders,   r => r.SearchPictureSubFolders, r => r.Checked);
        }
    }
}
