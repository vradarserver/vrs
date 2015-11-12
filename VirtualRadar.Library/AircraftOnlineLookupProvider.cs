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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftOnlineLookupProvider"/>.
    /// </summary>
    class AircraftOnlineLookupProvider : IAircraftOnlineLookupProvider
    {
        /// <summary>
        /// The JSON object that carries the settings returned by the server.
        /// </summary>
        class ServerSettings
        {
            public string DataSupplier { get; set; }

            public string SupplierCredits { get; set; }

            public string SupplierUrl { get; set; }

            public int MinSeconds { get; set; }

            public int MaxSeconds { get; set; }

            public int MaxBatchSize { get; set; }
        }

        /// <summary>
        /// The URL to fetch details from. This has to be called with a post whose body contains a single field called
        /// icaos, which is a hyphen-separated string of ICAOs. This replies with a JSON file containing all of the aircraft
        /// that could be found. We need to infer the missing ICAOs ourselves.
        /// </summary>
        private static readonly string ServiceUrl = "http://sdm.virtualradarserver.co.uk/Aircraft/GetAircraftByIcaos";

        /// <summary>
        /// The URL to fetch lookup settings from. This returns a single JSON object as outlined in <see cref="ServerSettings"/>.
        /// </summary>
        private static readonly string SettingsUrl = "http://sdm.virtualradarserver.co.uk/Aircraft/GetAircraftLookupSettings";

        /// <summary>
        /// The server settings last fetched by the provider.
        /// </summary>
        private ServerSettings _ServerSettings;

        /// <summary>
        /// The date and time the server settings were last fetched by the provider.
        /// </summary>
        private DateTime _ServerSettingsFetchedUtc;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MaxBatchSize { get { return _ServerSettings == null ? 10 : _ServerSettings.MaxBatchSize; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MinSecondsBetweenRequests { get { return _ServerSettings == null ? 1 : _ServerSettings.MinSeconds; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MaxSecondsAfterFailedRequest { get { return _ServerSettings == null ? 30 : _ServerSettings.MaxSeconds; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DataSupplier { get { return _ServerSettings == null ? null : _ServerSettings.DataSupplier; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string SupplierCredits { get { return _ServerSettings == null ? null : _ServerSettings.SupplierCredits; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string SupplierWebSiteUrl { get { return _ServerSettings == null ? null : _ServerSettings.SupplierUrl; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void InitialiseSupplierDetails()
        {
            FetchSettings();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="fetchedAircraft"></param>
        /// <param name="missingIcaos"></param>
        /// <returns></returns>
        public bool DoLookup(string[] icaos, IList<AircraftOnlineLookupDetail> fetchedAircraft, IList<string> missingIcaos)
        {
            FetchSettings();

            var formContent = String.Format("icaos={0}", HttpUtility.UrlEncode(String.Join("-", icaos)));
            var formBytes = Encoding.UTF8.GetBytes(formContent);

            var request = (HttpWebRequest)HttpWebRequest.Create(ServiceUrl);
            request.Method = "POST";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = formBytes.Length;
            using(var bodyStream = request.GetRequestStream()) {
                bodyStream.Write(formBytes, 0, formBytes.Length);
            }

            string jsonText = null;
            using(var response = request.GetResponse()) {
                using(var streamReader = new StreamReader(response.GetResponseStream())) {
                    jsonText = streamReader.ReadToEnd();
                }
            }

            var result = !String.IsNullOrEmpty(jsonText);
            if(result) {
                var aircraftDetails = JsonConvert.DeserializeObject<AircraftOnlineLookupDetail[]>(jsonText);
                fetchedAircraft.AddRange(aircraftDetails);
                missingIcaos.AddRange(icaos.Select(r => r.ToUpper()).Except(aircraftDetails.Select(r => r.Icao.ToUpper())));
            }

            return result;
        }

        /// <summary>
        /// Fetches the settings from the server. These are fetched once on startup and then once every hour.
        /// </summary>
        private void FetchSettings()
        {
            if(_ServerSettings == null || _ServerSettingsFetchedUtc.AddHours(1) <= DateTime.UtcNow) {
                var request = (HttpWebRequest)HttpWebRequest.Create(SettingsUrl);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using(var response = request.GetResponse()) {
                    using(var streamReader = new StreamReader(response.GetResponseStream())) {
                        var jsonText = streamReader.ReadToEnd();
                        var settings = JsonConvert.DeserializeObject<ServerSettings>(jsonText);
                        _ServerSettings = settings;
                        _ServerSettingsFetchedUtc = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
