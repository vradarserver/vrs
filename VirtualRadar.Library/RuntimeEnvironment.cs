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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IRuntimeEnvironment"/>.
    /// </summary>
    class RuntimeEnvironment : IRuntimeEnvironment
    {
        private static bool _IsMono;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsMono => _IsMono;

        private static string _MonoVersionText;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string MonoVersionText => _MonoVersionText;

        private static Version _MonoVersion;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Version MonoVersion => _MonoVersion;

        private bool? _Is64BitProcess;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Is64BitProcess
        {
            get {
                if(_Is64BitProcess == null) {
                    _Is64BitProcess = IntPtr.Size == 8;
                }
                return _Is64BitProcess.Value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTest
        {
            get { return false; }
            set { ; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ExecutablePath { get { return Application.StartupPath; } }

        public static CultureInfo _MainThreadCultureInfo;
        public CultureInfo MainThreadCultureInfo { get { return _MainThreadCultureInfo; } }

        /// <summary>
        /// The static initialiser.
        /// </summary>
        static RuntimeEnvironment()
        {
            // The assumption here is that the very first time this is called will be on the main thread,
            // so we just copy the current thread's culture info.
            _MainThreadCultureInfo = Thread.CurrentThread.CurrentCulture;

            ExtractMonoInformation();
        }

        private static void ExtractMonoInformation()
        {
            var monoRuntimeType = Type.GetType("Mono.Runtime");
            _IsMono = monoRuntimeType != null;

            if(_IsMono) {
                _MonoVersion = new Version(0, 0, 0, 0);

                try {
                    var getDisplayName = monoRuntimeType == null ? null : monoRuntimeType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    _MonoVersionText = getDisplayName.Invoke(null, null) as string;
                } catch(Exception ex) {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine($"Attempting to call Mono.Runtime.GetDisplayName() via reflection threw exception: {ex}");
                    _MonoVersionText = "";
                }

                var regex = new Regex(@"\b(?<version>\d+\.\d+.\d+)\b");
                var match = regex.Match(_MonoVersionText);
                if(match.Success) {
                    var versionText = match.Groups["version"].Value;
                    if(Version.TryParse(versionText, out var version)) {
                        _MonoVersion = version;
                    }
                }
            }
        }
    }
}
