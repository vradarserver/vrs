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

namespace VirtualRadar.Interface.Drawing
{
    /// <summary>
    /// Describes an image in memory.
    /// </summary>
    public interface IImage : IDisposable
    {
        /// <summary>
        /// Gets the dimensions of the image.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Shorthand for <see cref="Size.Width"/>.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Shorthand for <see cref="Size.Height"/>.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Creates a copy of the image.
        /// </summary>
        /// <returns></returns>
        IImage Clone();

        /// <summary>
        /// Returns the bytes representing the image formatted as per the parameter.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        byte[] GetImageBytes(ImageFormat imageFormat);

        /// <summary>
        /// Returns a clone of the image resized to the new size. Speed is preferred over quality.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        IImage Resize(int width, int height, ResizeMode mode = ResizeMode.Normal, IBrush zoomBackground = null, bool preferSpeedOverQuality = true);

        /// <summary>
        /// Returns a copy of the image rotated by the number of degrees specified.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        IImage Rotate(float degrees);

        /// <summary>
        /// Returns a copy of the image with the text lines drawn onto the bottom of it. Used to draw pin text onto map markers.
        /// </summary>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        IImage AddTextLines(IEnumerable<string> textLines, bool centreText, bool isHighDpi);
    }
}
