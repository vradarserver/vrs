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
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using System.IO;
using System.Web;
using System.Globalization;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The internal base class for all objects that can produce content in response to browser requests
    /// that are received by a server.
    /// </summary>
    abstract class Page
    {
        #region Providers
        /// <summary>
        /// Gets the responder that derivees can use to fill the response with content.
        /// </summary>
        protected IResponder Responder { get; private set; }

        /// <summary>
        /// Gets or sets the website's provider object - used to abstract away parts of the environment in tests.
        /// </summary>
        public IWebSiteProvider Provider { get; set; }
        #endregion

        #region Properties
        protected WebSite _WebSite;

        private static ISharedConfiguration _SharedConfiguration;
        /// <summary>
        /// Gets the shared configuration object for pages that want to use it.
        /// </summary>
        protected ISharedConfiguration SharedConfiguration
        {
            get {
                if(_SharedConfiguration == null) _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
                return _SharedConfiguration;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Page(WebSite webSite)
        {
            _WebSite = webSite;
            Responder = Factory.Resolve<IResponder>();
        }
        #endregion

        #region Query string extraction - QueryString, QueryLong etc.
        /// <summary>
        /// Returns the string associated with the name, optionally converting to uppercase first.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="toUpperCase"></param>
        /// <returns></returns>
        protected string QueryString(RequestReceivedEventArgs args, string name, bool toUpperCase)
        {
            var result = args.QueryString[name];
            if(result != null && toUpperCase) result = result.ToUpper();

            return result;
        }

        /// <summary>
        /// Returns the ICAO associated with the name. Returns null if the ICAO is garbage.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string QueryIcao(RequestReceivedEventArgs args, string name)
        {
            var result = args.QueryString[name];
            if(String.IsNullOrEmpty(result)) result = null;
            else {
                result = result.Trim().ToUpper();
                if(result.Length != 6 || result.LastIndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' }) != 5) {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the bool value associated with the name.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected bool QueryBool(RequestReceivedEventArgs args, string name, bool defaultValue)
        {
            bool result = defaultValue;

            int? value = QueryNInt(args, name);
            if(value != null) result = value == 0 ? false : true;

            return result;
        }

        /// <summary>
        /// Returns the date associated with the name, as parsed using the current region settings.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="assumeUniversalTime"></param>
        /// <param name="stripTimePortion"></param>
        /// <returns></returns>
        protected DateTime QueryDate(RequestReceivedEventArgs args, string name, DateTime defaultValue, bool assumeUniversalTime, bool stripTimePortion)
        {
            DateTime result = defaultValue;
            DateTimeStyles styles = assumeUniversalTime ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal;
            var queryValue = QueryString(args, name, false);
            if(!String.IsNullOrEmpty(queryValue) && !DateTime.TryParse(queryValue, CultureInfo.CurrentCulture, styles, out result)) result = defaultValue;

            return !stripTimePortion ? result : result.Date;
        }

        /// <summary>
        /// Returns the integer value associated with the name or a default value if missing.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="missingValue"></param>
        /// <returns></returns>
        protected int QueryInt(RequestReceivedEventArgs args, string name, int missingValue)
        {
            int result = missingValue;
            var queryValue = QueryString(args, name, false);
            if(!String.IsNullOrEmpty(queryValue) && !int.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) result = missingValue;

            return result;
        }

        /// <summary>
        /// Returns the long value associated with the name or a default value if missing.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="missingValue"></param>
        /// <returns></returns>
        protected long QueryLong(RequestReceivedEventArgs args, string name, long missingValue)
        {
            long result = missingValue;
            var queryValue = QueryString(args, name, false);
            if(!String.IsNullOrEmpty(queryValue) && !long.TryParse(queryValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) result = missingValue;

            return result;
        }

        /// <summary>
        /// Returns the bool? value associated with the name or null if the name is missing.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool? QueryNBool(RequestReceivedEventArgs args, string name)
        {
            bool? result = null;

            int? value = QueryNInt(args, name);
            if(value != null) result = value == 0 ? false : true;

            return result;
        }

        /// <summary>
        /// Returns the date (formatted as yyyy-MM-dd) passed as a query string parameter or null if either the date is not present or is invalid.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected DateTime? QueryNDateTime(RequestReceivedEventArgs args, string name)
        {
            return QueryNDateTime(QueryString(args, name, false));
        }

        /// <summary>
        /// Returns the date (formatted as yyyy-MM-dd) passed as a query string parameter or null if either the date is not present or is invalid.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected DateTime? QueryNDateTime(string text)
        {
            DateTime? result = null;
            if(!String.IsNullOrEmpty(text)) {
                DateTime date;
                if(DateTime.TryParseExact(text, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                    result = date;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the double? value associated with the name or null if there is no value.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected double? QueryNDouble(RequestReceivedEventArgs args, string name)
        {
            return QueryNDouble(QueryString(args, name, false));
        }

        /// <summary>
        /// Returns the double? value associated with the name or null if there is no value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected double? QueryNDouble(string text)
        {
            double? result = null;
            if(!String.IsNullOrEmpty(text)) {
                double value;
                if(double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) result = value;
            }

            return result;
        }

        /// <summary>
        /// Returns the T? value associated with the name or null if there was no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected Nullable<T> QueryNEnum<T>(RequestReceivedEventArgs args, string name)
            where T: struct
        {
            Nullable<T> result = null;

            int? value = QueryNInt(args, name);
            if(value != null) {
                if(Enum.IsDefined(typeof(T), value.Value)) result = (Nullable<T>)Enum.ToObject(typeof(T), value.Value);
            }

            return result;
        }

        /// <summary>
        /// Returns the int? value associated with the name or null if there is no value.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected int? QueryNInt(RequestReceivedEventArgs args, string name)
        {
            return QueryNInt(QueryString(args, name, false));
        }

        /// <summary>
        /// Parses an int? from the value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected int? QueryNInt(string text)
        {
            int? result = null;
            if(!String.IsNullOrEmpty(text)) {
                int value;
                if(int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) result = value;
            }

            return result;
        }
        #endregion

        #region Query string filter extraction - DecodeFilter, DecodeDateRangeFilter etc.
        /// <summary>
        /// Decodes the condition from the name for the filter passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private char DecodeFilter<T>(T filter, string name)
            where T: Filter
        {
            char result = '\0';

            for(var i = name.Length - 2;i < name.Length;++i) {
                var ch = name[i];
                switch(name[i]) {
                    case 'L':
                    case 'U':   filter.Condition = FilterCondition.Between;     result = ch; break;
                    case 'S':   filter.Condition = FilterCondition.StartsWith;  result = ch; break;
                    case 'E':   filter.Condition = FilterCondition.EndsWith;    result = ch; break;
                    case 'C':   filter.Condition = FilterCondition.Contains;    result = ch; break;
                    case 'Q':   filter.Condition = FilterCondition.Equals;      result = ch; break;
                    case 'N':   filter.ReverseCondition = true; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Decodes a date range filter's upper or lower value and condition.
        /// </summary>
        /// <param name="filterRange"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterRange<DateTime> DecodeDateRangeFilter(FilterRange<DateTime> filterRange, string name, string value)
        {
            if(filterRange == null) filterRange = new FilterRange<DateTime>();
            var conditionChar = DecodeFilter(filterRange, name);
            switch(conditionChar) {
                case 'L':   filterRange.LowerValue = QueryNDateTime(value); break;
                case 'U':   filterRange.UpperValue = QueryNDateTime(value); break;
                default:    filterRange.Condition = FilterCondition.Invalid; break;
            }

            return filterRange;
        }

        /// <summary>
        /// Decodes a double range filter's upper or lower value and condition.
        /// </summary>
        /// <param name="filterRange"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterRange<double> DecodeDoubleRangeFilter(FilterRange<double> filterRange, string name, string value)
        {
            if(filterRange == null) filterRange = new FilterRange<double>();
            var conditionChar = DecodeFilter(filterRange, name);
            switch(conditionChar) {
                case 'L':   filterRange.LowerValue = QueryNDouble(value); break;
                case 'U':   filterRange.UpperValue = QueryNDouble(value); break;
                default:    filterRange.Condition = FilterCondition.Invalid; break;
            }

            return filterRange;
        }

        /// <summary>
        /// Decodes an int range filter's upper or lower value and condition.
        /// </summary>
        /// <param name="filterRange"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterRange<int> DecodeIntRangeFilter(FilterRange<int> filterRange, string name, string value)
        {
            if(filterRange == null) filterRange = new FilterRange<int>();
            var conditionChar = DecodeFilter(filterRange, name);
            switch(conditionChar) {
                case 'L':   filterRange.LowerValue = QueryNInt(value); break;
                case 'U':   filterRange.UpperValue = QueryNInt(value); break;
                default:    filterRange.Condition = FilterCondition.Invalid; break;
            }

            return filterRange;
        }

        /// <summary>
        /// Decodes a string filter's value and condition.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterString DecodeStringFilter(string name, string value)
        {
            var result = new FilterString();
            DecodeFilter(result, name);
            result.Value = value;

            return result;
        }

        /// <summary>
        /// Decodes a bool filter's value and condition.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterBool DecodeBoolFilter(string name, string value)
        {
            FilterBool result = null;

            if(!String.IsNullOrEmpty(value)) {
                result = new FilterBool() {
                    Value = value != "0" && !value.Equals("false", StringComparison.OrdinalIgnoreCase)
                };
                DecodeFilter(result, name);
            }

            return result;
        }

        /// <summary>
        /// Decodes an enum filter's value and condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected FilterEnum<T> DecodeEnumFilter<T>(string name, string value)
            where T: struct, IComparable
        {
            var result = new FilterEnum<T>();
            DecodeFilter(result, name);

            var decoded = false;
            if(!String.IsNullOrEmpty(value)) {
                var number = QueryNInt(value);
                if(number != null && Enum.IsDefined(typeof(T), number)) {
                    result.Value = (T)((object)number.Value);
                    decoded = true;
                }
            }

            if(!decoded) result = null;

            return result;
        }
        #endregion

        #region Javascript formatters
        /// <summary>
        /// Formats the nullable value as a string using the invariant culture and plain formatting (no thousands separators etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="nullValue"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        protected virtual string FormatNullable<T>(T? value, string nullValue = null, int decimalPlaces = -1)
            where T: struct
        {
            string result = nullValue;

            if(value != null) {
                var formatString = decimalPlaces < 0 ? "{0}" : String.Format("{{0:F{0}}}", decimalPlaces);
                result = String.Format(CultureInfo.InvariantCulture, formatString, value);
            }

            return result;
        }
        #endregion

        #region Event handlers - HandleRequest, LoadConfiguration
        /// <summary>
        /// If the request represents an address that is known to the site then the response object in the
        /// arguments passed across is filled with the content for that address and args.Handled is set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void HandleRequest(object sender, RequestReceivedEventArgs args)
        {
            if(!args.Handled) args.Handled = DoHandleRequest((IWebServer)sender, args);
        }

        /// <summary>
        /// When overridden by a derivee this examines the args to see whether the request can be handled by the object and then,
        /// if it can, it supplies content for the request.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns>True if the request was handled by the object, false if it was not.</returns>
        protected abstract bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args);

        /// <summary>
        /// Called by the web site when the website is first constructed and whenever a change in configuration is detected.
        /// </summary>
        /// <param name="configuration"></param>
        public void LoadConfiguration(Configuration configuration)
        {
            DoLoadConfiguration(configuration);
        }

        /// <summary>
        /// Can be overridden by derivees to pick up changes to the configuration.
        /// </summary>
        /// <param name="configuration"></param>
        protected virtual void DoLoadConfiguration(Configuration configuration)
        {
            ;
        }
        #endregion
    }
}
