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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.WebSite;
using InterfaceFactory;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public static class AssemblyInitialise
    {
        [AssemblyInitialize]
        public static void Initialise(TestContext testContext)
        {
            Implementations.Register(Factory.Singleton);

            // The code relies upon a couple of classes implemented elsewhere. The tests are in effect checking that these implementations
            // are functioning correctly, which on the face of it isn't right. However I am loathe (in the case of the use of IResponder) to
            // say that the public behaviour of IWebSite is that it uses an IResponder under these circumstances and does not use it under
            // the others. I want IWebSite implementations to be free to produce output how they like. For the time being I'll leave that
            // one alone.
            //
            // The reliance upon IAircraftComparer is hard to defend in principle but it's hard to actually test that IAircraftComparer is
            // called correctly. It is invoked by the .NET framework sort methods so we can't really predict how many times it will be called
            // or which parameters will be passed to it. It was more pragmatic to say that we test the output of the sorts rather than attempt
            // to check that .NET's usage of IAircraftComparer looked alright.
            global::VirtualRadar.Library.Implementations.Register(Factory.Singleton);   // <-- need IAircraftComparer defined for sort tests.
            global::VirtualRadar.WebServer.Implementations.Register(Factory.Singleton); // <-- need IResponder defined for response content tests.
        }
    }
}
