using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Describes an instance of an aircraft list that has been recorded in state history.
    /// </summary>
    public class AircraftList
    {
        /// <summary>
        /// Gets or sets the unique ID for the aircraft list instance.
        /// </summary>
        public long AircraftListID   { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the sessiont that the aircraft list was recorded for.
        /// </summary>
        public long VrsSessionID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Aircraft"/> records hanging from
        /// the aircraft list are fully complete (true) or deltas from the immediately previous
        /// aircraft list for the same receiver (false).
        /// </summary>
        public bool IsKeyList { get; set; }

        /// <summary>
        /// Gets or sets the ID of the receiver that the aircraft list was taking messages from.
        /// </summary>
        public long ReceiverSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the date and time at UTC that the list was created.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time at UTC that the list was last updated.
        /// </summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
