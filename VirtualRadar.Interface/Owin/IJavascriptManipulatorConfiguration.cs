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

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that records the configuration of the Javascript manipulator.
    /// </summary>
    [Singleton]
    public interface IJavascriptManipulatorConfiguration
    {
        /// <summary>
        /// Adds a manipulator that can change the Javascript before it is sent back to the browser.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <remarks>
        /// Repeated adds of the same manipulator object are ignored.
        /// </remarks>
        void AddTextResponseManipulator(ITextResponseManipulator manipulator);

        /// <summary>
        /// Removes a manipulator that had been previously registered.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <remarks>
        /// Repeated removals of the same manipulator object are ignored.
        /// </remarks>
        void RemoveTextResponseManipulator(ITextResponseManipulator manipulator);

        /// <summary>
        /// Returns the text response manipulators that have been registered.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITextResponseManipulator> GetTextResponseManipulators();
    }
}
