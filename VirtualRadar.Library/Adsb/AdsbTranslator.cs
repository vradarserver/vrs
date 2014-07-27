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
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface;
using InterfaceFactory;
using VirtualRadar.Library.ModeS;

namespace VirtualRadar.Library.Adsb
{
    /// <summary>
    /// The default implementation of <see cref="IAdsbTranslator"/>.
    /// </summary>
    class AdsbTranslator : IAdsbTranslator
    {
        #region Fields
        /// <summary>
        /// The object that can stream bits out of an array of bytes for us.
        /// </summary>
        private IBitStream _BitStream;

        /// <summary>
        /// A lookup table of emitter categories.
        /// </summary>
        private static EmitterCategory[,] _EmitterCategories = new EmitterCategory[,] {
            { EmitterCategory.None, (EmitterCategory)131, (EmitterCategory)132, (EmitterCategory)133, (EmitterCategory)134, (EmitterCategory)135, (EmitterCategory)136, (EmitterCategory)137 },
            { EmitterCategory.None, EmitterCategory.SurfaceEmergencyVehicle, EmitterCategory.SurfaceServiceVehicle, EmitterCategory.PointObstacle, EmitterCategory.ClusterObstacle, EmitterCategory.LineObstacle, (EmitterCategory)126, (EmitterCategory)127 },
            { EmitterCategory.None, EmitterCategory.Glider, EmitterCategory.LighterThanAir, EmitterCategory.Parachutist, EmitterCategory.Ultralight, (EmitterCategory)115, EmitterCategory.UnmannedAerialVehicle, EmitterCategory.SpaceVehicle },
            { EmitterCategory.None, EmitterCategory.LightAircraft, EmitterCategory.SmallAircraft, EmitterCategory.LargeAircraft, EmitterCategory.HighVortexLargeAircraft, EmitterCategory.HeavyAircraft, EmitterCategory.HighPerformanceAircraft, EmitterCategory.Rotorcraft },
        };

        /// <summary>
        /// The lookup table of surface ground speeds.
        /// </summary>
        private static double[] _SurfaceGroundSpeed = new double[125];

        /// <summary>
        /// The lookup table of maximum lengths.
        /// </summary>
        private static readonly float[] _MaximumLengths = new float[] { 15F, 15F, 25F, 25F, 35F, 35F, 45F, 45F, 55F, 55F, 65F, 65F, 75F, 75F, 85F, 85F, };

