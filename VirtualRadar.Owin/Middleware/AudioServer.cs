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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Owin.Middleware
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
            _SharedConfiguration = Factory.Singleton.ResolveSingleton<ISharedConfiguration>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var context = PipelineContext.GetOrCreate(environment);
                if(!ServeAudio(context)) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        private bool ServeAudio(PipelineContext context)
        {
            var result = String.Equals(context.Request.PathNormalised.Value, "/Audio", StringComparison.OrdinalIgnoreCase);
            if(result) {
                result = context.Request.IsLocalOrLan || _SharedConfiguration.Get().InternetClientSettings.CanPlayAudio;
            }

            if(result) {
                switch(context.Request.Query["cmd"]?.ToLower()) {
                    case "say":
                        var text = context.Request.Query["line"];
                        if(text == null) {
                            result = false;
                        } else {
                            var audio = Factory.Singleton.Resolve<IAudio>();
                            var audioBytes = audio.SpeechToWavBytes(text);

                            var response = context.Response;
                            response.ContentType = MimeType.WaveAudio;
                            response.ContentLength = audioBytes.Length;
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.Body.Write(audioBytes, 0, audioBytes.Length);
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
