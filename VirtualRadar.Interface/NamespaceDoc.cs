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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The root namespace for the application interfaces.
    /// </summary>
    /// <remarks><para>
    /// The interfaces, enumerations and classes in this namespace are held in <b>VirtualRadar.Interface.dll</b>. Virtual
    /// Radar Server is loosely coupled (see the remarks against the InterfaceFactory namespace) and almost all of the
    /// classes used by the program are private to the library that they live in. They all implement interfaces and the
    /// program uses a singleton class factory in InterfaceFactory to resolve references to interfaces when the need
    /// arises.
    /// </para><para>
    /// These interfaces are all declared in this library, VirtualRadar.Interface.dll. The library does not contain any
    /// of the implementations. It has almost no references to other projects within Virtual Radar Server, aside from
    /// InterfaceFactory. Along with the interfaces for the program it also contains the enumerations, custom EventArgs,
    /// Exception and Attribute classes that the interfaces refer to. It also contains one or two public helper classes,
    /// usually static classes, that the implementations of the interfaces may find useful.
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
