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
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Interface.BaseStation
{
    /// <summary>
    /// An aircraft list that uses a feed of <see cref="BaseStationMessage"/>s as its source of information about aircraft.
    /// </summary>
    public interface IBaseStationAircraftList : IAircraftList
    {
        /// <summary>
        /// Gets or sets the listener that will pick up messages from a source of data and translate them into <see cref="BaseStationMessage"/>s for us.
        /// </summary>
        IListener Listener { get; set; }

        /// <summary>
        /// Gets or sets the object that will look up information about aircraft from the standing data files.
        /// </summary>
        IStandingDataManager StandingDataManager { get; set; }

        /// <summary>
        /// Gets or sets the object that's keeping polar plots for us. Can be null if the listener that's feeding this list doesn't support
        /// polar plots.
        /// </summary>
        IPolarPlotter PolarPlotter { get; set; }
    }
}
