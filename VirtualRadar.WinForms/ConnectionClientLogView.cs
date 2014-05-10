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
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IConnectionClientLogView"/>.
    /// </summary>
    public partial class ConnectionClientLogView : BaseForm, IConnectionClientLogView
    {
        /// <summary>
        /// The object that will be controlling this view.
        /// </summary>
        private IConnectionClientLogPresenter _Presenter;

        /// <summary>
        /// The object that will handle online help for the dialog.
        /// </summary>
        private OnlineHelpHelper _OnlineHelp;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionClientLogView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessionMap"></param>
        public void ShowClientsAndSessions(IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessionMap)
        {
            connectionClientListControl.Populate(clients, sessionMap);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="client"></param>
        public void RefreshClientReverseDnsDetails(LogClient client)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { RefreshClientReverseDnsDetails(client); }));
            else               connectionClientListControl.RefreshClientReverseDnsDetails(client);
        }

        /// <summary>
        /// Called when the view is fully constructed but not yet on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _Presenter = Factory.Singleton.Resolve<IConnectionClientLogPresenter>();
                _Presenter.Initialise(this);

                _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsConnectionClientLogDialog);
            }
        }

        /// <summary>
        /// Raised when a client has been selected in the client list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectionClientListControl_SelectionChanged(object sender, EventArgs e)
        {
            List<LogClient> clients = new List<LogClient>() { connectionClientListControl.SelectedClient };
            connectionSessionListControl.Populate(clients, connectionClientListControl.SelectedSessions);
        }
    }
}
