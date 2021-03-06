// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


namespace VirtualRadar.Interface.Drawing
{
    /// <summary>
    /// Describes a two dimensional image's size.
    /// </summary>
    public class Size
    {
        /// <summary>
        /// An empty size.
        /// </summary>
        public static readonly Size Empty = new Size();

        /// <summary>
        /// Gets the pixel width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the pixel height of the image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// True if the width and height are both zero.
        /// </summary>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Size()
        {
            ;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Returns a new size with the width passed across. If the width is unchanged then the current
        /// object is returned.
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public Size CloneNewWidth(int width) => width == Width ? this : new Size(width, Height);

        /// <summary>
        /// Returns a new size with the height passed across. If the height is unchanged then the current
        /// object is returned.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public Size CloneNewHeight(int height) => height == Height ? this : new Size(Width, height);
    }
}
