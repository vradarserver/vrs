// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Manages access to images stored on disk.
    /// </summary>
    public interface IImageFileManager
    {
        /// <summary>
        /// Loads the image stored in the file specified.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IImage LoadFromFile(string fileName);

        /// <summary>
        /// Attempts to figure out the dimensions of an image stored within a file as quickly as possible.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Size ImageFileDimensions(string fileName);

        /// <summary>
        /// Loads an image using the standard web site pipeline. If caching is allowed then a clone of the
        /// cached image is always returned.
        /// </summary>
        /// <param name="webPathAndFileName"></param>
        /// <param name="useImageCache"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        IImage LoadFromStandardPipeline(string webPathAndFileName, bool useImageCache, IDictionary<string, object> owinEnvironment);
    }
}
