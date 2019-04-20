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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// An immutable object that describes a pen.
    /// </summary>
    class PenKey
    {
        public int Red { get; }

        public int Green { get; }

        public int Blue { get; }

        public int Alpha { get; }

        public float StrokeWidth { get; }

        public PenKey(int red, int green, int blue, int alpha, float strokeWidth)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
            StrokeWidth = strokeWidth;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is PenKey other) {
                result =    Red ==          other.Red
                         && Green ==        other.Green
                         && Blue ==         other.Blue
                         && Alpha ==        other.Alpha
                         && StrokeWidth ==  other.StrokeWidth;
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ColourHelper.PackColour(Red, Green, Blue, Alpha).GetHashCode();
        }
    }
}
