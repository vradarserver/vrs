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
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.ImageSharp
{
    /// <summary>
    /// Converts between ImageSharp primitives and VirtualRadar.Interface.Drawing primitives.
    /// </summary>
    static class Convert
    {
        //----------------------------------------
        // FONT STYLE
        //----------------------------------------

        public static FontStyle ToImageSharpFontStyle(VrsDrawing.FontStyle vrsFontStyle)
        {
            switch(vrsFontStyle) {
                case VrsDrawing.FontStyle.Bold:         return FontStyle.Bold;
                case VrsDrawing.FontStyle.Italic:       return FontStyle.Italic;
                case VrsDrawing.FontStyle.Normal:       return FontStyle.Regular;
                default:                                throw new NotImplementedException();
            }
        }

        public static VrsDrawing.FontStyle ToVrsFontStyle(FontStyle isFontStyle)
        {
            switch(isFontStyle) {
                case FontStyle.Bold:        return VrsDrawing.FontStyle.Bold;
                case FontStyle.Italic:      return VrsDrawing.FontStyle.Italic;
                case FontStyle.Regular:     return VrsDrawing.FontStyle.Normal;
                default:                    return (VrsDrawing.FontStyle)0x7fffffff;  // there are more ImageSharp font styles than there are VRS font styles - throwing a NotImplemented exception will cause problems
            }
        }

        //----------------------------------------
        // HORIZONTAL ALIGNMENT
        //----------------------------------------

        public static HorizontalAlignment ToImageSharpHorizontalAlignment(VrsDrawing.HorizontalAlignment vrsHorizontalAlignment)
        {
            switch(vrsHorizontalAlignment) {
                case VrsDrawing.HorizontalAlignment.Centre: return HorizontalAlignment.Center;
                case VrsDrawing.HorizontalAlignment.Left:   return HorizontalAlignment.Left;
                case VrsDrawing.HorizontalAlignment.Right:  return HorizontalAlignment.Right;
                default:                                    throw new NotImplementedException();
            }
        }

        public static VrsDrawing.HorizontalAlignment  ToVrsHorizontalAlignment(HorizontalAlignment isHorizontalAlignment)
        {
            switch(isHorizontalAlignment) {
                case HorizontalAlignment.Center:    return VrsDrawing.HorizontalAlignment.Centre;
                case HorizontalAlignment.Left:      return VrsDrawing.HorizontalAlignment.Left;
                case HorizontalAlignment.Right:     return VrsDrawing.HorizontalAlignment.Right;
                default:                            throw new NotImplementedException();
            }
        }

        //----------------------------------------
        // POINT
        //----------------------------------------

        public static Point ToImageSharpPoint(VrsDrawing.Point vrsPoint) => new Point(vrsPoint.X, vrsPoint.Y);

        public static PointF ToImageSharpPointF(VrsDrawing.Point vrsPoint) => new PointF(vrsPoint.X, vrsPoint.Y);

        public static VrsDrawing.Point ToVrsPoint(Point isPoint) => new VrsDrawing.Point(isPoint.X, isPoint.Y);

        //----------------------------------------
        // SIZE
        //----------------------------------------

        public static Size ToImageSharpSize(VrsDrawing.Size vrsSize) => new Size(vrsSize.Width, vrsSize.Height);

        public static SizeF ToImageSharpSizeF(VrsDrawing.Size vrsSize) => new SizeF(vrsSize.Width, vrsSize.Height);

        public static VrsDrawing.Size ToVrsSize(Size isSize) => new VrsDrawing.Size(isSize.Width, isSize.Height);
    }
}
