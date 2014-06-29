// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.WinForms.Controls
{
    public partial class PasswordControl : UserControl
    {
        /// <summary>
        /// The dummy password shown to the user. This should be something that the user
        /// is unlikely to enter via the user interface.
        /// </summary>
        private const string _DummyPassword = "A\t\t\t\t\t\tA";

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private string Content
        {
            get { return TrimPassword ? textBox.Text.Trim() : textBox.Text; }
            set { textBox.Text = value; }
        }

        [DefaultValue(true)]
        public bool TrimPassword { get; set; }

        [DefaultValue(true)]
        public bool ShowEmptyPassword { get; set; }

        private string _Password;
        [DefaultValue(null)]
        public string Password
        {
            get { return Content == _DummyPassword ? _Password : Content; }
            set {
                if(!Object.Equals(_Password, value)) {
                    var text = TrimPassword ? (value ?? "").Trim() : value ?? "";
                    _Password = text;
                    var newContent = ShowEmptyPassword && text == "" ? text : _DummyPassword;
                    if(newContent == Content)   OnTextChanged(EventArgs.Empty);
                    else                        Content = newContent;
                }
            }
        }

        public event EventHandler PasswordTextChanged;
        protected virtual void OnPasswordTextChanged(EventArgs args)
        {
            if(PasswordTextChanged != null) PasswordTextChanged(this, args);
        }

        public PasswordControl()
        {
            InitializeComponent();
            TrimPassword = true;
            ShowEmptyPassword = true;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            OnPasswordTextChanged(e);
        }
    }
}
