using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An enumeration of the different types of transponder that VRS can decode messages from.
    /// </summary>
    public enum TransponderType
    {
        /// <summary>
        /// The transponder type is not known and no attempt has been made to guess it.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The transponder is sending Mode-S messages with no ADSB content.
        /// </summary>
        ModeS = 1,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB content, but the type of ADSB
        /// content cannot be determined. Assume that it's ADSB-1.
        /// </summary>
        Adsb = 2,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB-1 content.
        /// </summary>
        Adsb1 = 3,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB-2 content.
        /// </summary>
        Adsb2 = 4,
    }
}
