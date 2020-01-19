// Copyright © 2017 onwards, Andrew Whewell
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
using System.Net;
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebSite.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="IAudioServer"/>.
    /// </summary>
    class AudioServer : IAudioServer
    {
        /// <summary>
        /// Reference to singleton shared configuration object.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AudioServer()
        {
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                var context = OwinContext.Create(environment);
                if(!ServeAudio(context)) {
                    await next(environment);
                }
            };
        }

        private bool ServeAudio(OwinContext context)
        {
            var result = String.Equals(context.RequestPathNormalised, "/Audio", StringComparison.OrdinalIgnoreCase);
            if(result) {
                result = context.IsLocalOrLan || _SharedConfiguration.Get().InternetClientSettings.CanPlayAudio;
            }

            if(result) {
                var queryString = context.RequestQueryStringDictionary(caseSensitiveKeys: false);
                switch(queryString["cmd"]?.ToLower()) {
                    case "say":
                        var text = queryString["line"];
                        if(text == null) {
                            result = false;
                        } else {
                            var audio = Factory.Resolve<IAudio>();
                            var audioBytes = audio.SpeechToWavBytes(text);

                            context.ResponseHttpStatusCode = HttpStatusCode.OK;
                            context.ReturnBytes(
                                MimeType.WaveAudio,
                                audioBytes
                            );
                        }
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            return result;
        }
    }
}
