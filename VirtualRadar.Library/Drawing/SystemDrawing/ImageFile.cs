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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.SystemDrawing
{

    /// <summary>
    /// System.Drawing implementation of <see cref="VrsDrawing.IImageFile"/>.
    /// </summary>
    class ImageFile : VrsDrawing.IImageFile
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="drawAction"></param>
        /// <returns></returns>
        public VrsDrawing.IImage CloneAndDraw(VrsDrawing.IImage source, Action<VrsDrawing.IDrawing> drawAction)
        {
            VrsDrawing.IImage result = null;

            if(source is ImageWrapper sourceWrapper) {
                GdiPlusLock.EnforceSingleThread(() => {
                    var clone = (Image)sourceWrapper.NativeImage.Clone();
                    using(var drawing = new Drawing(Graphics.FromImage(clone))) {
                        drawAction(drawing);
                    }
                    result = new ImageWrapper(clone);
                });
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pixelWidth"></param>
        /// <param name="pixelHeight"></param>
        /// <returns></returns>
        public VrsDrawing.IImage Create(int pixelWidth, int pixelHeight)
        {
            return new ImageWrapper(
                new Bitmap(
                    pixelWidth,
                    pixelHeight,
                    PixelFormat.Format32bppArgb
                )
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        public VrsDrawing.Size LoadDimensions(Stream imageStream)
        {
            using(var image = Image.FromStream(imageStream)) {
                return Convert.ToVrsSize(image.Size);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public VrsDrawing.IImage LoadFromByteArray(byte[] array)
        {
            using(var memoryStream = new MemoryStream(array)) {
                return LoadFromStream(memoryStream);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public VrsDrawing.IImage LoadFromFile(string fileName)
        {
            return new ImageWrapper(
                Image.FromFile(fileName)
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public VrsDrawing.IImage LoadFromStream(Stream stream)
        {
            return new ImageWrapper(
                Image.FromStream(stream)
            );
        }
    }
}
