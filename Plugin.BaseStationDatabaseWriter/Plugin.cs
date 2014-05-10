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
using VirtualRadar.Interface.StandingData;

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
            public DateTime UtcNow                      { get { return DateTime.UtcNow; } }
            public DateTime LocalNow                    { get { return DateTime.Now; } }
            public IOptionsView CreateOptionsView()     { return new WinForms.OptionsView(); }
            public bool FileExists(string fileName)     { return File.Exists(fileName); }
            public long FileSize(string fileName)       { return new FileInfo(fileName).Length; }
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
        private Dictionary<string, FlightRecords> _FlightMap = new Dictionary<string, FlightRecords>();

        /// <summary>
        /// The feed whose aircraft messages are being recorded in the database.
        /// </summary>
        private IFeed _Feed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for testing.
        /// </summary>
        public IPluginProvider Provider { get; set; }

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
        public string Name { get { return "Database Writer"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version { get { return "2.0.0"; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription { get; private set; }
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
            lock(_SyncLock) {
                var optionsStorage = new OptionsStorage();
                _Options = optionsStorage.Load(this);

                _Database = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
                _Database.FileNameChanging += BaseStationDatabase_FileNameChanging;
                _Database.FileNameChanged += BaseStationDatabase_FileNameChanged;

                _StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
                _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;

                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                feedManager.FeedsChanged += FeedManager_FeedsChanged;

                StartSession();

                // If we process messages on the same thread as the listener raises the message received event on then we
                // will be running on the same thread as the aircraft list. Our processing can take some time, particularly if many
                // database writes have to happen simultaneously on startup, so to avoid blocking the update of the aircraft list
                // we create a background thread and process the messages on that.
                _BackgroundThreadMessageQueue = new BackgroundThreadQueue<BaseStationMessageEventArgs>("BaseStationDatabaseWriterMessageQueue");
                _BackgroundThreadMessageQueue.StartBackgroundThread(MessageQueue_MessageReceived, MessageQueue_ExceptionCaught);

                HookFeed();

                Factory.Singleton.Resolve<IHeartbeatService>().Singleton.SlowTick += Heartbeat_SlowTick;
            }
        }
        #endregion

        #region GuiThreadStartup
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            if(_BackgroundThreadMessageQueue != null) _BackgroundThreadMessageQueue.Dispose();
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
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var configuration = configurationStorage.Load();

                view.PluginEnabled = _Options.Enabled;
                view.AllowUpdateOfOtherDatabases = _Options.AllowUpdateOfOtherDatabases;
                view.DatabaseFileName = configuration.BaseStationSettings.DatabaseFileName;
                view.ReceiverId = _Options.ReceiverId;

                if(view.DisplayView()) {
                    lock(_SyncLock) {
                        _Options.Enabled = view.PluginEnabled;
                        _Options.AllowUpdateOfOtherDatabases = view.AllowUpdateOfOtherDatabases;
                        _Options.ReceiverId = view.ReceiverId;
                        var optionsStorage = new OptionsStorage();
                        optionsStorage.Save(this, _Options);

                        configuration.BaseStationSettings.DatabaseFileName = view.DatabaseFileName;
                        configurationStorage.Save(configuration);

                        UnhookFeed();
                        bool optionsPermit = _Options.Enabled && (_Options.AllowUpdateOfOtherDatabases || DatabaseCreatedByPlugin());
                        if(_Session != null && !optionsPermit) EndSession();
                        StartSession();
                        HookFeed();
                    }
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
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId);
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
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId);
                if(feed != _Feed) {
                    if(_Feed != null) {
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
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;

            lock(_SyncLock) {
                if(_Session == null) {
                    if(!_Options.Enabled) {
                        Status = PluginStrings.Disabled;
                        StatusDescription = null;
                    } else if(_Options.ReceiverId == 0) {
                        Status = PluginStrings.EnabledNoReceiver;
                    } else if(feedManager.GetByUniqueId(_Options.ReceiverId) == null) {
                        Status = PluginStrings.EnabledBadReceiver;
                    } else if(String.IsNullOrEmpty(_Database.FileName)) {
                        Status = PluginStrings.EnabledNoDatabase;
                        StatusDescription = null;
                    } else if(!Provider.FileExists(_Database.FileName)) {
                        Status = PluginStrings.EnabledNotUpdating;
                        StatusDescription = String.Format(PluginStrings.SomethingDoesNotExist, _Database.FileName);
                    } else if(Provider.FileSize(_Database.FileName) == 0L) {
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
                        } catch(ThreadAbortException) {
                        } catch(Exception ex) {
                            Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.StartSession caught exception {0}", ex.ToString()));
                            Status = PluginStrings.EnabledNotUpdating;
                            StatusDescription = String.Format(PluginStrings.ExceptionCaughtWhenStartingSession, ex.Message);

                            Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Database writer plugin caught exception on starting session: {0}", ex.ToString());
                        }
                    }
                    OnStatusChanged(EventArgs.Empty);
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
                        Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.EndSession caught exception {0}", ex.ToString()));
                        StatusDescription = String.Format(PluginStrings.ExceptionCaughtWhenClosingSession, ex.Message);
                        Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Database writer plugin caught exception on closing session: {0}", ex.ToString());
                    } finally {
                        _Session = null;
                    }
                    OnStatusChanged(EventArgs.Empty);
                }
            }
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
        private void TrackFlight(BaseStationMessage message)
        {
            if(IsTransmissionMessage(message)) {
                lock(_SyncLock) {
                    if(_Session != null) {
                        var localNow = Provider.LocalNow;

                        FlightRecords flightRecords;
                        if(!_FlightMap.TryGetValue(message.Icao24, out flightRecords)) {
                            flightRecords = new FlightRecords();
                            _Database.StartTransaction();
                            try {
                                flightRecords.Aircraft = FetchOrCreateAircraft(localNow, message.Icao24);
                                flightRecords.Flight = CreateFlight(localNow, flightRecords.Aircraft.AircraftID, message.Callsign);
                                flightRecords.EndTimeUtc = Provider.UtcNow;
                                _Database.EndTransaction();
                            } catch(ThreadAbortException) {
                            } catch(Exception ex) {
                                Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.TrackFlight caught exception {0}", ex.ToString()));
                                _Database.RollbackTransaction();
                                throw;
                            }
                            _FlightMap.Add(message.Icao24, flightRecords);
                        } else {
                            if(flightRecords.Flight.Callsign.Length == 0 && !String.IsNullOrEmpty(message.Callsign)) {
                                var databaseVersion = _Database.GetFlightById(flightRecords.Flight.FlightID);
                                flightRecords.Flight.Callsign = databaseVersion.Callsign = message.Callsign;
                                _Database.UpdateFlight(databaseVersion);
                            }
                        }

                        var flight = flightRecords.Flight;
                        flightRecords.EndTimeUtc = Provider.UtcNow;
                        flight.EndTime = localNow;
                        if(message.SquawkHasChanged.GetValueOrDefault()) flight.HadAlert = true;
                        if(message.IdentActive.GetValueOrDefault()) flight.HadSpi = true;
                        if(message.Squawk == 7500 || message.Squawk == 7600 || message.Squawk == 7700) flight.HadEmergency = true;
                        UpdateFirstLastValues(message, flight, flightRecords);
                        UpdateMessageCounters(message, flight);
                    }
                }
            }
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
            var result = _Database.GetAircraftByCode(icao24);
            if(result == null) {
                var codeBlock = _StandingDataManager.FindCodeBlock(icao24);
                result = new BaseStationAircraft() {
                    AircraftID = 0,
                    ModeS = icao24,
                    FirstCreated = now,
                    LastModified = now,
                    ModeSCountry = codeBlock == null ? null : codeBlock.Country,
                };
                _Database.InsertAircraft(result);
            }

            return result;
        }

        /// <summary>
        /// Refreshes the Mode-S countries on tracked aircraft that don't have them.
        /// </summary>
        private void RefreshMissingModeSCountries()
        {
            lock(_SyncLock) {
                if(_Session != null) {
                    var standingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;

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
                Callsign = String.IsNullOrEmpty(callsign) ? "" : callsign,
                StartTime = localNow,
            };
            _Database.InsertFlight(result);

            result.NumADSBMsgRec = 0;
            result.NumAirCallRepMsgRec = 0;
            result.NumAirPosMsgRec = 0;
            result.NumAirToAirMsgRec = 0;
            result.NumAirVelMsgRec = 0;
            result.NumIDMsgRec = 0;
            result.NumModeSMsgRec = 0;
            result.NumPosMsgRec = 0;
            result.NumSurAltMsgRec = 0;
            result.NumSurIDMsgRec = 0;
            result.NumSurPosMsgRec = 0;

            return result;
        }

        /// <summary>
        /// Updates the FirstX / LastX pairs of values of an in-store flight record.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flight"></param>
        /// <param name="flightRecords"></param>
        private static void UpdateFirstLastValues(BaseStationMessage message, BaseStationFlight flight, FlightRecords flightRecords)
        {
            bool isLocationZeroZero = message.Latitude.GetValueOrDefault() == 0F && message.Longitude.GetValueOrDefault() == 0F;

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
            if(message.Latitude != null && !isLocationZeroZero) {
                if(flight.FirstLat == null) flight.FirstLat = message.Latitude;
                flight.LastLat = message.Latitude;
            }
            if(message.Longitude != null && !isLocationZeroZero) {
                if(flight.FirstLon == null) flight.FirstLon = message.Longitude;
                flight.LastLon = message.Longitude;
            }
            if(message.Squawk != null) {
                if(flight.FirstSquawk == null) flight.FirstSquawk = message.Squawk;
                flight.LastSquawk = message.Squawk;
            }
            if(message.Track != null) {
                if(flight.FirstTrack == null) flight.FirstTrack = message.Track;
                flight.LastTrack = message.Track;
            }
            if(message.VerticalRate != null) {
                if(flight.FirstVerticalRate == null) flight.FirstVerticalRate = message.VerticalRate;
                flight.LastVerticalRate = message.VerticalRate;
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
            lock(_SyncLock) {
                var utcNow = Provider.UtcNow;
                var flushEntries = (flushAll ? _FlightMap : _FlightMap.Where(kvp => kvp.Value.EndTimeUtc.AddMinutes(25) <= utcNow)).ToArray();

                foreach(var kvp in flushEntries) {
                    _FlightMap.Remove(kvp.Key);
                    _Database.UpdateFlight(kvp.Value.Flight);
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
                TrackFlight(args.Message);
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.MessageRelay_MessageReceived caught exception {0}", ex.ToString()));
                Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Database writer plugin caught exception on message processing: {0}", ex.ToString());
                StatusDescription = String.Format(PluginStrings.ExceptionCaught, ex.Message);
                OnStatusChanged(EventArgs.Empty);
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
                FlushFlights(false);
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.Heartbeat_SlowTick caught exception {0}", ex.ToString()));
                Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Database writer plugin caught exception on flushing old flights: {0}", ex.ToString());
                StatusDescription = String.Format(PluginStrings.ExceptionCaught, ex.Message);
                OnStatusChanged(EventArgs.Empty);
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
        #endregion
    }
}
