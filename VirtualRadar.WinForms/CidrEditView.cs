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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of ICidrEditView
    /// </summary>
    public partial class CidrEditView : BaseForm, ICidrEditView
    {
        private ICidrEditPresenter _Presenter;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Cidr
        {
            get { return textBoxCidr.Text.Trim(); }
            set { textBoxCidr.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CidrIsValid
        {
            get { return _Presenter == null ? false : _Presenter.CidrIsValid; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FirstMatchingAddress
        {
            get { return labelFirstMatchingAddress.Text; }
            set { labelFirstMatchingAddress.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LastMatchingAddress
        {
            get { return labelLastMatchingAddress.Text; }
            set { labelLastMatchingAddress.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CidrChanged;

        /// <summary>
        /// Raises <see cref="CidrChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCidrChanged(EventArgs args)
        {
            EventHelper.Raise(CidrChanged, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CidrEditView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called after the view has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                FormsLocalise.Form(this);

                _Presenter = Factory.Resolve<ICidrEditPresenter>();
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// Called whenever the CIDR changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCidr_TextChanged(object sender, EventArgs e)
        {
            OnCidrChanged(e);
        }

        /// <summary>
        /// Called whenever the user presses a key in the CIDR text control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCidr_KeyDown(object sender, KeyEventArgs e)
        {
            if(!e.Handled && e.Modifiers == Keys.None) {
                var handled = true;
                switch(e.KeyCode) {
                    case Keys.Escape:
                        Close();
                        Cidr = "";
                        break;
                    case Keys.Return:
                        Close();
                        break;
                    default:
                        handled = false;
                        break;
                }

                e.Handled = handled;
                if(handled) e.SuppressKeyPress = true;
            }
        }
    }
}
