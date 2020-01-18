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
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Owin.StreamManipulator
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The default implementation of <see cref="IJavascriptManipulator"/>.
    /// </summary>
    class JavascriptManipulator : IJavascriptManipulator
    {
        /// <summary>
        /// The minifier that will minify Javascript responses for us.
        /// </summary>
        private IMinifier _Minifier;

        /// <summary>
        /// Singleton configuration object.
        /// </summary>
        private IJavascriptManipulatorConfiguration _Config;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                _Minifier = Factory.Resolve<IMinifier>();
                _Config = Factory.ResolveSingleton<IJavascriptManipulatorConfiguration>();

                ManipulateResponseStream(environment);

                await next(environment);
            };
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        private void ManipulateResponseStream(IDictionary<string, object> environment)
        {
            var context = OwinContext.Create(environment);
            var isJavaScriptContent = context.ResponseHeadersDictionary.ContentTypeValue.MediaTypeParsed == MediaType.JavaScript;

            if(isJavaScriptContent) {
                var stream = context.ResponseBody;
                stream.Position = 0;
                var textContent = TextContent.Load(stream, leaveOpen: true);

                foreach(var manipulator in _Config.GetTextResponseManipulators()) {
                    manipulator.ManipulateTextResponse(environment, textContent);
                }

                var suppressMinification = ((bool?)context.Environment[VrsEnvironmentKey.SuppressJavascriptMinification]) == true;
                var newContent = !suppressMinification ? _Minifier.MinifyJavaScript(textContent.Content) : textContent.Content;
                if(newContent.Length < textContent.Content.Length) {
                    textContent.Content = newContent;
                }

                if(textContent.IsDirty) {
                    stream.Position = 0;
                    stream.SetLength(0);
                    var bytes = textContent.GetBytes(includePreamble: true);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
