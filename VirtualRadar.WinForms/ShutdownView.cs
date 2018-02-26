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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IShutdownView"/>.
    /// </summary>
    public partial class ShutdownView : BaseForm, IShutdownView
    {
        /// <summary>
        /// The presenter that is controlling this form.
        /// </summary>
        private IShutdownPresenter _Presenter;

        // Objects passed to Initialise that will then be passed on to the presenter once the view is fully formed.
        private IUniversalPlugAndPlayManager _UPnpManager;
        private IBaseStationAircraftList _BaseStationAircraftList;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ShutdownView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uPnpManager"></param>
        /// <param name="baseStationAircraftList"></param>
        public void Initialise(IUniversalPlugAndPlayManager uPnpManager, IBaseStationAircraftList baseStationAircraftList)
        {
            _UPnpManager = uPnpManager;
            _BaseStationAircraftList = baseStationAircraftList;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        public void ReportProgress(string text)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { ReportProgress(text); }));
            else               labelProgressText.Text = text;
        }

        /// <summary>
        /// Called after the view has been created but before it is on display to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                labelProgressText.Text = "";

                _Presenter = Factory.Resolve<IShutdownPresenter>();
                _Presenter.UPnpManager = _UPnpManager;
                _Presenter.Initialise(this);

                backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Called on a background thread by the background thread worker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _Presenter.ShutdownApplication();
        }

        /// <summary>
        /// Raised when the background thread has finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }
    }
}
