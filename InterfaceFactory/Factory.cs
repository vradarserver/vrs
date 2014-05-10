// Copyright © 2010 onwards, Andrew Whewell
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

namespace InterfaceFactory
{
    /// <summary>
    /// A static class that exposes an instance of an <see cref="IClassFactory"/> that the application can
    /// use to register implementations of interfaces and instantiate those implementations later on.
    /// </summary>
    public static class Factory
    {
        private static IClassFactory _Singleton;
        /// <summary>
        /// Gets the singleton class factory.
        /// </summary>
        public static IClassFactory Singleton
        {
            get { return _Singleton; }
        }

        /// <summary>
        /// Creates the static instance of the factory and registers the default instance of the class factory.
        /// </summary>
        static Factory()
        {
            _Singleton = new ClassFactory();
            _Singleton.Register<IClassFactory, ClassFactory>();
        }

        /// <summary>
        /// Returns a copy of the current <see cref="Singleton"/>. Subsequent type registrations made to <see cref="Singleton"/>
        /// will not affect the copy.
        /// </summary>
        /// <returns>A copy of the current <see cref="Singleton"/>.</returns>
        public static IClassFactory TakeSnapshot()
        {
            IClassFactory result = _Singleton;
            _Singleton = _Singleton.CreateChildFactory();

            return result;
        }

        /// <summary>
        /// Assigns the factory passed across to <see cref="Singleton"/>.
        /// </summary>
        /// <param name="previousFactory">The snapshot that was taken with <see cref="TakeSnapshot"/>.</param>
        public static void RestoreSnapshot(IClassFactory previousFactory)
        {
            _Singleton = previousFactory;
        }
    }
}
