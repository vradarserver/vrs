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
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Responds to requests for audio.
    /// </summary>
    class AudioPage : Page
    {
        /// <summary>
        /// True if browsers connecting from the Internet are allowed to play audio.
        /// </summary>
        private bool _InternetClientCanRequestAudio;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public AudioPage(WebSite webSite) : base(webSite)
        {
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            base.DoLoadConfiguration(configuration);
            _InternetClientCanRequestAudio = configuration.InternetClientSettings.CanPlayAudio;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            bool result = false;

            if(!args.IsInternetRequest || _InternetClientCanRequestAudio) {
                if(args.PathAndFile.Equals("/Audio", StringComparison.OrdinalIgnoreCase)) {
                    string command = (string)args.QueryString["cmd"];
                    if(command != null && command.Equals("say", StringComparison.OrdinalIgnoreCase)) {
                        var text = args.QueryString["line"];
                        if(text != null) {
                            result = true;
                            IAudio audio = Factory.Resolve<IAudio>();
                            Responder.SendAudio(args.Request, args.Response, audio.SpeechToWavBytes(text), MimeType.WaveAudio);
                            args.Classification = ContentClassification.Audio;
                        }
                    }
                }
            }

            return result;
        }
    }
}
