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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftSanityChecker"/>.
    /// </summary>
    class AircraftSanityChecker : IAircraftSanityChecker
    {
        #region Private class - TimedValue
        /// <summary>
        /// Records a value and the time at which the value was seen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class TimedValue<T>
        {
            public DateTime Time { get; set; }
            public T Value { get; set; }

            public TimedValue(DateTime time, T value)
            {
                Time = time;
                Value = value;
            }
        }
        #endregion

        #region Private class - ValueHistory
        class ValueHistory<TValue, TNullable>
        {
            // The aircraft's unique ID
            public int AircraftId { get; set; }

            // A history of all recorded values up to the point where the value is considered probably correct.
            // Once the value is correct no further values are added to the history and the list is set to null.
            public List<TimedValue<TValue>> History { get; private set; }

            // True if the last message we saw was a good value
            public bool PreviousValueIsGood { get; set; }

            // True if FirstGoodValue has had a value set for it at some point
            public bool RecordedFirstGoodValue { get; set; }

            // Set to the first value in History once the first good value is seen. The next value checked will
            // wipe this value and it will never be set again.
            public TNullable FirstGoodValue { get; set; }

            // The last value seen for the aircraft
            public TimedValue<TValue> PreviousValue { get; set; }

            public ValueHistory()
            {
                History = new List<TimedValue<TValue>>();
            }

            public void AddValue(TimedValue<TValue> value)
            {
                PreviousValue = value;

                // We don't need to record any more history once the first good value has been seen.
                if(!RecordedFirstGoodValue) History.Add(value);
                else if(History != null)    History = null;
            }

            public void Reset()
            {
                PreviousValue = null;
                PreviousValueIsGood = false;
            }
        }

        class AltitudeHistory : ValueHistory<int, int?>
        {
        }

        class PositionHistory : ValueHistory<GlobalCoordinate, GlobalCoordinate>
        {
        }
        #endregion

        #region Fields
        private const int ResetHistorySeconds = 30;                 // Reset history after 30 seconds
        private const int FlushHistoryMinutes = 10;                 // The number of minutes before an aircraft's history is removed from the tracker entirely.
        private const int MaxClimbSpeedFeetPerSecond = 1200;        // = 72,000' per minute, which should cover everything
        private const int MaxDescendSpeedFeedPerSecond = -1300;     // For now I'll assume that 1300'/second is faster than normal terminal velocity and that military jets can descend a bit quicker than they can climb
        private const double MaxSpeedKilometersPerSecond = 1.0;     // This is 3,600 km/h, just over the top speed of a Lockheed SR-71 Blackbird

        /// <summary>
        /// The date and time at UTC that the maps were last flushed of old entries.
        /// </summary>
        private DateTime _LastFlushTime = DateTime.UtcNow;

        /// <summary>
        /// A map of aircraft identifiers to the history of altitudes seen for the aircraft.
        /// </summary>
        private Dictionary<int, ValueHistory<int, int?>> _AltitudeHistoryMap = new Dictionary<int,ValueHistory<int, int?>>();

        /// <summary>
        /// A lock on the altitude history map.
        /// </summary>
        private object _AltitudeHistoryMapSyncLock = new object();

        /// <summary>
        /// A map of aircraft identifiers to the history of positions seen for the aircraft.
        /// </summary>
        private Dictionary<int, ValueHistory<GlobalCoordinate, GlobalCoordinate>> _PositionHistoryMap = new Dictionary<int,ValueHistory<GlobalCoordinate, GlobalCoordinate>>();

        /// <summary>
        /// A lock on the position history map.
        /// </summary>
        private object _PositionHistoryMapSyncLock = new object();
        #endregion

        #region IsGoodAircraftIcao
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        public bool IsGoodAircraftIcao(string icao)
        {
            var result = !String.IsNullOrEmpty(icao) && icao.Length == 6;
            if(result) {
                if(icao == "000000") result = false;
            }

            return result;
        }
        #endregion

        #region CheckAltitude
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="messageReceived"></param>
        /// <param name="altitude"></param>
        /// <returns></returns>
        public Certainty CheckAltitude(int aircraftId, DateTime messageReceived, int altitude)
        {
            var timedValue = new TimedValue<int>(messageReceived, altitude);
            return CheckValue(aircraftId, timedValue, _AltitudeHistoryMap, _AltitudeHistoryMapSyncLock, CalculateAltitudeCertainty, FindFirstGoodAltitude);
        }

        /// <summary>
        /// Calculates the certainty between two values.
        /// </summary>
        /// <param name="seenGoodAltitude"></param>
        /// <param name="previousAltitude"></param>
        /// <param name="thisAltitude"></param>
        /// <returns></returns>
        private Certainty CalculateAltitudeCertainty(bool seenGoodAltitude, TimedValue<int> previousAltitude, TimedValue<int> thisAltitude)
        {
            var altitudeChange = (double)thisAltitude.Value - (double)previousAltitude.Value;
            var period = (thisAltitude.Time - previousAltitude.Time).TotalSeconds;
            var changePerSecond = altitudeChange / period;

            return (changePerSecond < 0.0 && changePerSecond < MaxDescendSpeedFeedPerSecond) ||
                   (changePerSecond > 0.0 && changePerSecond > MaxClimbSpeedFeetPerSecond)
                   ? seenGoodAltitude ? Certainty.CertainlyWrong 
                                      : Certainty.Uncertain
                   : Certainty.ProbablyRight;
        }

        /// <summary>
        /// Finds the first altitude in the history that is reachable from the very latest.
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        private int? FindFirstGoodAltitude(List<TimedValue<int>> history)
        {
            int? result = null;

            if(history != null) {
                var goodAltitude = history[history.Count - 1];
                foreach(var value in history) {
                    if(CalculateAltitudeCertainty(false, value, goodAltitude) == Certainty.ProbablyRight) {
                        result = value.Value;
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region FirstGoodAltitude
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <returns></returns>
        public int? FirstGoodAltitude(int aircraftId)
        {
            return FirstGoodValue(aircraftId, _AltitudeHistoryMap, _AltitudeHistoryMapSyncLock);
        }
        #endregion

        #region CheckPosition
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="messageReceived"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public Certainty CheckPosition(int aircraftId, DateTime messageReceived, double latitude, double longitude)
        {
            var timedValue = new TimedValue<GlobalCoordinate>(messageReceived, new GlobalCoordinate(latitude, longitude));
            return CheckValue(aircraftId, timedValue, _PositionHistoryMap, _PositionHistoryMapSyncLock, CalculatePositionCertainty, FindFirstGoodPosition);
        }

        /// <summary>
        /// Calcualtes whether we are certain than an aircraft could potentially move between two points in a given span of time.
        /// </summary>
        /// <param name="seenGoodPosition"></param>
        /// <param name="previousPosition"></param>
        /// <param name="thisPosition"></param>
        /// <returns></returns>
        private Certainty CalculatePositionCertainty(bool seenGoodPosition, TimedValue<GlobalCoordinate> previousPosition, TimedValue<GlobalCoordinate> thisPosition)
        {
            var result = Certainty.Uncertain;

            if(thisPosition.Value.Latitude != 0.0 || thisPosition.Value.Longitude != 0.0) {
                var distance = GreatCircleMaths.Distance(previousPosition.Value.Latitude, previousPosition.Value.Longitude, thisPosition.Value.Latitude, thisPosition.Value.Longitude);
                var time = (thisPosition.Time - previousPosition.Time).TotalSeconds;
                var speed = distance / time;

                result = speed <= MaxSpeedKilometersPerSecond ? Certainty.ProbablyRight 
                                                              : seenGoodPosition ? Certainty.CertainlyWrong : Certainty.Uncertain;
            }

            return result;
        }

        /// <summary>
        /// Figures out the first position that we've seen for the aircraft that would chime with the most recently seen position.
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        private GlobalCoordinate FindFirstGoodPosition(List<TimedValue<GlobalCoordinate>> history)
        {
            GlobalCoordinate result = null;

            if(history != null) {
                var goodPosition = history[history.Count - 1];
                foreach(var value in history) {
                    if(CalculatePositionCertainty(false, value, goodPosition) == Certainty.ProbablyRight) {
                        result = value.Value;
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region FirstGoodPosition
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <returns></returns>
        public GlobalCoordinate FirstGoodPosition(int aircraftId)
        {
            return FirstGoodValue(aircraftId, _PositionHistoryMap, _PositionHistoryMapSyncLock);
        }
        #endregion

        #region ResetAircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        public void ResetAircraft(int aircraftId)
        {
            lock(_AltitudeHistoryMapSyncLock) {
                if(_AltitudeHistoryMap.ContainsKey(aircraftId)) _AltitudeHistoryMap.Remove(aircraftId);
            }
            lock(_PositionHistoryMapSyncLock) {
                if(_PositionHistoryMap.ContainsKey(aircraftId)) _PositionHistoryMap.Remove(aircraftId);
            }
        }
        #endregion

        #region CheckValue
        /// <summary>
        /// Does all the work for <see cref="CheckAltitude"/> and <see cref="CheckPosition"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TNullable"></typeparam>
        /// <param name="aircraftId"></param>
        /// <param name="timedValue"></param>
        /// <param name="map"></param>
        /// <param name="mapLock"></param>
        /// <param name="calculateCertainty"></param>
        /// <param name="findFirstValue"></param>
        /// <returns></returns>
        private Certainty CheckValue<TValue, TNullable>(
            int                                                             aircraftId,
            TimedValue<TValue>                                              timedValue,
            Dictionary<int, ValueHistory<TValue, TNullable>>                map,
            object                                                          mapLock,
            Func<bool, TimedValue<TValue>, TimedValue<TValue>, Certainty>   calculateCertainty,
            Func<List<TimedValue<TValue>>, TNullable>                       findFirstValue
        )
        {
            FlushOldEntries();

            lock(mapLock) {
                ValueHistory<TValue, TNullable> valueHistory;
                if(!map.TryGetValue(aircraftId, out valueHistory)) {
                    valueHistory = new ValueHistory<TValue, TNullable>() {
                        AircraftId = aircraftId,
                    };
                    map.Add(aircraftId, valueHistory);
                }

                if(valueHistory.RecordedFirstGoodValue && valueHistory.FirstGoodValue != null) valueHistory.FirstGoodValue = default(TNullable);

                var resetThreshold = timedValue.Time.AddSeconds(-ResetHistorySeconds);
                var resetOnMessage = valueHistory.PreviousValue;
                if(resetOnMessage != null && resetOnMessage.Time <= resetThreshold) valueHistory.Reset();

                var result = Certainty.Uncertain;
                var previousValue = valueHistory.PreviousValue;
                valueHistory.AddValue(timedValue);

                if(previousValue != null) {
                    result = calculateCertainty(valueHistory.PreviousValueIsGood, previousValue, timedValue);
                    if(result != Certainty.ProbablyRight) valueHistory.Reset();
                    else {
                        if(!valueHistory.RecordedFirstGoodValue) {
                            valueHistory.FirstGoodValue = findFirstValue(valueHistory.History);
                            valueHistory.RecordedFirstGoodValue = true;
                        }
                        valueHistory.PreviousValueIsGood = true;
                    }
                }

                return result;
            }
        }
        #endregion

        #region FirstGoodValue
        /// <summary>
        /// Does all of the work for <see cref="FirstGoodAltitude"/> and <see cref="FirstGoodPosition"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TNullable"></typeparam>
        /// <param name="aircraftId"></param>
        /// <param name="map"></param>
        /// <param name="mapLock"></param>
        /// <returns></returns>
        private TNullable FirstGoodValue<TValue, TNullable>(int aircraftId, Dictionary<int, ValueHistory<TValue, TNullable>> map, object mapLock)
        {
            TNullable result = default(TNullable);

            lock(mapLock) {
                ValueHistory<TValue, TNullable> valueHistory;
                if(map.TryGetValue(aircraftId, out valueHistory)) {
                    result = valueHistory.FirstGoodValue;
                }
            }

            return result;
        }
        #endregion

        #region FlushOldEntries
        /// <summary>
        /// Removes old entries from the maps.
        /// </summary>
        private void FlushOldEntries()
        {
            var flushDue = _LastFlushTime.AddMinutes(1);
            if(DateTime.UtcNow >= flushDue) {
                _LastFlushTime = DateTime.UtcNow;

                FlushMap(_AltitudeHistoryMap, _AltitudeHistoryMapSyncLock);
                FlushMap(_PositionHistoryMap, _PositionHistoryMapSyncLock);
            }
        }

        private void FlushMap<TValue, TNullable>(Dictionary<int, ValueHistory<TValue, TNullable>> map, object mapLock)
        {
            var deleteThreshold = DateTime.UtcNow.AddMinutes(-FlushHistoryMinutes);
            lock(mapLock) {
                var deleteKeys = map.Where(r => r.Value.PreviousValue != null && r.Value.PreviousValue.Time <= deleteThreshold)
                                    .Select(r => r.Key)
                                    .ToList();
                foreach(var key in deleteKeys) {
                    map.Remove(key);
                }
            }
        }
        #endregion
    }
}
