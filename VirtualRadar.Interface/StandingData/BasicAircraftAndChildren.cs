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
