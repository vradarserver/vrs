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
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// A singleton instance that can filter aircraft messages of various types.
    /// </summary>
    static class Filter
    {
        /// <summary>
        /// An object that holds the configuration settings parsed into a more usable form.
        /// </summary>
        class ParsedConfiguration
        {
            private SpinLock _Lock = new SpinLock();

            private bool _Enabled;

            private bool _ProhibitMlat;

            private bool _ProhibitUnfilterableFeeds;

            private HashSet<string> _ProhibitedIcaoStrings = new HashSet<string>();

            private HashSet<int> _ProhibitedIcaoNumbers = new HashSet<int>();

            public void ApplyOptionsChange(Options options)
            {
                using(_Lock.AcquireLock()) {
                    _Enabled = options.Enabled;
                    _ProhibitUnfilterableFeeds = options.ProhibitUnfilterableFeeds;
                }
            }

            public void ApplyFilterConfigurationChange(FilterConfiguration filterConfiguration)
            {
                using(_Lock.AcquireLock()) {
                    _ProhibitMlat = filterConfiguration.ProhibitMlat;

                    _ProhibitedIcaoStrings.Clear();
                    _ProhibitedIcaoNumbers.Clear();
                    foreach(var icao in filterConfiguration.ProhibitedIcaos) {
                        var normalisedIcao = NormaliseIcao(icao);
                        if(normalisedIcao.Length == 6 && !_ProhibitedIcaoStrings.Contains(normalisedIcao)) {
                            try {
                                var icaoNumber = Convert.ToInt32(normalisedIcao, 16);
                                _ProhibitedIcaoStrings.Add(normalisedIcao);
                                _ProhibitedIcaoNumbers.Add(icaoNumber);
                            } catch(FormatException) {
                                // Unparsable ICAO. These should have been caught by validation, we're not
                                // going to make a song and dance about them here.
                            }
                        }
                    }
                }
            }

            public bool AreUnfilterableFeedsProhibited()
            {
                _Lock.Lock();
                try {
                    return _Enabled && _ProhibitUnfilterableFeeds;
                } finally {
                    _Lock.Unlock();
                }
            }

            public bool IsMlatProhibited()
            {
                _Lock.Lock();
                try {
                    return _Enabled && _ProhibitMlat;
                } finally {
                    _Lock.Unlock();
                }
            }

            public bool IsIcaoProhibited(string icao)
            {
                var result = false;
                if(!String.IsNullOrEmpty(icao)) {
                    var normalisedIcao = NormaliseIcao(icao);
                    _Lock.Lock();
                    try {
                        result = _Enabled && _ProhibitedIcaoStrings.Contains(icao);
                    } finally {
                        _Lock.Unlock();
                    }
                }

                return result;
            }

            public bool IsIcaoProhibited(int icao)
            {
                _Lock.Lock();
                try {
                    return _Enabled && _ProhibitedIcaoNumbers.Contains(icao);
                } finally {
                    _Lock.Unlock();
                }
            }

            private string NormaliseIcao(string icao)
            {
                return (icao ?? "").ToUpperInvariant().Trim();
            }
        }

        /// <summary>
        /// Manages multi-threaded access to the configuration settings.
        /// </summary>
        private static ParsedConfiguration _ParsedConfiguration = new ParsedConfiguration();

        /// <summary>
        /// True if <see cref="Initialise"/> has been called.
        /// </summary>
        private static bool _Initialised;

        /// <summary>
        /// Initialises the object.
        /// </summary>
        public static void Initialise(Plugin plugin)
        {
            if(!_Initialised) {
                _Initialised = true;

                var options = OptionsStorage.Load(plugin);
                _ParsedConfiguration.ApplyOptionsChange(options);

                var filterConfiguration = FilterConfigurationStorage.Load();
                _ParsedConfiguration.ApplyFilterConfigurationChange(filterConfiguration);

                OptionsStorage.OptionsChanged += OptionsStorage_OptionsChanged;
                FilterConfigurationStorage.FilterConfigurationChanged += FilterConfigurationStorage_FilterConfigurationChanged;
            }
        }

        /// <summary>
        /// Returns true if unfilterable feeds are prohibited;
        /// </summary>
        /// <returns></returns>
        public static bool AreUnfilterableFeedsProhibited()
        {
            return _ParsedConfiguration.AreUnfilterableFeedsProhibited();
        }

        /// <summary>
        /// Returns the event args passed in, possibly altered, or null if the event must not be passed on.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static BaseStationMessageEventArgs FilterEvent(BaseStationMessageEventArgs args)
        {
            var result = args;

            if(args != null && args.Message != null) {
                if(_ParsedConfiguration.IsIcaoProhibited(args.Message.Icao24)) result = null;
                else if((args.Message.IsMlat || args.IsOutOfBand) && _ParsedConfiguration.IsMlatProhibited()) {
                    result.Message.Latitude = null;
                    result.Message.Longitude = null;
                    result.Message.Track = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the event args passed in, possibly altered, or null if the event must not be passed on.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ModeSMessageEventArgs FilterEvent(ModeSMessageEventArgs args)
        {
            var result = args;

            if(args != null && args.ModeSMessage != null) {
                if(_ParsedConfiguration.IsIcaoProhibited(args.ModeSMessage.Icao24)) result = null;
                else if(args.ModeSMessage.IsMlat && _ParsedConfiguration.IsMlatProhibited()) result = null;
            }

            return result;
        }

        /// <summary>
        /// Raised when the options have changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OptionsStorage_OptionsChanged(object sender, EventArgs<Options> args)
        {
            _ParsedConfiguration.ApplyOptionsChange(args.Value);
        }

        /// <summary>
        /// Raised when the filter configuration has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void FilterConfigurationStorage_FilterConfigurationChanged(object sender, EventArgs<FilterConfiguration> args)
        {
            _ParsedConfiguration.ApplyFilterConfigurationChange(args.Value);
        }
    }
}
