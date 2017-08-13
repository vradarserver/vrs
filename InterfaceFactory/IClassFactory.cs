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
    /// An interface for objects that implement class factory functionality.
    /// </summary>
    public interface IClassFactory
    {
        /// <summary>
        /// Registers an implementation of an interface. A new instance of the implementation will
        /// be created for each call to Resolve.
        /// </summary>
        /// <typeparam name="TI">The interface type.</typeparam>
        /// <typeparam name="TM">The concrete type that implements TI.</typeparam>
        void Register<TI, TM>();

        /// <summary>
        /// Registers an implementation of an interface. A new instance of the implementation will
        /// be created for each call to Resolve.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="implementationType">The concrete type that implements interfaceType.</param>
        void Register(Type interfaceType, Type implementationType);

        /// <summary>
        /// Registers a method to call to create new instances of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        void Register<T>(Func<T> callBack);

        /// <summary>
        /// Registers an implementation of an interface. The instance passed across will be returned
        /// on each call to Resolve.
        /// </summary>
        /// <typeparam name="TI">The interface type.</typeparam>
        /// <param name="instance">An object whose type implements TI.</param>
        void RegisterInstance<TI>(TI instance);

        /// <summary>
        /// Registers an implementation of an interface. The instance passed across will be returned
        /// on each call to Resolve.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="instance">An object whose type implements interfaceType.</param>
        void RegisterInstance(Type interfaceType, object instance);

        /// <summary>
        /// Returns an object that implements the interface passed across.
        /// </summary>
        /// <typeparam name="T">The interface that the returned object will implement.</typeparam>
        /// <returns>An object that implements T.</returns>
        T Resolve<T>() where T: class;

        /// <summary>
        /// Returns an object that implements the interface passed across.
        /// </summary>
        /// <param name="interfaceType">The interface that the returned object will implement.</param>
        /// <returns>An object that implements interfaceType.</returns>
        object Resolve(Type interfaceType);

        /// <summary>
        /// Returns a new instance of an object that has been marked as a singleton with the <see cref="SingletonAttribute"/>.
        /// </summary>
        /// <typeparam name="T"></param>
        /// <returns></returns>
        /// <remarks>
        /// Attempts to use this to resolve objects that have not been marked as singletons will trigger an exception.
        /// Objects created using this method will not be returned by calls to <see cref="Resolve"/>.
        /// </remarks>
        T ResolveNewInstance<T>() where T: class;

        /// <summary>
        /// Returns a new instance of an object that has been marked as a singleton with the <see cref="SingletonAttribute"/>.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        /// <remarks>
        /// Attempts to use this to resolve objects that have not been marked as singletons will trigger an exception.
        /// Objects created using this method will not be returned by calls to <see cref="Resolve"/>.
        /// </remarks>
        object ResolveNewInstance(Type interfaceType);

        /// <summary>
        /// Returns a copy of the factory.
        /// </summary>
        /// <returns>A copy of the factory with all of the current interface registrations. Changes to the registrations
        /// on the copy will not affect the original, and vice versa.</returns>
        IClassFactory CreateChildFactory();
    }
}
