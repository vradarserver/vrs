using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Records the state of an aircraft.
    /// </summary>
    public class AircraftState
    {
        /// <summary>
        /// Gets or sets the unique ID of the state record in the database.
        /// </summary>
        public long AircraftStateID { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the aircraft list that this state belongs to.
        /// </summary>
        public long AircraftListID { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the flight, a collection of <see cref="AircraftState"/>
        /// records that describe the full flight of a single aircraft in the current session.
        /// </summary>
        public long FlightID { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the snapshot of the aircraft details.
        /// </summary>
        /// <remarks>
        /// The details for an aircraft can change over the duration of the flight, when this happens
        /// the <see cref="FlightID"/> will remain constant but the <see cref="AircraftSnapshotID"/> will
        /// change.
        /// </remarks>
        public long AircraftSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the nullable fields describe the full state of the
        /// aircraft (false) or whether they record changes from the most recently recorded state of
        /// the aircraft (true).
        /// </summary>
        /// <remarks><para>
        /// If this is not a delta record then nullable fields set to null indicate that the value has never
        /// been supplied for the aircraft.
        /// </para><para>
        /// If this is a delta record then non-null nullable fields indicate changes to previously established
        /// values. All other nullable fields need to be determined by finding the most record non-delta record
        /// and then reading every delta record between then and now, applying non-null values to the state to
        /// build up the aircraft state that can be changed by this delta.
        /// </para>
        /// </remarks>
        public bool IsDelta { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the receiver snapshot that describes the receiver that VRS is using
        /// to track this aircraft.
        /// </summary>
        public long? ReceiverSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the receiver snapshot that describes the receiver that is supplying
        /// positions that take precedence over positions (or the lack thereof) seen by <see cref="ReceiverSnapshotID"/>.
        /// This is usually an MLAT receiver.
        /// </summary>
        public long? PositionReceiverSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the callsign was read from a reliable source (false) or from a
        /// message that might not have been transmitting a callsign (true).
        /// </summary>
        public bool? IsCallsignSuspect { get; set; }

        /// <summary>
        /// Gets or sets the latitude component of the aircraft's position.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude component of the aircraft's position.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Latitude"/> and <see cref="Longitude"/> were calculated
        /// via MLAT rather than transmitted by the aircraft.
        /// </summary>
        public bool? IsMlat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft state contains values transmitted via TISB instead of ADSB.
        /// </summary>
        public bool? IsTisb { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's altitude in feet.
        /// </summary>
        public int? AltitudeFeet { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the <see cref="AltitudeTypeSnapshot"/> record that holds the <see cref="AltitudeType"/>
        /// enum value that determines whether the altitude is barometric or radar.
        /// </summary>
        public long? AltitudeTypeSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's autopilot / FMS target altitude in feet.
        /// </summary>
        public int? TargetAltitudeFeet { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ground air pressure setting in inches of mercury.
        /// </summary>
        public float? AirPressureInHg { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ground speed in knots.
        /// </summary>
        public int? GroundSpeedKnots { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the <see cref="SpeedTypeSnapshot"/> record that holds the <see cref="SpeedType"/> enum
        /// value that indicates whether <see cref="GroundSpeedKnots"/> the IAS or TAS and whether it is in reverse.
        /// </summary>
        public long? SpeedTypeSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the track angle in 360 degrees from north.
        /// </summary>
        public int? TrackDegrees { get; set; }

        /// <summary>
        /// Gets or sets the track angle in 360 degrees from north that the aircraft's autopilot or FMS is targetting.
        /// </summary>
        public int? TargetTrackDegrees { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that <see cref="TrackDegrees"/> is the aircraft heading and not its ground track.
        /// </summary>
        public bool? TrackIsHeading { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's vertical speed in feet per minute.
        /// </summary>
        public int? VerticalRateFeetMin { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the <see cref="AltitudeTypeSnapshotID"/> record that indicates whether the vertical speed
        /// is being supplied by an instrument that is measuring changes in air pressure or actual distance from the ground.
        /// enum
        /// </summary>
        public long? VerticalRateAltitudeTypeSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the four digit octal squawk code as a decimal number (e.g. squawk octal 1234 is expressed as the decimal
        /// value 1234).
        /// </summary>
        public int? Squawk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft is transmitting the ident signal.
        /// </summary>
        public bool? IdentActive { get; set; }

        /// <summary>
        /// Gets or sets the message's signal level. The signal level is meaningful only to the receiver that calculated it, signal
        /// levels cannot be compared between different receiver models.
        /// </summary>
        public int? SignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the <see cref="RouteSnapshot"/> that describes the full route that the aircraft is flying. This
        /// can change over the course of the flight, even if the callsign does not change (e.g. it can change after a successful route
        /// lookup).
        /// </summary>
        public long? RouteID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the flight is a positioning flight or ferry flight and does not follow a scheduled route.
        /// </summary>
        public bool? IsPositioningFlight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the flight is an ad-hoc charter flight and does not follow a scheduled route.
        /// </summary>
        public bool? IsCharterFlight { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of a <see cref="TransponderTypeSnapshot"/> record whose enum indicates which version of
        /// ADSB is supported by the aircraft's transponder.
        /// </summary>
        public long? TransponderTypeSnapshotID { get; set; }
    }
}
