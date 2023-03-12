// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Library.Adsb;

namespace Test.VirtualRadar.Library.Adsb
{
    [TestClass]
    public class AdsbTranslator_Tests
    {
        private IAdsbTranslator _Translator;
        private ModeSMessage _ModeSMessage;
        private static readonly VelocityType[] _ValidVelocityTypes = new VelocityType[] {
            VelocityType.AirspeedSubsonic,
            VelocityType.AirspeedSupersonic,
            VelocityType.GroundSpeedSubsonic,
            VelocityType.GroundSpeedSupersonic,
        };
        private ReceiverStatistics _Statistics;

        [TestInitialize]
        public void TestInitialise()
        {
            _Statistics = new ReceiverStatistics();
#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            _Translator = new AdsbTranslator();
#pragma warning restore CS0618
            _Translator.Statistics = _Statistics;
            _ModeSMessage = new ModeSMessage();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // We're just keeping this as a placeholder in case we need a cleanup in the future. There are
            // tests that manually call cleanup & initialise, don't want to have to update them all if a
            // cleanup becomes necessary.
        }

        /// <summary>
        /// Performs the very common task of sending a set of bits to the translator and getting a message back.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        private AdsbMessage Translate(string bits)
        {
            var bytes = TestDataParser.ConvertBitStringToBytes(bits);
            _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitter;
            _ModeSMessage.ExtendedSquitterMessage = bytes.ToArray();

            return _Translator.Translate(_ModeSMessage);
        }

        /// <summary>
        /// Performs the very common task of sending a set of bits to the translator and getting a message back.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        private AdsbMessage Translate(StringBuilder bits) => Translate(bits.ToString());

        //
        // TRANSLATE
        //
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Translate_Throws_If_No_Statistics_Are_Supplied()
        {
            _Translator.Statistics = null;
            _Translator.Translate(null);
        }

