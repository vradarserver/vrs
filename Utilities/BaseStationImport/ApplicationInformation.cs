// Copyright © 2017 onwards, Andrew Whewell
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace BaseStationImport
{
    /// <summary>
    /// Utility implementation of <see cref="IApplicationInformation"/>.
    /// </summary>
    class ApplicationInformation : IApplicationInformation
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortVersion { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FullVersion { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime BuildDate { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Copyright { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public CultureInfo CultureInfo => ProgramLifetime.ForcedCultureInfo;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Headless { get => true; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsService { get => false; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ApplicationInformation()
        {
            var assembly = Assembly.GetExecutingAssembly();

            Version = assembly.GetName().Version;
            ShortVersion = String.Format("{0}.{1}.{2}", Version.Major, Version.Minor, Version.Build);
            FullVersion = Version.ToString();
            ApplicationName = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).OfType<AssemblyTitleAttribute>().First().Title;
            ProductName = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().First().Product;
            BuildDate = ExtractBuildDate(assembly);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Description = fileVersionInfo.FileDescription;
            Copyright = fileVersionInfo.LegalCopyright;
        }

        /// <summary>
        /// Extracts the build date from the PE header of the main executable.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private DateTime ExtractBuildDate(Assembly assembly)
        {
            DateTime result = DateTime.MinValue;
            const int peHeaderOffset = 60;
            const int peLinkerTimestampOffset = 8;

            try {
                var fileName = assembly.Location;
                if(!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                    var buffer = new byte[2048];
                    using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                        var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if(peHeaderOffset + 4 < bytesRead) {
                            var linkerHeaderOffset = BitConverter.ToInt32(buffer, peHeaderOffset);
                            var timestampOffset = linkerHeaderOffset + peLinkerTimestampOffset;
                            if(timestampOffset + 4 < bytesRead) {
                                var timestamp = BitConverter.ToInt32(buffer, timestampOffset);
                                result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                result = result.AddSeconds(timestamp);
                            }
                        }
                    }
                }
            } catch {
                ;
            }

            return result;
        }
    }
}
