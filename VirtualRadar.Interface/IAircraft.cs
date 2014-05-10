// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes everything that is known about an aircraft that is being tracked.
    /// </summary>
    public interface IAircraft : ICloneable
    {
        #region Properties
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
        int ReceiverId { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="ReceiverId"/> was last changed.
        /// </summary>
        long ReceiverIdChanged { get; }

        /// <summary>
        /// Gets or sets the signal level of the last message applied to the aircraft. This is null if the receiver wasn't passing along
        /// signal levels.
        /// </summary>
        int? SignalLevel { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="SignalLevel"/> was last changed.
        /// </summary>
        long SignalLevelChanged { get; }

        /// <summary>
        /// Gets or sets the aircraft's ICAO identifier.
        /// </summary>
        string Icao24 { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Icao24"/> was last changed.
        /// </summary>
        long Icao24Changed { get; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Icao24"/> identifier does not comply with ICAO rules or is known to be a duplicate
        /// of another aircraft's ICAO.
        /// </summary>
        bool Icao24Invalid { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Icao24Invalid"/> was last changed.
        /// </summary>
        long Icao24InvalidChanged { get; }

        /// <summary>
        /// Gets or sets the ATC callsign of the aircraft.
        /// </summary>
        string Callsign { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Callsign"/> was last changed.
        /// </summary>
        long CallsignChanged { get; }

        /// <summary>
        /// Gets or sets the altitde (in feet) that the aircraft last reported.
        /// </summary>
        int? Altitude { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Altitude"/> was last changed.
        /// </summary>
        long AltitudeChanged { get; }

        /// <summary>
        /// Gets or sets the ground (in knots) that the aircraft last reported.
        /// </summary>
        float? GroundSpeed { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="GroundSpeed"/> was last changed.
        /// </summary>
        long GroundSpeedChanged { get; }

        /// <summary>
        /// Gets the last reported latitude of the aircraft.
        /// </summary>
        double? Latitude { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Latitude"/> was last changed.
        /// </summary>
        long LatitudeChanged { get; }

        /// <summary>
        /// Gets the last reported longitude of the aircraft.
        /// </summary>
        double? Longitude { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Longitude"/> was last changed.
        /// </summary>
        long LongitudeChanged { get; }

        /// <summary>
        /// Gets the UTC date and time that <see cref="Latitude"/> and <see cref="Longitude"/> were last changed.
        /// </summary>
        DateTime? PositionTime { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="PositionTime"/> was last changed.
        /// </summary>
        long PositionTimeChanged { get; }

        /// <summary>
        /// Gets or sets the last reported heading over the ground of the aircraft. If the aircraft is not
        /// reporting its track (see <see cref="IsTransmittingTrack"/>) then the code will calculate one for it.
        /// This is in degrees clockwise.
        /// </summary>
        /// <remarks>
        /// Tracks are not calculated until the aircraft has moved a small distance.
        /// </remarks>
        float? Track { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Track"/> was last changed.
        /// </summary>
        long TrackChanged { get; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is transmitting its track, or at least that BaseStation
        /// is reporting one for it.
        /// </summary>
        bool IsTransmittingTrack { get; set; }

        /// <summary>
        /// Gets or sets the last reported rate of ascent or descent (-ve) of the aircraft in feet per minute.
        /// </summary>
        int? VerticalRate { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="VerticalRate"/> was last changed.
        /// </summary>
        long VerticalRateChanged { get; }

        /// <summary>
        /// Gets or sets the squawk code transmitted by the aircraft.
        /// </summary>
        int? Squawk { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Squawk"/> was last changed.
        /// </summary>
        long SquawkChanged { get; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Squawk"/> represents an emergency code.
        /// </summary>
        bool? Emergency { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Emergency"/> was last changed.
        /// </summary>
        long EmergencyChanged { get; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        /// <remarks>This is whatever is recorded in the database for the registration and may contain non-compliant characters.</remarks>
        string Registration { get; set; }

        /// <summary>
        /// Gets the <see cref="Registration"/> with non-compliant characters stripped out.
        /// </summary>
        /// <remarks>It transpires that some people record non-compliant characters in the database <see cref="Registration"/> field for the aircraft. This removes
        /// all non-compliant characters so that only those permitted by annex 7 of the Convention on International Civil Aviation remain.</remarks>
        string IcaoCompliantRegistration { get; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Registration"/> was last changed.
        /// </summary>
        long RegistrationChanged { get; }

        /// <summary>
        /// Gets or sets the time that a message from the aircraft was first received in this session.
        /// </summary>
        DateTime FirstSeen { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="FirstSeen"/> was last changed.
        /// </summary>
        long FirstSeenChanged { get; }

        /// <summary>
        /// Gets or sets the time of the last message received from the aircraft.
        /// </summary>
        DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the number of messages received for the aircraft.
        /// </summary>
        long CountMessagesReceived { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="CountMessagesReceived"/> was last changed.
        /// </summary>
        long CountMessagesReceivedChanged { get; }

        /// <summary>
        /// Gets or sets the ICAO8643 type code for the aircraft.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Type"/> was last changed.
        /// </summary>
        long TypeChanged { get; }

        /// <summary>
        /// Gets or sets the name of the aircraft's maker.
        /// </summary>
        string Manufacturer { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Manufacturer"/> was last changed.
        /// </summary>
        long ManufacturerChanged { get; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft, as assigned by the manufacturer.
        /// </summary>
        string Model { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Model"/> was last changed.
        /// </summary>
        long ModelChanged { get; }

        /// <summary>
        /// Gets or sets the construction number assigned to the aircraft by the manufacturer.
        /// </summary>
        string ConstructionNumber { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="ConstructionNumber"/> was last changed.
        /// </summary>
        long ConstructionNumberChanged { get; }

        /// <summary>
        /// Gets or sets a description of the airport where the aircraft's journey began.
        /// </summary>
        string Origin { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Origin"/> was last changed.
        /// </summary>
        long OriginChanged { get; }

        /// <summary>
        /// Gets or sets a description of the airport where the aircraft's journey is scheduled to end.
        /// </summary>
        string Destination { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Destination"/> was last changed.
        /// </summary>
        long DestinationChanged { get; }

        /// <summary>
        /// Gets or sets a list of scheduled stops for the aircraft.
        /// </summary>
        ICollection<string> Stopovers { get; }

        /// <summary>
        /// Gets or sets the <see cref="DataVersion"/> that was current when <see cref="Stopovers"/> was last changed.
        /// </summary>
        long StopoversChanged { get; set; }

        /// <summary>
        /// Gets or sets the name of the company or individual that is operating the aircraft.
        /// </summary>
        string Operator { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Operator"/> was last changed.
        /// </summary>
        long OperatorChanged { get; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        string OperatorIcao { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="OperatorIcao"/> was last changed.
        /// </summary>
        long OperatorIcaoChanged { get; }

        /// <summary>
        /// Gets or sets the ICAO 8643 ATC wake turbulence category for this type of aircraft.
        /// </summary>
        WakeTurbulenceCategory WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="WakeTurbulenceCategory"/> was last changed.
        /// </summary>
        long WakeTurbulenceCategoryChanged { get; }

        /// <summary>
        /// Gets or sets the ICAO 8643 engine type for this type of aircraft.
        /// </summary>
        EngineType EngineType { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="EngineType"/> was last changed.
        /// </summary>
        long EngineTypeChanged { get; }

        /// <summary>
        /// Gets or sets the number of engines as per ICAO 8643. Note that this 'number' can include the value C...
        /// </summary>
        string NumberOfEngines { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="NumberOfEngines"/> was last changed.
        /// </summary>
        long NumberOfEnginesChanged { get; }

        /// <summary>
        /// Gets or sets the type of aircraft as recorded in ICAO 8643.
        /// </summary>
        Species Species { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Species"/> was last changed.
        /// </summary>
        long SpeciesChanged { get; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is probably a military aircraft.
        /// </summary>
        bool IsMilitary { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="IsMilitary"/> was last changed.
        /// </summary>
        long IsMilitaryChanged { get; }

        /// <summary>
        /// Gets or sets the country that the aircraft's ICAO24 is assigned to.
        /// </summary>
        string Icao24Country { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="Icao24Country"/> was last changed.
        /// </summary>
        long Icao24CountryChanged { get; }

        /// <summary>
        /// Gets or sets the full path and filename of the aircraft's picture or null if the aircraft has no picture.
        /// </summary>
        string PictureFileName { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="PictureFileName"/> was last changed.
        /// </summary>
        long PictureFileNameChanged { get; }

        /// <summary>
        /// Gets or sets the width of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        int PictureWidth { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="PictureWidth"/> was last changed.
        /// </summary>
        long PictureWidthChanged { get; }

        /// <summary>
        /// Gets or sets the height of the picture in pixels. This will be 0 if the aircraft has no picture.
        /// </summary>
        int PictureHeight { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="PictureHeight"/> was last changed.
        /// </summary>
        long PictureHeightChanged { get; }

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
        bool IsInteresting { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="IsInteresting"/> was last changed.
        /// </summary>
        long IsInterestingChanged { get; set; }

        /// <summary>
        /// Gets the number of flights that the aircraft has logged for it in the BaseStation database.
        /// </summary>
        int FlightsCount { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="FlightsCount"/> was last changed.
        /// </summary>
        long FlightsCountChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the vehicle is reporting itself as being on the ground.
        /// </summary>
        bool? OnGround { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="OnGround"/> was last changed.
        /// </summary>
        long OnGroundChanged { get; set; }

        /// <summary>
        /// Gets or sets the type of speed being transmitted by the aircraft.
        /// </summary>
        SpeedType SpeedType { get; set; }

        /// Gets the <see cref="DataVersion"/> that was current when <see cref="SpeedType"/> was last changed.
        long SpeedTypeChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the callsign came from a possible BDS2,0 message instead of an
        /// ADS-B message.
        /// </summary>
        bool CallsignIsSuspect { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="CallsignIsSuspect"/> was last changed.
        /// </summary>
        long CallsignIsSuspectChanged { get; set; }

        /// <summary>
        /// Gets the <see cref="UserTag"/> from the aircraft's database record.
        /// </summary>
        string UserTag { get; set; }

        /// <summary>
        /// Gets the <see cref="DataVersion"/> that was current when <see cref="UserTag"/> was last changed.
        /// </summary>
        long UserTagChanged { get; set; }
        #endregion

        #region Methods
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
        #endregion
    }
}
