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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Holds state that is shared between all instances of <see cref="ReportsController"/>.
    /// </summary>
    internal static class ReportsControllerSharedState
    {
        private static object _SyncLock = new object();

        private static ISharedConfiguration _SharedConfiguration;
        /// <summary>
        /// Gets the shared configuration used by all <see cref="ReportController"/>s.
        /// </summary>
        public static ISharedConfiguration SharedConfiguration
        {
            get {
                if(_SharedConfiguration == null) {
                    lock(_SyncLock) {
                        if(_SharedConfiguration == null) {
                            _SharedConfiguration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
                        }
                    }
                }
                return _SharedConfiguration;
            }
        }

        private static IAutoConfigBaseStationDatabase _AutoConfigBaseStationDatabase;
        /// <summary>
        /// Gets the BaseStation database used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IBaseStationDatabase BaseStationDatabase
        {
            get {
                if(_AutoConfigBaseStationDatabase == null) {
                    lock(_SyncLock) {
                        if(_AutoConfigBaseStationDatabase == null) {
                            _AutoConfigBaseStationDatabase = Factory.Singleton.ResolveSingleton<IAutoConfigBaseStationDatabase>();
                        }
                    }
                }
                return _AutoConfigBaseStationDatabase.Database;
            }
        }

        private static IStandingDataManager _StandingDataManager;
        /// <summary>
        /// Gets the StandingData database used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IStandingDataManager StandingDataManager
        {
            get {
                if(_StandingDataManager == null) {
                    lock(_SyncLock) {
                        if(_StandingDataManager == null) {
                            _StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
                        }
                    }
                }
                return _StandingDataManager;
            }
        }

        private static ICallsignParser _CallsignParser;
        /// <summary>
        /// Gets the callsign parser used by all <see cref="ReportController"/>s.
        /// </summary>
        public static ICallsignParser CallsignParser
        {
            get {
                if(_CallsignParser == null) {
                    lock(_SyncLock) {
                        if(_CallsignParser == null) {
                            _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
                        }
                    }
                }
                return _CallsignParser;
            }
        }

        private static IAircraftPictureManager _AircraftPictureManager;
        /// <summary>
        /// Gets the aircraft picture manager used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IAircraftPictureManager AircraftPictureManager
        {
            get {
                if(_AircraftPictureManager == null) {
                    lock(_SyncLock) {
                        if(_AircraftPictureManager == null) {
                            _AircraftPictureManager = Factory.Singleton.ResolveSingleton<IAircraftPictureManager>();
                        }
                    }
                }
                return _AircraftPictureManager;
            }
        }

        private static IAutoConfigPictureFolderCache _PictureFolderCache;
        /// <summary>
        /// Gets the picture folder cache used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IDirectoryCache PictureFolderCache
        {
            get {
                if(_PictureFolderCache == null) {
                    lock(_SyncLock) {
                        if(_PictureFolderCache == null) {
                            _PictureFolderCache = Factory.Singleton.ResolveSingleton<IAutoConfigPictureFolderCache>();
                        }
                    }
                }
                return _PictureFolderCache.DirectoryCache;
            }
        }

        private static IClock _Clock;
        /// <summary>
        /// Gets the clock used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IClock Clock
        {
            get {
                if(_Clock == null) {
                    lock(_SyncLock) {
                        if(_Clock == null) {
                            _Clock = Factory.Singleton.Resolve<IClock>();
                        }
                    }
                }
                return _Clock;
            }
        }

        private static IFileSystemProvider _FileSystemProvider;
        /// <summary>
        /// Gets the file system used by all <see cref="ReportController"/>s.
        /// </summary>
        public static IFileSystemProvider FileSystem
        {
            get {
                if(_FileSystemProvider == null) {
                    lock(_SyncLock) {
                        if(_FileSystemProvider == null) {
                            _FileSystemProvider = Factory.Singleton.Resolve<IFileSystemProvider>();
                        }
                    }
                }
                return _FileSystemProvider;
            }
        }

        /// <summary>
        /// Test-only method to reset state back to default.
        /// </summary>
        /// <remarks>
        /// This is only intended for use in tests. It does not use _SyncLock because it is not intended
        /// for use in multithreaded situations, the getters are not written in a way that can withstand
        /// the backing field being reset to null while the getter is running.
        /// </remarks>
        internal static void Reset()
        {
            foreach(var field in typeof(ReportsControllerSharedState).GetFields(BindingFlags.Static | BindingFlags.NonPublic)) {
                if(field.Name != nameof(_SyncLock)) {
                    field.SetValue(null, PropertyHelper.DefaultValue(field.FieldType));
                }
            }
        }
    }
}
