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
    /// The WinForms implementation of <see cref="IUsersListView"/>.
    /// </summary>
    public partial class UsersListView : BaseForm, IUsersListView
    {
        #region Fields
        private IUsersListPresenter _Presenter;
        private ListViewSortHelper _ListViewSortHelper;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<string> UserIds { get; private set; }

        /// <summary>
        /// Gets an array of users with a checkmark against them.
        /// </summary>
        private IUser[] CheckedUsers
        {
            get { return GetAllCheckedListViewTag<IUser>(listView); }
        }

        /// <summary>
        /// Gets the login name to add to the list view.
        /// </summary>
        private string LoginName
        {
            get { return textBoxLoginName.Text.Trim(); }
            set { textBoxLoginName.Text = value; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public UsersListView()
        {
            InitializeComponent();

            UserIds = new List<string>();
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the form has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _ListViewSortHelper = new ListViewSortHelper(listView);
                _ListViewSortHelper.SortColumn = 0;
                _ListViewSortHelper.SortOrder = SortOrder.Ascending;

                _Presenter = Factory.Singleton.Resolve<IUsersListPresenter>();
                _Presenter.Initialise(this);

                if(_Presenter.CanListAllUsers) HideLoginNamePanel();

                PopulateListView();
            }
        }

        /// <summary>
        /// Hides the login name panel.
        /// </summary>
        private void HideLoginNamePanel()
        {
            var extraHeight = panelLoginName.Height + (listView.Top - panelLoginName.Bottom);
            listView.Top = panelLoginName.Top;
            listView.Height += extraHeight;
            panelLoginName.Hide();
        }
        #endregion

        #region PopulateListView
        /// <summary>
        /// Loads the list view with user records.
        /// </summary>
        private void PopulateListView()
        {
            var users = _Presenter.GetUserList();

            PopulateListView<IUser>(listView, users, null, PopulateListViewItem, null);
            listView.Sort();
        }

        private void PopulateListViewItem(ListViewItem item)
        {
            FillAndCheckListViewItem<IUser>(item, r => 
            {
                return new string[] {
                    r.LoginName ?? "",
                    r.Name ?? "",
                    r.Enabled ? Strings.Yes : Strings.No,
                };
            }, r => UserIds.Contains(r.UniqueId));
        }
        #endregion

        #region Events subscribed
        private void buttonAddLoginName_Click(object sender, EventArgs e)
        {
            var loginName = LoginName;
            if(!String.IsNullOrEmpty(loginName)) {
                string errorMessage = null;
                var user = _Presenter.GetUserByLoginName(loginName);
                if(user == null) errorMessage = String.Format(Strings.LoginNameUnknown, loginName);
                else if(!UserIds.Contains(user.UniqueId)) {
                    UserIds.Add(user.UniqueId);

                    var listViewItem = new ListViewItem() {
                        Tag = user,
                    };
                    PopulateListViewItem(listViewItem);
                    listView.Items.Add(listViewItem);
                }

                textBoxLoginName.SelectAll();
                errorProvider.SetError(buttonAddLoginName, errorMessage);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            UserIds.Clear();
            UserIds.AddRange(CheckedUsers.Select(r => r.UniqueId));
        }
        #endregion
    }
}
