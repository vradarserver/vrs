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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.FlightSimulatorX;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class FlightSimulatorXPresenterTests
    {
        #region TestContext, fields, TestInitialise
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IFlightSimulatorXPresenter _Presenter;
        private Mock<IFlightSimulatorXPresenterProvider> _Provider;
        private ClockMock _Clock;
        private int _LimitTimedInvokeCallbacks;
        private Mock<IFlightSimulatorXView> _View;
        private Mock<ISimpleAircraftList> _FlightSimulatorAircraftList;
        private Mock<IFlightSimulatorX> _FlightSimulatorX;
        private List<IAircraft> _FSXAircraftList;
        private IAircraft _SelectedAircraft;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<ILog> _Log;
        private Mock<IWebServer> _WebServer;

        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IBaseStationAircraftList>> _BaseStationAircraftLists;
        private List<List<IAircraft>> _RealAircraftLists;
        private Mock<IFeedManager> _FeedManager;
        private Mock<IBaseStationAircraftList> _BaseStationAircraftList;
        private List<IAircraft> _RealAircraftList;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _FlightSimulatorX = TestUtilities.CreateMockImplementation<IFlightSimulatorX>();
            _FlightSimulatorX.Setup(fsx => fsx.IsInstalled).Returns(true);

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _Log = TestUtilities.CreateMockSingleton<ILog>();

            _Presenter = Factory.Resolve<IFlightSimulatorXPresenter>();

            _Provider = new Mock<IFlightSimulatorXPresenterProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _LimitTimedInvokeCallbacks = 1;
            int callbacks = 0;
            _Provider.Setup(p => p.TimedInvokeOnBackgroundThread(It.IsAny<Action>(), It.IsAny<int>())).Callback((Action callback, int milliseconds) => {
                if(callbacks++ < _LimitTimedInvokeCallbacks) callback();
            });
            _Presenter.Provider = _Provider.Object;

            _SelectedAircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _View = new Mock<IFlightSimulatorXView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _View.Setup(v => v.CanBeRefreshed).Returns(true);
            _View.Setup(v => v.SelectedRealAircraft).Returns(_SelectedAircraft);

            _BaseStationAircraftLists = new List<Mock<IBaseStationAircraftList>>();
            _Feeds = new List<Mock<IFeed>>();
            _RealAircraftLists = new List<List<IAircraft>>();
            var useVisibleFeeds = false;
            _FeedManager = FeedHelper.CreateMockFeedManager(_Feeds, _BaseStationAircraftLists, _RealAircraftLists, useVisibleFeeds, 1, 2);

            _Configuration.GoogleMapSettings.FlightSimulatorXReceiverId = 1;
            _BaseStationAircraftList = _BaseStationAircraftLists[0];
            _RealAircraftList = _RealAircraftLists[0];

            _FlightSimulatorAircraftList = new Mock<ISimpleAircraftList>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _FSXAircraftList = new List<IAircraft>();
            var syncLock = new object();
            _FlightSimulatorAircraftList.Setup(f => f.Aircraft).Returns(_FSXAircraftList);
            _FlightSimulatorAircraftList.Setup(f => f.ListSyncLock).Returns(syncLock);

            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Presenter.FlightSimulatorAircraftList = _FlightSimulatorAircraftList.Object;
            _Presenter.WebServer = _WebServer.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void FlightSimulatorXPresenter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Presenter = Factory.Resolve<IFlightSimulatorXPresenter>();

            Assert.IsNotNull(_Presenter.Provider);
            TestUtilities.TestProperty(_Presenter, r => r.Provider, _Presenter.Provider, _Provider.Object);
            TestUtilities.TestProperty(_Presenter, r => r.FlightSimulatorAircraftList, null, _FlightSimulatorAircraftList.Object);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FlightSimulatorXPresenter_Initialise_Throws_If_FlightSimulatorAircraftList_Property_Is_Null()
        {
            _Presenter.FlightSimulatorAircraftList = null;
            _Presenter.Initialise(_View.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FlightSimulatorXPresenter_Initialise_Throws_If_WebServer_Property_Is_Null()
        {
            _Presenter.WebServer = null;
            _Presenter.Initialise(_View.Object);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Sets_View_To_Correct_Initial_State_When_FSX_Is_Installed()
        {
            _WebServer.Setup(s => s.LocalAddress).Returns("local address");

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(true, _View.Object.ConnectToFlightSimulatorXEnabled);
            Assert.AreEqual(Strings.Disconnected, _View.Object.ConnectionStatus);

            Assert.AreEqual(false, _View.Object.RideAircraftEnabled);
            Assert.AreEqual("-", _View.Object.RideStatus);

            Assert.AreEqual(true, _View.Object.UseSlewMode);

            Assert.AreEqual("local address/fsx.html", _View.Object.FlightSimulatorPageAddress);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Sets_View_To_Correct_Initial_State_When_FSX_Is_Not_Installed()
        {
            _FlightSimulatorX.Setup(fsx => fsx.IsInstalled).Returns(false);
            _WebServer.Setup(s => s.LocalAddress).Returns("another local address");

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(false, _View.Object.ConnectToFlightSimulatorXEnabled);
            Assert.AreEqual(Strings.FlightSimulatorXIsNotInstalled, _View.Object.ConnectionStatus);

            Assert.AreEqual(false, _View.Object.RideAircraftEnabled);
            Assert.AreEqual("-", _View.Object.RideStatus);

            Assert.AreEqual("another local address/fsx.html", _View.Object.FlightSimulatorPageAddress);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Copies_Snapshot_Of_Aircraft_List_To_View()
        {
            var aircraft1 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var aircraft2 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _RealAircraftList.Add(aircraft1);
            _RealAircraftList.Add(aircraft2);

            _View.Setup(v => v.InitialiseRealAircraftListDisplay(It.IsAny<IList<IAircraft>>())).Callback((IList<IAircraft> aircraftList) => {
                Assert.AreEqual(2, aircraftList.Count);
                Assert.IsTrue(aircraftList.Contains(aircraft1));
                Assert.IsTrue(aircraftList.Contains(aircraft2));
            });

            _Presenter.Initialise(_View.Object);

            _View.Verify(v => v.InitialiseRealAircraftListDisplay(It.IsAny<IList<IAircraft>>()), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Copies_Correct_Aircraft_List_To_View()
        {
            var aircraft1 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var aircraft2 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _RealAircraftLists[1].Add(aircraft1);
            _RealAircraftLists[1].Add(aircraft2);
            _Configuration.GoogleMapSettings.FlightSimulatorXReceiverId = 2;

            _View.Setup(v => v.InitialiseRealAircraftListDisplay(It.IsAny<IList<IAircraft>>())).Callback((IList<IAircraft> aircraftList) => {
                Assert.AreEqual(2, aircraftList.Count);
                Assert.IsTrue(aircraftList.Contains(aircraft1));
                Assert.IsTrue(aircraftList.Contains(aircraft2));
            });

            _Presenter.Initialise(_View.Object);

            _View.Verify(v => v.InitialiseRealAircraftListDisplay(It.IsAny<IList<IAircraft>>()), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Periodically_Updates_List_Of_Real_Aircraft()
        {
            var aircraft1 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var aircraft2 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _RealAircraftList.Add(aircraft1);
            _RealAircraftList.Add(aircraft2);

            _View.Setup(v => v.UpdateRealAircraftListDisplay(It.IsAny<IList<IAircraft>>())).Callback((IList<IAircraft> aircraftList) => {
                Assert.AreEqual(2, aircraftList.Count);
                Assert.IsTrue(aircraftList.Contains(aircraft1));
                Assert.IsTrue(aircraftList.Contains(aircraft2));
            });

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.TimedInvokeOnBackgroundThread(It.IsAny<Action>(), 1000), Times.Exactly(2));
            _View.Verify(v => v.UpdateRealAircraftListDisplay(It.IsAny<IList<IAircraft>>()), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Initialise_Stops_Periodic_Update_Once_View_Is_Defunct()
        {
            _View.Setup(v => v.UpdateRealAircraftListDisplay(It.IsAny<IList<IAircraft>>())).Callback(() => {
                _View.Setup(v => v.CanBeRefreshed).Returns(false);
            });
            _LimitTimedInvokeCallbacks = 10;

            _Presenter.Initialise(_View.Object);

            _Provider.Verify(p => p.TimedInvokeOnBackgroundThread(It.IsAny<Action>(), 1000), Times.Exactly(2));
            _View.Verify(v => v.UpdateRealAircraftListDisplay(It.IsAny<IList<IAircraft>>()), Times.Once());
        }
        #endregion

        #region Connect to FSX
        [TestMethod]
        public void FlightSimulatorXPresenter_Connects_To_FSX_When_Connect_Button_Pressed()
        {
            IntPtr handle = new IntPtr(12345);
            _Presenter.Initialise(_View.Object);
            _View.Setup(v => v.WindowHandle).Returns(handle);

            _View.Raise(v => v.ConnectClicked += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(fsx => fsx.Connect(It.IsAny<IntPtr>()), Times.Once());
            _FlightSimulatorX.Verify(fsx => fsx.Connect(handle), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Disables_Connect_Button_After_Connect_Button_Pressed()
        {
            _Presenter.Initialise(_View.Object);

            _View.Raise(v => v.ConnectClicked += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.ConnectToFlightSimulatorXEnabled);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Reflects_Changes_In_Connection_Status_To_View()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(fsx => fsx.ConnectionStatus).Returns("The status");
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);

            Assert.AreEqual("The status", _View.Object.ConnectionStatus);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Enables_Ride_Aircraft_Controls_When_Connection_Established()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(true);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _View.Object.RideAircraftEnabled);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Disables_Connect_Controls_When_Connection_Established()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.ConnectToFlightSimulatorXEnabled = true;

            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(true);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.ConnectToFlightSimulatorXEnabled);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Disables_Ride_Aircraft_Controls_When_Connection_Broken()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(true);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);
            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(false);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.RideAircraftEnabled);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Enabled_Connect_Controls_When_Connection_Broken()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(true);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);
            _FlightSimulatorX.Setup(fsx => fsx.Connected).Returns(false);
            _FlightSimulatorX.Raise(fsx => fsx.ConnectionStatusChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _View.Object.ConnectToFlightSimulatorXEnabled);
        }
        #endregion

        #region Disconnect from FSX
        [TestMethod]
        public void FlightSimulatorXPresenter_Disconnects_From_FSX_When_View_Closes()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _View.Raise(v => v.CloseClicked += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(f => f.Disconnect(), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Does_Not_Disconnect_From_FSX_When_View_Closes_If_Not_Connected()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(false);
            _View.Raise(v => v.CloseClicked += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(f => f.Disconnect(), Times.Never());
        }
        #endregion

        #region IsSimConnectMessage
        [TestMethod]
        public void FlightSimulatorXPresenter_IsSimConnectMessage_Passes_Through_To_IFlightSimulatorX_Version()
        {
            Message message = new Message();
            _Presenter.IsSimConnectMessage(message);

            _FlightSimulatorX.Verify(fsx => fsx.IsSimConnectMessage(It.IsAny<Message>()), Times.Once());
            _FlightSimulatorX.Verify(fsx => fsx.IsSimConnectMessage(message), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_IsSimConnectMessage_Returns_Value_From_IFlightSimulatorX_Version()
        {
            foreach(var value in new bool[] { true, false }) {
                _FlightSimulatorX.Setup(fsx => fsx.IsSimConnectMessage(It.IsAny<Message>())).Returns(value);
                Assert.AreEqual(value, _Presenter.IsSimConnectMessage(new Message()), value.ToString());
            }
        }
        #endregion

        #region FSX exceptions
        [TestMethod]
        public void FlightSimulatorXPresenter_Rethrows_FSX_Exceptions()
        {
            _Presenter.Initialise(_View.Object);

            var exception = new FlightSimulatorXException();
            bool seenException = false;
            try {
                _FlightSimulatorX.Raise(f => f.FlightSimulatorXExceptionRaised += null, new EventArgs<FlightSimulatorXException>(exception));
            } catch(Exception ex) {
                seenException = ex == exception;
            }

            Assert.IsTrue(seenException);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Limits_Number_Of_Rethrows_Of_FSX_Exceptions()
        {
            // If absolutely every FSX exception creates an exception dialog in VRS it spams up the UI with exception windows, so the program
            // limits them to one every 20 seconds. Intervening exceptions are just logged.
            _Presenter.Initialise(_View.Object);

            try {
                _FlightSimulatorX.Raise(f => f.FlightSimulatorXExceptionRaised += null, new EventArgs<FlightSimulatorXException>(new FlightSimulatorXException()));
            } catch {
            }

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(19);

            bool seenException = false;
            try {
                _FlightSimulatorX.Raise(f => f.FlightSimulatorXExceptionRaised += null, new EventArgs<FlightSimulatorXException>(new FlightSimulatorXException()));
            } catch {
                seenException = true;
            }

            Assert.IsFalse(seenException);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Once());

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(2);

            try {
                _FlightSimulatorX.Raise(f => f.FlightSimulatorXExceptionRaised += null, new EventArgs<FlightSimulatorXException>(new FlightSimulatorXException()));
            } catch {
                seenException = true;
            }

            Assert.IsTrue(seenException);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
        #endregion

        #region FlightSimulatorXAircraftList handling
        [TestMethod]
        public void FlightSimulatorXPresenter_Requests_Fresh_Aircraft_Information_When_Timer_Ticks_On_View()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(f => f.RequestAircraftInformation(), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Does_Not_Request_Fresh_Aircraft_Information_When_Disconnected()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(false);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(f => f.RequestAircraftInformation(), Times.Never());
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ReadFSXAircraft$")]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Is_Copied_Into_FlightSimulatorAircraftList()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);

            _Clock.UtcNowValue = worksheet.DateTime("UtcNow");

            var aircraftInformation = new ReadAircraftInformation() {
                AirspeedIndicated = worksheet.Double("FSXAirspeedIndicated"),
                Altitude = worksheet.Double("FSXAltitude"),
                Latitude = worksheet.Double("FSXLatitude"),
                Longitude = worksheet.Double("FSXLongitude"),
                MaxAirspeedIndicated = worksheet.Double("FSXMaxAirspeedIndicated"),
                Model = worksheet.String("FSXModel"),
                OnGround = worksheet.Bool("FSXOnGround"),
                Operator = worksheet.String("FSXOperator"),
                Registration = worksheet.String("FSXRegistration"),
                Squawk = worksheet.Int("FSXSquawk"),
                TrueHeading = worksheet.Double("FSXTrueHeading"),
                Type = worksheet.String("FSXType"),
                VerticalSpeed = worksheet.Double("FSXVerticalSpeed"),
            };
            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(aircraftInformation));

            Assert.AreEqual(1, _FSXAircraftList.Count);
            var aircraft = _FSXAircraftList[0];
            Assert.AreEqual(worksheet.String("Icao24"), aircraft.Icao24);
            Assert.AreEqual(worksheet.Int("UniqueId"), aircraft.UniqueId);
            Assert.AreEqual(worksheet.NFloat("Latitude"), aircraft.Latitude);
            Assert.AreEqual(worksheet.NFloat("Longitude"), aircraft.Longitude);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, aircraft.DataVersion);
            Assert.AreEqual(worksheet.NFloat("GroundSpeed"), aircraft.GroundSpeed);
            Assert.AreEqual(worksheet.NFloat("Track"), aircraft.Track);
            Assert.AreEqual(worksheet.String("Type"), aircraft.Type);
            Assert.AreEqual(worksheet.String("Model"), aircraft.Model);
            Assert.AreEqual(worksheet.String("Operator"), aircraft.Operator);
            Assert.AreEqual(worksheet.String("Registration"), aircraft.Registration);
            Assert.AreEqual(worksheet.NInt("Squawk"), aircraft.Squawk);
            Assert.AreEqual(worksheet.NInt("Altitude"), aircraft.Altitude);
            Assert.AreEqual(worksheet.NInt("VerticalRate"), aircraft.VerticalRate);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Updates_Aircraft_List_On_Second_And_Subsequent_Events()
        {
            _Presenter.Initialise(_View.Object);

            _Clock.UtcNowValue = new DateTime(1998, 7, 6, 5, 4, 3, 2);

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Registration = "D-ABCD" }));
            Assert.AreEqual(1, _FSXAircraftList.Count);
            Assert.AreEqual("D-ABCD", _FSXAircraftList[0].Registration);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].DataVersion);

            _Clock.UtcNowValue = new DateTime(1998, 7, 6, 5, 4, 3, 2).AddMinutes(1);

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Registration = "G-ABCD" }));
            Assert.AreEqual(1, _FSXAircraftList.Count);
            Assert.AreEqual("G-ABCD", _FSXAircraftList[0].Registration);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].DataVersion);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Updates_Coordinates_Of_Aircraft()
        {
            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 17;
            _Presenter.Initialise(_View.Object);

            _Clock.UtcNowValue = new DateTime(1998, 7, 6, 5, 4, 3, 2);

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();
            aircraft.Setup(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>())).Callback(() => {
                Assert.AreEqual(1f, aircraft.Object.Latitude);
                Assert.AreEqual(2f, aircraft.Object.Longitude);
                Assert.AreEqual(3, aircraft.Object.Track);
            });

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() {
                Latitude = 1,
                Longitude = 2,
                TrueHeading = 3,
            }));

            aircraft.Verify(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
            aircraft.Verify(a => a.UpdateCoordinates(_Clock.UtcNowValue, 17), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Picks_Up_Configuration_Changes_To_Short_Coordinates_Length()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 17;

            _Clock.UtcNowValue = new DateTime(1998, 7, 6, 5, 4, 3, 2);

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 42;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Latitude = 1, Longitude = 2, TrueHeading = 3, }));

            aircraft.Verify(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
            aircraft.Verify(a => a.UpdateCoordinates(_Clock.UtcNowValue, 42), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Locks_Aircraft_List_Before_Modification()
        {
            // Hard to test this one really - we can replace the lock object with a null, that would cause it to throw an exception
            _FlightSimulatorAircraftList.Setup(f => f.ListSyncLock).Returns((object)null);

            _Presenter.Initialise(_View.Object);

            bool seenException = false;
            try {
                _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Registration = "D-ABCD" }));
            } catch(ArgumentNullException) {
                seenException = true;
            }
            Assert.IsTrue(seenException);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_Requested_Aircraft_Information_Setting_DataVersion()
        {
            _Presenter.Initialise(_View.Object);

            _Clock.UtcNowValue = new DateTime(1998, 7, 6, 5, 4, 3, 2);

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Registration = "D-ABCD" }));
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].DataVersion);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].RegistrationChanged);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMinutes(1);

            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { Registration = "G-ABCD" }));
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].DataVersion);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, _FSXAircraftList[0].RegistrationChanged);
        }
        #endregion

        #region RideAircraftClicked
        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Sets_Ride_Status_If_No_Aircraft_Is_Selected()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Setup(v => v.SelectedRealAircraft).Returns((IAircraft)null);
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            Assert.AreEqual(Strings.SelectAnAircraftToRide, _View.Object.RideStatus);
            Assert.IsFalse(_FlightSimulatorX.Object.IsSlewing);
            Assert.IsFalse(_FlightSimulatorX.Object.IsFrozen);
            Assert.IsTrue(_View.Object.RideAircraftEnabled);
            Assert.IsFalse(_View.Object.RidingAircraft);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Sets_Slew_Mode_If_UseSlew_Is_Set()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Object.UseSlewMode = true;
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            Assert.IsTrue(_FlightSimulatorX.Object.IsSlewing);
            Assert.IsFalse(_FlightSimulatorX.Object.IsFrozen);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Sets_Freexe_Mode_If_UseSlew_Is_Clear()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Object.UseSlewMode = false;
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            Assert.IsFalse(_FlightSimulatorX.Object.IsSlewing);
            Assert.IsTrue(_FlightSimulatorX.Object.IsFrozen);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Updates_View_When_Ride_Enabled()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _SelectedAircraft.Registration = "REG";
            _SelectedAircraft.Icao24 = "ICAO";
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            Assert.IsTrue(_View.Object.RideAircraftEnabled);
            Assert.IsTrue(_View.Object.RidingAircraft);
            Assert.AreEqual(String.Format(Strings.RidingAircraft, "REG"), _View.Object.RideStatus);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Uses_Icao24_If_Registration_Not_Known()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _SelectedAircraft.Icao24 = "ICAO";
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            Assert.AreEqual(String.Format(Strings.RidingAircraft, "ICAO"), _View.Object.RideStatus);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Releases_Slew_Mode_Before_Starting_Slew_Ride()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.UseSlewMode = true;
            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Releases_Slew_Mode_Before_Starting_Freeze_Ride()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.UseSlewMode = false;
            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Exactly(2)); // See further test on "enters and leaves slew mode before starting freeze mode"
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());      // Ditto
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Releases_Freeze_Mode_Before_Starting_Slew_Ride()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.UseSlewMode = true;
            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Releases_Freeze_Mode_Before_Starting_Freeze_Ride()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.UseSlewMode = false;
            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RideAircraftClicked_Starts_And_Stops_Slew_Mode_Before_Entering_Freeze_Mode()
        {
            // Slew mode is blipped before entering freeze mode - see notes in presenter

            _Presenter.Initialise(_View.Object);

            _View.Object.UseSlewMode = false;
            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Exactly(2));
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Once());
        }
        #endregion

        #region Riding Aircraft
        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Does_Nothing_If_No_Longer_Connected_To_FSX()
        {
            _Presenter.Initialise(_View.Object);

            _SelectedAircraft.UniqueId = 17;
            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(false);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Copes_If_Ridden_Aircraft_Is_No_Longer_Being_Picked_Up_On_Radio()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _SelectedAircraft.UniqueId = 17;

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Copes_If_There_Is_No_Longer_Any_Selected_Aircraft()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);
            _SelectedAircraft.UniqueId = 17;

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _View.Setup(v => v.SelectedRealAircraft).Returns((IAircraft)null);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Never());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Does_Nothing_If_User_Has_Not_Elected_To_Ride_An_Aircraft()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);
            _SelectedAircraft.UniqueId = 17;

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Never());
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "WriteFSXAircraft$")]
        public void FlightSimulatorXPresenter_RidingAircraft_Sends_Real_World_Aircraft_Information_To_FSX_On_View_Timer_Tick()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);

            _Clock.UtcNowValue = worksheet.DateTime("UtcNow");

            _SelectedAircraft.UniqueId = 92;

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _RealAircraftList.Add(aircraft.Object);
            aircraft.Object.UniqueId = 92;
            aircraft.Object.Latitude = worksheet.NFloat("Latitude");
            aircraft.Object.Longitude = worksheet.NFloat("Longitude");
            aircraft.Object.PositionTime = worksheet.NDateTime("PositionTime");
            aircraft.Object.GroundSpeed = worksheet.NFloat("GroundSpeed");
            aircraft.Object.Track = worksheet.NFloat("Track");
            aircraft.Object.Type = worksheet.String("Type");
            aircraft.Object.Model = worksheet.String("Model");
            aircraft.Object.Operator = worksheet.String("Operator");
            aircraft.Object.Registration = worksheet.String("Registration");
            aircraft.Object.Squawk = worksheet.NInt("Squawk");
            aircraft.Object.Altitude = worksheet.NInt("Altitude");
            aircraft.Object.VerticalRate = worksheet.NInt("VerticalRate");

            WriteAircraftInformation aircraftInformation = new WriteAircraftInformation();
            _FlightSimulatorX.Setup(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>())).Callback((WriteAircraftInformation writeAircraftInformation) => {
                aircraftInformation = writeAircraftInformation;
            });

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Once());

            Assert.AreEqual(worksheet.Double("FSXAirspeedIndicated"), aircraftInformation.AirspeedIndicated);
            Assert.AreEqual(worksheet.Double("FSXAltitude"), aircraftInformation.Altitude);
            Assert.AreEqual(worksheet.Double("FSXLatitude"), aircraftInformation.Latitude, 0.001);
            Assert.AreEqual(worksheet.Double("FSXLongitude"), aircraftInformation.Longitude, 0.001);
            Assert.AreEqual(worksheet.String("FSXOperator"), aircraftInformation.Operator);
            Assert.AreEqual(worksheet.String("FSXRegistration"), aircraftInformation.Registration);
            Assert.AreEqual(worksheet.Double("FSXTrueHeading"), aircraftInformation.TrueHeading, 0.001);
            Assert.AreEqual(worksheet.Double("FSXVerticalSpeed"), aircraftInformation.VerticalSpeed);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RideAircraftBank$")]
        public void FlightSimulatorXPresenter_RidingAircraft_Approximates_Bank_Angle_From_Track()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);

            WriteAircraftInformation aircraftInformation = new WriteAircraftInformation();
            _FlightSimulatorX.Setup(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>())).Callback((WriteAircraftInformation writeAircraftInformation) => {
                aircraftInformation = writeAircraftInformation;
            });

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            _SelectedAircraft.UniqueId = 1;

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);

            for(var i = 1;i <= 3;++i) {
                string uniqueIdColumn = String.Format("UniqueId{0}", i);
                string updateTimeColumn = String.Format("UpdateTime{0}", i);
                string trackColumn = String.Format("Track{0}", i);
                string bankColumn = String.Format("Bank{0}", i);

                if(worksheet.String(uniqueIdColumn) == null) break;

                var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
                if(_RealAircraftList.Count == 0) _RealAircraftList.Add(aircraft);
                else                             _RealAircraftList[0] = aircraft;

                _SelectedAircraft.UniqueId = aircraft.UniqueId = worksheet.Int(uniqueIdColumn);
                aircraft.LastUpdate = worksheet.DateTime(updateTimeColumn);
                aircraft.Track = worksheet.NFloat(trackColumn);
                _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
                Assert.AreEqual(worksheet.Double(bankColumn), aircraftInformation.Bank, i.ToString());
            }
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Airspeed_Is_Kept_Below_FSX_Aircraft_Maximum_Airspeed()
        {
            // The FSX aircraft need not be the same type etc. as the real-life one. If we exceed the FSX airspeed then it will simulate a crash in FSX, so we
            // need to pick up the maximum airspeed when reading aircraft information from FSX and use that to moderate the airspeed before writing it back to
            // FSX.

            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            aircraft.Object.UniqueId = 17;
            aircraft.Object.GroundSpeed = 320;
            _RealAircraftList.Add(aircraft.Object);

            _SelectedAircraft.UniqueId = 17;

            WriteAircraftInformation aircraftInformation = new WriteAircraftInformation();
            _FlightSimulatorX.Setup(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>())).Callback((WriteAircraftInformation writeAircraftInformation) => {
                aircraftInformation = writeAircraftInformation;
            });

            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _FlightSimulatorX.Raise(f => f.AircraftInformationReceived += null, new EventArgs<ReadAircraftInformation>(new ReadAircraftInformation() { MaxAirspeedIndicated = 200 }));

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            Assert.AreEqual(170, aircraftInformation.AirspeedIndicated);
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Stops_Ride_When_Stop_Button_Clicked()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _SelectedAircraft.UniqueId = aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);

            _View.Object.UseSlewMode = true;
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Once());

            _View.Raise(v => v.StopRidingAircraftClicked += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.RidingAircraft);
            Assert.AreEqual("-", _View.Object.RideStatus);
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Exactly(2));
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Exactly(2));
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Stops_Ride_When_User_Manually_Toggles_Slew_Mode()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _SelectedAircraft.UniqueId = aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);

            _View.Object.UseSlewMode = true;
            _View.Raise(v => v.RideAircraftClicked += null, EventArgs.Empty);
            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);

            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Once());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Once());

            _FlightSimulatorX.Raise(f => f.SlewToggled += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.RidingAircraft);
            Assert.AreEqual("-", _View.Object.RideStatus);
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Exactly(2));
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Exactly(2));
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Once());

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Once());
        }

        [TestMethod]
        public void FlightSimulatorXPresenter_RidingAircraft_Does_Not_Interfere_If_User_Toggles_Slew_While_Not_Riding_Aircraft()
        {
            _Presenter.Initialise(_View.Object);

            _FlightSimulatorX.Setup(f => f.Connected).Returns(true);
            _FlightSimulatorX.Raise(f => f.ConnectionStatusChanged += null, EventArgs.Empty);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _SelectedAircraft.UniqueId = aircraft.UniqueId = 17;
            _RealAircraftList.Add(aircraft);

            _View.Object.RidingAircraft = false;

            _FlightSimulatorX.Raise(f => f.SlewToggled += null, EventArgs.Empty);

            Assert.AreEqual(false, _View.Object.RidingAircraft);
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = false, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = false, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsFrozen = true, Times.Never());
            _FlightSimulatorX.VerifySet(f => f.IsSlewing = true, Times.Never());

            _View.Raise(v => v.RefreshFlightSimulatorXInformation += null, EventArgs.Empty);
            _FlightSimulatorX.Verify(f => f.MoveAircraft(It.IsAny<WriteAircraftInformation>()), Times.Never());
        }
        #endregion
    }
}
