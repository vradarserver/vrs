using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace Test.VirtualRadar.Owin
{
    /// <summary>
    /// IImage image wrapper versions of all of the resources exposed by <see cref="TestImages"/>.
    /// </summary>
    static class TestImagesWrapped
    {
        private static VrsDrawing.IImageFile _IImageFile;

        static TestImagesWrapped()
        {
            _IImageFile = Factory.ResolveSingleton<VrsDrawing.IImageFile>();
        }

        public static VrsDrawing.IImage AltitudeImageTest_01_png => WrappedImage(TestImages.AltitudeImageTest_01_png, ImageFormat.Png);

        public static VrsDrawing.IImage DLH_bmp => WrappedImage(TestImages.DLH_bmp, ImageFormat.Bmp);

        public static VrsDrawing.IImage OversizedLogo_bmp => WrappedImage(TestImages.OversizedLogo_bmp, ImageFormat.Bmp);

        public static VrsDrawing.IImage Picture_120x140_png => WrappedImage(TestImages.Picture_120x140_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_120x140_Resized_60x40_png => WrappedImage(TestImages.Picture_120x140_Resized_60x40_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_120x80_png => WrappedImage(TestImages.Picture_120x80_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_120x80_Resized_60x40_png => WrappedImage(TestImages.Picture_120x80_Resized_60x40_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_140x80_png => WrappedImage(TestImages.Picture_140x80_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_140x80_Resized_60x40_png => WrappedImage(TestImages.Picture_140x80_Resized_60x40_png, ImageFormat.Png);

        public static VrsDrawing.IImage Picture_700x400_png => WrappedImage(TestImages.Picture_700x400_png, ImageFormat.Png);

        public static VrsDrawing.IImage TestSquare_bmp => WrappedImage(TestImages.TestSquare_bmp, ImageFormat.Bmp);

        public static VrsDrawing.IImage TestSquare_png => WrappedImage(TestImages.TestSquare_png, ImageFormat.Png);

        private static VrsDrawing.IImage WrappedImage(Bitmap bitmap, ImageFormat imageFormat)
        {
            using(var clone = (Bitmap)bitmap.Clone()) {
                using(var memoryStream = new MemoryStream()) {
                    clone.Save(memoryStream, imageFormat);
                    return _IImageFile.LoadFromByteArray(memoryStream.ToArray());
                }
            }
        }
    }
}
