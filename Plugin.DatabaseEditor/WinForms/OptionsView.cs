// Copyright © 2015 onwards, Andrew Whewell
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.Plugin.DatabaseEditor.WinForms
{
    /// <summary>
    /// The WinForms implementation of IOptionsView.
    /// </summary>
    public partial class OptionsView : BaseForm, IOptionsView
    {
        #region Properties
        private Options _Options;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Options Options
        {
            get { return _Options; }
            set {
                if(_Options != value) {
                    _Options = value;
                }
            }
        }

        /// <summary>
        /// See interface options.
        /// </summary>
        public string IndexPageAddress
        {
            get { return linkLabelIndexPageAddress.Text; }
            set {
                if(linkLabelIndexPageAddress.Text != value) {
                    linkLabelIndexPageAddress.Text = value;
                }
            }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsView()
        {
            InitializeComponent();
        }
        #endregion

        #region Form setup - OnLoad, ApplyBindings
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
                ApplyBindings();
                InitialiseControlBinders();
            }
        }

        /// <summary>
        /// Binds controls to the properties.
        /// </summary>
        private void ApplyBindings()
        {
            AddControlBinder(new CheckBoxBoolBinder<Options>(Options, checkBoxEnabled, r => r.Enabled, (r,v) => r.Enabled = v));
            AddControlBinder(new AccessControlBinder<Options>(Options, accessControl, r => r.Access));
        }
        #endregion

        #region Actions - OpenIndexPage
        /// <summary>
        /// Opens a web browser on the index page.
        /// </summary>
        private void OpenIndexPage()
        {
            Process.Start(IndexPageAddress);
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the user clicks on the index page link label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelIndexPageAddress_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenIndexPage();
        }
        #endregion
    }
}
