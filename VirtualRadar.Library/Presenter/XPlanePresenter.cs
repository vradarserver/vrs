// Copyright © 2020 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.XPlane;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// Default implementation of <see cref="IXPlanePresenter"/>.
    /// </summary>
    class XPlanePresenter : IXPlanePresenter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IXPlaneView view)
        {
            var storage = Factory.Resolve<IXPlaneSettingsStorage>();
            var settings = storage.Load();

            view.Host = settings.Host;
            view.XPlanePort = settings.XPlanePort;
            view.ReplyPort = settings.ReplyPort;
            view.ConnectionStatus = Strings.NotConnected;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Connect(IXPlaneView view)
        {
            view.ConnectionStatus = "";

            var settings = new XPlaneSettings() {
                Host =          view.Host,
                XPlanePort =    view.XPlanePort,
                ReplyPort =     view.ReplyPort,
            };

            var validationMessage = ValidateSettings(settings);
            view.ConnectionStatus = validationMessage ?? "";

            if(String.IsNullOrEmpty(validationMessage)) {
                var connection = Factory.ResolveSingleton<IXPlaneConnection>();
                try {
                    connection.Connect(settings);
                    view.ConnectionStatus = Strings.Connected;
                } catch(Exception ex) {
                    view.ConnectionStatus = $"Caught exception when connecting: {ex}";
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Disconnect(IXPlaneView view)
        {
            view.ConnectionStatus = Strings.Disconnected;

            var connection = Factory.ResolveSingleton<IXPlaneConnection>();
            connection.Disconnect();
        }

        private string ValidateSettings(XPlaneSettings settings)
        {
            string message = null;

            if(String.IsNullOrEmpty(settings.Host)) {
                message = "Host must be supplied";
            } else if(!IPAddress.TryParse(settings.Host, out var address)) {
                message = "Host is not a valid host";
            } else if(settings.XPlanePort < 1 || settings.XPlanePort > 65534) {
                message = "XPlanePort is out of range";
            } else if(settings.ReplyPort < 1 || settings.ReplyPort > 65534) {
                message = "ReplyPort is out of range";
            }

            return message;
        }
    }
}
