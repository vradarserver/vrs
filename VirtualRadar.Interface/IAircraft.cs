﻿// Copyright © 2010 onwards, Andrew Whewell
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
    public interface IAircraft : ICloneable
    {
        /// <summary>
        /// Gets or sets the unique identifier of the aircraft.
        /// </summary>
        int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when the data was last changed.
        /// </summary>
        /// <remarks>This is set to a counter that is incremented when a message is received. The latest value of the counter is sent to
        /// browsers with the aircraft list, and they send that value back when they later ask for an update to the aircraft list. Each
        /// property is marked with the DataVersion current as-at the time they were last changed. By comparing the browser's last-known
        /// DataVersion against those that the properties have been marked with the code can determine which properties need to be sent
        /// to the browser in an update.</remarks>
        long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the ID of the last receiver to provide a message for this aircraft. Only varies when the aircraft is being fed
        /// messages from a merged feed.
        /// </summary>
        VersionedValue<Guid> ReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the signal level of the last message applied to the aircraft. If the value is  null then the receiver isn't
        /// passing along signal levels.
        /// </summary>
        VersionedValue<int?> SignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ICAO identifier.
        /// </summary>
        VersionedValue<string> Icao24 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Icao24"/> identifier does not comply with ICAO rules or is known to be a duplicate
        /// of another aircraft's ICAO.
        /// </summary>
        VersionedValue<bool> Icao24Invalid { get; set; }

        /// <summary>
        /// Gets or sets the ATC callsign of the aircraft.
        /// </summary>
        VersionedValue<string> Callsign { get; set; }

        /// <summary>
        /// Gets or sets the pressure altitde (in feet) that the aircraft last reported. If the aircraft is
        /// transmitting geometric altitude and the air pressure is known then this will be the calculated
        /// pressure altitude.
        /// </summary>
        VersionedValue<int?> Altitude { get; set; }

        /// <summary>
        /// Gets or sets the type of altitude being transmitted by the aircraft.
        /// </summary>
        VersionedValue<AltitudeType> AltitudeType { get; set; }

        /// <summary>
        /// Gets or sets the geometric altitude, i.e. how far the aircraft is above sea level. If the aircraft is
        /// transmitting barometric altitudes and the air pressure is known then this is the calculated geometric
        /// altitude.
        /// </summary>
        VersionedValue<int?> GeometricAltitude { get; set; }

        /// <summary>
        /// Gets or sets the pressure setting for the aircraft. The value will be null if air pressure downloads
        /// are switched off, or the server is unavailable, or no air pressures have been downloaded yet.
        /// </summary>
        VersionedValue<float?> AirPressureInHg { get; set; }

        /// <summary>
        /// Gets or sets the time that the air pressure setting was looked up.
        /// </summary>
        DateTime? AirPressureLookedUpUtc { get; set; }

        /// <summary>
        /// Gets or sets the altitude (in feet) that the aircraft's autopilot / FMS etc. is set to.
        /// </summary>
        VersionedValue<int?> TargetAltitude { get; set; }

        /// <summary>
        /// Gets or sets the speed (in knots) that the aircraft last reported.
        /// </summary>
        VersionedValue<float?> GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the type of speed being transmitted by the aircraft.
        /// </summary>
        VersionedValue<SpeedType> SpeedType { get; set; }

        /// <summary>
        /// Gets the last reported latitude of the aircraft.
        /// </summary>
        VersionedValue<double?> Latitude { get; set; }

        /// <summary>
        /// Gets the last reported longitude of the aircraft.
        /// </summary>
        VersionedValue<double?> Longitude { get; set; }

        /// <summary>
        /// Gets the UTC date and time that <see cref="Latitude"/> and <see cref="Longitude"/> were last changed.
        /// </summary>
        VersionedValue<DateTime?> PositionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the position was calculated by an MLAT server, or came off an MLAT feed.
        /// </summary>
        VersionedValue<bool?> PositionIsMlat { get; set; }

        /// <summary>
        /// Gets or sets the ID of the receiver that supplied the last position for the aircraft.
        /// </summary>
        VersionedValue<Guid?> PositionReceiverId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the last message received for the aircraft came from a TIS-B source.
        /// </summary>
        VersionedValue<bool> IsTisb { get; set; }

        /// <summary>
        /// Gets or sets the last reported heading over the ground of the aircraft. If the aircraft is not
        /// reporting its track (see <see cref="IsTransmittingTrack"/>) then the code will calculate one for it.
        /// This is in degrees clockwise.
        /// </summary>
        /// <remarks>
        /// Tracks are not calculated until the aircraft has moved a small distance.
        /// </remarks>
        VersionedValue<float?> Track { get; set; }

        /// <summary>
        /// Gets or sets the track or heading on the aircraft's autopilot / FMS etc.
        /// </summary>
        VersionedValue<float?> TargetTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Track"/> is the heading of the aircraft. If this
        /// is false then <see cref="Track"/> is the ground track, which is the default.
        /// </summary>
        VersionedValue<bool> TrackIsHeading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is transmitting its track, or at least that BaseStation
        /// is reporting one for it.
        /// </summary>
        bool IsTransmittingTrack { get; set; }

        /// <summary>
        /// Gets or sets the last reported rate of ascent or descent (-ve) of the aircraft in feet per minute.
        /// </summary>
        VersionedValue<int?> VerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the type of vertical rate being transmitted by the aircraft.
        /// </summary>
        VersionedValue<AltitudeType> VerticalRateType { get; set; }

        /// <summary>
        /// Gets or sets the squawk code transmitted by the aircraft.
        /// </summary>
        VersionedValue<int?> Squawk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft's ident is active.
        /// </summary>
        VersionedValue<bool?> IdentActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Squawk"/> represents an emergency code.
        /// </summary>
        VersionedValue<bool?> Emergency { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        /// <remarks>This is whatever is recorded in the database for the registration and may contain non-compliant characters.</remarks>
        VersionedValue<string> Registration { get; set; }

        /// <summary>
        /// Gets the <see cref="Registration"/> with non-compliant characters stripped out.
        /// </summary>
        /// <remarks>It transpires that some people record non-compliant characters in the database <see cref="Registration"/> field for the aircraft. This removes
        /// all non-compliant characters so that only those permitted by annex 7 of the Convention on International Civil Aviation remain.</remarks>
        string IcaoCompliantRegistration { get; }

        /// <summary>
        /// Gets or sets the time that a message from the aircraft was first received in this session.
        /// </summary>
        VersionedValue<DateTime> FirstSeen { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from the aircraft.
        /// </summary>
        /// <remarks>
        /// This is always set regardless of the type of feed that the aircraft message came in on.
        /// </remarks>
        DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from a Mode-S feed for the aircraft.
        /// </summary>
        DateTime LastModeSUpdate { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from a SatCom feed for the aircraft.
        /// </summary>
        DateTime LastSatcomUpdate { get; set; }

        /// <summary>
        /// Gets or sets the number of messages received for the aircraft.
        /// </summary>
        VersionedValue<long> CountMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the ICAO8643 type code for the aircraft.
        /// </summary>
        VersionedValue<string> Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the aircraft's maker.
        /// </summary>
        VersionedValue<string> Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft, as assigned by the manufacturer.
        /// </summary>
        VersionedValue<string> Model { get; set; }

        /// <summary>
        /// Gets or sets the construction number assigned to the aircraft by the manufacturer.
        /// </summary>
        VersionedValue<string> ConstructionNumber { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured as a string (it's a string in BaseStation.sqb).
        /// </summary>
        VersionedValue<string> YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the code for the origin airport.
        /// </summary>
        VersionedValue<string> OriginAirportCode { get; set; }

        /// <summary>
        /// Gets or sets a description of the airport where the aircraft's journey began.
        /// </summary>
        VersionedValue<string> Origin { get; set; }

        /// <summary>
        /// Gets or sets a description of the airport where the aircraft's journey is scheduled to end.
        /// </summary>
        VersionedValue<string> Destination { get; set; }

        /// <summary>
        /// Gets or sets the code of the airport where the aircraft's journey is scheduled to end.
        /// </summary>
        VersionedValue<string> DestinationAirportCode { get; set; }

        /// <summary>
        /// Gets or sets a list of scheduled stops for the aircraft.
        /// </summary>
        VersionedValue<IReadOnlyList<string>> Stopovers { get; }

        /// <summary>
        /// Gets or sets a list of airport codes for the aircraft's scheduled stops.
        /// </summary>
        VersionedValue<IReadOnlyList<string>> StopoverAirportCodes { get; }

        /// <summary>
        /// Gets or sets a value indicating that this is probably a positioning / ferry flight.
        /// </summary>
        VersionedValue<bool> IsPositioningFlight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the this probably a charter flight.
        /// </summary>
        VersionedValue<bool> IsCharterFlight { get; set; }

        /// <summary>
        /// Gets or sets the name of the company or individual that is operating the aircraft.
        /// </summary>
        VersionedValue<string> Operator { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        VersionedValue<string> OperatorIcao { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 ATC wake turbulence category for this type of aircraft.
        /// </summary>
        VersionedValue<WakeTurbulenceCategory> WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 engine type for this type of aircraft.
        /// </summary>
        VersionedValue<EngineType> EngineType { get; set; }

        /// <summary>
        /// Gets or sets an indication of where the engines are mounted on the aircraft.
        /// </summary>
        VersionedValue<EnginePlacement> EnginePlacement { get; set; }

        /// <summary>
        /// Gets or sets the number of engines as per ICAO 8643. Note that this 'number' can include the value C...
        /// </summary>
        VersionedValue<string> NumberOfEngines { get; set; }

        /// <summary>
        /// Gets or sets the type of aircraft as recorded in ICAO 8643.
        /// </summary>
        VersionedValue<Species> Species { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is probably a military aircraft.
        /// </summary>
        VersionedValue<bool> IsMilitary { get; set; }

        /// <summary>
        /// Gets or sets the country that the aircraft's ICAO24 is assigned to.
        /// </summary>
        VersionedValue<string> Icao24Country { get; set; }

        /// <summary>
        /// Gets or sets the full path and filename of the aircraft's picture or null if the aircraft has no picture.
        /// </summary>
        VersionedValue<string> PictureFileName { get; set; }

        /// <summary>
        /// Gets or sets the width of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        VersionedValue<int> PictureWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        VersionedValue<int> PictureHeight { get; set; }

        /// <summary>
        /// Gets or sets the transponder type used by the aircraft.
        /// </summary>
        VersionedValue<TransponderType> TransponderType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataVersion"/> that was current when the coordinates were first started for the aircraft.
        /// </summary>
        long FirstCoordinateChanged { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataVersion"/> that was current when the last coordinate was added for the aircraft.
        /// </summary>
        long LastCoordinateChanged { get; set; }

        /// <summary>
        /// Gets or sets the time of the latest coordinate on the lists.
        /// </summary>
        DateTime LatestCoordinateTime { get; set; }

        /// <summary>
        /// The list of coordinates representing the full track of the aircraft, from when it was first seen until now.
        /// </summary>
        /// <remarks>
        /// Coordinates are only added to this list when the aircraft changes direction
        /// but there is no limit to the number of coordinates held for the aircraft.
        /// </remarks>
        List<Coordinate> FullCoordinates { get; }

        /// <summary>
        /// The list of coordinates representing the short track of the aircraft, the locations reported by it for the past so-many seconds.
        /// </summary>
        /// <remarks>
        /// Coordinates are perpetually added to this list irrespective of the direction
        /// the aircraft is taking but coordinates older than the configuration value
        /// for the short trail duration are removed as snapshots are taken of the
        /// aircraft, or as new coordinates are added.
        /// </remarks>
        List<Coordinate> ShortCoordinates { get; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is flagged as interesting in BaseStation.
        /// </summary>
        VersionedValue<bool> IsInteresting { get; set; }

        /// <summary>
        /// Gets the number of flights that the aircraft has logged for it in the BaseStation database.
        /// </summary>
        VersionedValue<int> FlightsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the vehicle is reporting itself as being on the ground.
        /// </summary>
        VersionedValue<bool?> OnGround { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the callsign came from a possible BDS2,0 message instead of an
        /// ADS-B message.
        /// </summary>
        VersionedValue<bool> CallsignIsSuspect { get; set; }

        /// <summary>
        /// Gets the <see cref="UserNotes"/> from the aircraft's database record.
        /// </summary>
        VersionedValue<string> UserNotes { get; set; }

        /// <summary>
        /// Gets the <see cref="UserTag"/> from the aircraft's database record.
        /// </summary>
        VersionedValue<string> UserTag { get; set; }

        /// <summary>
        /// Removes all coordinates from the aircraft's coordinates lists.
        /// </summary>
        void ResetCoordinates();

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
        void UpdateCoordinates(DateTime utcNow, int shortCoordinateSeconds);
    }
}
