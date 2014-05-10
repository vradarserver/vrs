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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A class that helps forms service links to their online help pages.
    /// </summary>
    class OnlineHelpHelper
    {
        /// <summary>
        /// The form that this class is helping.
        /// </summary>
        private Form _Form;

        /// <summary>
        /// Gets the address of the online help for the form.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="onlineHelpAddress"></param>
        public OnlineHelpHelper(Form form, string onlineHelpAddress)
        {
            Address = onlineHelpAddress;

            _Form = form;
            _Form.KeyPreview = true;
            _Form.HelpButtonClicked += Form_HelpButtonClicked;
            _Form.KeyDown += Form_KeyDown;
        }

        /// <summary>
        /// Displays the online help.
        /// </summary>
        public void ShowHelp()
        {
            Process.Start(Address);
        }

        /// <summary>
        /// Called when the user clicks the help button on the form, if present.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Form_HelpButtonClicked(object sender, CancelEventArgs args)
        {
            ShowHelp();
            args.Cancel = true;
        }

        /// <summary>
        /// Called when the user presses a key on the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Form_KeyDown(object sender, KeyEventArgs args)
        {
            if(!args.Alt && !args.Control && !args.Shift && args.KeyCode == Keys.F1) {
                ShowHelp();
            }
        }
    }
}
