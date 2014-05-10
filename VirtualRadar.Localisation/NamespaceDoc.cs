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

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// The namespace that holds static classes for localising content and the tables of localised strings.
    /// </summary>
    /// <remarks><para>
    /// .NET has a mechanism for localising strings in forms but it does not lend itself to having
    /// a centralised set of strings for the entire application that are shared amongst all of the libraries. To
    /// make it easier to farm the string table out for translation all of the localised strings were concentrated
    /// into this one library and a static class, <see cref="Localise"/>, was written to perform the localisation
    /// of strings on forms. A public class, <see cref="Localiser"/>, can be used by plugins to perform the same
    /// duties for their translations.
    /// </para><para>
    /// If you want to test your alternate cultures you can start Virtual Radar Server at the command line with
    /// the -culture switch. For example, to force Virtual Radar Server to use the German culture settings:
    /// </para><para>
    /// <b>VirtualRadar.exe -culture:de-DE</b>
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
