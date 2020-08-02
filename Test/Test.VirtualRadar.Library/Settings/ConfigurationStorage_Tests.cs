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
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using Test.VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Library;
using VirtualRadar.Library.Settings;
using Moq;
using System.IO;

namespace Test.VirtualRadar.Library.Settings
{
    [TestClass]
    public class ConfigurationStorage_Tests
    {
        #region Private class - TestProvider
        class TestProvider : IConfigurationStorageProvider
        {
            public string Folder { get; set; }
        }
        #endregion

        #region TestContext, fields, TestInitialise, TestCleanup
        public TestContext                  TestContext { get; set; }
        private IClassFactory               _OriginalFactory;
        private IConfigurationStorage       _Implementation;
        private TestProvider                _Provider;
        private EventRecorder<EventArgs>    _ConfigurationChangedEvent;
        private Mock<IUserManager>          _UserManager;
        private Mock<IUser>                 _User;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Provider = new TestProvider();
            _Provider.Folder = TestContext.TestDeploymentDir;
            _Implementation = Factory.ResolveNewInstance<IConfigurationStorage>();
            _Implementation.Provider = _Provider;

            _ConfigurationChangedEvent = new EventRecorder<EventArgs>();

            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();
            _UserManager.Setup(r => r.CanCreateUsers).Returns(true);
            _UserManager.Setup(r => r.CanCreateUsersWithHash).Returns(true);
            _UserManager.Setup(r => r.GetUserByLoginName(It.IsAny<string>())).Returns((IUser)null);
            _User = TestUtilities.CreateMockImplementation<IUser>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            
            var settingsFileName = Path.Combine(TestContext.TestDeploymentDir, "Configuration.xml");
            if(File.Exists(settingsFileName)) File.Delete(settingsFileName);
        }
        #endregion

