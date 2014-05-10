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
        #region Private class - Key
        /// <summary>
        /// The immutable key used to index registrations of aircraft.
        /// </summary>
        public class Key
        {
            public string Icao24 { get; private set; }
            public string Callsign { get; private set; }
            public string OperatorIcao { get; private set; }

            public Key(IAircraft aircraft)
            {
                Icao24 = aircraft.Icao24;
                Callsign = aircraft.Callsign;
                OperatorIcao = aircraft.OperatorIcao;
            }

            public override string ToString()
            {
                return String.Format("[{0}] Call={1} Op={2}", Icao24, Callsign, OperatorIcao);
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as Key;
                    result = other != null && other.Icao24 == Icao24 && other.Callsign == Callsign && other.OperatorIcao == OperatorIcao;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return Icao24 == null ? 0 : Icao24.GetHashCode();
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The singleton instance of the standing data manager that we're going to use.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// The singleton instance of the parser that produces lists of alternate callsigns to use when fetching routes.
        /// </summary>
        private ICallsignParser _CallsignParser;
        #endregion

        #region Properties
        private static readonly ICallsignRouteFetcher _Singleton = new CallsignRouteFetcher();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICallsignRouteFetcher Singleton
        {
            get { return _Singleton; }
        }

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
        #endregion

        #region Events
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
            if(Fetched != null) Fetched(this, args);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing && !Disposed) {
                if(_StandingDataManager != null) _StandingDataManager.LoadCompleted -= StandingDataManager_LoadCompleted;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region DoInitialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialise()
        {
            _StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
            _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;
            _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
            base.DoInitialise();
        }
        #endregion

        #region RegisterAircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public CallsignRouteDetail RegisterAircraft(IAircraft aircraft)
        {
            if(aircraft == null) throw new ArgumentNullException("aircraft");
            
            CallsignRouteDetail result = null;
            if(!String.IsNullOrEmpty(aircraft.Callsign)) {
                var key = new Key(aircraft);
                result = DoRegisterAircraft(key, null);
            }

            return result;
        }
        #endregion

        #region DoFetchAircraft
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

                Route route = null;
                var callsigns = _CallsignParser.GetAllRouteCallsigns(key.Callsign, key.OperatorIcao);
                foreach(var callsign in callsigns) {
                    callsignUsed = callsign;
                    route = _StandingDataManager.FindRoute(callsignUsed);
                    if(route != null) break;
                }
                if(route == null) callsignUsed = key.Callsign;

                detail = new CallsignRouteDetail() {
                    Callsign = key.Callsign,
                    UsedCallsign = callsignUsed,
                    Icao24 = key.Icao24,
                    Route = route,
                };

                OnFetched(new EventArgs<CallsignRouteDetail>(detail));
            }

            return detail;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the standing data has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            RecheckAll(true);
        }
        #endregion
    }
}
