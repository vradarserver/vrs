// Copyright © 2015 onwards, Andrew Whewell
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The event args used by <see cref="IAircraftOnlineLookup"/>.
    /// </summary>
    public class AircraftOnlineLookupEventArgs : EventArgs
    {
        private List<AircraftOnlineLookupDetail> _AircraftDetails = new List<AircraftOnlineLookupDetail>();
        private ReadOnlyCollection<AircraftOnlineLookupDetail> _ReadOnlyAircraftDetails;
        /// <summary>
        /// Gets the details for the aircraft that could be found.
        /// </summary>
        public ReadOnlyCollection<AircraftOnlineLookupDetail> AircraftDetails
        {
            get {
                if(_ReadOnlyAircraftDetails == null) _ReadOnlyAircraftDetails = new ReadOnlyCollection<AircraftOnlineLookupDetail>(_AircraftDetails);
                return _ReadOnlyAircraftDetails;
            }
        }

        private List<string> _MissingIcaos = new List<string>();
        private ReadOnlyCollection<string> _ReadOnlyMissingIcaos;
        /// <summary>
        /// Gets the list of ICAOs for which the online source had no details.
        /// </summary>
        public ReadOnlyCollection<string> MissingIcaos
        {
            get {
                if(_ReadOnlyMissingIcaos == null) _ReadOnlyMissingIcaos = new ReadOnlyCollection<string>(_MissingIcaos);
                return _ReadOnlyMissingIcaos;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="fetchedAircraft"></param>
        /// <param name="missingIcaos"></param>
        public AircraftOnlineLookupEventArgs(IEnumerable<AircraftOnlineLookupDetail> fetchedAircraft, IEnumerable<string> missingIcaos)
        {
            _AircraftDetails.AddRange(fetchedAircraft);
            _MissingIcaos.AddRange(missingIcaos);
        }
    }
}
