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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Declares that the interface this is implemented by is intended to be used as a singleton.
    /// </summary>
    /// <typeparam name="T">The interface of the singleton object.</typeparam>
    /// <remarks><para>
    /// All interfaces that are intended to be used as a singleton (i.e. only one instance of them should be used across the
    /// entire application) implement this interface. This has a few advantages - it makes the use of a singleton obvious
    /// when reading the code, which hopefully avoids the accidental disposal of a singleton object, and the use of singletons
    /// can be detected by the unit tests which can then in turn automatically check that mocks of singleton objects are used
    /// correctly.
    /// </para><para>
    /// The drawback with this approach is that you must create an instance of the class before you can access the
    /// <see cref="Singleton"/> property. This instance is unused and so it represents a waste of time and memory. Further you
    /// might accidentally use the instance instead of its Singleton, which could produce some hard-to-track-down bugs.
    /// </para><para>
    /// An alternate approach would have been to use the facilities in the class factory to register singleton instances of
    /// an interface, which would not need a special interface. This would have worked but presents its own set of problems,
    /// the most tricky of all being how you handle singletons that are disposable. If you use the class factory approach then
    /// you can't tell whether you getting a unique instance of an object or a singleton instance when you resolve an interface,
    /// so you always call Dispose and hope the interface's implementation doesn't do anything until the Dispose call is made
    /// during program shutdown. This can complicate the implementation of the interface. Using the ISingleton&lt;&gt; approach
    /// you can say that all objects created by the class factory are distinct instances, you can assume that it will not return
    /// singletons, and if you ask it for a disposable object you will need to dispose of it when you've finished using it.
    /// </para></remarks>
    /// <example>
    /// This is the normal pattern for accessing an interface that implements ISingleton:
    /// <code>
    /// ILog log = Factory.Singleton.Resolve&lt;ILog&gt;().Singleton;
    /// log.WriteLine("This will appear in the log");
    /// </code>
    /// This is the normal pattern for creating the singleton object in an implementation:
    /// <code>
    /// public interface IMyInterface : ISingleton&lt;IMyInterface&gt;
    /// {
    /// }
    /// 
    /// class Implementation : IMyInterface
    /// {
    ///     private static readonly IMyInterface _Singleton = new Implementation();
    ///     public IMyInterface Singleton
    ///     {
    ///         get { return _Singleton; }
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface ISingleton<T>
    {
        /// <summary>
        /// Gets the single instance of the class that should be used throughout the application.
        /// </summary>
        T Singleton { get; }
    }
}
