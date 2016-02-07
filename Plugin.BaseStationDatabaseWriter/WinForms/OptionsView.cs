// Copyright © 2010 onwards, Andrew Whewell
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
using System.IO;
using InterfaceFactory;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.WinForms;
using System.Linq.Expressions;
using VirtualRadar.WinForms.PortableBinding;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter.WinForms
{
    /// <summary>
    /// Displays the options to the user.
    /// </summary>
    public partial class OptionsView : BaseForm, IOptionsView, INotifyPropertyChanged
    {
        /// <summary>
        /// The object that holds the buisness logic for the view.
        /// </summary>
        private OptionsPresenter _Presenter;

        private bool _PluginEnabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool PluginEnabled
        {
            get { return _PluginEnabled; }
            set { SetField(ref _PluginEnabled, value, () => PluginEnabled); }
        }

        private bool _AllowUpdateOfOtherDatabases;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AllowUpdateOfOtherDatabases
        {
            get { return _AllowUpdateOfOtherDatabases; }
            set { SetField(ref _AllowUpdateOfOtherDatabases, value, () => AllowUpdateOfOtherDatabases); }
        }

        private string _DatabaseFileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DatabaseFileName
        {
            get { return _DatabaseFileName; }
            set { SetField(ref _DatabaseFileName, value, () => DatabaseFileName); }
        }

        private bool _SaveDownloadedAircraftDetails;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SaveDownloadedAircraftDetails
        {
            get { return _SaveDownloadedAircraftDetails; }
            set { SetField(ref _SaveDownloadedAircraftDetails, value, () => SaveDownloadedAircraftDetails); }
        }

        private bool _RefreshOutOfDateAircraft;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool RefreshOutOfDateAircraft
        {
            get { return _RefreshOutOfDateAircraft; }
            set { SetField(ref _RefreshOutOfDateAircraft, value, () => RefreshOutOfDateAircraft); }
        }

        private int _ReceiverId;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId
        {
            get { return _ReceiverId; }
            set { SetField(ref _ReceiverId, value, () => ReceiverId); }
        }

        private List<CombinedFeed> _CombinedFeeds = new List<CombinedFeed>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<CombinedFeed> CombinedFeeds
        {
            get { return _CombinedFeeds; }
        }

        private string _OnlineLookupWriteActionNotice;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string OnlineLookupWriteActionNotice
        {
            get { return _OnlineLookupWriteActionNotice; }
            set { SetField(ref _OnlineLookupWriteActionNotice, value, () => OnlineLookupWriteActionNotice); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler SaveClicked;

        /// <summary>
        /// Raises <see cref="SaveClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSaveClicked(EventArgs args)
        {
            EventHelper.Raise(SaveClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler UseDefaultFileNameClicked;

        /// <summary>
        /// Raises <see cref="UseDefaultFileNameClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnUseDefaultFileNameClicked(EventArgs args)
        {
            EventHelper.Raise(UseDefaultFileNameClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CreateDatabaseClicked;

        /// <summary>
        /// Raises <see cref="CreateDatabaseClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCreateDatabaseClicked(EventArgs args)
        {
            EventHelper.Raise(CreateDatabaseClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            EventHelper.Raise(PropertyChanged, this, args);
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if (selectorExpression == null) throw new ArgumentNullException("selectorExpression");
            MemberExpression body = selectorExpression.Body as MemberExpression;
            if (body == null) throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

            return true;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowCreateDatabaseOutcome(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
                fileNameDatabase.BrowserTitle = PluginStrings.SelectDatabaseFile;

                _Presenter = new OptionsPresenter();
                _Presenter.Initialise(this);

                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxEnabled,                             r => r.PluginEnabled,                   (r,v) => r.PluginEnabled = v));
                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxOnlyUpdateDatabasesCreatedByPlugin,  r => !r.AllowUpdateOfOtherDatabases,    (r,v) => r.AllowUpdateOfOtherDatabases = !v)    { ModelPropertyName = PropertyHelper.ExtractName<OptionsView>(r => r.AllowUpdateOfOtherDatabases) });
                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxWriteOnlineLookupsToDatabase,        r => r.SaveDownloadedAircraftDetails,   (r,v) => r.SaveDownloadedAircraftDetails = v)   { UpdateMode = DataSourceUpdateMode.OnPropertyChanged, });
                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxRefreshOutOfDateAircraft,            r => r.RefreshOutOfDateAircraft,        (r,v) => r.RefreshOutOfDateAircraft = v)        { UpdateMode = DataSourceUpdateMode.OnPropertyChanged, });

                AddControlBinder(new FileNameStringBinder<OptionsView>(this, fileNameDatabase, r => r.DatabaseFileName, (r,v) => r.DatabaseFileName = v));

                AddControlBinder(new ComboBoxBinder<OptionsView, CombinedFeed, int>(this, comboBoxReceiverId, CombinedFeeds, r => r.ReceiverId, (r,v) => r.ReceiverId = v) {
                    GetListItemDescription = r => r.Name,
                    GetListItemValue = r => r.UniqueId,
                });

                AddControlBinder(new LabelStringBinder<OptionsView>(this, labelWriteOnlineLookupsNotice, r => r.OnlineLookupWriteActionNotice, (r,v) => r.OnlineLookupWriteActionNotice = v));

                InitialiseControlBinders();
                EnableDisableControls();
            }
        }

        private void AllowUpdateOfOtherDatabases_Format(object sender, ConvertEventArgs args)
        {
            args.Value = !((bool)args.Value);
        }

        private void AllowUpdateOfOtherDatabases_Parse(object sender, ConvertEventArgs args)
        {
            args.Value = !((bool)args.Value);
        }

        private void EnableDisableControls()
        {
            labelWriteOnlineLookupsNotice.Enabled = checkBoxWriteOnlineLookupsToDatabase.Checked;
            checkBoxRefreshOutOfDateAircraft.Enabled = checkBoxWriteOnlineLookupsToDatabase.Checked;
        }

        private void linkLabelUseDefaultFileName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnUseDefaultFileNameClicked(EventArgs.Empty);
        }

        private void buttonCreateDatabase_Click(object sender, EventArgs e)
        {
            OnCreateDatabaseClicked(e);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            OnSaveClicked(e);
        }

        private void checkBoxWriteOnlineLookupsToDatabase_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }
    }
}
