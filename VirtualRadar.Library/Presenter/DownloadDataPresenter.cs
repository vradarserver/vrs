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
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// Implements <see cref="IDownloadDataPresenter"/>.
    /// </summary>
    class DownloadDataPresenter : IDownloadDataPresenter
    {
        /// <summary>
        /// The GUI object being controlled by this class.
        /// </summary>
        IDownloadDataView _View;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IDownloadDataView view)
        {
            _View = view;
            _View.DownloadButtonClicked += View_DownloadButtonClicked;

            _View.Status = Factory.Resolve<IStandingDataManager>().Singleton.RouteStatus;
        }

        /// <summary>
        /// Raised when the user clicks the download button on the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_DownloadButtonClicked(object sender, EventArgs args)
        {
            var standingDataManager = Factory.Resolve<IStandingDataManager>().Singleton;

            var busyState = _View.ShowBusy(true, null);
            try {
                _View.Status = Strings.DownloadingPleaseWait;

                IStandingDataUpdater updater = Factory.Resolve<IStandingDataUpdater>();
                updater.Update();

                standingDataManager.Load();
            } finally {
                _View.Status = standingDataManager.RouteStatus;
                _View.ShowBusy(false, busyState);
            }
        }
    }
}
