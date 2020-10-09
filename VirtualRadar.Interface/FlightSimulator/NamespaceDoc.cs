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

namespace VirtualRadar.Interface.FlightSimulatorX
{
    /// <summary>
    /// The namespace for all of the interfaces and objects associated with talking to Microsoft's Flight Simulator X.
    /// </summary>
    /// <remarks><para>
    /// The main class here is <see cref="IFlightSimulatorX"/>. <see cref="ISimConnectWrapper"/> is also of interest as implementations
    /// of <see cref="IFlightSimulatorX"/> may use it to talk to FSX. You will need to have read the SimConnect documentation before you
    /// can make use of <see cref="ISimConnectWrapper"/>. Only the parts of SimConnect that were of interest to the application were wrapped.
    /// </para><para>
    /// <img src="../ClassDiagrams/_VirtualRadarInterfaceFlightSimulatorX.jpg" alt="" title="VirtualRadar.Interface.FlightSimulatorX class diagram"/>
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
