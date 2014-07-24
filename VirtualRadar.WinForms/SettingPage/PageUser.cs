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
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays a single user's details and accepts edits to them.
    /// </summary>
    public partial class PageUser : Page
    {
        public override Image PageIcon { get { return Images.User16x16; } }

        public IUser User { get { return PageObject as IUser; } }

        public PageUser()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            SetPageTitleProperty<IUser>(r => r.LoginName, () => User.LoginName);
            SetPageEnabledProperty<IUser>(r => r.Enabled, () => User.Enabled);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(User, checkBoxEnabled,   r => r.Enabled,     r => r.Checked);
            AddBinding(User, textBoxLoginName,  r => r.LoginName,   r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(User, textBoxPassword,   r => r.UIPassword,  r => r.Text);
            AddBinding(User, textBoxUserName,   r => r.Name,        r => r.Text);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.LoginName,    textBoxLoginName },
                { ValidationField.Password,     textBoxPassword },
                { ValidationField.Name,         textBoxUserName },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,  Strings.Enabled,    Strings.OptionsDescribeUserEnabled);
            SetInlineHelp(textBoxLoginName, Strings.LoginName,  Strings.OptionsDescribeLoginName);
            SetInlineHelp(textBoxPassword,  Strings.Password,   Strings.OptionsDescribeUserPassword);
            SetInlineHelp(textBoxUserName,  Strings.Name,       Strings.OptionsDescribeUserName);
        }
    }
}
