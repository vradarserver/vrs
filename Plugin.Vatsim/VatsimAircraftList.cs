// Copyright © 2022 onwards, Andrew Whewell
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
using System.Linq;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Plugin.Vatsim.VatsimApiModels;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// An aircraft list that is built from VATSIM pilot states.
    /// </summary>
    class VatsimAircraftList : IAircraftList
    {
        private object _SyncLock = new object();

        private long _DataVersion;

        private volatile Dictionary<int, IAircraft> _Aircraft = new Dictionary<int, IAircraft>();

        private IClock _Clock;

        private IStandingDataManager _StandingDataManager;

        private ISharedConfiguration _SharedConfiguration;

        private IRegistrationPrefixLookup _RegistrationPrefixLookup;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public AircraftListSource Source => AircraftListSource.FlightSimulatorX;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count
        {
            get {
                var aircraftList = _Aircraft;
                return aircraftList.Count;
            }
        }

        private bool _IsTracking;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTracking
        {
            get => _IsTracking;
            private set {
                if(value != IsTracking) {
                    _IsTracking = value;
                    OnTrackingStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CountChanged;

        /// <summary>
        /// Raises <see cref="CountChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCountChanged(EventArgs args) => CountChanged?.Invoke(this, args);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler TrackingStateChanged;

        /// <summary>
        /// Raises <see cref="TrackingStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnTrackingStateChanged(EventArgs args) => TrackingStateChanged?.Invoke(this, args);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args) => ExceptionCaught?.Invoke(this, args);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public VatsimAircraftList()
        {
            _Clock =                    Factory.Resolve<IClock>();
            _StandingDataManager =      Factory.ResolveSingleton<IStandingDataManager>();
            _SharedConfiguration =      Factory.ResolveSingleton<ISharedConfiguration>();
            _RegistrationPrefixLookup = Factory.ResolveSingleton<IRegistrationPrefixLookup>();
        }

        /// <summary>
        /// Finalises an object.
        /// </summary>
        ~VatsimAircraftList()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Stop();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IAircraft FindAircraft(int uniqueId)
        {
            var aircraftList = _Aircraft;
            return aircraftList
                .Values
                .FirstOrDefault(r => r.UniqueId == uniqueId);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            IsTracking = true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            IsTracking = false;
            lock(_SyncLock) {
                var newAircraftList = new Dictionary<int, IAircraft>();
                _Aircraft = newAircraftList;
            }
            IsTracking = false;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="snapshotTimeStamp"></param>
        /// <param name="snapshotDataVersion"></param>
        /// <returns></returns>
        public List<IAircraft> TakeSnapshot(out long snapshotTimeStamp, out long snapshotDataVersion)
        {
            var utcNow = _Clock.UtcNow;
            snapshotTimeStamp = utcNow.Ticks;
            snapshotDataVersion = -1L;

            var result = new List<IAircraft>();
            var config = _SharedConfiguration.Get();
            var timeoutThreshold = utcNow.AddSeconds(-config.BaseStationSettings.DisplayTimeoutSeconds);
            var aircraftList = _Aircraft;
            foreach(var aircraft in aircraftList.Values) {
                if(aircraft.LastModeSUpdate >= timeoutThreshold) {
                    if(aircraft.DataVersion > snapshotDataVersion) {
                        snapshotDataVersion = aircraft.DataVersion;
                    }
                    result.Add((IAircraft)aircraft.Clone());
                }
            }

            return result;
        }

        /// <summary>
        /// Applies VATSIM pilot states to the tracked aircraft.
        /// </summary>
        /// <param name="pilotStates"></param>
        public void ApplyPilotStates(IEnumerable<VatsimDataV3Pilot> pilotStates)
        {
            if(IsTracking && (pilotStates?.Any() ?? false)) {
                lock(_SyncLock) {
                    if(IsTracking) {
                        try {
                            ProcessPilotStateMessages(pilotStates);
                        } catch(ThreadAbortException) {
                            // Ignore these, they will get rethrown automatically and we don't want them logged
                        } catch(Exception ex) {
                            OnExceptionCaught(new EventArgs<Exception>(ex));
                        }
                    }
                }

                OnCountChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Apply global option changes to the aircraft list.
        /// </summary>
        public void ApplyOptions()
        {
            lock(_SyncLock) {
                foreach(var aircraft in _Aircraft.Values) {
                    lock(aircraft) {
                        SetOnGround(aircraft);
                        SetManufacturerAndModelFromType(aircraft, typeData: null, loadTypeDataIfMissing: true);
                        FixRegistrationByExaminingPrefix(aircraft);
                    }
                }
            }
        }

        private void ProcessPilotStateMessages(IEnumerable<VatsimDataV3Pilot> pilotStates)
        {
            var newAircraft = new Dictionary<int, IAircraft>();
            var config = _SharedConfiguration.Get();
            var utcNow = _Clock.UtcNow;

            foreach(var pilot in pilotStates) {
                if(!_Aircraft.TryGetValue(pilot.cid, out var aircraft)) {
                    if(pilot.cid < -1 || pilot.cid > 0) {
                        aircraft = Factory.Resolve<IAircraft>();
                        aircraft.UniqueId = pilot.cid;
                        aircraft.FirstSeen = utcNow;
                        newAircraft.Add(pilot.cid, aircraft);
                    }
                }

                if(aircraft != null) {
                    ApplyPilotStateToAircraft(utcNow, pilot, aircraft, config);
                }
            }

            var removeThreshold = utcNow.AddSeconds(-config.BaseStationSettings.TrackingTimeoutSeconds);
            var removeAircraftCids = _Aircraft
                .Where(kvp => kvp.Value.LastUpdate < removeThreshold)
                .Select(r => r.Key)
                .ToArray();

            if(newAircraft.Count > 0 || removeAircraftCids.Length > 0) {
                var newAircraftList = CollectionHelper.ShallowCopy(_Aircraft);

                foreach(var removeKey in removeAircraftCids) {
                    newAircraftList.Remove(removeKey);
                }
                foreach(var aircraft in newAircraft) {
                    newAircraftList.Add(aircraft.Key, aircraft.Value);
                }

                _Aircraft = newAircraftList;
            }
        }

        private void ApplyPilotStateToAircraft(DateTime utcNow, VatsimDataV3Pilot pilot, IAircraft aircraft, Configuration config)
        {
            lock(aircraft) {
                aircraft.DataVersion =      Interlocked.Increment(ref _DataVersion);
                aircraft.LastModeSUpdate =  utcNow;
                aircraft.LastUpdate =       utcNow;
                aircraft.Latitude =         pilot.latitude;
                aircraft.Longitude =        pilot.longitude;
                aircraft.Altitude =         pilot.altitude;
                aircraft.Track =            pilot.heading;
                aircraft.TransponderType =  TransponderType.Adsb;
                aircraft.GroundSpeed =      pilot.groundspeed;
                aircraft.Squawk =           String.IsNullOrEmpty(pilot.transponder) ? (int?)null : pilot.TransponderValue;
                aircraft.Emergency =        aircraft.Squawk == 7500 || aircraft.Squawk == 7600 || aircraft.Squawk == 7700;
                aircraft.UserTag =          $"[{pilot.cid}] {pilot.name}";
                aircraft.AirPressureInHg =  pilot.qnh_i_hg;
                aircraft.Icao24Country =    pilot.server;

                var operatorIcaoDataVersion = aircraft.OperatorIcaoChanged;
                var registrationDataVersion = aircraft.RegistrationChanged;

                var userNotes = $"{pilot.flight_plan?.route}\r\n{pilot.flight_plan?.remarks}";
                if(aircraft.UserNotes != userNotes) {
                    aircraft.UserNotes = userNotes;
                    var remarks = new VatsimRemarks(pilot.flight_plan?.remarks);
                    aircraft.Registration = remarks.Registration;
                    aircraft.OperatorIcao = remarks.OperatorIcao;
                    var icao24 = remarks.ModeSCode;
                    if(CustomConvert.Icao24(icao24) != -1) {
                        aircraft.Icao24 = icao24;        // <-- not sure whether this is a good idea, it'll lead to non-null / non-empty duplicates and it won't match the unique ID...
                    }
                }

                IEnumerable<Airline> airlinesForOperatorIcao = null;
                if(aircraft.Callsign != pilot.callsign) {
                    aircraft.Callsign = pilot.callsign;
                    var callsign = new Callsign(pilot.callsign);
                    airlinesForOperatorIcao = callsign.IsOriginalCallsignValid
                        ? _StandingDataManager.FindAirlinesForCode(callsign.Code)
                        : null;
                    var hasAirlineForCallsign = airlinesForOperatorIcao?.Any() ?? false;
                    if(hasAirlineForCallsign) {
                        aircraft.OperatorIcao = callsign.Code;
                    } else {
                        aircraft.Registration = aircraft.Callsign;
                    }
                }

                if(operatorIcaoDataVersion != aircraft.OperatorIcaoChanged) {
                    var airlines = airlinesForOperatorIcao
                        ?? _StandingDataManager.FindAirlinesForCode(aircraft.OperatorIcao);
                    aircraft.Operator = airlines.FirstOrDefault()?.Name ?? aircraft.OperatorIcao;
                }

                if(registrationDataVersion != aircraft.RegistrationChanged) {
                    FixRegistrationByExaminingPrefix(aircraft);
                }

                SetOnGround(aircraft);
                FillStandingDataFields(pilot, aircraft, config);

                aircraft.UpdateCoordinates(utcNow, config.GoogleMapSettings.ShortTrailLengthSeconds);
            }
        }

        private void FixRegistrationByExaminingPrefix(IAircraft aircraft)
        {
            var registration = aircraft.Registration?.ToUpperInvariant().Trim();
            var hasHyphen = registration?.Contains('-') ?? false;
            var showInvalidRegistrations = Plugin.Options.ShowInvalidRegistrations;

            if(!String.IsNullOrEmpty(registration) && (!hasHyphen || !showInvalidRegistrations)) {
                var prefixes = _RegistrationPrefixLookup
                    .FindDetailsForNoHyphenRegistration(registration);

                if(prefixes.Count == 0 && !showInvalidRegistrations) {
                    aircraft.Registration = null;
                } else if(prefixes.Count > 0 && !hasHyphen && !prefixes.Any(prefix => !prefix.HasHyphen)) {
                    var bestPrefix = prefixes
                        .OrderBy(r => r.Prefix)
                        .FirstOrDefault();
                    var match = bestPrefix.DecodeNoHyphenRegex.Match(registration);
                    if(match.Success) {     // <-- it really should match! but just in case...
                        aircraft.Registration = bestPrefix.FormatCode(
                            match.Groups["code"].Value
                        );
                    }
                }
            }
        }

        private void SetOnGround(IAircraft aircraft)
        {
            if(!Plugin.Options.AssumeSlowAircraftAreOnGround) {
                aircraft.OnGround = false;
            } else {
                aircraft.OnGround = aircraft.GroundSpeed <= Plugin.Options.SlowAircraftThresholdSpeedKnots;
            }
        }

        private void FillStandingDataFields(VatsimDataV3Pilot pilot, IAircraft aircraft, Configuration config)
        {
            var flightPlan = pilot.flight_plan;
            if(flightPlan != null) {
                lock(aircraft) {
                    if(aircraft.Type != flightPlan.aircraft_short) {
                        aircraft.Type = flightPlan.aircraft_short;

                        var typeData = _StandingDataManager.FindAircraftType(aircraft.Type ?? "");
                        aircraft.EngineType =               typeData?.EngineType ?? EngineType.None;
                        aircraft.EnginePlacement =          typeData?.EnginePlacement ?? EnginePlacement.Unknown;
                        aircraft.NumberOfEngines =          typeData?.Engines;
                        aircraft.Species =                  typeData?.Species ?? Species.None;
                        aircraft.WakeTurbulenceCategory =   typeData?.WakeTurbulenceCategory ?? WakeTurbulenceCategory.None;

                        SetManufacturerAndModelFromType(aircraft, typeData, loadTypeDataIfMissing: false);
                    }

                    var departureAirport = flightPlan.departure ?? "";
                    if(aircraft.OriginAirportCode != departureAirport) {
                        aircraft.OriginAirportCode = departureAirport;
                    }
                    if(!String.IsNullOrEmpty(aircraft.OriginAirportCode) && String.IsNullOrEmpty(aircraft.Origin)) {
                        var airport = _StandingDataManager.FindAirportForCode(aircraft.OriginAirportCode);
                        if(airport != null) {
                            aircraft.Origin = Describe.Airport(airport, config.GoogleMapSettings.PreferIataAirportCodes);
                        }
                    }

                    var arrivalAirport = flightPlan.arrival ?? "";
                    if(aircraft.DestinationAirportCode != arrivalAirport) {
                        aircraft.DestinationAirportCode = arrivalAirport;
                    }
                    if(!String.IsNullOrEmpty(aircraft.DestinationAirportCode) && String.IsNullOrEmpty(aircraft.Destination)) {
                        var airport = _StandingDataManager.FindAirportForCode(aircraft.DestinationAirportCode);
                        if(airport != null) {
                            aircraft.Destination = Describe.Airport(airport, config.GoogleMapSettings.PreferIataAirportCodes);
                        }
                    }
                }
            }
        }

        private void SetManufacturerAndModelFromType(IAircraft aircraft, AircraftType typeData, bool loadTypeDataIfMissing)
        {
            if(!Plugin.Options.InferModelFromModelType) {
                aircraft.Manufacturer = "";
                aircraft.Model = "";
            } else {
                if(typeData == null && loadTypeDataIfMissing) {
                    typeData = _StandingDataManager.FindAircraftType(aircraft.Type ?? "");
                }

                // This is going to be complete garbage quite a lot of the time...
                var manufacturer = typeData?.Manufacturers
                    ?.OrderBy(r => r, StringComparer.InvariantCultureIgnoreCase)
                    .FirstOrDefault()
                    ?? "";
                var model = typeData?.Models
                    ?.OrderBy(r => r, StringComparer.InvariantCultureIgnoreCase)
                    .FirstOrDefault()
                    ?? "";
                var hasModel = !String.IsNullOrEmpty(model);
                aircraft.Manufacturer = manufacturer;
                aircraft.Model = !String.IsNullOrEmpty(manufacturer) && hasModel
                    ? $"{manufacturer} {model}"     // most of VRS expects model to actually be manufacturer + model
                    : hasModel
                        ? model
                        : manufacturer;
            }
        }
    }
}
