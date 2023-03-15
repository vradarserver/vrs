// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.Feeds;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Library.ModeS
{
    /// <summary>
    /// The default implementation of <see cref="IModeSTranslator"/>.
    /// </summary>
    [Obsolete("Do not create instances of this directly. Use dependency injection instead. This is only public so that it can be unit tested")]
    public sealed class ModeSTranslator : IModeSTranslator
    {
        /// <summary>
        /// The class that can stream bits from an array of bytes for us.
        /// </summary>
        private BitStream _BitStream = new();

        /// <inheritdoc/>
        public ReceiverStatistics Statistics { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <param name="start"></param>
        /// <param name="signalLevel"></param>
        /// <param name="isMlat"></param>
        /// <returns></returns>
        public ModeSMessage Translate(byte[] rawMessage, int start, int? signalLevel, bool isMlat)
        {
            if(Statistics == null) {
                throw new InvalidOperationException("Statistics must be provided before Translate can do any work");
            }
            ModeSMessage result = null;

            var messageLength = rawMessage == null
                ? 0
                : rawMessage.Length - start;

            if(messageLength > 6) {
                _BitStream.Initialise(rawMessage);
                _BitStream.Skip(start * 8);
                var downlinkFormatValue = _BitStream.ReadByte(5);
                if(downlinkFormatValue >= 24) {
                    downlinkFormatValue = 24;
                    _BitStream.Skip(-3);
                }

                var isLongFrame = true;
                switch(downlinkFormatValue) {
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 24:
                        if(messageLength > 13) {
                            result = new ModeSMessage();
                        }
                        break;
                    default:
                        isLongFrame = false;
                        result = new ModeSMessage();
                        break;
                }

                if(result != null) {
                    result.SignalLevel = signalLevel;
                    result.IsMlat = isMlat;
                    result.DownlinkFormat = (DownlinkFormat)downlinkFormatValue;

                    switch(result.DownlinkFormat) {
                        case DownlinkFormat.ShortAirToAirSurveillance:      DecodeShortAirToAirSurveillance(result); break;         // DF0
                        case DownlinkFormat.SurveillanceAltitudeReply:      DecodeSurveillanceAltitudeReply(result); break;         // DF4
                        case DownlinkFormat.SurveillanceIdentityReply:      DecodeSurveillanceIdentityReply(result); break;         // DF5
                        case DownlinkFormat.AllCallReply:                   DecodeAllCallReply(result); break;                      // DF11
                        case DownlinkFormat.LongAirToAirSurveillance:       DecodeLongAirToAirSurveillance(result); break;          // DF16
                        case DownlinkFormat.ExtendedSquitter:               DecodeExtendedSquitter(result); break;                  // DF17
                        case DownlinkFormat.ExtendedSquitterNonTransponder: DecodeExtendedSquitterNonTransponder(result); break;    // DF18
                        case DownlinkFormat.MilitaryExtendedSquitter:       DecodeMilitaryExtendedSquitter(result); break;          // DF19
                        case DownlinkFormat.CommBAltitudeReply:             DecodeCommBAltitudeReply(result); break;                // DF20
                        case DownlinkFormat.CommBIdentityReply:             DecodeCommBIdentityReply(result); break;                // DF21
                        case DownlinkFormat.CommD:                          DecodeCommD(result); break;                             // DF24
                    }

                    if(Statistics != null) {
                        Statistics.Lock(r => {
                            ++r.ModeSMessagesReceived;
                            ++r.ModeSDFStatistics[(int)result.DownlinkFormat].MessagesReceived;
                            if(isLongFrame) {
                                ++r.ModeSLongFrameMessagesReceived;
                            } else {
                                ++r.ModeSShortFrameMessagesReceived;
                            }
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Decodes the fields from a short air-to-air surveillance reply (DF = 0).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeShortAirToAirSurveillance(ModeSMessage reply)
        {
            ExtractVerticalStatus(reply);
            reply.CrossLinkCapability = _BitStream.ReadBit();
            _BitStream.Skip(1);
            ExtractSensitivityLevel(reply);
            _BitStream.Skip(2);
            ExtractReplyInformation(reply);
            _BitStream.Skip(2);
            ExtractAltitudeCode(reply);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from a surveillance altitude reply (DF = 4).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeSurveillanceAltitudeReply(ModeSMessage reply)
        {
            ExtractFlightStatus(reply);
            ExtractDownlinkRequest(reply);
            ExtractUtilityMessage(reply);
            ExtractAltitudeCode(reply);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from a surveillance identity reply (DF = 5).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeSurveillanceIdentityReply(ModeSMessage reply)
        {
            ExtractFlightStatus(reply);
            ExtractDownlinkRequest(reply);
            ExtractUtilityMessage(reply);
            ExtractIdentity(reply);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from an all-call reply (DF = 11).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeAllCallReply(ModeSMessage reply)
        {
            ExtractCapability(reply);
            ExtractAircraftAddress(reply);
            ExtractParityInterrogator(reply);
        }

        /// <summary>
        /// Decodes the fields from a long air-to-air surveillance reply (DF = 16).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeLongAirToAirSurveillance(ModeSMessage reply)
        {
            ExtractVerticalStatus(reply);
            _BitStream.Skip(2);
            ExtractSensitivityLevel(reply);
            _BitStream.Skip(2);
            ExtractReplyInformation(reply);
            _BitStream.Skip(2);
            ExtractAltitudeCode(reply);
            reply.ACASMessage = ReadBytes(7);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from an extended squitter reply (DF = 17).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeExtendedSquitter(ModeSMessage reply)
        {
            ExtractCapability(reply);
            ExtractAircraftAddress(reply);
            ExtractExtendedSquitterMessage(reply);
            ExtractParityInterrogator(reply);
        }

        /// <summary>
        /// Decodes the fields from an extended squitter from non-transponder reply (DF = 18).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeExtendedSquitterNonTransponder(ModeSMessage reply)
        {
            reply.ControlField = (ControlField)_BitStream.ReadByte(3);
            switch(reply.ControlField) {
                case ControlField.AdsbDeviceTransmittingIcao24:
                case ControlField.AdsbRebroadcastOfExtendedSquitter:
                    ExtractAircraftAddress(reply);
                    ExtractExtendedSquitterMessage(reply);
                    break;
                case ControlField.FineFormatTisb:
                case ControlField.CoarseFormatTisb:
                case ControlField.AdsbDeviceNotTransmittingIcao24:
                    reply.NonIcao24Address = (int)_BitStream.ReadUInt32(24);
                    ExtractExtendedSquitterMessage(reply);
                    break;
                default:
                    reply.ExtendedSquitterSupplementaryMessage = ReadBytes(10);
                    break;
            }
            ExtractParityInterrogator(reply);
        }

        /// <summary>
        /// Decodes the fields from a military extended squitter reply (DF = 19).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeMilitaryExtendedSquitter(ModeSMessage reply)
        {
            reply.ApplicationField = (ApplicationField)_BitStream.ReadByte(3);
            if(reply.ApplicationField != ApplicationField.ADSB) {
                reply.ExtendedSquitterSupplementaryMessage = ReadBytes(13);
            } else {
                ExtractAircraftAddress(reply);
                ExtractExtendedSquitterMessage(reply);
                ExtractParityInterrogator(reply);
            }
        }

        /// <summary>
        /// Decodes the fields from a Comm-B altitude reply (DF = 20).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeCommBAltitudeReply(ModeSMessage reply)
        {
            ExtractFlightStatus(reply);
            ExtractDownlinkRequest(reply);
            ExtractUtilityMessage(reply);
            ExtractAltitudeCode(reply);
            ExtractCommBMessage(reply);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from a Comm-B identity reply (DF = 21).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeCommBIdentityReply(ModeSMessage reply)
        {
            ExtractFlightStatus(reply);
            ExtractDownlinkRequest(reply);
            ExtractUtilityMessage(reply);
            ExtractIdentity(reply);
            ExtractCommBMessage(reply);
            ExtractAircraftParityIdentifier(reply);
        }

        /// <summary>
        /// Decodes the fields from a Comm-D reply (DF = 24).
        /// </summary>
        /// <param name="reply"></param>
        private void DecodeCommD(ModeSMessage reply)
        {
            _BitStream.Skip(1);
            reply.ElmControl = (ElmControl)_BitStream.ReadByte(1);
            reply.DSegmentNumber = _BitStream.ReadByte(4);
            reply.CommDMessage = ReadBytes(10);
            ExtractAircraftParityIdentifier(reply);
        }

        private void ExtractAircraftAddress(ModeSMessage reply) =>          reply.Icao24 = (int)_BitStream.ReadUInt32(24);

        private void ExtractAircraftParityIdentifier(ModeSMessage reply) => reply.Icao24 = (int)_BitStream.ReadUInt32(24);

        private void ExtractCapability(ModeSMessage reply) =>               reply.Capability = (Capability)_BitStream.ReadByte(3);

        private void ExtractDownlinkRequest(ModeSMessage reply) =>          reply.DownlinkRequest = _BitStream.ReadByte(5);

        private void ExtractExtendedSquitterMessage(ModeSMessage reply) =>  reply.ExtendedSquitterMessage = ReadBytes(7);

        private void ExtractFlightStatus(ModeSMessage reply) =>             reply.FlightStatus = (FlightStatus)_BitStream.ReadByte(3);

        private void ExtractParityInterrogator(ModeSMessage reply) =>       reply.ParityInterrogatorIdentifier = (int)_BitStream.ReadUInt32(24);

        private void ExtractReplyInformation(ModeSMessage reply) =>         reply.ReplyInformation = _BitStream.ReadByte(4);

        private void ExtractSensitivityLevel(ModeSMessage reply) =>         reply.SensitivityLevel = _BitStream.ReadByte(3);

        private void ExtractUtilityMessage(ModeSMessage reply) =>           reply.UtilityMessage = _BitStream.ReadByte(6);

        private void ExtractVerticalStatus(ModeSMessage reply) =>           reply.VerticalStatus = (VerticalStatus)_BitStream.ReadByte(1);

        private void ExtractAltitudeCode(ModeSMessage reply)
        {
            reply.Altitude = _BitStream.ReadUInt16(13);
            reply.AltitudeIsMetric = (reply.Altitude & 0x40) != 0;
            if(!reply.AltitudeIsMetric.Value) {
                var decodedAltitude = ((reply.Altitude & 0x1F80) >> 2)
                                    | ((reply.Altitude & 0x20) >> 1)
                                    | (reply.Altitude & 0x0f);
                if((reply.Altitude & 0x10) != 0) {
                    decodedAltitude = ModeSAltitudeConversion.CalculateBinaryAltitude(decodedAltitude.Value);
                } else {
                    decodedAltitude = ModeSAltitudeConversion.LookupGillhamAltitude(decodedAltitude.Value);
                }
                reply.Altitude = decodedAltitude;
            }
        }

        private void ExtractCommBMessage(ModeSMessage reply)
        {
            reply.CommBMessage = ReadBytes(7);
            if(reply.CommBMessage[0] == 0x20) {
                _BitStream.Skip(-48);
                reply.PossibleCallsign = ModeSCharacterTranslator.ExtractCharacters(_BitStream, 8);
            }
        }

        private void ExtractIdentity(ModeSMessage reply)
        {
            var bits = (short)_BitStream.ReadUInt16(13);
            reply.Identity = ModeATranslator.DecodeModeA(bits);
        }

        private byte[] ReadBytes(int length)
        {
            var result = new byte[length];
            for(var i = 0;i < length;++i) {
                result[i] = _BitStream.ReadByte(8);
            }

            return result;
        }
    }
}
