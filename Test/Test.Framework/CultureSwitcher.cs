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
using System.Threading;
using System.Globalization;

namespace Test.Framework
{
    /// <summary>
    /// A class that switches the current thread to a particular culture when it is created and then
    /// restores the original culture when it's disposed.
    /// </summary>
    /// <remarks>
    /// Virtual Radar Server has an audience in several countries. Some tests need to make sure that
    /// the things work correctly regardless of the regional settings. This class can help with
    /// that, you use it in a using() block to switch the culture to a known region and then automatically
    /// switch it back at the end of the test, even if the test threw an
    /// exception.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public void My_Object_Parses_Strings_Correctly()
    /// {
    ///     var regions = new string[] { "en-GB", "de-DE", "fr-FR" };
    ///     foreach(var region in regions) {
    ///       using(var cultureSwitcher = new CultureSwitcher(region)) {
    ///           // Do some work here and assert that the result is as expected
    ///       }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class CultureSwitcher : IDisposable
    {
        private CultureInfo _CurrentCulture;
        private CultureInfo _CurrentUICulture;

        /// <summary>
        /// Gets the name of the culture currently in force.
        /// </summary>
        public string CultureName { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="culture">The name of the culture (e.g. 'en-GB' or 'de-DE') that the current thread
        /// will be set to.</param>
        public CultureSwitcher(string culture)
        {
            CultureName = culture;

            _CurrentCulture = Thread.CurrentThread.CurrentCulture;
            _CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CultureSwitcher()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the object. Resets the culture back to how the constructor found it.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Thread.CurrentThread.CurrentCulture = _CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = _CurrentUICulture;
            }
        }
    }
}