        [TestMethod]
        public void Translate_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Translator.Translate(null));
        }

        [TestMethod]
        public void Translate_Returns_Null_If_Passed_Message_With_No_Extended_Squitter()
        {
            _ModeSMessage.ExtendedSquitterMessage = null;
            Assert.IsNull(_Translator.Translate(_ModeSMessage));
        }

        [TestMethod]
        public void Translate_Returns_Null_If_Passed_Message_With_Extended_Squitter_Of_Incorrect_Length()
        {
            for(var i = 0;i < 50;++i) {
                if(i == 7) continue;
                _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitter;
                _ModeSMessage.ExtendedSquitterMessage = new byte[i];
                Assert.IsNull(_Translator.Translate(_ModeSMessage));
            }
        }

        [TestMethod]
        public void Translate_Decodes_Messages_Correctly()
        {
            // This is a monster test, I've ported it as-is from V2 but it really needs breaking up or doing away with it altogether

            var spreadsheet = new SpreadsheetTestData(TestData.RawDecodingTestData, "AdsbTranslate");
            spreadsheet.TestEveryRow(this, row => {
                var expectedValue = new SpreadsheetFieldValue(row, 17);

                var bits = row.String("ExtendedSquitterMessage");
                var bytes = TestDataParser.ConvertBitStringToBytes(bits);
                var df = row.String("DF");

                var countTestsPerformed = 0;
                for(var modeSDownlinkFormats = 0;modeSDownlinkFormats < 5;++modeSDownlinkFormats) {
                    var downlinkFormat = DownlinkFormat.ShortAirToAirSurveillance;
                    ControlField? controlField = null;
                    ApplicationField? applicationField = null;
                    switch(modeSDownlinkFormats) {
                        case 0: downlinkFormat = DownlinkFormat.ExtendedSquitter; break;
                        case 1: downlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder; controlField = ControlField.AdsbDeviceTransmittingIcao24; break;
                        case 2: downlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder; controlField = ControlField.AdsbDeviceNotTransmittingIcao24; break;
                        case 3: downlinkFormat = DownlinkFormat.ExtendedSquitterNonTransponder; controlField = ControlField.AdsbRebroadcastOfExtendedSquitter; break;
                        case 4: downlinkFormat = DownlinkFormat.MilitaryExtendedSquitter; applicationField = ApplicationField.ADSB; break;
                    }
                    if(df != "ALL") {
                        modeSDownlinkFormats = int.MaxValue - 1;

                        downlinkFormat = (DownlinkFormat)row.Int("DF");
                        var controlFieldValue = row.NInt("CF");
                        controlField = controlFieldValue == null ? (ControlField?)null : (ControlField)controlFieldValue.Value;
                        var applicationFieldValue = row.NInt("AF");
                        applicationField = applicationFieldValue == null ? (ApplicationField?)null : (ApplicationField)applicationFieldValue.Value;
                    }
                    ++countTestsPerformed;

                    TestCleanup();
                    TestInitialise();

                    _ModeSMessage.DownlinkFormat = downlinkFormat;
                    _ModeSMessage.ControlField = controlField;
                    _ModeSMessage.ApplicationField = applicationField;
                    _ModeSMessage.ExtendedSquitterMessage = bytes.ToArray();

                    var message = _Translator.Translate(_ModeSMessage);
                    var failMessage = $"Failed for DF:{(int)_ModeSMessage.DownlinkFormat} CF:{(int?)_ModeSMessage.ControlField} AF:{(int?)_ModeSMessage.ApplicationField}";

                    var countMessageObjects = 0;
                    object subMessage = null;
                    if(message.AirbornePosition != null)            { ++countMessageObjects; subMessage = message.AirbornePosition; }
                    if(message.SurfacePosition != null)             { ++countMessageObjects; subMessage = message.SurfacePosition; }
                    if(message.IdentifierAndCategory != null)       { ++countMessageObjects; subMessage = message.IdentifierAndCategory; }
                    if(message.AirborneVelocity != null)            { ++countMessageObjects; subMessage = message.AirborneVelocity; }
                    if(message.AircraftStatus != null)              { ++countMessageObjects; subMessage = message.AircraftStatus; }
                    if(message.TargetStateAndStatus != null)        { ++countMessageObjects; subMessage = message.TargetStateAndStatus; }
                    if(message.AircraftOperationalStatus != null)   { ++countMessageObjects; subMessage = message.AircraftOperationalStatus; }
                    if(message.CoarseTisbAirbornePosition != null)  { ++countMessageObjects; subMessage = message.CoarseTisbAirbornePosition; }
                    Assert.AreEqual(1, countMessageObjects, failMessage);

                    // Extract values that can appear on more than one message type
                    CompactPositionReportingCoordinate cpr = null;
                    bool? posTime = null;
                    EmergencyState? emergencyState = null;
                    byte? nacP = null;
                    bool? nicBaro = null;
                    byte? sil = null;
                    bool? silSupplement = null;
                    bool? isRebroadcast = null;
                    int? baroAlt = null;
                    double? gndSpeed = null;
                    double? gndTrack = null;
                    var surveillanceStatus = SurveillanceStatus.NoInformation;
                    if(message.AirbornePosition != null) {
                        cpr = message.AirbornePosition.CompactPosition;
                        posTime = message.AirbornePosition.PositionTimeIsExact;
                        baroAlt = message.AirbornePosition.BarometricAltitude;
                        surveillanceStatus = message.AirbornePosition.SurveillanceStatus;
                    } else if(message.SurfacePosition != null) {
                        cpr = message.SurfacePosition.CompactPosition;
                        posTime = message.SurfacePosition.PositionTimeIsExact;
                        gndSpeed = message.SurfacePosition.GroundSpeed;
                        gndTrack = message.SurfacePosition.GroundTrack;
                    } else if(message.AircraftStatus != null && message.AircraftStatus.EmergencyStatus != null) {
                        emergencyState = message.AircraftStatus.EmergencyStatus.EmergencyState;
                    } else if(message.TargetStateAndStatus != null && message.TargetStateAndStatus.Version1 != null) {
                        nacP = message.TargetStateAndStatus.Version1.NacP;
                        nicBaro = message.TargetStateAndStatus.Version1.NicBaro;
                        sil = message.TargetStateAndStatus.Version1.Sil;
                        emergencyState = message.TargetStateAndStatus.Version1.EmergencyState;
                    } else if(message.TargetStateAndStatus != null && message.TargetStateAndStatus.Version2 != null) {
                        nacP = message.TargetStateAndStatus.Version2.NacP;
                        nicBaro = message.TargetStateAndStatus.Version2.NicBaro;
                        sil = message.TargetStateAndStatus.Version2.Sil;
                        silSupplement = message.TargetStateAndStatus.Version2.SilSupplement;
                        isRebroadcast = message.TargetStateAndStatus.Version2.IsRebroadcast;
                    } else if(message.AircraftOperationalStatus != null) {
                        nacP = message.AircraftOperationalStatus.NacP;
                        nicBaro = message.AircraftOperationalStatus.NicBaro;
                        sil = message.AircraftOperationalStatus.Sil;
                        silSupplement = message.AircraftOperationalStatus.SilSupplement;
                        isRebroadcast = message.AircraftOperationalStatus.IsRebroadcast;
                    } else if(message.CoarseTisbAirbornePosition != null) {
                        cpr = message.CoarseTisbAirbornePosition.CompactPosition;
                        baroAlt = message.CoarseTisbAirbornePosition.BarometricAltitude;
                        gndSpeed = message.CoarseTisbAirbornePosition.GroundSpeed;
                        gndTrack = message.CoarseTisbAirbornePosition.GroundTrack;
                        surveillanceStatus = message.CoarseTisbAirbornePosition.SurveillanceStatus;
                    }

                    // Extract the full list of properties to check
                    var checkProperties = message.GetType().GetProperties().AsQueryable();
                    checkProperties = checkProperties.Concat(subMessage.GetType().GetProperties());
                    if(message.AircraftStatus != null) {
                        switch(message.AircraftStatus.AircraftStatusType) {
                            case AircraftStatusType.EmergencyStatus:
                                checkProperties = checkProperties.Concat(message.AircraftStatus.EmergencyStatus.GetType().GetProperties());
                                break;
                            case AircraftStatusType.TcasResolutionAdvisoryBroadcast:
                                checkProperties = checkProperties.Concat(message.AircraftStatus.TcasResolutionAdvisory.GetType().GetProperties());
                                break;
                        }
                    }
                    if(message.TargetStateAndStatus != null) {
                        switch(message.TargetStateAndStatus.TargetStateAndStatusType) {
                            case TargetStateAndStatusType.Version1: checkProperties = checkProperties.Concat(message.TargetStateAndStatus.Version1.GetType().GetProperties()); break;
                            case TargetStateAndStatusType.Version2: checkProperties = checkProperties.Concat(message.TargetStateAndStatus.Version2.GetType().GetProperties()); break;
                        }
                    }

                    Assert.IsNotNull(message, failMessage);
                    foreach(var messageProperty in checkProperties) {
                        switch(messageProperty.Name) {
                            case nameof(AdsbMessage.AirbornePosition):                  break;
                            case nameof(AdsbMessage.AirborneVelocity):                  break;
                            case nameof(AdsbMessage.AircraftOperationalStatus):         break;
                            case nameof(AdsbMessage.AircraftStatus):                    break;
                            case nameof(AdsbMessage.CoarseTisbAirbornePosition):        break;
                            case nameof(TcasResolutionAdvisory.FormattedThreatIcao24):  break;
                            case nameof(AdsbMessage.IdentifierAndCategory):             break;
                            case nameof(AdsbMessage.SurfacePosition):                   break;
                            case nameof(AdsbMessage.TargetStateAndStatus):              break;


                            case nameof(AdsbMessage.MessageFormat):
                                Assert.AreEqual(row.Enum<MessageFormat>("MessageFormat"), message.MessageFormat, failMessage);
                                break;
                            case nameof(AdsbMessage.ModeSMessage):
                                Assert.AreSame(_ModeSMessage, message.ModeSMessage, failMessage);
                                break;
                            case nameof(AdsbMessage.TisbIcaoModeAFlag):
                                Assert.AreEqual(row.NByte("IMF"), message.TisbIcaoModeAFlag, failMessage);
                                break;
                            case nameof(AdsbMessage.Type):
                                Assert.AreEqual(row.Byte("Type"), message.Type, failMessage);
                                break;


                            case nameof(AirbornePositionMessage.GeometricAltitude):
                                Assert.AreEqual(expectedValue.GetNInt("GA"), message.AirbornePosition.GeometricAltitude, failMessage);
                                break;
                            case nameof(AirbornePositionMessage.NicB):
                                Assert.AreEqual(expectedValue.GetNByte("NICB"), message.AirbornePosition.NicB, failMessage);
                                break;
                            case nameof(AirbornePositionMessage.PositionTimeIsExact):
                                Assert.AreEqual(expectedValue.GetNBool("TI"), posTime, failMessage);
                                break;
                            case nameof(AirbornePositionMessage.SurveillanceStatus):
                                Assert.AreEqual(expectedValue.GetEnum<SurveillanceStatus>("SS"), surveillanceStatus, failMessage);
                                break;


                            case nameof(AirborneVelocityMessage.Airspeed):
                                Assert.AreEqual(expectedValue.GetNDouble("AS"), message.AirborneVelocity.Airspeed, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.AirspeedExceeded):
                                Assert.AreEqual(expectedValue.GetNBool("AS:M"), message.AirborneVelocity.AirspeedExceeded, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.AirspeedIsTrueAirspeed):
                                Assert.AreEqual(expectedValue.GetNBool("AST"), message.AirborneVelocity.AirspeedIsTrueAirspeed, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.ChangeOfIntent):
                                Assert.AreEqual(expectedValue.GetNBool("IC"), message.AirborneVelocity.ChangeOfIntent, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.GeometricAltitudeDelta):
                                Assert.AreEqual(expectedValue.GetNShort("DBA"), message.AirborneVelocity.GeometricAltitudeDelta, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.GeometricAltitudeDeltaExceeded):
                                Assert.AreEqual(expectedValue.GetNBool("DBA:M"), message.AirborneVelocity.GeometricAltitudeDeltaExceeded, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.Heading):
                                Assert.AreEqual(expectedValue.GetNDouble("HDG"), message.AirborneVelocity.Heading, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.HorizontalVelocityError):
                                Assert.AreEqual(expectedValue.GetNDouble("NAC"), message.AirborneVelocity.HorizontalVelocityError, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.VectorVelocity):
                                Assert.AreEqual(expectedValue.GetString("VV"), message.AirborneVelocity.VectorVelocity == null ? null : message.AirborneVelocity.VectorVelocity.ToString(), failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.VelocityType):
                                Assert.AreEqual(expectedValue.GetEnum<VelocityType>("VT"), message.AirborneVelocity.VelocityType, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.VerticalRate):
                                Assert.AreEqual(expectedValue.GetNInt("VSI"), message.AirborneVelocity.VerticalRate, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.VerticalRateExceeded):
                                Assert.AreEqual(expectedValue.GetNBool("VSI:M"), message.AirborneVelocity.VerticalRateExceeded, failMessage);
                                break;
                            case nameof(AirborneVelocityMessage.VerticalRateIsBarometric):
                                Assert.AreEqual(expectedValue.GetNBool("SBV"), message.AirborneVelocity.VerticalRateIsBarometric, failMessage);
                                break;


                            case nameof(AircraftOperationalStatusMessage.AdsbVersion):
                                Assert.AreEqual(expectedValue.GetNByte("V"), message.AircraftOperationalStatus.AdsbVersion, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.AirborneCapability):
                                Assert.AreEqual(expectedValue.GetNInt("AC", true), (int?)message.AircraftOperationalStatus.AirborneCapability, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.AircraftOperationalStatusType):
                                Assert.AreEqual(expectedValue.GetEnum<AircraftOperationalStatusType>("AST"), message.AircraftOperationalStatus.AircraftOperationalStatusType, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.Gva):
                                Assert.AreEqual(expectedValue.GetNByte("GVA"), message.AircraftOperationalStatus.Gva, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.HorizontalReferenceIsMagneticNorth):
                                Assert.AreEqual(expectedValue.GetNBool("HRD"), message.AircraftOperationalStatus.HorizontalReferenceIsMagneticNorth, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.IsRebroadcast):
                                Assert.AreEqual(expectedValue.GetNBool("ADSR"), isRebroadcast, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.LateralAxisGpsOffset):
                                Assert.AreEqual(expectedValue.GetNShort("GLAT"), message.AircraftOperationalStatus.LateralAxisGpsOffset, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.LongitudinalAxisGpsOffset):
                                Assert.AreEqual(expectedValue.GetNByte("GLNG"), message.AircraftOperationalStatus.LongitudinalAxisGpsOffset, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.MaximumLength):
                                Assert.AreEqual(expectedValue.GetNDouble("MLN"), message.AircraftOperationalStatus.MaximumLength, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.MaximumWidth):
                                Assert.AreEqual(expectedValue.GetNDouble("MWD"), message.AircraftOperationalStatus.MaximumWidth, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.NacP):
                                Assert.AreEqual(expectedValue.GetNByte("NACP"), nacP, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.NicA):
                                Assert.AreEqual(expectedValue.GetNByte("NICA"), message.AircraftOperationalStatus.NicA, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.NicC):
                                Assert.AreEqual(expectedValue.GetNByte("NICC"), message.AircraftOperationalStatus.NicC, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.NicBaro):
                                Assert.AreEqual(expectedValue.GetNBool("NICBA"), nicBaro, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.OperationalMode):
                                Assert.AreEqual(expectedValue.GetNInt("OM", true), (int?)message.AircraftOperationalStatus.OperationalMode, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.Sil):
                                Assert.AreEqual(expectedValue.GetNByte("SIL"), sil, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.SilSupplement):
                                Assert.AreEqual(expectedValue.GetNBool("SILP"), silSupplement, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.SurfacePositionAngleIsTrack):
                                Assert.AreEqual(expectedValue.GetNBool("SPT"), message.AircraftOperationalStatus.SurfacePositionAngleIsTrack, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.SurfaceCapability):
                                Assert.AreEqual(expectedValue.GetNInt("SC", true), (int?)message.AircraftOperationalStatus.SurfaceCapability, failMessage);
                                break;
                            case nameof(AircraftOperationalStatusMessage.SystemDesignAssurance):
                                Assert.AreEqual(expectedValue.GetEnum<SystemDesignAssurance>("SDA"), message.AircraftOperationalStatus.SystemDesignAssurance, failMessage);
                                break;


                            case nameof(AircraftStatusMessage.AircraftStatusType):
                                Assert.AreEqual(expectedValue.GetEnum<AircraftStatusType>("AST"), message.AircraftStatus.AircraftStatusType, failMessage);
                                break;
                            case nameof(AircraftStatusMessage.EmergencyStatus):
                                if(message.AircraftStatus.AircraftStatusType != AircraftStatusType.EmergencyStatus) {
                                    Assert.IsNull(message.AircraftStatus.EmergencyStatus);
                                }
                                break;
                            case nameof(AircraftStatusMessage.TcasResolutionAdvisory):
                                if(message.AircraftStatus.AircraftStatusType != AircraftStatusType.TcasResolutionAdvisoryBroadcast) Assert.IsNull(message.AircraftStatus.TcasResolutionAdvisory);
                                break;


                            case nameof(CoarseTisbAirbornePosition.BarometricAltitude):
                                Assert.AreEqual(expectedValue.GetNInt("BA"), baroAlt, failMessage);
                                break;
                            case nameof(CoarseTisbAirbornePosition.CompactPosition):
                                Assert.AreEqual(expectedValue.GetString("CPR"), cpr == null ? null : cpr.ToString(), failMessage);
                                break;
                            case nameof(CoarseTisbAirbornePosition.GroundSpeed):
                                Assert.AreEqual(expectedValue.GetNDouble("GSPD"), gndSpeed, failMessage);
                                break;
                            case nameof(CoarseTisbAirbornePosition.GroundTrack):
                                Assert.AreEqual(expectedValue.GetNDouble("GTRK"), gndTrack, failMessage);
                                break;
                            case nameof(CoarseTisbAirbornePosition.ServiceVolumeID):
                                Assert.AreEqual(expectedValue.GetByte("SVID"), message.CoarseTisbAirbornePosition.ServiceVolumeID, failMessage);
                                break;


                            case nameof(EmergencyStatus.Squawk):
                                Assert.AreEqual(expectedValue.GetNShort("SQK"), message.AircraftStatus.EmergencyStatus.Squawk, failMessage);
                                break;


                            case nameof(IdentifierAndCategoryMessage.EmitterCategory):
                                Assert.AreEqual(GetExpectedEmitterCategory(expectedValue), message.IdentifierAndCategory.EmitterCategory, failMessage);
                                break;
                            case nameof(IdentifierAndCategoryMessage.Identification):
                                Assert.AreEqual(expectedValue.GetString("ID"), message.IdentifierAndCategory.Identification, failMessage);
                                break;


                            case nameof(SurfacePositionMessage.GroundSpeedExceeded):
                                Assert.AreEqual(expectedValue.GetNBool("GSPD:M"), message.SurfacePosition.GroundSpeedExceeded, failMessage);
                                break;
                            case nameof(SurfacePositionMessage.IsReversing):
                                Assert.AreEqual(expectedValue.GetNBool("REV"), message.SurfacePosition.IsReversing, failMessage);
                                break;


                            case nameof(TargetStateAndStatusVersion1.AltitudesAreMeanSeaLevel):
                                Assert.AreEqual(expectedValue.GetNBool("VMSL"), message.TargetStateAndStatus.Version1.AltitudesAreMeanSeaLevel, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.EmergencyState):
                                Assert.AreEqual(expectedValue.GetEnum<EmergencyState>("ES"), emergencyState, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.HorizontalDataSource):
                                Assert.AreEqual(expectedValue.GetEnum<HorizontalDataSource>("HDS"), message.TargetStateAndStatus.Version1.HorizontalDataSource, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.HorizontalModeIndicator):
                                Assert.AreEqual(expectedValue.GetEnum<HorizontalModeIndicator>("HMI"), message.TargetStateAndStatus.Version1.HorizontalModeIndicator, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.TargetAltitude):
                                Assert.AreEqual(expectedValue.GetNInt("ALT"), message.TargetStateAndStatus.Version1.TargetAltitude, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.TargetAltitudeCapability):
                                Assert.AreEqual(expectedValue.GetEnum<TargetAltitudeCapability>("TAC"), message.TargetStateAndStatus.Version1.TargetAltitudeCapability, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.TargetHeading):
                                Assert.AreEqual(expectedValue.GetNShort("HDG"), message.TargetStateAndStatus.Version1.TargetHeading, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.TargetHeadingIsTrack):
                                Assert.AreEqual(expectedValue.GetNBool("HDG-T"), message.TargetStateAndStatus.Version1.TargetHeadingIsTrack, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.TcasCapabilityMode):
                                Assert.AreEqual(expectedValue.GetEnum<TcasCapabilityMode>("TCC"), message.TargetStateAndStatus.Version1.TcasCapabilityMode, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.VerticalDataSource):
                                Assert.AreEqual(expectedValue.GetEnum<VerticalDataSource>("VDS"), message.TargetStateAndStatus.Version1.VerticalDataSource, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion1.VerticalModeIndicator):
                                Assert.AreEqual(expectedValue.GetEnum<VerticalModeIndicator>("VMI"), message.TargetStateAndStatus.Version1.VerticalModeIndicator, failMessage);
                                break;


                            case nameof(TargetStateAndStatusMessage.TargetStateAndStatusType):
                                Assert.AreEqual(expectedValue.GetEnum<TargetStateAndStatusType>("TST"), message.TargetStateAndStatus.TargetStateAndStatusType, failMessage);
                                break;
                            case nameof(TargetStateAndStatusMessage.Version1):
                                if(message.TargetStateAndStatus.TargetStateAndStatusType != TargetStateAndStatusType.Version1) Assert.IsNull(message.TargetStateAndStatus.Version1);
                                break;
                            case nameof(TargetStateAndStatusMessage.Version2):
                                if(message.TargetStateAndStatus.TargetStateAndStatusType != TargetStateAndStatusType.Version2) Assert.IsNull(message.TargetStateAndStatus.Version2);
                                break;


                            case nameof(TargetStateAndStatusVersion2.BarometricPressureSetting):
                                Assert.AreEqual(expectedValue.GetNDouble("QNH"), message.TargetStateAndStatus.Version2.BarometricPressureSetting, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsAltitudeHoldActive):
                                Assert.AreEqual(expectedValue.GetNBool("ALTH"), message.TargetStateAndStatus.Version2.IsAltitudeHoldActive, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsApproachModeActive):
                                Assert.AreEqual(expectedValue.GetNBool("APP"), message.TargetStateAndStatus.Version2.IsApproachModeActive, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsAutopilotEngaged):
                                Assert.AreEqual(expectedValue.GetNBool("APE"), message.TargetStateAndStatus.Version2.IsAutopilotEngaged, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsLnavEngaged):
                                Assert.AreEqual(expectedValue.GetNBool("LNAV"), message.TargetStateAndStatus.Version2.IsLnavEngaged, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsTcasOperational):
                                Assert.AreEqual(expectedValue.GetNBool("TCOP"), message.TargetStateAndStatus.Version2.IsTcasOperational, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.IsVnavEngaged):
                                Assert.AreEqual(expectedValue.GetNBool("VNAV"), message.TargetStateAndStatus.Version2.IsVnavEngaged, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.SelectedAltitude):
                                Assert.AreEqual(expectedValue.GetNInt("ALT"), message.TargetStateAndStatus.Version2.SelectedAltitude, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.SelectedAltitudeIsFms):
                                Assert.AreEqual(expectedValue.GetNBool("ALTF"), message.TargetStateAndStatus.Version2.SelectedAltitudeIsFms, failMessage);
                                break;
                            case nameof(TargetStateAndStatusVersion2.SelectedHeading):
                                Assert.AreEqual(expectedValue.GetNDouble("HDG"), message.TargetStateAndStatus.Version2.SelectedHeading, failMessage);
                                break;


                            case nameof(TcasResolutionAdvisory.MultipleThreatEncounter):
                                Assert.AreEqual(expectedValue.GetNBool("MTE"), message.AircraftStatus.TcasResolutionAdvisory.MultipleThreatEncounter, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.MultipleThreatResolutionAdvisory):
                                Assert.AreEqual(expectedValue.GetNShort("ARA-M", true), (short?)message.AircraftStatus.TcasResolutionAdvisory.MultipleThreatResolutionAdvisory, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ResolutionAdvisoryComplement):
                                Assert.AreEqual(expectedValue.GetNByte("RAC", true), (byte?)message.AircraftStatus.TcasResolutionAdvisory.ResolutionAdvisoryComplement, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ResolutionAdvisoryTerminated):
                                Assert.AreEqual(expectedValue.GetNBool("RAT"), message.AircraftStatus.TcasResolutionAdvisory.ResolutionAdvisoryTerminated, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.SingleThreatResolutionAdvisory):
                                Assert.AreEqual(expectedValue.GetNShort("ARA-S", true), (short?)message.AircraftStatus.TcasResolutionAdvisory.SingleThreatResolutionAdvisory, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ThreatAltitude):
                                Assert.AreEqual(expectedValue.GetNInt("TID-A"), message.AircraftStatus.TcasResolutionAdvisory.ThreatAltitude, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ThreatBearing):
                                Assert.AreEqual(expectedValue.GetNShort("TID-B"), message.AircraftStatus.TcasResolutionAdvisory.ThreatBearing, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ThreatIcao24):
                                Assert.AreEqual(expectedValue.GetNInt("TID", true), message.AircraftStatus.TcasResolutionAdvisory.ThreatIcao24, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ThreatRange):
                                Assert.AreEqual(expectedValue.GetNDouble("TID-R"), message.AircraftStatus.TcasResolutionAdvisory.ThreatRange, failMessage);
                                break;
                            case nameof(TcasResolutionAdvisory.ThreatRangeExceeded):
                                Assert.AreEqual(expectedValue.GetNBool("TID-R:M"), message.AircraftStatus.TcasResolutionAdvisory.ThreatRangeExceeded, failMessage);
                                break;


                            default:
                                Assert.Fail($"Code needs to be added to check the {messageProperty.Name} property");
                                break;
                        }
                    }
                }

                Assert.AreNotEqual(0, countTestsPerformed, $"DF{row.String("DF")}/CF{row.String("CF")}/AF{row.String("AF")} is not valid, no tests were performed");
            });
        }

        private EmitterCategory? GetExpectedEmitterCategory(SpreadsheetFieldValue expectedValue)
        {
            EmitterCategory? result = null;
            var expectedText = expectedValue.GetString("EC");
            if(expectedText != null) {
                byte value;
                if(byte.TryParse(expectedText, out value)) {
                    result = (EmitterCategory)value;
                } else {
                    result = (EmitterCategory)Enum.Parse(typeof(EmitterCategory), expectedText);
                }
            }

            return result;
        }

        [TestMethod]
        public void Translate_Updates_Statistics_When_Decoding_Messages()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.RawDecodingTestData, "AdsbTranslate");
            spreadsheet.TestEveryRow(this, row => {
                var bits = row.String("ExtendedSquitterMessage");
                _ModeSMessage.ExtendedSquitterMessage = TestDataParser.ConvertBitStringToBytes(bits).ToArray();
                _ModeSMessage.DownlinkFormat = DownlinkFormat.ExtendedSquitter;

                var message = _Translator.Translate(_ModeSMessage);

                if(message == null) {
                    Assert.AreEqual(0L, _Statistics.AdsbCount);
                    for(var i = 0;i < _Statistics.AdsbMessageFormatCount.Length;++i) {
                        Assert.AreEqual(0, _Statistics.AdsbMessageFormatCount[i], i.ToString());
                    }
                    for(var i = 0;i < _Statistics.AdsbTypeCount.Length;++i) {
                        Assert.AreEqual(0, _Statistics.AdsbTypeCount[i], i.ToString());
                    }
                } else {
                    Assert.AreEqual(1L, _Statistics.AdsbCount);
                    for(var i = 0;i < _Statistics.AdsbMessageFormatCount.Length;++i) {
                        Assert.AreEqual(i == (int)message.MessageFormat ? 1L : 0L, _Statistics.AdsbMessageFormatCount[i], i.ToString());
                    }
                    for(var i = 0;i < _Statistics.AdsbTypeCount.Length;++i) {
                        Assert.AreEqual(i == (int)message.Type ? 1L : 0L, _Statistics.AdsbTypeCount[i], i.ToString());
                    }
                }
            });
        }

        [TestMethod]
        public void Translate_Decodes_SurveillanceStatus_Correctly()
        {
            DoSurveillanceStatusCheck("00000", "0000000000000000000000000000000000000000000000000"); // Type 0
            DoSurveillanceStatusCheck("01001", "0111111111111001111111111111111101010101010101010"); // Type 9
            DoSurveillanceStatusCheck("01010", "0111111111111001111111111111111101010101010101010"); // Type 10
            DoSurveillanceStatusCheck("01011", "0111111111111001111111111111111101010101010101010"); // Type 11
            DoSurveillanceStatusCheck("01100", "0111111111111001111111111111111101010101010101010"); // Type 12
            DoSurveillanceStatusCheck("01101", "0111111111111001111111111111111101010101010101010"); // Type 13
            DoSurveillanceStatusCheck("01110", "0111111111111001111111111111111101010101010101010"); // Type 14
            DoSurveillanceStatusCheck("01111", "0111111111111001111111111111111101010101010101010"); // Type 15
            DoSurveillanceStatusCheck("10000", "0111111111111001111111111111111101010101010101010"); // Type 16
            DoSurveillanceStatusCheck("10001", "0111111111111001111111111111111101010101010101010"); // Type 17
            DoSurveillanceStatusCheck("10010", "0111111111111001111111111111111101010101010101010"); // Type 18
            DoSurveillanceStatusCheck("10100", "0111111111111001111111111111111101010101010101010"); // Type 20
            DoSurveillanceStatusCheck("10101", "0111111111111001111111111111111101010101010101010"); // Type 21
            DoSurveillanceStatusCheck("10110", "0111111111111001111111111111111101010101010101010"); // Type 22
        }

        private void DoSurveillanceStatusCheck(string bitsBeforeSS, string bitsAfterSS)
        {
            foreach(SurveillanceStatus surveillanceStatus in Enum.GetValues(typeof(SurveillanceStatus))) {
                var bits = new StringBuilder(bitsBeforeSS);
                bits.Append(TestDataParser.ConvertToBitString((int)surveillanceStatus, 2));
                bits.Append(bitsAfterSS);

                var message = Translate(bits);

                Assert.AreEqual(surveillanceStatus, message.AirbornePosition.SurveillanceStatus);
            }
        }

        [TestMethod]
        public void Translate_Decodes_Gillham_Altitudes_Correctly()
        {
            DoGillhamAltitudeDecodingCheck("00000000", "000000000000000000000000000000000000"); // Type 0
            DoGillhamAltitudeDecodingCheck("01001000", "000000000000000000000000000000000000"); // Type 9
            DoGillhamAltitudeDecodingCheck("01010000", "000000000000000000000000000000000000"); // Type 10
            DoGillhamAltitudeDecodingCheck("01011000", "000000000000000000000000000000000000"); // Type 11
            DoGillhamAltitudeDecodingCheck("01100000", "000000000000000000000000000000000000"); // Type 12
            DoGillhamAltitudeDecodingCheck("01101000", "000000000000000000000000000000000000"); // Type 13
            DoGillhamAltitudeDecodingCheck("01110000", "000000000000000000000000000000000000"); // Type 14
            DoGillhamAltitudeDecodingCheck("01111000", "000000000000000000000000000000000000"); // Type 15
            DoGillhamAltitudeDecodingCheck("10000000", "000000000000000000000000000000000000"); // Type 16
            DoGillhamAltitudeDecodingCheck("10001000", "000000000000000000000000000000000000"); // Type 17
            DoGillhamAltitudeDecodingCheck("10010000", "000000000000000000000000000000000000"); // Type 18
            DoGillhamAltitudeDecodingCheck("10100000", "000000000000000000000000000000000000"); // Type 20
            DoGillhamAltitudeDecodingCheck("10101000", "000000000000000000000000000000000000"); // Type 21
            DoGillhamAltitudeDecodingCheck("10110000", "000000000000000000000000000000000000"); // Type 22
        }

        private void DoGillhamAltitudeDecodingCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            var gillhamAltitudeTable = new SpreadsheetTestData(TestData.GillhamAltitudeTable, "AllAltitudes", hasHeadingRow: false);
            for(var rowNumber = 2;rowNumber < gillhamAltitudeTable.Rows.Count;++rowNumber) {
                var row = gillhamAltitudeTable.Rows[rowNumber];
                var altitude = row.Int(0);
                var bits = new StringBuilder(bitsBeforeAC);

                for(var bit = 0;bit < 12;++bit) {
                    if(bit == 7) {
                        bits.Append('0');
                    } else {
                        var index = bit + 1;
                        if(bit > 7) {
                            --index;
                        }
                        bits.Append(row.String(index));
                    }
                }

                bits.Append(bitsAfterAC);

                var message = Translate(bits);

                var actual = message.Type < 20
                    ? message.AirbornePosition.BarometricAltitude
                    : message.AirbornePosition.GeometricAltitude;
                Assert.AreEqual(altitude, actual);
            }
        }

        /*
        [TestMethod]
        public void Translate_Never_Throws_Exceptions_When_Decoding_Invalid_Gillham_Altitudes()
        {
            DoGillhamGracefulFailCheck("00000000", "000000000000000000000000000000000000"); // Type 0
            DoGillhamGracefulFailCheck("01001000", "000000000000000000000000000000000000"); // Type 9
            DoGillhamGracefulFailCheck("01010000", "000000000000000000000000000000000000"); // Type 10
            DoGillhamGracefulFailCheck("01011000", "000000000000000000000000000000000000"); // Type 11
            DoGillhamGracefulFailCheck("01100000", "000000000000000000000000000000000000"); // Type 12
            DoGillhamGracefulFailCheck("01101000", "000000000000000000000000000000000000"); // Type 13
            DoGillhamGracefulFailCheck("01110000", "000000000000000000000000000000000000"); // Type 14
            DoGillhamGracefulFailCheck("01111000", "000000000000000000000000000000000000"); // Type 15
            DoGillhamGracefulFailCheck("10000000", "000000000000000000000000000000000000"); // Type 16
            DoGillhamGracefulFailCheck("10001000", "000000000000000000000000000000000000"); // Type 17
            DoGillhamGracefulFailCheck("10010000", "000000000000000000000000000000000000"); // Type 18
            DoGillhamGracefulFailCheck("10100000", "000000000000000000000000000000000000"); // Type 20
            DoGillhamGracefulFailCheck("10101000", "000000000000000000000000000000000000"); // Type 21
            DoGillhamGracefulFailCheck("10110000", "000000000000000000000000000000000000"); // Type 22
        }

        private void DoGillhamGracefulFailCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            for(var acCode = 0;acCode < 2048;++acCode) {
                var bits = new StringBuilder(bitsBeforeAC);
                bits.Append(TestUtilities.ConvertToBitString(acCode, 11));
                bits.Insert(bitsBeforeAC.Length + 7, '0');
                bits.Append(bitsAfterAC);

                var message = Translate(bits);
            }
        }

        [TestMethod]
        public void Translate_Decodes_QBitOne_Altitudes_Correctly()
        {
            DoQBitOneAltitudeCheck("00000000", "000000000000000000000000000000000000"); // Type 0
            DoQBitOneAltitudeCheck("01001000", "000000000000000000000000000000000000"); // Type 9
            DoQBitOneAltitudeCheck("01010000", "000000000000000000000000000000000000"); // Type 10
            DoQBitOneAltitudeCheck("01011000", "000000000000000000000000000000000000"); // Type 11
            DoQBitOneAltitudeCheck("01100000", "000000000000000000000000000000000000"); // Type 12
            DoQBitOneAltitudeCheck("01101000", "000000000000000000000000000000000000"); // Type 13
            DoQBitOneAltitudeCheck("01110000", "000000000000000000000000000000000000"); // Type 14
            DoQBitOneAltitudeCheck("01111000", "000000000000000000000000000000000000"); // Type 15
            DoQBitOneAltitudeCheck("10000000", "000000000000000000000000000000000000"); // Type 16
            DoQBitOneAltitudeCheck("10001000", "000000000000000000000000000000000000"); // Type 17
            DoQBitOneAltitudeCheck("10010000", "000000000000000000000000000000000000"); // Type 18
            DoQBitOneAltitudeCheck("10100000", "000000000000000000000000000000000000"); // Type 20
            DoQBitOneAltitudeCheck("10101000", "000000000000000000000000000000000000"); // Type 21
            DoQBitOneAltitudeCheck("10110000", "000000000000000000000000000000000000"); // Type 22
        }

        private void DoQBitOneAltitudeCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            for(int altitude = -1000;altitude < 50200;altitude += 25) {
                var encodedAltitude = (altitude + 1000) / 25;
                var bits = new StringBuilder(bitsBeforeAC);
                var altitudeBits = TestUtilities.ConvertToBitString(encodedAltitude, 11);
                bits.AppendFormat("{0}1{1}", altitudeBits.Substring(0, 7), altitudeBits.Substring(7));
                bits.Append(bitsAfterAC);

                var message = Translate(bits);

                var actual = message.Type < 20 ? message.AirbornePosition.BarometricAltitude : message.AirbornePosition.GeometricAltitude;
                Assert.AreEqual(altitude, actual);
            }
        }

        [TestMethod]
        public void Translate_Decodes_Surface_Movements_Correctly()
        {
            DoSurfaceMovementCheck("00101", "10101010001111111111111111101010101010101010"); // Type 5
            DoSurfaceMovementCheck("00110", "10101010001111111111111111101010101010101010"); // Type 6
            DoSurfaceMovementCheck("00111", "10101010001111111111111111101010101010101010"); // Type 7
            DoSurfaceMovementCheck("01000", "10101010001111111111111111101010101010101010"); // Type 8
        }

        private void DoSurfaceMovementCheck(string bitsBefore, string bitsAfter)
        {
            for(var movement = 0;movement < 128;++movement) {
                var bits = new StringBuilder(bitsBefore);
                bits.Append(TestUtilities.ConvertToBitString(movement, 7));
                bits.Append(bitsAfter);
                var message = Translate(bits);

                Assert.AreEqual(movement == 124, message.SurfacePosition.GroundSpeedExceeded);
                Assert.AreEqual(movement == 127, message.SurfacePosition.IsReversing);

                if(movement == 0 || movement > 124) Assert.IsNull(message.SurfacePosition.GroundSpeed);
                else {
                    double expectedSpeed;
                    if(movement == 1) expectedSpeed = 0;
                    else if(movement == 2) expectedSpeed = 0.125;
                    else if(movement >= 3 && movement <= 8) expectedSpeed = 0.125 + ((movement - 2) * 0.145833315);
                    else if(movement >= 9 && movement <= 12) expectedSpeed = 1 + ((movement - 8) * 0.25);
                    else if(movement >= 13 && movement <= 38) expectedSpeed = 2 + ((movement - 12) * 0.5);
                    else if(movement >= 39 && movement <= 93) expectedSpeed = 15 + (movement - 38);
                    else if(movement >= 94 && movement <= 108) expectedSpeed = 70 + ((movement - 93) * 2.0);
                    else if(movement >= 109 && movement <= 123) expectedSpeed = 100 + ((movement - 108) * 5.0);
                    else expectedSpeed = 175.0;
                    Assert.AreEqual(expectedSpeed, message.SurfacePosition.GroundSpeed.Value, 0.00001, "Could not decode a movement of {0} correctly", movement);
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Surface_Track_Correctly()
        {
            DoSurfaceTrackCheck("001010000000", "001111111111111111101010101010101010"); // Type 5
            DoSurfaceTrackCheck("001100000000", "001111111111111111101010101010101010"); // Type 6
            DoSurfaceTrackCheck("001110000000", "001111111111111111101010101010101010"); // Type 7
            DoSurfaceTrackCheck("010000000000", "001111111111111111101010101010101010"); // Type 8
        }

        private void DoSurfaceTrackCheck(string bitsBefore, string bitsAfter)
        {
            foreach(var isValid in new bool[] { true, false }) {
                for(var track = 0;track < 128;++track) {
                    var bits = new StringBuilder(bitsBefore);
                    bits.Append(isValid ? '1' : '0');
                    bits.Append(TestUtilities.ConvertToBitString(track, 7));
                    bits.Append(bitsAfter);
                    var message = Translate(bits);

                    double? expectedTrack = null;
                    if(isValid) {
                        // Explicitly check the examples from the documentation, otherwise calculate the expected track
                        switch(track) {
                            case 0:     expectedTrack = 0.0; break;
                            case 1:     expectedTrack = 2.8125; break;
                            case 2:     expectedTrack = 5.6250; break;
                            case 3:     expectedTrack = 8.4375; break;
                            case 63:    expectedTrack = 177.1875; break;
                            case 64:    expectedTrack = 180.0; break;
                            case 65:    expectedTrack = 182.8125; break;
                            case 127:   expectedTrack = 357.1875; break;
                            default:    expectedTrack = track * 2.8125; break;
                        }
                    }

                    Assert.AreEqual(expectedTrack, message.SurfacePosition.GroundTrack, "Could not decode IsValid {0} / Track {1} correctly", isValid, track);
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Emitter_Categories_Correctly()
        {
            DoEmitterCategoryCheck("00001", "000001000010000011000100110000110001110010110011", new EmitterCategory[8] { 
                EmitterCategory.None,
                (EmitterCategory)131,
                (EmitterCategory)132,
                (EmitterCategory)133,
                (EmitterCategory)134,
                (EmitterCategory)135,
                (EmitterCategory)136,
                (EmitterCategory)137,
            });

            DoEmitterCategoryCheck("00010", "000001000010000011000100110000110001110010110011", new EmitterCategory[8] { 
                EmitterCategory.None,
                EmitterCategory.SurfaceEmergencyVehicle,
                EmitterCategory.SurfaceServiceVehicle,
                EmitterCategory.PointObstacle,
                EmitterCategory.ClusterObstacle,
                EmitterCategory.LineObstacle,
                (EmitterCategory)126,
                (EmitterCategory)127,
            });

            DoEmitterCategoryCheck("00011", "000001000010000011000100110000110001110010110011", new EmitterCategory[8] { 
                EmitterCategory.None,
                EmitterCategory.Glider,
                EmitterCategory.LighterThanAir,
                EmitterCategory.Parachutist,
                EmitterCategory.Ultralight,
                (EmitterCategory)115,
                EmitterCategory.UnmannedAerialVehicle,
                EmitterCategory.SpaceVehicle,
            });

            DoEmitterCategoryCheck("00100", "000001000010000011000100110000110001110010110011", new EmitterCategory[8] { 
                EmitterCategory.None,
                EmitterCategory.LightAircraft,
                EmitterCategory.SmallAircraft,
                EmitterCategory.LargeAircraft,
                EmitterCategory.HighVortexLargeAircraft,
                EmitterCategory.HeavyAircraft,
                EmitterCategory.HighPerformanceAircraft,
                EmitterCategory.Rotorcraft,
            });
        }

        private void DoEmitterCategoryCheck(string bitsBefore, string bitsAfter, EmitterCategory[] expectedCategories)
        {
            for(var category = 0;category < 8;++category) {
                var bits = new StringBuilder(bitsBefore);
                bits.Append(TestUtilities.ConvertToBitString(category, 3));
                bits.Append(bitsAfter);
                var message = Translate(bits);

                Assert.AreEqual(expectedCategories[category], message.IdentifierAndCategory.EmitterCategory);
            }
        }

        [TestMethod]
        public void Translate_Decodes_Identification_Correctly()
        {
            DoIdentificationCheck("00001 000");
            DoIdentificationCheck("00010 000");
            DoIdentificationCheck("00011 000");
            DoIdentificationCheck("00100 000");
        }

        private void DoIdentificationCheck(string bitsBefore)
        {
            foreach(var ch in "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789") {
                var expectedIdentification = new String(ch, 8);
                var encodedCh = ch >= 'A' ? (ch - 'A') + 1 : ch == ' ' ? 32 : 48 + (ch - '0');
                var bits = new StringBuilder(bitsBefore);
                for(var i = 0;i < 8;++i) {
                    bits.Append(TestUtilities.ConvertToBitString(encodedCh, 6));
                }

                var message = Translate(bits);

                Assert.AreEqual(expectedIdentification, message.IdentifierAndCategory.Identification, "Failed on character '{0}'", ch);
            }
        }

        [TestMethod]
        public void Translate_Handles_Unknown_Characters_In_Identification_Correctly()
        {
            DoUnknownCharacterIdentificationCheck("00001 000");
            DoUnknownCharacterIdentificationCheck("00010 000");
            DoUnknownCharacterIdentificationCheck("00011 000");
            DoUnknownCharacterIdentificationCheck("00100 000");
        }

        private void DoUnknownCharacterIdentificationCheck(string bitsBefore)
        {
            var undefinedCharacters = new List<int>();
            undefinedCharacters.Add(0);
            for(int i = 27;i < 32;undefinedCharacters.Add(i++)) ;
            for(int i = 33;i < 48;undefinedCharacters.Add(i++)) ;
            for(int i = 58;i < 64;undefinedCharacters.Add(i++)) ;

            foreach(var undefinedCharacter in undefinedCharacters) {
                // Encoded bits will be <undefined> + A + <undefined> + B + <undefined> + <undefined> + C + <undefined>
                var bits = new StringBuilder(bitsBefore);
                bits.AppendFormat("{0}000001{0}000010{0}{0}000011{0}", TestUtilities.ConvertToBitString(undefinedCharacter, 6));

                var message = Translate(bits);

                Assert.AreEqual("ABC", message.IdentifierAndCategory.Identification);
            }
        }

        [TestMethod]
        public void Translate_Handles_Unknown_AirborneVelocity_Subtypes_Correctly()
        {
            foreach(VelocityType subtype in Enum.GetValues(typeof(VelocityType))) {
                var expectDecode = _ValidVelocityTypes.Contains(subtype);
                var bits = String.Format("10011{0}101001011111111111010101010101111011110011010101", TestUtilities.ConvertToBitString((int)subtype, 3));
                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var velocity = Translate(bits).AirborneVelocity;

                Assert.AreEqual(subtype, velocity.VelocityType);
                if(expectDecode) {
                    Assert.IsTrue(velocity.VectorVelocity != null || velocity.Airspeed != null);
                    if(velocity.Airspeed != null) Assert.IsNotNull(velocity.Heading);
                    Assert.IsNotNull(velocity.HorizontalVelocityError);
                    Assert.IsNotNull(velocity.VerticalRate);
                    Assert.IsNotNull(velocity.GeometricAltitudeDelta);
                } else {
                    Assert.IsTrue(velocity.VectorVelocity == null && velocity.Airspeed == null && velocity.Heading == null);
                    Assert.IsNull(velocity.HorizontalVelocityError);
                    Assert.IsNull(velocity.VerticalRate);
                    Assert.IsNull(velocity.GeometricAltitudeDelta);
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Velocity_Change_Of_Intent_Correctly()
        {
            foreach(var velocityType in _ValidVelocityTypes) {
                foreach(var changeOfIntent in new bool[] { true, false }) {
                    var bits = String.Format("10011{0}{1}01001011111111111010101010101111011110011010101",
                                    TestUtilities.ConvertToBitString((int)velocityType, 3),
                                    TestUtilities.ConvertToBitString(changeOfIntent ? 1 : 0, 1));
                    var velocity = Translate(bits).AirborneVelocity;

                    Assert.AreEqual(changeOfIntent, velocity.ChangeOfIntent);
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Velocity_Horizontal_Margin_Of_Error_Correctly()
        {
            foreach(var velocityType in _ValidVelocityTypes) {
                for(var nac = 0;nac < 8;++nac) {
                    var bits = String.Format("10011{0}00{1}1011111111111010101010101111011110011010101",
                                    TestUtilities.ConvertToBitString((int)velocityType, 3),
                                    TestUtilities.ConvertToBitString(nac, 3));
                    var velocity = Translate(bits).AirborneVelocity;

                    switch(nac) {
                        case 0:     Assert.AreEqual(10.0F, velocity.HorizontalVelocityError); break;
                        case 1:     Assert.AreEqual(-10.0F, velocity.HorizontalVelocityError); break;
                        case 2:     Assert.AreEqual(-3.0F, velocity.HorizontalVelocityError); break;
                        case 3:     Assert.AreEqual(-1.0F, velocity.HorizontalVelocityError); break;
                        case 4:     Assert.AreEqual(-0.3F, velocity.HorizontalVelocityError); break;
                        default:    Assert.IsNull(velocity.HorizontalVelocityError); break;
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Vector_Velocities_Correctly()
        {
            foreach(var velocityType in new VelocityType[] { VelocityType.GroundSpeedSubsonic, VelocityType.GroundSpeedSupersonic }) {
                var isSupersonic = velocityType == VelocityType.GroundSpeedSupersonic;
                var velocityTypeBits = TestUtilities.ConvertToBitString((int)velocityType, 3);
                foreach(var isWesterly in new bool[] { true, false }) {
                    var isWesterlyBits = isWesterly ? '1' : '0';
                    foreach(var isSoutherly in new bool[] { true, false }) {
                        var isSoutherlyBits = isSoutherly ? '1' : '0';
                        for(var x = 0;x < 1024;++x) {
                            var xBits = TestUtilities.ConvertToBitString(x, 10);
                            for(var y = 0;y < 1024;++y) {
                                var bits = String.Format("10011{0}00100{1}{2}{3}{4}101111011110011010101",
                                                velocityTypeBits,
                                                isWesterlyBits,
                                                xBits,
                                                isSoutherlyBits,
                                                TestUtilities.ConvertToBitString(y, 10));
                                var vector = Translate(bits).AirborneVelocity.VectorVelocity;

                                Assert.AreEqual(isWesterly, vector.IsWesterlyVelocity);
                                Assert.AreEqual(isSoutherly, vector.IsSoutherlyVelocity);
                                Assert.AreEqual(x == 1023, vector.EastWestExceeded);
                                Assert.AreEqual(y == 1023, vector.NorthSouthExceeded);

                                // Assert explicit examples in the documentation, otherwise assert that the expected calculated value is correct
                                switch(x) {
                                    case 0:     Assert.IsNull(vector.EastWestVelocity); break;
                                    case 1:     Assert.AreEqual((short)0, vector.EastWestVelocity); break;
                                    case 2:     Assert.AreEqual((short)(isSupersonic ? 4 : 1), vector.EastWestVelocity); break;
                                    case 3:     Assert.AreEqual((short)(isSupersonic ? 8 : 2), vector.EastWestVelocity); break;
                                    case 1022:  Assert.AreEqual((short)(isSupersonic ? 4084 : 1021), vector.EastWestVelocity); break;
                                    default:    Assert.AreEqual((short)(isSupersonic ? (x - 1) * 4 : x - 1), vector.EastWestVelocity); break;
                                }

                                // Assert explicit examples in the documentation, otherwise assert that the expected calculated value is correct
                                switch(y) {
                                    case 0:     Assert.IsNull(vector.NorthSouthVelocity); break;
                                    case 1:     Assert.AreEqual((short)0, vector.NorthSouthVelocity); break;
                                    case 2:     Assert.AreEqual((short)(isSupersonic ? 4 : 1), vector.NorthSouthVelocity); break;
                                    case 3:     Assert.AreEqual((short)(isSupersonic ? 8 : 2), vector.NorthSouthVelocity); break;
                                    case 1022:  Assert.AreEqual((short)(isSupersonic ? 4084 : 1021), vector.NorthSouthVelocity); break;
                                    default:    Assert.AreEqual((short)(isSupersonic ? (y - 1) * 4 : y - 1), vector.NorthSouthVelocity); break;
                                }

                                if(y < 910 && y > 10) y += 100;
                            }

                            if(x < 910 && x > 10) x += 100;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Vertical_Rates_Correctly()
        {
            foreach(var velocityType in _ValidVelocityTypes) {
                var velocityTypeBits = TestUtilities.ConvertToBitString((int)velocityType, 3);
                foreach(var barometricSource in new bool[] { true, false }) {
                    var barometricBits = barometricSource ? '1' : '0';
                    foreach(var directionDown in new bool[] { true, false }) {
                        var directionBits = directionDown ? '1' : '0';
                        for(var verticalRate = 0;verticalRate < 512;++verticalRate) {
                            var bits = String.Format("10011{0}101001011111111111010101010{1}{2}{3}0011010101",
                                            velocityTypeBits,
                                            barometricBits,
                                            directionBits,
                                            TestUtilities.ConvertToBitString(verticalRate, 9));
                            var velocity = Translate(bits).AirborneVelocity;

                            Assert.AreEqual(barometricSource, velocity.VerticalRateIsBarometric);

                            // Assert explicit examples in the documentation, otherwise assert that the expected calculated value is correct
                            var sign = directionDown ? -1 : 1;
                            switch(verticalRate) {
                                case 0:     Assert.IsNull(velocity.VerticalRate); break;
                                case 1:     Assert.AreEqual(sign * 0, velocity.VerticalRate); break;
                                case 2:     Assert.AreEqual(sign * 64, velocity.VerticalRate); break;
                                case 3:     Assert.AreEqual(sign * 128, velocity.VerticalRate); break;
                                case 510:   Assert.AreEqual(sign * 32576, velocity.VerticalRate); break;
                                default:    Assert.AreEqual(sign * (verticalRate - 1) * 64, velocity.VerticalRate); break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Geometric_Altitude_Difference_Correctly()
        {
            foreach(var velocityType in _ValidVelocityTypes) {
                var velocityTypeBits = TestUtilities.ConvertToBitString((int)velocityType, 3);
                foreach(var altitudeBelowBarometric in new bool[] { true, false }) {
                    var altitudeBelowBarometricBits = altitudeBelowBarometric ? '1' : '0';
                    for(var delta = 0;delta < 128;++delta) {
                        var bits = String.Format("10011{0}1010010111111111110101010101011110111100{1}{2}",
                                        velocityTypeBits,
                                        altitudeBelowBarometricBits,
                                        TestUtilities.ConvertToBitString(delta, 7));
                        var velocity = Translate(bits).AirborneVelocity;

                        // Assert explicit examples in the documentation, otherwise assert that the expected calculated value is correct
                        var sign = altitudeBelowBarometric ? -1 : 1;
                        switch(delta) {
                            case 0:     Assert.IsNull(velocity.GeometricAltitudeDelta); break;
                            case 1:     Assert.AreEqual((short)0, velocity.GeometricAltitudeDelta); break;
                            case 2:     Assert.AreEqual((short)(sign * 25), velocity.GeometricAltitudeDelta); break;
                            case 3:     Assert.AreEqual((short)(sign * 50), velocity.GeometricAltitudeDelta); break;
                            case 126:   Assert.AreEqual((short)(sign * 3125), velocity.GeometricAltitudeDelta); break;
                            default:    Assert.AreEqual((short)(sign * (delta - 1) * 25), velocity.GeometricAltitudeDelta); break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Ignores_Unknown_AircraftStatus_Subtypes()
        {
            for(var subType = 0;subType < 8;++subType) {
                var bits = String.Format("11100{0}100001100000000000000000000000000000000000000000", TestUtilities.ConvertToBitString(subType, 3));
                var message = Translate(bits);

                Assert.AreEqual(subType, (int)message.AircraftStatus.AircraftStatusType);

                switch(subType) {
                    case 1:
                        Assert.IsNotNull(message.AircraftStatus.EmergencyStatus);
                        Assert.IsNull(message.AircraftStatus.TcasResolutionAdvisory);
                        break;
                    case 2:
                        Assert.IsNull(message.AircraftStatus.EmergencyStatus);
                        Assert.IsNotNull(message.AircraftStatus.TcasResolutionAdvisory);
                        break;
                    default:
                        Assert.IsNull(message.AircraftStatus.EmergencyStatus);
                        Assert.IsNull(message.AircraftStatus.TcasResolutionAdvisory);
                        break;
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Emergency_States_Correctly()
        {
            for(var emergencyState = 0;emergencyState < 8;++emergencyState) {
                var bits = String.Format("11100001{0}001100000000000000000000000000000000000000000", TestUtilities.ConvertToBitString(emergencyState, 3));
                var message = Translate(bits);

                Assert.AreEqual(emergencyState, (int)message.AircraftStatus.EmergencyStatus.EmergencyState);
            }
        }

        [TestMethod]
        public void Translate_Decodes_Mode_A_Codes_Correctly()
        {
            for(int a = 0;a < 8;++a) {
                for(int b = 0;b < 8;++b) {
                    for(int c = 0;c < 8;++c) {
                        for(int d = 0;d < 8;++d) {
                            short expectedIdentity = (short)((a * 1000) + (b * 100) + (c * 10) + d);

                            var aBits = TestUtilities.ConvertToBitString(a, 3);
                            var bBits = TestUtilities.ConvertToBitString(b, 3);
                            var cBits = TestUtilities.ConvertToBitString(c, 3);
                            var dBits = TestUtilities.ConvertToBitString(d, 3);
                            var bits = String.Format("{0}{1}{2}{3}{4}{5}0{6}{7}{8}{9}{10}{11}",
                                cBits[2], aBits[2], cBits[1], aBits[1], cBits[0], aBits[0],
                                bBits[2], dBits[2], bBits[1], dBits[1], bBits[0], dBits[0]);
                            bits = String.Format("11100 001 001 {0} 00000000 00000000 00000000 00000000", bits);

                            var message = Translate(bits);

                            if(expectedIdentity == 0) Assert.IsNull(message.AircraftStatus.EmergencyStatus.Squawk);
                            else Assert.AreEqual(expectedIdentity, message.AircraftStatus.EmergencyStatus.Squawk);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_TCAS_AR_ARAs_Correctly()
        {
            for(var ara0 = 0;ara0 < 2;++ara0) {
                for(var mte = 0;mte < 2;++mte) {
                    for(var araBit = 0;araBit < 13;++araBit) {
                        var expectedAdvisory = (int)Math.Pow(2, 12 - araBit);
                        var bits = String.Format("11100 010 {0}{1} 0000 0 {2} 00 00 00000000 00000000 00000000",
                                        ara0,
                                        String.Format("{0}1{1}", new String('0', araBit), new String('0', 12 - araBit)),
                                        mte);

                        var tcas = Translate(bits).AircraftStatus.TcasResolutionAdvisory;

                        Assert.AreEqual(mte == 1, tcas.MultipleThreatEncounter);

                        if(ara0 == 0 && mte == 0) {
                            Assert.IsNull(tcas.SingleThreatResolutionAdvisory);
                            Assert.IsNull(tcas.MultipleThreatResolutionAdvisory);
                        } else if(ara0 == 0 && mte == 1) {
                            Assert.IsNull(tcas.SingleThreatResolutionAdvisory);
                            Assert.AreEqual(expectedAdvisory, (int)tcas.MultipleThreatResolutionAdvisory);
                        } else if(ara0 == 1) {
                            Assert.AreEqual(expectedAdvisory, (int)tcas.SingleThreatResolutionAdvisory);
                            Assert.IsNull(tcas.MultipleThreatResolutionAdvisory);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_TCAS_Threat_Altitude_Correctly()
        {
            // Note that the TCAS message includes the D1 bit (presumably always zero) when encoding altitudes.
            // The Mode-S Gillham encodings don't include the D1 bit, so if the implementation is sharing decoding
            // of Gillham altitudes with Mode-S code it will have to strip the D1 bit out.
            var fileName = Path.Combine(TestContext.TestDeploymentDir, "GillhamAltitudeTable.csv");
            var lines = File.ReadAllLines(fileName);
            for(var lineNumber = 2;lineNumber < lines.Length;++lineNumber) {
                var cells = lines[lineNumber].Split(',');
                var altitude = int.Parse(cells[0]);
                var bits = new StringBuilder("11100 010 10000000000000 0000 0 1 10");

                for(int bit = 0;bit < 13;++bit) {
                    if(bit == 6 || bit == 8) bits.Append('0');
                    else {
                        var index = bit + 1;
                        if(bit > 6) --index;
                        if(bit > 8) --index;
                        bits.Append(cells[index]);
                    }
                }

                bits.Append("0000000000000");

                var tcas = Translate(bits).AircraftStatus.TcasResolutionAdvisory;

                Assert.AreEqual(altitude, tcas.ThreatAltitude);
            }
        }

        [TestMethod]
        public void Translate_Decodes_TCAS_Threat_Range_Correctly()
        {
            for(var rangeValue = 0;rangeValue < 128;++rangeValue) {
                var bits = String.Format("11100 010 10000000000000 0000 0 1 10 0000000000000 {0} 000000", TestUtilities.ConvertToBitString(rangeValue, 7));
                var tcas = Translate(bits).AircraftStatus.TcasResolutionAdvisory;

                Assert.AreEqual(rangeValue == 127, tcas.ThreatRangeExceeded);
                switch(rangeValue) {
                    case 0:     Assert.IsNull(tcas.ThreatRange); break;
                    case 1:     Assert.AreEqual(0.05F, tcas.ThreatRange); break;
                    case 126:   Assert.AreEqual(12.55F, tcas.ThreatRange); break;
                    default:    Assert.AreEqual((float)((rangeValue - 1) / 10F) + 0.05F, tcas.ThreatRange.Value, 0.001F); break; 
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_TCAS_Threat_Bearing_Correctly()
        {
            for(var bearingValue = 0;bearingValue < 64;++bearingValue) {
                var bits = String.Format("11100 010 10000000000000 0000 0 1 10 0000000000000 0000000 {0}", TestUtilities.ConvertToBitString(bearingValue, 6));
                var tcas = Translate(bits).AircraftStatus.TcasResolutionAdvisory;

                switch(bearingValue) {
                    case 0:
                    case 61:
                    case 62:
                    case 63:    Assert.IsNull(tcas.ThreatBearing); break;
                    case 1:     Assert.AreEqual((short)0, tcas.ThreatBearing); break;
                    case 59:    Assert.AreEqual((short)348, tcas.ThreatBearing); break;
                    case 60:    Assert.AreEqual((short)354, tcas.ThreatBearing); break;
                    default:    Assert.AreEqual((short)((bearingValue - 1) * 6), tcas.ThreatBearing); break;
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Unknown_Target_State_And_Status_Message_Formats_Correctly()
        {
            foreach(var backwardsCompatibleBit in new byte[] { 0, 1 }) {
                for(var subType = 0;subType < 4;++subType) {
                    var bits = String.Format("11101 {0} 0 00{1}00000 00000000 00000000 00000000 00000000 00000000", TestUtilities.ConvertToBitString(subType, 2), backwardsCompatibleBit);
                    var targetState = Translate(bits).TargetStateAndStatus;

                    Assert.AreEqual(subType, (int)targetState.TargetStateAndStatusType);

                    switch(subType) {
                        case 0:
                            if(backwardsCompatibleBit == 1) Assert.IsNull(targetState.Version1);
                            else                            Assert.IsNotNull(targetState.Version1);
                            Assert.IsNull(targetState.Version2);
                            break;
                        case 1:
                            Assert.IsNull(targetState.Version1);
                            Assert.IsNotNull(targetState.Version2);
                            break;
                        default:
                            Assert.IsNull(targetState.Version1);
                            Assert.IsNull(targetState.Version2);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_1_Target_Altitude_Correctly()
        {
            for(var verticalDataSource = 0;verticalDataSource < 4;++verticalDataSource) {
                for(var targetAltitudeType = 0;targetAltitudeType < 2;++targetAltitudeType) {
                    for(var targetAltitudeCapability = 0;targetAltitudeCapability < 4;++targetAltitudeCapability) {
                        for(var verticalModeIndicator = 0;verticalModeIndicator < 4;++verticalModeIndicator) {
                            for(var targetAltitudeValue = 0;targetAltitudeValue < 1024;++targetAltitudeValue) {
                                var bits = String.Format("11101 00 {0} {1} 0 {2} {3} {4} 0000000 00000000 00000000 00000000",
                                                TestUtilities.ConvertToBitString(verticalDataSource, 2),
                                                TestUtilities.ConvertToBitString(targetAltitudeType, 1),
                                                TestUtilities.ConvertToBitString(targetAltitudeCapability, 2),
                                                TestUtilities.ConvertToBitString(verticalModeIndicator, 2),
                                                TestUtilities.ConvertToBitString(targetAltitudeValue, 10));
                                var version1 = Translate(bits).TargetStateAndStatus.Version1;

                                Assert.AreEqual(verticalDataSource, (int)version1.VerticalDataSource);
                                Assert.AreEqual(targetAltitudeType == 1, version1.AltitudesAreMeanSeaLevel);
                                Assert.AreEqual(targetAltitudeCapability, (int)version1.TargetAltitudeCapability);

                                if(verticalDataSource == 0 || targetAltitudeValue > 1010) Assert.IsNull(version1.TargetAltitude);
                                else {
                                    switch(targetAltitudeValue) {
                                        case 0:     Assert.AreEqual(-1000, version1.TargetAltitude); break; // as per example table
                                        case 1:     Assert.AreEqual(-900, version1.TargetAltitude); break;  // as per example table
                                        case 2:     Assert.AreEqual(-800, version1.TargetAltitude); break;  // as per example table
                                        case 10:    Assert.AreEqual(0, version1.TargetAltitude); break;     // the table specifies that 11 is an altitude of 0 but it doesn't fit in with the other values
                                        case 11:    Assert.AreEqual(100, version1.TargetAltitude); break;   // see comment above, 12 is meant to be 100 but that doesn't fit in
                                        case 1010:  Assert.AreEqual(100000, version1.TargetAltitude); break;// as per example table
                                        default:    Assert.AreEqual((targetAltitudeValue * 100) - 1000, version1.TargetAltitude); break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_1_Target_Heading_Correctly()
        {
            for(var horizontalDataSource = 0;horizontalDataSource < 4;++horizontalDataSource) {
                for(var headingIsTrack = 0;headingIsTrack < 2;++headingIsTrack) {
                    for(var horizontalModeIndicator = 0; horizontalModeIndicator < 4;++horizontalModeIndicator) {
                        for(var headingValue = 0;headingValue < 512;++headingValue) {
                            var bits = String.Format("11101 000 00000000 00000000 0 {0} {1} {2} {3} 0 00000000 00000000",
                                        TestUtilities.ConvertToBitString(horizontalDataSource, 2),
                                        TestUtilities.ConvertToBitString(headingValue, 9),
                                        TestUtilities.ConvertToBitString(headingIsTrack, 1),
                                        TestUtilities.ConvertToBitString(horizontalModeIndicator, 2));
                            var version1 = Translate(bits).TargetStateAndStatus.Version1;

                            Assert.AreEqual(horizontalDataSource, (int)version1.HorizontalDataSource);
                            Assert.AreEqual(headingIsTrack == 1, version1.TargetHeadingIsTrack);
                            Assert.AreEqual(horizontalModeIndicator, (int)version1.HorizontalModeIndicator);

                            if(horizontalDataSource == 0 || headingValue > 359) Assert.IsNull(version1.TargetHeading);
                            else {
                                switch(headingValue) {
                                    case 0:     Assert.AreEqual((short)0, version1.TargetHeading); break;
                                    case 1:     Assert.AreEqual((short)1, version1.TargetHeading); break;
                                    case 2:     Assert.AreEqual((short)2, version1.TargetHeading); break;
                                    case 359:   Assert.AreEqual((short)359, version1.TargetHeading); break;
                                    default:    Assert.AreEqual((short)headingValue, version1.TargetHeading); break;
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_2_Selected_Altitude()
        {
            for(var altitudeType = 0;altitudeType < 2;++altitudeType) {
                for(var altitudeValue = 0;altitudeValue < 2048;++altitudeValue) {
                    var bits = String.Format("11101 01 0 {0} {1} 0000 00000000 00000000 00000000 00000000",
                                TestUtilities.ConvertToBitString(altitudeType, 1),
                                TestUtilities.ConvertToBitString(altitudeValue, 11));
                    var version2 = Translate(bits).TargetStateAndStatus.Version2;

                    Assert.AreEqual(altitudeType == 1, version2.SelectedAltitudeIsFms);
                    switch(altitudeValue) {
                        case 0:     Assert.IsNull(version2.SelectedAltitude); break;
                        case 1:     Assert.AreEqual(0, version2.SelectedAltitude); break;
                        case 2:     Assert.AreEqual(32, version2.SelectedAltitude); break;
                        case 3:     Assert.AreEqual(64, version2.SelectedAltitude); break;
                        case 2046:  Assert.AreEqual(65440, version2.SelectedAltitude); break;
                        case 2047:  Assert.AreEqual(65472, version2.SelectedAltitude); break;
                        default:    Assert.AreEqual((altitudeValue - 1) * 32, version2.SelectedAltitude); break;
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_2_Barometric_Pressure_Setting_Correctly()
        {
            for(var settingValue = 0;settingValue < 512;++settingValue) {
                var bits = String.Format("11101 01 0 0 00000000000 {0} 000 00000000 00000000 00000000", TestUtilities.ConvertToBitString(settingValue, 9));
                var version2 = Translate(bits).TargetStateAndStatus.Version2;

                switch(settingValue) {
                    case 0:     Assert.IsNull(version2.BarometricPressureSetting); break;
                    case 1:     Assert.AreEqual(800.0F, version2.BarometricPressureSetting); break;
                    case 2:     Assert.AreEqual(800.8F, version2.BarometricPressureSetting); break;
                    case 3:     Assert.AreEqual(801.6F, version2.BarometricPressureSetting); break;
                    case 510:   Assert.AreEqual(1207.2F, version2.BarometricPressureSetting); break;
                    case 511:   Assert.AreEqual(1208.0F, version2.BarometricPressureSetting); break;
                    default:    Assert.AreEqual((((float)settingValue - 1F) * 0.8F) + 800F, version2.BarometricPressureSetting); break;
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_2_Selected_Heading_Correctly()
        {
            for(var headingValid = 0;headingValid < 2;++headingValid) {
                for(var headingSign = 0;headingSign < 2;++headingSign) {
                    for(var headingValue = 0;headingValue < 256;++headingValue) {
                        var bits = String.Format("11101 01 0 0 00000000000 000000000 {0} {1} {2} 0 00000000 00000000",
                                        TestUtilities.ConvertToBitString(headingValid, 1),
                                        TestUtilities.ConvertToBitString(headingSign, 1),
                                        TestUtilities.ConvertToBitString(headingValue, 8));
                        var version2 = Translate(bits).TargetStateAndStatus.Version2;

                        if(headingValid == 0) Assert.IsNull(version2.SelectedHeading);
                        else {
                            var signedHeading = (headingSign << 8) | headingValue;
                            switch(signedHeading) {
                                case 0:     Assert.AreEqual(0.0, version2.SelectedHeading); break;
                                case 1:     Assert.AreEqual(0.703125, version2.SelectedHeading); break;
                                case 2:     Assert.AreEqual(1.406250, version2.SelectedHeading); break;
                                case 255:   Assert.AreEqual(179.296875, version2.SelectedHeading); break;
                                case 256:   Assert.AreEqual(180.0, version2.SelectedHeading); break;
                                case 257:   Assert.AreEqual(180.703125, version2.SelectedHeading); break;
                                case 258:   Assert.AreEqual(181.406250, version2.SelectedHeading); break;
                                case 384:   Assert.AreEqual(270.0, version2.SelectedHeading); break;
                                case 385:   Assert.AreEqual(270.703125, version2.SelectedHeading); break;
                                case 386:   Assert.AreEqual(271.406250, version2.SelectedHeading); break;
                                case 510:   Assert.AreEqual(358.593750, version2.SelectedHeading); break;
                                case 511:   Assert.AreEqual(359.296875, version2.SelectedHeading); break;
                                default:    Assert.AreEqual((double)signedHeading * 0.703125, version2.SelectedHeading); break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Decodes_Target_State_Version_2_Autopilot_State_Flags_Correctly()
        {
            for(var valid = 0;valid < 2;++valid) {
                for(var autopilot = 0;autopilot < 2;++autopilot) {
                    for(var vnav = 0;vnav < 2;++vnav) {
                        for(var altitudeHold = 0;altitudeHold < 2;++altitudeHold) {
                            for(var adsr = 0;adsr < 2;++adsr) {
                                for(var approach = 0;approach < 2;++approach) {
                                    for(var tcas = 0;tcas < 2;++tcas) {
                                        for(var lnav = 0;lnav < 2;++lnav) {
                                            var bits = String.Format("11101 01 0 00000000 00000000 00000000 00000000 000000 {0} {1} {2} {3} {4} {5} {6} {7} 00",
                                                valid,
                                                autopilot,
                                                vnav,
                                                altitudeHold,
                                                adsr,
                                                approach,
                                                tcas,
                                                lnav);
                                            var version2 = Translate(bits).TargetStateAndStatus.Version2;

                                            Assert.AreEqual(adsr == 1, version2.IsRebroadcast);
                                            Assert.AreEqual(tcas == 1, version2.IsTcasOperational);

                                            if(valid == 0) {
                                                Assert.IsNull(version2.IsAutopilotEngaged);
                                                Assert.IsNull(version2.IsVnavEngaged);
                                                Assert.IsNull(version2.IsAltitudeHoldActive);
                                                Assert.IsNull(version2.IsApproachModeActive);
                                                Assert.IsNull(version2.IsLnavEngaged);
                                            } else {
                                                Assert.AreEqual(autopilot == 1, version2.IsAutopilotEngaged);
                                                Assert.AreEqual(vnav == 1, version2.IsVnavEngaged);
                                                Assert.AreEqual(altitudeHold == 1, version2.IsAltitudeHoldActive);
                                                Assert.AreEqual(approach == 1, version2.IsApproachModeActive);
                                                Assert.AreEqual(lnav == 1, version2.IsLnavEngaged);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Ignores_Unknown_Versions_And_Types_Of_Aircraft_Operational_Status_Messages()
        {
            for(var version = 0;version < 7;++version) {
                for(var type = 0;type < 7;++type) {
                    var bits = String.Format("11111 {0} 00000000 00000000 00000000 00000000 {1} 00000 00000000",
                        TestUtilities.ConvertToBitString(type, 3),
                        TestUtilities.ConvertToBitString(version, 3));
                    var message = Translate(bits).AircraftOperationalStatus;

                    Assert.AreEqual(version, message.AdsbVersion);
                    Assert.AreEqual(type, (int)message.AircraftOperationalStatusType);

                    if((version == 1 || version == 2) && type < 2) {
                        Assert.AreEqual(type == 0 ? (ushort?)0 : (ushort?)null, (ushort?)message.AirborneCapability);
                        Assert.AreEqual(type == 1 ? 0 : (int?)null, (int?)message.SurfaceCapability);
                        Assert.AreEqual(type == 1 && version == 2 ? (byte?)0 : (byte?)null, message.NicC);
                        Assert.IsNotNull(message.OperationalMode);
                        Assert.IsNotNull(message.NicA);
                        Assert.AreEqual((byte)0, message.NacP);
                        Assert.AreEqual(type == 0 && version == 2? (byte?)0 : (byte?)null, message.Gva);
                        Assert.AreEqual((byte)0, message.Sil);
                        Assert.AreEqual(type == 0 ? (bool?)false : (bool?)null, message.NicBaro);
                        Assert.AreEqual(type == 1 ? (bool?)false : (bool?)null, message.SurfacePositionAngleIsTrack);
                        Assert.AreEqual(false, message.HorizontalReferenceIsMagneticNorth);
                        Assert.AreEqual(version == 1 ? (bool?)null : false, message.SilSupplement);
                        Assert.AreEqual(version == 1 ? (bool?)null : false, message.IsRebroadcast);
                        Assert.AreEqual(version == 1 ? (SystemDesignAssurance?)null : SystemDesignAssurance.None, message.SystemDesignAssurance);
                    } else {
                        Assert.AreEqual(version == 0 && type == 0 ? (ushort?)0 : (ushort?)null, message.AirborneCapability);
                        Assert.IsNull(message.SurfaceCapability);
                        Assert.IsNull(message.MaximumLength);
                        Assert.IsNull(message.MaximumWidth);
                        Assert.IsNull(message.OperationalMode);
                        Assert.IsNull(message.SystemDesignAssurance);
                        Assert.IsNull(message.LateralAxisGpsOffset);
                        Assert.IsNull(message.LongitudinalAxisGpsOffset);
                        Assert.IsNull(message.NicA);
                        Assert.IsNull(message.NicC);
                        Assert.IsNull(message.NacP);
                        Assert.IsNull(message.Gva);
                        Assert.IsNull(message.Sil);
                        Assert.IsNull(message.SilSupplement);
                        Assert.IsNull(message.NicBaro);
                        Assert.IsNull(message.SurfacePositionAngleIsTrack);
                        Assert.IsNull(message.HorizontalReferenceIsMagneticNorth);
                        Assert.IsNull(message.IsRebroadcast);
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Extracts_Maximum_Length_And_Width_In_Aircraft_Operational_Status_Messages_Correctly()
        {
            for(var version = 0;version < 7;++version) {
                for(var type = 0;type < 7;++type) {
                    for(var dimensionValue = 0;dimensionValue < 16;++dimensionValue) {
                        var bits = String.Format("11111 {0} 000000000000 {1} 00000000 00000000 {2} 00000 00000000",
                                        TestUtilities.ConvertToBitString(type, 3),
                                        TestUtilities.ConvertToBitString(dimensionValue, 4),
                                        TestUtilities.ConvertToBitString(version, 3));
                        var message = Translate(bits).AircraftOperationalStatus;

                        if(version < 1 || version > 2 || type != 1) {
                            Assert.IsNull(message.MaximumLength);
                            Assert.IsNull(message.MaximumWidth);
                        } else {
                            switch(dimensionValue) {
                                case 0:
                                    if(version == 2) {
                                        Assert.IsNull(message.MaximumLength);
                                        Assert.IsNull(message.MaximumWidth);
                                    } else {
                                        Assert.AreEqual(15F, message.MaximumLength);
                                        Assert.AreEqual(11.5F, message.MaximumWidth);
                                    }
                                    break;
                                case 1:     Assert.AreEqual(15F, message.MaximumLength); Assert.AreEqual(23F, message.MaximumWidth); break;
                                case 2:     Assert.AreEqual(25F, message.MaximumLength); Assert.AreEqual(28.5F, message.MaximumWidth); break;
                                case 3:     Assert.AreEqual(25F, message.MaximumLength); Assert.AreEqual(34F, message.MaximumWidth); break;
                                case 4:     Assert.AreEqual(35F, message.MaximumLength); Assert.AreEqual(33F, message.MaximumWidth); break;
                                case 5:     Assert.AreEqual(35F, message.MaximumLength); Assert.AreEqual(38F, message.MaximumWidth); break;
                                case 6:     Assert.AreEqual(45F, message.MaximumLength); Assert.AreEqual(39.5F, message.MaximumWidth); break;
                                case 7:     Assert.AreEqual(45F, message.MaximumLength); Assert.AreEqual(45F, message.MaximumWidth); break;
                                case 8:     Assert.AreEqual(55F, message.MaximumLength); Assert.AreEqual(45F, message.MaximumWidth); break;
                                case 9:     Assert.AreEqual(55F, message.MaximumLength); Assert.AreEqual(52F, message.MaximumWidth); break;
                                case 10:    Assert.AreEqual(65F, message.MaximumLength); Assert.AreEqual(59.5F, message.MaximumWidth); break;
                                case 11:    Assert.AreEqual(65F, message.MaximumLength); Assert.AreEqual(67F, message.MaximumWidth); break;
                                case 12:    Assert.AreEqual(75F, message.MaximumLength); Assert.AreEqual(72.5F, message.MaximumWidth); break;
                                case 13:    Assert.AreEqual(75F, message.MaximumLength); Assert.AreEqual(80F, message.MaximumWidth); break;
                                case 14:    Assert.AreEqual(85F, message.MaximumLength); Assert.AreEqual(80F, message.MaximumWidth); break;
                                case 15:    Assert.AreEqual(85F, message.MaximumLength); Assert.AreEqual(90F, message.MaximumWidth); break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Translate_Extracts_SDA_And_GPS_Antenna_Offset_In_Aircraft_Operational_Status_Messages_Correctly()
        {
            for(var version = 0;version < 8;++version) {
                for(var type = 0;type < 8;++type) {
                    for(var sda = 0;sda < 4;++sda) {
                        for(var gpsLeftRight = 0;gpsLeftRight < 2;++gpsLeftRight) {
                            for(var gpsLateralOffset = 0;gpsLateralOffset < 4;++gpsLateralOffset) {
                                for(var gpsLongitudinalOffset = 0;gpsLongitudinalOffset < 32;++gpsLongitudinalOffset) {
                                    var bits = String.Format("11111 {0} 00000000 00000000 000000 {1} {2}{3}{4} {5} 00000 00000000",
                                            TestUtilities.ConvertToBitString(type, 3),
                                            TestUtilities.ConvertToBitString(sda, 2),
                                            TestUtilities.ConvertToBitString(gpsLeftRight, 1),
                                            TestUtilities.ConvertToBitString(gpsLateralOffset, 2),
                                            TestUtilities.ConvertToBitString(gpsLongitudinalOffset, 5),
                                            TestUtilities.ConvertToBitString(version, 3));
                                    var message = Translate(bits).AircraftOperationalStatus;

                                    if(version == 2 && type < 2) Assert.AreEqual((SystemDesignAssurance)sda, message.SystemDesignAssurance);
                                    else                         Assert.IsNull(message.SystemDesignAssurance);

                                    if(version != 2 || type != 1) {
                                        Assert.IsNull(message.LateralAxisGpsOffset);
                                        Assert.IsNull(message.LongitudinalAxisGpsOffset);
                                    } else {
                                        switch(gpsLateralOffset) {
                                            case 0:     Assert.AreEqual(gpsLeftRight == 0 ? (short?)null : (short?)0, message.LateralAxisGpsOffset); break;
                                            case 1:     Assert.AreEqual(gpsLeftRight == 0 ? (short)2 : (short)-2, message.LateralAxisGpsOffset); break;
                                            case 2:     Assert.AreEqual(gpsLeftRight == 0 ? (short)4 : (short)-4, message.LateralAxisGpsOffset); break;
                                            case 3:     Assert.AreEqual(gpsLeftRight == 0 ? (short)6 : (short)-6, message.LateralAxisGpsOffset); break;
                                        }
                                        switch(gpsLongitudinalOffset) {
                                            case 0:     Assert.IsNull(message.LongitudinalAxisGpsOffset); break;
                                            case 1:     Assert.IsNull(message.LongitudinalAxisGpsOffset); break; // position offset applied by sensor - presumably POA bit in capability will be set for this?
                                            case 2:     Assert.AreEqual((byte)2, message.LongitudinalAxisGpsOffset); break;
                                            case 3:     Assert.AreEqual((byte)4, message.LongitudinalAxisGpsOffset); break;
                                            case 4:     Assert.AreEqual((byte)6, message.LongitudinalAxisGpsOffset); break;
                                            case 31:    Assert.AreEqual((byte)60, message.LongitudinalAxisGpsOffset); break;
                                            default:    Assert.AreEqual((byte)((gpsLongitudinalOffset - 1) * 2), message.LongitudinalAxisGpsOffset); break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        */
    }
}
