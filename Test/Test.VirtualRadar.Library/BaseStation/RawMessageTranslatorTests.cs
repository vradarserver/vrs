// Copyright © 2012 onwards, Andrew Whewell
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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.ModeS;
using System.Globalization;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.BaseStation
{
    [TestClass]
    public class RawMessageTranslatorTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;
        private IRawMessageTranslator _Translator;
        private ClockMock _Clock;
        private ModeSMessage _ModeSMessage;
        private AdsbMessage _AdsbMessage;
        private Random _Random = new Random();
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<IStandingDataManager> _StandingDataManager;
        private EventRecorder<EventArgs<string>> _PositionResetEvent;
        private DateTime _NowUtc;
        private Mock<IStatistics> _Statistics;
        private List<int> _RandomIcaos;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Clock = new ClockMock() { UtcNowValue = new DateTime(1999, 12, 31) };
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataManager.Setup(r => r.FindCodeBlock(It.IsAny<string>())).Returns(new CodeBlock() { Country = "X" });
            _StandingDataManager.Setup(r => r.CodeBlocksLoaded).Returns(true);
            _Statistics = StatisticsHelper.CreateLockableStatistics();

            _Translator = Factory.Singleton.Resolve<IRawMessageTranslator>();
            _Translator.Statistics = _Statistics.Object;

            _PositionResetEvent = new EventRecorder<EventArgs<string>>();
            _Translator.PositionReset += _PositionResetEvent.Handler;

            _NowUtc = new DateTime(2012, 1, 2, 3, 4, 5, 6);
            _ModeSMessage = new ModeSMessage() { DownlinkFormat = DownlinkFormat.AllCallReply, Icao24 = 0x112233, ParityInterrogatorIdentifier = 0 };
            _AdsbMessage = new AdsbMessage(_ModeSMessage);

            _RandomIcaos = new List<int>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _Translator.Dispose();
            Factory.RestoreSnapshot(_OriginalClassFactory);
        }
        #endregion

        #region Utility functions
        /// <summary>
        /// Returns a random ICAO24 that is guaranteed not to already have been used in the tests.
        /// </summary>
        /// <returns></returns>
        private int CreateRandomIcao24()
        {
            var result = 0;
            do {
                result = _Random.Next() & 0x00FFFFFF;
            } while(result == 0 || _RandomIcaos.Contains(result));
            _RandomIcaos.Add(result);

            return result;
        }

        /// <summary>
        /// Returns the ICAO24 code formatted in the same fashion as expected on the BaseStation message.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        private string FormatIcao24(int icao24)
        {
            return String.Format("{0:X6}", icao24);
        }

        /// <summary>
        /// Returns a set of Mode-S messages with downlink formats (and any other basic fields) set for all downlink
        /// formats that are known to have the parity bits overlaid on a value of zero. The AP field will always have
        /// a valid ICAO24 that we know is correct.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ModeSMessage> CreateModeSMessagesWithPIField()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.AllCallReply, ParityInterrogatorIdentifier = 0, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.AdsbDeviceTransmittingIcao24, ParityInterrogatorIdentifier = 0, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.AdsbRebroadcastOfExtendedSquitter, ParityInterrogatorIdentifier = 0, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter, ApplicationField = ApplicationField.ADSB, ParityInterrogatorIdentifier = 0, },
            };
        }

        /// <summary>
        /// Returns a set of Mode-S messages with downlink formats (and any other basic fields) set for all downlink
        /// formats that are known to transmit the ICAO24 code overlaid with parity bits. As we don't know the original
        /// ICAO24 code we cannot be sure that the content, once the parity has been stripped off, is correct.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ModeSMessage> CreateModeSMessagesWithNoPIField()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceIdentityReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.LongAirToAirSurveillance, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBIdentityReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommD, },
            };
        }

        /// <summary>
        /// Returns a set of ADS-B messages attached to Mode-S messages set for various extended squitters.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<AdsbMessage> CreateAdsbMessagesForExtendedSquitters()
        {
            return new AdsbMessage[] {
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0 }),
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.AdsbDeviceTransmittingIcao24, ParityInterrogatorIdentifier = 0 }),
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.AdsbRebroadcastOfExtendedSquitter, ParityInterrogatorIdentifier = 0 }),
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.FineFormatTisb, ParityInterrogatorIdentifier = 0 }) { TisbIcaoModeAFlag = 0 },
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.CoarseFormatTisb, ParityInterrogatorIdentifier = 0 }) { TisbIcaoModeAFlag = 0 },
                new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter, ApplicationField = ApplicationField.ADSB, ParityInterrogatorIdentifier = 0 }),
            };
        }

        /// <summary>
        /// Returns an ADS-B message that represents the extended squitter for fine-format TIS-B.
        /// </summary>
        /// <returns></returns>
        private AdsbMessage CreateAdsbMessageForFindFormatTisbExtendedSquitter()
        {
            return new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.FineFormatTisb, ParityInterrogatorIdentifier = 0 }) { TisbIcaoModeAFlag = 0 };
        }

        /// <summary>
        /// Returns a Coarse TIS-B Airborne Position ADS-B message.
        /// </summary>
        /// <returns></returns>
        private AdsbMessage CreateAdsbMessageForCoarseTisbAirbornePosition()
        {
            return new AdsbMessage(new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder, ControlField = ControlField.CoarseFormatTisb, ParityInterrogatorIdentifier = 0 }) { TisbIcaoModeAFlag = 0, CoarseTisbAirbornePosition = new CoarseTisbAirbornePosition() };
        }

        /// <summary>
        /// Returns a set of Mode-S messages for downlink formats that are not yet in use (as at the time of writing)
        /// or are reserved for military use.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ModeSMessage> CreateModeSMessagesForReservedDownlinkFormats()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF1, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF2, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF3, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF6, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF7, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF8, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF9, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF10, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF12, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF13, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF14, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF15, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF22, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.DF23, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithAltitudeCodes()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.LongAirToAirSurveillance, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithSquawks()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceIdentityReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBIdentityReply, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithPossibleCallsigns()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBIdentityReply, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithFlightStatus()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.SurveillanceIdentityReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBAltitudeReply, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.CommBIdentityReply, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithCapability()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.AllCallReply, ParityInterrogatorIdentifier = 0, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0, },
            };
        }

        private IEnumerable<ModeSMessage> CreateModeSMessagesWithVerticalStatus()
        {
            return new ModeSMessage[] {
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, },
                new ModeSMessage() { DownlinkFormat = DownlinkFormat.LongAirToAirSurveillance, },
            };
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void RawMessageTranslator_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Translator.Dispose();
            _Translator = Factory.Singleton.Resolve<IRawMessageTranslator>();

            TestUtilities.TestProperty(_Translator, r => r.AcceptIcaoInNonPICount, 0, 100);
            TestUtilities.TestProperty(_Translator, r => r.AcceptIcaoInNonPIMilliseconds, 0, 100);
            TestUtilities.TestProperty(_Translator, r => r.AcceptIcaoInPI0Count, 1, 100);
            TestUtilities.TestProperty(_Translator, r => r.AcceptIcaoInPI0Milliseconds, 1000, 42000);
            TestUtilities.TestProperty(_Translator, r => r.GlobalDecodeAirborneThresholdMilliseconds, 10000, 15000);
            TestUtilities.TestProperty(_Translator, r => r.GlobalDecodeFastSurfaceThresholdMilliseconds, 25000, 29000);
            TestUtilities.TestProperty(_Translator, r => r.GlobalDecodeSlowSurfaceThresholdMilliseconds, 50000, 70000);
            TestUtilities.TestProperty(_Translator, r => r.IgnoreMilitaryExtendedSquitter, false);
            TestUtilities.TestProperty(_Translator, r => r.LocalDecodeMaxSpeedAirborne, 15.0, 18.9);
            TestUtilities.TestProperty(_Translator, r => r.LocalDecodeMaxSpeedTransition, 5.0, 1.4);
            TestUtilities.TestProperty(_Translator, r => r.LocalDecodeMaxSpeedSurface, 3.0, 2.7);
            TestUtilities.TestProperty(_Translator, r => r.ReceiverLocation, null, new GlobalCoordinate());
            TestUtilities.TestProperty(_Translator, r => r.ReceiverRangeKilometres, 650, 1000);
            TestUtilities.TestProperty(_Translator, r => r.SuppressReceiverRangeCheck, false);
            TestUtilities.TestProperty(_Translator, r => r.TrackingTimeoutSeconds, 600, 3600);
            TestUtilities.TestProperty(_Translator, r => r.IgnoreInvalidCodeBlockInOtherMessages, true);
            TestUtilities.TestProperty(_Translator, r => r.IgnoreInvalidCodeBlockInParityMessages, false);
        }
        #endregion

        #region PositionReset event
        [TestMethod]
        public void RawMessageTranslator_PositionReset_Raised_When_Initial_Position_Subsequently_Proved_Wrong()
        {
            // The initial global decode of the odd/even pair only works when the two positions are within a certain
            // range of each other (~5nmi for airborne messages, 1.25nmi for surface). If they are further apart than
            // this then the initial position will decode to a value that is a couple of degrees latitude or longitude
            // away from the real position of the aircraft. This can eventually be detected but by then the position
            // may have been used for aircraft tracks etc., so this event is raised when the situation is seen and
            // before the message has been translated.

            var cpr = Factory.Singleton.Resolve<ICompactPositionReporting>();
            var adsbMessage = CreateAdsbMessagesForExtendedSquitters().First();
            adsbMessage.ModeSMessage.Icao24 = 0xabc123;
            var airborneMessage = adsbMessage.AirbornePosition = new AirbornePositionMessage();
            var now = DateTime.UtcNow;

            BaseStationMessage lastMessage = null;
            _PositionResetEvent.EventRaised += (o, a) => { Assert.IsNull(lastMessage); };

            // These two airborne messages too far apart to decode to the correct position. In this case the
            // latitude is correct but the longitude is 9° out.
            airborneMessage.CompactPosition = cpr.Encode(new GlobalCoordinate(51.0, -0.6), true, 17);
            _Translator.Translate(now, adsbMessage.ModeSMessage, adsbMessage);
            airborneMessage.CompactPosition = cpr.Encode(new GlobalCoordinate(50.999651, -0.314194), false, 17);
            var sanityCheck = _Translator.Translate(now, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(9.4, sanityCheck.Longitude.Value, 0.1);

            // The ICAO spec says that another two odd/even need to (eventually) be seen to validate the original fix.
            // On receipt of the second we should see the event get raised before the message is returned from Translate.
            airborneMessage.CompactPosition = cpr.Encode(new GlobalCoordinate(50.999650, -0.299904), true, 17);
            _Translator.Translate(now, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(0, _PositionResetEvent.CallCount);
            airborneMessage.CompactPosition = cpr.Encode(new GlobalCoordinate(50.999648, -0.285613), false, 17);
            lastMessage = _Translator.Translate(now, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(1, _PositionResetEvent.CallCount);
            Assert.AreEqual("ABC123", _PositionResetEvent.Args.Value);
        }
        #endregion

        #region Translate - Basic Details
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RawMessageTranslator_Translate_Throws_When_Statistics_Is_Null()
        {
            _Translator.Statistics = null;
            _Translator.Translate(_NowUtc, null, null);
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Returns_Null_When_Passed_Null_Messages()
        {
            Assert.IsNull(_Translator.Translate(_NowUtc, null, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Returns_Null_After_Dispose_Has_Been_Called()
        {
            var message = CreateModeSMessagesWithPIField().First();
            message.Icao24 = 12345;

            _Translator.Dispose();

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_SignalLevel_From_ModeSMessage_To_BaseStationMessage()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField().Concat(CreateModeSMessagesWithNoPIField())) {
                modeSMessage.Icao24 = 112233;

                modeSMessage.SignalLevel = 99;
                var baseStationMessage = _Translator.Translate(_NowUtc, modeSMessage, null);
                Assert.AreEqual(99, baseStationMessage.SignalLevel);

                modeSMessage.SignalLevel = null;
                baseStationMessage = _Translator.Translate(_NowUtc, modeSMessage, null);
                Assert.AreEqual(null, baseStationMessage.SignalLevel);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_IsMlat_From_ModeSMessage_To_BaseStationMessage()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField().Concat(CreateModeSMessagesWithNoPIField())) {
                modeSMessage.Icao24 = 112233;

                modeSMessage.IsMlat = true;
                var baseStationMessage = _Translator.Translate(_NowUtc, modeSMessage, null);
                Assert.AreEqual(true, baseStationMessage.IsMlat);

                modeSMessage.IsMlat = false;
                baseStationMessage = _Translator.Translate(_NowUtc, modeSMessage, null);
                Assert.AreEqual(false, baseStationMessage.IsMlat);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Sets_IsTisb_When_ModeSMessage_Is_FineFormat_Or_CoarseFormat_Tisb()
        {
            foreach(ControlField controlField in Enum.GetValues(typeof(ControlField))) {
                foreach(var imfValue in new byte?[] { null, 0, 1 }) {
                    _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder;
                    _ModeSMessage.ControlField = controlField;
                    _AdsbMessage.TisbIcaoModeAFlag = imfValue;

                    var baseStationMessage = _Translator.Translate(_NowUtc, _ModeSMessage, _AdsbMessage);

                    // I think we should ignore the IMF flag for our purposes, we just want an answer to the broad "was this TIS-B?", but the translator currently ignores messages that aren't IMF=0
                    var expected = imfValue == 0;
                    if(expected) {
                        switch(controlField) {
                            case ControlField.CoarseFormatTisb:
                            case ControlField.FineFormatTisb:
                                break;
                            default:
                                expected = false;
                                break;
                        }
                    }
                    var actual = baseStationMessage == null ? false : baseStationMessage.IsTisb;
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Accepts_Mode_S_Messages_That_Transmit_ICAO24_With_PI_Field()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();
                modeSMessage.Icao24 = icao24;

                Assert.IsNotNull(_Translator.Translate(_NowUtc, modeSMessage, null));
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Mode_S_Messages_That_Transmit_ICAO24_With_A_NonZero_PI_Field()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();
                modeSMessage.Icao24 = icao24;
                modeSMessage.ParityInterrogatorIdentifier = 1;

                Assert.IsNull(_Translator.Translate(_NowUtc, modeSMessage, null));
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Mode_S_Messages_That_Transmit_ICAO24_Should_Have_A_PI_Field_But_It_Is_Null()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();
                modeSMessage.Icao24 = icao24;
                modeSMessage.ParityInterrogatorIdentifier = null;

                Assert.IsNull(_Translator.Translate(_NowUtc, modeSMessage, null));
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Updates_Statistics_For_Mode_S_Messages_That_Transmit_ICAO24_With_A_NonZero_PI_Field()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                foreach(var badParity in new bool[] { true, false }) {
                    _Statistics.Object.AdsbRejected = 0;
                    var icao24 = CreateRandomIcao24();
                    modeSMessage.Icao24 = icao24;
                    modeSMessage.ParityInterrogatorIdentifier = badParity ? 1 : 0;

                    _Translator.Translate(_NowUtc, modeSMessage, null);

                    var expected = badParity && modeSMessage.DownlinkFormat != DownlinkFormat.AllCallReply ? 1 : 0;
                    Assert.AreEqual(expected, _Statistics.Object.AdsbRejected, "{0}/{1}", badParity, modeSMessage);
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AcceptIcao$")]
        public void RawMessageTranslator_Translate_Accepts_Icaos_As_Valid_Based_On_Number_Of_Times_Seen()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Translator.AcceptIcaoInPI0Count = worksheet.Int("PI0Count");
            _Translator.AcceptIcaoInPI0Milliseconds = worksheet.Int("PI0Ms");
            _Translator.AcceptIcaoInNonPICount = worksheet.Int("NonPICount");
            _Translator.AcceptIcaoInNonPIMilliseconds = worksheet.Int("NonPIMs");

            IEnumerable<ModeSMessage> messages;
            switch(worksheet.String("DownlinkFormat").ToUpper()) {
                case "HASPI":       messages = CreateModeSMessagesWithPIField(); break;
                case "NOPI":        messages = CreateModeSMessagesWithNoPIField(); break;
                default:            throw new NotImplementedException();
            }

            foreach(var message in messages) {
                message.Icao24 = CreateRandomIcao24();
                var originalDF = message.DownlinkFormat;
                var originalParity = message.ParityInterrogatorIdentifier;

                for(var messageNumber = 1;messageNumber <= 3;++messageNumber) {
                    var timeColumn = String.Format("Msg{0}Time", messageNumber);
                    var acceptedColumn = String.Format("Accepted{0}", messageNumber);
                    var piColumn = String.Format("Msg{0}PI", messageNumber);

                    if(worksheet.String(acceptedColumn) != null) {
                        message.DownlinkFormat = originalDF;
                        message.ParityInterrogatorIdentifier = worksheet.NInt(piColumn);
                        if(message.ParityInterrogatorIdentifier == null && originalParity != null)      message.DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance;
                        else if(message.ParityInterrogatorIdentifier != null && originalParity == null) message.DownlinkFormat = DownlinkFormat.ExtendedSquitter;

                        var now = _NowUtc.AddMilliseconds(worksheet.Int(timeColumn));
                        _Clock.UtcNowValue = now;
                        var accepted = _Translator.Translate(now, message, null) != null;
                        var expected = worksheet.Bool(acceptedColumn);

                        Assert.AreEqual(expected, accepted, "{0} message {1}: {2}", originalDF, messageNumber, worksheet.String("Comments"));
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Consider_Messages_With_Parity_DF_But_No_PI_As_Parity()
        {
            _Translator.AcceptIcaoInPI0Count = 2;
            _Translator.AcceptIcaoInPI0Milliseconds = 10000;
            _Translator.AcceptIcaoInNonPICount = 10;
            _Translator.AcceptIcaoInNonPIMilliseconds = 1;
            _Translator.IgnoreMilitaryExtendedSquitter = false;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter, ApplicationField = ApplicationField.AF1, ParityInterrogatorIdentifier = null, Icao24 = CreateRandomIcao24() };

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Consider_Messages_With_Parity_DF_But_No_PI_As_NonParity()
        {
            _Translator.AcceptIcaoInNonPICount = 2;
            _Translator.AcceptIcaoInNonPIMilliseconds = 10000;
            _Translator.AcceptIcaoInPI0Count = 10;
            _Translator.AcceptIcaoInPI0Milliseconds = 1;
            _Translator.IgnoreMilitaryExtendedSquitter = false;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter, ApplicationField = ApplicationField.AF1, ParityInterrogatorIdentifier = null, Icao24 = CreateRandomIcao24() };

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Count_NonParity_Messages_With_An_Unassigned_CodeBlock()
        {
            _Translator.IgnoreInvalidCodeBlockInOtherMessages = true;
            
            _Translator.AcceptIcaoInNonPICount = 2;
            _Translator.AcceptIcaoInNonPIMilliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns((CodeBlock)null);

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Can_Be_Forced_To_Use_NonParity_Messages_With_An_Unassigned_CodeBlock()
        {
            _Translator.IgnoreInvalidCodeBlockInOtherMessages = false;

            _Translator.AcceptIcaoInNonPICount = 2;
            _Translator.AcceptIcaoInNonPIMilliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns((CodeBlock)null);

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNotNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Count_NonParity_Messages_With_A_CodeBlock_For_An_Unknown_Country()
        {
            _Translator.AcceptIcaoInNonPICount = 2;
            _Translator.AcceptIcaoInNonPIMilliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns(new CodeBlock() { Country = "" });

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Count_NonParity_Messages_With_An_Unassigned_CodeBlock_When_CodeBlocks_Are_Not_Loaded()
        {
            _Translator.AcceptIcaoInNonPICount = 2;
            _Translator.AcceptIcaoInNonPIMilliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns((CodeBlock)null);
            _StandingDataManager.Setup(r => r.CodeBlocksLoaded).Returns(false);

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNotNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Count_Parity_Messages_With_An_Unassigned_CodeBlock()
        {
            _Translator.IgnoreInvalidCodeBlockInParityMessages = false;
            _Translator.AcceptIcaoInPI0Count = 2;
            _Translator.AcceptIcaoInPI0Milliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns((CodeBlock)null);

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNotNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Can_Be_Forced_Not_To_Count_Parity_Messages_With_An_Unassigned_CodeBlock()
        {
            _Translator.IgnoreInvalidCodeBlockInParityMessages = true;
            _Translator.AcceptIcaoInPI0Count = 2;
            _Translator.AcceptIcaoInPI0Milliseconds = 10000;

            var message = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(message.Icao24.ToString("X6"))).Returns((CodeBlock)null);

            Assert.IsNull(_Translator.Translate(_NowUtc, message, null));
            Assert.IsNull(_Translator.Translate(_NowUtc.AddMilliseconds(1), message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Uses_NonParity_Messages_With_An_Unassigned_CodeBlock_After_ICAO_Has_Been_Accepted()
        {
            var parityMessage = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, ParityInterrogatorIdentifier = 0, Icao24 = CreateRandomIcao24() };
            _StandingDataManager.Setup(r => r.FindCodeBlock(parityMessage.Icao24.ToString("X6"))).Returns((CodeBlock)null);
            _Translator.Translate(_NowUtc, parityMessage, null);

            var nonParityMessage = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ShortAirToAirSurveillance, Icao24 = parityMessage.Icao24 };

            Assert.IsNotNull(_Translator.Translate(_NowUtc, nonParityMessage, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Update_Statistics_For_Mode_S_Messages_Where_ADSB_Rejected_Because_Count_Not_Reached()
        {
            _Translator.AcceptIcaoInNonPICount = 3;
            _Translator.AcceptIcaoInNonPIMilliseconds = 1000;

            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                foreach(var badParity in new bool[] { true, false }) {
                    _Statistics.Object.AdsbRejected = 0;
                    var icao24 = CreateRandomIcao24();
                    modeSMessage.Icao24 = icao24;

                    _Translator.Translate(_NowUtc, modeSMessage, null);

                    Assert.AreEqual(0, _Statistics.Object.AdsbRejected, "{0}/{1}", badParity, modeSMessage);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Only_Accepts_Mode_S_Messages_Without_PI_Field_If_Preceeded_By_Message_With_PI_Field_Within_Global_Timeout_Period()
        {
            foreach(var cleanIcao24Message in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();

                foreach(var parityIcao24Message in CreateModeSMessagesWithNoPIField()) {
                    cleanIcao24Message.Icao24 = icao24;
                    parityIcao24Message.Icao24 = icao24;

                    TestCleanup();
                    TestInitialise();

                    _Translator.TrackingTimeoutSeconds = 100;
                    var timeoutMilliseconds = 100 * 1000;

                    // Message comes in with a clean ICAO24 at _NowUtc
                    _Clock.UtcNowValue = _NowUtc;
                    _Translator.Translate(_NowUtc, cleanIcao24Message, null);

                    // If a parity-overlaid message arrives before the timeout expires then the translator should accept it
                    var now = _NowUtc.AddMilliseconds(timeoutMilliseconds - 1);
                    _Clock.UtcNowValue = now;
                    Assert.IsNotNull(_Translator.Translate(now, parityIcao24Message, null));

                    // The parity-overlaid message should reset the timeout, so if one arrives a couple of milliseconds after the
                    // timeout would have expired from the first message seen for the aircraft then it should still accept it
                    now = now.AddMilliseconds(2);
                    _Clock.UtcNowValue = now;
                    Assert.IsNotNull(_Translator.Translate(now, parityIcao24Message, null));

                    // But if a message arrives after the timeout from the last message received has expired then it should be ignored
                    now = now.AddMilliseconds(timeoutMilliseconds);
                    _Clock.UtcNowValue = now;
                    Assert.IsNull(_Translator.Translate(now, parityIcao24Message, null));

                    // And it should keep on ignoring it
                    now = now.AddMilliseconds(1);
                    _Clock.UtcNowValue = now;
                    Assert.IsNull(_Translator.Translate(now, parityIcao24Message, null));
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Increments_Statistic_Counter_When_Short_Frame_Seen_Before_Long_Frame()
        {
            foreach(var cleanIcao24Message in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();

                foreach(var parityIcao24Message in CreateModeSMessagesWithNoPIField()) {
                    cleanIcao24Message.Icao24 = icao24;
                    parityIcao24Message.Icao24 = icao24;

                    TestCleanup();
                    TestInitialise();

                    // Message comes in with overlaid parity - shouldn't use this as we don't have a clean ICAO24 message yet
                    _Translator.Translate(_NowUtc, parityIcao24Message, null);
                    Assert.AreEqual(1L, _Statistics.Object.ModeSShortFrameWithoutLongFrameMessagesReceived);

                    // Counter keeps going up each time a message comes in without a clean ICAO24
                    _Translator.Translate(_NowUtc, parityIcao24Message, null);
                    Assert.AreEqual(2L, _Statistics.Object.ModeSShortFrameWithoutLongFrameMessagesReceived);

                    // Once a clean ICAO24 message comes in the counter should stop going up
                    _Translator.Translate(_NowUtc, cleanIcao24Message, null);
                    Assert.AreEqual(2L, _Statistics.Object.ModeSShortFrameWithoutLongFrameMessagesReceived);

                    // And subsequent parity overlaid ICAO24s won't increment the counter
                    _Translator.Translate(_NowUtc, parityIcao24Message, null);
                    Assert.AreEqual(2L, _Statistics.Object.ModeSShortFrameWithoutLongFrameMessagesReceived);

                    // Over the whole thing we don't count these as ADS-B rejected messages
                    Assert.AreEqual(0L, _Statistics.Object.AdsbRejected);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Removes_Timed_Out_Aircraft_On_Heartbeat_Event()
        {
            var icao24 = CreateRandomIcao24();
            var cleanIcao24Message = CreateModeSMessagesWithPIField().First();
            var parityIcao24Message = CreateModeSMessagesWithNoPIField().First();
            cleanIcao24Message.Icao24 = parityIcao24Message.Icao24 = icao24;

            var now = _NowUtc;
            _Clock.UtcNowValue = now;

            // Receipt of a clean ICAO24 message should begin the tracking of the aircraft
            Assert.IsNotNull(_Translator.Translate(now, cleanIcao24Message, null));

            _Translator.TrackingTimeoutSeconds = 400;
            var timeoutMilliseconds = 400 * 1000;

            // A heartbeat event raised just before the timeout expires on that message should not affect tracking of the aircraft
            now = now.AddMilliseconds(timeoutMilliseconds - 1);
            _Clock.UtcNowValue = now;
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNotNull(_Translator.Translate(now, parityIcao24Message, null));

            // A heartbeat event raised just on the timeout should remove the aircraft from the tracked list and prevent a parity
            // encoded ICAO24 from being converted into a BaseStation message.
            now = now.AddMilliseconds(timeoutMilliseconds);
            _Clock.UtcNowValue = now;
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNull(_Translator.Translate(now, parityIcao24Message, null));
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Updates_AdsbAircraftTracked_Statistic()
        {
            var icao24 = CreateRandomIcao24();
            var cleanIcao24Message = CreateModeSMessagesWithPIField().First();
            var parityIcao24Message = CreateModeSMessagesWithNoPIField().First();
            cleanIcao24Message.Icao24 = parityIcao24Message.Icao24 = icao24;

            _Translator.TrackingTimeoutSeconds = 300;
            var timeoutMilliseconds = 300 * 1000;

            _Translator.Translate(_NowUtc, cleanIcao24Message, null);
            Assert.AreEqual(1, _Statistics.Object.AdsbAircraftTracked);

            var now = _NowUtc.AddMilliseconds(timeoutMilliseconds - 1);
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            now = _NowUtc.AddMilliseconds(timeoutMilliseconds);
            _Clock.UtcNowValue = now;
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0L, _Statistics.Object.AdsbAircraftTracked);
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Mode_S_Messages_For_Reserved_Downlink_Formats()
        {
            foreach(var modeSMessage in CreateModeSMessagesForReservedDownlinkFormats()) {
                var icao24 = CreateRandomIcao24();
                modeSMessage.Icao24 = icao24;

                Assert.IsNull(_Translator.Translate(_NowUtc, modeSMessage, null));
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Mode_S_Messages_For_Reserved_Downlink_Formats_Even_When_Preceeded_By_Clean_Icao24_Message()
        {
            foreach(var cleanIcao24Message in CreateModeSMessagesWithPIField()) {
                TestCleanup();
                TestInitialise();

                var icao24 = CreateRandomIcao24();
                cleanIcao24Message.Icao24 = icao24;
                _Translator.Translate(_NowUtc, cleanIcao24Message, null);

                foreach(var reservedMessage in CreateModeSMessagesForReservedDownlinkFormats()) {
                    reservedMessage.Icao24 = icao24;
                    Assert.IsNull(_Translator.Translate(_NowUtc, reservedMessage, null));
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Extended_Squitter_From_Non_Transponders_When_Icao24_Is_Unknown_Format()
        {
            foreach(ControlField controlField in Enum.GetValues(typeof(ControlField))) {
                foreach(var imfValue in new byte?[] { null, 0, 1 }) {
                    TestCleanup();
                    TestInitialise();

                    var icao24 = CreateRandomIcao24();
                    _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder;
                    _ModeSMessage.ControlField = controlField;
                    _ModeSMessage.Icao24 = icao24;

                    _AdsbMessage.TisbIcaoModeAFlag = imfValue;

                    var message = _Translator.Translate(_NowUtc, _ModeSMessage, _AdsbMessage);
                    switch(controlField) {
                        case ControlField.AdsbDeviceTransmittingIcao24:
                        case ControlField.AdsbRebroadcastOfExtendedSquitter:
                            Assert.IsNotNull(message);
                            Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                            break;
                        case ControlField.FineFormatTisb:
                        case ControlField.CoarseFormatTisb:
                            if(imfValue == 0) {         // AA field on Mode-S is valid ICAO24
                                Assert.IsNotNull(message);
                                Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                            } else {                        // IMF = null: not decoded correctly, IMF = 1: AA field is squawk and tracking number
                                Assert.IsNull(message);
                            }
                            break;
                        default:
                            Assert.IsNull(message);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Updates_Statistics_For_Extended_Squitter_From_Non_Transponders_When_Icao24_Is_Unknown_Format()
        {
            foreach(ControlField controlField in Enum.GetValues(typeof(ControlField))) {
                foreach(var badParity in new bool[] { true, false }) {
                    TestCleanup();
                    TestInitialise();

                    var icao24 = CreateRandomIcao24();
                    _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder;
                    _ModeSMessage.ControlField = controlField;
                    _ModeSMessage.Icao24 = icao24;
                    _ModeSMessage.ParityInterrogatorIdentifier = badParity ? 1 : 0;

                    var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);

                    var expected = badParity && (controlField == ControlField.AdsbDeviceTransmittingIcao24 || controlField == ControlField.AdsbRebroadcastOfExtendedSquitter) ? 1 : 0;
                    Assert.AreEqual(expected, _Statistics.Object.AdsbRejected, "{0}/{1}", badParity, controlField);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_ICAO24_With_Parity_Messages_After_Extended_Squitter_From_Non_Transponders_With_Unknown_Icao24_Formats()
        {
            foreach(ControlField controlField in Enum.GetValues(typeof(ControlField))) {
                TestCleanup();
                TestInitialise();

                var icao24 = CreateRandomIcao24();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder;
                _ModeSMessage.ControlField = controlField;
                _ModeSMessage.Icao24 = icao24;
                _Translator.Translate(_NowUtc, _ModeSMessage, null);

                foreach(var parityIcao24ModeSMessage in CreateModeSMessagesWithNoPIField()) {
                    parityIcao24ModeSMessage.Icao24 = icao24;
                    var message = _Translator.Translate(_NowUtc, parityIcao24ModeSMessage, null);

                    switch(controlField) {
                        case ControlField.AdsbDeviceTransmittingIcao24:
                        case ControlField.AdsbRebroadcastOfExtendedSquitter:
                            Assert.IsNotNull(message);
                            Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                            break;
                        default:
                            Assert.IsNull(message);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Military_Extended_Squitter_When_Application_Field_Is_Reserved()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                TestCleanup();
                TestInitialise();

                var icao24 = CreateRandomIcao24();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter;
                _ModeSMessage.ApplicationField = applicationField;
                _ModeSMessage.Icao24 = icao24;
                var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);

                switch(applicationField) {
                    case ApplicationField.ADSB:
                        Assert.IsNotNull(message);
                        Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                        break;
                    default:
                        Assert.IsNull(message);
                        break;
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Updates_Statistics_For_Military_Extended_Squitter_When_Application_Field_Is_Reserved()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                foreach(var badParity in new bool[] { true, false }) {
                    foreach(var ignoreMilitaryAdsb in new bool[] { true, false }) {
                        TestCleanup();
                        TestInitialise();

                        var icao24 = CreateRandomIcao24();
                        _ModeSMessage.DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter;
                        _ModeSMessage.ApplicationField = applicationField;
                        _ModeSMessage.Icao24 = icao24;
                        _ModeSMessage.ParityInterrogatorIdentifier = badParity ? 1 : 0;
                        _Translator.IgnoreMilitaryExtendedSquitter = ignoreMilitaryAdsb;
                        var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);

                        var expected = badParity && !ignoreMilitaryAdsb && applicationField == ApplicationField.ADSB ? 1 : 0;
                        Assert.AreEqual(expected, _Statistics.Object.AdsbRejected, "{0}/{1}/{2}", badParity, ignoreMilitaryAdsb, applicationField);
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_ICAO24_With_Parity_Messages_After_Military_Extended_Squitter_With_Reserved_Application_Field()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                TestCleanup();
                TestInitialise();

                var icao24 = CreateRandomIcao24();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter;
                _ModeSMessage.ApplicationField = applicationField;
                _ModeSMessage.Icao24 = icao24;
                _Translator.Translate(_NowUtc, _ModeSMessage, null);

                foreach(var parityIcao24ModeSMessage in CreateModeSMessagesWithNoPIField()) {
                    parityIcao24ModeSMessage.Icao24 = icao24;
                    var message = _Translator.Translate(_NowUtc, parityIcao24ModeSMessage, null);

                    switch(applicationField) {
                        case ApplicationField.ADSB:
                            Assert.IsNotNull(message);
                            Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                            break;
                        default:
                            Assert.IsNull(message);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_Military_Extended_Squitter_When_IgnoreMilitaryExtendedSquitter_Is_Set()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                TestCleanup();
                TestInitialise();
                _Translator.IgnoreMilitaryExtendedSquitter = true;

                var icao24 = CreateRandomIcao24();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter;
                _ModeSMessage.ApplicationField = applicationField;
                _ModeSMessage.Icao24 = icao24;
                Assert.IsNull(_Translator.Translate(_NowUtc, _ModeSMessage, null));
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_ICAO24_With_Parity_Messages_After_Military_Extended_Squitter_When_IgnoreMilitaryExtendedSquitter_Is_Set()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                TestCleanup();
                TestInitialise();
                _Translator.IgnoreMilitaryExtendedSquitter = true;

                var icao24 = CreateRandomIcao24();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.MilitaryExtendedSquitter;
                _ModeSMessage.ApplicationField = applicationField;
                _ModeSMessage.Icao24 = icao24;
                _Translator.Translate(_NowUtc, _ModeSMessage, null);

                foreach(var parityIcao24ModeSMessage in CreateModeSMessagesWithNoPIField()) {
                    parityIcao24ModeSMessage.Icao24 = icao24;
                    Assert.IsNull(_Translator.Translate(_NowUtc, parityIcao24ModeSMessage, null));
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_SuppressTisbDecoding_Prevents_Creation_Of_BaseStationMessage_From_FineFormatTisb()
        {
            _Translator.SuppressTisbDecoding = true;
            var adsbMessage = CreateAdsbMessageForFindFormatTisbExtendedSquitter();
            adsbMessage.AirbornePosition = new AirbornePositionMessage() {
                BarometricAltitude = 100,
            };

            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(null, message);
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_SuppressTisbDecoding_Prevents_Creation_Of_BaseStationMessage_From_CoarseFormatTisb()
        {
            _Translator.SuppressTisbDecoding = true;
            var adsbMessage = CreateAdsbMessageForCoarseTisbAirbornePosition();
            adsbMessage.CoarseTisbAirbornePosition = new CoarseTisbAirbornePosition() {
                BarometricAltitude = 100,
            };

            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(null, message);
        }
        #endregion

        #region Translate - MessageType, TransmissionType
        [TestMethod]
        public void RawMessageTranslator_Translate_Sets_MessageType_Consistently()
        {
            var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.AreEqual(BaseStationMessageType.Transmission, message.MessageType);
            Assert.AreEqual(BaseStationStatusCode.None, message.StatusCode);

            message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.AreEqual(BaseStationMessageType.Transmission, message.MessageType);
            Assert.AreEqual(BaseStationStatusCode.None, message.StatusCode);
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Sets_TransmissionType_Correctly_For_Mode_S_Without_Adsb_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithPIField().Concat(CreateModeSMessagesWithNoPIField())) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                var downlinkFormat = modeSMessage.DownlinkFormat;

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                switch(downlinkFormat) {
                    case DownlinkFormat.AllCallReply:                   Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                    case DownlinkFormat.CommBAltitudeReply:             Assert.AreEqual(BaseStationTransmissionType.SurveillanceAlt, message.TransmissionType); break;
                    case DownlinkFormat.CommBIdentityReply:             Assert.AreEqual(BaseStationTransmissionType.SurveillanceId, message.TransmissionType); break;
                    case DownlinkFormat.CommD:                          Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                    case DownlinkFormat.ExtendedSquitter:               Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                    case DownlinkFormat.ExtendedSquitterNonTransponder: Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                    case DownlinkFormat.LongAirToAirSurveillance:       Assert.AreEqual(BaseStationTransmissionType.AirToAir, message.TransmissionType); break;
                    case DownlinkFormat.MilitaryExtendedSquitter:       Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                    case DownlinkFormat.ShortAirToAirSurveillance:      Assert.AreEqual(BaseStationTransmissionType.AirToAir, message.TransmissionType); break;
                    case DownlinkFormat.SurveillanceAltitudeReply:      Assert.AreEqual(BaseStationTransmissionType.SurveillanceAlt, message.TransmissionType); break;
                    case DownlinkFormat.SurveillanceIdentityReply:      Assert.AreEqual(BaseStationTransmissionType.SurveillanceId, message.TransmissionType); break;
                    default:                                            throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Sets_TransmissionType_Correctly_For_Mode_S_With_Adsb_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(MessageFormat messageFormat in Enum.GetValues(typeof(MessageFormat))) {
                    adsbMessage.MessageFormat = messageFormat;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    switch(messageFormat) {
                        case MessageFormat.AirbornePosition:            Assert.AreEqual(BaseStationTransmissionType.AirbornePosition, message.TransmissionType); break;
                        case MessageFormat.AirborneVelocity:            Assert.AreEqual(BaseStationTransmissionType.AirborneVelocity, message.TransmissionType); break;
                        case MessageFormat.AircraftOperationalStatus:   Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                        case MessageFormat.AircraftStatus:              Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                        case MessageFormat.IdentificationAndCategory:   Assert.AreEqual(BaseStationTransmissionType.IdentificationAndCategory, message.TransmissionType); break;
                        case MessageFormat.None:                        Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                        case MessageFormat.NoPositionInformation:       Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                        case MessageFormat.SurfacePosition:             Assert.AreEqual(BaseStationTransmissionType.SurfacePosition, message.TransmissionType); break;
                        case MessageFormat.TargetStateAndStatus:        Assert.AreEqual(BaseStationTransmissionType.AllCallReply, message.TransmissionType); break;
                        case MessageFormat.CoarseTisbAirbornePosition:  Assert.AreEqual(BaseStationTransmissionType.AirbornePosition, message.TransmissionType); break;
                        default:                                        throw new NotImplementedException();
                    }
                }
            }
        }
        #endregion

        #region Translate - ICAO24
        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Icao24_Code_From_Mode_S_Messages_That_Transmit_ICAO24_Without_Overlaid_Parity()
        {
            foreach(var modeSMessage in CreateModeSMessagesWithPIField()) {
                var icao24 = CreateRandomIcao24();
                modeSMessage.Icao24 = icao24;

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);
                Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Icao24_Code_From_Mode_S_Messages_With_Overlaid_Parity_If_Preceeded_By_Message_Without_Overlaid_Parity()
        {
            foreach(var cleanIcao24Message in CreateModeSMessagesWithPIField()) {
                TestCleanup();
                TestInitialise();

                var icao24 = CreateRandomIcao24();
                cleanIcao24Message.Icao24 = icao24;
                _Translator.Translate(_NowUtc, cleanIcao24Message, null);

                foreach(var parityIcao24Message in CreateModeSMessagesWithNoPIField()) {
                    parityIcao24Message.Icao24 = icao24;

                    var message = _Translator.Translate(_NowUtc, parityIcao24Message, null);

                    Assert.IsNotNull(message);
                    Assert.AreEqual(FormatIcao24(icao24), message.Icao24);
                }
            }
        }
        #endregion

        #region Translate - SessionId, AircraftId and FlightId
        [TestMethod]
        public void RawMessageTranslator_Translate_Writes_0_Into_SessionId_AircraftId_And_FlightId()
        {
            var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.AreEqual(0, message.SessionId);
            Assert.AreEqual(0, message.AircraftId);
            Assert.AreEqual(0, message.FlightId);
        }
        #endregion

        #region Translate - MessageGenerated, MessageLogged
        [TestMethod]
        public void RawMessageTranslator_Translate_Writes_Time_Message_Received_In_Utc_Into_MessageGenerated_And_MessageLogged()
        {
            var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.AreEqual(_NowUtc, message.MessageGenerated);
            Assert.AreEqual(_NowUtc, message.MessageLogged);
        }
        #endregion

        #region Translate - Callsign
        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Callsign_From_ModeS_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithPossibleCallsigns()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                modeSMessage.PossibleCallsign = "  CALL SIGN  ";

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                Assert.AreEqual("CALL SIGN", message.Callsign);
                Assert.IsTrue(message.Supplementary.CallsignIsSuspect.Value);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Copy_Empty_Callsign_From_ModeS_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithPossibleCallsigns()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                modeSMessage.PossibleCallsign = "";

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                Assert.AreEqual(null, message.Callsign);
                if(message.Supplementary != null) Assert.IsNull(message.Supplementary.CallsignIsSuspect.Value);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Does_Not_Copy_Callsign_From_ModeS_Messages_When_Option_To_Suppress_Is_Active()
        {
            _Translator.SuppressCallsignsFromBds20 = true;
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithPossibleCallsigns()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                modeSMessage.PossibleCallsign = "  CALL SIGN  ";

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                Assert.AreEqual(null, message.Callsign);
                if(message.Supplementary != null) Assert.IsFalse(message.Supplementary.CallsignIsSuspect.GetValueOrDefault());
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Callsign_From_Adsb_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.IdentifierAndCategory = new IdentifierAndCategoryMessage() { Identification = "  CALL SIGN  " };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.AreEqual("CALL SIGN", message.Callsign);
                Assert.IsFalse(message.Supplementary.CallsignIsSuspect.Value);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Callsign_From_Adsb_Messages_Even_If_SuppressCallsignFromBds20_Is_True()
        {
            _Translator.SuppressCallsignsFromBds20 = true;
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.IdentifierAndCategory = new IdentifierAndCategoryMessage() { Identification = "  CALL SIGN  " };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.AreEqual("CALL SIGN", message.Callsign);
                 Assert.IsFalse(message.Supplementary.CallsignIsSuspect.Value);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Ignores_ModeS_Callsigns_Once_One_Is_Seen_In_Adsb_Message()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);

            _ModeSMessage.PossibleCallsign = "DODGY";
            var message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.AreEqual("DODGY", message.Callsign);
            Assert.IsTrue(message.Supplementary.CallsignIsSuspect.GetValueOrDefault());

            _AdsbMessage.MessageFormat = MessageFormat.IdentificationAndCategory;
            _AdsbMessage.IdentifierAndCategory = new IdentifierAndCategoryMessage() { Identification = "REAL" };
            _ModeSMessage.PossibleCallsign = null;
            message = _Translator.Translate(_NowUtc, _ModeSMessage, _AdsbMessage);
            Assert.AreEqual("REAL", message.Callsign);
            Assert.IsFalse(message.Supplementary.CallsignIsSuspect.Value);

            _ModeSMessage.PossibleCallsign = "DODGY";
            message = _Translator.Translate(_NowUtc, _ModeSMessage, null);
            Assert.IsNull(message.Callsign);
            if(message.Supplementary != null) Assert.IsFalse(message.Supplementary.CallsignIsSuspect.GetValueOrDefault());
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Prefers_ADSB_Callsign_To_ModeS_Callsign_If_Both_Present()
        {
            // This should never happen in real life but if the messages are coming from a made-up feed then it might...
            _Translator.Translate(_NowUtc, _ModeSMessage, null);

            _ModeSMessage.PossibleCallsign = "DODGY";
            _AdsbMessage.MessageFormat = MessageFormat.IdentificationAndCategory;
            _AdsbMessage.IdentifierAndCategory = new IdentifierAndCategoryMessage() { Identification = "REAL" };
            var message = _Translator.Translate(_NowUtc, _ModeSMessage, _AdsbMessage);
            Assert.AreEqual("REAL", message.Callsign);
            Assert.IsFalse(message.Supplementary.CallsignIsSuspect.Value);
        }
        #endregion

        #region Translate - Altitude
        [TestMethod]
        public void RawMessageTranslator_Translate_Copies_Altitude_From_ModeS_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithAltitudeCodes()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                modeSMessage.Altitude = 3400;

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                Assert.AreEqual(3400, message.Altitude);
                Assert.IsNull(message.Supplementary);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Altitudes_From_Adsb_Airborne_Position_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var isBarometric in new bool[] { true, false }) {
                    adsbMessage.AirbornePosition = new AirbornePositionMessage();
                    if(isBarometric) adsbMessage.AirbornePosition.BarometricAltitude = 4100;
                    else             adsbMessage.AirbornePosition.GeometricAltitude = 4100;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(4100, message.Altitude);
                    if(!isBarometric) Assert.IsTrue(message.Supplementary.AltitudeIsGeometric.GetValueOrDefault());
                    else if(message.Supplementary != null) Assert.AreEqual(false, message.Supplementary.AltitudeIsGeometric);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Altitudes_From_Adsb_Coarse_TISB_Airborne_Position_Messages()
        {
            var adsbMessage = CreateAdsbMessageForCoarseTisbAirbornePosition();
            adsbMessage.CoarseTisbAirbornePosition.BarometricAltitude = 4100;

            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(4100, message.Altitude);
            if(message.Supplementary != null) Assert.AreEqual(false, message.Supplementary.AltitudeIsGeometric);
        }
        #endregion

        #region Translate - GroundSpeed
        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_GroundSpeed_From_Adsb_Airborne_Velocity_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var velocityType in new VelocityType[] { VelocityType.AirspeedSubsonic, VelocityType.AirspeedSupersonic, VelocityType.GroundSpeedSubsonic, VelocityType.GroundSpeedSupersonic }) {
                    foreach(var airspeedIsTrueAirspeed in new bool[] { true, false }) {
                        adsbMessage.AirborneVelocity = new AirborneVelocityMessage() { VelocityType = velocityType, };
                        switch(velocityType) {
                            case VelocityType.AirspeedSubsonic:
                            case VelocityType.AirspeedSupersonic:
                                adsbMessage.AirborneVelocity.Airspeed = 1234;
                                adsbMessage.AirborneVelocity.AirspeedIsTrueAirspeed = airspeedIsTrueAirspeed;
                                break;
                            case VelocityType.GroundSpeedSubsonic:
                            case VelocityType.GroundSpeedSupersonic:
                                adsbMessage.AirborneVelocity.VectorVelocity = new VectorVelocity() { EastWestVelocity = 1234, NorthSouthVelocity = 0, };
                                break;
                        }

                        var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                        switch(velocityType) {
                            case VelocityType.AirspeedSubsonic:
                            case VelocityType.AirspeedSupersonic:
                                Assert.AreEqual(1234F, message.GroundSpeed);
                                Assert.AreEqual(airspeedIsTrueAirspeed ? SpeedType.TrueAirSpeed : SpeedType.IndicatedAirSpeed, message.Supplementary.SpeedType);
                                break;
                            case VelocityType.GroundSpeedSubsonic:
                            case VelocityType.GroundSpeedSupersonic:
                                Assert.AreEqual(1234F, message.GroundSpeed);
                                if(message.Supplementary != null) Assert.AreEqual(SpeedType.GroundSpeed, message.Supplementary.SpeedType);
                                break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_GroundSpeed_From_Adsb_Surface_Position_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var isReversing in new bool[] { true, false }) {
                    adsbMessage.SurfacePosition = new SurfacePositionMessage() { GroundSpeed = 123, IsReversing = isReversing };

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(123F, message.GroundSpeed);
                    if(isReversing) Assert.AreEqual(SpeedType.GroundSpeedReversing, message.Supplementary.SpeedType);
                    else {
                        if(message.Supplementary != null) Assert.AreEqual(SpeedType.GroundSpeed, message.Supplementary.SpeedType);
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_GroundSpeed_From_Adsb_Coarse_TISB_Airborne_Position_Messages()
        {
            var adsbMessage = CreateAdsbMessageForCoarseTisbAirbornePosition();
            adsbMessage.CoarseTisbAirbornePosition.GroundSpeed = 123;

            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(123F, message.GroundSpeed);
            if(message.Supplementary != null) {
                Assert.AreEqual(SpeedType.GroundSpeed, message.Supplementary.SpeedType);
            }
        }
        #endregion

        #region Translate - Track
        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Track_From_Adsb_Airborne_Velocity_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var velocityType in new VelocityType[] { VelocityType.AirspeedSubsonic, VelocityType.AirspeedSupersonic, VelocityType.GroundSpeedSubsonic, VelocityType.GroundSpeedSupersonic }) {
                    adsbMessage.AirborneVelocity = new AirborneVelocityMessage() { VelocityType = velocityType, };
                    switch(velocityType) {
                        case VelocityType.AirspeedSubsonic:
                        case VelocityType.AirspeedSupersonic:
                            adsbMessage.AirborneVelocity.Heading = 193.153;
                            break;
                        case VelocityType.GroundSpeedSubsonic:
                        case VelocityType.GroundSpeedSupersonic:
                            adsbMessage.AirborneVelocity.VectorVelocity = new VectorVelocity() { EastWestVelocity = 1234, NorthSouthVelocity = 0, IsWesterlyVelocity = true, };
                            break;
                    }

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    switch(velocityType) {
                        case VelocityType.AirspeedSubsonic:
                        case VelocityType.AirspeedSupersonic:
                            Assert.AreEqual(193.2F, message.Track);
                            Assert.AreEqual(true, message.Supplementary.TrackIsHeading);
                            break;
                        case VelocityType.GroundSpeedSubsonic:
                        case VelocityType.GroundSpeedSupersonic:
                            Assert.AreEqual(270F, message.Track);
                            if(message.Supplementary != null) Assert.AreEqual(false, message.Supplementary.TrackIsHeading);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Track_From_Adsb_Surface_Position_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var isReversing in new bool[] { true, false }) {
                    adsbMessage.SurfacePosition = new SurfacePositionMessage() { GroundTrack = 97.153, IsReversing = isReversing };

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(97.2F, message.Track);
                    if(isReversing) Assert.AreEqual(SpeedType.GroundSpeedReversing, message.Supplementary.SpeedType);
                    else {
                        if(message.Supplementary != null) Assert.AreEqual(SpeedType.GroundSpeed, message.Supplementary.SpeedType);
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Track_From_Adsb_Coarse_TISB_Airborne_Position_Messages()
        {
            var adsbMessage = CreateAdsbMessageForCoarseTisbAirbornePosition();
            adsbMessage.CoarseTisbAirbornePosition.GroundTrack = 97.153;

            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(97.2F, message.Track);
        }
        #endregion

        #region Translate - Latitude and Longitude
        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RawTranslatePosition$")]
        public void RawMessageTranslator_Translate_Extracts_Position_From_ADSB_Messages_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Translator.ReceiverLocation = ParseGlobalPosition(worksheet.String("ReceiverPosn"));
            _Translator.ReceiverRangeKilometres = worksheet.Int("Range");
            _Translator.GlobalDecodeAirborneThresholdMilliseconds = worksheet.Int("GATS");
            _Translator.GlobalDecodeFastSurfaceThresholdMilliseconds = worksheet.Int("GFSTS");
            _Translator.GlobalDecodeSlowSurfaceThresholdMilliseconds = worksheet.Int("GSSTS");
            _Translator.LocalDecodeMaxSpeedAirborne = worksheet.Double("LAMS");
            _Translator.LocalDecodeMaxSpeedTransition = worksheet.Double("LTMS");
            _Translator.LocalDecodeMaxSpeedSurface = worksheet.Double("LSMS");
            _Translator.SuppressReceiverRangeCheck = worksheet.Bool("SRRC");
            _Translator.UseLocalDecodeForInitialPosition = worksheet.Bool("ULD");

            DateTime now = DateTime.UtcNow;
            for(var i = 1;i <= 4;++i) {
                var millisecondsColumn = String.Format("MSec{0}", i);
                var cprColumn = String.Format("CPR{0}", i);
                var speedColumn = String.Format("Spd{0}", i);
                var positionColumn = String.Format("Posn{0}", i);

                if(worksheet.String(cprColumn) == null) continue;
                var cpr = ParseCpr(worksheet.String(cprColumn));
                var speed = worksheet.NDouble(speedColumn);
                var expectedPosition = ParseGlobalPosition(worksheet.String(positionColumn));

                if(i != 1 && worksheet.String(millisecondsColumn) != null) {
                    now = now.AddMilliseconds(worksheet.Int(millisecondsColumn));
                }

                var modeSMessage = new ModeSMessage() { DownlinkFormat = DownlinkFormat.ExtendedSquitter, Icao24 = 0x112233, ParityInterrogatorIdentifier = 0 };
                var adsbMessage = new AdsbMessage(modeSMessage);
                switch(cpr.NumberOfBits) {
                    case 17:
                        adsbMessage.MessageFormat = MessageFormat.AirbornePosition;
                        adsbMessage.AirbornePosition = new AirbornePositionMessage() { CompactPosition = cpr };
                        break;
                    case 19:
                        adsbMessage.MessageFormat = MessageFormat.SurfacePosition;
                        adsbMessage.SurfacePosition = new SurfacePositionMessage() { CompactPosition = cpr, GroundSpeed = speed, };
                        break;
                }

                var baseStationMessage = _Translator.Translate(now, modeSMessage, adsbMessage);

                var failMessage = String.Format("Failed on message {0}", i);
                if(expectedPosition == null) {
                    if(baseStationMessage != null) {
                        if(baseStationMessage.Latitude != null || baseStationMessage.Longitude != null) {
                            Assert.Fail(String.Format("Position decoded to {0}/{1} erroneously. {2}", baseStationMessage.Latitude, baseStationMessage.Longitude, failMessage));
                        }
                    }
                } else {
                    Assert.IsNotNull(baseStationMessage.Latitude, failMessage);
                    Assert.IsNotNull(baseStationMessage.Longitude, failMessage);
                    Assert.AreEqual(expectedPosition.Latitude, baseStationMessage.Latitude.Value, 0.0001, failMessage);
                    Assert.AreEqual(expectedPosition.Longitude, baseStationMessage.Longitude.Value, 0.0001, failMessage);
                }
            }

            Assert.AreEqual(worksheet.Int("ResetCount"), _PositionResetEvent.CallCount);
            Assert.AreEqual(worksheet.Int("ResetCount") > 0 ? 1L : 0L, _Statistics.Object.AdsbPositionsReset);
            Assert.AreEqual(worksheet.Long("BadRange"), _Statistics.Object.AdsbPositionsOutsideRange);
            Assert.AreEqual(worksheet.Long("BadSpeed"), _Statistics.Object.AdsbPositionsExceededSpeedCheck);
        }

        private GlobalCoordinate ParseGlobalPosition(string text)
        {
            GlobalCoordinate result = null;
            if(!String.IsNullOrWhiteSpace(text)) {
                var chunks = text.Trim().Split('~');
                Assert.AreEqual(2, chunks.Length);
                double latitude = 0, longitude = 0;
                bool parsedOK = true;
                for(var i = 0;i < chunks.Length;++i) {
                    switch(i) {
                        case 0: parsedOK = parsedOK && double.TryParse(chunks[i], NumberStyles.Any, CultureInfo.InvariantCulture, out latitude); break;
                        case 1: parsedOK = parsedOK && double.TryParse(chunks[i], NumberStyles.Any, CultureInfo.InvariantCulture, out longitude); break;
                    }
                }
                Assert.IsTrue(parsedOK);
                result = new GlobalCoordinate(latitude, longitude);
            }

            return result;
        }

        private CompactPositionReportingCoordinate ParseCpr(string text)
        {
            CompactPositionReportingCoordinate result = null;
            if(!String.IsNullOrWhiteSpace(text)) {
                var chunks = text.Trim().Split('~');
                Assert.AreEqual(4, chunks.Length);
                double latitude = 0, longitude = 0;
                int bits = 0;
                bool? isOddFormat = null;
                bool parsedOK = true;
                for(var i = 0;i < chunks.Length;++i) {
                    var chunk = chunks[i];
                    switch(i) {
                        case 0: parsedOK = parsedOK && double.TryParse(chunk, NumberStyles.Any, CultureInfo.InvariantCulture, out latitude); break;
                        case 1: parsedOK = parsedOK && double.TryParse(chunk, NumberStyles.Any, CultureInfo.InvariantCulture, out longitude); break;
                        case 2: bits = chunk == "A" ? 17 : chunk == "S" ? 19 : 0; parsedOK = parsedOK && bits != 0; break;
                        case 3: isOddFormat = chunk == "O" ? true : chunk == "E" ? false : (bool?)null; parsedOK = parsedOK && isOddFormat != null; break;
                    }
                }
                Assert.IsTrue(parsedOK);
                result = Factory.Singleton.Resolve<ICompactPositionReporting>().Encode(new GlobalCoordinate(latitude, longitude), isOddFormat.Value, (byte)bits);
            }

            return result;
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Latitude_And_Longitude_From_Adsb_Coarse_TISB_Airborne_Position_Messages()
        {
            var expectedLatitude = 51.4;
            var expectedLongitude = -0.6;
            var encoder = Factory.Singleton.Resolve<ICompactPositionReporting>();
            var oddCpr = encoder.Encode(new GlobalCoordinate(expectedLatitude, expectedLongitude), oddFormat: true, numberOfBits: 12);
            var evenCpr = encoder.Encode(new GlobalCoordinate(expectedLatitude, expectedLongitude), oddFormat: false, numberOfBits: 12);

            var adsbMessage = CreateAdsbMessageForCoarseTisbAirbornePosition();
            adsbMessage.CoarseTisbAirbornePosition.CompactPosition = oddCpr;
            var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            adsbMessage.CoarseTisbAirbornePosition.CompactPosition = evenCpr;
            message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

            Assert.AreEqual(51.4, (double)message.Latitude, 0.1);
            Assert.AreEqual(-0.6, (double)message.Longitude, 0.1);
        }
        #endregion

        #region Translate - VerticalRate
        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_VerticalRate_From_ADSB_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                foreach(var rateIsBarometric in new bool[] { true, false }) {
                    var airborneVelocity = adsbMessage.AirborneVelocity = new AirborneVelocityMessage();
                    airborneVelocity.VerticalRate = 100;
                    airborneVelocity.VerticalRateIsBarometric = rateIsBarometric;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(100, message.VerticalRate);
                    if(rateIsBarometric) Assert.IsTrue(message.Supplementary.VerticalRateIsGeometric.GetValueOrDefault());
                    else if(message.Supplementary != null) Assert.IsFalse(message.Supplementary.VerticalRateIsGeometric.GetValueOrDefault());
                }
            }
        }
        #endregion

        #region Translate - Squawk
        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Squawk_From_ModeS_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithSquawks()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                modeSMessage.Identity = (short)1234;

                var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                Assert.AreEqual((short)1234, message.Squawk);
                Assert.IsNull(message.Supplementary);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Squawk_From_ADSB_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                var aircraftStatus = adsbMessage.AircraftStatus = new AircraftStatusMessage();
                aircraftStatus.EmergencyStatus = new EmergencyStatus();
                aircraftStatus.EmergencyStatus.Squawk = (short)2345;

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.AreEqual((short)2345, message.Squawk);
            }
        }
        #endregion

        #region Translate - SquawkHasChanged
        [TestMethod]
        public void RawMessageTranslator_Translate_SquawkHasChanged_Is_Extracted_From_ModeS_FlightStatus()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithFlightStatus()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(FlightStatus flightStatus in Enum.GetValues(typeof(FlightStatus))) {
                    modeSMessage.FlightStatus = flightStatus;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(flightStatus) {
                        case FlightStatus.Airborne:
                        case FlightStatus.FS6:
                        case FlightStatus.FS7:
                        case FlightStatus.OnGround:
                        case FlightStatus.SpiWithNoAlert:
                            Assert.IsFalse(message.SquawkHasChanged.Value);
                            break;
                        case FlightStatus.AirborneAlert:
                        case FlightStatus.OnGroundAlert:
                        case FlightStatus.SpiWithAlert:
                            Assert.IsTrue(message.SquawkHasChanged.Value);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_SquawkHasChanged_Is_Extracted_From_ADSB_AirbornePosition_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                var airbornePosition = adsbMessage.AirbornePosition = new AirbornePositionMessage();
                foreach(SurveillanceStatus surveillanceStatus in Enum.GetValues(typeof(SurveillanceStatus))) {
                    airbornePosition.SurveillanceStatus = surveillanceStatus;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    switch(surveillanceStatus) {
                        case SurveillanceStatus.NoInformation:
                        case SurveillanceStatus.PermanentAlert:
                        case SurveillanceStatus.SpecialPositionIdentification:
                            Assert.IsFalse(message.SquawkHasChanged.Value);
                            break;
                        case SurveillanceStatus.TemporaryAlert:
                            Assert.IsTrue(message.SquawkHasChanged.Value);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Translate - Emergency
        [TestMethod]
        public void RawMessageTranslator_Translate_Infers_Emergency_From_Squawk_On_ModeS_Messages()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithSquawks()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(var squawk in new short?[] { null, 7477, 7500, 7501, 7577, 7600, 7601, 7677, 7700, 7701 }) {
                    modeSMessage.Identity = squawk;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(squawk) {
                        case null:
                            Assert.IsNull(message.Emergency);
                            break;
                        case 7500:
                        case 7600:
                        case 7700:
                            Assert.IsTrue(message.Emergency.Value);
                            break;
                        default:
                            Assert.IsFalse(message.Emergency.Value);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Emergency_From_Adsb_AircraftStatus_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                var aircraftStatus = adsbMessage.AircraftStatus = new AircraftStatusMessage();
                foreach(EmergencyState emergencyState in Enum.GetValues(typeof(EmergencyState))) {
                    aircraftStatus.EmergencyStatus = new EmergencyStatus() { EmergencyState = emergencyState };

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(emergencyState != EmergencyState.None, message.Emergency);
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_Extracts_Emergency_From_Adsb_TargetStateAndStatus_Version_1_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                var targetStateAndStatus = adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() { TargetStateAndStatusType = TargetStateAndStatusType.Version1 };
                var version1 = targetStateAndStatus.Version1 = new TargetStateAndStatusVersion1();

                foreach(EmergencyState emergencyState in Enum.GetValues(typeof(EmergencyState))) {
                    version1.EmergencyState = emergencyState;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    Assert.AreEqual(emergencyState != EmergencyState.None, message.Emergency);
                }
            }
        }
        #endregion

        #region Translate - IdentActive
        [TestMethod]
        public void RawMessageTranslator_Translate_IdentActive_Is_Extracted_From_ModeS_FlightStatus()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithFlightStatus()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(FlightStatus flightStatus in Enum.GetValues(typeof(FlightStatus))) {
                    modeSMessage.FlightStatus = flightStatus;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(flightStatus) {
                        case FlightStatus.Airborne:
                        case FlightStatus.FS6:
                        case FlightStatus.FS7:
                        case FlightStatus.OnGround:
                        case FlightStatus.AirborneAlert:
                        case FlightStatus.OnGroundAlert:
                            Assert.IsFalse(message.IdentActive.Value);
                            break;
                        case FlightStatus.SpiWithNoAlert:
                        case FlightStatus.SpiWithAlert:
                            Assert.IsTrue(message.IdentActive.Value);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_IdentActive_Is_Extracted_From_ADSB_AirbornePosition_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                var airbornePosition = adsbMessage.AirbornePosition = new AirbornePositionMessage();
                foreach(SurveillanceStatus surveillanceStatus in Enum.GetValues(typeof(SurveillanceStatus))) {
                    airbornePosition.SurveillanceStatus = surveillanceStatus;

                    var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                    switch(surveillanceStatus) {
                        case SurveillanceStatus.NoInformation:
                        case SurveillanceStatus.PermanentAlert:
                        case SurveillanceStatus.TemporaryAlert:
                            Assert.IsFalse(message.IdentActive.Value);
                            break;
                        case SurveillanceStatus.SpecialPositionIdentification:
                            Assert.IsTrue(message.IdentActive.Value);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Translate - OnGround
        [TestMethod]
        public void RawMessageTranslator_Translate_OnGround_Is_Extracted_From_ModeS_FlightStatus()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithFlightStatus()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(FlightStatus flightStatus in Enum.GetValues(typeof(FlightStatus))) {
                    modeSMessage.FlightStatus = flightStatus;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(flightStatus) {
                        case FlightStatus.Airborne:
                        case FlightStatus.AirborneAlert:
                            Assert.IsFalse(message.OnGround.Value);
                            break;
                        case FlightStatus.OnGround:
                        case FlightStatus.OnGroundAlert:
                            Assert.IsTrue(message.OnGround.Value);
                            break;
                        default:
                            Assert.IsNull(message.OnGround);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_OnGround_Is_Extracted_From_ModeS_Capability()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithCapability()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(Capability capability in Enum.GetValues(typeof(Capability))) {
                    modeSMessage.Capability = capability;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(capability) {
                        case Capability.HasCommACommBAndAirborne:   Assert.IsFalse(message.OnGround.Value); break;
                        case Capability.HasCommACommBAndOnGround:   Assert.IsTrue(message.OnGround.Value); break;
                        default:                                    Assert.IsNull(message.OnGround); break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_OnGround_Is_Extracted_From_ModeS_VerticalStatus()
        {
            _Translator.Translate(_NowUtc, _ModeSMessage, null);
            foreach(var modeSMessage in CreateModeSMessagesWithVerticalStatus()) {
                modeSMessage.Icao24 = _ModeSMessage.Icao24;
                foreach(VerticalStatus verticalStatus in Enum.GetValues(typeof(VerticalStatus))) {
                    modeSMessage.VerticalStatus = verticalStatus;

                    var message = _Translator.Translate(_NowUtc, modeSMessage, null);

                    switch(verticalStatus) {
                        case VerticalStatus.Airborne:   Assert.IsFalse(message.OnGround.Value); break;
                        case VerticalStatus.OnGround:   Assert.IsTrue(message.OnGround.Value); break;
                    }
                }
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_OnGround_Is_Inferred_From_ADSB_SurfacePosition_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.SurfacePosition = new SurfacePositionMessage();

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsTrue(message.OnGround.Value);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_OnGround_Is_Not_Inferred_From_ADSB_AirbornePosition_Messages()
        {
            // It's possible for aircraft to transmit airborne position messages while on the ground so we can't trust them
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.AirbornePosition = new AirbornePositionMessage();

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNull(message.OnGround);
            }
        }
        #endregion

        #region Translate - AircraftOperationalStatus
        [TestMethod]
        public void RawMessageTranslator_Translate_AircraftOperationalStatus_Generates_Supplementary_Message_When_ADSB_0_Message()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.AircraftOperationalStatus = new AircraftOperationalStatusMessage() {
                    AdsbVersion = 0,
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(TransponderType.Adsb0, message.Supplementary.TransponderType);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_AircraftOperationalStatus_Generates_Supplementary_Message_When_ADSB_1_Message()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.AircraftOperationalStatus = new AircraftOperationalStatusMessage() {
                    AdsbVersion = 1,
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(TransponderType.Adsb1, message.Supplementary.TransponderType);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_AircraftOperationalStatus_Generates_Supplementary_Message_When_ADSB_2_Message()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.AircraftOperationalStatus = new AircraftOperationalStatusMessage() {
                    AdsbVersion = 2,
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(TransponderType.Adsb2, message.Supplementary.TransponderType);
            }
        }
        #endregion

        #region Translate - TargetStateAndStatus
        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Generates_Supplementary_Message_When_ADSB_1_Message()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version1,
                    Version1 = new TargetStateAndStatusVersion1(),
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(TransponderType.Adsb1, message.Supplementary.TransponderType);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Generates_Supplementary_Message_When_ADSB_2_Message()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version2,
                    Version2 = new TargetStateAndStatusVersion2(),
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(TransponderType.Adsb2, message.Supplementary.TransponderType);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Fills_Target_Altitude_For_ADSB_1_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version1,
                    Version1 = new TargetStateAndStatusVersion1() {
                        TargetAltitude = 100
                    }
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(100, message.Supplementary.TargetAltitude);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Fills_Target_Altitude_For_ADSB_2_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version2,
                    Version2 = new TargetStateAndStatusVersion2() {
                        SelectedAltitude = 100
                    }
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(100, message.Supplementary.TargetAltitude);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Fills_Target_Heading_For_ADSB_1_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version1,
                    Version1 = new TargetStateAndStatusVersion1() {
                        TargetHeading = 100,
                    }
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(100F, message.Supplementary.TargetHeading);
            }
        }

        [TestMethod]
        public void RawMessageTranslator_Translate_TargetStateAndStatus_Fills_Target_Heading_For_ADSB_2_Messages()
        {
            foreach(var adsbMessage in CreateAdsbMessagesForExtendedSquitters()) {
                adsbMessage.TargetStateAndStatus = new TargetStateAndStatusMessage() {
                    TargetStateAndStatusType = TargetStateAndStatusType.Version2,
                    Version2 = new TargetStateAndStatusVersion2() {
                        SelectedHeading = 100.23,
                    }
                };

                var message = _Translator.Translate(_NowUtc, adsbMessage.ModeSMessage, adsbMessage);

                Assert.IsNotNull(message);
                Assert.IsNotNull(message.Supplementary);
                Assert.AreEqual(100.23F, message.Supplementary.TargetHeading);
            }
        }
        #endregion
    }
}
