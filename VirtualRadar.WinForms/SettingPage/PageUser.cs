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
