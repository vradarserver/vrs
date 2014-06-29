using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageWebServerAuthentication : Page
    {
        public override string PageTitle { get { return Strings.Users; } }

        public override Image PageIcon { get { return Images.Server16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        [PageEnabled]
        [LocalisedDisplayName("UserMustAuthenticate")]
        [LocalisedDescription("OptionsDescribeWebServerUserMustAuthenticate")]
        public Observable<bool> UsersMustAuthenticate { get; private set; }

        [LocalisedDisplayName("PermittedUsers")]
        [LocalisedDescription("OptionsDescribeWebServerPermittedUsers")]
        public ObservableList<IUser> WebServerUsers { get; private set; }

        public PageWebServerAuthentication()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            UsersMustAuthenticate = BindProperty<bool>(checkBoxEnabled);
            WebServerUsers = BindListProperty<IUser>(listUsers);
        }

        protected override void InitialiseControls()
        {
            listUsers.MasterList = OptionsView.PageUsers.Users;
        }

        public override void PageSelected()
        {
            listUsers.RefreshContent();
        }

        private void listUsers_FetchRecordContent(object sender, Controls.BindingListView.RecordContentEventArgs e)
        {
            var user = e.Record as IUser;
            if(user != null) {
                e.ColumnTexts.Add(user.LoginName ?? "");
                e.ColumnTexts.Add(user.Enabled ? Strings.Yes : Strings.No);
                e.ColumnTexts.Add(user.Name ?? "");
            }
        }
    }
}
