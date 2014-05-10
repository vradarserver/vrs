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

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter.WinForms
{
    /// <summary>
    /// Displays the options to the user.
    /// </summary>
    public partial class OptionsView : Form, IOptionsView
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool PluginEnabled
        {
            get { return checkBoxEnabled.Checked; }
            set { checkBoxEnabled.Checked = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AllowUpdateOfOtherDatabases
        {
            get { return !checkBoxOnlyUpdateDatabasesCreatedByPlugin.Checked; }
            set { checkBoxOnlyUpdateDatabasesCreatedByPlugin.Checked = !value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DatabaseFileName
        {
            get { return fileNameDatabase.FileName.Trim(); }
            set { fileNameDatabase.FileName = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId
        {
            get { return feedSelectControl.SelectedFeedId; }
            set { feedSelectControl.SelectedFeedId = value; }
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

            PluginLocalise.Form(this);
            fileNameDatabase.BrowserTitle = PluginStrings.SelectDatabaseFile;
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
