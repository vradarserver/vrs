// Copyright © 2016 onwards, Andrew Whewell
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
using System.Threading;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of IAirPressureDownloader.
    /// </summary>
    class AirPressureDownloader : IAirPressureDownloader
    {
        /// <summary>
        /// The JSON object returned by the SDM call to fetch weather lookup settings.
        /// </summary>
        class ServerSettings
        {
            public string GetGlobalWeatherUrl { get; set; }
            public int GetGlobalWeatherIntervalMinutes { get; set; }
        }

        /// <summary>
        /// The URL to fetch lookup settings from. This returns a single JSON object as outlined in <see cref="ServerSettings"/>.
        /// </summary>
        private static string SettingsUrl { get; }

        /// <summary>
        /// The server settings last fetched.
        /// </summary>
        private ServerSettings _ServerSettings;

        /// <summary>
        /// The date and time the server settings were last fetched.
        /// </summary>
        private DateTime _ServerSettingsFetchedUtc;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int IntervalMinutes
        {
            get { return _ServerSettings == null ? 30 : _ServerSettings.GetGlobalWeatherIntervalMinutes; }
        }

        /// <summary>
        /// Static ctor.
        /// </summary>
        static AirPressureDownloader()
        {
            var webAddressManager = Factory.ResolveSingleton<IWebAddressManager>();
            SettingsUrl = webAddressManager.RegisterAddress("vrs-weather-lookup-settings", "http://sdm.virtualradarserver.co.uk/Weather/GetWeatherSettings?language={0}");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public AirPressure[] Fetch()
        {
            FetchSettings();

            var request = (HttpWebRequest)HttpWebRequest.Create(_ServerSettings.GetGlobalWeatherUrl);
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            string jsonText = null;
            using(var response = request.GetResponse()) {
                using(var streamReader = new StreamReader(response.GetResponseStream())) {
                    jsonText = streamReader.ReadToEnd();
                }
            }

            return JsonConvert.DeserializeObject<AirPressure[]>(jsonText);
        }

        /// <summary>
        /// Fetches the settings from the server. These are fetched once on startup and then once every hour.
        /// </summary>
        private void FetchSettings()
        {
            if(_ServerSettings == null || _ServerSettingsFetchedUtc.AddHours(1) <= DateTime.UtcNow) {
                var url = String.Format(SettingsUrl, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
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
