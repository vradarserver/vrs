// Copyright © 2019 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// ImageSharp implementation of <see cref="IImageFile"/>.
    /// </summary>
    class ImageFile : IImageFile
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        public Size LoadDimensions(Stream imageStream)
        {
            Size result = null;

            if(imageStream != null && imageStream.Length > 0) {
                var imageInfo = Image.Identify(imageStream);
                if(imageInfo != null) {
                    result = new Size(imageInfo.Width, imageInfo.Height);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Interface.Drawing.IImage LoadFromFile(string fileName)
        {
            return new ImageWrapper(Image.Load(fileName));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Interface.Drawing.IImage LoadFromStream(Stream stream)
        {
            return new ImageWrapper(Image.Load(stream));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public Interface.Drawing.IImage LoadFromByteArray(byte[] array)
        {
            return new ImageWrapper(Image.Load(array));
        }
    }
}
