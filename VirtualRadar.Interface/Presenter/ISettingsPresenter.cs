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
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that provide the business logic for
    /// <see cref="ISettingsView"/> views.
    /// </summary>
    public interface ISettingsPresenter : IPresenter<ISettingsView>, IDisposable
    {
        /// <summary>
        /// Gets or sets the object that abstracts away the environment for the presenter.
        /// </summary>
        ISettingsPresenterProvider Provider { get; set; }

        /// <summary>
        /// Creates a new merged feed. The object is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        MergedFeed CreateMergedFeed();

        /// <summary>
        /// Creates a new rebroadcast server. The object is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        RebroadcastSettings CreateRebroadcastServer();

        /// <summary>
        /// Creates a new receiver. The receiver is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        Receiver CreateReceiver();

        /// <summary>
        /// Creates a new receiver location. The receiver location is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        ReceiverLocation CreateReceiverLocation();

        /// <summary>
        /// Creates a new user. The user is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        IUser CreateUser();

        /// <summary>
        /// Returns a list of serial port names. Guaranteed not to throw an exception and to be current (i.e. no caching).
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSerialPortNames();

        /// <summary>
        /// Returns a collection of voice names. A voice name of null indicates the presence of a default voice.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetVoiceNames();

        /// <summary>
        /// Returns a collection of tile server setting names.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetTileServerSettingNames();

        /// <summary>
        /// Validates the current content of the entire form and reports the results back to the view.
        /// </summary>
        void ValidateView();

        /// <summary>
        /// Applies the answers from a receiver configuration wizard to the receiver passed across.
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="receiver"></param>
        void ApplyReceiverConfigurationWizard(IReceiverConfigurationWizardAnswers answers, Receiver receiver);
    }
}
