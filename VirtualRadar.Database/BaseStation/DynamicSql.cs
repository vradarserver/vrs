// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using Dapper;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Dynamic BaseStation SQL builder.
    /// </summary>
    public static class DynamicSql
    {
        /// <summary>
        /// The object that can parse callsigns out into alternates for us.
        /// </summary>
        private static ICallsignParser _CallsignParser;

        /// <summary>
        /// Run static initialisers.
        /// </summary>
        static DynamicSql()
        {
            _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
        }

        /// <summary>
        /// Returns the WHERE portion of an SQL statement contains the fields describing the criteria passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public static CriteriaAndProperties GetFlightsCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            var result = new CriteriaAndProperties();
            StringBuilder command = new StringBuilder();

            if(aircraft != null) {
                DynamicSqlBuilder.AddWhereClause(command, "[Flights].[AircraftID]", " = @aircraftID");
                result.Parameters.Add("aircraftID", aircraft.AircraftID);
            }

            if(criteria.UseAlternateCallsigns && criteria.Callsign != null && criteria.Callsign.Condition == FilterCondition.Equals && !String.IsNullOrEmpty(criteria.Callsign.Value)) {
                GetAlternateCallsignCriteria(command, result.Parameters, criteria.Callsign, "[Flights].[Callsign]");
            } else {
                DynamicSqlBuilder.AddCriteria(command, criteria.Callsign, result.Parameters, "[Flights].[Callsign]", "callsign");
            }

            DynamicSqlBuilder.AddCriteria(command, criteria.Date,           result.Parameters, "[Flights].[StartTime]",         "fromStartTime", "toStartTime");
            DynamicSqlBuilder.AddCriteria(command, criteria.Operator,       result.Parameters, "[Aircraft].[RegisteredOwners]", "registeredOwners");
            DynamicSqlBuilder.AddCriteria(command, criteria.Registration,   result.Parameters, "[Aircraft].[Registration]",     "registration");
            DynamicSqlBuilder.AddCriteria(command, criteria.Icao,           result.Parameters, "[Aircraft].[ModeS]",            "icao");
            DynamicSqlBuilder.AddCriteria(command, criteria.Country,        result.Parameters, "[Aircraft].[ModeSCountry]",     "modeSCountry");
            DynamicSqlBuilder.AddCriteria(command, criteria.IsEmergency,                       "[Flights].[HadEmergency]");
            DynamicSqlBuilder.AddCriteria(command, criteria.Type,           result.Parameters, "[Aircraft].[ICAOTypeCode]",     "modelIcao");
            DynamicSqlBuilder.AddCriteria(command, criteria.FirstAltitude,  result.Parameters, "[Flights].[FirstAltitude]",     "fromFirstAltitude", "toFirstAltitude");
            DynamicSqlBuilder.AddCriteria(command, criteria.LastAltitude,   result.Parameters, "[Flights].[LastAltitude]",      "fromLastAltitude", "toLastAltitude");

            result.SqlChunk = command.ToString();
            return result;
        }

        /// <summary>
        /// Translates from the sort field to an SQL field name.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        public static string CriteriaSortFieldToColumnName(string sortField)
        {
            string result = null;
            if(sortField != null) {
                switch(sortField.ToUpperInvariant()) {
                    case "CALLSIGN":        result = "[Flights].[Callsign]"; break;
                    case "DATE":            result = "[Flights].[StartTime]"; break;
                    case "FIRSTALTITUDE":   result = "[Flights].[FirstAltitude]"; break;
                    case "LASTALTITUDE":    result = "[Flights].[LastAltitude]"; break;

                    case "COUNTRY":         result = "[Aircraft].[ModeSCountry]"; break;
                    case "MODEL":           result = "[Aircraft].[Type]"; break;
                    case "TYPE":            result = "[Aircraft].[ICAOTypeCode]"; break;
                    case "OPERATOR":        result = "[Aircraft].[RegisteredOwners]"; break;
                    case "REG":             result = "[Aircraft].[Registration]"; break;
                    case "ICAO":            result = "[Aircraft].[ModeS]"; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Builds up the criteria and properties for all alternate callsigns.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <param name="criteria"></param>
        /// <param name="callsignField"></param>
        private static void GetAlternateCallsignCriteria(StringBuilder command, DynamicParameters parameters, FilterString criteria, string callsignField)
        {
            if(criteria != null && !String.IsNullOrEmpty(criteria.Value)) {
                var alternates = _CallsignParser.GetAllAlternateCallsigns(criteria.Value);
                for(var i = 0;i < alternates.Count;++i) {
                    var isFirst = i == 0;
                    var isLast = i + 1 == alternates.Count;
                    var callsign = alternates[i];
                    var parameterName = String.Format("callsign{0}", i + 1);
                    DynamicSqlBuilder.AddWhereClause(command, callsignField,
                        String.Format(" {0} @{1}", !criteria.ReverseCondition ? "=" : "<>", parameterName),
                        useOR: !isFirst && !criteria.ReverseCondition,
                        openParenthesis: isFirst,
                        closeParenthesis: isLast);
                    parameters.Add(parameterName, callsign);
                }
            }
        }
    }
}
