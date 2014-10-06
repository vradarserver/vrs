using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The aircraft record held by <see cref="IBasicAircraftLookupDatabase"/>.
    /// </summary>
    public class BasicAircraft
    {
        public int AircraftID { get; set; }

        public string Icao { get; set; }

        public string Registration { get; set; }

        public virtual int? BasicModelID { get; set; }

        public virtual int? BasicOperatorID { get; set; }

        public DateTime BaseStationUpdated { get; set; }
    }
}
