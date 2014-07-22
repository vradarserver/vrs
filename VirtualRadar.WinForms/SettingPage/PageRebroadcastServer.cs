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
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the settings for an individual rebroadcast server.
    /// </summary>
    public partial class PageRebroadcastServer : Page
    {
        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        public RebroadcastSettings RebroadcastSettings { get { return PageObject as RebroadcastSettings; } }

        public PageRebroadcastServer()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxReceiver.DataSource =   CreateSortingBindingSource<CombinedFeed>(SettingsView.CombinedFeed, r => r.Name);
            comboBoxFormat.DataSource =     CreateSortingEnumSource<RebroadcastFormat>(r => Describe.RebroadcastFormat(r));

            SetPageTitleProperty<RebroadcastSettings>(r => r.Name, () => RebroadcastSettings.Name);
            SetPageEnabledProperty<RebroadcastSettings>(r => r.Enabled, () => RebroadcastSettings.Enabled);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(RebroadcastSettings, checkBoxEnabled,        r => r.Enabled,         r => r.Checked);
            AddBinding(RebroadcastSettings, textBoxName,            r => r.Name,            r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(RebroadcastSettings, comboBoxReceiver,       r => r.ReceiverId,      r => r.SelectedValue);
            AddBinding(RebroadcastSettings, comboBoxFormat,         r => r.Format,          r => r.SelectedValue);
            AddBinding(RebroadcastSettings, numericPort,            r => r.Port,            r => r.Value);
            AddBinding(RebroadcastSettings, numericStaleSeconds,    r => r.StaleSeconds,    r => r.Value);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.Name,                     textBoxName },
                { ValidationField.RebroadcastServerPort,    numericPort },
                { ValidationField.Format,                   comboBoxFormat },
                { ValidationField.RebroadcastReceiver,      comboBoxReceiver },
                { ValidationField.StaleSeconds,             numericStaleSeconds },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,      Strings.Enabled,        Strings.OptionsDescribeRebroadcastServerEnabled);
            SetInlineHelp(textBoxName,          Strings.Name,           Strings.OptionsDescribeRebroadcastServerName);
            SetInlineHelp(comboBoxReceiver,     Strings.Receiver,       Strings.OptionsDescribeRebroadcastReceiver);
            SetInlineHelp(comboBoxFormat,       Strings.Format,         Strings.OptionsDescribeRebroadcastServerFormat);
            SetInlineHelp(numericPort,          Strings.Port,           Strings.OptionsDescribeRebroadcastServerPort);
            SetInlineHelp(numericStaleSeconds,  Strings.StaleSeconds,   Strings.OptionsDescribeRebroadcastStaleSeconds);
        }
    }
}
