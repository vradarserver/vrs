using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Records statistics against an individual Mode S DF number.
    /// </summary>
    public class ModeSDFStatistics
    {
        /// <summary>
        /// Gets or sets the DF number that the statistics are for.
        /// </summary>
        public DownlinkFormat DF { get; set; }

        /// <summary>
        /// Gets or sets the number of messages received.
        /// </summary>
        public long MessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of messages with bad parity.
        /// </summary>
        public long BadParityPI { get; set; }

        /// <summary>
        /// Creates a copy of the statistics object.
        /// </summary>
        /// <returns></returns>
        public ModeSDFStatistics Clone()
        {
            return new ModeSDFStatistics() {
                DF =                DF,
                BadParityPI =       BadParityPI,
                MessagesReceived =  MessagesReceived,
            };
        }
    }
}
