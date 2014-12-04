// Copyright © 2013 onwards, Andrew Whewell
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
using System.Security.Cryptography;
using System.IO;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// Describes the content of a single line in a checksum file resource.
    /// </summary>
    public class ChecksumFileEntry
    {
        /// <summary>
        /// The object that calculates checksums for us.
        /// </summary>
        /// <remarks>
        /// Originally the code used MD5 to calculate checksums, which was actually
        /// pretty fast... on modern chips it's quicker than calculating the CRC.
        /// However MD5 isn't FIPS approved, so users with the FIPS switch turned
        /// on in Windows couldn't run VRS. It would give them an exception whenever
        /// they tried to view the website. Hence the move to CRC64s.
        /// </remarks>
        private static Crc64 _ChecksumCalculator = new Crc64();

        /// <summary>
        /// Gets or sets the checksum for the file.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the file from root.
        /// </summary>
        /// <remarks>
        /// For example, if the file's full path is c:\Site\Root\Subfolder\File.html and the root folder
        /// of the site is c:\Site\Root then this property would be \Subfolder\File.html.
        /// </remarks>
        public string FileName { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Checksum, FileSize, FileName);
        }

        /// <summary>
        /// Generates a checksum for a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GenerateChecksum(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException("fileName");
            if(!File.Exists(fileName)) throw new InvalidOperationException(String.Format("{0} does not exist", fileName));

            var bytes = File.ReadAllBytes(fileName);
            return _ChecksumCalculator.ComputeChecksumString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Returns the length of the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static long GetFileSize(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException("fileName");
            if(!File.Exists(fileName)) throw new InvalidOperationException(String.Format("{0} does not exist", fileName));

            return new FileInfo(fileName).Length;
        }

        /// <summary>
        /// Returns the filename of the file from the root folder.
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileNameFromFullPath(string rootFolder, string fileName)
        {
            if(String.IsNullOrEmpty(rootFolder)) throw new ArgumentNullException("rootFolder");
            rootFolder = rootFolder.Replace("/", "\\");
            fileName = fileName.Replace("/", "\\");
            if(rootFolder[rootFolder.Length - 1] != '\\') rootFolder += '\\';

            if(!fileName.StartsWith(rootFolder)) throw new InvalidOperationException(String.Format("{0} does not start with {1}", fileName, rootFolder));

            return fileName.Substring(rootFolder.Length - 1);
        }

        /// <summary>
        /// Returns the full path and name of the file from the root folder.
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public string GetFullPathFromRoot(string rootFolder)
        {
            if(rootFolder == null) throw new ArgumentNullException("rootFolder");

            string result = null;
            if(!String.IsNullOrEmpty(FileName)) {
                var fileName = Path.DirectorySeparatorChar == '\\' ? FileName : FileName.Replace('\\', Path.DirectorySeparatorChar);
                result = Path.Combine(rootFolder, fileName.Substring(1));
            }

            return result;
        }
    }
}
