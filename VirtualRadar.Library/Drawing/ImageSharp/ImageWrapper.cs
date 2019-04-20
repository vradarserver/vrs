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
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.ImageSharp
{
    /// <summary>
    /// The ImageSharp implementation of IImage - wraps an ImageSharp image.
    /// </summary>
    class ImageWrapper : CommonImageWrapper<Image<Rgba32>>
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeImage"></param>
        public ImageWrapper(Image<Rgba32> nativeImage) : base(nativeImage)
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override VrsDrawing.Size ExtractSizeFromNativeImage()
        {
            return Convert.ToVrsSize(NativeImage.Size());
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override Interface.Drawing.IImage Clone()
        {
            var clone = NativeImage.Clone();
            return new ImageWrapper(clone);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        public override VrsDrawing.IImage Resize(int width, int height, VrsDrawing.ResizeMode mode = VrsDrawing.ResizeMode.Normal, VrsDrawing.IBrush zoomBackground = null, bool preferSpeedOverQuality = true)
        {
            var resized = mode == VrsDrawing.ResizeMode.Zoom
                ? ResizeZoom(width, height, zoomBackground)
                : ResizeNotZoom(mode, width, height);

            return new ImageWrapper(resized);
        }

        /// <summary>
        /// Performs all versions of the resize operation except Zoom (aka Pad).
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Image<Rgba32> ResizeNotZoom(VrsDrawing.ResizeMode mode, int width, int height)
        {
            var options = new ResizeOptions() {
                Mode =      ResizeMode.Crop,
                Position =  AnchorPositionMode.TopLeft,
                Size =      new SixLabors.Primitives.Size(width, height),
                Sampler =   new NearestNeighborResampler(),
            };

            switch(mode) {
                case VrsDrawing.ResizeMode.Centre:
                    options.Position = AnchorPositionMode.Center;
                    break;
                case VrsDrawing.ResizeMode.Stretch:
                    options.Mode = ResizeMode.Stretch;
                    break;
                case VrsDrawing.ResizeMode.Normal:
                    break;
                default:
                    throw new NotImplementedException();
            }

            return NativeImage.Clone(context => context.Resize(options));
        }

        /// <summary>
        /// Performs a zoom resize.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="padBrush"></param>
        /// <returns></returns>
        /// <remarks>
        /// ImageSharp **almost** supports zoom as VRS understands it. Setting mode to Pad and position
        /// to Center gets you mostly there, you just can't control the background brush. So in the
        /// future this could get a lot simpler if / when background pad brush support is added.
        /// </remarks>
        private Image<Rgba32> ResizeZoom(int width, int height, VrsDrawing.IBrush padBrush)
        {
            using(var result = new Image<Rgba32>(width, height)) {
                var resizeWidth = width;
                var resizeHeight = height;
                var widthPercent = (double)width / (double)NativeImage.Width;
                var heightPercent = (double)height / (double)NativeImage.Height;
                if(widthPercent > heightPercent)        resizeWidth = Math.Min(width, (int)(((double)NativeImage.Width * heightPercent) + 0.5));
                else if(heightPercent > widthPercent)   resizeHeight = Math.Min(height, (int)(((double)NativeImage.Height * widthPercent) + 0.5));

                var left = (width - resizeWidth) / 2;
                var top = (height - resizeHeight) / 2;

                using(var resizedTemporary = NativeImage.Clone(context => context.Resize(resizeWidth, resizeHeight, new NearestNeighborResampler()))) {
                    return result.Clone(context => {
                        if(padBrush is BrushWrapper padBrushWrapper) {
                            context.Fill(padBrushWrapper.NativeBrush);
                        }
                        context.DrawImage(resizedTemporary, new SixLabors.Primitives.Point(left, top), 1.0F);
                    });
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public override byte[] GetImageBytes(VrsDrawing.ImageFormat imageFormat)
        {
            using(var memoryStream = new MemoryStream()) {
                switch(imageFormat) {
                    case VrsDrawing.ImageFormat.Bmp:    NativeImage.SaveAsBmp(memoryStream); break;
                    case VrsDrawing.ImageFormat.Gif:    NativeImage.SaveAsGif(memoryStream); break;
                    case VrsDrawing.ImageFormat.Jpeg:   NativeImage.SaveAsJpeg(memoryStream); break;
                    case VrsDrawing.ImageFormat.Png:    NativeImage.SaveAsPng(memoryStream); break;
                    default:                            throw new NotImplementedException();
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public override VrsDrawing.IImage Rotate(float degrees)
        {
            var image = NativeImage.Clone(context => {
                context.Rotate(degrees);
            });

            return new ImageWrapper(image);
        }
    }
}
