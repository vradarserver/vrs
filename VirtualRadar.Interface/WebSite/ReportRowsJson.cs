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
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The base class for all top-level report JSON objects.
    /// </summary>
    [DataContract]
    public abstract class ReportRowsJson
    {
        /// <summary>
        /// Gets or sets the total number of rows that match the report criteria.
        /// </summary>
        [DataMember(Name="countRows", IsRequired=true)]
        public int? CountRows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the column that the data has been grouped by.
        /// </summary>
        [DataMember(Name="groupBy")]
        public string GroupBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how long the report took to process.
        /// </summary>
        [DataMember(Name="processingTime")]
        public string ProcessingTime { get; set; }

        /// <summary>
        /// Gets or sets the content of any errors or exceptions that were thrown during the processing of the report.
        /// </summary>
        [DataMember(Name="errorText", EmitDefaultValue=false)]
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that silhouettes can be shown on the report.
        /// </summary>
        [DataMember(Name="silhouettesAvailable")]
        public bool SilhouettesAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that operator flags can be shown on the report.
        /// </summary>
        [DataMember(Name="operatorFlagsAvailable")]
        public bool OperatorFlagsAvailable { get; set; }
    }
}
