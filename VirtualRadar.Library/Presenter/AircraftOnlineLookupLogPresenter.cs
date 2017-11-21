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
    /// Default implementation of <see cref="IAircraftOnlineLookupLogPresenter"/>.
    /// </summary>
    class AircraftOnlineLookupLogPresenter : Presenter<IAircraftOnlineLookupLogView>, IAircraftOnlineLookupLogPresenter
    {
        /// <summary>
        /// The log that we're showing entries from.
        /// </summary>
        IAircraftOnlineLookupLog _AircraftOnlineLookupLog;

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AircraftOnlineLookupLogPresenter()
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
                if(_AircraftOnlineLookupLog != null) {
                    _AircraftOnlineLookupLog.ResponsesChanged -= AircraftOnlineLookupLog_ResponsesChanged;
                    _AircraftOnlineLookupLog = null;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(IAircraftOnlineLookupLogView view)
        {
            base.Initialise(view);

            var log = Factory.Singleton.ResolveSingleton<IAircraftOnlineLookupLog>();
            _View.Populate(log.GetResponses());

            _AircraftOnlineLookupLog = log;
            _AircraftOnlineLookupLog.ResponsesChanged += AircraftOnlineLookupLog_ResponsesChanged;
        }

        /// <summary>
        /// Called when the log changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftOnlineLookupLog_ResponsesChanged(object sender, EventArgs args)
        {
            _View.Populate(_AircraftOnlineLookupLog.GetResponses());
        }
    }
}