        #region Ctor and properties
        [TestMethod]
        public void Ctor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Implementation = Factory.ResolveNewInstance<IConfigurationStorage>();
            Assert.IsNotNull(_Implementation.Provider);
            Assert.AreNotSame(_Provider, _Implementation.Provider);
            _Implementation.Provider = _Provider;
            Assert.AreSame(_Provider, _Implementation.Provider);
        }

        [TestMethod]
        public void Folder_Setter_Stores_Folder()
        {
            _Implementation.Folder = "xX";
            Assert.AreEqual("xX", _Provider.Folder);
        }

        [TestMethod]
        public void Folder_Getter_Retrieves_Folder()
        {
            Assert.AreEqual(TestContext.TestDeploymentDir, _Implementation.Folder);
        }
        #endregion

        #region Erase
        [TestMethod]
        public void Erase_Deletes_Existing_Settings()
        {
            _Implementation.Save(CreateKnownConfiguration());
            _Implementation.Erase();

            Configuration configuration = _Implementation.Load();
            foreach(PropertyInfo property in configuration.GetType().GetProperties()) {
                switch(property.Name) {
                    case nameof(Configuration.BaseStationSettings):     BaseStationSettingsTests.CheckProperties(configuration.BaseStationSettings); break;
                    case nameof(Configuration.FlightRouteSettings):     FlightRouteSettingsTests.CheckProperties(configuration.FlightRouteSettings); break;
                    case nameof(Configuration.WebServerSettings):       WebServerSettingsTests.CheckProperties(configuration.WebServerSettings); break;
                    case nameof(Configuration.GoogleMapSettings):       GoogleMapSettingsTests.CheckProperties(configuration.GoogleMapSettings, assumeInitialConfig: true); break;
                    case nameof(Configuration.VersionCheckSettings):    VersionCheckSettingsTests.CheckProperties(configuration.VersionCheckSettings); break;
                    case nameof(Configuration.InternetClientSettings):  InternetClientSettingsTests.CheckProperties(configuration.InternetClientSettings); break;
                    case nameof(Configuration.AudioSettings):           AudioSettingsTests.CheckProperties(configuration.AudioSettings); break;
                    case nameof(Configuration.RawDecodingSettings):     RawDecodingSettingsTests.CheckProperties(configuration.RawDecodingSettings); break;
                    case nameof(Configuration.ReceiverLocations):       Assert.AreEqual(0, configuration.ReceiverLocations.Count); break;
                    case nameof(Configuration.RebroadcastSettings):     Assert.AreEqual(0, configuration.RebroadcastSettings.Count); break;
                    case nameof(Configuration.Receivers):
                        Assert.AreEqual(1, configuration.Receivers.Count);
                        ReceiverTests.CheckProperties(configuration.Receivers[0], assumeInitialConfig: true);
                        break;
                    case nameof(Configuration.MergedFeeds):             Assert.AreEqual(0, configuration.MergedFeeds.Count); break;
                    case nameof(Configuration.DataVersion):             Assert.AreEqual(0, configuration.DataVersion); break;
                    case nameof(Configuration.MonoSettings):            MonoSettingsTests.CheckProperties(configuration.MonoSettings); break;
                    case nameof(Configuration.StateHistorySettings):    StateHistorySettings_Tests.CheckProperties(configuration.StateHistorySettings); break;
                    default:                                            Assert.Fail("Missing {0} property test", property.Name); break;
                }
            }
        }

        [TestMethod]
        public void Erase_Raises_ConfigurationChanged()
        {
            var configuration = new Configuration() { AudioSettings = new AudioSettings() { VoiceName = "This was saved" } };
            _Implementation.Save(configuration);

            _Implementation.ConfigurationChanged += _ConfigurationChangedEvent.Handler;
            _ConfigurationChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual(null, _Implementation.Load().AudioSettings.VoiceName); };

            _Implementation.Erase();

            Assert.AreEqual(1, _ConfigurationChangedEvent.CallCount);
            Assert.IsNotNull(_ConfigurationChangedEvent.Args);
            Assert.AreSame(_Implementation, _ConfigurationChangedEvent.Sender);
        }
        #endregion

        #region Save and Load
        [TestMethod]
        public void Save_Raises_ConfigurationChanged()
        {
            var configuration = new Configuration() { AudioSettings = new AudioSettings() { VoiceName = "This was saved" } };
            _Implementation.ConfigurationChanged += _ConfigurationChangedEvent.Handler;
            _ConfigurationChangedEvent.EventRaised += (object sender, EventArgs args) => { Assert.AreEqual("This was saved", _Implementation.Load().AudioSettings.VoiceName); };

            _Implementation.Save(configuration);
            Assert.AreEqual(1, _ConfigurationChangedEvent.CallCount);
            Assert.IsNotNull(_ConfigurationChangedEvent.Args);
            Assert.AreSame(_Implementation, _ConfigurationChangedEvent.Sender);
        }

        [TestMethod]
        public void Save_Increments_DataVersion()
        {
            var config = _Implementation.Load();
            var initialDataVersion = config.DataVersion;

            _Implementation.Save(config);

            Assert.AreEqual(initialDataVersion + 1, config.DataVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictingUpdateException))]
        public void Save_Rejects_Unexpected_DataVersion()
        {
            var config = _Implementation.Load();
            _Implementation.Save(config);

            --config.DataVersion;
            _Implementation.Save(config);
        }

        [TestMethod]
        public void Save_Backs_Up_Current_Configuration()
        {
            var config = _Implementation.Load();
            _Implementation.Save(config);

            var configFileName = Path.Combine(_Implementation.Folder, "Configuration.xml");
            var oldConfigContent = File.ReadAllText(configFileName);

            var backupFolder = Path.Combine(_Implementation.Folder, "ConfigBackups");
            if(Directory.Exists(backupFolder)) {
                Directory.Delete(backupFolder, recursive: true);
            }

            config.AudioSettings.Enabled = !config.AudioSettings.Enabled;
            _Implementation.Save(config);

            Assert.IsTrue(Directory.Exists(backupFolder));
            var backupFileName = Directory.GetFiles(backupFolder, "Configuration-*.xml").Single();
            var backupContent = File.ReadAllText(backupFileName);
            Assert.AreEqual(oldConfigContent, backupContent);
        }

        [TestMethod]
        public void Load_Asks_XmlSerialiser_To_Use_Defaults_If_Enum_Unrecognised()
        {
            var config = new Configuration();

            var xmlSerialiser = TestUtilities.CreateMockImplementation<IXmlSerialiser>();
            xmlSerialiser.Object.UseDefaultEnumValueIfUnknown = false;
            xmlSerialiser.Setup(r => r.Deserialise<Configuration>(It.IsAny<Stream>())).Returns(config);
            xmlSerialiser.Setup(r => r.Deserialise<Configuration>(It.IsAny<TextReader>())).Returns(config);

            _Implementation = Factory.Singleton.Resolve<IConfigurationStorage>();
            _Implementation.Load();

            Assert.AreEqual(true, xmlSerialiser.Object.UseDefaultEnumValueIfUnknown);
        }

        [TestMethod]
        public void Save_And_Load_Can_Roundtrip_Values()
        {
            var configuration = CreateKnownConfiguration();
            _Implementation.Save(configuration);

            var readBack = _Implementation.Load();
            foreach(PropertyInfo property in configuration.GetType().GetProperties()) {
                switch(property.Name) {
                    case nameof(Configuration.BaseStationSettings):
                        Assert.AreEqual("Aa", readBack.BaseStationSettings.Address);
                        Assert.AreEqual(100, readBack.BaseStationSettings.AutoSavePolarPlotsMinutes);
                        Assert.AreEqual(65535, readBack.BaseStationSettings.Port);
                        Assert.AreEqual("/dev/com4", readBack.BaseStationSettings.ComPort);
                        Assert.AreEqual(2400, readBack.BaseStationSettings.BaudRate);
                        Assert.AreEqual(7, readBack.BaseStationSettings.DataBits);
                        Assert.AreEqual(StopBits.OnePointFive, readBack.BaseStationSettings.StopBits);
                        Assert.AreEqual(Parity.Space, readBack.BaseStationSettings.Parity);
                        Assert.AreEqual(Handshake.XOnXOff, readBack.BaseStationSettings.Handshake);
                        Assert.AreEqual("startup", readBack.BaseStationSettings.StartupText);
                        Assert.AreEqual("shutdown", readBack.BaseStationSettings.ShutdownText);
                        Assert.AreEqual("Bb", readBack.BaseStationSettings.DatabaseFileName);
                        Assert.AreEqual(DataSource.Sbs3, readBack.BaseStationSettings.DataSource);
                        Assert.AreEqual("Cc", readBack.BaseStationSettings.OperatorFlagsFolder);
                        Assert.AreEqual("Ee", readBack.BaseStationSettings.SilhouettesFolder);
                        Assert.AreEqual("Ff", readBack.BaseStationSettings.OutlinesFolder);
                        Assert.AreEqual(120, readBack.BaseStationSettings.DisplayTimeoutSeconds);
                        Assert.AreEqual("Pp", readBack.BaseStationSettings.PicturesFolder);
                        Assert.AreEqual(true, readBack.BaseStationSettings.IgnoreBadMessages);
                        Assert.AreEqual(true, readBack.BaseStationSettings.AutoReconnectAtStartup);
                        Assert.AreEqual(ConnectionType.COM, readBack.BaseStationSettings.ConnectionType);
                        Assert.AreEqual(3600, readBack.BaseStationSettings.TrackingTimeoutSeconds);
                        Assert.AreEqual(true, readBack.BaseStationSettings.MinimiseToSystemTray);
                        Assert.AreEqual(true, readBack.BaseStationSettings.SearchPictureSubFolders);
                        Assert.AreEqual(false, readBack.BaseStationSettings.DownloadGlobalAirPressureReadings);
                        Assert.AreEqual(300, readBack.BaseStationSettings.SatcomDisplayTimeoutMinutes);
                        Assert.AreEqual(400, readBack.BaseStationSettings.SatcomTrackingTimeoutMinutes);
                        break;
                    case nameof(Configuration.FlightRouteSettings):
                        Assert.AreEqual(true, readBack.FlightRouteSettings.AutoUpdateEnabled);
                        break;
                    case nameof(Configuration.WebServerSettings):
                        Assert.AreEqual(AuthenticationSchemes.Basic, readBack.WebServerSettings.AuthenticationScheme);
                        Assert.AreEqual("Tt", readBack.WebServerSettings.BasicAuthenticationUser);
                        Assert.AreEqual(true, readBack.WebServerSettings.BasicAuthenticationPasswordHash.PasswordMatches("Funk to funky"));
                        Assert.AreEqual(true, readBack.WebServerSettings.EnableUPnp);
                        Assert.AreEqual(8181, readBack.WebServerSettings.UPnpPort);
                        Assert.AreEqual(false, readBack.WebServerSettings.IsOnlyInternetServerOnLan);
                        Assert.AreEqual(true, readBack.WebServerSettings.AutoStartUPnP);
                        Assert.AreEqual(true, readBack.WebServerSettings.ConvertedUser);
                        Assert.AreEqual(2, readBack.WebServerSettings.BasicAuthenticationUserIds.Count);
                        Assert.AreEqual("First", readBack.WebServerSettings.BasicAuthenticationUserIds[0]);
                        Assert.AreEqual("Second", readBack.WebServerSettings.BasicAuthenticationUserIds[1]);
                        break;
                    case nameof(Configuration.GoogleMapSettings):
                        Assert.AreEqual("SomeSettings", readBack.GoogleMapSettings.InitialSettings);
                        Assert.AreEqual(-12.123456, readBack.GoogleMapSettings.InitialMapLatitude);
                        Assert.AreEqual(120.123987, readBack.GoogleMapSettings.InitialMapLongitude);
                        Assert.AreEqual("HYBRID", readBack.GoogleMapSettings.InitialMapType);
                        Assert.AreEqual(7, readBack.GoogleMapSettings.InitialMapZoom);
                        Assert.AreEqual(8, readBack.GoogleMapSettings.MinimumRefreshSeconds);
                        Assert.AreEqual(9, readBack.GoogleMapSettings.InitialRefreshSeconds);
                        Assert.AreEqual(400, readBack.GoogleMapSettings.ShortTrailLengthSeconds);
                        Assert.AreEqual(DistanceUnit.Kilometres, readBack.GoogleMapSettings.InitialDistanceUnit);
                        Assert.AreEqual(HeightUnit.Metres, readBack.GoogleMapSettings.InitialHeightUnit);
                        Assert.AreEqual(SpeedUnit.MilesPerHour, readBack.GoogleMapSettings.InitialSpeedUnit);
                        Assert.AreEqual(true, readBack.GoogleMapSettings.PreferIataAirportCodes);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.EnableBundling);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.EnableMinifying);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.EnableCompression);
                        Assert.AreEqual(123, readBack.GoogleMapSettings.WebSiteReceiverId);
                        Assert.AreEqual(456, readBack.GoogleMapSettings.ClosestAircraftReceiverId);
                        Assert.AreEqual(789, readBack.GoogleMapSettings.FlightSimulatorXReceiverId);
                        Assert.AreEqual(ProxyType.Reverse, readBack.GoogleMapSettings.ProxyType);
                        Assert.AreEqual("ABC123-456", readBack.GoogleMapSettings.DirectoryEntryKey);
                        Assert.AreEqual(true, readBack.GoogleMapSettings.EnableCorsSupport);
                        Assert.AreEqual("ABC.COM", readBack.GoogleMapSettings.AllowCorsDomains);
                        Assert.AreEqual("API Key", readBack.GoogleMapSettings.GoogleMapsApiKey);
                        Assert.AreEqual(true, readBack.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.UseSvgGraphicsOnDesktop);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.UseSvgGraphicsOnMobile);
                        Assert.AreEqual(false, readBack.GoogleMapSettings.UseSvgGraphicsOnReports);
                        Assert.AreEqual(MapProvider.Leaflet, readBack.GoogleMapSettings.MapProvider);
                        Assert.AreEqual("My Tile Server", readBack.GoogleMapSettings.TileServerSettingName);
                        break;
                    case nameof(Configuration.VersionCheckSettings):
                        Assert.AreEqual(false, readBack.VersionCheckSettings.CheckAutomatically);
                        Assert.AreEqual(12, readBack.VersionCheckSettings.CheckPeriodDays);
                        break;
                    case nameof(Configuration.InternetClientSettings):
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanRunReports);
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanShowPictures);
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanShowPinText);
                        Assert.AreEqual(15, readBack.InternetClientSettings.TimeoutMinutes);
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanPlayAudio);
                        Assert.AreEqual(true, readBack.InternetClientSettings.AllowInternetProximityGadgets);
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanSubmitRoutes);
                        Assert.AreEqual(true, readBack.InternetClientSettings.CanShowPolarPlots);
                        break;
                    case nameof(Configuration.AudioSettings):
                        Assert.AreEqual(false, readBack.AudioSettings.Enabled);
                        Assert.AreEqual("Male Voice", readBack.AudioSettings.VoiceName);
                        Assert.AreEqual(2, readBack.AudioSettings.VoiceRate);
                        break;
                    case nameof(Configuration.RawDecodingSettings):
                        Assert.AreEqual(17.25, readBack.RawDecodingSettings.AcceptableAirborneSpeed);
                        Assert.AreEqual(19.75, readBack.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed);
                        Assert.AreEqual(21.25, readBack.RawDecodingSettings.AcceptableSurfaceSpeed);
                        Assert.AreEqual(19, readBack.RawDecodingSettings.AirborneGlobalPositionLimit);
                        Assert.AreEqual(17, readBack.RawDecodingSettings.FastSurfaceGlobalPositionLimit);
                        Assert.AreEqual(12, readBack.RawDecodingSettings.SlowSurfaceGlobalPositionLimit);
                        Assert.AreEqual(true, readBack.RawDecodingSettings.IgnoreMilitaryExtendedSquitter);
                        Assert.AreEqual(true, readBack.RawDecodingSettings.IgnoreInvalidCodeBlockInParityMessages);
                        Assert.AreEqual(false, readBack.RawDecodingSettings.IgnoreInvalidCodeBlockInOtherMessages);
                        Assert.AreEqual(7, readBack.RawDecodingSettings.ReceiverLocationId);
                        Assert.AreEqual(400, readBack.RawDecodingSettings.ReceiverRange);
                        Assert.AreEqual(false, readBack.RawDecodingSettings.SuppressReceiverRangeCheck);
                        Assert.AreEqual(true, readBack.RawDecodingSettings.UseLocalDecodeForInitialPosition);
                        Assert.AreEqual(true, readBack.RawDecodingSettings.IgnoreCallsignsInBds20);
                        Assert.AreEqual(20, readBack.RawDecodingSettings.AcceptIcaoInNonPICount);
                        Assert.AreEqual(21, readBack.RawDecodingSettings.AcceptIcaoInNonPISeconds);
                        Assert.AreEqual(7, readBack.RawDecodingSettings.AcceptIcaoInPI0Count);
                        Assert.AreEqual(42, readBack.RawDecodingSettings.AcceptIcaoInPI0Seconds);
                        Assert.AreEqual(false, readBack.RawDecodingSettings.SuppressIcao0);
                        Assert.AreEqual(true, readBack.RawDecodingSettings.SuppressTisbDecoding);
                        Assert.AreEqual(false, readBack.RawDecodingSettings.AssumeDF18CF1IsIcao);
                        break;
                    case nameof(Configuration.ReceiverLocations):
                        Assert.AreEqual(2, readBack.ReceiverLocations.Count);

                        Assert.AreEqual(74, readBack.ReceiverLocations[0].UniqueId);
                        Assert.AreEqual("Home", readBack.ReceiverLocations[0].Name);
                        Assert.AreEqual(51.234, readBack.ReceiverLocations[0].Latitude);
                        Assert.AreEqual(-0.642, readBack.ReceiverLocations[0].Longitude);

                        Assert.AreEqual(90, readBack.ReceiverLocations[1].UniqueId);
                        Assert.AreEqual("Away", readBack.ReceiverLocations[1].Name);
                        Assert.AreEqual(17.123, readBack.ReceiverLocations[1].Latitude);
                        Assert.AreEqual(12.456, readBack.ReceiverLocations[1].Longitude);
                        break;
                    case nameof(Configuration.RebroadcastSettings):
                        Assert.AreEqual(2, readBack.RebroadcastSettings.Count);

                        Assert.AreEqual(1, readBack.RebroadcastSettings[0].UniqueId);
                        Assert.AreEqual(true, readBack.RebroadcastSettings[0].Enabled);
                        Assert.AreEqual(RebroadcastFormat.Passthrough, readBack.RebroadcastSettings[0].Format);
                        Assert.AreEqual("Server 1", readBack.RebroadcastSettings[0].Name);
                        Assert.AreEqual(false, readBack.RebroadcastSettings[0].IsTransmitter);
                        Assert.AreEqual("unused", readBack.RebroadcastSettings[0].TransmitAddress);
                        Assert.AreEqual(10000, readBack.RebroadcastSettings[0].Port);
                        Assert.AreEqual(true, readBack.RebroadcastSettings[0].UseKeepAlive);
                        Assert.AreEqual(15000, readBack.RebroadcastSettings[0].IdleTimeoutMilliseconds);
                        Assert.AreEqual(-1, readBack.RebroadcastSettings[0].ReceiverId);
                        Assert.AreEqual(7, readBack.RebroadcastSettings[0].StaleSeconds);
                        Assert.AreEqual(null, readBack.RebroadcastSettings[0].Passphrase);
                        Assert.AreEqual(1000, readBack.RebroadcastSettings[0].SendIntervalMilliseconds);

                        Assert.AreEqual(2, readBack.RebroadcastSettings[1].UniqueId);
                        Assert.AreEqual(false, readBack.RebroadcastSettings[1].Enabled);
                        Assert.AreEqual(RebroadcastFormat.Port30003, readBack.RebroadcastSettings[1].Format);
                        Assert.AreEqual("Server 2", readBack.RebroadcastSettings[1].Name);
                        Assert.AreEqual(true, readBack.RebroadcastSettings[1].IsTransmitter);
                        Assert.AreEqual("remote.address.com", readBack.RebroadcastSettings[1].TransmitAddress);
                        Assert.AreEqual(10001, readBack.RebroadcastSettings[1].Port);
                        Assert.AreEqual(false, readBack.RebroadcastSettings[1].UseKeepAlive);
                        Assert.AreEqual(20000, readBack.RebroadcastSettings[1].IdleTimeoutMilliseconds);
                        Assert.AreEqual(1, readBack.RebroadcastSettings[1].ReceiverId);
                        Assert.AreEqual(10, readBack.RebroadcastSettings[1].StaleSeconds);
                        Assert.AreEqual("Two", readBack.RebroadcastSettings[1].Passphrase);
                        Assert.AreEqual(2000, readBack.RebroadcastSettings[1].SendIntervalMilliseconds);

                        Assert.AreEqual(DefaultAccess.Deny, readBack.RebroadcastSettings[0].Access.DefaultAccess);
                        Assert.AreEqual(2, readBack.RebroadcastSettings[0].Access.Addresses.Count);
                        Assert.AreEqual("1.2.3.4/10", readBack.RebroadcastSettings[0].Access.Addresses[0]);
                        Assert.AreEqual("virtualradarserver.co.uk", readBack.RebroadcastSettings[0].Access.Addresses[1]);

                        Assert.AreEqual(DefaultAccess.Allow, readBack.RebroadcastSettings[1].Access.DefaultAccess);
                        Assert.AreEqual(1, readBack.RebroadcastSettings[1].Access.Addresses.Count);
                        Assert.AreEqual("192.168.0.7", readBack.RebroadcastSettings[1].Access.Addresses[0]);

                        break;
                    case nameof(Configuration.Receivers):
                        Assert.AreEqual(2, readBack.Receivers.Count);

                        var receiver = readBack.Receivers[0];
                        Assert.AreEqual(true, receiver.Enabled);
                        Assert.AreEqual(1, receiver.UniqueId);
                        Assert.AreEqual("First", receiver.Name);
                        Assert.AreEqual(DataSource.Port30003, receiver.DataSource);
                        Assert.AreEqual(ConnectionType.TCP, receiver.ConnectionType);
                        Assert.AreEqual(false, receiver.AutoReconnectAtStartup);
                        Assert.AreEqual(true, receiver.IsPassive);
                        Assert.AreEqual("192.168.0.1", receiver.Address);
                        Assert.AreEqual(30004, receiver.Port);
                        Assert.AreEqual(true, receiver.UseKeepAlive);
                        Assert.AreEqual(30000, receiver.IdleTimeoutMilliseconds);
                        Assert.AreEqual(null, receiver.ComPort);
                        Assert.AreEqual(19200, receiver.BaudRate);
                        Assert.AreEqual(7, receiver.DataBits);
                        Assert.AreEqual(StopBits.One, receiver.StopBits);
                        Assert.AreEqual(Parity.Even, receiver.Parity);
                        Assert.AreEqual(Handshake.XOnXOff, receiver.Handshake);
                        Assert.AreEqual("Start", receiver.StartupText);
                        Assert.AreEqual("", receiver.ShutdownText);
                        Assert.AreEqual(1, receiver.ReceiverLocationId);
                        Assert.AreEqual(null, receiver.Passphrase);
                        Assert.AreEqual(ReceiverUsage.Normal, receiver.ReceiverUsage);
                        Assert.AreEqual(true, receiver.IsSatcomFeed);
                        Assert.AreEqual("http://example.com:1/path/page", receiver.WebAddress);
                        Assert.AreEqual(12345, receiver.FetchIntervalMilliseconds);
                        Assert.AreEqual(Guid.Parse("6363bea8-4077-4131-85bd-39b8a1a211cb"), receiver.Key);

                        receiver = readBack.Receivers[1];
                        Assert.AreEqual(false, receiver.Enabled);
                        Assert.AreEqual(2, receiver.UniqueId);
                        Assert.AreEqual("Second", receiver.Name);
                        Assert.AreEqual(DataSource.Beast, receiver.DataSource);
                        Assert.AreEqual(ConnectionType.COM, receiver.ConnectionType);
                        Assert.AreEqual(true, receiver.AutoReconnectAtStartup);
                        Assert.AreEqual(false, receiver.IsPassive);
                        Assert.AreEqual("127.0.0.1", receiver.Address);
                        Assert.AreEqual(30003, receiver.Port);
                        Assert.AreEqual(false, receiver.UseKeepAlive);
                        Assert.AreEqual(20000, receiver.IdleTimeoutMilliseconds);
                        Assert.AreEqual("COM3", receiver.ComPort);
                        Assert.AreEqual(2400, receiver.BaudRate);
                        Assert.AreEqual(8, receiver.DataBits);
                        Assert.AreEqual(StopBits.OnePointFive, receiver.StopBits);
                        Assert.AreEqual(Parity.Odd, receiver.Parity);
                        Assert.AreEqual(Handshake.RequestToSend, receiver.Handshake);
                        Assert.AreEqual("", receiver.StartupText);
                        Assert.AreEqual("Stop", receiver.ShutdownText);
                        Assert.AreEqual(2, receiver.ReceiverLocationId);
                        Assert.AreEqual("Two", receiver.Passphrase);
                        Assert.AreEqual(ReceiverUsage.MergeOnly, receiver.ReceiverUsage);
                        Assert.AreEqual(false, receiver.IsSatcomFeed);
                        Assert.AreEqual("http://example.com:2/path/page", receiver.WebAddress);
                        Assert.AreEqual(67890, receiver.FetchIntervalMilliseconds);
                        Assert.AreEqual(Guid.Parse("d66cbcca-19e9-4656-875e-a19031ea4cbc"), receiver.Key);

                        Assert.AreEqual(DefaultAccess.Deny, readBack.Receivers[0].Access.DefaultAccess);
                        Assert.AreEqual(2, readBack.Receivers[0].Access.Addresses.Count);
                        Assert.AreEqual("1.2.3.4/10", readBack.Receivers[0].Access.Addresses[0]);
                        Assert.AreEqual("virtualradarserver.co.uk", readBack.Receivers[0].Access.Addresses[1]);

                        Assert.AreEqual(DefaultAccess.Allow, readBack.Receivers[1].Access.DefaultAccess);
                        Assert.AreEqual(1, readBack.Receivers[1].Access.Addresses.Count);
                        Assert.AreEqual("192.168.0.7", readBack.Receivers[1].Access.Addresses[0]);

                        break;
                    case nameof(Configuration.MergedFeeds):
                        Assert.AreEqual(2, readBack.MergedFeeds.Count);

                        var mergedFeed = readBack.MergedFeeds[0];
                        Assert.AreEqual(true, mergedFeed.Enabled);
                        Assert.AreEqual(1, mergedFeed.UniqueId);
                        Assert.AreEqual("First", mergedFeed.Name);
                        Assert.AreEqual(1000, mergedFeed.IcaoTimeout);
                        Assert.AreEqual(false, mergedFeed.IgnoreAircraftWithNoPosition);
                        Assert.AreEqual(ReceiverUsage.Normal, mergedFeed.ReceiverUsage);
                        Assert.AreEqual(2, mergedFeed.ReceiverIds.Count);
                        Assert.AreEqual(1, mergedFeed.ReceiverIds[0]);
                        Assert.AreEqual(2, mergedFeed.ReceiverIds[1]);
                        Assert.AreEqual(2, mergedFeed.ReceiverFlags.Count);
                        Assert.AreEqual(1, mergedFeed.ReceiverFlags[0].UniqueId);
                        Assert.AreEqual(false, mergedFeed.ReceiverFlags[0].IsMlatFeed);
                        Assert.AreEqual(2, mergedFeed.ReceiverFlags[1].UniqueId);
                        Assert.AreEqual(true, mergedFeed.ReceiverFlags[1].IsMlatFeed);
                        Assert.AreEqual(Guid.Parse("b0fb95a0-8321-40f1-bbae-dccac3b072a2"), mergedFeed.Key);

                        mergedFeed = readBack.MergedFeeds[1];
                        Assert.AreEqual(false, mergedFeed.Enabled);
                        Assert.AreEqual(2, mergedFeed.UniqueId);
                        Assert.AreEqual("Second", mergedFeed.Name);
                        Assert.AreEqual(2000, mergedFeed.IcaoTimeout);
                        Assert.AreEqual(true, mergedFeed.IgnoreAircraftWithNoPosition);
                        Assert.AreEqual(ReceiverUsage.HideFromWebSite, mergedFeed.ReceiverUsage);
                        Assert.AreEqual(2, mergedFeed.ReceiverIds.Count);
                        Assert.AreEqual(2, mergedFeed.ReceiverIds[0]);
                        Assert.AreEqual(1, mergedFeed.ReceiverIds[1]);
                        Assert.AreEqual(2, mergedFeed.ReceiverFlags[0].UniqueId);
                        Assert.AreEqual(true,  mergedFeed.ReceiverFlags[0].IsMlatFeed);
                        Assert.AreEqual(1, mergedFeed.ReceiverFlags[1].UniqueId);
                        Assert.AreEqual(false, mergedFeed.ReceiverFlags[1].IsMlatFeed);
                        Assert.AreEqual(Guid.Parse("32cb9a22-14e2-4cd4-b5c5-54ce84a29470"), mergedFeed.Key);
                        break;
                    case nameof(Configuration.DataVersion):
                        Assert.AreEqual(102, readBack.DataVersion);     // Save adds one to the saved DataVersion of 101
                        break;
                    case nameof(Configuration.MonoSettings):
                        var monoSettings = readBack.MonoSettings;
                        Assert.AreEqual(false, monoSettings.UseMarkerLabels);
                        break;
                    case nameof(Configuration.StateHistorySettings):
                        var stateHistorySettings = readBack.StateHistorySettings;
                        Assert.AreEqual(false, stateHistorySettings.Enabled);
                        Assert.AreEqual("Foo", stateHistorySettings.NonStandardFolder);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private Configuration CreateKnownConfiguration()
        {
            var result = new Configuration();
            foreach(PropertyInfo property in result.GetType().GetProperties()) {
                switch(property.Name) {
                    case nameof(Configuration.BaseStationSettings):     CreateKnownBaseStationSettings(result); break;
                    case nameof(Configuration.FlightRouteSettings):     CreateKnownFlightRouteSettings(result); break;
                    case nameof(Configuration.WebServerSettings):       CreateKnownWebServerSettings(result); break;
                    case nameof(Configuration.GoogleMapSettings):       CreateKnownGoogleMapSettings(result); break;
                    case nameof(Configuration.VersionCheckSettings):    CreateKnownVersionCheckSettings(result); break;
                    case nameof(Configuration.InternetClientSettings):  CreateKnownInternetClientSettings(result); break;
                    case nameof(Configuration.AudioSettings):           CreateKnownAudioSettings(result); break;
                    case nameof(Configuration.RawDecodingSettings):     CreateKnownRawDecodingSettings(result); break;
                    case nameof(Configuration.ReceiverLocations):       CreateKnownReceiverLocations(result); break;
                    case nameof(Configuration.RebroadcastSettings):     CreateKnownRebroadcastSettings(result); break;
                    case nameof(Configuration.Receivers):               CreateKnownReceivers(result); break;
                    case nameof(Configuration.MergedFeeds):             CreateKnownMergedFeeds(result); break;
                    case nameof(Configuration.DataVersion):             result.DataVersion = 101; break;
                    case nameof(Configuration.MonoSettings):            CreateKnownMonoSettings(result); break;
                    case nameof(Configuration.StateHistorySettings):    CreateKnownStateHistorySettings(result); break;
                    default:                                            throw new NotImplementedException($"Unknown property {property.Name}");
                }
            }

            return result;
        }

        private static void CreateKnownBaseStationSettings(Configuration configuration)
        {
            configuration.BaseStationSettings = new BaseStationSettings() {
                Address = "Aa",
                Port = 65535,
                DataSource = DataSource.Sbs3,
                DatabaseFileName = "Bb",
                OperatorFlagsFolder = "Cc",
                SilhouettesFolder = "Ee",
                OutlinesFolder = "Ff",
                DisplayTimeoutSeconds = 120,
                PicturesFolder = "Pp",
                IgnoreBadMessages = true,
                ConnectionType = ConnectionType.COM,
                ComPort = "/dev/com4",
                BaudRate = 2400,
                DataBits = 7,
                StopBits = StopBits.OnePointFive,
                Parity = Parity.Space,
                Handshake = Handshake.XOnXOff,
                StartupText = "startup",
                ShutdownText = "shutdown",
                AutoReconnectAtStartup = true,
                TrackingTimeoutSeconds = 3600,
                MinimiseToSystemTray = true,
                SearchPictureSubFolders = true,
                AutoSavePolarPlotsMinutes = 100,
                DownloadGlobalAirPressureReadings = false,
                SatcomDisplayTimeoutMinutes = 300,
                SatcomTrackingTimeoutMinutes = 400,
            };
        }

        private static void CreateKnownFlightRouteSettings(Configuration configuration)
        {
            configuration.FlightRouteSettings = new FlightRouteSettings() {
                AutoUpdateEnabled = true,
            };
        }

        private static void CreateKnownWebServerSettings(Configuration configuration)
        {
            configuration.WebServerSettings = new WebServerSettings() {
                AuthenticationScheme = AuthenticationSchemes.Basic,
                BasicAuthenticationUser = "Tt",
                BasicAuthenticationPasswordHash = new Hash("Funk to funky"),
                EnableUPnp = true,
                UPnpPort = 8181,
                IsOnlyInternetServerOnLan = false,
                AutoStartUPnP = true,
                ConvertedUser = true,
                BasicAuthenticationUserIds = {
                    "First",
                    "Second",
                }
            };
        }

        private static void CreateKnownGoogleMapSettings(Configuration configuration)
        {
            configuration.GoogleMapSettings = new GoogleMapSettings() {
                InitialSettings = "SomeSettings",
                InitialMapLatitude = -12.123456,
                InitialMapLongitude = 120.123987,
                InitialMapType = "HYBRID",
                InitialMapZoom = 7,
                InitialRefreshSeconds = 9,
                MinimumRefreshSeconds = 8,
                ShortTrailLengthSeconds = 400,
                InitialDistanceUnit = DistanceUnit.Kilometres,
                InitialHeightUnit = HeightUnit.Metres,
                InitialSpeedUnit = SpeedUnit.MilesPerHour,
                PreferIataAirportCodes = true,
                EnableBundling = false,
                EnableMinifying = false,
                EnableCompression = false,
                WebSiteReceiverId = 123,
                ClosestAircraftReceiverId = 456,
                FlightSimulatorXReceiverId = 789,
                ProxyType = ProxyType.Reverse,
                DirectoryEntryKey = "ABC123-456",
                EnableCorsSupport = true,
                AllowCorsDomains = "ABC.COM",
                GoogleMapsApiKey = "API Key",
                UseGoogleMapsAPIKeyWithLocalRequests = true,
                UseSvgGraphicsOnDesktop = false,
                UseSvgGraphicsOnMobile = false,
                UseSvgGraphicsOnReports = false,
                MapProvider = MapProvider.Leaflet,
                TileServerSettingName = "My Tile Server",
            };
        }

        private static void CreateKnownVersionCheckSettings(Configuration configuration)
        {
            configuration.VersionCheckSettings = new VersionCheckSettings() {
                CheckAutomatically = false,
                CheckPeriodDays = 12,
            };
        }

        private static void CreateKnownInternetClientSettings(Configuration configuration)
        {
            configuration.InternetClientSettings = new InternetClientSettings() {
                CanRunReports = true,
                CanShowPinText = true,
                CanShowPictures = true,
                TimeoutMinutes = 15,
                CanPlayAudio = true,
                AllowInternetProximityGadgets = true,
                CanSubmitRoutes = true,
                CanShowPolarPlots = true,
            };
        }

        private static void CreateKnownAudioSettings(Configuration configuration)
        {
            configuration.AudioSettings = new AudioSettings() {
                Enabled = false,
                VoiceName = "Male Voice",
                VoiceRate = 2,
            };
        }

        private static void CreateKnownRawDecodingSettings(Configuration configuration)
        {
            configuration.RawDecodingSettings = new RawDecodingSettings() {
                AcceptableAirborneSpeed = 17.25,
                AcceptableAirSurfaceTransitionSpeed = 19.75,
                AcceptableSurfaceSpeed = 21.25,
                AirborneGlobalPositionLimit = 19,
                FastSurfaceGlobalPositionLimit = 17,
                SlowSurfaceGlobalPositionLimit = 12,
                IgnoreCallsignsInBds20 = true,
                IgnoreMilitaryExtendedSquitter = true,
                IgnoreInvalidCodeBlockInParityMessages = true,
                IgnoreInvalidCodeBlockInOtherMessages = false,
                ReceiverLocationId = 7,
                ReceiverRange = 400,
                SuppressReceiverRangeCheck = false,
                UseLocalDecodeForInitialPosition = true,
                AcceptIcaoInNonPICount = 20,
                AcceptIcaoInNonPISeconds = 21,
                AcceptIcaoInPI0Count = 7,
                AcceptIcaoInPI0Seconds = 42,
                SuppressIcao0 = false,
                SuppressTisbDecoding = true,
                AssumeDF18CF1IsIcao = false,
            };
        }

        private static void CreateKnownReceiverLocations(Configuration configuration)
        {
            configuration.ReceiverLocations.Clear();
            configuration.ReceiverLocations.AddRange(new ReceiverLocation[] {
                new ReceiverLocation() {
                    UniqueId = 74,
                    Name = "Home",
                    Latitude = 51.234,
                    Longitude = -0.642,
                },
                new ReceiverLocation() {
                    UniqueId = 90,
                    Name = "Away",
                    Latitude = 17.123,
                    Longitude = 12.456,
                },
            });
        }

        private static void CreateKnownRebroadcastSettings(Configuration configuration)
        {
            configuration.RebroadcastSettings.Clear();
            configuration.RebroadcastSettings.AddRange(new RebroadcastSettings[] {
                new RebroadcastSettings() {
                    UniqueId = 1,
                    Enabled = true,
                    Format = RebroadcastFormat.Passthrough,
                    Name = "Server 1",
                    IsTransmitter = false,
                    TransmitAddress = "unused",
                    Port = 10000,
                    UseKeepAlive = true,
                    IdleTimeoutMilliseconds = 15000,
                    ReceiverId = -1,
                    StaleSeconds = 7,
                    Access = {
                        DefaultAccess = DefaultAccess.Deny,
                        Addresses = {
                            "1.2.3.4/10",
                            "virtualradarserver.co.uk",
                        }
                    },
                    Passphrase = null,
                    SendIntervalMilliseconds = 1000,
                },
                new RebroadcastSettings() {
                    UniqueId = 2,
                    Enabled = false,
                    Format = RebroadcastFormat.Port30003,
                    Name = "Server 2",
                    IsTransmitter = true,
                    TransmitAddress = "remote.address.com",
                    Port = 10001,
                    UseKeepAlive = false,
                    IdleTimeoutMilliseconds = 20000,
                    ReceiverId = 1,
                    StaleSeconds = 10,
                    Access = {
                        DefaultAccess = DefaultAccess.Allow,
                        Addresses = {
                            "192.168.0.7",
                        },
                    },
                    Passphrase = "Two",
                    SendIntervalMilliseconds = 2000,
                },
            });
        }

        private static void CreateKnownReceivers(Configuration configuration)
        {
            configuration.Receivers.Clear();
            configuration.Receivers.AddRange(new Receiver[] {
                new Receiver() {
                    Enabled = true,
                    UniqueId = 1,
                    Name = "First",
                    DataSource = DataSource.Port30003,
                    ConnectionType = ConnectionType.TCP,
                    AutoReconnectAtStartup = false,
                    IsPassive = true,
                    Address = "192.168.0.1",
                    Port = 30004,
                    UseKeepAlive = true,
                    IdleTimeoutMilliseconds = 30000,
                    ComPort = null,
                    BaudRate = 19200,
                    DataBits = 7,
                    StopBits = StopBits.One,
                    Parity = Parity.Even,
                    Handshake = Handshake.XOnXOff,
                    StartupText = "Start",
                    ShutdownText = "",
                    ReceiverLocationId = 1,
                    Access = {
                        DefaultAccess = DefaultAccess.Deny,
                        Addresses = {
                            "1.2.3.4/10",
                            "virtualradarserver.co.uk",
                        }
                    },
                    Passphrase = null,
                    ReceiverUsage = ReceiverUsage.Normal,
                    IsSatcomFeed = true,
                    WebAddress = "http://example.com:1/path/page",
                    FetchIntervalMilliseconds = 12345,
                    Key = Guid.Parse("6363bea8-4077-4131-85bd-39b8a1a211cb"),
                },
                new Receiver() {
                    Enabled = false,
                    UniqueId = 2,
                    Name = "Second",
                    DataSource = DataSource.Beast,
                    ConnectionType = ConnectionType.COM,
                    AutoReconnectAtStartup = true,
                    IsPassive = false,
                    Address = "127.0.0.1",
                    Port = 30003,
                    UseKeepAlive = false,
                    IdleTimeoutMilliseconds = 20000,
                    ComPort = "COM3",
                    BaudRate = 2400,
                    DataBits = 8,
                    StopBits = StopBits.OnePointFive,
                    Parity = Parity.Odd,
                    Handshake = Handshake.RequestToSend,
                    StartupText = "",
                    ShutdownText = "Stop",
                    ReceiverLocationId = 2,
                    Access = {
                        DefaultAccess = DefaultAccess.Allow,
                        Addresses = {
                            "192.168.0.7",
                        },
                    },
                    Passphrase = "Two",
                    ReceiverUsage = ReceiverUsage.MergeOnly,
                    IsSatcomFeed = false,
                    WebAddress = "http://example.com:2/path/page",
                    FetchIntervalMilliseconds = 67890,
                    Key = Guid.Parse("d66cbcca-19e9-4656-875e-a19031ea4cbc"),
                },
            });
        }

        private static void CreateKnownMergedFeeds(Configuration configuration)
        {
            configuration.MergedFeeds.Clear();
            configuration.MergedFeeds.AddRange(new MergedFeed[] {
                new MergedFeed() {
                    Enabled = true,
                    UniqueId = 1,
                    Name = "First",
                    IcaoTimeout = 1000,
                    IgnoreAircraftWithNoPosition = false,
                    ReceiverUsage = ReceiverUsage.Normal,
                    ReceiverIds = { 1, 2 },
                    ReceiverFlags = {
                        new MergedFeedReceiver() { UniqueId = 1, IsMlatFeed = false, },
                        new MergedFeedReceiver() { UniqueId = 2, IsMlatFeed = true, },
                    },
                    Key = Guid.Parse("b0fb95a0-8321-40f1-bbae-dccac3b072a2"),
                },
                new MergedFeed() {
                    Enabled = false,
                    UniqueId = 2,
                    Name = "Second",
                    IcaoTimeout = 2000,
                    IgnoreAircraftWithNoPosition = true,
                    ReceiverUsage = ReceiverUsage.HideFromWebSite,
                    ReceiverIds = { 2, 1 },
                    ReceiverFlags = {
                        new MergedFeedReceiver() { UniqueId = 2, IsMlatFeed = true, },
                        new MergedFeedReceiver() { UniqueId = 1, IsMlatFeed = false, },
                    },
                    Key = Guid.Parse("32cb9a22-14e2-4cd4-b5c5-54ce84a29470"),
                },
            });
        }

        private static void CreateKnownMonoSettings(Configuration configuration)
        {
            configuration.MonoSettings = new MonoSettings() {
                UseMarkerLabels = false,
            };
        }

        private static void CreateKnownStateHistorySettings(Configuration configuration)
        {
            configuration.StateHistorySettings = new StateHistorySettings() {
                Enabled =           false,
                NonStandardFolder = "Foo",
            };
        }
        #endregion

        #region IgnoreBadMessages automated modifications
        [TestMethod]
        public void IgnoreBadMessages_Cannot_Be_Assigned()
        {
            // The connection flag "IgnoreBadMessages" has been retired. If the user has the value set to false in their
            // configuration then Load overrides this and forces it to true. It is no longer presented to the user.
            var configuration = new Configuration();
            configuration.BaseStationSettings.IgnoreBadMessages = false;
            _Implementation.Save(configuration);

            var readBack = _Implementation.Load();
            Assert.IsTrue(readBack.BaseStationSettings.IgnoreBadMessages);
        }
        #endregion

        #region RebroadcastServer automated modifications
        [TestMethod]
        public void RebroadcastServer_UniqueID_Bug_Worked_Around()
        {
            // Between the introduction of rebroadcast servers and version 2 of VRS the rebroadcast server settings did
            // not have unique IDs. We're just checking here that when we load old records that don't have UniqueId filled
            // they'll be automatically filled in by the load code.
            var configuration = new Configuration();
            configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Name = "A record" });
            configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Name = "Another record" });
            configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Name = "Third" });
            _Implementation.Save(configuration);

            var readback = _Implementation.Load();
            Assert.AreEqual(3, readback.RebroadcastSettings.Count);
            foreach(var record in readback.RebroadcastSettings) {
                Assert.AreNotEqual(0, record.UniqueId);
            }
            Assert.AreEqual(3, readback.RebroadcastSettings.Select(r => r.UniqueId).Distinct().Count());
        }
        #endregion

        #region Receiver automated modifications
        [TestMethod]
        public void Receiver_Created_If_None_Exists()
        {
            var readBack = _Implementation.Load();

            Assert.AreEqual(1, readBack.Receivers.Count);
            var receiver = readBack.Receivers[0];
            Assert.AreEqual("Receiver", receiver.Name);
            Assert.AreEqual("127.0.0.1", receiver.Address);
            Assert.AreEqual(true, receiver.AutoReconnectAtStartup);
            Assert.AreEqual(ConnectionType.TCP, receiver.ConnectionType);
            Assert.AreEqual(DataSource.Port30003, receiver.DataSource);
            Assert.AreEqual(true, receiver.Enabled);
            Assert.AreEqual(30003, receiver.Port);
            Assert.AreEqual(1, receiver.UniqueId);
            Assert.AreNotEqual(Guid.Empty, receiver.Key);

            Assert.AreEqual(1, readBack.GoogleMapSettings.ClosestAircraftReceiverId);
            Assert.AreEqual(1, readBack.GoogleMapSettings.FlightSimulatorXReceiverId);
            Assert.AreEqual(1, readBack.GoogleMapSettings.WebSiteReceiverId);
        }

        [TestMethod]
        public void Receiver_Can_Be_Created_From_Version_One_Settings()
        {
            var configuration = new Configuration();
            configuration.BaseStationSettings = new BaseStationSettings() {
                Address = "Aa",
                Port = 65535,
                DataSource = DataSource.Sbs3,
                ConnectionType = ConnectionType.COM,
                ComPort = "/dev/com4",
                BaudRate = 2400,
                DataBits = 7,
                StopBits = StopBits.OnePointFive,
                Parity = Parity.Space,
                Handshake = Handshake.XOnXOff,
                StartupText = "startup",
                ShutdownText = "shutdown",
                AutoReconnectAtStartup = true,
            };
            configuration.RawDecodingSettings = new RawDecodingSettings() {
                ReceiverLocationId = 7,
            };
            configuration.GoogleMapSettings.ClosestAircraftReceiverId = 0;
            configuration.GoogleMapSettings.WebSiteReceiverId = 0;
            configuration.ReceiverLocations.Clear();
            configuration.ReceiverLocations.AddRange(new ReceiverLocation[] {
                new ReceiverLocation() {
                    UniqueId = 7,
                    Name = "Somewhere",
                    Latitude = 1,
                    Longitude = 2,
                }
            });
            configuration.RebroadcastSettings.Clear();
            configuration.RebroadcastSettings.AddRange(new RebroadcastSettings[] {
                new RebroadcastSettings() {
                    Enabled = true,
                    Format = RebroadcastFormat.Avr,
                    Name = "First",
                    Port = 1234,
                    ReceiverId = 0,
                },
                new RebroadcastSettings() {
                    Enabled = false,
                    Format = RebroadcastFormat.Passthrough,
                    Name = "Second",
                    Port = 5678,
                    ReceiverId = 0,
                }
            });
            _Implementation.Save(configuration);

            var readBack = _Implementation.Load();
            Assert.AreEqual(1, readBack.Receivers.Count);

            var receiver = readBack.Receivers[0];
            Assert.AreNotEqual(0, receiver.UniqueId);
            Assert.AreEqual(true, receiver.Enabled);
            Assert.AreEqual("Receiver", receiver.Name);
            Assert.AreEqual("Aa", receiver.Address);
            Assert.AreEqual(65535, receiver.Port);
            Assert.AreEqual(DataSource.Sbs3, receiver.DataSource);
            Assert.AreEqual(ConnectionType.COM, receiver.ConnectionType);
            Assert.AreEqual("/dev/com4", receiver.ComPort);
            Assert.AreEqual(2400, receiver.BaudRate);
            Assert.AreEqual(7, receiver.DataBits);
            Assert.AreEqual(StopBits.OnePointFive, receiver.StopBits);
            Assert.AreEqual(Parity.Space, receiver.Parity);
            Assert.AreEqual(Handshake.XOnXOff, receiver.Handshake);
            Assert.AreEqual("startup", receiver.StartupText);
            Assert.AreEqual("shutdown", receiver.ShutdownText);
            Assert.AreEqual(true, receiver.AutoReconnectAtStartup);
            Assert.AreNotEqual(Guid.Empty, receiver.Key);

            Assert.AreEqual(2, readBack.RebroadcastSettings.Count);
            Assert.AreEqual(receiver.UniqueId, readBack.RebroadcastSettings[0].ReceiverId);
            Assert.AreEqual(receiver.UniqueId, readBack.RebroadcastSettings[1].ReceiverId);

            Assert.AreNotEqual(0, readBack.GoogleMapSettings.ClosestAircraftReceiverId);
            Assert.AreNotEqual(0, readBack.GoogleMapSettings.FlightSimulatorXReceiverId);
            Assert.AreNotEqual(0, readBack.GoogleMapSettings.WebSiteReceiverId);
        }

        [TestMethod]
        public void Receiver_Key_Filled_If_Empty_When_Loaded()
        {
            // State history brought with it the need to give receivers and merged feeds unique IDs
            // that cannot be shared with old receivers that have been deleted. Nowadays I would probably
            // have the presenter that loads and initialises the application look for out-of-date
            // configuration and update it to fit current requirements, but there is already code to do
            // that and it's in the ConfigurationStorage class, so we're sticking with that.

            // Save a configuration with an empty receiver key
            var configuration = new Configuration();
            configuration.Receivers.Add(new Receiver() { Key = Guid.Empty });
            _Implementation.Save(configuration);

            // Read it back - the read should have created a Key GUID and also saved it, so it should be
            // the same in future reads

            var readback = _Implementation.Load();
            var key = readback.Receivers[0].Key;
            Assert.AreNotEqual(Guid.Empty, key);

            var reloadedKey = _Implementation.Load().Receivers[0].Key;
            Assert.AreEqual(key, reloadedKey);
        }
        #endregion

        #region BasicAuthentication automated modifications
        private byte[] SaveBasicAuthenticationConfiguration(Action<Configuration> modifyConfiguration = null)
        {
            var configuration = new Configuration();
            var hashBytes = new byte[] { 0x01, 0x02, 0x03, };
            var hash = new Hash() {
                Version = 999,
            };
            hash.Buffer.AddRange(hashBytes);
            configuration.WebServerSettings.BasicAuthenticationUser = "MyUser";
            configuration.WebServerSettings.BasicAuthenticationPasswordHash = hash;
            if(modifyConfiguration != null) modifyConfiguration(configuration);

            _Implementation.Save(configuration);

            return hashBytes;
        }

        [TestMethod]
        public void User_Can_Be_Created_From_Version_One_Settings()
        {
            var hashBytes = SaveBasicAuthenticationConfiguration();

            IUser createdUser = null;
            Hash createdHash = null;
            _UserManager.Setup(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>())).Callback((IUser user, Hash copyHash) => {
                user.UniqueId = "AssignedID";
                createdUser = user;
                createdHash = copyHash;
            });

            var config = _Implementation.Load();

            var settings = config.WebServerSettings;
            Assert.IsTrue(settings.ConvertedUser);
            Assert.AreEqual(1, settings.BasicAuthenticationUserIds.Count);
            Assert.AreEqual("AssignedID", settings.BasicAuthenticationUserIds[0]);

            Assert.AreEqual("MyUser", createdUser.LoginName);
            Assert.AreEqual("Converted Basic Authentication User", createdUser.Name);
            Assert.AreEqual(true, createdUser.Enabled);
            Assert.AreEqual(999, createdHash.Version);
            Assert.IsTrue(hashBytes.SequenceEqual(createdHash.Buffer));
        }

        [TestMethod]
        public void User_Not_Created_From_Version_One_If_It_Already_Exists()
        {
            SaveBasicAuthenticationConfiguration(r => r.WebServerSettings.ConvertedUser = true);

            var config = _Implementation.Load();

            _UserManager.Verify(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>()), Times.Never());
            Assert.AreEqual(true, config.WebServerSettings.ConvertedUser);
            Assert.AreEqual(0, config.WebServerSettings.BasicAuthenticationUserIds.Count);
        }

        [TestMethod]
        public void User_Not_Created_From_Version_One_If_Name_Is_Null()
        {
            var saveConfig = new Configuration();
            saveConfig.WebServerSettings.BasicAuthenticationUser = null;
            _Implementation.Save(saveConfig);

            var config = _Implementation.Load();

            _UserManager.Verify(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>()), Times.Never());
            Assert.AreEqual(false, config.WebServerSettings.ConvertedUser);
            Assert.AreEqual(0, config.WebServerSettings.BasicAuthenticationUserIds.Count);
        }

        [TestMethod]
        public void User_Not_Created_From_Version_One_If_Name_Is_Empty()
        {
            var saveConfig = new Configuration();
            saveConfig.WebServerSettings.BasicAuthenticationUser = "";
            _Implementation.Save(saveConfig);

            var config = _Implementation.Load();

            _UserManager.Verify(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>()), Times.Never());
            Assert.AreEqual(false, config.WebServerSettings.ConvertedUser);
            Assert.AreEqual(0, config.WebServerSettings.BasicAuthenticationUserIds.Count);
        }

        [TestMethod]
        public void User_Not_Created_From_Version_One_If_UserManager_Does_Not_Support_VRS_Hashes()
        {
            SaveBasicAuthenticationConfiguration();

            _UserManager.Setup(r => r.CanCreateUsersWithHash).Returns(false);
            var config = _Implementation.Load();

            _UserManager.Verify(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>()), Times.Never());
            Assert.AreEqual(false, config.WebServerSettings.ConvertedUser);
            Assert.AreEqual(0, config.WebServerSettings.BasicAuthenticationUserIds.Count);
        }

        [TestMethod]
        public void Can_Convert_Basic_Authentication_User_To_Existing_User()
        {
            var hashBytes = SaveBasicAuthenticationConfiguration();

            _User.Setup(r => r.UniqueId).Returns("ABC123");
            _UserManager.Setup(r => r.GetUserByLoginName("MyUser")).Returns(_User.Object);

            var config = _Implementation.Load();

            var settings = config.WebServerSettings;
            Assert.IsTrue(settings.ConvertedUser);
            Assert.AreEqual(1, settings.BasicAuthenticationUserIds.Count);
            Assert.AreEqual("ABC123", settings.BasicAuthenticationUserIds[0]);

            _UserManager.Verify(r => r.CreateUser(It.IsAny<IUser>()), Times.Never());
            _UserManager.Verify(r => r.CreateUserWithHash(It.IsAny<IUser>(), It.IsAny<Hash>()), Times.Never());
        }

        [TestMethod]
        public void Can_Use_Existing_User_Even_If_UserManager_Disallows_Use_Of_VRS_Hash()
        {
            var hashBytes = SaveBasicAuthenticationConfiguration();

            _User.Setup(r => r.UniqueId).Returns("ABC123");
            _UserManager.Setup(r => r.GetUserByLoginName("MyUser")).Returns(_User.Object);
            _UserManager.Setup(r => r.CanCreateUsersWithHash).Returns(false);

            var config = _Implementation.Load();

            var settings = config.WebServerSettings;
            Assert.IsTrue(settings.ConvertedUser);
            Assert.AreEqual("ABC123", settings.BasicAuthenticationUserIds[0]);
        }

        [TestMethod]
        public void Does_Not_Convert_The_User_If_It_Is_Already_In_The_List()
        {
            var hashBytes = SaveBasicAuthenticationConfiguration(r => r.WebServerSettings.BasicAuthenticationUserIds.Add("ABC123"));

            _User.Setup(r => r.UniqueId).Returns("ABC123");
            _UserManager.Setup(r => r.GetUserByLoginName("MyUser")).Returns(_User.Object);

            var config = _Implementation.Load();

            var settings = config.WebServerSettings;
            Assert.IsTrue(settings.ConvertedUser);
            Assert.AreEqual(1, settings.BasicAuthenticationUserIds.Count);
            Assert.AreEqual("ABC123", settings.BasicAuthenticationUserIds[0]);
        }
        #endregion

        #region MergedFeed automated modifications
        [TestMethod]
        public void MergedFeed_Key_Filled_If_Empty_When_Loaded()
        {
            // See notes against Receiver version of this. Exactly the same reasoning and behaviour.

            var configuration = new Configuration();
            configuration.MergedFeeds.Add(new MergedFeed() { Key = Guid.Empty });
            _Implementation.Save(configuration);

            // Read it back - the read should have created a Key GUID and also saved it, so it should be
            // the same in future reads

            var readback = _Implementation.Load();
            var key = readback.MergedFeeds[0].Key;
            Assert.AreNotEqual(Guid.Empty, key);

            var reloadedKey = _Implementation.Load().MergedFeeds[0].Key;
            Assert.AreEqual(key, reloadedKey);
        }
        #endregion
    }
}
