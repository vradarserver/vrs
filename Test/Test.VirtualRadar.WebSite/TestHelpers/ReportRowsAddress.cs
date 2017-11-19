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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.WebSite.TestHelpers
{
    /// <summary>
    /// Copy of old V2 report JSON test helper.
    /// </summary>
    class ReportRowsAddress
    {
        public string Page { get { return "/ReportRows.json"; } }
        public string Report { get; set; }
        public int? FromRow { get; set; }
        public int? ToRow { get; set; }
        public string SortField1 { get; set; }
        public string SortField2 { get; set; }
        public bool? SortAscending1 { get; set; }
        public bool? SortAscending2 { get; set; }
        public bool UseAlternativeCallsigns { get; set; }

        public DateFilter Date { get; set; }
        public StringFilter Icao24 { get; set; }
        public StringFilter Registration { get; set; }
        public StringFilter Callsign { get; set; }
        public BoolFilter<bool> IsEmergency { get; set; }
        public BoolFilter<bool> IsMilitary { get; set; }
        public StringFilter Operator { get; set; }
        public StringFilter Country { get; set; }
        public BoolFilter<WakeTurbulenceCategory> WakeTurbulenceCategory { get; set; }
        public BoolFilter<Species> Species { get; set; }
        public StringFilter Type { get; set; }
        public NumericFilter<int> FirstAltitude { get; set; }
        public NumericFilter<int> LastAltitude { get; set; }

        public ReportRowsAddress()
        {
        }

        public string Address
        {
            get
            {
                var queryValues = new Dictionary<string,string>();

                AddQueryString(queryValues, "rep", Report);
                AddQueryString(queryValues, "fromRow", FromRow);
                AddQueryString(queryValues, "toRow", ToRow);
                AddQueryString(queryValues, "sort1", SortField1);
                AddQueryString(queryValues, "sort2", SortField2);
                AddQueryString(queryValues, "sort1dir", SortAscending1, (bool? value) => { return value.Value ? "asc" : "desc"; } );
                AddQueryString(queryValues, "sort2dir", SortAscending2, (bool? value) => { return value.Value ? "asc" : "desc"; } );
                AddQueryString(queryValues, "altCall", UseAlternativeCallsigns ? "1" : "0");

                if(Date != null)                    Date.AddQueryValues("date-", queryValues);
                if(Icao24 != null)                  Icao24.AddQueryValues("icao-", queryValues);
                if(Registration != null)            Registration.AddQueryValues("reg-", queryValues);
                if(Callsign != null)                Callsign.AddQueryValues("call-", queryValues);
                if(IsEmergency != null)             IsEmergency.AddQueryValues("emg-", queryValues);
                if(IsMilitary != null)              IsMilitary.AddQueryValues("mil-", queryValues);
                if(Operator != null)                Operator.AddQueryValues("op-", queryValues);
                if(Country != null)                 Country.AddQueryValues("cou-", queryValues);
                if(WakeTurbulenceCategory != null)  WakeTurbulenceCategory.AddQueryValues("wtc-", queryValues);
                if(Species != null)                 Species.AddQueryValues("spc-", queryValues);
                if(Type != null)                    Type.AddQueryValues("typ-", queryValues);
                if(FirstAltitude != null)           FirstAltitude.AddQueryValues("falt-", queryValues);
                if(LastAltitude != null)            LastAltitude.AddQueryValues("lalt-", queryValues);

                var queryString = new StringBuilder();
                foreach(var kvp in queryValues) {
                    queryString.AppendFormat("{0}{1}={2}", queryString.Length == 0 ? "?" : "&", kvp.Key, HttpUtility.UrlEncode(kvp.Value));
                }

                return String.Format("{0}{1}", Page, queryString);
            }
        }

        private void AddQueryString<T>(Dictionary<string, string> queryValues, string key, T value, Func<T, string> toString = null)
        {
            if(value != null) queryValues.Add(key, toString == null ? value.ToString() : toString(value));
        }

        public void SetFromWorksheet(string propertyName, string queryValue)
        {
            switch(propertyName) {
                case "FromDate":
                    if(Date == null) Date = new DateFilter(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) Date.Lower = DateTime.ParseExact("yyyy-MM-dd", queryValue, CultureInfo.InvariantCulture);
                    break;
                case "ToDate":
                    if(Date == null) Date = new DateFilter(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) Date.Upper = DateTime.ParseExact("yyyy-MM-dd", queryValue, CultureInfo.InvariantCulture);
                    break;
                case "FromFirstAltitude":
                    if(FirstAltitude == null) FirstAltitude = new NumericFilter<int>(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) FirstAltitude.Lower = int.Parse(queryValue, CultureInfo.InvariantCulture);
                    break;
                case "ToFirstAltitude":
                    if(FirstAltitude == null) FirstAltitude = new NumericFilter<int>(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) FirstAltitude.Upper = int.Parse(queryValue, CultureInfo.InvariantCulture);
                    break;
                case "FromLastAltitude":
                    if(LastAltitude == null) LastAltitude = new NumericFilter<int>(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) LastAltitude.Lower = int.Parse(queryValue, CultureInfo.InvariantCulture);
                    break;
                case "ToLastAltitude":
                    if(LastAltitude == null) LastAltitude = new NumericFilter<int>(null, null, false);
                    if(!String.IsNullOrEmpty(queryValue)) LastAltitude.Upper = int.Parse(queryValue, CultureInfo.InvariantCulture);
                    break;
                case "Icao24":                  Icao24 = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                case "Registration":            Registration = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                case "Callsign":                Callsign = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                case "IsEmergency":             IsEmergency = new BoolFilter<bool>(queryValue == "True", false); break;
                case "IsMilitary":              IsMilitary = new BoolFilter<bool>(queryValue == "True", false); break;
                case "Operator":                Operator = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                case "Country":                 Country = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                case "WakeTurbulenceCategory":  WakeTurbulenceCategory = new BoolFilter<WakeTurbulenceCategory>((WakeTurbulenceCategory)Enum.Parse(typeof(WakeTurbulenceCategory), queryValue), false); break;
                case "Species":                 Species = new BoolFilter<Species>((Species)Enum.Parse(typeof(Species), queryValue), false); break;
                case "Type":                    Type = new StringFilter(queryValue, FilterCondition.Equals, false); break;
                default:                        throw new NotImplementedException($"Unknown property name {propertyName}");
            }
        }
    }
}
