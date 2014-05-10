// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// The interface for objects that can abstract away the environment for <see cref="IStandingDataUpdater"/>.
    /// </summary>
    public interface IStandingDataUpdaterProvider
    {
        /// <summary>
        /// Returns true if the file exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool FileExists(string fileName);

        /// <summary>
        /// Returns the entire content of the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string[] ReadLines(string fileName);

        /// <summary>
        /// Overwrites the entire file with the text passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lines"></param>
        void WriteLines(string fileName, string[] lines);

        /// <summary>
        /// Downloads the content of a text file from the URL passed across.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string[] DownloadLines(string url);

        /// <summary>
        /// Downloads the URL passed across to the file passed across, overwriting the file if it already exists.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="temporaryFileName"></param>
        /// <remarks>
        /// The URL is assumed to be pointing to a GZIP format compressed source, the file written is the
        /// decompressed version.
        /// </remarks>
        void DownloadAndDecompressFile(string url, string temporaryFileName);

        /// <summary>
        /// Deletes the live file and moves the temporary file to the live filename.
        /// </summary>
        /// <param name="temporaryFileName"></param>
        /// <param name="liveFileName"></param>
        void MoveFile(string temporaryFileName, string liveFileName);
    }
}
