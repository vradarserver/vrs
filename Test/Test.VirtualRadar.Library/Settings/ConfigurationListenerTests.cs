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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.Settings
{
    [TestClass]
    public class ConfigurationListenerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IConfigurationListener _Listener;
        private Configuration _Configuration;
        private EventRecorder<ConfigurationListenerEventArgs> _PropertyChanged;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = new Configuration();
            _Listener = Factory.Resolve<IConfigurationListener>();

            _PropertyChanged = new EventRecorder<ConfigurationListenerEventArgs>();
            _Listener.PropertyChanged += _PropertyChanged.Handler;

            _Listener.Initialise(_Configuration);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _Listener.Dispose();
        }
        #endregion

        #region RaisedEvent, SetValue
        /// <summary>
        /// Returns true if an event with the given properties has been raised by the listener.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="propertyName"></param>
        /// <param name="record"></param>
        /// <param name="isListChild"></param>
        /// <returns></returns>
        private bool RaisedEvent(ConfigurationListenerGroup group, string propertyName, object record, bool isListChild = false)
        {
            int index = -1;
            for(var i = 0;i < _PropertyChanged.AllArgs.Count;++i) {
                var args = _PropertyChanged.AllArgs[i];
                if(args.Configuration == _Configuration &&
                   args.Group == group &&
                   args.IsListChild == isListChild &&
                   args.PropertyName == propertyName &&
                   args.Record == record) {
                    index = i;
                    break;
                }
            }

            object argsSender = index == -1 ? null : _PropertyChanged.AllSenders[index];

            return index != -1 && Object.ReferenceEquals(argsSender, _Listener);
        }

        /// <summary>
        /// Returns true if an event with the given properties has been raised by the listener.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="record"></param>
        /// <param name="isListChild"></param>
        /// <returns></returns>
        private bool RaisedEvent<T>(ConfigurationListenerGroup group, Expression<Func<T, object>> propertyExpression, object record, bool isListChild = false)
        {
            var propertyName = PropertyHelper.ExtractName<T>(propertyExpression);
            return RaisedEvent(group, propertyName, record, isListChild);
        }

        /// <summary>
        /// Sets a value on a settings object. Throws if no delegate is supplied for the property passed across. Returns false if
        /// the property name was found in the <paramref name="silentPropertyMap"/> map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <param name="propertyName"></param>
        /// <param name="raisePropertyMap"></param>
        /// <param name="silentPropertyMap"></param>
        /// <returns></returns>
        private bool SetValue<T>(T settings, string propertyName, Dictionary<Expression<Func<T, object>>, Action<T>> raisePropertyMap, Dictionary<Expression<Func<T, object>>, Action<T>> silentPropertyMap = null)
        {
            var result = true;

            Action<T> setAction = null;

            if(silentPropertyMap != null) {
                foreach(var kvp in silentPropertyMap) {
                    var silentPropertyName = PropertyHelper.ExtractName(kvp.Key);
                    if(silentPropertyName == propertyName) {
                        result = false;
                        setAction = kvp.Value;
                    }
                }
            }

            foreach(var kvp in raisePropertyMap) {
                var actionPropertyName = PropertyHelper.ExtractName(kvp.Key);
                if(actionPropertyName == propertyName) {
                    Assert.IsNull(setAction, "{0}.{1} was specified as both a silent property and a property that raises events", typeof(T).Name, propertyName);
                    setAction = kvp.Value;
                    break;
                }
            }

            if(setAction == null) {
                Assert.Fail("No set method was supplied for {0}.{1}", typeof(T).Name, propertyName);
            }
            setAction(settings);

            return result;
        }
        #endregion

        #region Audio
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_Audio_Changes()
        {
            foreach(var propertyName in typeof(AudioSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.AudioSettings;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<AudioSettings,object>>,Action<AudioSettings>>() {
                    { r => r.Enabled,   r => r.Enabled = !r.Enabled },
                    { r => r.VoiceName, r => r.VoiceName = "Hello" },
                    { r => r.VoiceRate, r => r.VoiceRate += 1 },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.Audio, propertyName, settings), "Audio.{0}", propertyName);
            }
        }
        #endregion

        #region BaseStationSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_BaseStationSettings_Changes()
        {
            foreach(var propertyName in typeof(BaseStationSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.BaseStationSettings;
                var testRaised = SetValue(settings, propertyName, new Dictionary<Expression<Func<BaseStationSettings,object>>,Action<BaseStationSettings>>() {
                    { r => r.AutoSavePolarPlotsMinutes,         r => r.AutoSavePolarPlotsMinutes += 1 },
                    { r => r.DatabaseFileName,                  r => r.DatabaseFileName = "TEST" },
                    { r => r.DisplayTimeoutSeconds,             r => r.DisplayTimeoutSeconds += 1 },
                    { r => r.MinimiseToSystemTray,              r => r.MinimiseToSystemTray = !r.MinimiseToSystemTray },
                    { r => r.OperatorFlagsFolder,               r => r.OperatorFlagsFolder = "TEST" },
                    { r => r.OutlinesFolder,                    r => r.OutlinesFolder = "TEST" },
                    { r => r.PicturesFolder,                    r => r.PicturesFolder = "TEST" },
                    { r => r.SearchPictureSubFolders,           r => r.SearchPictureSubFolders = !r.SearchPictureSubFolders },
                    { r => r.SilhouettesFolder,                 r => r.SilhouettesFolder = "TEST" },
                    { r => r.TrackingTimeoutSeconds,            r => r.TrackingTimeoutSeconds += 1 },
                    { r => r.LookupAircraftDetailsOnline,       r => r.LookupAircraftDetailsOnline = !r.LookupAircraftDetailsOnline },
                    { r => r.SatcomDisplayTimeoutMinutes,       r => r.SatcomDisplayTimeoutMinutes += 1 },
                    { r => r.SatcomTrackingTimeoutMinutes,      r => r.SatcomTrackingTimeoutMinutes += 1 },
                    { r => r.DownloadGlobalAirPressureReadings, r => r.DownloadGlobalAirPressureReadings = !r.DownloadGlobalAirPressureReadings },
                }, silentPropertyMap: new Dictionary<Expression<Func<BaseStationSettings,object>>, Action<BaseStationSettings>>() {
                    { r => r.Address,                       r => r.Address = "TEST" },
                    { r => r.AutoReconnectAtStartup,        r => r.AutoReconnectAtStartup = !r.AutoReconnectAtStartup },
                    { r => r.BaudRate,                      r => r.BaudRate += 1 },
                    { r => r.ComPort,                       r => r.ComPort = "TEST" },
                    { r => r.ConnectionType,                r => r.ConnectionType = ConnectionType.COM },
                    { r => r.DataBits,                      r => r.DataBits += 1 },
                    { r => r.DataSource,                    r => r.DataSource = DataSource.Beast },
                    { r => r.Handshake,                     r => r.Handshake = System.IO.Ports.Handshake.RequestToSendXOnXOff },
                    { r => r.IgnoreBadMessages,             r => r.IgnoreBadMessages = !r.IgnoreBadMessages },
                    { r => r.Parity,                        r => r.Parity = System.IO.Ports.Parity.Mark },
                    { r => r.Port,                          r => r.Port += 1 },
                    { r => r.ShutdownText,                  r => r.ShutdownText = "TEST" },
                    { r => r.StartupText,                   r => r.StartupText = "TEST" },
                    { r => r.StopBits,                      r => r.StopBits = System.IO.Ports.StopBits.OnePointFive },
                });

                if(testRaised) {
                    Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.BaseStation, propertyName, settings), "BaseStationSettings.{0}", propertyName);
                } else {
                    Assert.AreEqual(0, _PropertyChanged.CallCount, "BaseStationSettings.{0}", propertyName);
                }
            }
        }
        #endregion

        #region FlightRouteSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_FlightRouteSettings_Changes()
        {
            foreach(var propertyName in typeof(FlightRouteSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.FlightRouteSettings;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<FlightRouteSettings,object>>,Action<FlightRouteSettings>>() {
                    { r => r.AutoUpdateEnabled, r => r.AutoUpdateEnabled = !r.AutoUpdateEnabled },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.FlightRoute, propertyName, settings), "FlightRouteSettings.{0}", propertyName);
            }
        }
        #endregion

        #region GoogleMapSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_GoogleMapSettings_Changes()
        {
            foreach(var propertyName in typeof(GoogleMapSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.GoogleMapSettings;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<GoogleMapSettings,object>>,Action<GoogleMapSettings>>() {
                    { r => r.AllowCorsDomains,                      r => r.AllowCorsDomains = "TEST" },
                    { r => r.ClosestAircraftReceiverId,             r => r.ClosestAircraftReceiverId += 1 },
                    { r => r.DirectoryEntryKey,                     r => r.DirectoryEntryKey = "TEST" },
                    { r => r.EnableBundling,                        r => r.EnableBundling = !r.EnableBundling },
                    { r => r.EnableCompression,                     r => r.EnableCompression = !r.EnableCompression },
                    { r => r.EnableCorsSupport,                     r => r.EnableCorsSupport = !r.EnableCorsSupport },
                    { r => r.EnableMinifying,                       r => r.EnableMinifying = !r.EnableMinifying },
                    { r => r.FlightSimulatorXReceiverId,            r => r.FlightSimulatorXReceiverId += 1 },
                    { r => r.GoogleMapsApiKey,                      r => r.GoogleMapsApiKey = "TEST" },
                    { r => r.InitialDistanceUnit,                   r => r.InitialDistanceUnit = DistanceUnit.Kilometres },
                    { r => r.InitialHeightUnit,                     r => r.InitialHeightUnit = HeightUnit.Metres },
                    { r => r.InitialMapLatitude,                    r => r.InitialMapLatitude += 1 },
                    { r => r.InitialMapLongitude,                   r => r.InitialMapLongitude += 1 },
                    { r => r.InitialMapType,                        r => r.InitialMapType = "TEST" },
                    { r => r.InitialMapZoom,                        r => r.InitialMapZoom += 1 },
                    { r => r.InitialRefreshSeconds,                 r => r.InitialRefreshSeconds += 1 },
                    { r => r.InitialSettings,                       r => r.InitialSettings = "TEST" },
                    { r => r.InitialSpeedUnit,                      r => r.InitialSpeedUnit = SpeedUnit.MilesPerHour },
                    { r => r.MinimumRefreshSeconds,                 r => r.MinimumRefreshSeconds += 1 },
                    { r => r.PreferIataAirportCodes,                r => r.PreferIataAirportCodes = !r.PreferIataAirportCodes },
                    { r => r.ProxyType,                             r => r.ProxyType = ProxyType.Forward },
                    { r => r.ShortTrailLengthSeconds,               r => r.ShortTrailLengthSeconds += 1 },
                    { r => r.UseGoogleMapsAPIKeyWithLocalRequests,  r => r.UseGoogleMapsAPIKeyWithLocalRequests = !r.UseGoogleMapsAPIKeyWithLocalRequests },
                    { r => r.UseSvgGraphicsOnDesktop,               r => r.UseSvgGraphicsOnDesktop = !r.UseSvgGraphicsOnDesktop },
                    { r => r.UseSvgGraphicsOnMobile,                r => r.UseSvgGraphicsOnMobile = !r.UseSvgGraphicsOnMobile },
                    { r => r.UseSvgGraphicsOnReports,               r => r.UseSvgGraphicsOnReports = !r.UseSvgGraphicsOnReports },
                    { r => r.WebSiteReceiverId,                     r => r.WebSiteReceiverId += 1 },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.GoogleMapSettings, propertyName, settings), "GoogleMapSettings.{0}", propertyName);
            }
        }
        #endregion

        #region MergedFeed
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_MergedFeed_List_Changes()
        {
            _Configuration.MergedFeeds.Add(new MergedFeed());
            Assert.IsTrue(RaisedEvent<Configuration>(ConfigurationListenerGroup.Configuration, r => r.MergedFeeds, _Configuration, isListChild: false));
        }

        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_MergedFeed_Changes()
        {
            foreach(var propertyName in typeof(MergedFeed).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                _Configuration.MergedFeeds.Add(new MergedFeed());

                var settings = _Configuration.MergedFeeds[0];
                SetValue(settings, propertyName, new Dictionary<Expression<Func<MergedFeed,object>>,Action<MergedFeed>>() {
                    { r => r.Enabled,                       r => r.Enabled = !r.Enabled },
                    { r => r.IcaoTimeout,                   r => r.IcaoTimeout += 1 },
                    { r => r.IgnoreAircraftWithNoPosition,  r => r.IgnoreAircraftWithNoPosition = !r.IgnoreAircraftWithNoPosition },
                    { r => r.Name,                          r => r.Name = "TEST" },
                    { r => r.ReceiverIds,                   r => r.ReceiverIds.Add(100) },
                    { r => r.ReceiverFlags,                 r => r.ReceiverFlags.Add(new MergedFeedReceiver() { UniqueId = 100, IsMlatFeed = true }) },
                    { r => r.ReceiverUsage,                 r => r.ReceiverUsage = ReceiverUsage.MergeOnly },
                    { r => r.UniqueId,                      r => r.UniqueId += 1 },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.MergedFeed, propertyName, settings, isListChild: true), "MergedFeed.{0}", propertyName);
            }
        }

        [TestMethod]
        public void ConfigurationListener_Does_Not_Raise_Events_After_MergedFeed_Is_Detached()
        {
            var record = new MergedFeed();
            _Configuration.MergedFeeds.Add(record);

            _Configuration.MergedFeeds.Remove(record);
            record.UniqueId += 1;

            Assert.IsFalse(RaisedEvent<MergedFeed>(ConfigurationListenerGroup.MergedFeed, r => r.UniqueId, record, isListChild: true));
            Assert.AreEqual(2, _PropertyChanged.CallCount);
        }
        #endregion

        #region MergedFeedReceiver
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_MergedFeedRecevier_Changes()
        {
            foreach(var propertyName in typeof(MergedFeedReceiver).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var mergedFeed = new MergedFeed();
                var settings = new MergedFeedReceiver();
                _Configuration.MergedFeeds.Add(new MergedFeed() { ReceiverFlags = { settings } });

                SetValue(settings, propertyName, new Dictionary<Expression<Func<MergedFeedReceiver,object>>,Action<MergedFeedReceiver>>() {
                    { r => r.UniqueId,      r => r.UniqueId += 1 },
                    { r => r.IsMlatFeed,    r => r.IsMlatFeed = !r.IsMlatFeed },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.MergedFeedReceiver, propertyName, settings, isListChild: true), "MergedFeedReceiver.{0}", propertyName);
            }
        }

        [TestMethod]
        public void ConfigurationListener_Does_Not_Raise_Events_After_MergedFeedReceiver_Is_Detached()
        {
            var mergedFeed = new MergedFeed();
            var settings = new MergedFeedReceiver();
            _Configuration.MergedFeeds.Add(new MergedFeed() { ReceiverFlags = { settings } });

            _Configuration.MergedFeeds[0].ReceiverFlags.Remove(settings);
            settings.UniqueId += 1;

            Assert.IsFalse(RaisedEvent<MergedFeed>(ConfigurationListenerGroup.MergedFeedReceiver, r => r.UniqueId, settings, isListChild: true));
            Assert.AreEqual(2, _PropertyChanged.CallCount);
        }
        #endregion

        #region Mono
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_Mono_Settings_Change()
        {
            foreach(var propertyName in typeof(MonoSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.MonoSettings;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<MonoSettings,object>>,Action<MonoSettings>>() {
                    { r => r.UseMarkerLabels,   r => r.UseMarkerLabels = !r.UseMarkerLabels },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.MonoSettings, propertyName, settings), "Mono.{0}", propertyName);
            }
        }
        #endregion

        #region RawDecodingSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_RawDecodingSettings_Changes()
        {
            foreach(var propertyName in typeof(RawDecodingSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.RawDecodingSettings;
                var testRaised = SetValue(settings, propertyName, new Dictionary<Expression<Func<RawDecodingSettings,object>>,Action<RawDecodingSettings>>() {
                    { r => r.AcceptableAirborneSpeed,                   r => r.AcceptableAirborneSpeed += 1 },
                    { r => r.AcceptableAirSurfaceTransitionSpeed,       r => r.AcceptableAirSurfaceTransitionSpeed += 1 },
                    { r => r.AcceptableSurfaceSpeed,                    r => r.AcceptableSurfaceSpeed += 1 },
                    { r => r.AcceptIcaoInNonPICount,                    r => r.AcceptIcaoInNonPICount += 1 },
                    { r => r.AcceptIcaoInNonPISeconds,                  r => r.AcceptIcaoInNonPISeconds += 1 },
                    { r => r.AcceptIcaoInPI0Count,                      r => r.AcceptIcaoInPI0Count += 1 },
                    { r => r.AcceptIcaoInPI0Seconds,                    r => r.AcceptIcaoInPI0Seconds += 1 },
                    { r => r.AirborneGlobalPositionLimit,               r => r.AirborneGlobalPositionLimit += 1 },
                    { r => r.FastSurfaceGlobalPositionLimit,            r => r.FastSurfaceGlobalPositionLimit += 1 },
                    { r => r.IgnoreCallsignsInBds20,                    r => r.IgnoreCallsignsInBds20 = !r.IgnoreCallsignsInBds20 },
                    { r => r.IgnoreInvalidCodeBlockInOtherMessages,     r => r.IgnoreInvalidCodeBlockInOtherMessages = !r.IgnoreInvalidCodeBlockInOtherMessages },
                    { r => r.IgnoreInvalidCodeBlockInParityMessages,    r => r.IgnoreInvalidCodeBlockInParityMessages = !r.IgnoreInvalidCodeBlockInParityMessages },
                    { r => r.IgnoreMilitaryExtendedSquitter,            r => r.IgnoreMilitaryExtendedSquitter = !r.IgnoreMilitaryExtendedSquitter },
                    { r => r.ReceiverRange,                             r => r.ReceiverRange += 1 },
                    { r => r.SlowSurfaceGlobalPositionLimit,            r => r.SlowSurfaceGlobalPositionLimit += 1 },
                    { r => r.SuppressIcao0,                             r => r.SuppressIcao0 = !r.SuppressIcao0 },
                    { r => r.SuppressReceiverRangeCheck,                r => r.SuppressReceiverRangeCheck = !r.SuppressReceiverRangeCheck },
                    { r => r.SuppressTisbDecoding,                      r => r.SuppressTisbDecoding = !r.SuppressTisbDecoding },
                    { r => r.UseLocalDecodeForInitialPosition,          r => r.UseLocalDecodeForInitialPosition = !r.UseLocalDecodeForInitialPosition },
                }, silentPropertyMap: new Dictionary<Expression<Func<RawDecodingSettings,object>>,Action<RawDecodingSettings>>() {
                    { r => r.ReceiverLocationId,                    r => r.ReceiverLocationId += 1 },
                });

                if(testRaised) {
                    Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.RawDecodingSettings, propertyName, settings), "RawDecodingSettings.{0}", propertyName);
                } else {
                    Assert.AreEqual(0, _PropertyChanged.CallCount, propertyName);
                }
            }
        }
        #endregion

        #region RebroadcastSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_RebroadcastSettings_List_Changes()
        {
            _Configuration.RebroadcastSettings.Add(new RebroadcastSettings());
            Assert.IsTrue(RaisedEvent<Configuration>(ConfigurationListenerGroup.Configuration, r => r.RebroadcastSettings, _Configuration, isListChild: false));
        }

        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_RebroadcastSettings_Changes()
        {
            foreach(var propertyName in typeof(RebroadcastSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                _Configuration.RebroadcastSettings.Add(new RebroadcastSettings());

                var settings = _Configuration.RebroadcastSettings[0];
                SetValue(settings, propertyName, new Dictionary<Expression<Func<RebroadcastSettings,object>>,Action<RebroadcastSettings>>() {
                    { r => r.Access,                    r => r.Access = new Access() { DefaultAccess = DefaultAccess.Deny } },
                    { r => r.Enabled,                   r => r.Enabled = !r.Enabled },
                    { r => r.Format,                    r => r.Format = RebroadcastFormat.Avr },
                    { r => r.IdleTimeoutMilliseconds,   r => r.IdleTimeoutMilliseconds += 1 },
                    { r => r.IsTransmitter,             r => r.IsTransmitter = !r.IsTransmitter },
                    { r => r.Name,                      r => r.Name = "TEST" },
                    { r => r.Passphrase,                r => r.Passphrase = "TEST" },
                    { r => r.Port,                      r => r.Port += 1 },
                    { r => r.ReceiverId,                r => r.ReceiverId += 1 },
                    { r => r.SendIntervalMilliseconds,  r => r.SendIntervalMilliseconds += 1 },
                    { r => r.StaleSeconds,              r => r.StaleSeconds += 1 },
                    { r => r.TransmitAddress,           r => r.TransmitAddress += 1 },
                    { r => r.UniqueId,                  r => r.UniqueId += 1 },
                    { r => r.UseKeepAlive,              r => r.UseKeepAlive = !r.UseKeepAlive },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.RebroadcastSetting, propertyName, settings, isListChild: true), "RebroadcastSettings.{0}", propertyName);
            }
        }

        [TestMethod]
        public void ConfigurationListener_Does_Not_Raise_Events_After_RebroadcastSettings_Is_Detached()
        {
            var record = new RebroadcastSettings();
            _Configuration.RebroadcastSettings.Add(record);

            _Configuration.RebroadcastSettings.Remove(record);
            record.UniqueId += 1;

            Assert.IsFalse(RaisedEvent<RebroadcastSettings>(ConfigurationListenerGroup.RebroadcastSetting, r => r.UniqueId, record, isListChild: true));
            Assert.AreEqual(2, _PropertyChanged.CallCount);
        }
        #endregion

        #region RebroadcastSettings.Access
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_RebroadcastSettings_Access_Changes()
        {
            foreach(var propertyName in typeof(Access).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                _Configuration.RebroadcastSettings.Add(new RebroadcastSettings());

                var settings = _Configuration.RebroadcastSettings[0].Access;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<Access,object>>,Action<Access>>() {
                    { r => r.Addresses,     r => r.Addresses.Add("TEST") },
                    { r => r.DefaultAccess, r => r.DefaultAccess = DefaultAccess.Deny },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.Access, propertyName, settings, isListChild: true), "RebroadcastSettings.Access.{0}", propertyName);
            }
        }
        #endregion

        #region Receiver
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_Receiver_List_Changes()
        {
            _Configuration.Receivers.Add(new Receiver());
            Assert.IsTrue(RaisedEvent<Configuration>(ConfigurationListenerGroup.Configuration, r => r.Receivers, _Configuration, isListChild: false));
        }

        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_Receiver_Changes()
        {
            foreach(var propertyName in typeof(Receiver).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                _Configuration.Receivers.Add(new Receiver());

                var settings = _Configuration.Receivers[0];
                SetValue(settings, propertyName, new Dictionary<Expression<Func<Receiver,object>>,Action<Receiver>>() {
                    { r => r.Access,                    r => r.Access = new Access() { DefaultAccess = DefaultAccess.Deny } },
                    { r => r.Address,                   r => r.Address = "TEST" },
                    { r => r.AutoReconnectAtStartup,    r => r.AutoReconnectAtStartup = !r.AutoReconnectAtStartup },
                    { r => r.BaudRate,                  r => r.BaudRate += 1 },
                    { r => r.ComPort,                   r => r.ComPort = "TEST" },
                    { r => r.ConnectionType,            r => r.ConnectionType = ConnectionType.COM },
                    { r => r.DataBits,                  r => r.DataBits += 1 },
                    { r => r.DataSource,                r => r.DataSource = DataSource.Beast },
                    { r => r.Enabled,                   r => r.Enabled = !r.Enabled },
                    { r => r.Handshake,                 r => r.Handshake = System.IO.Ports.Handshake.RequestToSendXOnXOff },
                    { r => r.IdleTimeoutMilliseconds,   r => r.IdleTimeoutMilliseconds++ },
                    { r => r.IsPassive,                 r => r.IsPassive = !r.IsPassive },
                    { r => r.IsSatcomFeed,              r => r.IsSatcomFeed = !r.IsSatcomFeed },
                    { r => r.Name,                      r => r.Name = "TEST" },
                    { r => r.Parity,                    r => r.Parity = System.IO.Ports.Parity.Mark },
                    { r => r.Passphrase,                r => r.Passphrase = r.Passphrase + 'A' },
                    { r => r.Port,                      r => r.Port += 1 },
                    { r => r.ReceiverLocationId,        r => r.ReceiverLocationId += 1 },
                    { r => r.ReceiverUsage,             r => r.ReceiverUsage = ReceiverUsage.HideFromWebSite },
                    { r => r.ShutdownText,              r => r.ShutdownText = "TEST" },
                    { r => r.StartupText,               r => r.StartupText = "TEST" },
                    { r => r.StopBits,                  r => r.StopBits = System.IO.Ports.StopBits.OnePointFive },
                    { r => r.UniqueId,                  r => r.UniqueId += 1 },
                    { r => r.UseKeepAlive,              r => r.UseKeepAlive = !r.UseKeepAlive },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.Receiver, propertyName, settings, isListChild: true), "Receiver.{0}", propertyName);
            }
        }

        [TestMethod]
        public void ConfigurationListener_Does_Not_Raise_Events_After_Receiver_Is_Detached()
        {
            var record = new Receiver();
            _Configuration.Receivers.Add(record);

            _Configuration.Receivers.Remove(record);
            record.UniqueId += 1;

            Assert.IsFalse(RaisedEvent<Receiver>(ConfigurationListenerGroup.Receiver, r => r.UniqueId, record, isListChild: true));
            Assert.AreEqual(2, _PropertyChanged.CallCount);
        }
        #endregion

        #region ReceiverLocation
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_ReceiverLocation_List_Changes()
        {
            _Configuration.ReceiverLocations.Add(new ReceiverLocation());
            Assert.IsTrue(RaisedEvent<Configuration>(ConfigurationListenerGroup.Configuration, r => r.ReceiverLocations, _Configuration, isListChild: false));
        }

        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_ReceiverLocation_Changes()
        {
            foreach(var propertyName in typeof(ReceiverLocation).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                _Configuration.ReceiverLocations.Add(new ReceiverLocation());

                var settings = _Configuration.ReceiverLocations[0];
                SetValue(settings, propertyName, new Dictionary<Expression<Func<ReceiverLocation,object>>,Action<ReceiverLocation>>() {
                    { r => r.IsBaseStationLocation,     r => r.IsBaseStationLocation = !r.IsBaseStationLocation },
                    { r => r.Latitude,                  r => r.Latitude += 1 },
                    { r => r.Longitude,                 r => r.Longitude += 1 },
                    { r => r.Name,                      r => r.Name = "TEST" },
                    { r => r.UniqueId,                  r => r.UniqueId += 1 },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.ReceiverLocation, propertyName, settings, isListChild: true), "ReceiverLocation.{0}", propertyName);
            }
        }

        [TestMethod]
        public void ConfigurationListener_Does_Not_Raise_Events_After_ReceiverLocation_Is_Detached()
        {
            var record = new ReceiverLocation();
            _Configuration.ReceiverLocations.Add(record);

            _Configuration.ReceiverLocations.Remove(record);
            record.UniqueId += 1;

            Assert.IsFalse(RaisedEvent<ReceiverLocation>(ConfigurationListenerGroup.ReceiverLocation, r => r.UniqueId, record, isListChild: true));
            Assert.AreEqual(2, _PropertyChanged.CallCount);
        }
        #endregion

        #region VersionCheckSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_VersionCheckSettings_Changes()
        {
            foreach(var propertyName in typeof(VersionCheckSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.VersionCheckSettings;
                SetValue(settings, propertyName, new Dictionary<Expression<Func<VersionCheckSettings,object>>,Action<VersionCheckSettings>>() {
                    { r => r.CheckAutomatically,        r => r.CheckAutomatically = !r.CheckAutomatically },
                    { r => r.CheckPeriodDays,           r => r.CheckPeriodDays += 1 },
                });

                Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.VersionCheckSettings, propertyName, settings), "VersionCheckSettings.{0}", propertyName);
            }
        }
        #endregion

        #region WebServerSettings
        [TestMethod]
        public void ConfigurationListener_Raises_Events_When_WebServerSettings_Changes()
        {
            foreach(var propertyName in typeof(WebServerSettings).GetProperties().Select(r => r.Name)) {
                TestCleanup();
                TestInitialise();

                var settings = _Configuration.WebServerSettings;
                var testRaised = SetValue(settings, propertyName, new Dictionary<Expression<Func<WebServerSettings,object>>,Action<WebServerSettings>>() {
                    { r => r.AdministratorUserIds,              r => r.AdministratorUserIds.Add("1") },
                    { r => r.AuthenticationScheme,              r => r.AuthenticationScheme = System.Net.AuthenticationSchemes.IntegratedWindowsAuthentication },
                    { r => r.AutoStartUPnP,                     r => r.AutoStartUPnP = !r.AutoStartUPnP },
                    { r => r.BasicAuthenticationUserIds,        r => r.BasicAuthenticationUserIds.Add("Hello") },
                    { r => r.ConvertedUser,                     r => r.ConvertedUser = !r.ConvertedUser },
                    { r => r.EnableUPnp,                        r => r.EnableUPnp = !r.EnableUPnp },
                    { r => r.IsOnlyInternetServerOnLan,         r => r.IsOnlyInternetServerOnLan = !r.IsOnlyInternetServerOnLan },
                    { r => r.UPnpPort,                          r => r.UPnpPort += 1 },
                }, silentPropertyMap: new Dictionary<Expression<Func<WebServerSettings,object>>,Action<WebServerSettings>>() {
                    { r => r.BasicAuthenticationPasswordHash,   r => r.BasicAuthenticationPasswordHash = new Hash() },
                    { r => r.BasicAuthenticationUser,           r => r.BasicAuthenticationUser = "TEST" },
                });

                if(testRaised) {
                    Assert.IsTrue(RaisedEvent(ConfigurationListenerGroup.WebServerSettings, propertyName, settings), "WebServerSettings.{0}", propertyName);
                } else {
                    Assert.AreEqual(0, _PropertyChanged.CallCount, propertyName);
                }
            }
        }
        #endregion
    }
}
