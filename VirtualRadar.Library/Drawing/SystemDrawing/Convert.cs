using System;
using System.Drawing;
using System.Drawing.Imaging;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.SystemDrawing
{
    /// <summary>
    /// Converts between System.Drawing primitives and VirtualRadar.Interface.Drawing primitives.
    /// </summary>
    class Convert
    {
        //----------------------------------------
        // FONT STYLE
        //----------------------------------------

        public static FontStyle ToSystemDrawingFontStyle(VrsDrawing.FontStyle vrsFontStyle)
        {
            switch(vrsFontStyle) {
                case VrsDrawing.FontStyle.Bold:         return FontStyle.Bold;
                case VrsDrawing.FontStyle.Italic:       return FontStyle.Italic;
                case VrsDrawing.FontStyle.Normal:       return FontStyle.Regular;
                default:                                throw new NotImplementedException();
            }
        }

        public static VrsDrawing.FontStyle ToVrsFontStyle(FontStyle sdFontStyle)
        {
            switch(sdFontStyle) {
                case FontStyle.Bold:        return VrsDrawing.FontStyle.Bold;
                case FontStyle.Italic:      return VrsDrawing.FontStyle.Italic;
                case FontStyle.Regular:     return VrsDrawing.FontStyle.Normal;
                default:                    return (VrsDrawing.FontStyle)0x7fffffff;  // there are more System.Drawing font styles than there are VRS font styles - throwing a NotImplemented exception will cause problems
            }
        }

        //----------------------------------------
        // HORIZONTAL ALIGNMENT
        //----------------------------------------

        public static StringAlignment ToSystemDrawingStringAlignment(VrsDrawing.HorizontalAlignment vrsHorizontalAlignment)
        {
            switch(vrsHorizontalAlignment) {
                case VrsDrawing.HorizontalAlignment.Centre: return StringAlignment.Center;
                case VrsDrawing.HorizontalAlignment.Left:   return StringAlignment.Near;
                case VrsDrawing.HorizontalAlignment.Right:  return StringAlignment.Far;
                default:                                    throw new NotImplementedException();
            }
        }

        public static VrsDrawing.HorizontalAlignment ToVrsHorizontalAlignment(StringAlignment sdStringAlignment)
        {
            switch(sdStringAlignment) {
                case StringAlignment.Center:    return VrsDrawing.HorizontalAlignment.Centre;
                case StringAlignment.Near:      return VrsDrawing.HorizontalAlignment.Left;
                case StringAlignment.Far:       return VrsDrawing.HorizontalAlignment.Right;
                default:                        throw new NotImplementedException();
            }
        }

        //----------------------------------------
        // IMAGE FORMAT
        //----------------------------------------

        public static ImageFormat ToSystemDrawingImageFormat(VrsDrawing.ImageFormat vrsImageFormat)
        {
            switch(vrsImageFormat) {
                case VrsDrawing.ImageFormat.Bmp:    return ImageFormat.Bmp;
                case VrsDrawing.ImageFormat.Gif:    return ImageFormat.Gif;
                case VrsDrawing.ImageFormat.Jpeg:   return ImageFormat.Jpeg;
                case VrsDrawing.ImageFormat.Png:    return ImageFormat.Png;
                default:                            throw new NotImplementedException();
            }
        }

        public static VrsDrawing.ImageFormat ToVrsImageFormat(ImageFormat sdImageFormat)
        {
            if(sdImageFormat == ImageFormat.Bmp)    return VrsDrawing.ImageFormat.Bmp;
            if(sdImageFormat == ImageFormat.Gif)    return VrsDrawing.ImageFormat.Gif;
            if(sdImageFormat == ImageFormat.Jpeg)   return VrsDrawing.ImageFormat.Jpeg;
            if(sdImageFormat == ImageFormat.Png)    return VrsDrawing.ImageFormat.Png;
            throw new NotImplementedException();
        }

        //----------------------------------------
        // SIZE
        //----------------------------------------

        public static Size ToSystemDrawingSize(VrsDrawing.Size vrsSize) => new Size(vrsSize.Width, vrsSize.Height);

        public static SizeF ToSystemDrawingSizeF(VrsDrawing.Size vrsSize) => new SizeF(vrsSize.Width, vrsSize.Height);

        public static VrsDrawing.Size ToVrsSize(Size isSize) => new VrsDrawing.Size(isSize.Width, isSize.Height);
    }
}
