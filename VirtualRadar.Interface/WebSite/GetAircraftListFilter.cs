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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// Describes a single filter in the v3 AircraftList.json call parameters.
    /// </summary>
    public class GetAircraftListFilter
    {
        /// <summary>
        /// Gets or sets the field to apply the filter to.
        /// </summary>
        public GetAircraftListFilterField Field { get; set; }

        /// <summary>
        /// Gets or sets the filter condition.
        /// </summary>
        public FilterCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the condition is reversed.
        /// </summary>
        public bool Not { get; set; }

        /// <summary>
        /// Gets or sets the string value of the filter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the bool value of the filter.
        /// </summary>
        public bool? Is { get; set; }

        /// <summary>
        /// Gets or sets the lower end of a range.
        /// </summary>
        public double? From { get; set; }

        /// <summary>
        /// Gets or sets an upper end to a range.
        /// </summary>
        public double? To { get; set; }

        /// <summary>
        /// Gets the northern edge of a bound.
        /// </summary>
        public double? North { get; set; }

        /// <summary>
        /// Gets the southern edge of a bound.
        /// </summary>
        public double? South { get; set; }

        /// <summary>
        /// Gets the western edge of a bound.
        /// </summary>
        public double? West { get; set; }

        /// <summary>
        /// Gets an eastern edge of a bound.
        /// </summary>
        public double? East { get; set; }
    }
}
