﻿// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Globalization;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can expose information about the application.
    /// </summary>
    public interface IApplicationInformation
    {
        /// <summary>
        /// Gets full version information about the application.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the short version number (e.g. for 1.2.3.4 it returns '1.2.3').
        /// </summary>
        string ShortVersion { get; }

        /// <summary>
        /// Gets the full version number (e.g. 1.2.3.4).
        /// </summary>
        string FullVersion { get; }

        /// <summary>
        /// Gets the short version number that the beta is based on. Returns null if not a beta.
        /// </summary>
        string BetaBasedOnShortVersion { get; }

        /// <summary>
        /// Gets the full version number that the beta is based on. Returns null if not a beta.
        /// </summary>
        string BetaBasedOnFullVersion { get; }

        /// <summary>
        /// Gets a value indicating that this is a beta version.
        /// </summary>
        bool IsBeta { get; }

        /// <summary>
        /// Gets the beta number or empty string if <see cref="IsBeta"/> is false.
        /// </summary>
        string BetaRevision { get; }

        /// <summary>
        /// Gets the build date of the application.
        /// </summary>
        DateTime BuildDate { get; }

        /// <summary>
        /// Gets the content of the AssemblyTitle attribute for the application.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the content of the AssemblyProduct attribute for the application.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Gets a description of the application.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the copyright attribution text for the application.
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// Gets the culture info that was forced by a command switch on startup or null if the default culture info is to be used.
        /// </summary>
        CultureInfo CultureInfo { get; }

        /// <summary>
        /// Gets a value indicating that the program is running without a GUI.
        /// </summary>
        bool Headless { get; }

        /// <summary>
        /// Gets a value indicating that the program is running as a service.
        /// </summary>
        bool IsService { get; }
    }
}
