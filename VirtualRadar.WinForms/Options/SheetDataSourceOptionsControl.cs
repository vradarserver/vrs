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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Resources;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// Implements a user control for entry of data source options.
    /// </summary>
    public partial class SheetDataSourceOptionsControl : SheetControl
    {
        #region Sheet Properties
        /// <summary>
        /// See base class docs.
        /// </summary>
        public override string SheetTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

        /// <summary>
        /// See base class docs.
        /// </summary>
        public override Image Icon { get { return Images.Notebook16x16; } }
        #endregion

        public string DatabaseFileName
        {
            get { return fileDatabaseFileName.FileName; }
            set { fileDatabaseFileName.FileName = value; }
        }

        public string FlagsFolder
        {
            get { return folderFlags.Folder; }
            set { folderFlags.Folder = value; }
        }

        public string SilhouettesFolder
        {
            get { return folderSilhouettes.Folder; }
            set { folderSilhouettes.Folder = value; }
        }
        
        public string PicturesFolder
        {
            get { return folderPictures.Folder; }
            set { folderPictures.Folder = value; }
        }

        public bool SearchPictureSubFolders
        {
            get { return checkBoxSearchPictureSubFolders.Checked; }
            set { checkBoxSearchPictureSubFolders.Checked = value; }
        }

        public SheetDataSourceOptionsControl() : base()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                AddLocalisedDescription(fileDatabaseFileName,               Strings.DatabaseFileName,           Strings.OptionsDescribeDataSourcesDatabaseFileName);
                AddLocalisedDescription(folderFlags,                        Strings.FlagsFolder,                Strings.OptionsDescribeDataSourcesFlagsFolder);
                AddLocalisedDescription(folderSilhouettes,                  Strings.SilhouettesFolder,          Strings.OptionsDescribeDataSourcesSilhouettesFolder);
                AddLocalisedDescription(folderPictures,                     Strings.PicturesFolder,             Strings.OptionsDescribeDataSourcesPicturesFolder);
                AddLocalisedDescription(checkBoxSearchPictureSubFolders,    Strings.SearchPictureSubFolders,    Strings.OptionsDescribeDataSourcesSearchPictureSubFolders);

                RaisesValueChanged(
                    fileDatabaseFileName,
                    folderFlags,
                    folderSilhouettes,
                    folderPictures
                );

                _ValidationHelper.RegisterValidationField(ValidationField.BaseStationDatabase,  fileDatabaseFileName);
                _ValidationHelper.RegisterValidationField(ValidationField.FlagsFolder,          folderFlags);
                _ValidationHelper.RegisterValidationField(ValidationField.SilhouettesFolder,    folderSilhouettes);
                _ValidationHelper.RegisterValidationField(ValidationField.PicturesFolder,       folderPictures);
            }
        }
    }
}
