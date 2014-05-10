// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.SQLite
{
    /// <summary>
    /// The namespace for all of the interfaces and objects associated with wrapping different SQLite providers.
    /// </summary>
    /// <remarks><para>
    /// Up until version 1.1.2 of VRS the application bound directly to Robert Simpson's SQLite ADO.Net
    /// Provider. When the Mono version came along it used a Mono build from source of the provider. However
    /// under some Linux systems this was not very stable, so the new approach is to use the native Mono SQLite
    /// library when running under Mono.
    /// </para><para>
    /// This wrapper only wraps the parts of SQLite used by VRS.
    /// </para>
    /// <para>
    /// <img src="../ClassDiagrams/_VirtualRadarInterfaceSQLite.jpg" alt="" title="VirtualRadar.Interface.SQLite class diagram"/>
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
