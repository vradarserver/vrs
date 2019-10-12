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
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for singleton objects that can tell the program about the environment it's running under.
    /// </summary>
    [Singleton]
    public interface IRuntimeEnvironment : ISingleton<IRuntimeEnvironment>
    {
        /// <summary>
        /// Gets a value indicating that the program is running under Mono.
        /// </summary>
        bool IsMono { get; }

        /// <summary>
        /// Gets the Mono runtime version - but only major.minor.build, no revision. This returns null if <see cref="IsMono"/> is false.
        /// </summary>
        /// <remarks>
        /// Bear in mind that if you compare a version of 1.2.3 against a version of 1.2.3.0 then they are not equal. You need to
        /// create the version that you want to compare against without a revision number if you want to compare for equality.
        /// </remarks>
        Version MonoVersion { get; }

        /// <summary>
        /// Gets the raw text that was parsed into <see cref="MonoVersion"/>. This returns null if <see cref="IsMono"/> is false.
        /// </summary>
        string MonoVersionText { get; }

        /// <summary>
        /// Gets a value indicating that the program is running as a 64-bit process.
        /// </summary>
        bool Is64BitProcess { get; }

        /// <summary>
        /// Gets or sets a value indicating that the application is running under the unit test environment.
        /// </summary>
        /// <remarks>
        /// The default implementation of this property always returns false. Some tests may provide mock implementations
        /// that always return true. The intention here is to allow multithreading code to detect when it's running under
        /// unit tests and turn itself off, forcing all actions onto a single thread thereby making classes that use
        /// multi-threading internally a lot more testable.
        /// </remarks>
        bool IsTest { get; set; }

        /// <summary>
        /// Gets the path to the application.
        /// </summary>
        string ExecutablePath { get; }

        /// <summary>
        /// Gets the main thread's culture info.
        /// </summary>
        CultureInfo MainThreadCultureInfo { get; }
    }
}
