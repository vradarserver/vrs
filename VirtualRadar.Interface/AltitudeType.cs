using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The different types of altitude and vertical rate decoded by VRS.
    /// </summary>
    public enum AltitudeType
    {
        /// <summary>
        /// The value is pressure alitude against a settings of 1013.25mb. If the altitude type
        /// is unknown (e.g. the aircraft data is coming off a cooked feed) then it defaults to this.
        /// </summary>
        Barometric = 0,

        /// <summary>
        /// The value is the height above the elipsoid.
        /// </summary>
        Geometric = 1,
    }
}
