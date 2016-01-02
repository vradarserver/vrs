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
using System.Linq;
using System.Text;
using System.Threading;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that implements a simple lock that avoids the overhead imposed by using lock().
    /// </summary>
    /// <remarks><para>
    /// You can create one of these objects to share amongst all of your threads and then call <see cref="Lock"/>
    /// to acquire a lock. If the object is already locked then the thread will spin until the lock can be acquired.
    /// Every call to <see cref="Lock"/> <em>must</em> be paired with a call to <see cref="Unlock"/>, otherwise
    /// the lock remains held forever and every other thread will block.
    /// </para><para>
    /// A child class, <see cref="Acquire"/>, exists to wrap the <see cref="Lock"/> and <see cref="Unlock"/> within
    /// a using statement, ensuring that the lock and unlock operations are always paired but at the expense of
    /// creating an object for it.
    /// </para><para>
    /// Unlike the traditional C# lock call a thread can lock itself if it calls <see cref="Lock"/> twice - care must
    /// be taken to avoid double-locks. What you gain in speed you lose in convenience.
    /// </para></remarks>
    public class SpinLock
    {
        /// <summary>
        /// A wrapper class that calls Lock in its constructor and Unlock in its Dispose.
        /// </summary>
        public class Acquire : IDisposable
        {
            private SpinLock _SpinLock;

            /// <summary>
            /// Creates a new object, locking the <see cref="SpinLock"/> passed across.
            /// </summary>
            /// <param name="spinLock"></param>
            public Acquire(SpinLock spinLock)
            {
                _SpinLock = spinLock;
                _SpinLock.Lock();
            }

            /// <summary>
            /// Releases the lock on <see cref="SpinLock"/>.
            /// </summary>
            public void Dispose()
            {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// The counter that indicates whether the object has acquired the lock. This is 0 if the object has not been locked by anything.
        /// </summary>
        private int _Locked;

        /// <summary>
        /// Acquires the lock on the SpinLock, blocking indefinitely until it manages to acquire the lock. Ensure that <see cref="Unlock"/> is always
        /// called after Lock has returned.
        /// </summary>
        public void Lock()
        {
            while(Interlocked.Exchange(ref _Locked, 1) != 0) {
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Releases the lock on the SpinLock. This will unlock the SpinLock even if Lock was never called - only call this after <see cref="Lock"/>
        /// has returned.
        /// </summary>
        public void Unlock()
        {
            Interlocked.Exchange(ref _Locked, 0);
        }

        /// <summary>
        /// Locks the SpinLock and returns an object that unlocks the SpinLock when it is disposed.
        /// </summary>
        /// <returns></returns>
        public Acquire AcquireLock()
        {
            return new Acquire(this);
        }
    }
}
