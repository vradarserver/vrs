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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// The base for all configuration objects that handle text response manipulators.
    /// </summary>
    class BaseTextResponseManipulatorConfiguration
    {
        /// <summary>
        /// The manipulators that have been registered.
        /// </summary>
        private List<ITextResponseManipulator> _Manipulators = new List<ITextResponseManipulator>();

        /// <summary>
        /// The lock to use when overwriting _Manipulators.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="manipulator"></param>
        public void AddTextResponseManipulator(ITextResponseManipulator manipulator)
        {
            if(manipulator == null) {
                throw new ArgumentNullException(nameof(manipulator));
            }

            lock(_SyncLock) {
                if(!_Manipulators.Any(r => Object.ReferenceEquals(manipulator, r))) {
                    var newList = CollectionHelper.ShallowCopy(_Manipulators);
                    newList.Add(manipulator);
                    _Manipulators = newList;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddTextResponseManipulator<T>()
            where T: ITextResponseManipulator
        {
            var manipulators = _Manipulators;
            if(!manipulators.Any(r => r is T)) {
                lock(_SyncLock) {
                    if(!_Manipulators.Any(r => r is T)) {
                        var newList = CollectionHelper.ShallowCopy(_Manipulators);
                        var implementation = typeof(T).IsInterface ? (T)Factory.Resolve(typeof(T)) : (T)Activator.CreateInstance<T>();
                        newList.Add(implementation);
                        _Manipulators = newList;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="manipulator"></param>
        public void RemoveTextResponseManipulator(ITextResponseManipulator manipulator)
        {
            lock(_SyncLock) {
                var newList = CollectionHelper.ShallowCopy(_Manipulators);
                newList.Remove(manipulator);
                _Manipulators = newList;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITextResponseManipulator> GetTextResponseManipulators()
        {
            var result = _Manipulators;
            return result;
        }
    }
}
