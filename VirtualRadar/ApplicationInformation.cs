// Copyright © 2010 onwards, Andrew Whewell  
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;

namespace VirtualRadar
{
    /// <summary>
    /// The default implementation of <see cref="IApplicationInformation"/>.
    /// </summary>
    class ApplicationInformation : IApplicationInformation
    {
        // Static fields that hold the information exposed by the object. It never changes over the
        // course of the application's execution so there's no need to keep re-fetching it.
        private static bool _Loaded;
        private static Version _Version;
        private static string _ShortVersion;
        private static string _FullVersion;
        private static string _BetaBasedOnShortVersion;
        private static string _BetaBasedOnFullVersion;
        private static string _BetaRevision;
        private static string _ApplicationName;
        private static string _ProductName;
        private static string _Description;
        private static string _Copyright;
        private static bool _Headless;
        private static DateTime _BuildDate;
        private static bool _IsService;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Version Version => _Version;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShortVersion => _ShortVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FullVersion => _FullVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BetaBasedOnShortVersion => _BetaBasedOnShortVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BetaBasedOnFullVersion => _BetaBasedOnFullVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsBeta => !String.IsNullOrEmpty(_BetaBasedOnFullVersion);

        public string BetaRevision => _BetaRevision ?? "";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime BuildDate => _BuildDate;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ApplicationName => _ApplicationName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ProductName => _ProductName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Description => _Description;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Copyright => _Copyright;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public CultureInfo CultureInfo => CultureInfo.CurrentCulture;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Headless => _Headless;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsService => _IsService;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ApplicationInformation()
        {
            if(!_Loaded) {
                _Loaded = true;

                var assembly = Assembly.GetExecutingAssembly();

                _Version = assembly.GetName().Version;
                _ShortVersion = $"{Version.Major}.{Version.Minor}.{Version.Build}";
                _FullVersion = Version.ToString();
                _ApplicationName = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).OfType<AssemblyTitleAttribute>().First().Title;
                _ProductName = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().First().Product;
                _Description = $"{Strings.ApplicationDescription}{Environment.NewLine}{Environment.NewLine}{Strings.License}:{Environment.NewLine}{Environment.NewLine}{Strings.LicenseContent}";
                _Copyright = Strings.Copyright;
                _BuildDate = ExtractBuildDate(assembly);

                var informationalVersion = (assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
                if(!String.IsNullOrEmpty(informationalVersion)) {
                    var betaRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)-beta-(?<betaRevision>\d+)$");
                    var match = betaRegex.Match(informationalVersion);
                    if(match.Success) {
                        var major = match.Groups["major"].Value;
                        var minor = match.Groups["minor"].Value;
                        var build = match.Groups["build"].Value;
                        var revision = match.Groups["revision"].Value;
                        var betaRevision = match.Groups["betaRevision"].Value;
                        _BetaBasedOnFullVersion = $"{major}.{minor}.{build}.{revision}";
                        _BetaBasedOnShortVersion = $"{major}.{minor}.{build}";
                        _BetaRevision = betaRevision.TrimStart('0');
                    }
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Headless"/> property.
        /// </summary>
        /// <param name="headless"></param>
        public static void SetHeadless(bool headless)
        {
            _Headless = headless;
        }

        /// <summary>
        /// Sets the <see cref="IsService"/> property.
        /// </summary>
        /// <param name="isService"></param>
        public static void SetIsService(bool isService)
        {
            _IsService = isService;
        }

        /// <summary>
        /// Extracts the build date from the PE header of the main executable.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static DateTime ExtractBuildDate(Assembly assembly)
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
