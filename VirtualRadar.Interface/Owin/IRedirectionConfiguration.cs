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

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that holds the configuration for the redirection middleware.
    /// </summary>
    public interface IRedirectionConfiguration : ISingleton<IRedirectionConfiguration>
    {
        /// <summary>
        /// Registers a redirection from one path to another.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="redirectToPathFromRoot"></param>
        /// <param name="redirectionContext"></param>
        /// <remarks>
        /// If a redirection already exists for the parameters passed across then it is replaced with the new redirection.
        /// </remarks>
        void AddRedirection(string pathFromRoot, string redirectToPathFromRoot, RedirectionContext redirectionContext);

        /// <summary>
        /// Deregisters a previously registered redirection.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="redirectionContext"></param>
        void RemoveRedirection(string pathFromRoot, RedirectionContext redirectionContext);

        /// <summary>
        /// Returns the path from root to redirect to given the parameters passed across or null if no redirection is active
        /// for the path supplied.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        string RedirectToPathFromRoot(string pathFromRoot, RedirectionRequestContext requestContext);
    }
}
