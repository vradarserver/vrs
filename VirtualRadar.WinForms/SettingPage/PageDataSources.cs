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
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Presents the data sources values to the user.
    /// </summary>
    public partial class PageDataSources : Page
    {
        public override string PageTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

        public override Image PageIcon { get { return Images.Notebook16x16; } }

        public PageDataSources()
        {
            InitializeComponent();
        }

        protected override void CreateBindings()
        {
            AddBinding(SettingsView, fileDatabaseFileName,              r => r.Configuration.BaseStationSettings.DatabaseFileName,          r => r.FileName);
            AddBinding(SettingsView, folderFlags,                       r => r.Configuration.BaseStationSettings.OperatorFlagsFolder,       r => r.Folder);
            AddBinding(SettingsView, folderSilhouettes,                 r => r.Configuration.BaseStationSettings.SilhouettesFolder,         r => r.Folder);
            AddBinding(SettingsView, folderPictures,                    r => r.Configuration.BaseStationSettings.PicturesFolder,            r => r.Folder);
            AddBinding(SettingsView, checkBoxSearchPictureSubFolders,   r => r.Configuration.BaseStationSettings.SearchPictureSubFolders,   r => r.Checked);
        }

        protected override void AssociateValidationFields()
        {
            SetValidationFields(new Dictionary<ValidationField, Control>() {
                { ValidationField.BaseStationDatabase,  fileDatabaseFileName },
                { ValidationField.FlagsFolder,          folderFlags },
                { ValidationField.SilhouettesFolder,    folderSilhouettes },
                { ValidationField.PicturesFolder,       folderPictures },
            });
        }

        protected override void AssociateInlineHelp()
        {
            SetInlineHelp(fileDatabaseFileName,             Strings.DatabaseFileName,           Strings.OptionsDescribeDataSourcesDatabaseFileName);
            SetInlineHelp(folderFlags,                      Strings.FlagsFolder,                Strings.OptionsDescribeDataSourcesFlagsFolder);
            SetInlineHelp(folderSilhouettes,                Strings.SilhouettesFolder,          Strings.OptionsDescribeDataSourcesSilhouettesFolder);
            SetInlineHelp(folderPictures,                   Strings.PicturesFolder,             Strings.OptionsDescribeDataSourcesPicturesFolder);
            SetInlineHelp(checkBoxSearchPictureSubFolders,  Strings.SearchPictureSubFolders,    Strings.OptionsDescribeDataSourcesSearchPictureSubFolders);
        }
    }
}
