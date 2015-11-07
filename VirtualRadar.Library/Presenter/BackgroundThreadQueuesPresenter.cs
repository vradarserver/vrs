// Copyright © 2015 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IBackgroundThreadQueuesPresenter"/>.
    /// </summary>
    class BackgroundThreadQueuesPresenter : Presenter<IBackgroundThreadQueuesView>, IBackgroundThreadQueuesPresenter
    {
        /// <summary>
        /// The heartbeat service that was hooked in <see cref="Initialise"/>.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~BackgroundThreadQueuesPresenter()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_HeartbeatService != null) {
                    _HeartbeatService.FastTick -= HeartbeatService_FastTick;
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(IBackgroundThreadQueuesView view)
        {
            base.Initialise(view);

            var heartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
            heartbeatService.FastTick += HeartbeatService_FastTick;
            _HeartbeatService = heartbeatService;

            ShowQueues();
        }

        /// <summary>
        /// Refreshes the view's display of queues.
        /// </summary>
        private void ShowQueues()
        {
            _View.RefreshDisplay(QueueRepository.GetAllQueues());
        }

        /// <summary>
        /// Called once every second or so by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_FastTick(object sender, EventArgs args)
        {
            ShowQueues();
        }
    }
}
