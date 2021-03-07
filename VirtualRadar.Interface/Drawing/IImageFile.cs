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
using System.IO;
using InterfaceFactory;

namespace VirtualRadar.Interface.Drawing
{
    /// <summary>
    /// Handles the creation and storage of images.
    /// </summary>
    [Singleton]
    public interface IImageFile
    {
        /// <summary>
        /// Creates a drawing environment on a copy of the image passed across. Calls the <paramref name="drawAction"/>
        /// method passing a drawing object that can be used to modify the clone. Returns the clone.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="drawAction"></param>
        /// <returns></returns>
        IImage CloneAndDraw(IImage source, Action<IDrawing> drawAction);

        /// <summary>
        /// Returns a new empty 32-bit RGBA image with the pixel width and height passed across.
        /// </summary>
        /// <param name="pixelWidth"></param>
        /// <param name="pixelHeight"></param>
        /// <returns></returns>
        IImage Create(int pixelWidth, int pixelHeight);

        /// <summary>
        /// Returns a new empty 32-bit RGBA image with the pixel width and height passed across and
        /// filled with the brush passed across.
        /// </summary>
        /// <param name="pixelWidth"></param>
        /// <param name="pixelHeight"></param>
        /// <param name="backgroundBrush"></param>
        /// <returns></returns>
        IImage Create(int pixelWidth, int pixelHeight, IBrush backgroundBrush);

        /// <summary>
        /// Given a stream pointing at the start of an image file this will return the size of the image
        /// represented by the stream.
        /// </summary>
        /// <param name="imageStream"></param>
        /// <returns>
        /// The pixel width and height of the image or null if the stream is null, the stream is empty or the
        /// stream does not represent an image.
        /// </returns>
        Size LoadDimensions(Stream imageStream);

        /// <summary>
        /// Reads an image out of a file on disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IImage LoadFromFile(string fileName);

        /// <summary>
        /// Reads an image from a stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IImage LoadFromStream(Stream stream);

        /// <summary>
        /// Reads an image from a byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        IImage LoadFromByteArray(byte[] array);
    }
}
