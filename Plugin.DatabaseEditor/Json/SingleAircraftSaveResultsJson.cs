using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Plugin.DatabaseEditor.Json
{
    /// <summary>
    /// The JSON returned by a save on a single aircraft.
    /// </summary>
    class SingleAircraftSaveResultsJson : ResponseJson
    {
        /// <summary>
        /// Gets or sets the aircraft saved.
        /// </summary>
        public BaseStationAircraft Aircraft { get; set; }
    }
}
