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
        /// The lock object used to synchronise writes to the object.
        /// </summary>
        private object _SyncLock = new();

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

        /// <summary>
        /// The ID of the last receiver to provide a message for this aircraft. Only varies when the aircraft is being fed
        /// messages from a merged feed.
        /// </summary>
        public VersionedValue<Guid> ReceiverId { get; } = new();

        /// <summary>
        /// The signal level of the last message applied to the aircraft. If the value is  null then the receiver isn't
        /// passing along signal levels.
        /// </summary>
        public VersionedValue<int?> SignalLevel { get; } = new();

        /// <summary>
        /// The aircraft's ICAO identifier.
        /// </summary>
        public VersionedValue<string> Icao24 { get; } = new();

        /// <summary>
        /// A value indicating that the <see cref="Icao24"/> identifier does not comply with ICAO rules or is known to be a duplicate
        /// of another aircraft's ICAO.
        /// </summary>
        public VersionedValue<bool> Icao24Invalid { get; } = new();

        /// <summary>
        /// The ATC callsign of the aircraft.
        /// </summary>
        public VersionedValue<string> Callsign { get; } = new();

        /// <summary>
        /// The pressure altitde (in feet) that the aircraft last reported. If the aircraft is
        /// transmitting geometric altitude and the air pressure is known then this will be the calculated
        /// pressure altitude.
        /// </summary>
        public VersionedValue<int?> Altitude { get; } = new();

        /// <summary>
        /// The type of altitude being transmitted by the aircraft.
        /// </summary>
        public VersionedValue<AltitudeType> AltitudeType { get; } = new();

        /// <summary>
        /// The geometric altitude, i.e. how far the aircraft is above sea level. If the aircraft is
        /// transmitting barometric altitudes and the air pressure is known then this is the calculated geometric
        /// altitude.
        /// </summary>
        public VersionedValue<int?> GeometricAltitude { get; } = new();

        /// <summary>
        /// The pressure setting for the aircraft. The value will be null if air pressure downloads
        /// are switched off, or the server is unavailable, or no air pressures have been downloaded yet.
        /// </summary>
        public VersionedValue<double?> AirPressureInHg { get; } = new();

        /// <summary>
        /// Gets and sets the time that the air pressure setting was looked up.
        /// </summary>
        public DateTime? AirPressureLookedUpUtc { get; set; } = new();

        /// <summary>
        /// The altitude (in feet) that the aircraft's autopilot / FMS etc. is set to.
        /// </summary>
        public VersionedValue<int?> TargetAltitude { get; } = new();

        /// <summary>
        /// The speed (in knots) that the aircraft last reported.
        /// </summary>
        public VersionedValue<double?> GroundSpeed { get; } = new();

        /// <summary>
        /// The type of speed being transmitted by the aircraft.
        /// </summary>
        public VersionedValue<SpeedType> SpeedType { get; } = new();

        /// <summary>
        /// The last reported latitude of the aircraft.
        /// </summary>
        public VersionedValue<double?> Latitude { get; } = new();

        /// <summary>
        /// The last reported longitude of the aircraft.
        /// </summary>
        public VersionedValue<double?> Longitude { get; } = new();

        /// <summary>
        /// The UTC date and time that <see cref="Latitude"/> and <see cref="Longitude"/> were last changed.
        /// </summary>
        public VersionedValue<DateTime?> PositionTime { get; } = new();

        /// <summary>
        /// A value indicating that the position was calculated by an MLAT server, or came off an MLAT feed.
        /// </summary>
        public VersionedValue<bool?> PositionIsMlat { get; } = new();

        /// <summary>
        /// The ID of the receiver that supplied the last position for the aircraft.
        /// </summary>
        public VersionedValue<Guid?> PositionReceiverId { get; } = new();

        /// <summary>
        /// A value indicating that the last message received for the aircraft came from a TIS-B source.
        /// </summary>
        public VersionedValue<bool> IsTisb { get; } = new();

        /// <summary>
        /// The last reported heading over the ground of the aircraft. If the aircraft is not reporting its
        /// track (see <see cref="IsTransmittingTrack"/>) then the code will calculate one for it. This is in
        /// degrees clockwise.
        /// </summary>
        /// <remarks>Tracks are not calculated until the aircraft has moved a small distance.</remarks>
        public VersionedValue<double?> Track { get; } = new();

        /// <summary>
        /// The track or heading on the aircraft's autopilot / FMS etc.
        /// </summary>
        public VersionedValue<double?> TargetTrack { get; } = new();

        /// <summary>
        /// A value indicating that the <see cref="Track"/> is the heading of the aircraft. If this
        /// is false then <see cref="Track"/> is the ground track, which is the default.
        /// </summary>
        public VersionedValue<bool> TrackIsHeading { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is transmitting its track, or at least that
        /// BaseStation is reporting one for it.
        /// </summary>
        public bool IsTransmittingTrack { get; set; }

        /// <summary>
        /// The last reported rate of ascent or descent (-ve) of the aircraft in feet per minute.
        /// </summary>
        public VersionedValue<int?> VerticalRate { get; } = new();

        /// <summary>
        /// The type of vertical rate being transmitted by the aircraft.
        /// </summary>
        public VersionedValue<AltitudeType> VerticalRateType { get; } = new();

        /// <summary>
        /// The squawk code transmitted by the aircraft.
        /// </summary>
        public VersionedValue<int?> Squawk { get; } = new();

        /// <summary>
        /// A value indicating that the aircraft's ident is active.
        /// </summary>
        public VersionedValue<bool?> IdentActive { get; } = new();

        /// <summary>
        /// A value indicating that the <see cref="Squawk"/> represents an emergency code.
        /// </summary>
        public VersionedValue<bool?> Emergency { get; } = new();

        /// <summary>
        /// The aircraft's registration.
        /// </summary>
        /// <remarks>This is whatever is recorded in the database for the registration and may contain non-compliant characters.</remarks>
        public VersionedValue<string> Registration { get; } = new();

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

        /// <summary>
        /// The time that a message from the aircraft was first received in this session.
        /// </summary>
        public VersionedValue<DateTime> FirstSeen { get; } = new();

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

        /// <summary>
        /// The number of messages received for the aircraft.
        /// </summary>
        public VersionedValue<long> CountMessagesReceived { get; } = new();

        /// <summary>
        /// The ICAO8643 type code for the aircraft.
        /// </summary>
        public VersionedValue<string> Type { get; } = new();

        /// <summary>
        /// The name of the aircraft's maker.
        /// </summary>
        public VersionedValue<string> Manufacturer { get; } = new();

        /// <summary>
        /// A description of the model of the aircraft, as assigned by the manufacturer.
        /// </summary>
        public VersionedValue<string> Model { get; } = new();

        /// <summary>
        /// The construction number assigned to the aircraft by the manufacturer.
        /// </summary>
        public VersionedValue<string> ConstructionNumber { get; } = new();

        /// <summary>
        /// The year the aircraft was manufactured as a string (it's a string in BaseStation.sqb).
        /// </summary>
        public VersionedValue<string> YearBuilt { get; } = new();

        /// <summary>
        /// The airport where the aircraft's journey began.
        /// </summary>
        public VersionedValue<Airport> Origin { get; } = new();

        /// <summary>
        /// The airport where the aircraft's journey is scheduled to end.
        /// </summary>
        public VersionedValue<Airport> Destination { get; } = new();

        /// <summary>
        /// A list of scheduled stops for the aircraft.
        /// </summary>
        public VersionedValue<IReadOnlyList<Airport>> Stopovers { get; } = new();

        /// <summary>
        /// A value indicating that this is probably a positioning / ferry flight.
        /// </summary>
        public VersionedValue<bool> IsPositioningFlight { get; } = new();

        /// <summary>
        /// A value indicating that the this probably a charter flight.
        /// </summary>
        public VersionedValue<bool> IsCharterFlight { get; } = new();

        /// <summary>
        /// The name of the company or individual that is operating the aircraft.
        /// </summary>
        public VersionedValue<string> Operator { get; } = new();

        /// <summary>
        /// The ICAO code for the aircraft's operator.
        /// </summary>
        public VersionedValue<string> OperatorIcao { get; } = new();

        /// <summary>
        /// The ICAO 8643 ATC wake turbulence category for this type of aircraft.
        /// </summary>
        public VersionedValue<WakeTurbulenceCategory> WakeTurbulenceCategory { get; } = new();

        /// <summary>
        /// The ICAO 8643 engine type for this type of aircraft.
        /// </summary>
        public VersionedValue<EngineType> EngineType { get; } = new();

        /// <summary>
        /// An indication of where the engines are mounted on the aircraft.
        /// </summary>
        public VersionedValue<EnginePlacement> EnginePlacement { get; } = new();

        /// <summary>
        /// The number of engines as per ICAO 8643. Note that this 'number' can include the value C...
        /// </summary>
        public VersionedValue<string> NumberOfEngines { get; } = new();

        /// <summary>
        /// The type of aircraft as recorded in ICAO 8643.
        /// </summary>
        public VersionedValue<Species> Species { get; } = new();

        /// <summary>
        /// A value indicating that the aircraft is probably a military aircraft.
        /// </summary>
        public VersionedValue<bool> IsMilitary { get; } = new();

        /// <summary>
        /// The country that the aircraft's ICAO24 is assigned to.
        /// </summary>
        public VersionedValue<string> Icao24Country { get; } = new();

        /// <summary>
        /// The full path and filename of the aircraft's picture or null if the aircraft has no picture.
        /// </summary>
        public VersionedValue<string> PictureFileName { get; } = new();

        /// <summary>
        /// The width of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        public VersionedValue<int> PictureWidth { get; } = new();

        /// <summary>
        /// The height of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        public VersionedValue<int> PictureHeight { get; } = new();

        /// <summary>
        /// The transponder type used by the aircraft.
        /// </summary>
        public VersionedValue<TransponderType> TransponderType { get; } = new();

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

        /// <summary>
        /// A value indicating that the aircraft is flagged as interesting in BaseStation.
        /// </summary>
        public VersionedValue<bool> IsInteresting { get; } = new();

        /// <summary>
        /// The number of flights that the aircraft has logged for it in the BaseStation database.
        /// </summary>
        public VersionedValue<int> FlightsCount { get; } = new();

        /// <summary>
        /// A value indicating that the vehicle is reporting itself as being on the ground.
        /// </summary>
        public VersionedValue<bool?> OnGround { get; } = new();

        /// <summary>
        /// A value indicating that the callsign came from a possible BDS2,0 message instead of an
        /// ADS-B message.
        /// </summary>
        public VersionedValue<bool> CallsignIsSuspect { get; } = new();

        /// <summary>
        /// The <see cref="UserNotes"/> from the aircraft's database record.
        /// </summary>
        public VersionedValue<string> UserNotes { get; } = new();

        /// <summary>
        /// The <see cref="UserTag"/> from the aircraft's database record.
        /// </summary>
        public VersionedValue<string> UserTag { get; } = new();

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the <see cref="FullCoordinates"/> and <see cref="ShortCoordinates"/> lists using the current values
        /// on the aircraft.
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        /// <returns></returns>
        public Aircraft Clone()
        {
            var result = (Aircraft)Activator.CreateInstance(GetType());

            lock(this) {
                result.FullCoordinates  .AddRange(FullCoordinates);
                result.ShortCoordinates .AddRange(ShortCoordinates);

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

                result.AirPressureInHg.         CopyFrom(AirPressureInHg);
                result.Altitude.                CopyFrom(Altitude);
                result.AltitudeType.            CopyFrom(AltitudeType);
                result.Callsign.                CopyFrom(Callsign);
                result.CallsignIsSuspect.       CopyFrom(CallsignIsSuspect);
                result.ConstructionNumber.      CopyFrom(ConstructionNumber);
                result.GeometricAltitude.       CopyFrom(GeometricAltitude);
                result.CountMessagesReceived.   CopyFrom(CountMessagesReceived);
                result.Destination.             CopyFrom(Destination);
                result.Emergency.               CopyFrom(Emergency);
                result.EnginePlacement.         CopyFrom(EnginePlacement);
                result.EngineType.              CopyFrom(EngineType);
                result.FirstSeen.               CopyFrom(FirstSeen);
                result.FlightsCount.            CopyFrom(FlightsCount);
                result.GroundSpeed.             CopyFrom(GroundSpeed);
                result.Icao24.                  CopyFrom(Icao24);
                result.Icao24Country.           CopyFrom(Icao24Country);
                result.Icao24Invalid.           CopyFrom(Icao24Invalid);
                result.IdentActive.             CopyFrom(IdentActive);
                result.IsCharterFlight.         CopyFrom(IsCharterFlight);
                result.IsInteresting.           CopyFrom(IsInteresting);
                result.IsMilitary.              CopyFrom(IsMilitary);
                result.IsPositioningFlight.     CopyFrom(IsPositioningFlight);
                result.IsTisb.                  CopyFrom(IsTisb);
                result.Latitude.                CopyFrom(Latitude);
                result.Longitude.               CopyFrom(Longitude);
                result.Manufacturer.            CopyFrom(Manufacturer);
                result.Model.                   CopyFrom(Model);
                result.NumberOfEngines.         CopyFrom(NumberOfEngines);
                result.OnGround.                CopyFrom(OnGround);
                result.Operator.                CopyFrom(Operator);
                result.OperatorIcao.            CopyFrom(OperatorIcao);
                result.Origin.                  CopyFrom(Origin);
                result.PictureFileName.         CopyFrom(PictureFileName);
                result.PictureHeight.           CopyFrom(PictureHeight);
                result.PictureWidth.            CopyFrom(PictureWidth);
                result.PositionIsMlat.          CopyFrom(PositionIsMlat);
                result.PositionReceiverId.      CopyFrom(PositionReceiverId);
                result.PositionTime.            CopyFrom(PositionTime);
                result.ReceiverId.              CopyFrom(ReceiverId);
                result.Registration.            CopyFrom(Registration);
                result.SignalLevel.             CopyFrom(SignalLevel);
                result.Species.                 CopyFrom(Species);
                result.SpeedType.               CopyFrom(SpeedType);
                result.Squawk.                  CopyFrom(Squawk);
                result.Stopovers.               CopyFrom(Stopovers);
                result.TargetAltitude.          CopyFrom(TargetAltitude);
                result.TargetTrack.             CopyFrom(TargetTrack);
                result.Track.                   CopyFrom(Track);
                result.TrackIsHeading.          CopyFrom(TrackIsHeading);
                result.TransponderType.         CopyFrom(TransponderType);
                result.Type.                    CopyFrom(Type);
                result.UserNotes.               CopyFrom(UserNotes);
                result.UserTag.                 CopyFrom(UserTag);
                result.VerticalRate.            CopyFrom(VerticalRate);
                result.VerticalRateType.        CopyFrom(VerticalRateType);
                result.WakeTurbulenceCategory.  CopyFrom(WakeTurbulenceCategory);
                result.YearBuilt.               CopyFrom(YearBuilt);
            }

            return result;
        }
    }
}
