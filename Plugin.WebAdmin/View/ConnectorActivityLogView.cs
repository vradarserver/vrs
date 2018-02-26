// Copyright © 2016 onwards, Andrew Whewell
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
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    public class ConnectorActivityLogView : IConnectorActivityLogView
    {
        private IConnectorActivityLogPresenter _Presenter;

        private IConnectorActivityLog _ConnectorActivityLog;

        private object _SyncLock = new object();

        public IConnector Connector { get; set; }

        public bool HideConnectorName { get; set; }

        public ConnectorActivityEvent[] SelectedConnectorActivityEvents { get; set; }

        public ConnectorActivityEvent[] ConnectorActivityEvents { get; private set; }

        public event EventHandler RefreshClicked;
        private void OnRefreshClicked()
        {
            EventHelper.Raise(RefreshClicked, this, EventArgs.Empty);
        }

        #pragma warning disable 0067
        public event EventHandler CopySelectedItemsToClipboardClicked;
        #pragma warning restore 0067

        public ConnectorActivityLogView()
        {
            SelectedConnectorActivityEvents = new ConnectorActivityEvent[0];
        }

        public DialogResult ShowView()
        {
            _ConnectorActivityLog = Factory.ResolveSingleton<IConnectorActivityLog>();
            _Presenter = Factory.Resolve<IConnectorActivityLogPresenter>();
            _Presenter.Initialise(this);

            return DialogResult.OK;
        }

        public void Dispose()
        {
            ;
        }

        public void Populate(IEnumerable<ConnectorActivityEvent> connectorActivityEvents)
        {
            ConnectorActivityEvents = connectorActivityEvents.ToArray();
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            lock(_SyncLock) {
                OnRefreshClicked();

                var result = new ViewModel() {
                    Events = ConnectorActivityEvents.Select(r => new EventModel(r)).ToArray(),
                    Connectors = _ConnectorActivityLog
                        .GetActiveConnectors()
                        .Select(r => new ConnectorModel(r))
                        .OrderBy(r => r.Name)
                        .ToArray()
                };

                return result;
            }
        }
    }
}
