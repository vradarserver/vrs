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
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// An enumeration of the standard sizes for images used by the web site.
    /// </summary>
    public enum StandardWebSiteImageSize
    {
        /// <summary>
        /// Not applicable / not specified.
        /// </summary>
        None,

        /// <summary>
        /// The original size of the stock image.
        /// </summary>
        Full,

        /// <summary>
        /// The size of a thumbnail of an aircraft picture in the aircraft list.
        /// </summary>
        PictureListThumbnail,

        /// <summary>
        /// The size of an aircraft picture in the aircraft detail panel.
        /// </summary>
        PictureDetail,

        /// <summary>
        /// The size of an aircraft picture in the iPhone detail panel.
        /// </summary>
        IPhoneDetail,

        /// <summary>
        /// The size of an aircraft picture in the iPad detail panel.
        /// </summary>
        IPadDetail,

        /// <summary>
        /// The size of an aircraft picture conforming to the 200 x 133 standard.
        /// </summary>
        /// <remarks>
        /// If the original is larger than 200 x 133 it is scaled to fit on the longest edge
        /// and then cropped to 200 x 133 from the centre, as per <see cref="PictureListThumbnail"/>.
        /// If the original is smaller than 200 x 133 then it is zoomed up so that the shortest
        /// side is either 200 or 133, and then cropped from the centre.
        /// </remarks>
        BaseStation,
    }
}
