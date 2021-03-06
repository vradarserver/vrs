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
    /// Describes an integer coordinate point within an image.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// The point at 0, 0.
        /// </summary>
        public static readonly Point Empty = new Point();

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y coordinate.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Point()
        {
            ;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns a new Point with the X value changed. If there is no change in X value then
        /// the current object is returned.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Point CloneNewX(int x) => x == X ? this : new Point(x, Y);

        /// <summary>
        /// Returns a new Point with the Y value changed. If there is no change in Y value then
        /// the current object is returned.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point CloneNewY(int y) => y == Y ? this : new Point(X, y);
    }
}
