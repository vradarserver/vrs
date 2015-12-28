// Copyright © 2015 onwards, Andrew Whewell
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// See interface docs.
    /// </summary>
    class ImageDimensionsFetcher : IImageDimensionsFetcher
    {
        /// <summary>
        /// A private enumeration of the file types that the fetcher understands.
        /// </summary>
        enum FileType
        {
            Unknown,
            WindowsBitmap,
            OS2Bitmap,
            Gif,
            Png,
            Jpeg,
        };

        /// <summary>
        /// A class that describes the values that identify each file format.
        /// </summary>
        class MagicNumber
        {
            public byte[] Prefix { get; private set; }

            public FileType FileType { get; private set; }

            public MagicNumber(FileType fileType, byte[] prefix)
            {
                Prefix = prefix;
                FileType = fileType;
            }

            public bool MatchesFilePrefix(byte[] filePrefix, int prefixLength)
            {
                var result = prefixLength >= Prefix.Length;
                if(result) {
                    for(var i = 0;i < Prefix.Length;++i) {
                        if(filePrefix[i] != Prefix[i]) {
                            result = false;
                            break;
                        }
                    }
                }

                return result;
            }
        };

        /// <summary>
        /// An array of the known magic numbers in descending order of prefix length.
        /// </summary>
        private static readonly MagicNumber[] _MagicNumbersSorted = new MagicNumber[] {
            new MagicNumber(FileType.Png,           new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, }),
            new MagicNumber(FileType.Gif,           new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61, }),
            new MagicNumber(FileType.Gif,           new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, }),
            new MagicNumber(FileType.WindowsBitmap, new byte[] { 0x42, 0x4D, }),
            new MagicNumber(FileType.OS2Bitmap,     new byte[] { 0x42, 0x4A, }),
            new MagicNumber(FileType.Jpeg,          new byte[] { 0xff, 0xd8, }),
        };

        // The number of bytes to allocate to a buffer.
        private const int BufferSize = 128;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Size ReadDimensions(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException("fileName");
            if(fileName == "") throw new ArgumentException("Filename cannot be empty", fileName);
            if(!File.Exists(fileName)) throw new FileNotFoundException("Image file does not exist", fileName);

            using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return ReadDimensions(stream);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Size ReadDimensions(Stream stream)
        {
            if(stream.Position > 0) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var longestPrefix = _MagicNumbersSorted[0].Prefix.Length;
            var filePrefix = new byte[longestPrefix];
            var bytesRead = stream.Read(filePrefix, 0, filePrefix.Length);
            var magicNumber = _MagicNumbersSorted.FirstOrDefault(r => r.MatchesFilePrefix(filePrefix, bytesRead));
            var buffer = new byte[BufferSize];

            var result = Size.Empty;
            if(magicNumber != null) {
                result = DecodeFileFormat(stream, magicNumber, buffer);
            }
            if(result.IsEmpty) {
                result = LoadImageAndReadDimensions(stream);
            }

            return result;
        }

        /// <summary>
        /// The expensive way to do it - load the entire image and read the dimensions from it.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private Size LoadImageAndReadDimensions(Stream stream)
        {
            if(stream.Position > 0) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using(var image = Image.FromStream(stream)) {
                return image.Size;
            }
        }

        /// <summary>
        /// The cheap but more tedious way to do it - decode the file format and pick the size out of it.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="magicNumber"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodeFileFormat(Stream stream, MagicNumber magicNumber, byte[] buffer)
        {
            var result = Size.Empty;

            switch(magicNumber.FileType) {
                case FileType.WindowsBitmap:    result = DecodeWindowsBitmap(stream, buffer); break;
                case FileType.OS2Bitmap:        result = DecodeOS2Bitmap(stream, buffer); break;
                case FileType.Gif:              result = DecodeGif(stream, buffer); break;
                case FileType.Jpeg:             result = DecodeJpeg(stream, result, buffer); break;
                case FileType.Png:              result = DecodePng(stream, buffer); break;
                default:                        throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Extracts the dimensions of an image from a Windows BMP format file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodeWindowsBitmap(Stream stream, byte[] buffer)
        {
            // Using file format information from http://en.wikipedia.org/wiki/BMP_file_format

            stream.Seek(18, SeekOrigin.Begin);
            var width = ReadLittleEndianInt32(stream, buffer);
            var height = ReadLittleEndianInt32(stream, buffer);
            var result = new Size(width, height);

            return result;
        }

        /// <summary>
        /// Extracts the dimensions of an image from an OS/2 BMP format file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodeOS2Bitmap(Stream stream, byte[] buffer)
        {
            // Using file format information from http://en.wikipedia.org/wiki/BMP_file_format

            stream.Seek(18, SeekOrigin.Begin);
            var width = ReadLittleEndianInt16(stream, buffer);
            var height = ReadLittleEndianInt16(stream, buffer);
            var result = new Size(width, height);

            return result;
        }

        /// <summary>
        /// Extracts the dimensions of an image from a GIF format file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodeGif(Stream stream, byte[] buffer)
        {
            // Using file format information from http://en.wikipedia.org/wiki/GIF#File_format

            stream.Seek(6, SeekOrigin.Begin);
            var width = ReadLittleEndianInt16(stream, buffer);
            var height = ReadLittleEndianInt16(stream, buffer);
            var result = new Size(width, height);

            return result;
        }

        /// <summary>
        /// Extracts the dimensions of an image from a JPEG format file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="errorSize"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodeJpeg(Stream stream, Size errorSize, byte[] buffer)
        {
            // Using the file format information from http://en.wikipedia.org/wiki/JPEG#Syntax_and_structure
            // ... and http://lad.dsc.ufcg.edu.br/multimidia/jpegmarker.pdf (SOF layout)
            // If there's a flavour of JPEG that isn't covered here then we return errorSize, which will trigger a full
            // load of the image and extraction of the dimensions from that.

            var result = errorSize;

            // Find each block until we get a block that contains dimensions. Blocks start with 0xFF.
            stream.Seek(2, SeekOrigin.Begin);
            var finished = false;
            while(!finished) {
                var segmentMarkerPosition = FindJpegSegmentStart(stream, buffer);
                finished = segmentMarkerPosition == -1;
                if(!finished) {
                    var frameType = stream.ReadByte();
                    var segmentPayloadStart = stream.Position;
                    var segmentLength = ReadBigEndianInt16(stream, buffer);

                    // Reset the segment length if the segment has no payload (i.e. it's followed immediately by another segment marker)
                    if(segmentLength >= 0xFF00) segmentLength = 0;

                    switch(frameType) {
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                            stream.ReadByte();      // sample position
                            var height = ReadBigEndianInt16(stream, buffer);
                            var width = ReadBigEndianInt16(stream, buffer);
                            result = new Size(width, height);
                            finished = true;
                            break;
                        default:
                            break;
                    }

                    if(!finished) {
                        stream.Seek(segmentPayloadStart + segmentLength, SeekOrigin.Begin);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the start of the next segment in a JPEG file stream. Positions the stream on the frame type and
        /// returns the stream position, or returns -1 if no more segments could be found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private long FindJpegSegmentStart(Stream stream, byte[] buffer)
        {
            long result = -1;

            var finished = false;
            while(!finished) {
                var bufferPosition = stream.Position;
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                finished = bytesRead == -1;
                for(var i = 0;!finished && i < bytesRead;++i) {
                    if(buffer[i] == 0xFF) {
                        result = bufferPosition + i + 1;
                        stream.Seek(result, SeekOrigin.Begin);
                        finished = true;
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Extracts the dimensions of an image from a PNG format file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Size DecodePng(Stream stream, byte[] buffer)
        {
            // Using file format information from http://www.libpng.org/pub/png/spec/1.0/PNG-Contents.html

            stream.Seek(16, SeekOrigin.Begin);
            var width = ReadBigEndianInt32(stream, buffer);
            var height = ReadBigEndianInt32(stream, buffer);
            var result = new Size(width, height);

            return result;
        }

        /// <summary>
        /// Reads a number of bytes into the buffer and returns them.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="buffer"></param>
        private void ReadIntegerBytes(Stream stream, int length, byte[] buffer)
        {
            if(length > buffer.Length) throw new InvalidOperationException(String.Format("The buffer is {0} bytes long, needs to be at least {1} bytes", buffer.Length, length));
            var bytesRead = stream.Read(buffer, 0, length);
            if(bytesRead != length) throw new IOException(String.Format("Could not read {0} bytes from the stream", length));
        }

        /// <summary>
        /// Reads a big-endian two byte integer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int ReadBigEndianInt16(Stream stream, byte[] buffer)
        {
            ReadIntegerBytes(stream, 2, buffer);
            return (((int)buffer[0]) << 8) +
                   (int)buffer[1];
        }

        /// <summary>
        /// Reads a little-endian two byte integer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int ReadLittleEndianInt16(Stream stream, byte[] buffer)
        {
            ReadIntegerBytes(stream, 2, buffer);
            return (((int)buffer[1]) << 8) +
                   (int)buffer[0];
        }

        /// <summary>
        /// Reads a big-endian four byte integer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int ReadBigEndianInt32(Stream stream, byte[] buffer)
        {
            ReadIntegerBytes(stream, 4, buffer);
            return (((int)buffer[0]) << 24) +
                   (((int)buffer[1]) << 16) +
                   (((int)buffer[2]) << 8) +
                   (int)buffer[3];
        }

        /// <summary>
        /// Reads a little-endian four byte integer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int ReadLittleEndianInt32(Stream stream, byte[] buffer)
        {
            ReadIntegerBytes(stream, 4, buffer);
            return (((int)buffer[3]) << 24) +
                   (((int)buffer[2]) << 16) +
                   (((int)buffer[1]) << 8) +
                   (int)buffer[0];
        }
    }
}
