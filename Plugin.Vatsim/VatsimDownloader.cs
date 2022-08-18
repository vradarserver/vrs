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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Plugin.Vatsim.VatsimApiModels;

namespace VirtualRadar.Plugin.Vatsim
{
    static class VatsimDownloader
    {
        private static System.Timers.Timer _Timer;

        private static object _SyncLock = new object();

        private static Status _VatsimStatus;

        private static bool _Started;
        public static bool Started => _Started && _VatsimStatus != null;

        public static int RefreshIntervalMilliseconds => Math.Min(1000, Plugin.Options.RefreshIntervalSeconds * 1000);

        public static EventHandler StartedChanged;

        private static void OnStartedChanged(EventArgs args) => StartedChanged?.Invoke(null, args);

        public static EventHandler<EventArgs<VatsimDataV3>> DataDownloaded;

        private static void OnDataDownloaded(EventArgs<VatsimDataV3> args) => DataDownloaded?.Invoke(null, args);

        public static int DownloadIntervalMilliseconds
        {
            get => (int)_Timer.Interval;
            set {
                if(_Timer.Interval != value) {
                    _Timer.Interval = value;
                }
            }
        }

        static VatsimDownloader()
        {
            _Timer = new System.Timers.Timer() {
                AutoReset = false,
                Interval = RefreshIntervalMilliseconds,
                Enabled = false,
            };
            _Timer.Elapsed += Timer_Elapsed;
        }

        public static void Start()
        {
            if(!_Started) {
                _Started = true;
                _Timer.Interval = RefreshIntervalMilliseconds;
                _Timer.Enabled = true;
                DownloadFromVatsimOnBackgroundThread();
                OnStartedChanged(EventArgs.Empty);
            }
        }

        public static void Stop()
        {
            if(_Started) {
                _Started = false;
                _Timer.Enabled = false;
                lock(_SyncLock) {
                    _VatsimStatus = null;
                }
                OnStartedChanged(EventArgs.Empty);
            }
        }

        private static void Timer_Elapsed(object sender, EventArgs args)
        {
            if(_Started) {
                try {
                    DownloadFromVatsim();
                } finally {
                    if(_Started) {
                        _Timer.Interval = RefreshIntervalMilliseconds;
                        _Timer.Enabled = true;
                    }
                }
            }
        }

        private static void DownloadFromVatsimOnBackgroundThread()
        {
            ThreadPool.QueueUserWorkItem(state => {
                try {
                    if(_Started) {
                        DownloadFromVatsim();
                    }
                } catch {
                    // Never let anything bubble out of a background thread, it will do an immediate halt
                }
            });
        }

        private static void DownloadFromVatsim()
        {
            try {
                lock(_SyncLock) {
                    DownloadStatus();
                    DownloadDataV3();
                }
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                try {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine($"{nameof(VatsimDownloader)}.{nameof(DownloadFromVatsim)} caught exception during download: {ex}");
                } catch {
                    ;
                }
            }
        }

        private static void DownloadStatus()
        {
            if(_VatsimStatus == null) {
                var jsonText = DownloadJsonFromVatsim("https://status.vatsim.net/status.json");
                if(!String.IsNullOrEmpty(jsonText)) {
                    var candidate = JsonConvert.DeserializeObject<Status>(jsonText);
                    if(candidate.data?.v3?.Any() ?? false) {
                        _VatsimStatus = candidate;
                        OnStartedChanged(EventArgs.Empty);
                    }
                }
            }
        }

        private static void DownloadDataV3()
        {
            if(_VatsimStatus?.data?.v3?.Any() ?? false) {
                var url = RoundRobin.ChooseRandomUrl(_VatsimStatus.data.v3);
                if(!String.IsNullOrEmpty(url)) {
                    var jsonText = DownloadJsonFromVatsim(url);
                    if(!String.IsNullOrEmpty(jsonText)) {
                        var dataV3 = JsonConvert.DeserializeObject<VatsimDataV3>(jsonText);
                        OnDataDownloaded(new EventArgs<VatsimDataV3>(dataV3));
                    }
                }
            }
        }

        private static string DownloadJsonFromVatsim(string url)
        {
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            string result = null;
            using(var response = webRequest.GetResponse()) {
                using(var streamReader = new StreamReader(response.GetResponseStream())) {
                    result = streamReader.ReadToEnd();
                }
            }

            return result;
        }
    }
}
