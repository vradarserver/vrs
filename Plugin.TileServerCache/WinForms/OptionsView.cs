// Copyright © 2019 onwards, Andrew Whewell
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
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms;

namespace VirtualRadar.Plugin.TileServerCache.WinForms
{
    partial class OptionsView : BaseForm, IOptionsView
    {
        public long DataVersion { get; set; }

        public bool IsPluginEnabled
        {
            get => chkPluginEnabled.Checked;
            set => chkPluginEnabled.Checked = value;
        }

        public bool IsOfflineModeEnabled
        {
            get => chkOfflineModeEnabled.Checked;
            set => chkOfflineModeEnabled.Checked = value;
        }

        public bool UseDefaultCacheFolder
        {
            get => chkUseDefaultCacheFolder.Checked;
            set => chkUseDefaultCacheFolder.Checked = value;
        }

        public string CacheFolderOverride
        {
            get => fldCacheFolderOverride.Folder;
            set => fldCacheFolderOverride.Folder = value;
        }

        public int TileServerTimeoutSeconds
        {
            get => (int)nudTileServerTimeoutSeconds.Value;
            set => nudTileServerTimeoutSeconds.Value = value;
        }

        public bool CacheMapTiles
        {
            get => chkCacheMapTiles.Checked;
            set => chkCacheMapTiles.Checked = value;
        }

        public bool CacheLayerTiles
        {
            get => chkCacheLayerTiles.Checked;
            set => chkCacheLayerTiles.Checked = value;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsView()
        {
            InitializeComponent();

            _ValidationHelper = new ValidationHelper(errErrorProvider);
            _ValidationHelper.RegisterValidationField(ValidationField.CacheFolder, fldCacheFolderOverride);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool DisplayView()
        {
            EnableDisableControls();

            return ShowDialog() == DialogResult.OK;
        }

        private void EnableDisableControls()
        {
            fldCacheFolderOverride.Enabled = !UseDefaultCacheFolder;
        }

        private void ShowRecentRequests()
        {
            using(var dialog = new RecentRequestsView()) {
                dialog.ShowDialog(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(DialogResult == DialogResult.OK) {
                var controller = new OptionsController();
                if(!controller.DoValidation(this)) {
                    DialogResult = DialogResult.None;
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        private void ChkUseDefaultCacheFolder_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void BtnRecentRequests_Click(object sender, EventArgs e)
        {
            ShowRecentRequests();
        }
    }
}
