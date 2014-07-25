// Copyright © 2014 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface that <see cref="IReceiverConfigurationWizard"/> views
    /// expose to carry the answers supplied by the user.
    /// </summary>
    public interface IReceiverConfigurationWizardAnswers
    {
        /// <summary>
        /// Gets or sets the broad class of receiver - SDR vs. Dedicated.
        /// </summary>
        ReceiverClass ReceiverClass { get; set; }

        /// <summary>
        /// Gets or sets the SDR decoder in use. Ignore if receiver class is not SDR.
        /// </summary>
        SdrDecoder SdrDecoder { get; set; }

        /// <summary>
        /// Gets or sets the dedicated receiver in use. Ignore if receiver class is not Dedicated.
        /// </summary>
        DedicatedReceiver DedicatedReceiver { get; set; }

        /// <summary>
        /// Gets or sets the connection type used by the receiver. Ignore if the receiver
        /// class is SDR, or if the dedicated receiver is anything except Beast.
        /// </summary>
        ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user is using BaseStation. Ignore if the
        /// receiver is not Kinetics.
        /// </summary>
        bool IsUsingBaseStation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the feed is coming from a program and that
        /// program is on the same machine as VRS.
        /// </summary>
        bool IsLoopback { get; set; }

        /// <summary>
        /// Gets or sets the address to use if the receiver is reached over the network and
        /// it cannot be reached over the loopback interface.
        /// </summary>
        string NetworkAddress { get; set; }
    }
}
