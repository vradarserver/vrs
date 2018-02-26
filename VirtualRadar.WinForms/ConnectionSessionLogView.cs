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
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Implements <see cref="IConnectionSessionLogView"/> with a WinForms dialog box.
    /// </summary>
    public partial class ConnectionSessionLogView : BaseForm, IConnectionSessionLogView
    {
        /// <summary>
        /// The presenter that is managing this view.
        /// </summary>
        private IConnectionSessionLogPresenter _Presenter;

        /// <summary>
        /// The object that will handle online help for us.
        /// </summary>
        private OnlineHelpHelper _OnlineHelp;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime StartDate
        {
            get { return dateTimePickerStartDate.Value; }
            set { dateTimePickerStartDate.Value = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime EndDate
        {
            get { return dateTimePickerEndDate.Value; }
            set { dateTimePickerEndDate.Value = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ShowSessionsClicked;

        /// <summary>
        /// Raises <see cref="ShowSessionsClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnShowSessionsClicked(EventArgs args)
        {
            EventHelper.Raise(ShowSessionsClicked, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionSessionLogView() : base()
        {
            InitializeComponent();

            _ValidationHelper = new ValidationHelper(errorProvider);
            _ValidationHelper.RegisterValidationField(ValidationField.EndDate, dateTimePickerEndDate);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessions"></param>
        public void ShowSessions(IEnumerable<LogClient> clients, IEnumerable<LogSession> sessions)
        {
            connectionSessionListControl.Populate(clients, sessions);
        }

        /// <summary>
        /// Called after the form has loaded but is not yet on screen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _Presenter = Factory.Resolve<IConnectionSessionLogPresenter>();
                _Presenter.Initialise(this);

                _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsConnectionSessionLogDialog);

                OnShowSessionsClicked(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raised when the Show button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShow_Click(object sender, EventArgs e)
        {
            OnShowSessionsClicked(EventArgs.Empty);
        }
    }
}
