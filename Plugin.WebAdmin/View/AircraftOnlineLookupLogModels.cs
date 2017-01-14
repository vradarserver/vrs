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
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog
{
    public class ViewModel
    {
        public LogEntry[] LogEntries { get; private set; }

        public ViewModel(IEnumerable<AircraftOnlineLookupLogEntry> logEntries)
        {
            LogEntries = logEntries.Select(r => new LogEntry(r)).ToArray();
        }
    }

    public class LogEntry
    {
        public string Time { get; private set; }

        public string Icao { get; private set; }

        public string Registration { get; private set; }

        public string Country { get; private set; }

        public string Manufacturer { get; private set; }

        public string Model { get; private set; }

        public string ModelIcao { get; private set; }

        public string Operator { get; private set; }

        public string OperatorIcao { get; private set; }

        public string Serial { get; private set; }

        public int? YearBuilt { get; private set; }

        public LogEntry(AircraftOnlineLookupLogEntry entry)
        {
            Time = String.Format("{0:HH:mm:ss}", entry.ResponseUtc.ToLocalTime());
            Icao = entry.Icao;

            var detail = entry.Detail;
            if(detail != null) {
                Registration =  detail.Registration;
                Country =       detail.Country;
                Manufacturer =  detail.Manufacturer;
                Model =         detail.Model;
                ModelIcao =     detail.ModelIcao;
                Operator =      detail.Operator;
                OperatorIcao =  detail.OperatorIcao;
                Serial =        detail.Serial;
                YearBuilt =     detail.YearBuilt;
            }
        }
    }
}
