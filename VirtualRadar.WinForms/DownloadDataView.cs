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
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Implements <see cref="IDownloadDataView"/> with a WinForms dialog.
    /// </summary>
    public partial class DownloadDataView : BaseForm, IDownloadDataView
    {
        /// <summary>
        /// The presenter that will be controlling this view.
        /// </summary>
        private IDownloadDataPresenter _Presenter;

        private string _Status;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status
        {
            get { return _Status; }
            set { _Status = value; labelRouteStatus.Text = String.Format("{0}: {1}", Strings.CurrentRouteData, _Status ?? ""); Application.DoEvents(); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler DownloadButtonClicked;

        /// <summary>
        /// Raises <see cref="DownloadButtonClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDownloadButtonClicked(EventArgs args)
        {
            EventHelper.Raise(DownloadButtonClicked, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public DownloadDataView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raised after the dialog has been initialised but before it is shown on screen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) {
                FormsLocalise.Form(this);

                Status = "";

                _Presenter = Factory.Resolve<IDownloadDataPresenter>();
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// Raised when the user clicks the download button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDownload_Click(object sender, EventArgs e)
        {
            OnDownloadButtonClicked(e);
        }
    }
}
