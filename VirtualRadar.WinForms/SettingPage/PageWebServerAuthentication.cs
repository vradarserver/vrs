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
using VirtualRadar.Interface.Settings;
using System.Net;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Allows configuration of the valid user list for the web site.
    /// </summary>
    public partial class PageWebServerAuthentication : Page
    {
        public override string PageTitle { get { return Strings.Users; } }

        public override Image PageIcon { get { return Images.Server16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageWebServerAuthentication()
        {
            InitializeComponent();
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();

            AddBinding(SettingsView, checkBoxEnabled, r => r.Configuration.WebServerSettings.AuthenticationScheme, r => r.Checked, format: Enabled_Format, parse: Enabled_Parse);

            listUsers.DataSource = CreateListBindingSource<IUser>(SettingsView.Users);
            listUsers.CheckedSubset = SettingsView.Configuration.WebServerSettings.BasicAuthenticationUserIds;
            listUsers.ExtractSubsetValue = (r) => ((IUser)r).UniqueId;
        }

        private void Enabled_Format(object sender, ConvertEventArgs args)
        {
            var scheme = (AuthenticationSchemes)args.Value;
            args.Value = scheme == AuthenticationSchemes.Basic;
        }

        private void Enabled_Parse(object sender, ConvertEventArgs args)
        {
            var enabled = (bool)args.Value;
            args.Value = enabled ? AuthenticationSchemes.Basic : AuthenticationSchemes.Anonymous;
        }

        private void listUsers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
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
