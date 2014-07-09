// Copyright © 2012 onwards, Andrew Whewell
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
using System.IO;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control containing a directory textbox control and a browse button.
    /// </summary>
    [DefaultBindingProperty("Folder")]
    public partial class FolderControl : BaseUserControl
    {
        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Folder
        {
            get { return textBoxFolder.Text; }
            set { textBoxFolder.Text = value; }
        }

        /// <summary>
        /// Gets or sets the Description browser property.
        /// </summary>
        [DefaultValue("::PleaseSelectAFolder::")]
        public string BrowserDescription { get; set; }

        /// <summary>
        /// Gets or sets the ShowNewFolderButton browser property.
        /// </summary>
        [DefaultValue(true)]
        public bool BrowserShowNewFolderButton { get; set; }

        /// <summary>
        /// Raised when the folder text is changed.
        /// </summary>
        public event EventHandler FolderTextChanged;

        /// <summary>
        /// Raises <see cref="FolderTextChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFolderTextChanged(EventArgs args)
        {
            if(FolderTextChanged != null) FolderTextChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FolderControl()
        {
            InitializeComponent();

            BrowserDescription = "::PleaseSelectAFolder::";
            BrowserShowNewFolderButton = true;
        }

        /// <summary>
        /// Called when the user clicks the browse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            string folder = Folder;
            if(String.IsNullOrEmpty(folder) || !Directory.Exists(folder)) folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            using(var dialog = new FolderBrowserDialog() {
                Description = Localise.GetLocalisedText(BrowserDescription),
                SelectedPath = folder,
                ShowNewFolderButton = BrowserShowNewFolderButton,
            }) {
                if(dialog.ShowDialog() == DialogResult.OK) Folder = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Raised when the user changes the text in the folder control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxFolder_TextChanged(object sender, EventArgs e)
        {
            OnFolderTextChanged(e);
        }
    }
}
