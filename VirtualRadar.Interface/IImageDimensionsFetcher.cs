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
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for an object that can determine the dimensions of an image stored in an image file.
    /// </summary>
    /// <remarks>
    /// Implementations must be thread-safe.
    /// </remarks>
    public interface IImageDimensionsFetcher
    {
        /// <summary>
        /// Reads the dimensions of the image from the file, hopefully without actually loading the entire file.
        /// </summary>
        /// <param name="fileName">Full path to the image file. The user must be able to open the file for reading.</param>
        /// <returns></returns>
        Size ReadDimensions(string fileName);

        /// <summary>
        /// Reads the dimensions of the image from the stream, hopefully without actually loading the entire file.
        /// </summary>
        /// <param name="stream">A stream of image bits. The stream must be positionable.</param>
        /// <returns></returns>
        Size ReadDimensions(Stream stream);
    }
}
