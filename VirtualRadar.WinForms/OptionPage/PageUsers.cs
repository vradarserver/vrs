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
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageUsers : Page
    {
        private RecordListHelper<IUser, PageUser> _ListHelper;

        public override string PageTitle { get { return Strings.Users; } }

        public override Image PageIcon { get { return Images.User16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public ObservableList<IUser> Users { get; private set; }

        public PageUsers()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            Users = BindListProperty<IUser>(listUsers);
            _ListHelper = new RecordListHelper<IUser,PageUser>(this, listUsers, Users);
        }

        protected override void InitialiseControls()
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            labelUserManager.Text = userManager.Name;
            listUsers.CheckBoxes = userManager.CanEditUsers && userManager.CanChangeEnabledState;
            listUsers.AllowAdd = userManager.CanCreateUsers;
            listUsers.AllowUpdate = userManager.CanEditUsers;
            listUsers.AllowDelete = userManager.CanDeleteUsers;
        }

        private void listUsers_FetchRecordContent(object sender, Controls.BindingListView.RecordContentEventArgs e)
        {
            var record = e.Record as IUser;

            if(record != null) {
                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.LoginName ?? "");
                e.ColumnTexts.Add(record.Name ?? "");
            }
        }

        protected override Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            return _ListHelper.CreatePageForNewChildRecord(observableList, record);
        }

        private void listUsers_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => {
                var result = Factory.Singleton.Resolve<IUser>();
                result.Enabled = true;
                return result;
            });
        }

        private void listUsers_CheckedChanged(object sender, Controls.BindingListView.RecordCheckedEventArgs e)
        {
            _ListHelper.SetEnabledForListCheckedChanged(e, r => r.RecordEnabled);
        }

        private void listUsers_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listUsers_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }
    }
}
