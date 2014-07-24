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
