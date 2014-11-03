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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Wraps the named mutex that can be used to synchronise write operations with
    /// other processes.
    /// </summary>
    /// <remarks><para>
    /// Occasionally users get into a bit of bother when VRS and another utility tries
    /// to write to BaseStation.sqb. Because SQLite employs locks if the two processes
    /// both attempt a simultaneous write on the same part of the file they can get a
    /// lock exception.
    /// </para><para>
    /// To help ease the situation VRS can synchronise its writes with 3rd party utilities
    /// via a named mutex. The idea is that both VRS and the utility each ask Windows
    /// to create a named mutex called BaseStation_Write_Mutex_2347892. Before each write
    /// they lock this mutex, and after they have finished their write they unlock the mutex.
    /// If writes are made within a transaction then the lock should be placed around the
    /// entire transaction.
    /// </para><para>
    /// Windows will block the thread while it waits for a lock to be released, so in this way
    /// we hopefully should not get any clashes.
    /// </para><para>
    /// To guard against an application acquiring the lock and never freeing it you should
    /// place a timeout on the wait for the lock. VRS currently uses a timeout of 60 seconds.
    /// If the wait ever times out then abandon the synchronisation scheme for the remainder
    /// of the session - from that point on just make your writes without locking the mutex.
    /// </para><para>
    /// This class is not thread safe.
    /// </para></remarks>
    static class BaseStationMutex
    {
        #region Inner class - WriteLock
        /// <summary>
        /// A helper class that acquires a lock when it's created and releases the lock when it's disposed.
        /// </summary>
        public class WriteLock : IDisposable
        {
            /// <summary>
            /// Creates a new object.
            /// </summary>
            public WriteLock()
            {
                BaseStationMutex.AcquireWriteLock();
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            public void Dispose()
            {
                BaseStationMutex.ReleaseWriteLock();
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The name that VRS and 3rd party utilities must use when creating the named write lock mutex.
        /// </summary>
        public static readonly string WriteMutexName = "BaseStation_Write_Mutex_2347892";

        /// <summary>
        /// The number of milliseconds the code will wait before it gives up trying to acquire the lock.
        /// </summary>
        public static readonly int WriteMutexTimeoutMs = 60000;

        /// <summary>
        /// The mutex that we use to lock writes.
        /// </summary>
        private static Mutex _WriteMutex;

        /// <summary>
        /// True if we have given up on synchronised writes.
        /// </summary>
        private static bool _AbandonedSynchronisedWrites;

        /// <summary>
        /// True if we're running under Mono. Mono will JIT the mutex instructions but they don't
        /// work, we can't use them.
        /// </summary>
        private static bool _IsMono;
        #endregion

        #region Constructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static BaseStationMutex()
        {
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>();
            _IsMono = runtimeEnvironment.IsMono;
        }
        #endregion

        #region AcquireWriteLock, ReleaseWriteLock
        /// <summary>
        /// Attempts to acquire a lock for a write. Always pair calls to this with
        /// calls to <see cref="ReleaseWriteLock"/>. Calls can be nested.
        /// </summary>
        public static void AcquireWriteLock()
        {
            if(CreateMutex()) {
                var acquired = false;
                try {
                    acquired = _WriteMutex.WaitOne(WriteMutexTimeoutMs, exitContext: false);
                } catch(AbandonedMutexException) {
                    // These are innocuous, it just means that something locked the mutex and then
                    // died before it could release the lock.
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("BaseStation synchronised writes mutex was abandoned - reacquiring it");

                    // We now own the mutex - release it and destroy it so we can start again
                    try {
                        _WriteMutex.ReleaseMutex();
                    } catch {}
                    DestroyMutex();

                    // Try again...
                    if(CreateMutex()) {
                        try {
                            acquired = _WriteMutex.WaitOne(WriteMutexTimeoutMs, exitContext: false);
                        } catch(AbandonedMutexException) {
                            acquired = false;
                            try {
                                _WriteMutex.ReleaseMutex();
                            } catch {}
                        }
                    }
                }

                if(!acquired) {
                    // We timed out - stop using synchronisation
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Could not acquire the synchronisation lock for BaseStation writes before the timeout expired - turning off synchronised writes");
                    _AbandonedSynchronisedWrites = true;
                    DestroyMutex();
                }
            }
        }

        /// <summary>
        /// Releases a lock previously acquired via <see cref="AcquireWriteLock"/>. Does nothing
        /// if the lock could not be acquired.
        /// </summary>
        public static void ReleaseWriteLock()
        {
            if(_WriteMutex != null) {
                _WriteMutex.ReleaseMutex();
            }
        }
        #endregion

        #region CreateMutex, DestroyMutex
        /// <summary>
        /// Populates <see cref="_WriteMutex"/> with the named mutex used for synchronised
        /// writes. Does nothing if the mutex has already been created or if we have abandoned
        /// synchronised writes due to a timeout.
        /// </summary>
        /// <returns></returns>
        private static bool CreateMutex()
        {
            if(_WriteMutex == null && !_AbandonedSynchronisedWrites && !_IsMono) {
                // Create the named mutex without trying to own it
                _WriteMutex = new Mutex(false, WriteMutexName);
    
                // Set permissions so that any process is allowed to lock the mutex
                var allowEveryoneRule = new MutexAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    MutexRights.FullControl,
                    AccessControlType.Allow
                );
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                _WriteMutex.SetAccessControl(securitySettings);
            }

            return _WriteMutex != null;
        }

        /// <summary>
        /// Disposes of the <see cref="_WriteMutex"/>.
        /// </summary>
        private static void DestroyMutex()
        {
            if(_WriteMutex != null) {
                try {
                    _WriteMutex.Close();
                } catch {}
                try {
                    ((IDisposable)_WriteMutex).Dispose();
                } catch {}

                _WriteMutex = null;
            }
        }
        #endregion
    }
}
