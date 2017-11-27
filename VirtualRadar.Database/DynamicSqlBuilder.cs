// Copyright © 2015 onwards, Andrew Whewell
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
using Dapper;
using VirtualRadar.Interface;

namespace VirtualRadar.Database
{
    /// <summary>
    /// A class that can help when building dynamic SQL statements.
    /// </summary>
    public static class DynamicSqlBuilder
    {
        /// <summary>
        /// Adds a where clause to the statement passed across.
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="fieldName"></param>
        /// <param name="condition"></param>
        /// <param name="useOR"></param>
        /// <param name="openParenthesis"></param>
        /// <param name="closeParenthesis"></param>
        public static void AddWhereClause(StringBuilder criteriaText, string fieldName, string condition, bool useOR = false, bool openParenthesis = false, bool closeParenthesis = false)
        {
            if(criteriaText.Length > 0) criteriaText.Append(useOR ? " OR " : " AND ");
            if(openParenthesis) criteriaText.Append('(');
            criteriaText.Append(fieldName);
            criteriaText.Append(condition);
            if(closeParenthesis) criteriaText.Append(')');
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <param name="fieldName"></param>
        /// <param name="startDateParameterName"></param>
        /// <param name="endDateParameterName"></param>
        public static void AddCriteria(StringBuilder criteriaText, FilterRange<DateTime> filter, DynamicParameters parameters, string fieldName, string startDateParameterName, string endDateParameterName)
        {
            HandleDateRangeFilter(filter,
                (bothUsed) => { AddWhereClause(criteriaText, fieldName, String.Format(" {0} @{1}", !filter.ReverseCondition ? ">=" : "<", startDateParameterName), openParenthesis: bothUsed); },
                (bothUsed) => { AddWhereClause(criteriaText, fieldName, String.Format(" {0} @{1}", !filter.ReverseCondition ? "<=" : ">", endDateParameterName), useOR: bothUsed && filter.ReverseCondition, closeParenthesis: bothUsed); }
            );

            HandleDateRangeFilter(filter,
                (bothUsed) => { parameters.Add(startDateParameterName, filter.LowerValue); },
                (bothUsed) => { parameters.Add(endDateParameterName,   filter.UpperValue); }
            );
        }

        public static void HandleDateRangeFilter(FilterRange<DateTime> filter, Action<bool> onLowerValue, Action<bool> onUpperValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Between:
                        var lowerPresent = filter.LowerValue != null && filter.LowerValue.Value.Year != DateTime.MinValue.Year;
                        var upperPresent = filter.UpperValue != null && filter.UpperValue.Value.Year != DateTime.MaxValue.Year;
                        if(lowerPresent) onLowerValue(lowerPresent && upperPresent);
                        if(upperPresent) onUpperValue(lowerPresent && upperPresent);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <param name="fieldName"></param>
        /// <param name="fromParameterName"></param>
        /// <param name="toParameterName"></param>
        public static void AddCriteria(StringBuilder criteriaText, FilterRange<int> filter, DynamicParameters parameters, string fieldName, string fromParameterName, string toParameterName)
        {
            HandleIntRangeFilter(filter,
                (bothUsed) => { AddWhereClause(criteriaText, fieldName, String.Format(" {0} @{1}", !filter.ReverseCondition ? ">=" : "<", fromParameterName), openParenthesis: bothUsed); },
                (bothUsed) => { AddWhereClause(criteriaText, fieldName, String.Format(" {0} @{1}", !filter.ReverseCondition ? "<=" : ">", toParameterName), useOR: bothUsed && filter.ReverseCondition, closeParenthesis: bothUsed); }
            );

            HandleIntRangeFilter(filter,
                (bothUsed) => { parameters.Add(fromParameterName, filter.LowerValue); },
                (bothUsed) => { parameters.Add(toParameterName,   filter.UpperValue); }
            );
        }

        public static void HandleIntRangeFilter(FilterRange<int> filter, Action<bool> onLowerValue, Action<bool> onUpperValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Between:
                        var lowerPresent = filter.LowerValue != null && filter.LowerValue != int.MinValue;
                        var upperPresent = filter.UpperValue != null && filter.UpperValue != int.MaxValue;
                        if(lowerPresent) onLowerValue(lowerPresent && upperPresent);
                        if(upperPresent) onUpperValue(lowerPresent && upperPresent);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <param name="fieldName"></param>
        /// <param name="parameterName"></param>
        public static void AddCriteria(StringBuilder criteriaText, FilterString filter, DynamicParameters parameters, string fieldName, string parameterName)
        {
            HandleStringFilter(filter, (condition, matchNull) => {
                if(matchNull) {
                    if(condition == FilterCondition.Equals) {
                        AddWhereClause(criteriaText, fieldName, String.Format(" IS{0} NULL", filter.ReverseCondition ? " NOT" : ""), openParenthesis: true);
                        AddWhereClause(criteriaText, fieldName, String.Format(" {0} ''", filter.ReverseCondition ? "<>" : "="), useOR: !filter.ReverseCondition, closeParenthesis: true);
                    }
                } else {
                    var conditionText = "";
                    switch(condition) {
                        case FilterCondition.Contains:
                        case FilterCondition.EndsWith:
                        case FilterCondition.StartsWith:    conditionText = String.Format(" {0}LIKE @{1} ESCAPE '/'", filter.ReverseCondition ? "NOT " : "", parameterName); break;
                        case FilterCondition.Equals:        conditionText = String.Format(" {0} @{1}", filter.ReverseCondition ? "<>" : "=", parameterName); break;
                    }
                    AddWhereClause(criteriaText, fieldName, conditionText);
                }
            });

            HandleStringFilter(filter, (condition, matchNull) => {
                if(!matchNull) {
                    var value = filter.Value;
                    var escapedValue = (value ?? "").Replace("%", "/%").Replace("_", "%_");
                    switch(condition) {
                        case FilterCondition.Contains:      value = String.Format("%{0}%", escapedValue); break;
                        case FilterCondition.EndsWith:      value = String.Format("%{0}", escapedValue); break;
                        case FilterCondition.StartsWith:    value = String.Format("{0}%", escapedValue); break;
                    }
                    parameters.Add(parameterName, value);
                }
            });
        }

        private static void HandleStringFilter(FilterString filter, Action<FilterCondition, bool> onValue)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Contains:
                    case FilterCondition.EndsWith:
                    case FilterCondition.Equals:
                    case FilterCondition.StartsWith:    onValue(filter.Condition, String.IsNullOrEmpty(filter.Value)); break;
                    default:                            throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds a single criteria to the statement passed across.
        /// </summary>
        /// <param name="criteriaText"></param>
        /// <param name="filter"></param>
        /// <param name="fieldName"></param>
        public static void AddCriteria(StringBuilder criteriaText, FilterBool filter, string fieldName)
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Equals:    AddWhereClause(criteriaText, fieldName, String.Format(" {0} 0", filter.Value && !filter.ReverseCondition ? "<>" : "=")); break;
                    default:                        throw new NotImplementedException();
                }
            }
        }
    }
}
