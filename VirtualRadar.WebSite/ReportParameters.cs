// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The V2 report page parameters passed by query string.
    /// </summary>
    class ReportParameters : SearchBaseStationCriteria
    {
        /// <summary>
        /// Gets or sets the type of report that is requesting data rows.
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets or sets the first row in the set to return or -1 for the first row.
        /// </summary>
        public int FromRow { get; set; }

        /// <summary>
        /// Gets or sets the last row in the set to return or -1 for the last row.
        /// </summary>
        public int ToRow { get; set; }

        /// <summary>
        /// Gets or sets the first field to sort on or null for no sorting.
        /// </summary>
        public string SortField1 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the first field should be sorted in ascending order.
        /// </summary>
        public bool SortAscending1 { get; set; }

        /// <summary>
        /// Gets or sets the second field to sort on or null for no sorting.
        /// </summary>
        public string SortField2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the second field should be sorted in ascending order.
        /// </summary>
        public bool SortAscending2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the report only include military aircraft. This is not information
        /// that is stored in the database so it can increase the time required to gather the requested rows.
        /// </summary>
        public FilterBool IsMilitary { get; set; }

        /// <summary>
        /// Gets or sets the wake turbulence category that the report is interested in. This is not information
        /// that is stored in the database so it can increase the time required to gather the requested rows.
        /// </summary>
        public FilterEnum<WakeTurbulenceCategory> WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets or sets the species of aircraft that the report is interested in. This is not information
        /// that is stored in the database so it can increase the time required to gather the requested rows.
        /// </summary>
        public FilterEnum<Species> Species { get; set; }
    }
}
