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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageUser : Page
    {
        public override Image PageIcon { get { return Images.User16x16; } }

        public IUser User { get { return PageObject as IUser; } }

        [PageEnabled]
        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeUserEnabled")]
        public Observable<bool> RecordEnabled { get; private set; }

        [PageTitle]
        [LocalisedDisplayName("LoginName")]
        [LocalisedDescription("OptionsDescribeLoginName")]
        [ValidationField(ValidationField.LoginName)]
        public Observable<string> LoginName { get; private set; }

        [LocalisedDisplayName("Password")]
        [LocalisedDescription("OptionsDescribeUserPassword")]
        [ValidationField(ValidationField.Password)]
        public Observable<string> Password { get; private set; }

        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeUserName")]
        [ValidationField(ValidationField.Name)]
        public Observable<string> UserName { get; private set; }

        public PageUser()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordEnabled = BindProperty<bool>(checkBoxEnabled);
            LoginName = BindProperty<string>(textBoxLoginName);
            Password = BindProperty<string>(textBoxPassword);
            UserName = BindProperty<string>(textBoxUserName);
        }

        protected override void InitialiseControls()
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            checkBoxEnabled.Enabled = userManager.CanEditUsers && userManager.CanChangeEnabledState;
            textBoxLoginName.Enabled = userManager.CanEditUsers;
            textBoxPassword.Enabled = userManager.CanEditUsers && userManager.CanChangePassword;
            textBoxUserName.Enabled = userManager.CanEditUsers;
        }

        protected override void CopyRecordToObservables()
        {
            RecordEnabled.Value =   User.Enabled;
            LoginName.Value =       User.LoginName;
            Password.Value =        User.UIPassword;
            UserName.Value =        User.Name;
        }

        protected override void CopyObservablesToRecord()
        {
            User.Enabled =          RecordEnabled.Value;
            User.LoginName =        LoginName.Value;
            User.UIPassword =       Password.Value;
            User.Name =             UserName.Value;
        }
    }
}
