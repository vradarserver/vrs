using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// Lets the user view and modify the data source options.
    /// </summary>
    public partial class PageDataSources : Page
    {
        public override string PageTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

        public override Image PageIcon { get { return Images.Notebook16x16; } }

        [LocalisedDisplayName("DatabaseFileName")]
        [LocalisedDescription("OptionsDescribeDataSourcesDatabaseFileName")]
        [ValidationField(ValidationField.BaseStationDatabase)]
        public Observable<string> DatabaseFileName { get; private set; }

        [LocalisedDisplayName("FlagsFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesFlagsFolder")]
        [ValidationField(ValidationField.FlagsFolder)]
        public Observable<string> FlagsFolder { get; private set; }

        [LocalisedDisplayName("SilhouettesFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesSilhouettesFolder")]
        [ValidationField(ValidationField.SilhouettesFolder)]
        public Observable<string> SilhouettesFolder { get; private set; }

        [LocalisedDisplayName("PicturesFolder")]
        [LocalisedDescription("OptionsDescribeDataSourcesPicturesFolder")]
        [ValidationField(ValidationField.PicturesFolder)]
        public Observable<string> PicturesFolder { get; private set; }

        [LocalisedDisplayName("SearchPictureSubFolders")]
        [LocalisedDescription("OptionsDescribeDataSourcesSearchPictureSubFolders")]
        public Observable<bool> SearchPictureSubFolders { get; private set; }

        public PageDataSources() : base()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            DatabaseFileName =          BindProperty<string>(fileDatabaseFileName);
            FlagsFolder =               BindProperty<string>(folderFlags);
            SilhouettesFolder =         BindProperty<string>(folderSilhouettes);
            PicturesFolder =            BindProperty<string>(folderPictures);
            SearchPictureSubFolders =   BindProperty<bool>(checkBoxSearchPictureSubFolders);
        }
    }
}
