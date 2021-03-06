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
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.BaseStation;
using System.Globalization;
using System.Threading;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Implements <see cref="ISplashView"/> using a WinForms dialog.
    /// </summary>
    public partial class SplashView : BaseForm, ISplashView
    {
        #region Fields
        /// <summary>
        /// The presenter that is controlling this view.
        /// </summary>
        private ISplashPresenter _Presenter;

        // The references that are taken by Initialise and then passed straight to the provider when it's created.
        private EventHandler<EventArgs<Exception>> _BackgroundThreadExceptionHandler;
        private string[] _CommandLineArgs;

        // The culture info in use on the UI thread.
        private CultureInfo _UIThreadCultureInfo;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ApplicationName
        {
            get { return labelApplicationTitle.Text; }
            set { labelApplicationTitle.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ApplicationVersion
        {
            get { return labelApplicationVersion.Text; }
            set { labelApplicationVersion.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool LoadSucceeded { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationAircraftList BaseStationAircraftList { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorXAircraftList { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUniversalPlugAndPlayManager UPnpManager { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SplashView() : base()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="commandLineArgs"></param>
        /// <param name="backgroundThreadExceptionHandler"></param>
        public void Initialise(string[] commandLineArgs, EventHandler<EventArgs<Exception>> backgroundThreadExceptionHandler)
        {
            _CommandLineArgs = commandLineArgs;
            _BackgroundThreadExceptionHandler = backgroundThreadExceptionHandler;
        }
        #endregion

        #region ReportProgress, ReportProblem, YesNoPrompt
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
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="quitApplication"></param>
        public void ReportProblem(string message, string title, bool quitApplication)
        {
            MessageBox.Show(message, title);
            if(quitApplication) Environment.Exit(1);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="defaultYes"></param>
        /// <returns></returns>
        public bool YesNoPrompt(string message, string title, bool defaultYes)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultYes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when the form has loaded but before it's on view.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                pictureBoxLogo.Image = ResourceImages.Logo128x128;
                Icon = ResourceImages.ApplicationIcon;
                labelApplicationTitle.Text = "";
                labelApplicationVersion.Text = "";
                labelProgressText.Text = "";

                _Presenter = Factory.Resolve<ISplashPresenter>();
                _Presenter.BackgroundThreadExceptionHandler = _BackgroundThreadExceptionHandler;
                _Presenter.CommandLineArgs = _CommandLineArgs;
                _Presenter.Initialise(this);

                _UIThreadCultureInfo = Thread.CurrentThread.CurrentUICulture;
                backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Raised on a background thread when the background worker starts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = _UIThreadCultureInfo;
            _Presenter.StartApplication();
        }

        /// <summary>
        /// Raised when the background thread has finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
            LoadSucceeded = e.Error == null;
            if(e.Error != null) throw new System.ApplicationException("An exception was thrown on a background thread, see inner exception for details", e.Error);
        }
        #endregion
    }
}
