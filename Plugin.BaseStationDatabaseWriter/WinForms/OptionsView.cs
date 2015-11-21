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
        #region CombinedFeed
        /// <summary>
        /// The class that summarises receivers and merged feeds.
        /// </summary>
        class CombinedFeed
        {
            public int UniqueId { get; set; }

            public string Name { get; set; }
        }
        #endregion

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

        private int _ReceiverId;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId
        {
            get { return _ReceiverId; }
            set { SetField(ref _ReceiverId, value, () => ReceiverId); }
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
        /// <returns></returns>
        public bool DisplayView()
        {
            return ShowDialog() == DialogResult.OK;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
                fileNameDatabase.BrowserTitle = PluginStrings.SelectDatabaseFile;
                
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var config = configurationStorage.Load();
                var combinedFeed = config.Receivers.Select(r =>   new CombinedFeed() { UniqueId = r.UniqueId, Name = r.Name })
                           .Concat(config.MergedFeeds.Select(r => new CombinedFeed() { UniqueId = r.UniqueId, Name = r.Name }))
                           .ToArray();

                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxEnabled,                             r => r.PluginEnabled,                   (r,v) => r.PluginEnabled = v));
                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxOnlyUpdateDatabasesCreatedByPlugin,  r => !r.AllowUpdateOfOtherDatabases,    (r,v) => r.AllowUpdateOfOtherDatabases = !v) { ModelPropertyName = PropertyHelper.ExtractName<OptionsView>(r => r.AllowUpdateOfOtherDatabases) });
                AddControlBinder(new CheckBoxBoolBinder<OptionsView>(this, checkBoxWriteOnlineLookupsToDatabase,        r => r.SaveDownloadedAircraftDetails,   (r,v) => r.SaveDownloadedAircraftDetails = v));

                AddControlBinder(new FileNameStringBinder<OptionsView>(this, fileNameDatabase, r => r.DatabaseFileName, (r,v) => r.DatabaseFileName = v));

                AddControlBinder(new ComboBoxBinder<OptionsView, CombinedFeed, int>(this, comboBoxReceiverId, combinedFeed, r => r.ReceiverId, (r,v) => r.ReceiverId = v) {
                    GetListItemDescription = r => r.Name,
                    GetListItemValue = r => r.UniqueId,
                });

                InitialiseControlBinders();
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

        private void linkLabelUseDefaultFileName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            DatabaseFileName = Path.Combine(configurationStorage.Folder, "BaseStation.sqb");
        }

        private void buttonCreateDatabase_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(DatabaseFileName)) {
                bool fileExists = File.Exists(DatabaseFileName);
                bool zeroLength = fileExists && new FileInfo(DatabaseFileName).Length == 0;
                if(fileExists && !zeroLength)  MessageBox.Show("The database file already exists", "Cannot Create Database File");
                else {
                    var databaseService = Factory.Singleton.Resolve<IBaseStationDatabase>();
                    databaseService.CreateDatabaseIfMissing(DatabaseFileName);

                    MessageBox.Show(String.Format("Created the database file {0}", DatabaseFileName), "Created Database File");
                }
            }
        }
    }
}
