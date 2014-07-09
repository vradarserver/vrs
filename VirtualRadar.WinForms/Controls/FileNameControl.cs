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
    /// A user control containing a filename textbox control and a browse button.
    /// </summary>
    [DefaultBindingProperty("FileName")]
    public partial class FileNameControl : BaseUserControl
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FileName
        {
            get { return textBoxFileName.Text; }
            set { textBoxFileName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the default folder to open on.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BrowserDefaultFolder { get; set; }

        /// <summary>
        /// Gets or sets the AddExtension browser property.
        /// </summary>
        [DefaultValue(true)]
        public bool BrowserAddExtension { get; set; }

        /// <summary>
        /// Gets or sets the CheckFileExists browser property.
        /// </summary>
        [DefaultValue(false)]
        public bool BrowserCheckFileExists { get; set; }

        /// <summary>
        /// Gets or sets the DefaultExt browser property.
        /// </summary>
        [DefaultValue("")]
        public string BrowserDefaultExt { get; set; }

        /// <summary>
        /// Gets or sets the Filter browser property.
        /// </summary>
        [DefaultValue("All files (*.*)|*.*")]
        public string BrowserFilter { get; set; }

        /// <summary>
        /// Gets or sets the Title browser property.
        /// </summary>
        [DefaultValue("::PleaseSelectAFile::")]
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Raised when the filename text is changed.
        /// </summary>
        public event EventHandler FileNameTextChanged;

        /// <summary>
        /// Raises <see cref="FileNameTextChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFileNameTextChanged(EventArgs args)
        {
            if(FileNameTextChanged != null) FileNameTextChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileNameControl() : base()
        {
            InitializeComponent();

            BrowserAddExtension = true;
            BrowserDefaultExt = "";
            BrowserFilter = "All files (*.*)|*.*";
            BrowserTitle = "::PleaseSelectAFile::";
        }

        /// <summary>
        /// Called when the user clicks the browse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            string folder = null;
            if(!String.IsNullOrEmpty(FileName)) folder = Path.GetDirectoryName(FileName);
            if(!Directory.Exists(folder)) {
                if(!String.IsNullOrEmpty(BrowserDefaultFolder) && Directory.Exists(BrowserDefaultFolder)) folder = BrowserDefaultFolder;
                else folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            using(var dialog = new OpenFileDialog() {
                AddExtension = BrowserAddExtension,
                AutoUpgradeEnabled = true,
                CheckFileExists = BrowserCheckFileExists,
                DefaultExt = BrowserDefaultExt,
                FileName = FileName,
                Filter = BrowserFilter,
                InitialDirectory = folder,
                Multiselect = false,
                Title = Localise.GetLocalisedText(BrowserTitle),
            }) {
                if(dialog.ShowDialog() == DialogResult.OK) FileName = dialog.FileName;
            }
        }

        /// <summary>
        /// Raised when the user changes the text in the filename control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxFileName_TextChanged(object sender, EventArgs e)
        {
            OnFileNameTextChanged(e);
        }
    }
}
