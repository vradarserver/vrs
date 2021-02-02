using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Holds a snapshot of an aircraft's details.
    /// </summary>
    public class AircraftSnapshot : SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier of the snapshot record.
        /// </summary>
        public long AircraftSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's six hex digit ICAO Mode-S ID.
        /// </summary>
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the ID of the aircraft's model snapshot.
        /// </summary>
        public long? ModelSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's serial number.
        /// </summary>
        public string ConstructionNumber { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was built. This is a string because the underlying
        /// data sources record the year built as a string.
        /// </summary>
        public string YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the aircraft's operator snapshot.
        /// </summary>
        public long? OperatorSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the aircraft's country of registration snapshot.
        /// </summary>
        public long? CountrySnapshotID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft's ICAO was allocated from a block
        /// reserved by the country's registrar for military use.
        /// </summary>
        public bool? IsMilitary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is considered interesting.
        /// </summary>
        public bool? IsInteresting { get; set; }

        /// <summary>
        /// Gets or sets notes entered by the user against the aircraft.
        /// </summary>
        public string UserNotes { get; set; }

        /// <summary>
        /// Gets or sets a set of tags entered by the user against the aircraft.
        /// </summary>
        public string UserTag { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override byte[] FingerprintProperties()
        {
            return TakeFingerprint(
                Icao,
                Registration,
                ModelSnapshotID,
                ConstructionNumber,
                YearBuilt,
                OperatorSnapshotID,
                CountrySnapshotID,
                IsMilitary,
                IsInteresting,
                UserNotes,
                UserTag
            );
        }

        /// <summary>
        /// Creates a fingerprint from component parts.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="registration"></param>
        /// <param name="modelSnapshotID"></param>
        /// <param name="constructionNumber"></param>
        /// <param name="yearBuilt"></param>
        /// <param name="operatorSnapshotID"></param>
        /// <param name="countrySnapshotID"></param>
        /// <param name="isMilitary"></param>
        /// <param name="isInteresting"></param>
        /// <param name="userNotes"></param>
        /// <param name="userTag"></param>
        /// <returns></returns>
        public static byte[] TakeFingerprint(
            string icao,
            string registration,
            long? modelSnapshotID,
            string constructionNumber,
            string yearBuilt,
            long? operatorSnapshotID,
            long? countrySnapshotID,
            bool? isMilitary,
            bool? isInteresting,
            string userNotes,
            string userTag
        ) => Sha1Fingerprint.CreateFingerprintFromObjects(
            icao,
            registration,
            modelSnapshotID,
            constructionNumber,
            yearBuilt,
            operatorSnapshotID,
            countrySnapshotID,
            isMilitary,
            isInteresting,
            userNotes,
            userTag
        );
    }
}
