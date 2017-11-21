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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// Handles interop with the console.
    /// </summary>
    public static class Console
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static readonly int SW_HIDE = 0;
        static readonly int SW_SHOW = 5;

        static bool _Initialised;
        static bool _IsMono;
        private static bool IsMono
        {
            get {
                Initialise();
                return _IsMono;
            }
        }

        /// <summary>
        /// Initialises the object on first use.
        /// </summary>
        private static void Initialise()
        {
            if(!_Initialised) {
                _Initialised = true;
                var runtimeEnvironment = Factory.Singleton.ResolveSingleton<IRuntimeEnvironment>();
                _IsMono = runtimeEnvironment.IsMono;
            }
        }

        /// <summary>
        /// Shows the console window.
        /// </summary>
        public static void ShowConsole()
        {
            if(!IsMono) {
                Win32ShowConsole();
            }
        }

        /// <summary>
        /// Makes the interop calls required to show the console window under Win32.
        /// </summary>
        private static void Win32ShowConsole()
        {
            var consoleHandle = GetConsoleWindow();
            if(consoleHandle == IntPtr.Zero) {
                AllocConsole();
            } else {
                ShowWindow(consoleHandle, SW_SHOW);
            }
        }

        /// <summary>
        /// Hides the console window.
        /// </summary>
        public static void HideConsole()
        {
            Initialise();
            if(!IsMono) {
                Win32HideConsole();
            }
        }

        /// <summary>
        /// Makes the interop calls required to hide the console window under Win32.
        /// </summary>
        private static void Win32HideConsole()
        {
            var consoleHandle = GetConsoleWindow();
            if(consoleHandle != IntPtr.Zero) {
                ShowWindow(consoleHandle, SW_HIDE);
            }
        }
    }
}
