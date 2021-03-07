// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Drawing;
using System.IO;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// This creates clones of System.Drawing images from VirtualRadar.Resources. Previously the resources
    /// library would do this but it no longer references System.Drawing.
    /// </summary>
    /// <remarks><para>
    /// System.Drawing is only used by VirtualRadar.WinForms, with the intention being that it's only going
    /// to be used by Windows builds in the short term. Longer term all builds will be using the same cross
    /// platform UI but I don't know what that will be yet.
    /// </para><para>
    /// The resources library used to return clones of images but now it returns image byte arrays. This just
    /// creates images from those byte arrays. Only the images used by WinForms are cloned here.
    /// </para></remarks>
    static class ResourceImages
    {
        public static Image Add16x16 => CreateImage(Images.Add16x16);

        public static Icon ApplicationIcon => CreateIcon(Images.ApplicationIcon);

        public static Image ArrowBack16x16 => CreateImage(Images.ArrowBack16x16);

        public static Image ArrowForward16x16 => CreateImage(Images.ArrowForward16x16);

        public static Image Cancel16x16 => CreateImage(Images.Cancel16x16);

        public static Image Decoding16x16 => CreateImage(Images.Decoding16x16);

        public static Image Edit16x16 => CreateImage(Images.Edit16x16);

        public static Image Gear16x16 => CreateImage(Images.Gear16x16);

        public static Image HelpAbout => CreateImage(Images.HelpAbout);

        public static Image Location16x16 => CreateImage(Images.Location16x16);

        public static Image Logo128x128 => CreateImage(Images.Logo128x128);

        public static Image MergedFeed16x16 => CreateImage(Images.MergedFeed16x16);

        public static Image Notebook16x16 => CreateImage(Images.Notebook16x16);

        public static Image Radio16x16 => CreateImage(Images.Radio16x16);

        public static Image Radio48x48 => CreateImage(Images.Radio48x48);

        public static Image Rebroadcast16x16 => CreateImage(Images.Rebroadcast16x16);

        public static Image Server16x16 => CreateImage(Images.Server16x16);

        public static Image Site16x16 => CreateImage(Images.Site16x16);

        public static Image Test16x16 => CreateImage(Images.Test16x16);

        public static Image Transparent_16x16 => CreateImage(Images.Transparent_16x16);

        public static Image User16x16 => CreateImage(Images.User16x16);

        public static Image Wizard16x16 => CreateImage(Images.Wizard16x16);

        private static Icon CreateIcon(byte[] iconBytes)
        {
            using(var stream = new MemoryStream(iconBytes)) {
                return new Icon(stream);
            }
        }

        private static Image CreateImage(byte[] imageBytes)
        {
            using(var stream = new MemoryStream(imageBytes)) {
                return Bitmap.FromStream(stream);
            }
        }
    }
}
