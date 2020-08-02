// Copyright © 2020 onwards, Andrew Whewell
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
using VirtualRadar.Interface.XPlane;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// WinForms implementation of <see cref="IXPlaneView"/>.
    /// </summary>
    public partial class XPlaneView : BaseForm, IXPlaneView
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Host
        {
            get => txtXPlaneHost.Text;
            set => txtXPlaneHost.Text = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int XPlanePort
        {
            get => (int)nudXPlanePort.Value;
            set => nudXPlanePort.Value = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReplyPort
        {
            get => (int)nudReplyPort.Value;
            set => nudReplyPort.Value = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ConnectionStatus
        {
            get => lblConnectionStatus.Text;
            set => lblConnectionStatus.Text = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CloseClicked;

        /// <summary>
        /// Raises <see cref="CloseClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCloseClicked(EventArgs args)
        {
            EventHelper.Raise(CloseClicked, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public XPlaneView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called once the form has been initialised but before it's on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                var presenter = Factory.Resolve<IXPlanePresenter>();
                presenter.Initialise(this);
            }
        }

        /// <summary>
        /// Called when the form is closing but before it has closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            OnCloseClicked(e);

            var presenter = Factory.Resolve<IXPlanePresenter>();
            presenter.Disconnect(this);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var presenter = Factory.Resolve<IXPlanePresenter>();
            presenter.Connect(this);
        }
    }
}
