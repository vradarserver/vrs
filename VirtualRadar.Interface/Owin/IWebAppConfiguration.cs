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
using Owin;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton that handles the configuration of the OWIN pipeline for the application.
    /// </summary>
    public interface IWebAppConfiguration : ISingleton<IWebAppConfiguration>
    {
        /// <summary>
        /// Records a method that will be called with the <see cref="IAppBuilder"/> passed to <see cref="Configure"/>.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="priority">A value indicating the order in which callbacks are called, lowest first.</param>
        /// <returns>A handle that can be passed to <see cref="RemoveCallback"/>.</returns>
        IWebAppConfigurationCallbackHandle AddCallback(Action<IAppBuilder> callback, MiddlewarePriority priority);

        /// <summary>
        /// Removes the callback associated with the handle passed across.
        /// </summary>
        /// <param name="callbackHandle"></param>
        void RemoveCallback(IWebAppConfigurationCallbackHandle callbackHandle);

        /// <summary>
        /// Configures a new instance of an OWIN web app by calling each registered callback in ascending order of priority.
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <remarks>
        /// Callbacks that have been added with the same priority value are called in random order.
        /// </remarks>
        void Configure(IAppBuilder appBuilder);
    }
}
