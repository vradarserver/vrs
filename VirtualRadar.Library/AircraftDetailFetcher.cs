// Copyright © 2014 onwards, Andrew Whewell
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
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftDetailFetcher"/>.
    /// </summary>
    sealed class AircraftDetailFetcher : AircraftFetcher<string, AircraftDetail>, IAircraftDetailFetcher
    {
        #region Private class - PictureDetail
        /// <summary>
        /// Holds the information that we need to look up an aircraft's local picture.
        /// </summary>
        class LookupPictureDetail
        {
            public string Icao { get; set; }

            public string Registration { get; set; }

            public PictureDetail PictureDetail { get; set; }

            public PictureDetail Result { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The auto-config database that is being used.
        /// </summary>
        private IAutoConfigBaseStationDatabase _AutoConfigDatabase;

        /// <summary>
        /// The basic aircraft details database.
        /// </summary>
        private IBasicAircraftLookupDatabase _BasicAircraftLookup;

        /// <summary>
        /// The picture folder directory cache that is being used.
        /// </summary>
        private IDirectoryCache _PictureFolderCache;

        /// <summary>
        /// The object that finds pictures for aircraft.
        /// </summary>
        private IAircraftPictureManager _PictureManager;

        /// <summary>
        /// The object that fetches standing data records for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// True if the standing data is to be reloaded even if values it depends upon have not changed.
        /// </summary>
        private bool _ForceRefreshOfStandingData;

        /// <summary>
        /// A lock that prevents multiple threads from changing <see cref="_ForceRefreshOfStandingData"/>.
        /// </summary>
        private object _ForceRefreshOfStandingDataSyncLock = new object();

        /// <summary>
        /// The thread that picture lookups are made on. We need a separate thread because if picture lookups are slow
        /// (e.g. they're over an Internet VPN) then they will prevent the database lookup from appearing on the site
        /// until they're complete.
        /// </summary>
        private BackgroundThreadQueue<LookupPictureDetail> _PictureLookupThread;

        /// <summary>
        /// The results of the picture lookup thread's endeavours.
        /// </summary>
        private List<LookupPictureDetail> _PictureLookupResults = new List<LookupPictureDetail>();

        /// <summary>
        /// The lock on the <see cref="_PictureLookupResults"/> list.
        /// </summary>
        private object _PictureLookupResultsSyncLock = new object();

        /// <summary>
        /// True if a picture lookup exception has been logged. We only log the first ever exception to avoid saturating the log.
        /// </summary>
        private bool _LoggedPictureLookupException;
        #endregion

        #region Properties
        private static readonly IAircraftDetailFetcher _Singleton = new AircraftDetailFetcher();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftDetailFetcher Singleton
        {
            get { return _Singleton; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override int AutomaticRecheckIntervalMilliseconds
        {
            get { return 60000; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override int AutomaticDeregisterIntervalMilliseconds
        {
            get { return 90000; }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<AircraftDetail>> Fetched;

        /// <summary>
        /// Raises <see cref="Fetched"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFetched(EventArgs<AircraftDetail> args)
        {
            EventHelper.Raise(Fetched, this, args);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && !Disposed) {
                if(_AutoConfigDatabase != null) {
                    _AutoConfigDatabase.Database.AircraftUpdated -= BaseStationDatabase_AircraftUpdated;
                    _AutoConfigDatabase.Database.FileNameChanged -= BaseStationDatabase_FileNameChanged;
                }
                if(_PictureFolderCache != null) Factory.Singleton.Resolve<IAutoConfigPictureFolderCache>().Singleton.CacheConfigurationChanged -= AutoConfigPictureFolderCache_CacheConfigurationChanged;
                if(_StandingDataManager != null) _StandingDataManager.LoadCompleted -= StandingDataManager_LoadCompleted;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region DoInitialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialise()
        {
            _AutoConfigDatabase = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton;
            _AutoConfigDatabase.Database.AircraftUpdated += BaseStationDatabase_AircraftUpdated;
            _AutoConfigDatabase.Database.FileNameChanged += BaseStationDatabase_FileNameChanged;

            _BasicAircraftLookup = Factory.Singleton.Resolve<IBasicAircraftLookupDatabase>().Singleton;
            _BasicAircraftLookup.ContentUpdated += BasicAircraftLookup_ContentUpdated;

            _PictureManager = Factory.Singleton.Resolve<IAircraftPictureManager>().Singleton;
            var autoConfigurationPictureFolderCache = Factory.Singleton.Resolve<IAutoConfigPictureFolderCache>().Singleton;
            _PictureFolderCache = autoConfigurationPictureFolderCache.DirectoryCache;
            autoConfigurationPictureFolderCache.CacheConfigurationChanged += AutoConfigPictureFolderCache_CacheConfigurationChanged;

            _StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
            _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;

            if(_PictureLookupThread == null) {
                _PictureLookupThread = new BackgroundThreadQueue<LookupPictureDetail>("PictureLookup", false);
                _PictureLookupThread.StartBackgroundThread(PictureLookupThread_ProcessLookup, PictureLookupThread_ProcessException);
            }

            base.DoInitialise();
        }
        #endregion

        #region RegisterAircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public AircraftDetail RegisterAircraft(IAircraft aircraft)
        {
            if(aircraft == null) throw new ArgumentNullException("aircraft");

            AircraftDetail result = null;
            if(!String.IsNullOrEmpty(aircraft.Icao24)) result = DoRegisterAircraft(aircraft.Icao24, aircraft);

            return result;
        }
        #endregion

        #region DoFetchAircraft, DoFetchManyAircraft
        /// <summary>
        /// Fetches the details for a single aircraft.
        /// </summary>
        /// <param name="fetchedDetail"></param>
        /// <returns></returns>
        protected override AircraftDetail DoFetchAircraft(AircraftFetcher<string, AircraftDetail>.FetchedDetail fetchedDetail)
        {
            var databaseAircraft = _AutoConfigDatabase.Database.GetAircraftByCode(fetchedDetail.Aircraft.Icao24);
            var basicAircraft = _BasicAircraftLookup.GetAircraftAndChildrenByIcao(fetchedDetail.Aircraft.Icao24);

            return ApplyDatabaseRecord(fetchedDetail.Detail, databaseAircraft, basicAircraft, fetchedDetail.Aircraft, fetchedDetail.IsFirstFetch);
        }

        /// <summary>
        /// Fetches many aircraft simultaneously.
        /// </summary>
        /// <param name="fetchedDetails"></param>
        /// <returns></returns>
        protected override bool DoFetchManyAircraft(IEnumerable<AircraftFetcher<string, AircraftDetail>.FetchedDetail> fetchedDetails)
        {
            var allIcaos = fetchedDetails.Select(r => r.Key).ToArray();
            var newIcaos = fetchedDetails.Where(r => r.Detail == null || r.Detail.Aircraft == null).Select(r => r.Key).ToArray();
            var existingIcaos = fetchedDetails.Where(r => r.Detail != null && r.Detail.Aircraft != null).Select(r => r.Key).ToArray();

            var aircraftAndFlightCounts = _AutoConfigDatabase.Database.GetManyAircraftAndFlightsCountByCode(newIcaos);
            var aircraft = _AutoConfigDatabase.Database.GetManyAircraftByCode(existingIcaos);
            var allBasicAircraft = _BasicAircraftLookup.GetManyAircraftAndChildrenByCode(allIcaos);

            foreach(var kvp in fetchedDetails) {
                var icao24 = kvp.Key;
                var fetchedDetail = kvp;
                BaseStationAircraft databaseAircraft = null;
                BaseStationAircraftAndFlightsCount databaseAircraftAndFlights = null;
                BasicAircraftAndChildren basicAircraft = null;
                if(!aircraft.TryGetValue(icao24, out databaseAircraft)) {
                    aircraftAndFlightCounts.TryGetValue(icao24, out databaseAircraftAndFlights);
                }
                allBasicAircraft.TryGetValue(icao24, out basicAircraft);
                kvp.Detail = ApplyDatabaseRecord(
                    fetchedDetail.Detail,
                    databaseAircraftAndFlights ?? databaseAircraft,
                    basicAircraft,
                    fetchedDetail.Aircraft,
                    fetchedDetail.IsFirstFetch,
                    databaseAircraftAndFlights != null ? databaseAircraftAndFlights.FlightsCount
                                                       : fetchedDetail.Detail != null ? fetchedDetail.Detail.FlightsCount : 0
                );
            }

            return true;
        }

        /// <summary>
        /// Applies the fetched aircraft record to the aircraft detail, raising any events appropriate.
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="databaseAircraft"></param>
        /// <param name="aircraft"></param>
        /// <param name="basicAircraft"></param>
        /// <param name="isFirstFetch"></param>
        /// <param name="flightsCount"></param>
        private AircraftDetail ApplyDatabaseRecord(AircraftDetail detail, BaseStationAircraft databaseAircraft, BasicAircraftAndChildren basicAircraft, IAircraft aircraft, bool isFirstFetch, int flightsCount = -1)
        {
            var databaseAircraftChanged = detail == null;
            if(!databaseAircraftChanged) {
                if(detail.Aircraft == null) databaseAircraftChanged = databaseAircraft != null;
                else {
                    databaseAircraftChanged = !detail.Aircraft.Equals(databaseAircraft);
                    if(!databaseAircraftChanged && flightsCount > -1 && flightsCount != detail.FlightsCount) databaseAircraftChanged = true;
                }
            }

            var basicAircraftChanged = detail == null;
            if(!basicAircraftChanged) {
                if(detail.BasicAircraft== null) basicAircraftChanged = basicAircraft != null;
                else                            basicAircraftChanged = !detail.BasicAircraft.Equals(basicAircraft);
            }

            if(_ForceRefreshOfStandingData && detail != null) detail.AircraftType = null;

            var icaoTypeCode = databaseAircraft == null ? null : databaseAircraft.ICAOTypeCode;
            if(String.IsNullOrEmpty(icaoTypeCode) && basicAircraft != null) icaoTypeCode = basicAircraft.ModelIcao;

            var aircraftType = detail == null ? null : detail.AircraftType;
            var aircraftTypeChanged = detail == null;
            if(icaoTypeCode != null && (_ForceRefreshOfStandingData || detail == null || detail.Aircraft == null || detail.ModelIcao != icaoTypeCode)) {
                aircraftType = _StandingDataManager.FindAircraftType(icaoTypeCode);
                aircraftTypeChanged = true;
            }

            if(databaseAircraftChanged || basicAircraftChanged || aircraftTypeChanged) {
                if(flightsCount == -1) {
                    flightsCount = detail != null ? detail.FlightsCount 
                                          : databaseAircraft == null ? 0
                                          : _AutoConfigDatabase.Database.GetCountOfFlightsForAircraft(databaseAircraft, new SearchBaseStationCriteria() {
                                                Date = new FilterRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
                                            });
                }

                detail = new AircraftDetail() {
                    Aircraft = databaseAircraft,
                    BasicAircraft = basicAircraft,
                    AircraftType = aircraftType,
                    FlightsCount = flightsCount,
                    Icao24 = aircraft.Icao24,
                    Picture = detail == null ? null : detail.Picture,
                };
                OnFetched(new EventArgs<AircraftDetail>(detail));
            }

            var registration = databaseAircraft != null ? databaseAircraft.Registration : aircraft.Registration;
            if(registration == null && basicAircraft != null) registration = basicAircraft.Registration;

            if(_PictureLookupThread != null) {
                _PictureLookupThread.Enqueue(new LookupPictureDetail() {
                    Icao = aircraft.Icao24,
                    Registration = registration,
                    PictureDetail = detail == null ? null : detail.Picture,
                });
            }

            return detail;
        }

        /// <summary>
        /// Processes the successful completion of a picture lookup operation on the background thread.
        /// </summary>
        /// <param name="lookupPictureDetail"></param>
        private void ApplyPictureLookup(LookupPictureDetail lookupPictureDetail)
        {
            var fetchedDetail = GetFetchedDetailUnderLock(lookupPictureDetail.Icao);
            if(fetchedDetail != null) {
                var detail = fetchedDetail.Detail;
                if(detail != null) {
                    var pictureDetail = lookupPictureDetail.Result;
                    var pictureDetailChanged = false;
                    if(detail.Picture == null) pictureDetailChanged = pictureDetail != null;
                    else                       pictureDetailChanged = !detail.Picture.Equals(pictureDetail);

                    if(pictureDetailChanged) {
                        fetchedDetail.Detail.Picture = pictureDetail;
                        OnFetched(new EventArgs<AircraftDetail>(detail));
                    }
                }
            }
        }
        #endregion

        #region DoExtraFastTimerTickWork
        /// <summary>
        /// Called when the fast timer has ticked.
        /// </summary>
        protected override void DoExtraFastTimerTickWork()
        {
            List<LookupPictureDetail> lookupPictureResults = null;
            lock(_PictureLookupResultsSyncLock) {
                if(_PictureLookupResults.Count > 0) {
                    lookupPictureResults = new List<LookupPictureDetail>(_PictureLookupResults);
                    _PictureLookupResults.Clear();
                }
            }

            if(lookupPictureResults != null) {
                foreach(var lookupPictureResult in lookupPictureResults) {
                    ApplyPictureLookup(lookupPictureResult);
                }
            }
        }
        #endregion

        #region RefetchWithoutDatabaseSearch
        /// <summary>
        /// Refetches all of the registered aircraft without also running database searches.
        /// </summary>
        private void RefetchWithoutDatabaseSearch()
        {
            var allRegistered = GetRegisteredAircraft();
            foreach(var registered in allRegistered) {
                var currentIcao24 = registered.Key;
                var currentDetail = registered.Value;
                FauxFetchAircraft(currentIcao24, (string icao24, AircraftDetail detail, bool isFirstFetch, IAircraft aircraft) => {
                    if(detail != null) detail = ApplyDatabaseRecord(detail, detail.Aircraft, detail.BasicAircraft, aircraft, isFirstFetch);
                    return detail;
                });
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the database reports that an aircraft record has been updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_AircraftUpdated(object sender, EventArgs<BaseStationAircraft> args)
        {
            try {
                var databaseAircraft = args.Value;

                if(databaseAircraft != null && !String.IsNullOrEmpty(databaseAircraft.ModeS)) {
                    FauxFetchAircraft(databaseAircraft.ModeS.ToUpper(), (string icao24, AircraftDetail detail, bool isFirstFetch, IAircraft aircraft) => {
                        detail = ApplyDatabaseRecord(detail, databaseAircraft, detail.BasicAircraft, aircraft, isFirstFetch);
                        return detail;
                    });
                }
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception during refresh of aircraft database detail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the database reports a configuration change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_FileNameChanged(object sender, EventArgs args)
        {
            try {
                ForceRefetchOnFastTick = true;
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception during refresh of aircraft database detail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the basic aircraft details have changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BasicAircraftLookup_ContentUpdated(object sender, EventArgs args)
        {
            try {
                ForceRefetchOnFastTick = true;
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception during refresh of aircraft database detail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the picture folder cache's configuration has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoConfigPictureFolderCache_CacheConfigurationChanged(object sender, EventArgs args)
        {
            try {
                _LoggedPictureLookupException = false;
                RefetchWithoutDatabaseSearch();
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception during refresh of aircraft pictures: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the standing data has been reloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            try {
                lock(_ForceRefreshOfStandingDataSyncLock) {
                    var existingForceFlag = _ForceRefreshOfStandingData;
                    try {
                        _ForceRefreshOfStandingData = true;
                        RefetchWithoutDatabaseSearch();
                    } finally {
                        _ForceRefreshOfStandingData = existingForceFlag;
                    }
                }
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Caught exception during refetch of aircraft type codes: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when a picture needs to be fetched.
        /// </summary>
        /// <param name="lookupPictureDetail"></param>
        private void PictureLookupThread_ProcessLookup(LookupPictureDetail lookupPictureDetail)
        {
            lookupPictureDetail.Result = _PictureManager.FindPicture(_PictureFolderCache, lookupPictureDetail.Icao, lookupPictureDetail.Registration, lookupPictureDetail.PictureDetail);
            lock(_PictureLookupResultsSyncLock) {
                _PictureLookupResults.Add(lookupPictureDetail);
            }
        }

        /// <summary>
        /// Called when the picture lookup throws an exception.
        /// </summary>
        /// <param name="ex"></param>
        private void PictureLookupThread_ProcessException(Exception ex)
        {
            if(!_LoggedPictureLookupException) {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                if(log != null) {
                    _LoggedPictureLookupException = true;
                    log.WriteLine("Caught exception during fetch of aircraft picture: {0}", ex.ToString());
                }
            }
        }
        #endregion
    }
}
