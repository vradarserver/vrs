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
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The view that lets the user maintain a list of users.
    /// </summary>
    public partial class UsersView : ListDetailEditorForm, IUsersView
    {
        #region Fields
        private IUsersPresenter _Presenter;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<IUser> Records { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUser SelectedRecord
        {
            get { return GetSelectedListViewTag<IUser>(listView); }
            set { SelectListViewItemByTag(listView, value); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserManagerName
        {
            get { return labelUserManager.Text; }
            set { labelUserManager.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UserEnabled
        {
            get { return checkBoxEnabled.Checked; }
            set { checkBoxEnabled.Checked = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LoginName
        {
            get { return textBoxLoginName.Text.Trim(); }
            set { textBoxLoginName.Text = value ?? ""; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserName
        {
            get { return textBoxName.Text.Trim(); }
            set { textBoxName.Text = value ?? ""; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Password
        {
            get { return textBoxPassword.Text.Trim(); }
            set { textBoxPassword.Text = value ?? ""; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool NewEnabled
        {
            get { return buttonNew.Enabled; }
            set { buttonNew.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool DeleteEnabled
        {
            get { return buttonDelete.Enabled; }
            set { buttonDelete.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UserEnabledEnabled
        {
            get { return checkBoxEnabled.Enabled; }
            set { checkBoxEnabled.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool LoginNameEnabled
        {
            get { return textBoxLoginName.Enabled; }
            set { textBoxLoginName.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UserNameEnabled
        {
            get { return textBoxName.Enabled; }
            set { textBoxName.Enabled = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool PasswordEnabled
        {
            get { return textBoxPassword.Enabled; }
            set { textBoxPassword.Enabled = value; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public UsersView()
        {
            InitializeComponent();

            Records = new List<IUser>();
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the form has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            if(!DesignMode) {
                Localise.Form(this);

                _ValidationHelper = new ValidationHelper(errorProvider);
                _ValidationHelper.RegisterValidationField(ValidationField.Enabled, checkBoxEnabled);
                _ValidationHelper.RegisterValidationField(ValidationField.LoginName, textBoxLoginName);
                _ValidationHelper.RegisterValidationField(ValidationField.Name, textBoxName);
                _ValidationHelper.RegisterValidationField(ValidationField.Password, textBoxPassword);

                RegisterFormDetail(groupBoxSingleRecord, () => SelectedRecord);
                WireStandardEvents(listView, buttonNew, buttonDelete, buttonReset, new object[] {
                    checkBoxEnabled,
                    textBoxLoginName,
                    textBoxName,
                    textBoxPassword,
                }, buttonSave);

                _Presenter = Factory.Singleton.Resolve<IUsersPresenter>();
                _Presenter.Initialise(this);
            }

            base.OnLoad(e);
        }
        #endregion

        #region PopulateRecords, PopulateListViewItem, RefreshSelectedRecord, FocusOnEditFields
        /// <summary>
        /// Fills the records list view.
        /// </summary>
        protected override void PopulateRecords()
        {
            PopulateRecordsListView(listView, Records, SelectedRecord, r => SelectedRecord = r);
        }

        /// <summary>
        /// Fills a single list item's text with the values from the associated server.
        /// </summary>
        /// <param name="item"></param>
        protected override void PopulateListViewItem(ListViewItem item)
        {
            FillListViewItem<IUser>(item, r => 
            {
                return new string[] {
                    r.Enabled ? Strings.Yes : Strings.No,
                    r.LoginName ?? "",
                    r.Name ?? "",
                };
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshSelectedRecord()
        {
            DoRefreshSelectedRecord(listView, SelectedRecord);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void FocusOnEditFields()
        {
            textBoxLoginName.Focus();
        }
        #endregion
    }
}