        /// <summary>
        /// The lookup table of maximum widths.
        /// </summary>
        private static readonly float[] _MaximumWidths = new float[] { 11.5F, 23F, 28.5F, 34F, 33F, 38F, 39.5F, 45F, 45F, 52F, 59.5F, 67F, 72.5F, 80F, 80F, 90F, };
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStatistics Statistics { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AdsbTranslator()
        {
            _BitStream = Factory.Singleton.Resolve<IBitStream>();
        }

        /// <summary>
        /// The implicitly public static constructor.
        /// </summary>
        static AdsbTranslator()
        {
            BuildSurfaceGroundSpeedLookupTable();
        }

        /// <summary>
        /// Constructs a lookup table of surface ground speeds.
        /// </summary>
        private static void BuildSurfaceGroundSpeedLookupTable()
        {
            _SurfaceGroundSpeed[1] = 0.0;
            FillSurfaceGroundSpeed(2, 2, 0.0, 0.125);
            FillSurfaceGroundSpeed(3, 8, 0.125, 1.0);
            FillSurfaceGroundSpeed(9, 12, 1.0, 2.0);
            FillSurfaceGroundSpeed(13, 38, 2.0, 15.0);
            FillSurfaceGroundSpeed(39, 93, 15.0, 70.0);
            FillSurfaceGroundSpeed(94, 108, 70.0, 100.0);
            FillSurfaceGroundSpeed(109, 123, 100.0, 175.0);
            _SurfaceGroundSpeed[124] = 175.0;
        }

        /// <summary>
        /// Takes a representation of the surface position movement decode table, calculates the appropriate values and copies them into the surface ground speed lookup table.
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="endRange"></param>
        /// <param name="lowerSpeed"></param>
        /// <param name="upperSpeed"></param>
        private static void FillSurfaceGroundSpeed(int startRange, int endRange, double lowerSpeed, double upperSpeed)
        {
            var speedIncrement = (upperSpeed - lowerSpeed) / ((endRange - startRange) + 1);
            var speed = lowerSpeed;
            for(var i = startRange;i <= endRange;++i) {
                speed += speedIncrement;
                _SurfaceGroundSpeed[i] = speed;
            }
        }
        #endregion

        #region Translate
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <returns></returns>
        public AdsbMessage Translate(ModeSMessage modeSMessage)
        {
            if(Statistics == null) throw new InvalidOperationException("Statistics must be supplied before Translate can be called");

            AdsbMessage result = null;
            if(modeSMessage != null && modeSMessage.ExtendedSquitterMessage != null && modeSMessage.ExtendedSquitterMessage.Length == 7) {
                _BitStream.Initialise(modeSMessage.ExtendedSquitterMessage);

                result = new AdsbMessage(modeSMessage);
                result.Type = _BitStream.ReadByte(5);

                switch(result.Type) {
                    case 0:     DecodeAirbornePosition(result); break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:     DecodeIdentification(result); break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:     DecodeSurfacePosition(result); break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:    DecodeAirbornePosition(result); break;
                    case 19:    DecodeVelocity(result); break;
                    case 20:
                    case 21:
                    case 22:    DecodeAirbornePosition(result); break;
                    case 28:    DecodeAircraftStatus(result); break;
                    case 29:    DecodeTargetStateAndStatus(result); break;
                    case 31:    DecodeAircraftOperationalStatus(result); break;
                }

                if(Statistics != null) {
                    Statistics.Lock(r => {
                        ++r.AdsbCount;
                        ++r.AdsbMessageFormatCount[(int)result.MessageFormat];
                        ++r.AdsbTypeCount[result.Type];
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Decodes an identification and emitter category message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeIdentification(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.IdentificationAndCategory;
            var subMessage = message.IdentifierAndCategory = new IdentifierAndCategoryMessage();

            subMessage.EmitterCategory = _EmitterCategories[message.Type - 1, _BitStream.ReadByte(3)];
            subMessage.Identification = ModeSCharacterTranslator.ExtractCharacters(_BitStream, 8);
        }

        /// <summary>
        /// Decodes a surface position message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeSurfacePosition(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.SurfacePosition;
            var subMessage = message.SurfacePosition = new SurfacePositionMessage();

            var movement = _BitStream.ReadByte(7);
            if(movement > 0) {
                if(movement < 124) subMessage.GroundSpeed = _SurfaceGroundSpeed[movement];
                else if(movement == 124) {
                    subMessage.GroundSpeed = 175.0;
                    subMessage.GroundSpeedExceeded = true;
                } else if(movement == 127) {
                    subMessage.IsReversing = true;
                }
            }

            var trackIsValid = _BitStream.ReadBit();
            var track = _BitStream.ReadByte(7);
            if(trackIsValid) subMessage.GroundTrack = track * 2.8125;

            subMessage.PositionTimeIsExact = _BitStream.ReadBit();
            subMessage.CompactPosition = ExtractCprCoordinate(message, 19);
        }

        /// <summary>
        /// Decodes an airborne position message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeAirbornePosition(AdsbMessage message)
        {
            message.MessageFormat = message.Type == 0 ? MessageFormat.NoPositionInformation : MessageFormat.AirbornePosition;
            var subMessage = message.AirbornePosition = new AirbornePositionMessage();

            subMessage.SurveillanceStatus = (SurveillanceStatus)_BitStream.ReadByte(2);
            if(message.Type == 0) _BitStream.Skip(1);
            else subMessage.NicB = (byte)(_BitStream.ReadBit() ? 2 : 0);
            var rawAltitude = _BitStream.ReadUInt16(12);

            var acCode = ((rawAltitude & 0xfe0) >> 1) | (rawAltitude & 0x0f);
            int? altitude = null;
            if((rawAltitude & 0x10) != 0) altitude = ModeSAltitudeConversion.CalculateBinaryAltitude(acCode);
            else altitude= ModeSAltitudeConversion.LookupGillhamAltitude(acCode);
            if(message.Type < 20) subMessage.BarometricAltitude = altitude;
            else subMessage.GeometricAltitude = altitude;

            subMessage.PositionTimeIsExact = _BitStream.ReadBit();
            subMessage.CompactPosition = ExtractCprCoordinate(message, 17);
        }

        /// <summary>
        /// Decodes an airborne velocity message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeVelocity(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.AirborneVelocity;
            var subMessage = message.AirborneVelocity = new AirborneVelocityMessage();

            var velocityType = _BitStream.ReadByte(3);
            subMessage.VelocityType = (VelocityType)velocityType;
            if(velocityType > 0 && velocityType < 5) {
                subMessage.ChangeOfIntent = _BitStream.ReadBit();
                _BitStream.Skip(1);

                switch(_BitStream.ReadByte(3)) {
                    case 0: subMessage.HorizontalVelocityError = 10F; break;
                    case 1: subMessage.HorizontalVelocityError = -10F; break;
                    case 2: subMessage.HorizontalVelocityError = -3F; break;
                    case 3: subMessage.HorizontalVelocityError = -1F; break;
                    case 4: subMessage.HorizontalVelocityError = -0.3F; break;
                }

                if(velocityType == 3 || velocityType == 4) {
                    var headingAvailable = _BitStream.ReadBit();
                    var heading = _BitStream.ReadUInt16(10);
                    if(headingAvailable) subMessage.Heading = (double)heading * 0.3515625;
                    subMessage.AirspeedIsTrueAirspeed = _BitStream.ReadBit();
                    var airspeed = _BitStream.ReadUInt16(10);
                    if(airspeed != 0) {
                        if(airspeed == 1023) subMessage.AirspeedExceeded = true;
                        subMessage.Airspeed = airspeed - 1;
                        if(velocityType == 4) subMessage.Airspeed *= 4;
                    }
                } else {
                    var vector = subMessage.VectorVelocity = new VectorVelocity() {
                        IsWesterlyVelocity = _BitStream.ReadBit(),
                        EastWestVelocity = (short?)_BitStream.ReadUInt16(10),
                        IsSoutherlyVelocity = _BitStream.ReadBit(),
                        NorthSouthVelocity = (short?)_BitStream.ReadUInt16(10),
                    };
                    if(vector.EastWestVelocity == 0) vector.EastWestVelocity = null;
                    else {
                        vector.EastWestExceeded = vector.EastWestVelocity == 1023;
                        vector.EastWestVelocity = (short)((vector.EastWestVelocity - 1) * (velocityType == 1 ? 1 : 4));
                    }
                    if(vector.NorthSouthVelocity == 0) vector.NorthSouthVelocity = null;
                    else {
                        vector.NorthSouthExceeded = vector.NorthSouthVelocity == 1023;
                        vector.NorthSouthVelocity = (short)((vector.NorthSouthVelocity - 1) * (velocityType == 1 ? 1 : 4));
                    }
                }

                subMessage.VerticalRateIsBarometric = _BitStream.ReadBit();
                var verticalRateSign = _BitStream.ReadBit() ? -1 : 1;
                var verticalRate = _BitStream.ReadUInt16(9);
                if(verticalRate != 0) {
                    subMessage.VerticalRateExceeded = verticalRateSign == 511;
                    subMessage.VerticalRate = verticalRateSign * ((verticalRate - 1) * 64);
                }

                _BitStream.Skip(2);
                var geometricDeltaSign = _BitStream.ReadBit() ? -1 : 1;
                var geometricDelta = _BitStream.ReadByte(7);
                if(geometricDelta != 0) {
                    subMessage.GeometricAltitudeDeltaExceeded = geometricDelta == 127;
                    subMessage.GeometricAltitudeDelta = (short)(geometricDeltaSign * ((geometricDelta - 1) * 25));
                }
            }
        }

        /// <summary>
        /// Decodes an aircraft status message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeAircraftStatus(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.AircraftStatus;
            var subMessage = message.AircraftStatus = new AircraftStatusMessage();

            subMessage.AircraftStatusType = (AircraftStatusType)_BitStream.ReadByte(3);

            switch(subMessage.AircraftStatusType) {
                case AircraftStatusType.EmergencyStatus:
                    var emergency = subMessage.EmergencyStatus = new EmergencyStatus();
                    emergency.EmergencyState = (EmergencyState)_BitStream.ReadByte(3);
                    var modeA = (short)_BitStream.ReadUInt16(13);
                    if(modeA != 0) emergency.Squawk = ModeS.ModeATranslator.DecodeModeA(modeA);
                    break;
                case AircraftStatusType.TcasResolutionAdvisoryBroadcast:
                    var tcas = subMessage.TcasResolutionAdvisory = new TcasResolutionAdvisory();
                    var araCoding = _BitStream.ReadBit();
                    var araValue = (short)_BitStream.ReadUInt16(13);
                    tcas.ResolutionAdvisoryComplement = (ResolutionAdvisoryComplement)_BitStream.ReadByte(4);
                    tcas.ResolutionAdvisoryTerminated = _BitStream.ReadBit();
                    tcas.MultipleThreatEncounter = _BitStream.ReadBit();
                    if(araCoding) tcas.SingleThreatResolutionAdvisory = (SingleThreatResolutionAdvisory)araValue;
                    else if(tcas.MultipleThreatEncounter) tcas.MultipleThreatResolutionAdvisory = (MultipleThreatResolutionAdvisory)araValue;

                    switch(_BitStream.ReadByte(2)) {
                        case 1:
                            tcas.ThreatIcao24 = (int)_BitStream.ReadUInt32(24);
                            break;
                        case 2:
                            var threatAltitude = _BitStream.ReadUInt16(13);
                            var threatRange = _BitStream.ReadByte(7);
                            var threatBearing = _BitStream.ReadByte(6);

                            var strippedAltitude = ((threatAltitude & 0x1F80) >> 2) | ((threatAltitude & 0x20) >> 1) | (threatAltitude & 0x0f);
                            tcas.ThreatAltitude = ModeSAltitudeConversion.LookupGillhamAltitude(strippedAltitude);

                            if(threatRange > 0) {
                                tcas.ThreatRange = ((float)(threatRange - 1) / 10F) + 0.05F;
                                tcas.ThreatRangeExceeded = threatRange == 127;
                            }

                            if(threatBearing > 0 && threatBearing < 61) tcas.ThreatBearing = (short)((threatBearing - 1) * 6);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Decodes the target state and status message.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeTargetStateAndStatus(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.TargetStateAndStatus;
            var subMessage = message.TargetStateAndStatus = new TargetStateAndStatusMessage();

            subMessage.TargetStateAndStatusType = (TargetStateAndStatusType)_BitStream.ReadByte(2);

            switch(subMessage.TargetStateAndStatusType) {
                case TargetStateAndStatusType.Version1:
                    var verticalDataSource = (VerticalDataSource)_BitStream.ReadByte(2);
                    var altitudeIsMeanSeaLevel = _BitStream.ReadBit();
                    if(!_BitStream.ReadBit()) { // backwards compatibility bit - if this is non-zero then it's a version 0 TCP message that we need to ignore
                        var version1 = subMessage.Version1 = new TargetStateAndStatusVersion1();
                        version1.VerticalDataSource = verticalDataSource;
                        version1.AltitudesAreMeanSeaLevel = altitudeIsMeanSeaLevel;
                        version1.TargetAltitudeCapability = (TargetAltitudeCapability)_BitStream.ReadByte(2);
                        version1.VerticalModeIndicator = (VerticalModeIndicator)_BitStream.ReadByte(2);
                        var altitude = (int)_BitStream.ReadUInt16(10);
                        if(version1.VerticalDataSource != VerticalDataSource.None && altitude < 1011) version1.TargetAltitude = (altitude * 100) - 1000;
                        version1.HorizontalDataSource = (HorizontalDataSource)_BitStream.ReadByte(2);
                        var heading = _BitStream.ReadUInt16(9);
                        if(version1.HorizontalDataSource != HorizontalDataSource.None && heading < 360) version1.TargetHeading = (short)heading;
                        version1.TargetHeadingIsTrack = _BitStream.ReadBit();
                        version1.HorizontalModeIndicator = (HorizontalModeIndicator)_BitStream.ReadByte(2);
                        version1.NacP = _BitStream.ReadByte(4);
                        version1.NicBaro = _BitStream.ReadBit();
                        version1.Sil = _BitStream.ReadByte(2);
                        _BitStream.Skip(5);
                        version1.TcasCapabilityMode = (TcasCapabilityMode)_BitStream.ReadByte(2);
                        version1.EmergencyState = (EmergencyState)_BitStream.ReadByte(3);
                    }
                    break;
                case TargetStateAndStatusType.Version2:
                    var version2 = message.TargetStateAndStatus.Version2 = new TargetStateAndStatusVersion2();
                    version2.SilSupplement = _BitStream.ReadBit();
                    version2.SelectedAltitudeIsFms = _BitStream.ReadBit();
                    var selAltitude = _BitStream.ReadUInt16(11);
                    if(selAltitude != 0) version2.SelectedAltitude = (selAltitude - 1) * 32;
                    var pressure = _BitStream.ReadUInt16(9);
                    if(pressure != 0) version2.BarometricPressureSetting = (((float)pressure - 1F) * 0.8F) + 800F;
                    bool headingIsValid = _BitStream.ReadBit();
                    var headingValue = _BitStream.ReadUInt16(9);
                    if(headingIsValid) version2.SelectedHeading = (double)headingValue * 0.703125;
                    version2.NacP = _BitStream.ReadByte(4);
                    version2.NicBaro = _BitStream.ReadBit();
                    version2.Sil = _BitStream.ReadByte(2);
                    var autopilotValid = _BitStream.ReadBit();
                    version2.IsAutopilotEngaged = _BitStream.ReadBit();
                    version2.IsVnavEngaged = _BitStream.ReadBit();
                    version2.IsAltitudeHoldActive = _BitStream.ReadBit();
                    version2.IsRebroadcast = _BitStream.ReadBit();
                    version2.IsApproachModeActive = _BitStream.ReadBit();
                    version2.IsTcasOperational = _BitStream.ReadBit();
                    version2.IsLnavEngaged = _BitStream.ReadBit();
                    if(!autopilotValid) {
                        version2.IsAutopilotEngaged = version2.IsVnavEngaged = version2.IsAltitudeHoldActive = version2.IsApproachModeActive = version2.IsLnavEngaged = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// Decodes the content of aircraft operational status messages.
        /// </summary>
        /// <param name="message"></param>
        private void DecodeAircraftOperationalStatus(AdsbMessage message)
        {
            message.MessageFormat = MessageFormat.AircraftOperationalStatus;
            var subMessage = message.AircraftOperationalStatus = new AircraftOperationalStatusMessage();

            var subType = _BitStream.ReadByte(3);
            subMessage.AircraftOperationalStatusType = (AircraftOperationalStatusType)subType;
            bool isSurfaceCapability = subMessage.AircraftOperationalStatusType == AircraftOperationalStatusType.SurfaceCapability;
            var cc = _BitStream.ReadUInt16(isSurfaceCapability ? 12 : 16);
            var len = isSurfaceCapability ? _BitStream.ReadByte(4) : (byte)0;
            var om = _BitStream.ReadUInt16(16);
            var version = subMessage.AdsbVersion = _BitStream.ReadByte(3);

            if(version < 3 && subType < 2) {
                switch(subType) {
                    case 0: subMessage.AirborneCapability = cc; break;
                    case 1:
                        if(version > 0) {
                            subMessage.SurfaceCapability = (SurfaceCapability)cc;
                            if(version != 0 && (version != 2 || len != 0)) {
                                subMessage.MaximumLength = _MaximumLengths[len];
                                subMessage.MaximumWidth = _MaximumWidths[len];
                            }
                        }
                        break;
                }
                if(version > 0) {
                    subMessage.OperationalMode = (OperationalMode)om;
                    if(version == 2) {
                        subMessage.SystemDesignAssurance = (SystemDesignAssurance)((om & 0x300) >> 8);
                        if(subType == 1) {
                            var gpsRight = (om & 0x80);
                            var gpsLat = (om & 0x60) >> 5;
                            var gpsLon = om & 0x1f;
                            if(gpsRight != 0 || gpsLat != 0) subMessage.LateralAxisGpsOffset = (short)((gpsRight != 0 ? -1 : 1) * (gpsLat * 2));
                            if(gpsLon > 1) subMessage.LongitudinalAxisGpsOffset = (byte)((gpsLon - 1) * 2);
                        }
                    }
                    subMessage.NicA = _BitStream.ReadBit() ? (byte)4 : (byte)0;
                    if(version == 2 && subType == 1) subMessage.NicC = (byte)(om & 0x01);
                    subMessage.NacP = _BitStream.ReadByte(4);
                    if(version == 2 && subType == 0) subMessage.Gva = _BitStream.ReadByte(2);
                    else                             _BitStream.Skip(2);
                    subMessage.Sil = _BitStream.ReadByte(2);
                    if(subType == 0) subMessage.NicBaro = _BitStream.ReadBit();
                    else             subMessage.SurfacePositionAngleIsTrack = _BitStream.ReadBit();
                    subMessage.HorizontalReferenceIsMagneticNorth = _BitStream.ReadBit();
                    if(version == 2) {
                        subMessage.SilSupplement = _BitStream.ReadBit();
                        subMessage.IsRebroadcast = _BitStream.ReadBit();
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the fields describing a Compact Position Reporting coordinate in a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="encodingBits"></param>
        /// <returns></returns>
        private CompactPositionReportingCoordinate ExtractCprCoordinate(AdsbMessage message, byte encodingBits)
        {
            CompactPositionReportingCoordinate result = null;

            if(message.Type != 0) {
                bool isOddFormat = _BitStream.ReadBit();
                int latitude = (int)_BitStream.ReadUInt32(17);
                int longitude = (int)_BitStream.ReadUInt32(17);
                result = new CompactPositionReportingCoordinate(latitude, longitude, isOddFormat, encodingBits);
            }

            return result;
        }
        #endregion
    }
}
