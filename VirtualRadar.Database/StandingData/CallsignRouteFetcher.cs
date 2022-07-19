// Copyright © 2014 onwards, Andrew Whewell
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
using System.Text;
using System.Text.RegularExpressions;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.StandingData
{
    /// <summary>
    /// The default implementation of <see cref="ICallsignRouteFetcher"/>.
    /// </summary>
    class CallsignRouteFetcher : AircraftFetcher<CallsignRouteFetcher.Key, CallsignRouteDetail>, ICallsignRouteFetcher
    {
        /// <summary>
        /// The immutable key used to index registrations of aircraft.
        /// </summary>
        public class Key
        {
            public string Icao24 { get; }
            public string Callsign { get; }
            public string OperatorIcao { get; }

            public Key(IAircraft aircraft)
            {
                Icao24 = aircraft.Icao24?.ToUpperInvariant();
                Callsign = aircraft.Callsign?.ToUpperInvariant();
                OperatorIcao = aircraft.OperatorIcao?.ToUpperInvariant();
            }

            public override string ToString()
            {
                return $"[{Icao24}] Call={Callsign} Op={OperatorIcao}";
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as Key;
                    result = other != null
                          && other.Icao24 == Icao24
                          && other.Callsign == Callsign
                          && other.OperatorIcao == OperatorIcao;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return Icao24?.GetHashCode() ?? 0;
            }
        }

        /// <summary>
        /// The singleton instance of the standing data manager that we're going to use.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// The singleton instance of the parser that produces lists of alternate callsigns to use when fetching routes.
        /// </summary>
        private ICallsignParser _CallsignParser;

        /// <summary>
        /// See interface docs.
        /// </summary>
        protected override int AutomaticDeregisterIntervalMilliseconds
        {
            get { return 90000; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <remarks>
        /// We do not automatically recheck routes once we have established that they do or do not exist.
        /// Unlike BaseStation.sqb records the user is not at liberty to change the SDM records, their
        /// changes will get overwritten on the next fetch of the database from the mothership.
        /// </remarks>
        protected override int AutomaticRecheckIntervalMilliseconds
        {
            get { return 0; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<CallsignRouteDetail>> Fetched;

        /// <summary>
        /// Raises <see cref="Fetched"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFetched(EventArgs<CallsignRouteDetail> args)
        {
            EventHelper.Raise(Fetched, this, args);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing && !Disposed) {
                if(_StandingDataManager != null) {
                    _StandingDataManager.LoadCompleted -= StandingDataManager_LoadCompleted;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialise()
        {
            _StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
            _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;
            _CallsignParser = Factory.Resolve<ICallsignParser>();
            base.DoInitialise();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public CallsignRouteDetail RegisterAircraft(IAircraft aircraft)
        {
            if(aircraft == null) {
                throw new ArgumentNullException("aircraft");
            }
            
            CallsignRouteDetail result = null;
            if(!String.IsNullOrEmpty(aircraft.Callsign)) {
                var key = new Key(aircraft);
                result = DoRegisterAircraft(key, null);
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="fetchedDetail"></param>
        /// <returns></returns>
        protected override CallsignRouteDetail DoFetchAircraft(AircraftFetcher<CallsignRouteFetcher.Key, CallsignRouteDetail>.FetchedDetail fetchedDetail)
        {
            var key = fetchedDetail.Key;
            var detail = fetchedDetail.Detail;

            if(!String.IsNullOrEmpty(key.Icao24) && !String.IsNullOrEmpty(key.Callsign)) {
                var callsignUsed = key.Callsign;
                var parsed = new Callsign(callsignUsed);

                var airlines = _StandingDataManager.FindAirlinesForCode(parsed.Code);
                var isPositioning = airlines.Any(r => r.IsPositioningFlightNumber(parsed.TrimmedNumber));
                var isCharter =     airlines.Any(r => r.IsCharterFlightNumber(parsed.TrimmedNumber));

                Route route = null;
                if(!isPositioning && !isCharter) {
                    var callsigns = _CallsignParser.GetAllRouteCallsigns(key.Callsign, key.OperatorIcao);
                    foreach(var callsign in callsigns) {
                        callsignUsed = callsign;
                        route = _StandingDataManager.FindRoute(callsignUsed);
                        if(route != null) {
                            break;
                        }
                    }
                    if(route == null) {
                        callsignUsed = key.Callsign;
                    }
                }

                detail = new CallsignRouteDetail() {
                    Callsign =              key.Callsign,
                    UsedCallsign =          callsignUsed,
                    Icao24 =                key.Icao24,
                    Route =                 route,
                    IsCharterFlight =       isCharter,
                    IsPositioningFlight =   isPositioning,
                };

                OnFetched(new EventArgs<CallsignRouteDetail>(detail));
            }

            return detail;
        }

        /// <summary>
        /// Called when the standing data has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            RecheckAll(true);
        }
    }
}
