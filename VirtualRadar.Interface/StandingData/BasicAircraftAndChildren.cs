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

namespace VirtualRadar.Interface.StandingData
{
    // Need to disable 0659, it's telling us that we're overriding Equals but not GetHashCode. This object
    // is not immutable, it cannot be used as a key. No need to override GetHashCode.
    #pragma warning disable 0659

    /// <summary>
    /// A subclass of <see cref="BasicAircraft"/> that holds references to the child records.
    /// </summary>
    public class BasicAircraftAndChildren : BasicAircraft
    {
        /// <summary>
        /// Gets or sets the associated <see cref="BasicModel"/> record.
        /// </summary>
        public BasicModel Model { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="BasicOperator"/> record.
        /// </summary>
        public BasicOperator Operator { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override int? BasicModelID
        {
            get { return Model == null ? (int?)null : Model.ModelID; }
            set { if(Model != null && value != null) Model.ModelID = value.Value; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override int? BasicOperatorID
        {
            get { return Operator == null ? (int?)null : Operator.OperatorID; }
            set { if(Operator != null && value != null) Operator.OperatorID = value.Value; }
        }

        /// <summary>
        /// Gets the model ICAO.
        /// </summary>
        public string ModelIcao { get { return Model == null ? null : Model.Icao; } }

        /// <summary>
        /// Gets the model name.
        /// </summary>
        public string ModelName { get { return Model == null ? null : Model.Name; } }

        /// <summary>
        /// Gets the operator ICAO.
        /// </summary>
        public string OperatorIcao { get { return Operator == null ? null : Operator.Icao; } }

        /// <summary>
        /// Gets the operator name.
        /// </summary>
        public string OperatorName { get { return Operator == null ? null : Operator.Name; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                var other = obj as BasicAircraftAndChildren;
                result = other != null &&
                    other.AircraftID ==         AircraftID &&
                    other.BaseStationUpdated == BaseStationUpdated &&
                    other.BasicModelID ==       BasicModelID &&
                    other.BasicOperatorID ==    BasicOperatorID &&
                    other.Icao ==               Icao &&
                    other.ModelIcao ==          ModelIcao &&
                    other.ModelName ==          ModelName &&
                    other.OperatorIcao ==       OperatorIcao &&
                    other.OperatorName ==       OperatorName &&
                    other.Registration ==       Registration;
            }

            return result;
        }
    }
}
