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

namespace Test.VirtualRadar.WebSite.TestHelpers
{
    /// <summary>
    /// Copy of the old V2 report JSON filter.
    /// </summary>
    class StringFilter : Filter
    {
        public string Value { get; set; }
        public FilterCondition Condition { get; set; }

        public StringFilter(string value, FilterCondition condition, bool reversed) : base(reversed)
        {
            Value = value;
            Condition = condition;
        }

        public override void AddQueryValues(string filterName, Dictionary<string, string> queryValues)
        {
            char conditionCharacter;
            switch(Condition) {
                case FilterCondition.Between:       throw new InvalidOperationException("String filters cannot have the 'Between' condition");
                case FilterCondition.Contains:      conditionCharacter = 'C'; break;
                case FilterCondition.EndsWith:      conditionCharacter = 'E'; break;
                case FilterCondition.Equals:        conditionCharacter = 'Q'; break;
                case FilterCondition.StartsWith:    conditionCharacter = 'S'; break;
                default:                            throw new NotImplementedException();
            }

            queryValues.Add(FilterName(filterName, conditionCharacter), Value);
        }

        public override Type GetPropertyType()
        {
            return typeof(string);
        }
    }
}
