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
using System.Runtime.CompilerServices;

namespace InterfaceFactory
{
    /// <summary>
    /// The namespace for the class factory.
    /// </summary>
    /// <remarks><para>
    /// Virtual Radar Server has been written on the principle that the classes that make up the source are loosely
    /// coupled - instead of having direct references to each other they have references to interfaces and they do not
    /// know anything about the classes that implement those interfaces.
    /// </para><para>
    /// This library contains a singleton class factory that the executable and all of the libraries have access to. The class
    /// factory can be told which classes implement which interfaces, and it can also create instances of those classes
    /// for the application to use.
    /// </para><para>
    /// The singleton class factory is held by the public static class <see cref="Factory"/>. The property
    /// Singleton exposes the singleton <see cref="IClassFactory"/> class factory.
    /// </para><para>
    /// Virtual Radar Server originally used Microsoft's Unity library to register implementations of interfaces. Unity was replaced
    /// with this library after there was an issue with a particular object. This issue no longer exists so one day the implementation
    /// of <see cref="IClassFactory"/> may be changed to use Unity again. This should not affect any code that is currently using the
    /// class factory.
    /// </para><para>
    /// <img src="../ClassDiagrams/_InterfaceFactory.jpg" alt="" title="InterfaceFactory class diagram"/>
    /// </para></remarks>
    /// <example><para>
    /// This declares an interface and a class that implements it. It shows how the implementation can be registered with the singleton
    /// class factory and how to request a new instance of the class that implements the interface.
    /// </para><para>
    /// Normally the interface would be in one library while the implementation (and the registration of the implementation) would be in
    /// another. The code that requests a new instance of the implementation could be anywhere within the application.
    /// </para><code>
    /// // The public interface that the application will use.
    /// public interface IMyInterface
    /// {
    ///     void DoSomeWork();
    /// }
    ///
    /// // The private class that implements the interface. Nothing that uses this
    /// // class will know anything about it other than it implements IMyInterface.
    /// class PrivateClass : IMyInterface
    /// {
    ///     public void DoSomeWork() { MessageBox.Show("Doing some work"); }
    /// }
    ///
    /// // The call that will tell the singleton class factory that new instances
    /// // of PrivateClass should be created when it is asked for instantiations of
    /// // IMyInterface:
    /// Factory.Singleton.Register&lt;IMyInterface, PrivateClass&gt;();
    /// 
    /// // The call that can be made anywhere within the application to get an
    /// // implementation of IMyInterface. In this case the singleton class factory
    /// // will create a new instance of PrivateClass and return it:
    /// IMyInterface instance = Factory.Singleton.Resolve&lt;IMyInterface&gt;();
    /// instance.DoSomeWork();
    /// </code><para>
    /// </para></example>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
