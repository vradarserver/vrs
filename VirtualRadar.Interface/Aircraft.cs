// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes everything that is known about an aircraft that is being tracked.
    /// </summary>
    public class Aircraft
    {
        /// <summary>
        /// Number of ticks in a second.
        /// </summary>
        private const long _TicksPerSecond = 10000000L;

        /// <summary>
        /// The threshold distance in KM between two points that can cause the trails to reset. If the distance between
        /// two consecutive points for an aircraft is higher than this and the time threshold is passed (see <see cref="_ResetCoordinatesTime"/>)
        /// then the trail is reset.
        /// </summary>
        /// <remarks>This is about 10 nautical miles. Any 'jump' between two positions that is less than this distance will never
        /// trigger a trail reset, even if it is wrong. This figure may need tuning. It needs to be quite large though to counteract
        /// the 'shrinking time' effect arising from not knowing the real time a message was transmitted by an aircraft (BaseStation's
        /// timestamp is inaccurate).</remarks>
        private const double _ResetCoordinatesDistance = 18.0;

        /// <summary>
        /// See <see cref="_ResetCoordinatesDistance"/>. The period is in 100-nanosecond units. If the distance between two points exceeds
        /// <see cref="_ResetCoordinatesDistance"/> then the factor by which it is exceeded is multiplied by this threshold. This is the
        /// time that the aircraft is given to cover that distance - if it manages to cover it quicker then the trail is reset. If
        /// it took longer than this then the aircraft just dropped out of range for a bit and the trail is preserved.
        /// </summary>
        /// <remarks>Originally this was set at the speed of an SR-71, ~ mach 3, but in tests with a receiver that was transmitting
        /// a lot of bad positions some A319s were seen travelling at mach 2 :) So it's had to be tuned down quite a bit. The speed
        /// of sound at sea level is 0.34 km/sec.</remarks>
        private const double _ResetCoordinatesTime = (18.0 / 0.4) * 10000000.0;  // anything that covers 18km in under 45 seconds will reset the trail

        /// <summary>
        /// The lock object used to synchronise writes to the object.
        /// </summary>
        private readonly object _SyncLock = new();

        /// <summary>
        /// Gets or sets the unique identifier of the aircraft.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when the data was last changed.
        /// </summary>
        /// <remarks>
        /// This is set to a counter that is incremented when a message is received. The latest value of the
        /// counter is sent to browsers with the aircraft list, and they send that value back when they later
        /// ask for an update to the aircraft list. Each property is marked with the DataVersion current as-at
        /// the time they were last changed. By comparing the browser's last-known DataVersion against those
        /// that the properties have been marked with the code can determine which properties need to be sent
        /// to the browser in an update.
        /// </remarks>
        public long DataVersion { get; set; }

        private readonly VersionedValue<Guid> _ReceiverId = new();
        public long ReceiverIdChanged => _ReceiverId.DataVersion;
        /// <summary>
        /// The ID of the last receiver to provide a message for this aircraft. Only varies when the aircraft is being fed
        /// messages from a merged feed.
        /// </summary>
        public Guid ReceiverId
        {
            get => _ReceiverId.Value;
            set => _ReceiverId.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int?> _SignalLevel = new();
        public long SignalLevelChanged => _SignalLevel.DataVersion;
        /// <summary>
        /// The signal level of the last message applied to the aircraft. If the value is  null then the receiver isn't
        /// passing along signal levels.
        /// </summary>
        public int? SignalLevel
        {
            get => _SignalLevel.Value;
            set => _SignalLevel.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Icao24 = new();
        public long Icao24Changed => _Icao24.DataVersion;
        /// <summary>
        /// The aircraft's ICAO identifier.
        /// </summary>
        public string Icao24
        {
            get => _Icao24.Value;
            set => _Icao24.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _Icao24Invalid = new();
        public long Icao24InvalidChanged => _Icao24Invalid.DataVersion;
        /// <summary>
        /// A value indicating that the <see cref="Icao24"/> identifier does not comply with ICAO rules or is known to be a duplicate
        /// of another aircraft's ICAO.
        /// </summary>
        public bool Icao24Invalid
        {
            get => _Icao24Invalid.Value;
            set => _Icao24Invalid.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Callsign = new();
        public long CallsignChanged => _Callsign.DataVersion;
        /// <summary>
        /// The ATC callsign of the aircraft.
        /// </summary>
        public string Callsign
        {
            get => _Callsign.Value;
            set => _Callsign.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int?> _Altitude = new();
        public long AltitudeChanged => _Altitude.DataVersion;
        /// <summary>
        /// The pressure altitde (in feet) that the aircraft last reported. If the aircraft is
        /// transmitting geometric altitude and the air pressure is known then this will be the calculated
        /// pressure altitude.
        /// </summary>
        public int? Altitude
        {
            get => _Altitude.Value;
            set => _Altitude.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<AltitudeType> _AltitudeType = new();
        public long AltitudeTypeChanged => _AltitudeType.DataVersion;
        /// <summary>
        /// The type of altitude being transmitted by the aircraft.
        /// </summary>
        public AltitudeType AltitudeType
        {
            get => _AltitudeType.Value;
            set => _AltitudeType.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int?> _GeometricAltitude = new();
        public long GeometricAltitudeChanged => _GeometricAltitude.DataVersion;
        /// <summary>
        /// The geometric altitude, i.e. how far the aircraft is above sea level. If the aircraft is
        /// transmitting barometric altitudes and the air pressure is known then this is the calculated geometric
        /// altitude.
        /// </summary>
        public int? GeometricAltitude
        {
            get => _GeometricAltitude.Value;
            set => _GeometricAltitude.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _AirPressureInHg = new();
        public long AirPressureInHgChanged => _AirPressureInHg.DataVersion;
        /// <summary>
        /// The pressure setting for the aircraft. The value will be null if air pressure downloads
        /// are switched off, or the server is unavailable, or no air pressures have been downloaded yet.
        /// </summary>
        public double? AirPressureInHg
        {
            get => _AirPressureInHg.Value;
            set => _AirPressureInHg.SetValue(value, DataVersion);
        }

        /// <summary>
        /// Gets and sets the time that the air pressure setting was looked up.
        /// </summary>
        public DateTime? AirPressureLookedUpUtc { get; set; } = new();

        private readonly VersionedValue<int?> _TargetAltitude = new();
        public long TargetAltitudeChanged => _TargetAltitude.DataVersion;
        /// <summary>
        /// The altitude (in feet) that the aircraft's autopilot / FMS etc. is set to.
        /// </summary>
        public int? TargetAltitude
        {
            get => _TargetAltitude.Value;
            set => _TargetAltitude.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _GroundSpeed = new();
        public long GroundSpeedChanged => _GroundSpeed.DataVersion;
        /// <summary>
        /// The speed (in knots) that the aircraft last reported.
        /// </summary>
        public double? GroundSpeed
        {
            get => _GroundSpeed.Value;
            set => _GroundSpeed.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<SpeedType> _SpeedType = new();
        public long SpeedTypeChanged => _SpeedType.DataVersion;
        /// <summary>
        /// The type of speed being transmitted by the aircraft.
        /// </summary>
        public SpeedType SpeedType
        {
            get => _SpeedType.Value;
            set => _SpeedType.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _Latitude = new();
        public long LatitudeChanged => _Latitude.DataVersion;
        /// <summary>
        /// The last reported latitude of the aircraft.
        /// </summary>
        public double? Latitude
        {
            get => _Latitude.Value;
            set => _Latitude.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _Longitude = new();
        public long LongitudeChanged => _Longitude.DataVersion;
        /// <summary>
        /// The last reported longitude of the aircraft.
        /// </summary>
        public double? Longitude
        {
            get => _Longitude.Value;
            set => _Longitude.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<DateTime?> _PositionTime = new();
        public long PositionTimeChanged => _PositionTime.DataVersion;
        /// <summary>
        /// The UTC date and time that <see cref="Latitude"/> and <see cref="Longitude"/> were last changed.
        /// </summary>
        public DateTime? PositionTime
        {
            get => _PositionTime.Value;
            set => _PositionTime.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool?> _PositionIsMlat = new();
        public long PositionIsMlatChanged => _PositionIsMlat.DataVersion;
        /// <summary>
        /// A value indicating that the position was calculated by an MLAT server, or came off an MLAT feed.
        /// </summary>
        public bool? PositionIsMlat
        {
            get => _PositionIsMlat.Value;
            set => _PositionIsMlat.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<Guid?> _PositionReceiverId = new();
        public long PositionReceiverIdChanged => _PositionReceiverId.DataVersion;
        /// <summary>
        /// The ID of the receiver that supplied the last position for the aircraft.
        /// </summary>
        public Guid? PositionReceiverId
        {
            get => _PositionReceiverId.Value;
            set => _PositionReceiverId.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _IsTisb = new();
        public long IsTisbChanged => _IsTisb.DataVersion;
        /// <summary>
        /// A value indicating that the last message received for the aircraft came from a TIS-B source.
        /// </summary>
        public bool IsTisb
        {
            get => _IsTisb.Value;
            set => _IsTisb.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _Track = new();
        public long TrackChanged => _Track.DataVersion;
        /// <summary>
        /// The last reported heading over the ground of the aircraft. If the aircraft is not reporting its
        /// track (see <see cref="IsTransmittingTrack"/>) then the code will calculate one for it. This is in
        /// degrees clockwise.
        /// </summary>
        /// <remarks>Tracks are not calculated until the aircraft has moved a small distance.</remarks>
        public double? Track
        {
            get => _Track.Value;
            set => _Track.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<double?> _TargetTrack = new();
        public long TargetTrackChanged => _TargetTrack.DataVersion;
        /// <summary>
        /// The track or heading on the aircraft's autopilot / FMS etc.
        /// </summary>
        public double? TargetTrack
        {
            get => _TargetTrack.Value;
            set => _TargetTrack.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _TrackIsHeading = new();
        public long TrackIsHeadingChanged => _TrackIsHeading.DataVersion;
        /// <summary>
        /// A value indicating that the <see cref="Track"/> is the heading of the aircraft. If this
        /// is false then <see cref="Track"/> is the ground track, which is the default.
        /// </summary>
        public bool TrackIsHeading
        {
            get => _TrackIsHeading.Value;
            set => _TrackIsHeading.SetValue(value, DataVersion);
        }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is transmitting its track, or at least that
        /// BaseStation is reporting one for it.
        /// </summary>
        public bool IsTransmittingTrack { get; set; }

        private readonly VersionedValue<int?> _VerticalRate = new();
        public long VerticalRateChanged => _VerticalRate.DataVersion;
        /// <summary>
        /// The last reported rate of ascent or descent (-ve) of the aircraft in feet per minute.
        /// </summary>
        public int? VerticalRate
        {
            get => _VerticalRate.Value;
            set => _VerticalRate.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<AltitudeType> _VerticalRateType = new();
        public long VerticalRateTypeChanged => _VerticalRateType.DataVersion;
        /// <summary>
        /// The type of vertical rate being transmitted by the aircraft.
        /// </summary>
        public AltitudeType VerticalRateType
        {
            get => _VerticalRateType.Value;
            set => _VerticalRateType.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int?> _Squawk = new();
        public long SquawkChanged => _Squawk.DataVersion;
        /// <summary>
        /// The squawk code transmitted by the aircraft.
        /// </summary>
        public int? Squawk
        {
            get => _Squawk.Value;
            set => _Squawk.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool?> _IdentActive = new();
        public long IdentActiveChanged => _IdentActive.DataVersion;
        /// <summary>
        /// A value indicating that the aircraft's ident is active.
        /// </summary>
        public bool? IdentActive
        {
            get => _IdentActive.Value;
            set => _IdentActive.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool?> _Emergency = new();
        public long EmergencyChanged => _Emergency.DataVersion;
        /// <summary>
        /// A value indicating that the <see cref="Squawk"/> represents an emergency code.
        /// </summary>
        public bool? Emergency
        {
            get => _Emergency.Value;
            set => _Emergency.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Registration = new();
        public long RegistrationChanged => _Registration.DataVersion;
        /// <summary>
        /// The aircraft's registration.
        /// </summary>
        /// <remarks>This is whatever is recorded in the database for the registration and may contain non-compliant characters.</remarks>
        public string Registration
        {
            get => _Registration.Value;
            set => _Registration.SetValue(value, DataVersion);
        }

        private string _IcaoCompliantRegistration;
        private string _IcaoCompliantRegistrationBasedOnRegistration;
        /// <summary>
        /// Gets the <see cref="Registration"/> with non-compliant characters stripped out.
        /// </summary>
        /// <remarks>It transpires that some people record non-compliant characters in the database <see cref="Registration"/> field for the aircraft. This removes
        /// all non-compliant characters so that only those permitted by annex 7 of the Convention on International Civil Aviation remain.</remarks>
        public string IcaoCompliantRegistration
        {
            get {
                if(_IcaoCompliantRegistrationBasedOnRegistration != Registration) {
                    _IcaoCompliantRegistrationBasedOnRegistration = Registration;
                    _IcaoCompliantRegistration = Describe.IcaoCompliantRegistration(Registration);
                }
                return _IcaoCompliantRegistration;
            }
        }

        private readonly VersionedValue<DateTime> _FirstSeen = new();
        public long FirstSeenChanged => _FirstSeen.DataVersion;
        /// <summary>
        /// The time that a message from the aircraft was first received in this session.
        /// </summary>
        public DateTime FirstSeen
        {
            get => _FirstSeen.Value;
            set => _FirstSeen.SetValue(value, DataVersion);
        }

        /// <summary>
        /// Gets or sets the time of the last message received from the aircraft. This is not versioned.
        /// </summary>
        /// <remarks>
        /// This is always set regardless of the type of feed that the aircraft message came in on.
        /// </remarks>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from a Mode-S feed for the aircraft.
        /// This is not versioned.
        /// </summary>
        public DateTime LastModeSUpdate { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from a SatCom feed for the aircraft.
        /// This is not versioned.
        /// </summary>
        public DateTime LastSatcomUpdate { get; set; }

        private readonly VersionedValue<long> _CountMessagesReceived = new();
        public long CountMessagesReceivedChanged => _CountMessagesReceived.DataVersion;
        /// <summary>
        /// The number of messages received for the aircraft.
        /// </summary>
        public long CountMessagesReceived
        {
            get => _CountMessagesReceived.Value;
            set => _CountMessagesReceived.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Type = new();
        public long TypeChanged => _Type.DataVersion;
        /// <summary>
        /// The ICAO8643 type code for the aircraft.
        /// </summary>
        public string Type
        {
            get => _Type.Value;
            set => _Type.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Manufacturer = new();
        public long ManufacturerChanged => _Manufacturer.DataVersion;
        /// <summary>
        /// The name of the aircraft's maker.
        /// </summary>
        public string Manufacturer
        {
            get => _Manufacturer.Value;
            set => _Manufacturer.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Model = new();
        public long ModelChanged => _Model.DataVersion;
        /// <summary>
        /// A description of the model of the aircraft, as assigned by the manufacturer.
        /// </summary>
        public string Model
        {
            get => _Model.Value;
            set => _Model.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _ConstructionNumber = new();
        public long ConstructionNumberChanged => _ConstructionNumber.DataVersion;
        /// <summary>
        /// The construction number assigned to the aircraft by the manufacturer.
        /// </summary>
        public string ConstructionNumber
        {
            get => _ConstructionNumber.Value;
            set => _ConstructionNumber.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _YearBuilt = new();
        public long YearBuiltChanged => _YearBuilt.DataVersion;
        /// <summary>
        /// The year the aircraft was manufactured as a string (it's a string in BaseStation.sqb).
        /// </summary>
        public string YearBuilt
        {
            get => _YearBuilt.Value;
            set => _YearBuilt.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<Airport> _Origin = new();
        public long OriginChanged => _Origin.DataVersion;
        /// <summary>
        /// The airport where the aircraft's journey began.
        /// </summary>
        public Airport Origin
        {
            get => _Origin.Value;
            set => _Origin.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<Airport> _Destination = new();
        public long DestinationChanged => _Destination.DataVersion;
        /// <summary>
        /// The airport where the aircraft's journey is scheduled to end.
        /// </summary>
        public Airport Destination
        {
            get => _Destination.Value;
            set => _Destination.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<IReadOnlyList<Airport>> _Stopovers = new();
        public long StopoversChanged => _Stopovers.DataVersion;
        /// <summary>
        /// A list of scheduled stops for the aircraft.
        /// </summary>
        public IReadOnlyList<Airport> Stopovers
        {
            get => _Stopovers.Value ?? Array.Empty<Airport>();
            set => _Stopovers.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _IsPositioningFlight = new();
        public long IsPositioningFlightChanged => _IsPositioningFlight.DataVersion;
        /// <summary>
        /// A value indicating that this is probably a positioning / ferry flight.
        /// </summary>
        public bool IsPositioningFlight
        {
            get => _IsPositioningFlight.Value;
            set => _IsPositioningFlight.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _IsCharterFlight = new();
        public long IsCharterFlightChanged => _IsCharterFlight.DataVersion;
        /// <summary>
        /// A value indicating that the this probably a charter flight.
        /// </summary>
        public bool IsCharterFlight
        {
            get => _IsCharterFlight.Value;
            set => _IsCharterFlight.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Operator = new();
        public long OperatorChanged => _Operator.DataVersion;
        /// <summary>
        /// The name of the company or individual that is operating the aircraft.
        /// </summary>
        public string Operator
        {
            get => _Operator.Value;
            set => _Operator.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _OperatorIcao = new();
        public long OperatorIcaoChanged => _OperatorIcao.DataVersion;
        /// <summary>
        /// The ICAO code for the aircraft's operator.
        /// </summary>
        public string OperatorIcao
        {
            get => _OperatorIcao.Value;
            set => _OperatorIcao.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<WakeTurbulenceCategory> _WakeTurbulenceCategory = new();
        public long WakeTurbulenceCategoryChanged => _WakeTurbulenceCategory.DataVersion;
        /// <summary>
        /// The ICAO 8643 ATC wake turbulence category for this type of aircraft.
        /// </summary>
        public WakeTurbulenceCategory WakeTurbulenceCategory
        {
            get => _WakeTurbulenceCategory.Value;
            set => _WakeTurbulenceCategory.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<EngineType> _EngineType = new();
        public long EngineTypeChanged => _EngineType.DataVersion;
        /// <summary>
        /// The ICAO 8643 engine type for this type of aircraft.
        /// </summary>
        public EngineType EngineType
        {
            get => _EngineType.Value;
            set => _EngineType.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<EnginePlacement> _EnginePlacement = new();
        public long EnginePlacementChanged => _EnginePlacement.DataVersion;
        /// <summary>
        /// An indication of where the engines are mounted on the aircraft.
        /// </summary>
        public EnginePlacement EnginePlacement
        {
            get => _EnginePlacement.Value;
            set => _EnginePlacement.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _NumberOfEngines = new();
        public long NumberOfEnginesChanged => _NumberOfEngines.DataVersion;
        /// <summary>
        /// The number of engines as per ICAO 8643. Note that this 'number' can include the value C...
        /// </summary>
        public string NumberOfEngines
        {
            get => _NumberOfEngines.Value;
            set => _NumberOfEngines.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<Species> _Species = new();
        public long SpeciesChanged => _Species.DataVersion;
        /// <summary>
        /// The type of aircraft as recorded in ICAO 8643.
        /// </summary>
        public Species Species
        {
            get => _Species.Value;
            set => _Species.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _IsMilitary = new();
        public long IsMilitaryChanged => _IsMilitary.DataVersion;
        /// <summary>
        /// A value indicating that the aircraft is probably a military aircraft.
        /// </summary>
        public bool IsMilitary
        {
            get => _IsMilitary.Value;
            set => _IsMilitary.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _Icao24Country = new();
        public long Icao24CountryChanged => _Icao24Country.DataVersion;
        /// <summary>
        /// The country that the aircraft's ICAO24 is assigned to.
        /// </summary>
        public string Icao24Country
        {
            get => _Icao24Country.Value;
            set => _Icao24Country.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _PictureFileName = new();
        public long PictureFileNameChanged => _PictureFileName.DataVersion;
        /// <summary>
        /// The full path and filename of the aircraft's picture or null if the aircraft has no picture.
        /// </summary>
        public string PictureFileName
        {
            get => _PictureFileName.Value;
            set => _PictureFileName.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int> _PictureWidth = new();
        public long PictureWidthChanged => _PictureWidth.DataVersion;
        /// <summary>
        /// The width of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        public int PictureWidth
        {
            get => _PictureWidth.Value;
            set => _PictureWidth.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int> _PictureHeight = new();
        public long PictureHeightChanged => _PictureHeight.DataVersion;
        /// <summary>
        /// The height of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        public int PictureHeight
        {
            get => _PictureHeight.Value;
            set => _PictureHeight.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<TransponderType> _TransponderType = new();
        public long TransponderTypeChanged => _TransponderType.DataVersion;
        /// <summary>
        /// The transponder type used by the aircraft.
        /// </summary>
        public TransponderType TransponderType
        {
            get => _TransponderType.Value;
            set => _TransponderType.SetValue(value, DataVersion);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataVersion"/> that was current when the coordinates were first
        /// started for the aircraft.
        /// </summary>
        public long FirstCoordinateChanged { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataVersion"/> that was current when the last coordinate was added for
        /// the aircraft.
        /// </summary>
        public long LastCoordinateChanged { get; set; }

        /// <summary>
        /// Gets or sets the time of the latest coordinate on the lists.
        /// </summary>
        public DateTime LatestCoordinateTime { get; set; }

        /// <summary>
        /// The list of coordinates representing the full track of the aircraft, from when it was first seen until now.
        /// </summary>
        /// <remarks>
        /// Coordinates are only added to this list when the aircraft changes direction
        /// but there is no limit to the number of coordinates held for the aircraft.
        /// </remarks>
        public List<Coordinate> FullCoordinates { get; } = new();

        /// <summary>
        /// The list of coordinates representing the short track of the aircraft, the locations reported by it for the past so-many seconds.
        /// </summary>
        /// <remarks>
        /// Coordinates are perpetually added to this list irrespective of the direction
        /// the aircraft is taking but coordinates older than the configuration value
        /// for the short trail duration are removed as snapshots are taken of the
        /// aircraft, or as new coordinates are added.
        /// </remarks>
        public List<Coordinate> ShortCoordinates { get; } = new();

        private readonly VersionedValue<bool> _IsInteresting = new();
        public long IsInterestingChanged => _IsInteresting.DataVersion;
        /// <summary>
        /// A value indicating that the aircraft is flagged as interesting in BaseStation.
        /// </summary>
        public bool IsInteresting
        {
            get => _IsInteresting.Value;
            set => _IsInteresting.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<int> _FlightsCount = new();
        public long FlightsCountChanged => _FlightsCount.DataVersion;
        /// <summary>
        /// The number of flights that the aircraft has logged for it in the BaseStation database.
        /// </summary>
        public int FlightsCount
        {
            get => _FlightsCount.Value;
            set => _FlightsCount.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool?> _OnGround = new();
        public long OnGroundChanged => _OnGround.DataVersion;
        /// <summary>
        /// A value indicating that the vehicle is reporting itself as being on the ground.
        /// </summary>
        public bool? OnGround
        {
            get => _OnGround.Value;
            set => _OnGround.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<bool> _CallsignIsSuspect = new();
        public long CallsignIsSuspectChanged => _CallsignIsSuspect.DataVersion;
        /// <summary>
        /// A value indicating that the callsign came from a possible BDS2,0 message instead of an
        /// ADS-B message.
        /// </summary>
        public bool CallsignIsSuspect
        {
            get => _CallsignIsSuspect.Value;
            set => _CallsignIsSuspect.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _UserNotes = new();
        public long UserNotesChanged => _UserNotes.DataVersion;
        /// <summary>
        /// The <see cref="UserNotes"/> from the aircraft's database record.
        /// </summary>
        public string UserNotes
        {
            get => _UserNotes.Value;
            set => _UserNotes.SetValue(value, DataVersion);
        }

        private readonly VersionedValue<string> _UserTag = new();
        public long UserTagChanged => _UserTag.DataVersion;
        /// <summary>
        /// The <see cref="UserTag"/> from the aircraft's database record.
        /// </summary>
        public string UserTag
        {
            get => _UserTag.Value;
            set => _UserTag.SetValue(value, DataVersion);
        }

        /// <summary>
        /// Creates a new aircraft object.
        /// </summary>
        public Aircraft()
        {
        }

        /// <summary>
        /// Runs the delegate within a lock.
        /// </summary>
        /// <param name="lockedDelegate"></param>
        public void Lock(Action<Aircraft> lockedDelegate)
        {
            lock(_SyncLock) {
                lockedDelegate(this);
            }
        }

        /// <summary>
        /// Removes all coordinates from the aircraft's coordinates lists.
        /// </summary>
        public void ResetCoordinates()
        {
            Lock(_ => {
                LatestCoordinateTime = default;
                FullCoordinates.Clear();
                ShortCoordinates.Clear();
                FirstCoordinateChanged = 0;
                LastCoordinateChanged = 0;
            });
        }

        /// <summary>
        /// Updates the <see cref="FullCoordinates"/> and <see cref="ShortCoordinates"/> lists using the current values
        /// on the aircraft. It is assumed that this is being called from within <see cref="Lock"/>.
        /// </summary>
        /// <param name="utcNow">The UTC time at which the message was processed. </param>
        /// <param name="shortCoordinateSeconds">The number of seconds of short coordinates that are to be maintained. This normally comes
        /// from configurations - I don't want to have every aircraft in a 500 aircraft system called when configurations are changed so the
        /// responsibility for passing the correct value is shifted to higher up the chain.</param>
        /// <remarks>
        /// This is not entirely straight-forward - duplicate points are not written to the lists, the short list has to
        /// be kept to a configured number of seconds and intermediate coordinates between two points in the full list
        /// are expunged. Further updates less than a second after the last coordinate in the lists are dropped. This is
        /// all done in an effort to keep the lists down to a reasonable length.
        /// </remarks>
        public void UpdateCoordinates(DateTime utcNow, int shortCoordinateSeconds)
        {
            if(Latitude != null && Longitude != null) {
                var nowTick = utcNow.Ticks;

                var lastFullCoordinate = FullCoordinates.Count == 0
                    ? null
                    : FullCoordinates[FullCoordinates.Count - 1];
                var secondLastFullCoordinate = FullCoordinates.Count < 2
                    ? null
                    : FullCoordinates[FullCoordinates.Count - 2];

                if(   lastFullCoordinate == null
                   || Latitude != lastFullCoordinate.Latitude
                   || Longitude != lastFullCoordinate.Longitude
                   || Altitude != lastFullCoordinate.Altitude
                   || GroundSpeed != lastFullCoordinate.GroundSpeed
                ) {
                    PositionTime = utcNow;

                    // Check to see whether the aircraft appears to be moving impossibly fast and, if it is, reset its trail. Do this even if
                    // the gap between this message and the last is below the threshold for adding to the trails.
                    if(lastFullCoordinate != null) {
                        var distance = GreatCircleMaths.Distance(lastFullCoordinate.Latitude, lastFullCoordinate.Longitude, Latitude, Longitude);
                        if(distance > _ResetCoordinatesDistance) {
                            var fastestTime = _ResetCoordinatesTime * (distance / _ResetCoordinatesDistance);
                            if(nowTick - lastFullCoordinate.Tick < fastestTime) ResetCoordinates();
                        }
                    }

                    // Only update the trails if more than one second has elapsed since the last position update
                    var lastUpdateTick = lastFullCoordinate == null
                        ? 0L
                        : lastFullCoordinate.Tick;
                    if(nowTick - lastUpdateTick >= _TicksPerSecond) {
                        var track = Round.TrackHeading(Track);
                        var altitude = Round.TrackAltitude(Altitude);
                        var groundSpeed = Round.TrackGroundSpeed(GroundSpeed);
                        var coordinate = new Coordinate(
                            DataVersion,
                            nowTick,
                            Latitude.Value,
                            Longitude.Value,
                            track,
                            altitude,
                            groundSpeed
                        );
                        var positionChanged =  lastFullCoordinate != null
                                            && (
                                                   coordinate.Latitude != lastFullCoordinate.Latitude
                                                || coordinate.Longitude != lastFullCoordinate.Longitude
                                               );

                        if(   FullCoordinates.Count > 1
                           && track != null
                           && lastFullCoordinate.Heading == track
                           && secondLastFullCoordinate.Heading == track
                           && (
                                   !positionChanged
                                || (
                                       lastFullCoordinate.Altitude == altitude
                                    && secondLastFullCoordinate.Altitude == altitude
                                   )
                              )
                           && (
                                   !positionChanged
                                || (
                                       lastFullCoordinate.GroundSpeed == groundSpeed
                                    && secondLastFullCoordinate.GroundSpeed == groundSpeed
                                   )
                              )
                        ) {
                            FullCoordinates[^1] = coordinate;
                        } else {
                            FullCoordinates.Add(coordinate);
                        }

                        var earliestAllowable = nowTick - (_TicksPerSecond * shortCoordinateSeconds);
                        var firstAllowableIndex = ShortCoordinates.FindIndex(c => c.Tick >= earliestAllowable);
                        if(firstAllowableIndex == -1) {
                            ShortCoordinates.Clear();
                        } else if(firstAllowableIndex > 0) {
                            ShortCoordinates.RemoveRange(0, firstAllowableIndex);
                        }
                        ShortCoordinates.Add(coordinate);

                        if(FirstCoordinateChanged == 0) {
                            FirstCoordinateChanged = DataVersion;
                        }

                        LastCoordinateChanged = DataVersion;
                        LatestCoordinateTime = utcNow;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        /// <returns></returns>
        public Aircraft Clone()
        {
            var result = (Aircraft)Activator.CreateInstance(GetType());

            Lock(_ => {
                result.FullCoordinates.         AddRange(FullCoordinates);
                result.ShortCoordinates.        AddRange(ShortCoordinates);

                result.AirPressureLookedUpUtc = AirPressureLookedUpUtc;
                result.FirstCoordinateChanged = FirstCoordinateChanged;
                result.LastCoordinateChanged =  LastCoordinateChanged;
                result.DataVersion =            DataVersion;
                result.IsTransmittingTrack =    IsTransmittingTrack;
                result.LastUpdate =             LastUpdate;
                result.LastModeSUpdate =        LastModeSUpdate;
                result.LastSatcomUpdate =       LastSatcomUpdate;
                result.LatestCoordinateTime =   LatestCoordinateTime;
                result.UniqueId =               UniqueId;

                result._AirPressureInHg.        CopyFrom(_AirPressureInHg);
                result._Altitude.               CopyFrom(_Altitude);
                result._AltitudeType.           CopyFrom(_AltitudeType);
                result._Callsign.               CopyFrom(_Callsign);
                result._CallsignIsSuspect.      CopyFrom(_CallsignIsSuspect);
                result._ConstructionNumber.     CopyFrom(_ConstructionNumber);
                result._GeometricAltitude.      CopyFrom(_GeometricAltitude);
                result._CountMessagesReceived.  CopyFrom(_CountMessagesReceived);
                result._Destination.            CopyFrom(_Destination);
                result._Emergency.              CopyFrom(_Emergency);
                result._EnginePlacement.        CopyFrom(_EnginePlacement);
                result._EngineType.             CopyFrom(_EngineType);
                result._FirstSeen.              CopyFrom(_FirstSeen);
                result._FlightsCount.           CopyFrom(_FlightsCount);
                result._GroundSpeed.            CopyFrom(_GroundSpeed);
                result._Icao24.                 CopyFrom(_Icao24);
                result._Icao24Country.          CopyFrom(_Icao24Country);
                result._Icao24Invalid.          CopyFrom(_Icao24Invalid);
                result._IdentActive.            CopyFrom(_IdentActive);
                result._IsCharterFlight.        CopyFrom(_IsCharterFlight);
                result._IsInteresting.          CopyFrom(_IsInteresting);
                result._IsMilitary.             CopyFrom(_IsMilitary);
                result._IsPositioningFlight.    CopyFrom(_IsPositioningFlight);
                result._IsTisb.                 CopyFrom(_IsTisb);
                result._Latitude.               CopyFrom(_Latitude);
                result._Longitude.              CopyFrom(_Longitude);
                result._Manufacturer.           CopyFrom(_Manufacturer);
                result._Model.                  CopyFrom(_Model);
                result._NumberOfEngines.        CopyFrom(_NumberOfEngines);
                result._OnGround.               CopyFrom(_OnGround);
                result._Operator.               CopyFrom(_Operator);
                result._OperatorIcao.           CopyFrom(_OperatorIcao);
                result._Origin.                 CopyFrom(_Origin);
                result._PictureFileName.        CopyFrom(_PictureFileName);
                result._PictureHeight.          CopyFrom(_PictureHeight);
                result._PictureWidth.           CopyFrom(_PictureWidth);
                result._PositionIsMlat.         CopyFrom(_PositionIsMlat);
                result._PositionReceiverId.     CopyFrom(_PositionReceiverId);
                result._PositionTime.           CopyFrom(_PositionTime);
                result._ReceiverId.             CopyFrom(_ReceiverId);
                result._Registration.           CopyFrom(_Registration);
                result._SignalLevel.            CopyFrom(_SignalLevel);
                result._Species.                CopyFrom(_Species);
                result._SpeedType.              CopyFrom(_SpeedType);
                result._Squawk.                 CopyFrom(_Squawk);
                result._Stopovers.              CopyFrom(_Stopovers);
                result._TargetAltitude.         CopyFrom(_TargetAltitude);
                result._TargetTrack.            CopyFrom(_TargetTrack);
                result._Track.                  CopyFrom(_Track);
                result._TrackIsHeading.         CopyFrom(_TrackIsHeading);
                result._TransponderType.        CopyFrom(_TransponderType);
                result._Type.                   CopyFrom(_Type);
                result._UserNotes.              CopyFrom(_UserNotes);
                result._UserTag.                CopyFrom(_UserTag);
                result._VerticalRate.           CopyFrom(_VerticalRate);
                result._VerticalRateType.       CopyFrom(_VerticalRateType);
                result._WakeTurbulenceCategory. CopyFrom(_WakeTurbulenceCategory);
                result._YearBuilt.              CopyFrom(_YearBuilt);
            });

            return result;
        }
    }
}
