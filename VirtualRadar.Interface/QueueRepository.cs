// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Exposes a collection of every queue running within the program.
    /// </summary>
    public static class QueueRepository
    {
        /// <summary>
        /// A static list of every queue running on the system.
        /// </summary>
        private static List<IQueue> _Queues = new();

        /// <summary>
        /// The lock object for the static list of queues.
        /// </summary>
        private static object _QueuesLock = new();

        /// <summary>
        /// Adds a queue to the static list of queues.
        /// </summary>
        /// <param name="queue"></param>
        public static void AddQueue(IQueue queue)
        {
            lock(_QueuesLock) {
                if(!_Queues.Contains(queue)) {
                    _Queues.Add(queue);
                }
            }
        }

        /// <summary>
        /// Removes a queue from the static list of queues.
        /// </summary>
        /// <param name="queue"></param>
        internal static void RemoveQueue(IQueue queue)
        {
            lock(_QueuesLock) {
                if(_Queues.Contains(queue)) {
                    _Queues.Remove(queue);
                }
            }
        }

        /// <summary>
        /// Returns an array of every queue currently running in the system.
        /// </summary>
        /// <returns></returns>
        public static IQueue[] GetAllQueues()
        {
            lock(_Queues) {
                return _Queues.ToArray();
            }
        }
    }
}
