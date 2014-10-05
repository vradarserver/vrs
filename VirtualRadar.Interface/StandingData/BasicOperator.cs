using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The operators held by <see cref="IBasicAircraftLookupDatabase"/>.
    /// </summary>
    public class BasicOperator
    {
        public int OperatorID { get; set; }

        public string Icao { get; set; }

        public string Name { get; set; }
    }
}
