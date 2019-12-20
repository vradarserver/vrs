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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;
using System.ComponentModel;
using VirtualRadar.Interface.PortableBinding;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraft"/>.
    /// </summary>
    class Aircraft : IAircraft
    {
        #region Fields
        /// <summary>
        /// Number of ticks in a second.
        /// </summary>
        private const long TicksPerSecond = 10000000L;

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
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long DataVersion { get; set; }

        private int _ReceiverId;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverId { get { return _ReceiverId; } set { if(value != _ReceiverId) { _ReceiverId = value; ReceiverIdChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ReceiverIdChanged { get; private set; }

        private int? _SignalLevel;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? SignalLevel { get { return _SignalLevel; } set { if(value != _SignalLevel) { _SignalLevel = value; SignalLevelChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long SignalLevelChanged { get; private set; }

        private string _Icao24;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Icao24 { get { return _Icao24; } set { if(value != _Icao24) { _Icao24 = value; Icao24Changed = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long Icao24Changed { get; private set; }

        private bool _Icao24Invalid;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Icao24Invalid { get { return _Icao24Invalid; } set { if(value != _Icao24Invalid) { _Icao24Invalid = value; Icao24InvalidChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long Icao24InvalidChanged { get; private set; }

        private string _Callsign;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Callsign { get { return _Callsign; } set { if(value != _Callsign) { _Callsign = value; CallsignChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CallsignChanged { get; private set; }

        private int? _Altitude;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? Altitude { get { return _Altitude; } set { if(value != _Altitude) { _Altitude = value; AltitudeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AltitudeChanged { get; private set; }

        private AltitudeType _AltitudeType;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public AltitudeType AltitudeType { get { return _AltitudeType; } set { if(value != _AltitudeType) { _AltitudeType = value; AltitudeTypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AltitudeTypeChanged { get; set; }

        private int? _CorrectedAltitude;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? GeometricAltitude { get { return _CorrectedAltitude; } set { if(value != _CorrectedAltitude) { _CorrectedAltitude = value; GeometricAltitudeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long GeometricAltitudeChanged { get; private set; }

        private float? _AirPressureInHg;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public float? AirPressureInHg { get { return _AirPressureInHg; } set { if(value != _AirPressureInHg) { _AirPressureInHg = value; AirPressureInHgChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AirPressureInHgChanged { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime? AirPressureLookedUpUtc { get; set; }

        private int? _TargetAltitude;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? TargetAltitude { get { return _TargetAltitude; } set { if(value != _TargetAltitude) { _TargetAltitude = value; TargetAltitudeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TargetAltitudeChanged { get; private set; }

        private float? _GroundSpeed;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public float? GroundSpeed { get { return _GroundSpeed; } set { if(value != _GroundSpeed) { _GroundSpeed = value; GroundSpeedChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long GroundSpeedChanged { get; private set; }

        private SpeedType _SpeedType;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public SpeedType SpeedType { get { return _SpeedType; } set { if(value != _SpeedType) { _SpeedType = value; SpeedTypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long SpeedTypeChanged { get; set; }

        private double? _Latitude;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public double? Latitude { get { return _Latitude; } set { if(value != _Latitude) { _Latitude = value; LatitudeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long LatitudeChanged { get; private set; }

        private double? _Longitude;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public double? Longitude { get { return _Longitude; } set { if(value != _Longitude) { _Longitude = value; LongitudeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long LongitudeChanged { get; private set; }

        private DateTime? _PositionTime;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime? PositionTime { get { return _PositionTime; } set { if(value != _PositionTime) { _PositionTime = value; PositionTimeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionTimeChanged { get; private set; }

        private bool? _PositionIsMlat;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool? PositionIsMlat { get { return _PositionIsMlat; } set { if(value != _PositionIsMlat) { _PositionIsMlat = value; PositionIsMlatChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionIsMlatChanged { get; private set; }

        private int? _PositionReceiverId;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? PositionReceiverId { get { return _PositionReceiverId; } set { if(value != _PositionReceiverId) { _PositionReceiverId = value; PositionReceiverIdChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionReceiverIdChanged { get; private set; }

        private bool _IsTisb;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTisb { get { return _IsTisb; } set { if(value != _IsTisb) { _IsTisb = value; IsTisbChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IsTisbChanged { get; private set; }

        private float? _Track;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public float? Track { get { return _Track; } set { if(value != _Track) { _Track = value; TrackChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TrackChanged { get; private set; }

        private float? _TargetTrack;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public float? TargetTrack { get { return _TargetTrack; } set { if(value != _TargetTrack) { _TargetTrack = value; TargetTrackChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TargetTrackChanged { get; private set; }

        private bool _TrackIsHeading;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool TrackIsHeading { get { return _TrackIsHeading; } set { if(value != _TrackIsHeading) { _TrackIsHeading = value; TrackIsHeadingChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TrackIsHeadingChanged { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTransmittingTrack { get; set; }

        private int? _VerticalRate;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? VerticalRate { get { return _VerticalRate; } set { if(value != _VerticalRate) { _VerticalRate = value; VerticalRateChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long VerticalRateChanged { get; private set; }

        private AltitudeType _VerticalRateType;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public AltitudeType VerticalRateType { get { return _VerticalRateType; } set { if(value != _VerticalRateType) { _VerticalRateType = value; VerticalRateTypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long VerticalRateTypeChanged { get; set; }

        private int? _Squawk;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int? Squawk { get { return _Squawk; } set { if(value != _Squawk) { _Squawk = value; SquawkChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long SquawkChanged { get; private set; }

        private bool? _IdentActive;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool? IdentActive { get { return _IdentActive; } set { if(value != _IdentActive) { _IdentActive = value; IdentActiveChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IdentActiveChanged { get; private set; }

        private bool? _Emergency;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool? Emergency { get { return _Emergency; } set { if(value != _Emergency) { _Emergency = value; EmergencyChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long EmergencyChanged { get; private set; }

        private string _Registration;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Registration { get { return _Registration; } set { if(value != _Registration) { _Registration = value; RegistrationChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long RegistrationChanged { get; private set; }

        private string _IcaoCompliantRegistration;
        private string _IcaoCompliantRegistrationBasedOnRegistration;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string IcaoCompliantRegistration
        {
            get
            {
                if(_IcaoCompliantRegistrationBasedOnRegistration != Registration) {
                    _IcaoCompliantRegistrationBasedOnRegistration = Registration;
                    _IcaoCompliantRegistration = Describe.IcaoCompliantRegistration(Registration);
                }

                return _IcaoCompliantRegistration;
            }
        }

        private DateTime _FirstSeen;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime FirstSeen { get { return _FirstSeen; } set { if(value != _FirstSeen) { _FirstSeen = value; FirstSeenChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long FirstSeenChanged { get; private set; }

        private long _CountMessagesReceived;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CountMessagesReceived { get { return _CountMessagesReceived; } set { if(value != _CountMessagesReceived) { _CountMessagesReceived = value; CountMessagesReceivedChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CountMessagesReceivedChanged { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime LastModeSUpdate { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime LastSatcomUpdate { get; set; }

        private string _Type;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Type { get { return _Type; } set { if(value != _Type) { _Type = value; TypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TypeChanged { get; private set; }

        private string _Manufacturer;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Manufacturer { get { return _Manufacturer; } set { if(value != _Manufacturer) { _Manufacturer = value; ManufacturerChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ManufacturerChanged { get; private set; }

        private string _Model;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Model { get { return _Model; } set { if(value != _Model) { _Model = value; ModelChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModelChanged { get; private set; }

        private string _ConstructionNumber;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ConstructionNumber { get { return _ConstructionNumber; } set { if(value != _ConstructionNumber) { _ConstructionNumber = value; ConstructionNumberChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ConstructionNumberChanged { get; private set; }

        private string _YearBuilt;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string YearBuilt { get { return _YearBuilt; } set { if(value != _YearBuilt) { _YearBuilt = value; YearBuiltChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long YearBuiltChanged { get; private set; }

        private string _Origin;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Origin { get { return _Origin; } set { if(value != _Origin) { _Origin = value; OriginChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long OriginChanged { get; private set; }

        private string _Destination;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Destination { get { return _Destination; } set { if(value != _Destination) { _Destination = value; DestinationChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long DestinationChanged { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<string> Stopovers { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long StopoversChanged { get; set; }

        private bool _IsPositioningFlight;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsPositioningFlight { get => _IsPositioningFlight; set { if(value != _IsPositioningFlight) { _IsPositioningFlight = value; IsPositioningFlightChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IsPositioningFlightChanged { get; private set; }

        private bool _IsCharterFlight;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsCharterFlight { get => _IsCharterFlight; set { if(value != _IsCharterFlight) { _IsCharterFlight = value; IsCharterFlightChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IsCharterFlightChanged { get; private set; }

        private string _Operator;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Operator { get { return _Operator; } set { if(value != _Operator) { _Operator = value; OperatorChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long OperatorChanged { get; private set; }

        private string _OperatorIcao;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string OperatorIcao { get { return _OperatorIcao; } set { if(value != _OperatorIcao) { _OperatorIcao = value; OperatorIcaoChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long OperatorIcaoChanged { get; private set; }

        private WakeTurbulenceCategory _WakeTurbulenceCategory;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public WakeTurbulenceCategory WakeTurbulenceCategory { get { return _WakeTurbulenceCategory; } set { if(value != _WakeTurbulenceCategory) { _WakeTurbulenceCategory = value; WakeTurbulenceCategoryChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long WakeTurbulenceCategoryChanged { get; private set; }

        private EngineType _EngineType;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public EngineType EngineType { get { return _EngineType; } set { if(value != _EngineType) { _EngineType = value; EngineTypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long EngineTypeChanged { get; private set; }

        private EnginePlacement _EnginePlacement;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public EnginePlacement EnginePlacement { get { return _EnginePlacement; } set { if(value != _EnginePlacement) { _EnginePlacement = value; EnginePlacementChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long EnginePlacementChanged { get; private set; }

        private string _NumberOfEngines;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NumberOfEngines { get { return _NumberOfEngines; } set { if(value != _NumberOfEngines) { _NumberOfEngines = value; NumberOfEnginesChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long NumberOfEnginesChanged { get; private set; }

        private Species _Species { get; set; }
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Species Species { get { return _Species; } set { if(value != _Species) { _Species = value; SpeciesChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long SpeciesChanged { get; private set; }

        private bool _IsMilitary { get; set; }
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsMilitary { get { return _IsMilitary; } set { if(value != _IsMilitary) { _IsMilitary = value; IsMilitaryChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IsMilitaryChanged { get; private set; }

        private string _Icao24Country;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Icao24Country { get { return _Icao24Country; } set { if(value != _Icao24Country) { _Icao24Country = value; Icao24CountryChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long Icao24CountryChanged { get; private set; }

        private string _PictureFileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PictureFileName { get { return _PictureFileName; } set { if(value != _PictureFileName) { _PictureFileName = value; PictureFileNameChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PictureFileNameChanged { get; private set; }

        private int _PictureWidth;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int PictureWidth { get { return _PictureWidth; } set { if(value != _PictureWidth) { _PictureWidth = value; PictureWidthChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PictureWidthChanged { get; private set; }

        private int _PictureHeight;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int PictureHeight { get { return _PictureHeight; } set { if(value != _PictureHeight) { _PictureHeight = value; PictureHeightChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PictureHeightChanged { get; private set; }

        private TransponderType _TransponderType;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public TransponderType TransponderType { get { return _TransponderType; } set { if(value != _TransponderType) { _TransponderType = value; TransponderTypeChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long TransponderTypeChanged { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long FirstCoordinateChanged { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long LastCoordinateChanged { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime LatestCoordinateTime { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<Coordinate> FullCoordinates { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<Coordinate> ShortCoordinates { get; private set; }

        private bool _IsInteresting;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsInteresting { get { return _IsInteresting; } set { if(value != _IsInteresting) { _IsInteresting = value; IsInterestingChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long IsInterestingChanged { get; set; }

        private int _FlightsCount;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int FlightsCount { get { return _FlightsCount; } set { if(value != _FlightsCount) { _FlightsCount = value; FlightsCountChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long FlightsCountChanged { get; set; }

        private bool? _OnGround;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool? OnGround { get { return _OnGround; } set { if(value != _OnGround) { _OnGround = value; OnGroundChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long OnGroundChanged { get; set; }

        private bool _CallsignIsSuspect;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CallsignIsSuspect { get { return _CallsignIsSuspect; } set { if(value != _CallsignIsSuspect) { _CallsignIsSuspect = value; CallsignIsSuspectChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CallsignIsSuspectChanged { get; set; }

        private string _UserTag { get; set; }
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserTag { get { return _UserTag; } set { if(value != _UserTag) { _UserTag = value; UserTagChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long UserTagChanged { get; set; }

        private string _UserNotes { get; set; }
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserNotes { get { return _UserNotes; } set { if(value != _UserNotes) { _UserNotes = value; UserNotesChanged = DataVersion; } } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long UserNotesChanged { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Aircraft()
        {
            var stopOvers = new NotifyList<string>();
            stopOvers.ListChanged += Stopovers_ListChanged;
            Stopovers = stopOvers;

            FullCoordinates = new List<Coordinate>();
            ShortCoordinates = new List<Coordinate>();
        }
        #endregion

        #region Clone
        /// <summary>
        /// See interface docs. Creates a shallow copy - all objects held by the aircraft are immutable so we
        /// don't need to make copies of them.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Aircraft result = new Aircraft();

            lock(this) {
                result.AirPressureInHg = AirPressureInHg;
                result.AirPressureLookedUpUtc = AirPressureLookedUpUtc;
                result.Altitude = Altitude;
                result.AltitudeType = AltitudeType;
                result.Callsign = Callsign;
                result.CallsignIsSuspect = CallsignIsSuspect;
                result.ConstructionNumber = ConstructionNumber;
                result.GeometricAltitude = GeometricAltitude;
                result.CountMessagesReceived = CountMessagesReceived;
                result.FirstCoordinateChanged = FirstCoordinateChanged;
                result.LastCoordinateChanged = LastCoordinateChanged;
                result.FullCoordinates.AddRange(FullCoordinates);
                result.ShortCoordinates.AddRange(ShortCoordinates);
                result.DataVersion = DataVersion;
                result.Destination = Destination;
                result.Emergency = Emergency;
                result.EnginePlacement = EnginePlacement;
                result.EngineType = EngineType;
                result.FirstSeen = FirstSeen;
                result.FlightsCount = FlightsCount;
                result.GroundSpeed = GroundSpeed;
                result.Icao24 = Icao24;
                result.Icao24Country = Icao24Country;
                result.Icao24Invalid = Icao24Invalid;
                result.IdentActive = IdentActive;
                result.IsCharterFlight = IsCharterFlight;
                result.IsInteresting = IsInteresting;
                result.IsMilitary = IsMilitary;
                result.IsPositioningFlight = IsPositioningFlight;
                result.IsTisb = IsTisb;
                result.IsTransmittingTrack = IsTransmittingTrack;
                result.LastUpdate = LastUpdate;
                result.LastModeSUpdate = LastModeSUpdate;
                result.LastSatcomUpdate = LastSatcomUpdate;
                result.LatestCoordinateTime = LatestCoordinateTime;
                result.Latitude = Latitude;
                result.Longitude = Longitude;
                result.Manufacturer = Manufacturer;
                result.Model = Model;
                result.NumberOfEngines = NumberOfEngines;
                result.OnGround = OnGround;
                result.Operator = Operator;
                result.OperatorIcao = OperatorIcao;
                result.Origin = Origin;
                result.PictureFileName = PictureFileName;
                result.PictureHeight = PictureHeight;
                result.PictureWidth = PictureWidth;
                result.PositionIsMlat = PositionIsMlat;
                result.PositionReceiverId = PositionReceiverId;
                result.PositionTime = PositionTime;
                result.ReceiverId = ReceiverId;
                result.Registration = Registration;
                result.SignalLevel = SignalLevel;
                result.Species = Species;
                result.SpeedType = SpeedType;
                result.Squawk = Squawk;
                result.TargetAltitude = TargetAltitude;
                result.TargetTrack = TargetTrack;
                result.Track = Track;
                result.TrackIsHeading = TrackIsHeading;
                result.TransponderType = TransponderType;
                result.Type = Type;
                result.UniqueId = UniqueId;
                result.UserNotes = UserNotes;
                result.UserTag = UserTag;
                result.VerticalRate = VerticalRate;
                result.VerticalRateType = VerticalRateType;
                result.WakeTurbulenceCategory = WakeTurbulenceCategory;
                result.YearBuilt = YearBuilt;

                foreach(var stopover in Stopovers) {
                    result.Stopovers.Add(stopover);
                }

                result.AirPressureInHgChanged = AirPressureInHgChanged;
                result.AltitudeChanged = AltitudeChanged;
                result.AltitudeTypeChanged = AltitudeTypeChanged;
                result.CallsignChanged = CallsignChanged;
                result.CallsignIsSuspectChanged = CallsignIsSuspectChanged;
                result.ConstructionNumberChanged = ConstructionNumberChanged;
                result.GeometricAltitudeChanged = GeometricAltitudeChanged;
                result.CountMessagesReceivedChanged = CountMessagesReceivedChanged;
                result.DestinationChanged = DestinationChanged;
                result.EmergencyChanged = EmergencyChanged;
                result.EnginePlacementChanged = EnginePlacementChanged;
                result.EngineTypeChanged = EngineTypeChanged;
                result.FirstSeenChanged = FirstSeenChanged;
                result.FlightsCountChanged = FlightsCountChanged;
                result.GroundSpeedChanged = GroundSpeedChanged;
                result.Icao24Changed = Icao24Changed;
                result.Icao24CountryChanged = Icao24CountryChanged;
                result.Icao24InvalidChanged = Icao24InvalidChanged;
                result.IdentActiveChanged = IdentActiveChanged;
                result.IsCharterFlightChanged = IsCharterFlightChanged;
                result.IsInterestingChanged = IsInterestingChanged;
                result.IsMilitaryChanged = IsMilitaryChanged;
                result.IsPositioningFlightChanged = IsPositioningFlightChanged;
                result.IsTisbChanged = IsTisbChanged;
                result.LatitudeChanged = LatitudeChanged;
                result.LongitudeChanged = LongitudeChanged;
                result.ManufacturerChanged = ManufacturerChanged;
                result.ModelChanged = ModelChanged;
                result.NumberOfEnginesChanged = NumberOfEnginesChanged;
                result.OnGroundChanged = OnGroundChanged;
                result.OperatorChanged = OperatorChanged;
                result.OperatorIcaoChanged = OperatorIcaoChanged;
                result.OriginChanged = OriginChanged;
                result.PictureFileNameChanged = PictureFileNameChanged;
                result.PictureHeightChanged = PictureHeightChanged;
                result.PictureWidthChanged = PictureWidthChanged;
                result.PositionIsMlatChanged = PositionIsMlatChanged;
                result.PositionReceiverIdChanged = PositionReceiverIdChanged;
                result.PositionTimeChanged = PositionTimeChanged;
                result.ReceiverIdChanged = ReceiverIdChanged;
                result.RegistrationChanged = RegistrationChanged;
                result.SignalLevelChanged = SignalLevelChanged;
                result.SpeciesChanged = SpeciesChanged;
                result.SpeedTypeChanged = SpeedTypeChanged;
                result.SquawkChanged = SquawkChanged;
                result.StopoversChanged = StopoversChanged;
                result.TargetAltitudeChanged = TargetAltitudeChanged;
                result.TargetTrackChanged = TargetTrackChanged;
                result.TrackChanged = TrackChanged;
                result.TrackIsHeadingChanged = TrackIsHeadingChanged;
                result.TransponderTypeChanged = TransponderTypeChanged;
                result.TypeChanged = TypeChanged;
                result.UserNotesChanged = UserNotesChanged;
                result.UserTagChanged = UserTagChanged;
                result.VerticalRateChanged = VerticalRateChanged;
                result.VerticalRateTypeChanged = VerticalRateTypeChanged;
                result.WakeTurbulenceCategoryChanged = WakeTurbulenceCategoryChanged;
                result.YearBuiltChanged = YearBuiltChanged;
            }

            return result;
        }
        #endregion

        #region ResetCoordinates, UpdateCoordinates
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ResetCoordinates()
        {
            lock(this) {
                LatestCoordinateTime = default(DateTime);
                FullCoordinates.Clear();
                ShortCoordinates.Clear();
                FirstCoordinateChanged = 0;
                LastCoordinateChanged = 0;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="utcNow"></param>
        /// <param name="shortCoordinateSeconds"></param>
        public void UpdateCoordinates(DateTime utcNow, int shortCoordinateSeconds)
        {
            if(Latitude != null && Longitude != null) {
                var nowTick = utcNow.Ticks;

                var lastFullCoordinate = FullCoordinates.Count == 0 ? null : FullCoordinates[FullCoordinates.Count - 1];
                var secondLastFullCoordinate = FullCoordinates.Count < 2 ? null : FullCoordinates[FullCoordinates.Count - 2];
                if(lastFullCoordinate == null || Latitude != lastFullCoordinate.Latitude || Longitude != lastFullCoordinate.Longitude || Altitude != lastFullCoordinate.Altitude || GroundSpeed != lastFullCoordinate.GroundSpeed) {
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
                    long lastUpdateTick = lastFullCoordinate == null ? 0 : lastFullCoordinate.Tick;
                    if(nowTick - lastUpdateTick >= TicksPerSecond) {
                        var track = Round.TrackHeading(Track);
                        var altitude = Round.TrackAltitude(Altitude);
                        var groundSpeed = Round.TrackGroundSpeed(GroundSpeed);
                        var coordinate = new Coordinate(DataVersion, nowTick, Latitude.Value, Longitude.Value, track, altitude, groundSpeed);
                        var positionChanged = lastFullCoordinate != null && (coordinate.Latitude != lastFullCoordinate.Latitude || coordinate.Longitude != lastFullCoordinate.Longitude);

                        if(FullCoordinates.Count > 1 &&
                            track != null && lastFullCoordinate.Heading == track && secondLastFullCoordinate.Heading == track &&
                            (!positionChanged || lastFullCoordinate.Altitude == altitude && secondLastFullCoordinate.Altitude == altitude) &&
                            (!positionChanged || lastFullCoordinate.GroundSpeed == groundSpeed && secondLastFullCoordinate.GroundSpeed == groundSpeed)
                        ) {
                            FullCoordinates[FullCoordinates.Count - 1] = coordinate;
                        } else {
                            FullCoordinates.Add(coordinate);
                        }

                        long earliestAllowable = nowTick - (TicksPerSecond * shortCoordinateSeconds);
                        var firstAllowableIndex = ShortCoordinates.FindIndex(c => c.Tick >= earliestAllowable);
                        if(firstAllowableIndex == -1)    ShortCoordinates.Clear();
                        else if(firstAllowableIndex > 0) ShortCoordinates.RemoveRange(0, firstAllowableIndex);
                        ShortCoordinates.Add(coordinate);

                        if(FirstCoordinateChanged == 0) FirstCoordinateChanged = DataVersion;
                        LastCoordinateChanged = DataVersion;
                        LatestCoordinateTime = utcNow;
                    }
                }
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when the stopovers on a route are changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Stopovers_ListChanged(object sender, EventArgs args)
        {
            StopoversChanged = DataVersion;
        }
        #endregion
    }
}
