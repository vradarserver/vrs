// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms;

namespace VirtualRadar.Plugin.CustomContent.WinForms
{
    public partial class OptionsView : BaseForm, IOptionsView
    {
        #region Private enum
        /// <summary>
        /// An enumeration of the two values assignable to the "Start" boolean.
        /// </summary>
        enum At
        {
            Start,      // true
            End         // false
        }
        #endregion

        #region Fields
        private IOptionsPresenter _Presenter;
        private bool _SuppressItemSelectedEventHandler;
        #endregion

        #region Properties
        public IWebSite WebSite { get; set; }

        public SiteRoot SiteRoot { get; set; }

        public bool PluginEnabled
        {
            get { return checkBoxPluginViewEnabled.Checked; }
            set { checkBoxPluginViewEnabled.Checked = value; }
        }

        public List<InjectSettings> InjectSettings { get; private set; }

        public InjectSettings SelectedInjectSettings
        {
            get { return GetSelectedListViewTag<InjectSettings>(listViewInjectSettings); }
            set { SelectListViewItemByTag(listViewInjectSettings, value); }
        }

        public bool InjectEnabled
        {
            get { return checkBoxInjectEnabled.Checked; }
            set { checkBoxInjectEnabled.Checked = value; }
        }

        public string InjectFileName
        {
            get { return fileNameControlInjectFile.FileName; }
            set { fileNameControlInjectFile.FileName = value; }
        }

        public bool InjectAtStart
        {
            get { return GetSelectedComboBoxValue<At>(comboBoxInjectAt, At.Start) == At.Start; }
            set { SelectComboBoxItemByValue(comboBoxInjectAt, value ? At.Start : At.End); }
        }

        public InjectionLocation InjectOf
        {
            get { return GetSelectedComboBoxValue<InjectionLocation>(comboBoxInjectOf, InjectionLocation.Head); }
            set { SelectComboBoxItemByValue(comboBoxInjectOf, value); }
        }

        public string InjectPathAndFile
        {
            get { return textBoxInjectPathAndFile.Text.Trim(); }
            set { textBoxInjectPathAndFile.Text = value; }
        }

        public string SiteRootFolder
        {
            get { return folderControlSiteRootFolder.Folder.Trim(); }
            set { folderControlSiteRootFolder.Folder = value; }
        }

        public string DefaultInjectionFilesFolder
        {
            get { return fileNameControlInjectFile.BrowserDefaultFolder; }
            set { fileNameControlInjectFile.BrowserDefaultFolder = value; }
        }
        #endregion

        #region Events
        public event EventHandler DeleteInjectSettingsClicked;
        protected virtual void OnDeleteInjectSettingsClicked(EventArgs args)
        {
            if(DeleteInjectSettingsClicked != null) DeleteInjectSettingsClicked(this, args);
        }

        public event EventHandler NewInjectSettingsClicked;
        protected virtual void OnNewInjectSettingsClicked(EventArgs args)
        {
            if(NewInjectSettingsClicked != null) NewInjectSettingsClicked(this, args);
        }

        public event EventHandler ResetClicked;
        protected virtual void OnResetClicked(EventArgs args)
        {
            if(ResetClicked != null) ResetClicked(this, args);
        }

        public event CancelEventHandler SaveClicked;
        protected virtual void OnSaveClicked(CancelEventArgs args)
        {
            if(SaveClicked != null) SaveClicked(this, args);
        }

        public event EventHandler SelectedInjectSettingsChanged;
        protected virtual void OnSelectedInjectSettingsChanged(EventArgs args)
        {
            if(SelectedInjectSettingsChanged != null) SelectedInjectSettingsChanged(this, args);
        }
        #endregion

        #region Constructor
        public OptionsView()
        {
            InitializeComponent();

            InjectSettings = new List<InjectSettings>();

            _ValidationHelper = new ValidationHelper(errorProvider);
            _ValidationHelper.RegisterValidationField(ValidationField.Name, fileNameControlInjectFile);
            _ValidationHelper.RegisterValidationField(ValidationField.PathAndFile, textBoxInjectPathAndFile);
            _ValidationHelper.RegisterValidationField(ValidationField.SiteRootFolder, folderControlSiteRootFolder);
        }
        #endregion

        #region DisplayView, ShowValidationMessage
        public bool DisplayView()
        {
            PopulateInjectSettings();
            return ShowDialog() == DialogResult.OK;
        }
        #endregion

        #region PopulateInjectSettings, PopulateListViewItem
        /// <summary>
        /// Fills the InjectSettings list view.
        /// </summary>
        private void PopulateInjectSettings()
        {
            var currentSuppressState = _SuppressItemSelectedEventHandler;
            try {
                _SuppressItemSelectedEventHandler = true;
                PopulateListView(listViewInjectSettings, InjectSettings, SelectedInjectSettings, PopulateListViewItem, r => SelectedInjectSettings = r);
            } finally {
                _SuppressItemSelectedEventHandler = currentSuppressState;
            }
        }

        /// <summary>
        /// Fills a single list item's text with the values from the associated server.
        /// </summary>
        /// <param name="item"></param>
        private void PopulateListViewItem(ListViewItem item)
        {
            FillListViewItem<InjectSettings>(item, r => new String[] {
                r.Enabled ? Strings.Yes : Strings.No,
                r.File,
                r.InjectionLocation.ToString().ToUpper(),
                r.Start ? CustomContentStrings.Start : CustomContentStrings.End,
                r.PathAndFile,
            });
        }
        #endregion

        #region RefreshSelectedInjectSettings, RefreshInjectSettings
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshSelectedInjectSettings()
        {
            var selectedItem = FindListViewItemForRecord(listViewInjectSettings, SelectedInjectSettings);
            if(selectedItem != null) PopulateListViewItem(selectedItem);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshInjectSettings()
        {
            PopulateInjectSettings();
        }
        #endregion

        #region FocusOnEditFields
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void FocusOnEditFields()
        {
            fileNameControlInjectFile.Focus();
        }
        #endregion

        #region Events subscribed
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                var localiser = new Localiser(typeof(CustomContentStrings));
                localiser.Form(this);

                FillDropDownWithValues<At>(comboBoxInjectAt, new At[] { At.Start, At.End }, r => {
                    switch(r) {
                        case At.Start:  return CustomContentStrings.Start;
                        case At.End:    return CustomContentStrings.End;
                        default:        throw new NotImplementedException();
                    }
                });

                FillDropDownWithValues<InjectionLocation>(comboBoxInjectOf, Enum.GetValues(typeof(InjectionLocation)).OfType<InjectionLocation>(), r => r.ToString().ToUpper());

                _ValueChangedHelper.HookValueChanged(new Control[] {
                    checkBoxInjectEnabled,
                    fileNameControlInjectFile,
                    comboBoxInjectAt,
                    comboBoxInjectOf,
                    textBoxInjectPathAndFile,
                });

                _Presenter = Factory.Singleton.Resolve<IOptionsPresenter>();
                _Presenter.Initialise(this);
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            OnResetClicked(e);
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            OnDeleteInjectSettingsClicked(e);
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            OnNewInjectSettingsClicked(e);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var args = new CancelEventArgs();
            OnSaveClicked(args);
            if(args.Cancel) DialogResult = DialogResult.None;
        }

        private void listViewInjectSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!_SuppressItemSelectedEventHandler) OnSelectedInjectSettingsChanged(EventArgs.Empty);
        }
        #endregion

    }
}
