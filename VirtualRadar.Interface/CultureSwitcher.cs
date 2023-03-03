// Copyright © 2012 onwards, Andrew Whewell
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
    /// A class that switches the current thread to a particular culture when it is created and then
    /// restores the original culture when it's disposed.
    /// </summary>
    public class CultureSwitcher : IDisposable
    {
        private readonly CultureInfo _CurrentCulture;
        private readonly CultureInfo _CurrentUICulture;

        private static CultureInfo _MainThreadCulture;
        /// <summary>
        /// Gets the main thread's CultureInfo.
        /// </summary>
        public static CultureInfo MainThreadCulture => _MainThreadCulture;

        /// <summary>
        /// Gets the name of the culture currently in force.
        /// </summary>
        public string CultureName { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CultureSwitcher() : this(null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="culture">The name of the culture (e.g. 'en-GB' or 'de-DE') that the current thread
        /// will be set to.</param>
        public CultureSwitcher(string culture)
        {
            var cultureInfo = culture == null ? MainThreadCulture : new CultureInfo(culture);
            CultureName = cultureInfo.Name;

            _CurrentCulture = Thread.CurrentThread.CurrentCulture;
            _CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
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

        /// <summary>
        /// Initialises the <see cref="MainThreadCulture"/> property for <see cref="CultureSwitcher"/>.
        /// </summary>
        /// <param name="mainThreadCulture"></param>
        public static void InitialiseCultureSwitcher(CultureInfo mainThreadCulture = null)
        {
            _MainThreadCulture = mainThreadCulture ?? Thread.CurrentThread.CurrentCulture;
        }
    }
}
