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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using System.Text.RegularExpressions;

namespace VirtualRadar.Library.BaseStation
{
    /// <summary>
    /// The default implementation of <see cref="IBaseStationAircraftList"/>.
    /// </summary>
    sealed class BaseStationAircraftList : IBaseStationAircraftList
    {
        #region Private Class - TrackCalculationParameters
        /// <summary>
        /// A private class that records parameters necessary for calculating tracks.
        /// </summary>
        class TrackCalculationParameters
        {
            /// <summary>
            /// Gets or sets the latitude used for the last track calculation.
            /// </summary>
            public double LastLatitude { get; set; }

            /// <summary>
            /// Gets or sets the longitude used for the last track calculation.
            /// </summary>
            public double LastLongitude { get; set; }

            /// <summary>
            /// Gets or sets the track last transmitted for the aircraft.
            /// </summary>
            public float? LastTransmittedTrack { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the transmitted track on ground appears to
            /// have locked to the track as it was when the aircraft was first started up.
            /// </summary>
            /// <remarks>
            /// This problem appears to affect 757-200s. When going from airborne to surface the
            /// SurfacePosition tracks are correct, but when the aircraft is started the tracks
            /// in SurfacePositions lock to the heading the aircraft was in on startup and never
            /// report the correct track until after the aircraft has taken off and landed.
            /// </remarks>
            public bool TrackFrozen { get; set; }

            /// <summary>
            /// Gets or sets the time at UTC when the track was considered to be frozen. Frozen
            /// tracks are expired - some operators continue to transmit messages for many hours
            /// while the aircraft is on the ground; because the track after landing was correct
            /// it will still be considered to be correct once the aircraft taxis to takeoff,
            /// this reset prevents that.
            /// </summary>
            public DateTime TrackFrozenAt { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Number of ticks in a second.
        /// </summary>
        private const long TicksPerSecond = 10000000L;

        /// <summary>
        /// The object that abstracts away the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// True once <see cref="Start"/> has been called. This indicates that all properties are in a good
        /// state. Although properties such as the listener and database can be changed at any time the intention
        /// is that they are configured once and then remain constant over the lifetime of the aircraft list.
        /// </summary>
        private bool _Started;

        /// <summary>
        /// The last DataVersion applied to an aircraft.
        /// </summary>
        private long _DataVersion;

        /// <summary>
        /// A map of unique identifiers to aircraft objects.
        /// </summary>
        private Dictionary<int, IAircraft> _AircraftMap = new Dictionary<int, IAircraft>();

        /// <summary>
        /// The object that fetches aircraft details for us.
        /// </summary>
        private IAircraftDetailFetcher _AircraftDetailFetcher;

        /// <summary>
        /// The object that fetches callsign routes for us.
        /// </summary>
        private ICallsignRouteFetcher _CallsignRouteFetcher;

        /// <summary>
        /// The object that synchronises access to <see cref="_AircraftMap"/>.
        /// </summary>
        private object _AircraftMapLock = new object();

        /// <summary>
        /// The object that synchronises access to the fields that are copied from the current configuration.
        /// </summary>
        private object _ConfigurationLock = new object();

        /// <summary>
        /// The number of seconds of coordinates that are held in the ShortCoordinates list for aircraft.
        /// </summary>
        private int _ShortTrailLengthSeconds;

        /// <summary>
        /// The number of seconds that has to elapse since the last message for an aircraft before <see cref="TakeSnapshot"/>
        /// suppresses it from the returned list.
        /// </summary>
        private int _SnapshotTimeoutSeconds;

        /// <summary>
        /// The number of seconds that has to elapse before old aircraft are removed from the list.
        /// </summary>
        private int _TrackingTimeoutSeconds;

        /// <summary>
        /// A map of aircraft identifiers to the parameters used for calculating its track. This is a parallel list to
        /// <see cref="_AircraftMap"/> and is locked using <see cref="_AircraftMapLock"/>.
        /// </summary>
        private Dictionary<int, TrackCalculationParameters> _CalculatedTrackCoordinates = new Dictionary<int,TrackCalculationParameters>();

        /// <summary>
        /// The time that the last removal of old aircraft from the list was performed.
        /// </summary>
        private DateTime _LastRemoveOldAircraftTime;

        /// <summary>
        /// A copy of the prefer IATA codes setting from the configuration.
        /// </summary>
        private bool _PreferIataAirportCodes;

        /// <summary>
        /// An empty BaseStationAircraft object.
        /// </summary>
        private BaseStationAircraft _EmptyBaseStationAircraft = new BaseStationAircraft();

        /// <summary>
        /// An object that can detect bad altitudes and positions.
        /// </summary>
        private IAircraftSanityChecker _SanityChecker;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public AircraftListSource Source { get { return AircraftListSource.BaseStation; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count
        {
            get
            {
                int result;
                lock(_AircraftMap) result = _AircraftMap.Count;
                return result;
            }
        }

        IListener _Port30003Listener;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IListener Listener
        {
            get { return _Port30003Listener; }
            set
            {
                if(_Port30003Listener != value) {
                    if(_Port30003Listener != null) {
                        _Port30003Listener.Port30003MessageReceived -= BaseStationListener_MessageReceived;
                        _Port30003Listener.SourceChanged -= BaseStationListener_SourceChanged;
                        _Port30003Listener.PositionReset -= BaseStationListener_PositionReset;
                    }
                    _Port30003Listener = value;
                    if(_Port30003Listener != null) {
                        _Port30003Listener.Port30003MessageReceived += BaseStationListener_MessageReceived;
                        _Port30003Listener.SourceChanged += BaseStationListener_SourceChanged;
                        _Port30003Listener.PositionReset += BaseStationListener_PositionReset;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager StandingDataManager { get; set; }

        /// <summary>
        /// See interface docs. Be careful with this property - it can be nulled out by another thread while
        /// it's in use, always take a local copy before use.
        /// </summary>
        public IPolarPlotter PolarPlotter { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>. Note that the class is sealed, hence this is private instead of protected virtual.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            if(ExceptionCaught != null) ExceptionCaught(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CountChanged;

        /// <summary>
        /// Raises <see cref="CountChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnCountChanged(EventArgs args)
        {
            if(CountChanged != null) CountChanged(this, args);
        }
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationAircraftList()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();

            _AircraftDetailFetcher = Factory.Singleton.Resolve<IAircraftDetailFetcher>().Singleton;
            _AircraftDetailFetcher.Fetched += AircraftDetailFetcher_Fetched;
            _CallsignRouteFetcher = Factory.Singleton.Resolve<ICallsignRouteFetcher>().Singleton;
            _CallsignRouteFetcher.Fetched += CallsignRouteFetcher_Fetched;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~BaseStationAircraftList()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalises or disposes of the object. Note that this class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Port30003Listener != null) _Port30003Listener.Port30003MessageReceived -= BaseStationListener_MessageReceived;
                if(_AircraftDetailFetcher != null) _AircraftDetailFetcher.Fetched -= AircraftDetailFetcher_Fetched;
                if(_CallsignRouteFetcher != null) _CallsignRouteFetcher.Fetched -= CallsignRouteFetcher_Fetched;
                if(_SanityChecker != null) _SanityChecker.Dispose();
            }
        }
        #endregion

        #region Start, LoadConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(!_Started) {
                if(Listener == null) throw new InvalidOperationException("You must supply a Port30003 listener before the aircraft list can be started");
                if(StandingDataManager == null) throw new InvalidOperationException("You must supply a standing data manager before the aircraft list can be started");

                LoadConfiguration();

                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

                _SanityChecker = Factory.Singleton.Resolve<IAircraftSanityChecker>();

                Factory.Singleton.Resolve<IHeartbeatService>().Singleton.SlowTick += Heartbeat_SlowTick;
                Factory.Singleton.Resolve<IStandingDataManager>().Singleton.LoadCompleted += StandingDataManager_LoadCompleted;

                _Started = true;
            }
        }

        /// <summary>
        /// Reads all of the important values out of the configuration file.
        /// </summary>
        private void LoadConfiguration()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

            lock(_ConfigurationLock) {
                _ShortTrailLengthSeconds = configuration.GoogleMapSettings.ShortTrailLengthSeconds;
                _SnapshotTimeoutSeconds = configuration.BaseStationSettings.DisplayTimeoutSeconds;
                _TrackingTimeoutSeconds = configuration.BaseStationSettings.TrackingTimeoutSeconds;
                _PreferIataAirportCodes = configuration.GoogleMapSettings.PreferIataAirportCodes;
            }
        }
        #endregion

        #region ProcessMessage, ApplyMessageToAircraft, CalculateTrack
        /// <summary>
        /// Adds information contained within the message to the object held for the aircraft (creating a new object if one
        /// does not already exist).
        /// </summary>
        /// <param name="message"></param>
        private void ProcessMessage(BaseStationMessage message)
        {
            try {
                if(message.MessageType == BaseStationMessageType.Transmission) {
                    var uniqueId = ConvertIcaoToUniqueId(message.Icao24);
                    if(uniqueId != -1) {
                        var now = _Clock.UtcNow;

                        var isInsane = false;
                        var isOnGround = message.OnGround.GetValueOrDefault();
                        var altitudeCertainty = isOnGround ? Certainty.ProbablyRight : Certainty.Uncertain;
                        var positionCertainty = Certainty.Uncertain;
                        if(message.Altitude != null && !isOnGround) {
                            isInsane = (altitudeCertainty = _SanityChecker.CheckAltitude(uniqueId, now, message.Altitude.Value)) == Certainty.CertainlyWrong;
                        }
                        if(!isInsane && message.Latitude != null && message.Longitude != null && (message.Latitude != 0.0 || message.Longitude != 0.0)) {
                            isInsane = (positionCertainty = _SanityChecker.CheckPosition(uniqueId, now, message.Latitude.Value, message.Longitude.Value)) == Certainty.CertainlyWrong;
                        }

                        if(!isInsane) {
                            bool isNewAircraft = false;

                            // This must keep the aircraft map locked until the aircraft is fully formed. This means that we have a lock within a lock,
                            // so care must be taken to avoid deadlocks.
                            lock(_AircraftMapLock) {
                                IAircraft aircraft;
                                isNewAircraft = !_AircraftMap.TryGetValue(uniqueId, out aircraft);
                                if(isNewAircraft) {
                                    aircraft = Factory.Singleton.Resolve<IAircraft>();
                                    aircraft.UniqueId = uniqueId;
                                    _AircraftMap.Add(uniqueId, aircraft);
                                }

                                ApplyMessageToAircraft(message, aircraft, isNewAircraft);
                            }

                            if(altitudeCertainty == Certainty.ProbablyRight && positionCertainty == Certainty.ProbablyRight) {
                                if(PolarPlotter != null) {
                                    PolarPlotter.AddCheckedCoordinate(uniqueId, isOnGround ? 0 : message.Altitude.Value, message.Latitude.Value, message.Longitude.Value);
                                }
                            }

                            if(isNewAircraft) OnCountChanged(EventArgs.Empty);
                        }
                    }
                }
            } catch(Exception ex) {
                Debug.WriteLine(String.Format("BaseStationAircraftList.ProcessMessage caught exception: {0}", ex.ToString()));
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        private void ApplyMessageToAircraft(BaseStationMessage message, IAircraft aircraft, bool isNewAircraft)
        {
            var now = _Clock.UtcNow;

            // We want to retrieve all of the lookups without writing anything to the aircraft. Then all of the values
            // that need changing on the aircraft will be set in one lock with one DataVersion so they're all consistent.

            CodeBlock codeBlock = null;
            if(isNewAircraft) codeBlock = StandingDataManager.FindCodeBlock(message.Icao24);

            bool callsignChanged;
            string operatorIcao;
            lock(aircraft) {  // <-- nothing should be changing Callsign, we're the only thread that writes it, but just in case...
                callsignChanged = !String.IsNullOrEmpty(message.Callsign) && message.Callsign != aircraft.Callsign;
                operatorIcao = aircraft.OperatorIcao;
            }

            var track = CalculateTrack(message, aircraft);

            lock(aircraft) {
                GenerateDataVersion(aircraft);

                aircraft.LastUpdate = now;
                aircraft.ReceiverId = message.ReceiverId;
                aircraft.SignalLevel = message.SignalLevel;
                ++aircraft.CountMessagesReceived;
                if(isNewAircraft) {
                    aircraft.FirstSeen = now;
                    aircraft.TransponderType = TransponderType.ModeS;
                }
                if(aircraft.Icao24 == null) aircraft.Icao24 = message.Icao24;

                if(!String.IsNullOrEmpty(message.Callsign)) aircraft.Callsign = message.Callsign;
                if(message.Altitude != null) aircraft.Altitude = message.Altitude;
                if(message.GroundSpeed != null) aircraft.GroundSpeed = message.GroundSpeed;
                if(track != null) aircraft.Track = track;
                if(message.Track != null && message.Track != 0.0) aircraft.IsTransmittingTrack = true;
                if(message.Latitude != null) aircraft.Latitude = message.Latitude;
                if(message.Longitude != null) aircraft.Longitude = message.Longitude;
                if(message.VerticalRate != null) aircraft.VerticalRate = message.VerticalRate;
                if(message.OnGround != null) aircraft.OnGround = message.OnGround;
                if(message.Squawk != null) {
                    aircraft.Squawk = message.Squawk;
                    aircraft.Emergency = message.Squawk == 7500 || message.Squawk == 7600 || message.Squawk == 7700;
                }

                if(aircraft.TransponderType == TransponderType.ModeS) {
                    if((message.GroundSpeed != null && message.GroundSpeed != 0) ||
                       (message.VerticalRate != null && message.VerticalRate != 0) ||
                       (message.Latitude != null && message.Latitude != 0) ||
                       (message.Longitude != null && message.Longitude != 0) ||
                       (message.Track != null && message.Track != 0)) {
                        aircraft.TransponderType = TransponderType.Adsb;
                    }
                }

                var supplementaryMessage = message != null && message.Supplementary != null ? message.Supplementary : null;
                if(supplementaryMessage != null) {
                    ApplySupplementaryMessage(aircraft, supplementaryMessage);
                }

                var callsignRouteDetail = _CallsignRouteFetcher.RegisterAircraft(aircraft);
                if(isNewAircraft || callsignChanged) {
                    ApplyRoute(aircraft, callsignRouteDetail == null ? null : callsignRouteDetail.Route);
                }

                ApplyCodeBlock(aircraft , codeBlock);

                if(message.Latitude != null && message.Longitude != null) {
                    aircraft.UpdateCoordinates(now, _ShortTrailLengthSeconds);
                }

                var aircraftDetail = _AircraftDetailFetcher.RegisterAircraft(aircraft);
                if(isNewAircraft) {
                    if(aircraftDetail != null) ApplyAircraftDetail(aircraft, aircraftDetail);
                }
            }
        }

        private static void ApplySupplementaryMessage(IAircraft aircraft, BaseStationSupplementaryMessage supplementaryMessage)
        {
            if(supplementaryMessage.AltitudeIsGeometric != null) aircraft.AltitudeType = supplementaryMessage.AltitudeIsGeometric.Value ? AltitudeType.Geometric : AltitudeType.Barometric;
            if(supplementaryMessage.VerticalRateIsGeometric != null) aircraft.VerticalRateType = supplementaryMessage.VerticalRateIsGeometric.Value ? AltitudeType.Geometric : AltitudeType.Barometric;
            if(supplementaryMessage.SpeedType != null) aircraft.SpeedType = supplementaryMessage.SpeedType.Value;
            if(supplementaryMessage.CallsignIsSuspect != null) aircraft.CallsignIsSuspect = supplementaryMessage.CallsignIsSuspect.Value;
            if(supplementaryMessage.TrackIsHeading != null) aircraft.TrackIsHeading = supplementaryMessage.TrackIsHeading.Value;
            if(supplementaryMessage.TargetAltitude != null) aircraft.TargetAltitude = supplementaryMessage.TargetAltitude.Value;
            if(supplementaryMessage.TargetHeading != null) aircraft.TargetTrack = supplementaryMessage.TargetHeading.Value;

            if(supplementaryMessage.TransponderType != null) {
                switch(supplementaryMessage.TransponderType.Value) {
                    case TransponderType.Adsb2:
                        aircraft.TransponderType = TransponderType.Adsb2;
                        break;
                    case TransponderType.Adsb1:
                        if(aircraft.TransponderType != TransponderType.Adsb2) {
                            aircraft.TransponderType = TransponderType.Adsb1;
                        }
                        break;
                    case TransponderType.Adsb0:
                        if(aircraft.TransponderType != TransponderType.Adsb1 && aircraft.TransponderType != TransponderType.Adsb2) {
                            aircraft.TransponderType = TransponderType.Adsb0;
                        }
                        break;
                    case TransponderType.Adsb:
                        if(aircraft.TransponderType == TransponderType.ModeS) {
                            aircraft.TransponderType = TransponderType.Adsb;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Applies the aircraft details to the aircraft passed across. It is assumed that the aircraft has been
        /// locked for update and that the DataVersion is already correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="aircraftDetail"></param>
        private void ApplyAircraftDetail(IAircraft aircraft, AircraftDetail aircraftDetail)
        {
            var baseStationAircraft = aircraftDetail.Aircraft ?? _EmptyBaseStationAircraft;
            var operatorIcao = aircraft.OperatorIcao;

            aircraft.Registration =         aircraftDetail.DatabaseRegistration;
            aircraft.Type =                 aircraftDetail.ModelIcao;
            aircraft.Manufacturer =         baseStationAircraft.Manufacturer;
            aircraft.Model =                aircraftDetail.ModelName;
            aircraft.ConstructionNumber =   baseStationAircraft.SerialNo;
            aircraft.Operator =             aircraftDetail.OperatorName;
            aircraft.OperatorIcao =         aircraftDetail.OperatorIcao;
            aircraft.IsInteresting =        baseStationAircraft.Interested;
            aircraft.UserTag =              baseStationAircraft.UserTag;
            aircraft.FlightsCount =         aircraftDetail.FlightsCount;

            if(operatorIcao != aircraft.OperatorIcao) {
                var callsignRouteDetail = _CallsignRouteFetcher.RegisterAircraft(aircraft);
                if(callsignRouteDetail != null && callsignRouteDetail.Route != null) {
                    ApplyRoute(aircraft, callsignRouteDetail.Route);
                }
            }
            ApplyAircraftPicture(aircraft, aircraftDetail.Picture);
            ApplyAircraftType(aircraft, aircraftDetail.AircraftType);
        }

        /// <summary>
        /// Applies the aircraft's picture details to the aircraft. Assumes that the aircraft has been locked
        /// and that the data version is already correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="pictureDetail"></param>
        private void ApplyAircraftPicture(IAircraft aircraft, PictureDetail pictureDetail)
        {
            var fileName = pictureDetail == null ? null : pictureDetail.FileName;
            var width = pictureDetail == null ? 0 : pictureDetail.Width;
            var height = pictureDetail == null ? 0 : pictureDetail.Height;

            aircraft.PictureFileName = fileName;
            aircraft.PictureWidth = width;
            aircraft.PictureHeight = height;
        }

        /// <summary>
        /// Applies the code block to the aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="codeBlock"></param>
        private static void ApplyCodeBlock(IAircraft aircraft, CodeBlock codeBlock)
        {
            if(codeBlock != null) {
                aircraft.Icao24Country = codeBlock.Country;
                aircraft.IsMilitary = codeBlock.IsMilitary;
            }
        }

        /// <summary>
        /// Applies route data to the aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="route"></param>
        private void ApplyRoute(IAircraft aircraft, Route route)
        {
            var origin = route == null ? null : Describe.Airport(route.From, _PreferIataAirportCodes);
            var destination = route == null ? null : Describe.Airport(route.To, _PreferIataAirportCodes);
            var stopovers = new List<string>();
            if(route != null) {
                foreach(var stopover in route.Stopovers) {
                    stopovers.Add(Describe.Airport(stopover, _PreferIataAirportCodes));
                }
            }

            if(aircraft.Origin != origin)           aircraft.Origin = origin;
            if(aircraft.Destination != destination) aircraft.Destination = destination;
            if(!aircraft.Stopovers.SequenceEqual(stopovers)) {
                aircraft.Stopovers.Clear();
                foreach(var stopover in stopovers) {
                    aircraft.Stopovers.Add(stopover);
                }
            }
        }

        /// <summary>
        /// If the message contains a track then it is simply returned, otherwise if it's possible to calculate the track then
        /// it is calculated and returned.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private float? CalculateTrack(BaseStationMessage message, IAircraft aircraft)
        {
            var result = message.Track;

            var onGround = message.OnGround.GetValueOrDefault();
            var positionPresent = message.Latitude != null && message.Longitude != null;
            var trackNeverTransmitted = !aircraft.IsTransmittingTrack;

            if(result == 0.0 && trackNeverTransmitted) result = null;

            if(positionPresent && (onGround || trackNeverTransmitted)) {
                TrackCalculationParameters calcParameters;
                lock(_AircraftMapLock) {
                    _CalculatedTrackCoordinates.TryGetValue(aircraft.UniqueId, out calcParameters);
                    if(calcParameters != null && onGround && calcParameters.TrackFrozenAt.AddMinutes(30) <= _Clock.UtcNow) {
                        _CalculatedTrackCoordinates.Remove(aircraft.UniqueId);
                        calcParameters = null;
                    }
                }

                var trackSuspect = message.Track == null || message.Track == 0.0;
                var trackFrozen = onGround && (calcParameters == null || calcParameters.TrackFrozen);
                if(trackSuspect || trackFrozen) {
                    var trackCalculated = false;
                    if(calcParameters == null) {
                        calcParameters = new TrackCalculationParameters() { LastLatitude = message.Latitude.Value, LastLongitude = message.Longitude.Value, LastTransmittedTrack = message.Track, TrackFrozen = true, TrackFrozenAt = _Clock.UtcNow };
                        lock(_AircraftMapLock) _CalculatedTrackCoordinates.Add(aircraft.UniqueId, calcParameters);
                        trackCalculated = true;
                    } else if(message.Latitude != calcParameters.LastLatitude || message.Longitude != calcParameters.LastLongitude) {
                        if(trackFrozen && onGround && calcParameters.LastTransmittedTrack != message.Track) {
                            trackFrozen = calcParameters.TrackFrozen = false;
                        }
                        if(trackSuspect || trackFrozen) {
                            var minimumDistanceKm = message.OnGround.GetValueOrDefault() ? 0.010 : 0.25;
                            if(GreatCircleMaths.Distance(message.Latitude, message.Longitude, calcParameters.LastLatitude, calcParameters.LastLongitude).GetValueOrDefault() >= minimumDistanceKm) {
                                result = (float?)GreatCircleMaths.Bearing(calcParameters.LastLatitude, calcParameters.LastLongitude, message.Latitude, message.Longitude, null, false, false);
                                result = Round.Track(result);
                                calcParameters.LastLatitude = message.Latitude.Value;
                                calcParameters.LastLongitude = message.Longitude.Value;
                                trackCalculated = true;
                            }
                            calcParameters.LastTransmittedTrack = message.Track;
                        }
                    }
                    if(!trackCalculated && (trackSuspect || trackFrozen)) result = aircraft.Track;
                }
            }

            return result;
        }
        #endregion

        #region ConvertIcaoToUniqueId, GenerateDataVersion
        /// <summary>
        /// Derives the unique identifier for the aircraft from an ICAO24 code.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        private int ConvertIcaoToUniqueId(string icao)
        {
            int uniqueId = -1;
            if(_SanityChecker.IsGoodAircraftIcao(icao)) {
                try {
                    uniqueId = Convert.ToInt32(icao, 16);
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("BaseStationAircraftList.ConvertIcaoToUniqueId caught exception {0}", ex.ToString()));
                }
            }

            return uniqueId;
        }

        /// <summary>
        /// Sets a valid DataVersion. Always lock the aircraft map before calling this.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <remarks>
        /// The tests call for this to be based on UtcNow, the idea being that if the webServer is quit and restarted while
        /// a browser is connected the chances are good that the browser data version will correspond with the data versions
        /// held by the new instance of the webServer. Not sure how useful that will be but it's not hard to do. However there
        /// are times when we cannot use UTC - if the clock gets reset or if two messages come in on the same tick. When that
        /// happens we just fallback to incrementing the dataversion.
        /// </remarks>
        private void GenerateDataVersion(IAircraft aircraft)
        {
            // This can be called on any of the threads that update the list. The _DataVersionLock value is really a property of
            // the aircraft map - it represents the highest possible DataVersion of any aircraft within the map. The caller should
            // have established a lock on the aircraft map before calling us to ensure that snapshots of the map are not taken until
            // all changes that involve a new DataVersion have been applied, but just in case they don't we get it again here. If
            // we don't, and if they forgot to acquire the lock, then we could get inconsistencies.
            lock(_AircraftMapLock) {
                lock(aircraft) {
                    var dataVersion = _Clock.UtcNow.Ticks;
                    if(dataVersion <= _DataVersion) dataVersion = _DataVersion + 1;
                    aircraft.DataVersion = _DataVersion = dataVersion;
                }
            }
        }
        #endregion

        #region FindAircraft, TakeSnapshot
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IAircraft FindAircraft(int uniqueId)
        {
            IAircraft result = null;

            lock(_AircraftMapLock) {
                IAircraft aircraft;
                if(_AircraftMap.TryGetValue(uniqueId, out aircraft)) {
                    lock(aircraft) {
                        result = (IAircraft)aircraft.Clone();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="snapshotTimeStamp"></param>
        /// <param name="snapshotDataVersion"></param>
        /// <returns></returns>
        public List<IAircraft> TakeSnapshot(out long snapshotTimeStamp, out long snapshotDataVersion)
        {
            snapshotTimeStamp = _Clock.UtcNow.Ticks;
            snapshotDataVersion = -1L;

            long hideThreshold;
            lock(_ConfigurationLock) {
                hideThreshold = snapshotTimeStamp - (_SnapshotTimeoutSeconds * TicksPerSecond);
            }

            List<IAircraft> result = new List<IAircraft>();
            lock(_AircraftMapLock) {
                foreach(var aircraft in _AircraftMap.Values) {
                    if(aircraft.LastUpdate.Ticks < hideThreshold) continue;
                    if(aircraft.DataVersion > snapshotDataVersion) snapshotDataVersion = aircraft.DataVersion;
                    result.Add((IAircraft)aircraft.Clone());
                }
            }

            return result;
        }
        #endregion

        #region ApplyAircraftType, RefreshCodeBlocks
        /// <summary>
        /// Applies the aircraft type details to the aircraft. It is assumed that the aircraft has been
        /// locked and the data version is correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="typeDetails"></param>
        private static void ApplyAircraftType(IAircraft aircraft, AircraftType typeDetails)
        {
            aircraft.NumberOfEngines = typeDetails == null ? null : typeDetails.Engines;
            aircraft.EngineType = typeDetails == null ? EngineType.None : typeDetails.EngineType;
            aircraft.Species = typeDetails == null ? Species.None : typeDetails.Species;
            aircraft.WakeTurbulenceCategory = typeDetails == null ? WakeTurbulenceCategory.None : typeDetails.WakeTurbulenceCategory;
        }

        /// <summary>
        /// Refreshes code block information.
        /// </summary>
        private void RefreshCodeBlocks()
        {
            var standingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;

            lock(_AircraftMapLock) {
                foreach(var aircraft in _AircraftMap.Values) {
                    var codeBlock = standingDataManager.FindCodeBlock(aircraft.Icao24);
                    if(codeBlock != null && (aircraft.Icao24Country != codeBlock.Country || aircraft.IsMilitary != codeBlock.IsMilitary)) {
                        lock(aircraft) {
                            GenerateDataVersion(aircraft);
                            ApplyCodeBlock(aircraft, codeBlock);
                        }
                    }
                }
            }
        }
        #endregion

        #region RemoveOldAircraft, ResetAircraftList, 
        /// <summary>
        /// Removes aircraft that have not been seen for a while.
        /// </summary>
        private void RemoveOldAircraft()
        {
            var removeList = new List<int>();

            lock(_AircraftMapLock) {
                var threshold = _Clock.UtcNow.Ticks - (_TrackingTimeoutSeconds * TicksPerSecond);

                foreach(var aircraft in _AircraftMap.Values) {
                    if(aircraft.LastUpdate.Ticks < threshold) removeList.Add(aircraft.UniqueId);
                }

                foreach(var uniqueId in removeList) {
                    var aircraft = _AircraftMap[uniqueId];
                    _AircraftMap.Remove(uniqueId);

                    if(_CalculatedTrackCoordinates.ContainsKey(uniqueId)) _CalculatedTrackCoordinates.Remove(uniqueId);
                }
            }

            if(removeList.Count > 0) OnCountChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes all of the aircraft in the aircraft list.
        /// </summary>
        private void ResetAircraftList()
        {
            lock(_AircraftMapLock) {
                _AircraftMap.Clear();
                _CalculatedTrackCoordinates.Clear();
            }

            OnCountChanged(EventArgs.Empty);
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Raised when the aircraft detail fetcher fetches an aircraft's record from the database, or
        /// detects that the details have been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftDetailFetcher_Fetched(object sender, EventArgs<AircraftDetail> args)
        {
            try {
                var aircraftDetail = args.Value;
                var uniqueId = ConvertIcaoToUniqueId(aircraftDetail.Icao24);
                if(uniqueId != -1) {
                    lock(_AircraftMapLock) {
                        IAircraft aircraft;
                        if(_AircraftMap.TryGetValue(uniqueId, out aircraft)) {
                            lock(aircraft) {
                                GenerateDataVersion(aircraft);
                                ApplyAircraftDetail(aircraft, aircraftDetail);
                            }
                        }
                    }
                }
            } catch(ThreadAbortException) {
                // Ignore these, they get rethrown automatically by .NET
            } catch(Exception ex) {
                // We shouldn't get an exception :) But if we do we want it to go through the normal
                // exception handling mechanism.
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Raised when the callsign route fetcher indicates that a route has been loaded or changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CallsignRouteFetcher_Fetched(object sender, EventArgs<CallsignRouteDetail> args)
        {
            try {
                var callsignRouteDetail = args.Value;
                var uniqueId = ConvertIcaoToUniqueId(callsignRouteDetail.Icao24);
                if(uniqueId != -1) {
                    lock(_AircraftMapLock) {
                        IAircraft aircraft;
                        if(_AircraftMap.TryGetValue(uniqueId, out aircraft) && aircraft.Callsign == callsignRouteDetail.Callsign) {
                            lock(aircraft) {
                                GenerateDataVersion(aircraft);
                                ApplyRoute(aircraft, callsignRouteDetail.Route);
                            }
                        }
                    }
                }
            } catch(ThreadAbortException) {
                // Ignore these, they get rethrown automatically by .NET
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Raised by <see cref="IListener"/> when a message is received from BaseStation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(_Started) ProcessMessage(args.Message);
        }

        /// <summary>
        /// Raised by <see cref="IListener"/> when the listener's source of feed data is changing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_SourceChanged(object sender, EventArgs args)
        {
            ResetAircraftList();
        }

        /// <summary>
        /// Raised by the <see cref="IListener"/> when the listener detects that the positions up to
        /// now are not correct and need to be thrown away.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_PositionReset(object sender, EventArgs<string> args)
        {
            var key = ConvertIcaoToUniqueId(args.Value);
            _SanityChecker.ResetAircraft(key);

            lock(_AircraftMapLock) {
                IAircraft aircraft;
                if(_AircraftMap.TryGetValue(key, out aircraft)) {
                    aircraft.ResetCoordinates();
                }
            }
        }

        /// <summary>
        /// Raised when the user changes the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Periodically raised on a background thread by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            var now = _Clock.UtcNow;

            // Remove old aircraft once every ten minutes. We don't have test coverage for this because we cannot
            // observe the effect - taking a snapshot of the aircraft list also removes old aircraft. This is just
            // a failsafe to prevent a buildup of objects when no-one is using the website.
            if(_LastRemoveOldAircraftTime.AddSeconds(_TrackingTimeoutSeconds) <= now) {
                _LastRemoveOldAircraftTime = now;
                RemoveOldAircraft();
            }
        }

        /// <summary>
        /// Raised after the standing data has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            RefreshCodeBlocks();
        }
        #endregion
    }
}
