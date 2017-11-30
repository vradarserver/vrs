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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    /// <summary>
    /// Implements <see cref="IPlugin"/> to tell VRS about our plugin.
    /// </summary>
    public class Plugin : IPlugin
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IPluginProvider"/>.
        /// </summary>
        class DefaultProvider : IPluginProvider
        {
            public DateTime UtcNow                                  { get { return DateTime.UtcNow; } }
            public DateTime LocalNow                                { get { return DateTime.Now; } }
            public IOptionsView CreateOptionsView()                 { return new WinForms.OptionsView(); }
            public IOnlineLookupCache CreateOnlineLookupCache()     { return new OnlineLookupCache(); }
        }
        #endregion

        #region Private class - FlightRecords
        /// <summary>
        /// Holds information about a flight that is currently being tracked.
        /// </summary>
        class FlightRecords
        {
            public BaseStationAircraft Aircraft { get; set; }
            public BaseStationFlight Flight { get; set; }
            public DateTime EndTimeUtc { get; set; }
            public bool? OnGround { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that different threads synchronise on before using the contents of the fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The options that govern the plugin's behaviour.
        /// </summary>
        private Options _Options = new Options();

        /// <summary>
        /// The object that queues messages from BaseStation and relays them to us on a background thread.
        /// </summary>
        private BackgroundThreadQueue<BaseStationMessageEventArgs> _BackgroundThreadMessageQueue;

        /// <summary>
        /// The object that is handling the database for us.
        /// </summary>
        private IBaseStationDatabase _Database;

        /// <summary>
        /// The object that looks up values in the standing data for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// The session record that this plugin has created in the BaseStation database.
        /// </summary>
        private BaseStationSession _Session;

        /// <summary>
        /// A map of ICAO24 codes to a private class holding the database records being maintained for the flight.
        /// </summary>
        private Dictionary<int, FlightRecords> _FlightMap = new Dictionary<int, FlightRecords>();

        /// <summary>
        /// The feed whose aircraft messages are being recorded in the database.
        /// </summary>
        private IFeed _Feed;

        /// <summary>
        /// The private heartbeat service that the plugin uses. Some of the database operations can take a long time
        /// and we don't want to clog up other things using the service.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// The aircraft online details object that caches records to BaseStation.sqb. All we do is register
        /// the cache and control its Enabled property. We do not write records to it, the aircraft online lookup manager
        /// does that.
        /// </summary>
        private IOnlineLookupCache _OnlineLookupCache;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for testing.
        /// </summary>
        public IPluginProvider Provider { get; set; }

        /// <summary>
        /// Gets the last initialised instance of the plugin object. At run-time only one plugin
        /// object gets created and initialised.
        /// </summary>
        public static Plugin Singleton { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id { get { return "VirtualRadar.Plugin.BaseStationDatabaseWriter"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return PluginStrings.PluginName; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.4.0"; } }

        private string _Status;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status
        {
            get { return _Status; }
            set {
                if(_Status != value) {
                    _Status = value ?? "";
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        private string _StatusDescription;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription
        {
            get { return _StatusDescription; }
            set {
                if(_StatusDescription != value) {
                    _StatusDescription = value ?? "";
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Raises <see cref="StatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStatusChanged(EventArgs args)
        {
            if(StatusChanged != null) StatusChanged(this, args);
        }

        /// <summary>
        /// Raised when <see cref="OptionsStorage"/> saves a new set of options.
        /// </summary>
        public event EventHandler<EventArgs<Options>> SettingsChanged;

        /// <summary>
        /// Raises <see cref="SettingsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        internal void RaiseSettingsChanged(EventArgs<Options> args)
        {
            ApplyOptions(args.Value);
            EventHelper.Raise(SettingsChanged, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Plugin()
        {
            Provider = new DefaultProvider();
            Status = PluginStrings.Disabled;
        }
        #endregion

        #region RegisterImplementations
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            ;
        }
        #endregion

        #region Startup
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            Singleton = this;

            lock(_SyncLock) {
                var optionsStorage = new OptionsStorage();
                _Options = optionsStorage.Load();

                _Database = Factory.Singleton.ResolveSingleton<IAutoConfigBaseStationDatabase>().Database;
                _Database.FileNameChanging += BaseStationDatabase_FileNameChanging;
                _Database.FileNameChanged += BaseStationDatabase_FileNameChanged;

                _StandingDataManager = Factory.Singleton.ResolveSingleton<IStandingDataManager>();
                _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;

                var feedManager = Factory.Singleton.ResolveSingleton<IFeedManager>();
                feedManager.FeedsChanged += FeedManager_FeedsChanged;

                _OnlineLookupCache = Provider.CreateOnlineLookupCache();
                _OnlineLookupCache.Database = _Database;
                _OnlineLookupCache.RefreshOutOfDateAircraft = _Options.RefreshOutOfDateAircraft;
                _OnlineLookupCache.EnabledChanged += OnlineLookupCache_EnabledChanged;
                StartSession();

                var onlineLookupManager = Factory.Singleton.ResolveSingleton<IAircraftOnlineLookupManager>();
                onlineLookupManager.RegisterCache(_OnlineLookupCache, 100, letManagerControlLifetime: false);

                // If we process messages on the same thread as the listener raises the message received event on then we
                // will be running on the same thread as the aircraft list. Our processing can take some time, particularly if many
                // database writes have to happen simultaneously on startup, so to avoid blocking the update of the aircraft list
                // we create a background thread and process the messages on that.
                _BackgroundThreadMessageQueue = new BackgroundThreadQueue<BaseStationMessageEventArgs>("BaseStationDatabaseWriterMessageQueue", 200000);
                _BackgroundThreadMessageQueue.StartBackgroundThread(MessageQueue_MessageReceived, MessageQueue_ExceptionCaught);

                HookFeed();

                _HeartbeatService = Factory.Singleton.Resolve<IHeartbeatService>();
                _HeartbeatService.SlowTick += Heartbeat_SlowTick;
                _HeartbeatService.Start();
            }
        }
        #endregion

        #region GuiThreadStartup
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.Singleton.ResolveSingleton<IWebAdminViewManager>();
            webAdminViewManager.RegisterTranslations(typeof(PluginStrings), "DatabaseWriterPlugin");
            webAdminViewManager.AddWebAdminView(new WebAdminView("/WebAdmin/", "DatabaseWriterPluginOptions.html", PluginStrings.WebAdminMenuName, () => new WebAdmin.OptionsView(), typeof(PluginStrings)) {
                Plugin = this,
            });
            webAdminViewManager.RegisterWebAdminViewFolder(PluginFolder, "Web");
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            if(_OnlineLookupCache != null)              _OnlineLookupCache.Enabled = false;
            if(_BackgroundThreadMessageQueue != null)   _BackgroundThreadMessageQueue.Dispose();
            if(_HeartbeatService != null) {
                _HeartbeatService.SlowTick -= Heartbeat_SlowTick;
                _HeartbeatService.Dispose();
            }
            EndSession();
        }
        #endregion

        #region ShowWinFormsOptionsUI
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = Provider.CreateOptionsView()) {
                view.ShowView();
            }
        }

        private void ApplyOptions(Options options)
        {
            lock(_SyncLock) {
                UnhookFeed();

                _Options.Enabled =                          options.Enabled;
                _Options.AllowUpdateOfOtherDatabases =      options.AllowUpdateOfOtherDatabases;
                _Options.ReceiverId =                       options.ReceiverId;
                _Options.SaveDownloadedAircraftDetails =    options.SaveDownloadedAircraftDetails;
                _Options.RefreshOutOfDateAircraft =         options.RefreshOutOfDateAircraft;

                bool optionsPermit = _Options.Enabled && (_Options.AllowUpdateOfOtherDatabases || DatabaseCreatedByPlugin());
                if(_Session != null && !optionsPermit) {
                    EndSession();
                }
                StartSession();
                HookFeed();

                _OnlineLookupCache.RefreshOutOfDateAircraft = _Options.RefreshOutOfDateAircraft;
                if(!_Options.SaveDownloadedAircraftDetails) {
                    _OnlineLookupCache.Enabled = false;
                } else {
                    _OnlineLookupCache.Enabled = _Session != null;
                }
            }
        }
        #endregion

        #region HookFeed, UnhookFeed
        /// <summary>
        /// Hooks the feed specified in options, unhooking the previous feed if there was one.
        /// </summary>
        private void HookFeed()
        {
            lock(_SyncLock) {
                var feedManager = Factory.Singleton.ResolveSingleton<IFeedManager>();
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId, ignoreInvisibleFeeds: false);
                if(feed != _Feed) {
                    if(feed != null) {
                        feed.Listener.Port30003MessageReceived += MessageListener_MessageReceived;
                        feed.Listener.SourceChanged += MessageListener_SourceChanged;
                    }

                    _Feed = feed;
                }
            }
        }

        /// <summary>
        /// Unhooks the current feed if it isn't currently valid.
        /// </summary>
        private void UnhookFeed()
        {
            lock(_SyncLock) {
                var feedManager = Factory.Singleton.ResolveSingleton<IFeedManager>();
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId, ignoreInvisibleFeeds: false);
                if(feed != _Feed) {
                    if(_Feed != null && _Feed.Listener != null) {
                        _Feed.Listener.Port30003MessageReceived -= MessageListener_MessageReceived;
                        _Feed.Listener.SourceChanged -= MessageListener_SourceChanged;
                    }

                    _Feed = null;
                }
            }
        }
        #endregion

        #region StartSession, EndSession, TrackFlight, FlushFlights
        /// <summary>
        /// Creates a new session.
        /// </summary>
        private void StartSession()
        {
            var feedManager = Factory.Singleton.ResolveSingleton<IFeedManager>();

            lock(_SyncLock) {
                if(_Session == null) {
                    if(!_Options.Enabled) {
                        Status = PluginStrings.Disabled;
                        StatusDescription = null;
                    } else if(_Options.ReceiverId == 0) {
                        Status = PluginStrings.EnabledNoReceiver;
                    } else if(feedManager.GetByUniqueId(_Options.ReceiverId, ignoreInvisibleFeeds: false) == null) {
                        Status = PluginStrings.EnabledBadReceiver;
                    } else if(String.IsNullOrEmpty(_Database.FileName)) {
                        Status = PluginStrings.EnabledNoDatabase;
                        StatusDescription = null;
                    } else if(!_Database.FileExists()) {
                        Status = PluginStrings.EnabledNotUpdating;
                        StatusDescription = String.Format(PluginStrings.SomethingDoesNotExist, _Database.FileName);
                    } else if(_Database.FileIsEmpty()) {
                        Status = PluginStrings.EnabledNotUpdating;
                        StatusDescription = String.Format(PluginStrings.SomethingIsZeroLength, _Database.FileName);
                    } else if(!_Options.AllowUpdateOfOtherDatabases && !DatabaseCreatedByPlugin()) {
                        Status = PluginStrings.EnabledNotUpdating;
                        StatusDescription = PluginStrings.UpdatingDatabasesNotCreatedByPluginForbidden;
                    } else {
                        Status = String.Format(PluginStrings.EnabledAndUpdatingSomething, _Database.FileName);
                        StatusDescription = null;

                        try {
                            _Database.WriteSupportEnabled = true;

                            var location = _Database.GetLocations().OrderBy(c => c.LocationID).FirstOrDefault();

                            _Session = new BaseStationSession() {
                                LocationID = location == null ? 0 : location.LocationID,
                                StartTime = Provider.LocalNow,
                            };
                            _Database.InsertSession(_Session);

                            _OnlineLookupCache.Enabled = _Session != null && _Options.SaveDownloadedAircraftDetails;
                        } catch(ThreadAbortException) {
                        } catch(Exception ex) {
                            AbandonSession(ex, PluginStrings.ExceptionCaughtWhenStartingSession);
                            Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.StartSession caught exception {0}", ex.ToString()));
                            Factory.Singleton.ResolveSingleton<ILog>().WriteLine("Database writer plugin caught exception on starting session: {0}", ex.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Closes an open session.
        /// </summary>
        private void EndSession()
        {
            lock(_SyncLock) {
                if(_Session != null) {
                    Status = PluginStrings.NotUpdatingDatabase;
                    try {
                        FlushFlights(true);

                        _Session.EndTime = Provider.LocalNow;
                        _Database.UpdateSession(_Session);

                        StatusDescription = null;
                    } catch(ThreadAbortException) {
                    } catch(Exception ex) {
                        AbandonSession(ex, PluginStrings.ExceptionCaughtWhenClosingSession, Status);
                        Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.EndSession caught exception {0}", ex.ToString()));
                        Factory.Singleton.ResolveSingleton<ILog>().WriteLine("Database writer plugin caught exception on closing session: {0}", ex.ToString());
                    } finally {
                        _Session = null;
                        _OnlineLookupCache.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the exception passed across represents an SQLite lock exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsSQLiteLockException(Exception ex)
        {
            var result = false;

            var sqliteExceptionWrapper = Factory.Singleton.Resolve<ISQLiteException>();
            sqliteExceptionWrapper.Initialise(ex);
            result = sqliteExceptionWrapper.IsLocked;

            return result;
        }

        /// <summary>
        /// Aborts the current session entirely without attempting any further writes to the database.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="format"></param>
        /// <param name="status"></param>
        private void AbandonSession(Exception ex, string format, string status = null)
        {
            _Session = null;
            Status = status ?? PluginStrings.EnabledNotUpdating;
            StatusDescription = String.Format(format, ex == null ? "null" : ex.Message);
        }

        /// <summary>
        /// Returns true if the database was created by Virtual Radar Server.
        /// </summary>
        /// <returns></returns>
        private bool DatabaseCreatedByPlugin()
        {
            return _Database.GetDatabaseHistory().Where(h => h.IsCreationOfDatabaseByVirtualRadarServer).Any();
        }

        /// <summary>
        /// Creates database records and updates internal objects to track an aircraft that is currently transmitting messages.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isMlat"></param>
        /// <remarks>
        /// This defers the recording of the flight for as long as possible so that if the database is locked then we don't
        /// have a record of the flight, and the next message will try again to record the flight.
        /// </remarks>
        private void TrackFlight(BaseStationMessage message, bool isMlat)
        {
            if(IsTransmissionMessage(message)) {
                lock(_SyncLock) {
                    if(_Session != null) {
                        var localNow = Provider.LocalNow;
                        var icao24 = CustomConvert.Icao24(message.Icao24);
                        if(icao24 > 0) {
                            FlightRecords flightRecords;
                            if(!_FlightMap.TryGetValue(icao24, out flightRecords)) {
                                flightRecords = new FlightRecords() {
                                    Aircraft = new BaseStationAircraft() {
                                        ModeS = message.Icao24,
                                        FirstCreated = localNow,
                                    },
                                    Flight = new BaseStationFlight() {
                                        Callsign = message.Callsign,
                                        StartTime = localNow,
                                        NumADSBMsgRec = 0,
                                        NumAirCallRepMsgRec = 0,
                                        NumAirPosMsgRec = 0,
                                        NumAirToAirMsgRec = 0,
                                        NumAirVelMsgRec = 0,
                                        NumIDMsgRec = 0,
                                        NumModeSMsgRec = 0,
                                        NumPosMsgRec = 0,
                                        NumSurAltMsgRec = 0,
                                        NumSurIDMsgRec = 0,
                                        NumSurPosMsgRec = 0,
                                    },
                                };

                                _FlightMap.Add(icao24, flightRecords);
                            }

                            var flight = flightRecords.Flight;
                            flightRecords.EndTimeUtc = Provider.UtcNow;
                            flight.EndTime = localNow;
                            if(!isMlat) {
                                if(message.SquawkHasChanged.GetValueOrDefault()) flight.HadAlert = true;
                                if(message.IdentActive.GetValueOrDefault()) flight.HadSpi = true;
                                if(message.Squawk == 7500 || message.Squawk == 7600 || message.Squawk == 7700) flight.HadEmergency = true;
                            }
                            UpdateFirstLastValues(message, flight, flightRecords, isMlat);
                            UpdateMessageCounters(message, flight);

                            if(!String.IsNullOrEmpty(message.Callsign)) {
                                if(message.Callsign != flightRecords.Flight.Callsign) {
                                    flightRecords.Flight.Callsign = message.Callsign;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Looks for flight records where the records haven't been created yet and creates them.
        /// </summary>
        private void WriteNewFlights()
        {
            FlightRecords[] newFlights = null;
            lock(_SyncLock) {
                newFlights = _FlightMap.Where(r => r.Value.Aircraft.AircraftID == 0 && r.Value.Flight.FlightID == 0).Select(r => r.Value).ToArray();
            }

            foreach(var flight in newFlights) {
                WriteFlightRecords(flight);
            }
        }

        /// <summary>
        /// Creates or reads all of the database records associated with a <see cref="FlightRecords"/> object.
        /// </summary>
        /// <param name="flightRecords"></param>
        private void WriteFlightRecords(FlightRecords flightRecords)
        {
            if(flightRecords.Aircraft.AircraftID == 0 && flightRecords.Flight.FlightID == 0) {
                _Database.PerformInTransaction(() => {
                    var aircraft = FetchOrCreateAircraft(flightRecords.Aircraft.FirstCreated, flightRecords.Aircraft.ModeS);
                    if(aircraft != null) {
                        var flight = CreateFlight(flightRecords.Flight.StartTime, aircraft.AircraftID, flightRecords.Flight.Callsign);
                        lock(_SyncLock) {
                            flightRecords.Aircraft = aircraft;
                            flightRecords.Flight = ApplyFlightDetails(flightRecords.Flight, flight);
                        }
                    }

                    return true;
                });
            }
        }

        /// <summary>
        /// Copies most of the flight details from the source to the destination and returns the destination. The flight ID and creation time
        /// are not touched, neither are the aircraft references.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private BaseStationFlight ApplyFlightDetails(BaseStationFlight source, BaseStationFlight destination)
        {
            destination.Callsign =                  source.Callsign ?? "";
            destination.EndTime =                   source.EndTime;
            destination.FirstAltitude =             source.FirstAltitude;
            destination.FirstGroundSpeed =          source.FirstGroundSpeed;
            destination.FirstIsOnGround =           source.FirstIsOnGround;
            destination.FirstLat =                  source.FirstLat;
            destination.FirstLon =                  source.FirstLon;
            destination.FirstSquawk =               source.FirstSquawk;
            destination.FirstTrack =                source.FirstTrack;
            destination.FirstVerticalRate =         source.FirstVerticalRate;
            destination.HadAlert =                  source.HadAlert;
            destination.HadEmergency =              source.HadEmergency;
            destination.HadSpi =                    source.HadSpi;
            destination.LastAltitude =              source.LastAltitude;
            destination.LastGroundSpeed =           source.LastGroundSpeed;
            destination.LastIsOnGround =            source.LastIsOnGround;
            destination.LastLat =                   source.LastLat;
            destination.LastLon =                   source.LastLon;
            destination.LastSquawk =                source.LastSquawk;
            destination.LastTrack =                 source.LastTrack;
            destination.LastVerticalRate =          source.LastVerticalRate;
            destination.NumADSBMsgRec =             source.NumADSBMsgRec;
            destination.NumAirCallRepMsgRec =       source.NumAirCallRepMsgRec;
            destination.NumAirPosMsgRec =           source.NumAirPosMsgRec;
            destination.NumAirToAirMsgRec =         source.NumAirToAirMsgRec;
            destination.NumAirVelMsgRec =           source.NumAirVelMsgRec;
            destination.NumIDMsgRec =               source.NumIDMsgRec;
            destination.NumModeSMsgRec =            source.NumModeSMsgRec;
            destination.NumPosMsgRec =              source.NumPosMsgRec;
            destination.NumSurAltMsgRec =           source.NumSurAltMsgRec;
            destination.NumSurIDMsgRec =            source.NumSurIDMsgRec;
            destination.NumSurPosMsgRec =           source.NumSurPosMsgRec;

            return destination;
        }

        /// <summary>
        /// Returns true if the message holds values transmitted from a vehicle.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsTransmissionMessage(BaseStationMessage message)
        {
            return !String.IsNullOrEmpty(message.Icao24) &&
                   message.MessageType == BaseStationMessageType.Transmission &&
                   message.TransmissionType != BaseStationTransmissionType.None;
        }

        /// <summary>
        /// Fetches the aircraft record from the database for the ICAO24 code passed across. If there is no
        /// record for the ICAO24 code then one is created.
        /// </summary>
        /// <param name="localNow"></param>
        /// <param name="icao24"></param>
        /// <returns></returns>
        private BaseStationAircraft FetchOrCreateAircraft(DateTime now, string icao24)
        {
            return _Database.GetOrInsertAircraftByCode(icao24, out var created);
        }

        /// <summary>
        /// Refreshes the Mode-S countries on tracked aircraft that don't have them.
        /// </summary>
        private void RefreshMissingModeSCountries()
        {
            lock(_SyncLock) {
                if(_Session != null) {
                    var standingDataManager = Factory.Singleton.ResolveSingleton<IStandingDataManager>();

                    foreach(var flightRecord in _FlightMap.Values) {
                        var codeBlock = standingDataManager.FindCodeBlock(flightRecord.Aircraft.ModeS);
                        if(codeBlock != null && codeBlock.Country != flightRecord.Aircraft.ModeSCountry) {
                            flightRecord.Aircraft.ModeSCountry = codeBlock.Country;
                            _Database.UpdateAircraftModeSCountry(flightRecord.Aircraft.AircraftID, flightRecord.Aircraft.ModeSCountry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates, inserts and returns a new flight record for the aircraft and callsign passed across.
        /// </summary>
        /// <param name="localNow"></param>
        /// <param name="aircraftId"></param>
        /// <param name="callsign"></param>
        /// <returns></returns>
        private BaseStationFlight CreateFlight(DateTime localNow, int aircraftId, string callsign)
        {
            var result = new BaseStationFlight() {
                FlightID = 0,
                AircraftID = aircraftId,
                SessionID = _Session.SessionID,
                Callsign = callsign ?? "",
                StartTime = localNow,
            };
            _Database.InsertFlight(result);

            return result;
        }

        /// <summary>
        /// Updates the FirstX / LastX pairs of values of an in-store flight record.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flight"></param>
        /// <param name="flightRecords"></param>
        /// <param name="isMlat"></param>
        private static void UpdateFirstLastValues(BaseStationMessage message, BaseStationFlight flight, FlightRecords flightRecords, bool isMlat)
        {
            bool isLocationZeroZero = message.Latitude.GetValueOrDefault() == 0F && message.Longitude.GetValueOrDefault() == 0F;

            if(message.Latitude != null && !isLocationZeroZero) {
                if(flight.FirstLat == null) flight.FirstLat = message.Latitude;
                flight.LastLat = message.Latitude;
            }
            if(message.Longitude != null && !isLocationZeroZero) {
                if(flight.FirstLon == null) flight.FirstLon = message.Longitude;
                flight.LastLon = message.Longitude;
            }
            if(message.Track != null) {
                if(flight.FirstTrack == null) flight.FirstTrack = message.Track;
                flight.LastTrack = message.Track;
            }

            if(!isMlat) {
                if(message.Altitude != null) {
                    if(flight.FirstAltitude == null) flight.FirstAltitude = message.Altitude;
                    flight.LastAltitude = message.Altitude;
                }
                if(message.GroundSpeed != null) {
                    if(flight.FirstGroundSpeed == null) flight.FirstGroundSpeed = message.GroundSpeed;
                    flight.LastGroundSpeed = message.GroundSpeed;
                }
                if(message.OnGround != null) {
                    if(flightRecords.OnGround == null) flightRecords.OnGround = flight.FirstIsOnGround = message.OnGround.Value;
                    flight.LastIsOnGround = message.OnGround.Value;
                }
                if(message.Squawk != null) {
                    if(flight.FirstSquawk == null) flight.FirstSquawk = message.Squawk;
                    flight.LastSquawk = message.Squawk;
                }
                if(message.VerticalRate != null) {
                    if(flight.FirstVerticalRate == null) flight.FirstVerticalRate = message.VerticalRate;
                    flight.LastVerticalRate = message.VerticalRate;
                }
            }
        }

        /// <summary>
        /// Updates the counters of total messages for a flight.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flight"></param>
        private static void UpdateMessageCounters(BaseStationMessage message, BaseStationFlight flight)
        {
            switch(message.TransmissionType) {
                case BaseStationTransmissionType.IdentificationAndCategory: ++flight.NumIDMsgRec; break;
                case BaseStationTransmissionType.SurfacePosition:           ++flight.NumSurPosMsgRec; break;
                case BaseStationTransmissionType.AirbornePosition:          ++flight.NumAirPosMsgRec; break;
                case BaseStationTransmissionType.AirborneVelocity:          ++flight.NumAirVelMsgRec; break;
                case BaseStationTransmissionType.SurveillanceAlt:           ++flight.NumSurAltMsgRec; break;
                case BaseStationTransmissionType.SurveillanceId:            ++flight.NumSurIDMsgRec; break;
                case BaseStationTransmissionType.AirToAir:                  ++flight.NumAirToAirMsgRec; break;
                case BaseStationTransmissionType.AllCallReply:              ++flight.NumAirCallRepMsgRec; break;
            }

            if(message.Latitude == null && message.Longitude == null && message.GroundSpeed == null && message.Track == null && message.VerticalRate == null) {
                ++flight.NumModeSMsgRec;
            } else {
                ++flight.NumADSBMsgRec;
                if(message.Latitude != null && message.Longitude != null) ++flight.NumPosMsgRec;
            }
        }

        /// <summary>
        /// Updates the flights that the plugin is tracking to show information about the beginning and end of the flight.
        /// </summary>
        /// <param name="flushAll"></param>
        private void FlushFlights(bool flushAll)
        {
            var utcNow = Provider.UtcNow;

            KeyValuePair<int, FlightRecords>[] flushEntries = null;
            lock(_SyncLock) {
                flushEntries = (flushAll ? _FlightMap : _FlightMap.Where(kvp => kvp.Value.EndTimeUtc.AddMinutes(25) <= utcNow)).ToArray();
            }

            foreach(var kvp in flushEntries) {
                _Database.UpdateFlight(kvp.Value.Flight);
                lock(_SyncLock) {
                    _FlightMap.Remove(kvp.Key);
                }
            }
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Raised before the BaseStation database's filename is updated to reflect configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_FileNameChanging(object sender, EventArgs args)
        {
            EndSession();
        }

        /// <summary>
        /// Raised after the BaseStation database's filename is changed as a result of configuration changes, either instigated
        /// via our configuration screen or the main configuration screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_FileNameChanged(object sender, EventArgs args)
        {
            StartSession();
        }

        /// <summary>
        /// Called when the listener changes source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MessageListener_SourceChanged(object sender, EventArgs args)
        {
            EndSession();
            StartSession();
        }

        /// <summary>
        /// Called by the listener when a BaseStation message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MessageListener_MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(_BackgroundThreadMessageQueue != null) _BackgroundThreadMessageQueue.Enqueue(args);
        }

        /// <summary>
        /// Called when the background queue pops a message off the queue of messages.
        /// </summary>
        /// <param name="args"></param>
        private void MessageQueue_MessageReceived(BaseStationMessageEventArgs args)
        {
            try {
                TrackFlight(args.Message, isMlat: args.IsOutOfBand);
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                AbandonSession(ex, PluginStrings.ExceptionCaughtWhenProcessingMessage);
                Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.MessageRelay_MessageReceived caught exception {0}", ex.ToString()));
                Factory.Singleton.ResolveSingleton<ILog>().WriteLine("Database writer plugin caught exception on message processing: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when an exception is allowed to bubble up from <see cref="MessageReceived"/>. This never happens,
        /// it's just here because the background thread queue object needs it.
        /// </summary>
        /// <param name="exception"></param>
        private void MessageQueue_ExceptionCaught(Exception exception)
        {
            ;
        }

        /// <summary>
        /// Called periodically on a background thread by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            try {
                WriteNewFlights();
                FlushFlights(false);
                if(StatusDescription == PluginStrings.DatabaseLocked) StatusDescription = null;
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                if(IsSQLiteLockException(ex)) {
                    StatusDescription = PluginStrings.DatabaseLocked;
                } else {
                    Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.Heartbeat_SlowTick caught exception {0}", ex.ToString()));
                    Factory.Singleton.ResolveSingleton<ILog>().WriteLine("Database writer plugin caught exception on flushing old flights: {0}", ex.ToString());
                    StatusDescription = String.Format(PluginStrings.ExceptionCaught, ex.Message);
                }
            }
        }

        /// <summary>
        /// Called when the standing data manager is loaded with new data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            RefreshMissingModeSCountries();
        }

        /// <summary>
        /// Called when the feed manager reports a change in feeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FeedManager_FeedsChanged(object sender, EventArgs args)
        {
            UnhookFeed();
            StartSession();
            HookFeed();
        }

        /// <summary>
        /// Called when the online cache changes status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnlineLookupCache_EnabledChanged(object sender, EventArgs args)
        {
            if(String.IsNullOrEmpty(StatusDescription) || StatusDescription == PluginStrings.WritingOnlineLookupsToDatabase) {
                StatusDescription = _OnlineLookupCache.Enabled ? PluginStrings.WritingOnlineLookupsToDatabase : null;
            }
        }
        #endregion
    }
}
