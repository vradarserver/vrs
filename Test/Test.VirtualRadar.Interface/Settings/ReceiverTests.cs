// Copyright © 2013 onwards, Andrew Whewell
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using Test.Framework;
using System.IO.Ports;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class ReceiverTests
    {
        [TestMethod]
        public void Receiver_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(new Receiver());
        }

        public static void CheckProperties(Receiver settings, bool assumeInitialConfig = false)
        {
            TestUtilities.TestProperty(settings, r => r.Enabled, true);
            TestUtilities.TestProperty(settings, r => r.UniqueId, assumeInitialConfig ? 1 : 0, 2);
            TestUtilities.TestProperty(settings, r => r.Name, assumeInitialConfig ? "Receiver" : null, "ABC");
            TestUtilities.TestProperty(settings, r => r.DataSource, DataSource.Port30003, DataSource.Sbs3);
            TestUtilities.TestProperty(settings, r => r.ConnectionType, ConnectionType.TCP, ConnectionType.COM);
            TestUtilities.TestProperty(settings, r => r.AutoReconnectAtStartup, true);
            TestUtilities.TestProperty(settings, r => r.IsPassive, false);
            TestUtilities.TestProperty(settings, r => r.Address, "127.0.0.1", "VirtualRadar");
            TestUtilities.TestProperty(settings, r => r.Port, 30003, 19000);
            TestUtilities.TestProperty(settings, r => r.UseKeepAlive, true);
            TestUtilities.TestProperty(settings, r => r.IdleTimeoutMilliseconds, 60000, 45000);
            TestUtilities.TestProperty(settings, r => r.ComPort, null, "ABC");
            TestUtilities.TestProperty(settings, r => r.BaudRate, 115200, 2400);
            TestUtilities.TestProperty(settings, r => r.DataBits, 8, 7);
            TestUtilities.TestProperty(settings, r => r.StopBits, StopBits.One, StopBits.None);
            TestUtilities.TestProperty(settings, r => r.Parity, Parity.None, Parity.Odd);
            TestUtilities.TestProperty(settings, r => r.Passphrase, null, "Ab");
            TestUtilities.TestProperty(settings, r => r.Handshake, Handshake.None, Handshake.XOnXOff);
            TestUtilities.TestProperty(settings, r => r.StartupText, "#43-02\\r", "anything");
            TestUtilities.TestProperty(settings, r => r.ShutdownText, "#43-00\\r", "anything");
            TestUtilities.TestProperty(settings, r => r.ReceiverLocationId, 0, 1);
            TestUtilities.TestProperty(settings, r => r.ReceiverUsage, ReceiverUsage.Normal, ReceiverUsage.MergeOnly);
            TestUtilities.TestProperty(settings, r => r.IsSatcomFeed, false);
            TestUtilities.TestProperty(settings, r => r.WebAddress, null, "Abc");
            TestUtilities.TestProperty(settings, r => r.FetchIntervalMilliseconds, 1000, 12000);

            Assert.IsNotNull(settings.Access);
        }
    }
}
