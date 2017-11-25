// Copyright © 2016 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// Marks a method on a view returned by a <see cref="WebAdminView"/> as being callable from
    /// the web page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    public class WebAdminMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating that the method should be deferred.
        /// </summary>
        /// <remarks><para>
        /// Some methods may want to do something that can cause the web server to close all connections and restart
        /// (this applies in particular to anything to do with UPnP). This can cause havoc with the WebAdmin site as
        /// it will close the connection while the browser is waiting for a reply, which at the very least will cause
        /// the browser to re-send the request.
        /// </para><para>
        /// Setting this property tells the web admin plugin that it should defer the execution of the method until
        /// after a response has been returned for the request. The response will carry a job identifier back to the
        /// browser and the browser is then expected to poll the server until the job has been run. Pages that use the
        /// ajax() method in ViewId do not need to do anything, the ajax() method will handle the job polling for them.
        /// </para>
        /// </remarks>
        public bool DeferExecution { get; set; }

        /// <summary>
        /// Gets or sets the name of the string parameter that should be filled with the current user's name.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebAdminMethodAttribute() : base()
        {
        }
    }
}
