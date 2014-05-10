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
using System.Runtime.CompilerServices;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The namespace for the interfaces and objects that deal with the application's web server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The main interface of interest here is <see cref="IWebServer"/> which describes an object that listens for
    /// requests from web browsers and raises an event when it sees them.
    /// </para><para>
    /// The web server does not contain any content - other objects hook the events on <see cref="IWebServer"/> and
    /// set the <see cref="RequestReceivedEventArgs.Handled"/>, <see cref="RequestReceivedEventArgs.Classification"/>
    /// and <see cref="RequestReceivedEventArgs.Response"/> properties as appropriate. By default this duty is performed
    /// by the <see cref="WebSite.IWebSite"/> object.
    /// </para><para>
    /// <img src="../ClassDiagrams/_VirtualRadarInterfaceWebServer.jpg" alt="" title="VirtualRadar.Interface.WebServer class diagram"/>
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
