// Copyright © 2014 onwards, Andrew Whewell
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
using System.Windows.Forms;
using System.Drawing;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// An image list wrapper that you can add images to and then retrieve the index of added
    /// images later.
    /// </summary>
    class DynamicImageList : IDisposable
    {
        /// <summary>
        /// The image list that's wrapped by this class.
        /// </summary>
        private ImageList _ImageList;

        /// <summary>
        /// A list of original image references for images that went into the image list.
        /// </summary>
        /// <remarks>
        /// We need this so that we can compare the original images against incoming images to see if
        /// they are the same reference. Referential equality is good enough for us, the images are
        /// all coming from resources.
        /// </remarks>
        private List<Image> _ImageReferences = new List<Image>();

        /// <summary>
        /// Gets the image list being wrapped by the class.
        /// </summary>
        public ImageList ImageList
        {
            get { return _ImageList; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <remarks>
        /// Defaults to an image size of 16x16 pixels.
        /// </remarks>
        public DynamicImageList() : this(new Size(16, 16))
        {
            ;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="size"></param>
        public DynamicImageList(Size size)
        {
            _ImageList = new ImageList() {
                ImageSize = size,
                ColorDepth = ColorDepth.Depth32Bit,
            };
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DynamicImageList()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _ImageReferences.Clear();

                if(_ImageList != null) _ImageList.Dispose();
                _ImageList = null;
            }
        }

        /// <summary>
        /// Adds the image to the image list, returns its index. If the image already exists then
        /// the index of the existing image is returned and the wrapped image list remains unchanged.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public int AddImage(Image image)
        {
            var result = GetIndex(image);

            if(result == -1) {
                result = ImageList.Images.Count;
                ImageList.Images.Add(image);
                _ImageReferences.Add(image);
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the image passed across.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        /// <remarks>
        /// Referential equality is good enough for us, all images are coming from resources.
        /// </remarks>
        public int GetIndex(Image image)
        {
            int result = -1;

            for(var i = 0;i < _ImageReferences.Count;++i) {
                var existingImage = _ImageReferences[i];
                if(Object.ReferenceEquals(existingImage, image)) {
                    result = i;
                    break;
                }
            }

            return result;
        }
    }
}
