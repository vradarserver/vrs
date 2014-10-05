using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The model record held by <see cref="IBasicAircraftLookupDatabase"/>.
    /// </summary>
    public class BasicModel
    {
        public int ModelID { get; set; }

        public string Icao { get; set; }

        public string Name { get; set; }
    }
}
