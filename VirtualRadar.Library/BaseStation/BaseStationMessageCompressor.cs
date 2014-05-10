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
using VirtualRadar.Interface.BaseStation;
using System.IO;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.BaseStation
{
    /// <summary>
    /// The default implementation of <see cref="IBaseStationMessageCompressor"/>.
    /// </summary>
    class BaseStationMessageCompressor : IBaseStationMessageCompressor
    {
        #region Fields
        /// <summary>
        /// The object that will calculate checksums for us.
        /// </summary>
        private static Crc16 _ChecksumCalculator = new Crc16();
        #endregion

        #region Enums - OptionalFields, IntSize, CompressedFlags
        [Flags]
        enum OptionalFields : short
        {
            None                = 0,
            CallSign            = 0x0001,
            Altitude            = 0x0002,
            GroundSpeed         = 0x0004,
            Track               = 0x0008,
            Latitude            = 0x0010,
            Longitude           = 0x0020,
            VerticalRate        = 0x0040,
            Squawk              = 0x0080,
            SquawkHasChanged    = 0x0100,
            Emergency           = 0x0200,
            IdentActive         = 0x0400,
            OnGround            = 0x0800,
        }

        enum IntSize
        {
            None                = 0,
            SByte               = 1,
            SShort              = 2,
            SInt                = 3,
        }

        [Flags]
        enum CompressedFlags : byte
        {
            None                = 0,
            SquawkHasChanged    = 0x01,
            Emergency           = 0x02,
            IdentActive         = 0x04,
            OnGround            = 0x08,
        }
        #endregion

        #region Compress, Decompress
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public byte[] Compress(BaseStationMessage message)
        {
            byte[] result = null;

            if(IsMeaningfulMessage(message)) {
                using(var stream = new MemoryStream()) {
                    using(var writer = new BinaryWriter(stream)) {
                        stream.WriteByte(0); // <-- reserve a byte for the buffer length
                        writer.Write((ushort)0); // <-- reserve a word for the checksum
                        stream.WriteByte((byte)(int)message.TransmissionType);
                        EncodeIcao(stream, message.Icao24);
                        long optionalFlagsOffset = stream.Position;
                        stream.WriteByte(0); // <-- reserve two bytes for the optional fields flags
                        stream.WriteByte(0); // <--    "
                        OptionalFields optionalFields = OptionalFields.None;
                        if(!String.IsNullOrEmpty(message.Callsign)) { optionalFields |= OptionalFields.CallSign; EncodeString(stream, message.Callsign); }
                        if(message.Altitude != null)                { optionalFields |= OptionalFields.Altitude; EncodeFloatInt(stream, message.Altitude.Value); }
                        if(message.GroundSpeed != null)             { optionalFields |= OptionalFields.GroundSpeed; EncodeFloatShort(writer, message.GroundSpeed.Value); }
                        if(message.Track != null)                   { optionalFields |= OptionalFields.Track; EncodeFloatShort(writer, message.Track.Value * 10f); }
                        if(message.Latitude != null)                { optionalFields |= OptionalFields.Latitude; EncodeFloat(writer, (float)message.Latitude.Value); }
                        if(message.Longitude != null)               { optionalFields |= OptionalFields.Longitude; EncodeFloat(writer, (float)message.Longitude.Value); }
                        if(message.VerticalRate != null)            { optionalFields |= OptionalFields.VerticalRate; EncodeFloatShort(writer, message.VerticalRate.Value); }
                        if(message.Squawk != null)                  { optionalFields |= OptionalFields.Squawk; EncodeShort(writer, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, message.Squawk.Value))); }

                        CompressedFlags flags = CompressedFlags.None;
                        bool hasFlag = false;
                        if(message.SquawkHasChanged != null)        { optionalFields |= OptionalFields.SquawkHasChanged; hasFlag = true; if(message.SquawkHasChanged.Value) flags |= CompressedFlags.SquawkHasChanged; }
                        if(message.Emergency != null)               { optionalFields |= OptionalFields.Emergency; hasFlag = true; if(message.Emergency.Value) flags |= CompressedFlags.Emergency; }
                        if(message.IdentActive != null)             { optionalFields |= OptionalFields.IdentActive; hasFlag = true; if(message.IdentActive.Value) flags |= CompressedFlags.IdentActive; }
                        if(message.OnGround != null)                { optionalFields |= OptionalFields.OnGround; hasFlag = true; if(message.OnGround.Value) flags |= CompressedFlags.OnGround; }
                        if(hasFlag) stream.WriteByte((byte)flags);

                        stream.Seek(optionalFlagsOffset, SeekOrigin.Begin);
                        EncodeShort(writer, (short)optionalFields);
                    }

                    result = stream.ToArray();
                    if(result.Length != 0) {
                        result[0] = (byte)result.Length;
                        var checksum = CalculateChecksum(result);
                        result[1] = (byte)(checksum & 0x00ff);
                        result[2] = (byte)((checksum & 0xff00) >> 8);
                    }
                }
            }

            return result ?? new byte[0];
        }

        private ushort CalculateChecksum(byte[] array)
        {
            var shortArray = new byte[array.Length - 3];
            Array.Copy(array, 3, shortArray, 0, shortArray.Length);

            return _ChecksumCalculator.ComputeChecksum(shortArray);
        }

        /// <summary>
        /// Returns false if the message doesn't carry useful information and can be discarded.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool IsMeaningfulMessage(BaseStationMessage message)
        {
            return message.MessageType == BaseStationMessageType.Transmission &&
                   message.TransmissionType != BaseStationTransmissionType.AllCallReply &&
                   message.TransmissionType != BaseStationTransmissionType.None &&
                   message.Icao24 != null && message.Icao24.Length == 6;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool IsCompressedMessage(byte[] bytes)
        {
            bool result = false;

            if(bytes != null && bytes.Length > 2 && bytes.Length == (int)bytes[0]) {
                var checksum = CalculateChecksum(bytes);
                result = (byte)(checksum & 0x00ff) == bytes[1] &&
                         (byte)((checksum & 0xff00) >> 8) == bytes[2];
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public BaseStationMessage Decompress(byte[] buffer)
        {
            BaseStationMessage result = null;

            if(buffer.Length > 0 && (int)buffer[0] == buffer.Length) {
                using(var stream = new MemoryStream(buffer)) {
                    using(var reader = new BinaryReader(stream)) {
                        result = new BaseStationMessage();
                        stream.ReadByte(); // <-- length of buffer, we can skip this

                        ushort checksum = reader.ReadUInt16();  // the checksum is skipped as well - caller should ensure it's a valid message before decompression

                        result.MessageType = BaseStationMessageType.Transmission;
                        result.StatusCode = BaseStationStatusCode.None;
                        result.TransmissionType = (BaseStationTransmissionType)stream.ReadByte();
                        result.Icao24 = DecodeIcao(stream);
                        result.MessageGenerated = result.MessageLogged = DateTime.Now;

                        OptionalFields optionalFields = (OptionalFields)DecodeShort(reader);

                        if((optionalFields & OptionalFields.CallSign) != 0)     result.Callsign = DecodeString(stream);
                        if((optionalFields & OptionalFields.Altitude) != 0)     result.Altitude = (int)DecodeFloatInt(stream);
                        if((optionalFields & OptionalFields.GroundSpeed) != 0)  result.GroundSpeed = (int)DecodeFloatShort(reader);
                        if((optionalFields & OptionalFields.Track) != 0)        result.Track = DecodeFloatShort(reader) / 10f;
                        if((optionalFields & OptionalFields.Latitude) != 0)     result.Latitude = DecodeFloat(reader);
                        if((optionalFields & OptionalFields.Longitude) != 0)    result.Longitude = DecodeFloat(reader);
                        if((optionalFields & OptionalFields.VerticalRate) != 0) result.VerticalRate = (int)DecodeFloatShort(reader);
                        if((optionalFields & OptionalFields.Squawk) != 0)       result.Squawk = DecodeShort(reader);

                        bool hasSquawkHasChanged =  (optionalFields & OptionalFields.SquawkHasChanged) != 0;
                        bool hasEmergency =         (optionalFields & OptionalFields.Emergency) != 0;
                        bool hasIdentActive =       (optionalFields & OptionalFields.IdentActive) != 0;
                        bool hasOnGround =          (optionalFields & OptionalFields.OnGround) != 0;

                        if(hasSquawkHasChanged || hasEmergency || hasIdentActive || hasOnGround) {
                            CompressedFlags flags = (CompressedFlags)stream.ReadByte();
                            if(hasSquawkHasChanged)     result.SquawkHasChanged = (flags & CompressedFlags.SquawkHasChanged) != 0;
                            if(hasEmergency)            result.Emergency = (flags & CompressedFlags.Emergency) != 0;
                            if(hasIdentActive)          result.IdentActive = (flags & CompressedFlags.IdentActive) != 0;
                            if(hasOnGround)             result.OnGround = (flags & CompressedFlags.OnGround) != 0;
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region Encode..., Decode...
        private static void EncodeIcao(Stream stream, string icao)
        {
            int value = Convert.ToInt32(icao, 16);
            Encode24BitInt(stream, value);
        }

        private static string DecodeIcao(Stream stream)
        {
            return Decode24BitInt(stream).ToString("X6");
        }

        private static void EncodeString(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string DecodeString(Stream stream)
        {
            int length = (int)stream.ReadByte();
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, buffer.Length);
        }

        private static void EncodeFloatInt(Stream stream, float value)
        {
            int intValue = (int)Math.Min(8388607, Math.Max(-8388607, value));
            bool isNegative = intValue < 0;
            intValue = Math.Abs(intValue);
            if(isNegative) intValue |= 0x800000;

            Encode24BitInt(stream, intValue);
        }

        private static float DecodeFloatInt(Stream stream)
        {
            int intValue = Decode24BitInt(stream);
            if((intValue & 0x800000) != 0) intValue = -(intValue & 0x7fffff);

            return (float)intValue;
        }

        private static void EncodeFloatShort(BinaryWriter writer, float value)
        {
            EncodeShort(writer, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value)));
        }

        private static float DecodeFloatShort(BinaryReader reader)
        {
            return (float)DecodeShort(reader);
        }

        private static void Encode24BitInt(Stream stream, int value)
        {
            stream.WriteByte((byte)((value & 0x00ff0000) >> 16));
            stream.WriteByte((byte)((value & 0x0000ff00) >> 8));
            stream.WriteByte((byte)((value & 0x000000ff)));
        }

        private static int Decode24BitInt(Stream stream)
        {
            return (stream.ReadByte() << 16) + (stream.ReadByte() << 8) + stream.ReadByte();
        }

        private static void EncodeFloat(BinaryWriter writer, float value)
        {
            writer.Write(value);
        }

        private static float DecodeFloat(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        private static void EncodeInt(BinaryWriter writer, int value)
        {
            writer.Write(value);
        }

        private static int DecodeInt(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        private static void EncodeShort(BinaryWriter writer, short value)
        {
            writer.Write(value);
        }

        private static short DecodeShort(BinaryReader reader)
        {
            return reader.ReadInt16();
        }
        #endregion
    }
}
