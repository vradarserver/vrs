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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// This is a wrapper around Win32's FindFirstFile and FindNextFile. When VRS moves over to .NET 4 we can get
    /// rid of this.
    /// </summary>
    /// <remarks><para>
    /// Before .NET 4.0 the DirectoryInfo.GetFiles() method would read all of the files in a folder in one go but
    /// it would lazily read the attributes, lengths etc. of those files. When reading the names and last modified
    /// dates of 7500 files across the network using DirectoryInfo it was taking a minute in 3.5 and a couple of
    /// seconds in 4.
    /// </para><para>
    /// I need it to be very fast but I can't justify switching the product over to .Net 4.0 just for that, so instead
    /// I've done this wrapper around Win32's FindFirstFile / FindNextFile. When I do move VRS onto .Net 4 I can
    /// probably ditch this. Also, if VRS is being ported to some other platform this can probably be reimplemented in C#
    /// using a DirectoryInfo.
    /// </para></remarks>
    public class FindFile
    {
        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto, BestFitMapping=false)]
        static extern IntPtr FindFirstFile(string fileName, out FindFileData data);

        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto, BestFitMapping=false)]
        static extern bool FindNextFile(IntPtr hndFindFile, out FindFileData data);

        [DllImport("kernel32.dll")]
        static extern bool FindClose(IntPtr handle); 

        private readonly static string _PathSeparator = new String(Path.DirectorySeparatorChar, 1);

        /// <summary>
        /// Returns a list of files in a folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IList<FindFileData> GetFiles(string path)
        {
            List<FindFileData> result = new List<FindFileData>();

            if(Factory.Resolve<IRuntimeEnvironment>().IsMono) MonoGetFiles(result, path);
            else                                                        Win32GetFiles(result, path);

            return result;
        }

        /// <summary>
        /// Builds a list of files in a folder under Mono.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="path"></param>
        private void MonoGetFiles(List<FindFileData> result, string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach(var file in directoryInfo.GetFiles()) {
                result.Add(new FindFileData() {
                    cFileName = file.Name,
                });
            }
        }

        /// <summary>
        /// Builds a list of files in a folder under Win32.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="path"></param>
        private void Win32GetFiles(List<FindFileData> result, string path)
        {
            if(path == null) throw new ArgumentNullException(path);
            path = String.Format("{0}{1}*", path, path.EndsWith(_PathSeparator) ? "" : _PathSeparator);

            IntPtr findHandle = IntPtr.Zero;
            FindFileData fileData;
            try {
                findHandle = FindFirstFile(path, out fileData);
                if(findHandle.ToInt32() != -1) {
                    do {
                        if((fileData.dwFileAttributes & 0x00000010) == 0) result.Add(fileData);  // 0x10 is FILE_ATTRIBUTE_DIRECTORY
                        fileData = new FindFileData();
                    } while(FindNextFile(findHandle, out fileData));
                }
            } finally {
                if(findHandle != IntPtr.Zero && findHandle.ToInt32() != -1) FindClose(findHandle);
            }
        }
    }
}
