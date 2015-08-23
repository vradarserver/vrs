// Copyright © 2015 onwards, Andrew Whewell
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
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface for objects that handle graphics manipulation for the web site.
    /// </summary>
    public interface IWebSiteGraphics : ISingleton<IWebSiteGraphics>
    {
        /// <summary>
        /// Rotates the image passed across by a number of degrees, running clockwise with 0 being north.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        Image RotateImage(Image original, double degrees);

        /// <summary>
        /// Widens the image and centres the original image within it. The new pixels are transparent.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        Image WidenImage(Image original, int width, bool centreHorizontally);

        /// <summary>
        /// Heightens the image and centres the original image within it. The new pixels are transparent.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreVertically"></param>
        /// <returns></returns>
        Image HeightenImage(Image original, int height, bool centreVertically);

        /// <summary>
        /// Doubles the width and height of the image for use on high DPI displays.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        Image ResizeForHiDpi(Image original);

        /// <summary>
        /// Returns a new bitmap with the dimensions specified. The entire bitmap is filled with the
        /// background and then the original bitmap is drawn into the centre. If the aspect ratio of
        /// the original image is different to that of the new image then the ResizeMode indicates how
        /// to deal with it.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        Bitmap ResizeBitmap(Bitmap original, int width, int height, ResizeImageMode mode, Brush zoomBackground, bool preferSpeedOverQuality);

        /// <summary>
        /// Creates a fully-transparent image of the size specified.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Image CreateBlankImage(int width, int height);

        /// <summary>
        /// Creates an iPhone splash page image.
        /// </summary>
        /// <param name="webSiteAddress"></param>
        /// <param name="isIPad"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        Image CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts);

        /// <summary>
        /// Creates a new image of the required height with an altitude stalk drawn centred on the X
        /// pixel passed across.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        /// <returns></returns>
        Image AddAltitudeStalk(Image original, int height, int centreX);

        /// <summary>
        /// Overlays lines of text onto the image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        Image AddTextLines(Image image, IEnumerable<string> textLines, bool centreText, bool isHighDpi);

        /// <summary>
        /// When passed the current temporary image and the image that will become the new temporary image this
        /// disposes of the old temporary image and returns the new one. Can cope when the same image is passed
        /// across for both the temporary and new image.
        /// </summary>
        /// <param name="tempImage"></param>
        /// <param name="newImage"></param>
        /// <returns></returns>
        Image UseImage(Image tempImage, Image newImage);
    }
}
