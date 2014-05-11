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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using System.IO;
using System.Net;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class WebServerSettingsTests
    {
        private WebServerSettings _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = new WebServerSettings();
        }

        [TestMethod]
        public void WebServerSettings_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(_Implementation);
        }

        public static void CheckProperties(WebServerSettings settings)
        {
            Assert.AreEqual(0, settings.BasicAuthenticationUserIds.Count);

            TestUtilities.TestProperty(settings, r => r.AuthenticationScheme, AuthenticationSchemes.Anonymous, AuthenticationSchemes.Basic);
            TestUtilities.TestProperty(settings, r => r.AutoStartUPnP, false);
            TestUtilities.TestProperty(settings, r => r.BasicAuthenticationUser, null, "Bb");
            TestUtilities.TestProperty(settings, r => r.BasicAuthenticationPasswordHash, null, new Hash());
            TestUtilities.TestProperty(settings, r => r.ConvertedUser, false);
            TestUtilities.TestProperty(settings, r => r.EnableUPnp, false);
            TestUtilities.TestProperty(settings, r => r.IsOnlyInternetServerOnLan, true);
            TestUtilities.TestProperty(settings, r => r.UPnpPort, 80, 99);
        }
    }
}
