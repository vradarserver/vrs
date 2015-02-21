// Copyright © 2015 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A base class for simple state objects that carry information in BeginXXXXX calls. Holds a wait
    /// handle that can be manually signalled in the EndXXXXXX call and waited on in calling thread.
    /// </summary>
    public class WaitState : IDisposable
    {
        /// <summary>
        /// Gets a wait handle that can be used to signal the Begin thread that the End thread has completed.
        /// </summary>
        /// <remarks>
        /// Note that this will be null after the object has been disposed.
        /// </remarks>
        public ManualResetEvent WaitHandle { get; private set; }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~WaitState()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WaitState()
        {
            WaitHandle = new ManualResetEvent(false);
        }

        /// <summary>
        /// See interface docs.
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
                WaitHandle.Close();
                WaitHandle = null;
            }
        }
    }
}
