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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// Default implementation of <see cref="IConnectorActivityLogPresenter"/>.
    /// </summary>
    class ConnectorActivityLogPresenter : Presenter<IConnectorActivityLogView>, IConnectorActivityLogPresenter
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(IConnectorActivityLogView view)
        {
            base.Initialise(view);
            _View.CopySelectedItemsToClipboardClicked += View_CopySelectedItemsToClipboardClicked;
            _View.RefreshClicked += View_RefreshClicked;

            Populate();
        }

        /// <summary>
        /// Populates the view.
        /// </summary>
        private void Populate()
        {
            var events = _View.Connector != null ? _View.Connector.GetActivityHistory() : Factory.Singleton.Resolve<IConnectorActivityLog>().Singleton.GetActivityHistory();
            _View.Populate(events);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="utcTime"></param>
        /// <returns></returns>
        public string FormatTime(DateTime utcTime)
        {
            return utcTime.ToLocalTime().ToString("G");
        }

        /// <summary>
        /// Called when the user clicks the copy to clipboard button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void View_CopySelectedItemsToClipboardClicked(object sender, EventArgs e)
        {
            var buffer = new StringBuilder();
            var events = _View.SelectedConnectorActivityEvents;
            if(events.Length == 0) events = _View.ConnectorActivityEvents;
            foreach(var activity in events) {
                buffer.AppendLine(String.Format("[{0}] [{1}] [{2}] {3}",
                    FormatTime(activity.Time),
                    activity.ConnectorName,
                    Describe.ConnectorActivityType(activity.Type),
                    activity.Message)
                );
            }

            try {
                Clipboard.SetText(buffer.ToString());
            } catch {
                // The clipboard occasionally throws some bizarro exception but even when
                // it does it's usually worked
            }
        }

        /// <summary>
        /// Called when the user clicks the refresh button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void View_RefreshClicked(object sender, EventArgs e)
        {
            Populate();
        }
    }
}
