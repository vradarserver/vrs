using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays and allows for the editing of the list of users.
    /// </summary>
    public partial class PageUsers : Page
    {
        private RecordListHelper<IUser, PageUser> _ListHelper;

        public override string PageTitle { get { return Strings.Users; } }

        public override Image PageIcon { get { return Images.User16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageUsers()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Users, () => new PageUser());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, labelUserManager, r => r.UserManager, r => r.Text);
            _ListHelper = new RecordListHelper<IUser,PageUser>(this, listUsers, SettingsView.Users);
        }

        #region User list handling
        private void listUsers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var record = e.Record as IUser;

            if(record != null) {
                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.LoginName ?? "");
                e.ColumnTexts.Add(record.Name ?? "");
            }
        }

        private void listUsers_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => SettingsView.CreateUser());
        }

        private void listUsers_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listUsers_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }

        private void listUsers_CheckedChanged(object sender, Controls.BindingListView.RecordCheckedEventArgs e)
        {
            if(listUsers.CheckBoxes) {
                _ListHelper.SetEnabledForListCheckedChanged(e, (user, enabled) => user.Enabled = enabled);
            }
        }
        #endregion
    }
}
