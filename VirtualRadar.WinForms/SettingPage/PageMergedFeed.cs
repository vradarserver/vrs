using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Handles the display of a merged feed's settings to the user.
    /// </summary>
    public partial class PageMergedFeed : Page
    {
        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        public MergedFeed MergedFeed { get { return PageObject as MergedFeed; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageMergedFeed()
        {
            InitializeComponent();
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(MergedFeed, checkBoxEnabled,                         r => r.Enabled,                         r => r.Checked);
            AddBinding(MergedFeed, textBoxName,                             r => r.Name,                            r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(MergedFeed, numericIcaoTimeout,                      r => r.IcaoTimeout,                     r => r.Value,   format: IcaoTimeout_Format, parse: IcaoTimeout_Parse);
            AddBinding(MergedFeed, checkBoxIgnoreAircraftWithNoPosition,    r => r.IgnoreAircraftWithNoPosition,    r => r.Checked);

            listReceiverIds.DataSource = CreateListBindingSource<Receiver>(SettingsView.Configuration.Receivers);
            listReceiverIds.CheckedSubset = MergedFeed.ReceiverIds;
            listReceiverIds.ExtractSubsetValue = (r) => ((Receiver)r).UniqueId;

            SetPageTitleProperty<MergedFeed>(r => r.Name, () => MergedFeed.Name);
            SetPageEnabledProperty<MergedFeed>(r => r.Enabled, () => MergedFeed.Enabled);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.Name,         textBoxName },
                { ValidationField.IcaoTimeout,  numericIcaoTimeout },
                { ValidationField.ReceiverIds,  listReceiverIds },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,                      Strings.Enabled,                        Strings.OptionsDescribeMergedFeedEnabled);
            SetInlineHelp(textBoxName,                          Strings.Name,                           Strings.OptionsDescribeMergedFeedName);
            SetInlineHelp(numericIcaoTimeout,                   Strings.IcaoTimeout,                    Strings.OptionsDescribeIcaoTimeout);
            SetInlineHelp(checkBoxIgnoreAircraftWithNoPosition, Strings.IgnoreAircraftWithNoPosition,   Strings.OptionsDescribeIcaoTimeout);
            SetInlineHelp(listReceiverIds,                      "",                                     "");
        }

        private void IcaoTimeout_Format(object sender, ConvertEventArgs args)
        {
            var value = (int)args.Value;
            args.Value = ((decimal)value) / 1000M;
        }

        private void IcaoTimeout_Parse(object sender, ConvertEventArgs args)
        {
            var value = (decimal)args.Value;
            args.Value = (int)(value * 1000M);
        }

        private void listReceiverIds_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs args)
        {
            var receiver = args.Record as Receiver;
            if(receiver != null) {
                args.ColumnTexts.Add(receiver.Name);
                args.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
            }
        }
    }
}
