// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="IResponseStreamWrapperConfiguration"/>.
    /// </summary>
    class ResponseStreamWrapperConfiguration : IResponseStreamWrapperConfiguration
    {
        /// <summary>
        /// Used to control writes to the fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The list of stream manipulators held by the configuration. Always take a reference
        /// to this before reading it, always lock <see cref="_SyncLock"/> before overwriting it.
        /// </summary>
        private List<WeakReference<IStreamManipulator>> _StreamManipulators = new List<WeakReference<IStreamManipulator>>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="streamManipulator"></param>
        public void AddStreamManipulator(IStreamManipulator streamManipulator)
        {
            lock(_SyncLock) {
                var newList = CopyStreamManipulatorsList();

                if(!newList.Any(r => {
                    r.TryGetTarget(out IStreamManipulator manipulator);
                    return Object.ReferenceEquals(manipulator, streamManipulator);
                })) {
                    newList.Add(new WeakReference<IStreamManipulator>(streamManipulator));

                    newList.Sort((WeakReference<IStreamManipulator> lhs, WeakReference<IStreamManipulator> rhs) => {
                        lhs.TryGetTarget(out IStreamManipulator lhsManipulator);
                        rhs.TryGetTarget(out IStreamManipulator rhsManipulator);

                        return (lhsManipulator?.ResponseStreamPriority ?? 0) - (rhsManipulator?.ResponseStreamPriority ?? 0);
                    });

                    _StreamManipulators = newList;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="streamManipulator"></param>
        public void RemoveStreamManipulator(IStreamManipulator streamManipulator)
        {
            lock(_SyncLock) {
                _StreamManipulators = CopyStreamManipulatorsList(exclude: streamManipulator);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IStreamManipulator[] GetStreamManipulators()
        {
            var result = new List<IStreamManipulator>();

            var references = _StreamManipulators;
            var sawDeadReference = false;
            foreach(var reference in references) {
                if(!reference.TryGetTarget(out IStreamManipulator streamManipulator)) {
                    sawDeadReference = true;
                } else {
                    result.Add(streamManipulator);
                }
            }

            if(sawDeadReference) {
                PruneStreamManipulatorsList();
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns a copy of the stream manipulators map. ALWAYS CALL WITHIN A LOCK. Should be no dead references in returned map.
        /// </summary>
        /// <param name="exclude"></param>
        /// <returns></returns>
        private List<WeakReference<IStreamManipulator>> CopyStreamManipulatorsList(IStreamManipulator exclude = null)
        {
            var result = new List<WeakReference<IStreamManipulator>>();

            foreach(var manipulatorRef in _StreamManipulators) {
                if(manipulatorRef.TryGetTarget(out IStreamManipulator manipulator)) {
                    if(exclude == null || !Object.ReferenceEquals(manipulator, exclude)) {
                        result.Add(manipulatorRef);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes dead references from the stream manipulators list.
        /// </summary>
        private void PruneStreamManipulatorsList()
        {
            lock(_SyncLock) {
                _StreamManipulators = CopyStreamManipulatorsList();
            }
        }
    }
}
