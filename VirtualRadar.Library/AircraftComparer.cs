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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="IAircraftComparer"/>.
    /// </summary>
    class AircraftComparer : IAircraftComparer
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<KeyValuePair<string, bool>> SortBy { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Coordinate BrowserLocation { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDictionary<int, double?> PrecalculatedDistances { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftComparer()
        {
            SortBy = new List<KeyValuePair<string, bool>>();
            PrecalculatedDistances = new Dictionary<int, double?>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IAircraft x, IAircraft y)
        {
            int result = Object.ReferenceEquals(x, y) ? 0 : -1;
            if(result != 0) {
                for(int i = result = 0;result == 0 && i < SortBy.Count;++i) {
                    result = CompareColumn(x, y, result, SortBy[i].Key, SortBy[i].Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Compares the values in the sortColumn on the lhs and rhs aircraft and returns their relative sort order.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="result"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortAscending"></param>
        /// <returns></returns>
        private int CompareColumn(IAircraft lhs, IAircraft rhs, int result, string sortColumn, bool sortAscending)
        {
            switch(sortColumn) {
                case AircraftComparerColumn.Altitude:               result = Nullable.Compare(lhs.Altitude, rhs.Altitude); break;
                case AircraftComparerColumn.Callsign:               result = String.Compare(lhs.Callsign, rhs.Callsign); break;
                case AircraftComparerColumn.Destination:            result = String.Compare(lhs.Destination, rhs.Destination); break;
                case AircraftComparerColumn.DistanceFromHere:       result = BrowserLocation == null ? 0 : Nullable.Compare(CalculateDistance(lhs), CalculateDistance(rhs)); break;
                case AircraftComparerColumn.FlightsCount:           result = lhs.FlightsCount - rhs.FlightsCount; break;
                case AircraftComparerColumn.GroundSpeed:            result = Nullable.Compare(lhs.GroundSpeed, rhs.GroundSpeed); break;
                case AircraftComparerColumn.Icao24:                 result = String.Compare(lhs.Icao24, rhs.Icao24); break;
                case AircraftComparerColumn.Icao24Country:          result = String.Compare(lhs.Icao24Country, rhs.Icao24Country); break;
                case AircraftComparerColumn.Model:                  result = String.Compare(lhs.Model, rhs.Model, true); break;
                case AircraftComparerColumn.NumberOfEngines:        result = String.Compare(lhs.NumberOfEngines, rhs.NumberOfEngines); if(result == 0) result = String.Compare(EngineTypeDescription(lhs), EngineTypeDescription(rhs)); break;
                case AircraftComparerColumn.Operator:               result = String.Compare(lhs.Operator, rhs.Operator, true); break;
                case AircraftComparerColumn.OperatorIcao:           result = String.Compare(lhs.OperatorIcao, rhs.OperatorIcao); break;
                case AircraftComparerColumn.Origin:                 result = String.Compare(lhs.Origin, rhs.Origin, true); break;
                case AircraftComparerColumn.Registration:           result = String.Compare(lhs.Registration, rhs.Registration); break;
                case AircraftComparerColumn.Species:                result = String.Compare(SpeciesDescription(lhs), SpeciesDescription(rhs)); break;
                case AircraftComparerColumn.Squawk:                 result = (int)(lhs.Squawk.GetValueOrDefault() - rhs.Squawk.GetValueOrDefault()); break;
                case AircraftComparerColumn.Type:                   result = String.Compare(lhs.Type, rhs.Type); break;
                case AircraftComparerColumn.VerticalRate:           result = Nullable.Compare(lhs.VerticalRate, rhs.VerticalRate); break;
                case AircraftComparerColumn.WakeTurbulenceCategory: result = lhs.WakeTurbulenceCategory - rhs.WakeTurbulenceCategory; break;

                case AircraftComparerColumn.FirstSeen:
                default:                                            result = DateTime.Compare(lhs.FirstSeen, rhs.FirstSeen); break;
            }

            return sortAscending ? result : -result;
        }

        /// <summary>
        /// Returns the distance from the browser to the aircraft. If the aircraft is in the <see cref="PrecalculatedDistances"/> map then
        /// the pre-calculated distance is returned.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private double? CalculateDistance(IAircraft aircraft)
        {
            double? result;
            if(!PrecalculatedDistances.TryGetValue(aircraft.UniqueId, out result)) {
                result = GreatCircleMaths.Distance(BrowserLocation.Latitude, BrowserLocation.Longitude, aircraft.Latitude, aircraft.Longitude);
            }

            return result;
        }

        /// <summary>
        /// Returns the English description of the engine type of the aircraft passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private string EngineTypeDescription(IAircraft aircraft)
        {
            return aircraft.EngineType == EngineType.None ? "" : aircraft.EngineType.ToString();
        }

        /// <summary>
        /// Returns the English description of the species of the aircraft passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private string SpeciesDescription(IAircraft aircraft)
        {
            return aircraft.Species == Species.None ? "" : aircraft.Species.ToString();
        }
    }
}
