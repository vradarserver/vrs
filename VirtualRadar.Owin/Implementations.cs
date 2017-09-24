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
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;

namespace VirtualRadar.Owin
{
    /// <summary>
    /// Registers implementations of interfaces with a class factory.
    /// </summary>
    public static class Implementations
    {
        /// <summary>
        /// Registers implementations of interfaces.
        /// </summary>
        /// <param name="factory"></param>
        public static void Register(IClassFactory factory)
        {
            factory.Register<Interface.Owin.IAccessConfiguration, Configuration.AccessConfiguration>();
            factory.Register<Interface.Owin.IAuthenticationConfiguration, Configuration.AuthenticationConfiguration>();
            factory.Register<Interface.Owin.IFileSystemServerConfiguration, Configuration.FileSystemServerConfiguration>();
            factory.Register<Interface.Owin.IImageServerConfiguration, Configuration.ImageServerConfiguration>();
            factory.Register<Interface.Owin.IRedirectionConfiguration, Configuration.RedirectionConfiguration>();
            factory.Register<Interface.Owin.IWebAppConfiguration, Configuration.WebAppConfiguration>();

            factory.Register<Interface.Owin.IAccessFilter, Middleware.AccessFilter>();
            factory.Register<Interface.Owin.IAudioServer, Middleware.AudioServer>();
            factory.Register<Interface.Owin.IBasicAuthenticationFilter, Middleware.BasicAuthenticationFilter>();
            factory.Register<Interface.Owin.ICorsHandler, Middleware.CorsHandler>();
            factory.Register<Interface.Owin.IFileSystemServer, Middleware.FileSystemServer>();
            factory.Register<Interface.Owin.IImageServer, Middleware.ImageServer>();
            factory.Register<Interface.Owin.IRedirectionFilter, Middleware.RedirectionFilter>();
            factory.Register<Interface.Owin.IResponseStreamWrapper, Middleware.ResponseStreamWrapper>();

            factory.Register<Interface.Owin.IJavascriptManipulator, StreamManipulator.JavascriptManipulator>();

            factory.Register<Interface.Owin.ILoopbackHost, LoopbackHost>();
            factory.Register<Interface.Owin.IStandardPipeline, StandardPipeline>();
        }
    }
}
