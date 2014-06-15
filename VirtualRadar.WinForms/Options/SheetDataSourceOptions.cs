// Copyright © 2012 onwards, Andrew Whewell
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
using System.ComponentModel;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Settings;
using System.Globalization;
using System.Drawing.Design;
using System.IO.Ports;
using System.Drawing;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The sheet that displays properties for data sources.
    /// </summary>
    class SheetDataSourceOptions : Sheet<SheetDataSourceOptions>
    {
        /// <summary>
        /// See base class docs.
        /// </summary>
        public override string SheetTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

        /// <summary>
        /// See base class docs.
        /// </summary>
        public override Image Icon { get { return Images.Notebook16x16; } }

        // The number and display order of categories on the sheet
        private const int AircraftDataCategory = 0;
        private const int TotalCategories = 1;

        [DisplayOrder(10)]
        [LocalisedDisplayName("DatabaseFileName")]
        [LocalisedCategory("AircraftData", AircraftDataCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesDatabaseFileName")]
        [FileNameBrowser(BrowserTitle="::SelectBaseStationDatabaseFile::", Filter="SQLite database files (*.sqb)|*.sqb|All files (*.*)|*.*", DefaultExtension=".sqb")]
        [Editor(typeof(FileNameUITypeEditor), typeof(UITypeEditor))]
        [RaisesValuesChanged]
        public string DatabaseFileName { get; set; }
        public bool ShouldSerializeDatabaseFileName() { return ValueHasChanged(r => r.DatabaseFileName); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("FlagsFolder")]
        [LocalisedCategory("AircraftData", AircraftDataCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesFlagsFolder")]
        [FolderBrowser(Description="::PleaseSelectFlagsFolder::")]
        [Editor(typeof(FolderUITypeEditor), typeof(UITypeEditor))]
        [RaisesValuesChanged]
        public string FlagsFolder { get; set; }
        public bool ShouldSerializeFlagsFolder() { return ValueHasChanged(r => r.FlagsFolder); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("SilhouettesFolder")]
        [LocalisedCategory("AircraftData", AircraftDataCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesSilhouettesFolder")]
        [FolderBrowser(Description="::PleaseSelectSilhouettesFolder::")]
        [Editor(typeof(FolderUITypeEditor), typeof(UITypeEditor))]
        [RaisesValuesChanged]
        public string SilhouettesFolder { get; set; }
        public bool ShouldSerializeSilhouettesFolder() { return ValueHasChanged(r => r.SilhouettesFolder); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("PicturesFolder")]
        [LocalisedCategory("AircraftData", AircraftDataCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesPicturesFolder")]
        [FolderBrowser(Description="::PleaseSelectPicturesFolder::")]
        [Editor(typeof(FolderUITypeEditor), typeof(UITypeEditor))]
        [RaisesValuesChanged]
        public string PicturesFolder { get; set; }
        public bool ShouldSerializePicturesFolder() { return ValueHasChanged(r => r.PicturesFolder); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("SearchPictureSubFolders")]
        [LocalisedCategory("AircraftData", AircraftDataCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeDataSourcesSearchPictureSubFolders")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool SearchPictureSubFolders { get; set; }
        public bool ShouldSerializeSearchPictureSubFolders() { return ValueHasChanged(r => r.SearchPictureSubFolders); }
    }
}
