// Copyright © 2022 onwards, Andrew Whewell
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
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IRegistrationPrefixLookup"/>.
    /// </summary>
    /// <remarks>
    /// This fetches a CSV file from the VRS standing data repository to provide all of
    /// the prefixes. It maintains a local cache and refreshes the file roughly once a
    /// day, or at startup.
    /// </remarks>
    class RegistrationPrefixLookup : IRegistrationPrefixLookup
    {
        private bool _Initialised;
        private const string DownloadAddressName = "vrs-reg-prefixes-csv";

        // The address to download the CSV file from.
        private string _DownloadUrl;

        // The parser that extracts values out of the CSV file. It's thread-safe.
        private readonly CsvParser _CsvParser = new CsvParser();

        // Locks writes to maps etc. Always take a reference to the map when reading
        // and lock when overwriting.
        private readonly object _SyncLock = new object();

        // A map of the first letter of a registration to a bucket of details for prefixes
        // that start with that letter. The letter is always upper-case, as are the prefixes.
        private volatile Dictionary<char, RegistrationPrefixDetail[]> _RegistrationFirstLetterToDetailsMap;

        // Times at UTC of the last download attempt (whether successful or not) and the last
        // successful download attempt.
        private DateTime _LastAttemptUtc;
        private DateTime _LastSuccessfulDownloadUtc;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fullRegistration"></param>
        /// <returns></returns>
        public RegistrationPrefixDetail FindDetailForFullRegistration(string fullRegistration)
        {
            Initialise();
            StartDownloadIfOutOfDate();

            var normalisedRegistration = NormaliseRegistration(fullRegistration, removeHyphen: false);

            return FindPrefixDetail(
                normalisedRegistration,
                prefix => prefix.DecodeFullRegex.IsMatch(normalisedRegistration)
            ).FirstOrDefault();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="noHyphenRegistration"></param>
        /// <returns></returns>
        public IList<RegistrationPrefixDetail> FindDetailsForNoHyphenRegistration(string noHyphenRegistration)
        {
            Initialise();
            StartDownloadIfOutOfDate();

            var normalisedRegistration = NormaliseRegistration(noHyphenRegistration, removeHyphen: true);

            return FindPrefixDetail(
                normalisedRegistration,
                prefix => prefix.DecodeNoHyphenRegex.IsMatch(normalisedRegistration)
            );
        }

        private string NormaliseRegistration(string registration, bool removeHyphen)
        {
            var result = (registration ?? "").Trim().ToUpperInvariant();
            if(removeHyphen) {
                result = result.Replace("-", "");
            }

            return result;
        }

        private IList<RegistrationPrefixDetail> FindPrefixDetail(string normalisedRegistration, Func<RegistrationPrefixDetail, bool> predicate)
        {
            IList<RegistrationPrefixDetail> result = null;

            var buckets = _RegistrationFirstLetterToDetailsMap;
            if(buckets != null && normalisedRegistration.Length > 0) {
                if(buckets.TryGetValue(normalisedRegistration[0], out var bucket)) {
                    result = bucket
                        .Where(prefix => normalisedRegistration.StartsWith(prefix.Prefix) && predicate(prefix))
                        .ToArray();
                }
            }

            return result ?? new RegistrationPrefixDetail[0];
        }

        private void Initialise()
        {
            if(!_Initialised) {
                lock(_SyncLock) {
                    if(!_Initialised) {
                        try {
                            RegisterDownloadAddresses();
                            Load();
                        } finally {
                            _Initialised = true;
                        }
                    }
                }
            }
        }

        private void StartDownloadIfOutOfDate()
        {
            var now = DateTime.UtcNow;

            if(_LastSuccessfulDownloadUtc.AddHours(24) <= now) {
                if(_LastAttemptUtc.AddMinutes(1) <= now) {
                    _LastAttemptUtc = now;
                    ThreadPool.QueueUserWorkItem(DownloadOnBackgroundThread);
                }
            }
        }

        private void DownloadOnBackgroundThread(object unusedState)
        {
            try {
                DownloadAndSaveFile();
                Load();
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                try {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine($"Caught exception when downloading registration prefix details: {ex}");
                } catch {
                    // Don't let anything bubble out
                }
            }
        }

        private void DownloadAndSaveFile()
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(_DownloadUrl);
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            using(var response = request.GetResponse()) {
                using(var streamReader = new StreamReader(response.GetResponseStream())) {
                    var content = streamReader.ReadToEnd();
                    if(content.Length > 0) {
                        lock(_SyncLock) {
                            File.WriteAllText(
                                LocalCopyFileName(),
                                content
                            );
                            _LastSuccessfulDownloadUtc = DateTime.UtcNow;
                        }
                    }
                }
            }
        }

        private string LocalCopyFileName()
        {
            return Path.Combine(
                Factory.ResolveSingleton<IConfigurationStorage>().Folder,
                "reg-prefixes.csv"
            );
        }

        private void Load()
        {
            var fullPath = LocalCopyFileName();
            if(File.Exists(fullPath)) {
                string[] contentLines = null;
                lock(_SyncLock) {
                    contentLines = File.ReadAllLines(fullPath);
                }
                ParseContentLines(contentLines);
            }
        }

        private void ParseContentLines(IEnumerable<string> contentLines)
        {
            var regPrefixes = new List<RegistrationPrefixDetail>();
            IDictionary<string, int> headers = null;
            foreach(var line in contentLines) {
                var chunks = _CsvParser.ParseLineToChunks(line);
                if(chunks.Count >= 6) {
                    if(headers == null) {
                        headers = _CsvParser.ExtractOrdinals(chunks);
                    } else {
                        regPrefixes.Add(new RegistrationPrefixDetail(
                            prefix:                 chunks[headers["Prefix"]],
                            countryISO2:            chunks[headers["CountryISO2"]],
                            hasHyphen:              chunks[headers["HasHyphen"]] == "1",
                            decodeFullRegex:        chunks[headers["DecodeFullRegex"]],
                            decodeNoHyphenRegex:    chunks[headers["DecodeNoHyphenRegex"]],
                            formatTemplate:         chunks[headers["FormatTemplate"]]
                        ));
                    }
                }
            }

            var bucketMap = regPrefixes
                .GroupBy(r => r.Prefix[0])
                .ToDictionary(r => r.Key, r => r.ToArray());

            lock(_SyncLock) {
                _RegistrationFirstLetterToDetailsMap = bucketMap;
            }
        }

        private void RegisterDownloadAddresses()
        {
            var webAddressManager = Factory.ResolveSingleton<IWebAddressManager>();
            _DownloadUrl = webAddressManager.RegisterAddress(
                DownloadAddressName,
                "https://raw.githubusercontent.com/vradarserver/standing-data/main/registration-prefixes/schema-01/reg-prefixes.csv"
            );
        }
    }
}
