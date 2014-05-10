// Copyright © 2012 onwards, Andrew Whewell
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

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class RawDecodingSettingsTests
    {
        [TestMethod]
        public void RawDecodingSettings_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(new RawDecodingSettings());
        }

        public static void CheckProperties(RawDecodingSettings settings)
        {
            TestUtilities.TestProperty(settings, r => r.AcceptableAirborneSpeed, 15.0, 11.112);
            TestUtilities.TestProperty(settings, r => r.AcceptableAirSurfaceTransitionSpeed, 5.0, 4.63);
            TestUtilities.TestProperty(settings, r => r.AcceptableSurfaceSpeed, 3.0, 1.4);
            TestUtilities.TestProperty(settings, r => r.AirborneGlobalPositionLimit, 10, 15);
            TestUtilities.TestProperty(settings, r => r.FastSurfaceGlobalPositionLimit, 25, 30);
            TestUtilities.TestProperty(settings, r => r.IgnoreCallsignsInBds20, false);
            TestUtilities.TestProperty(settings, r => r.IgnoreMilitaryExtendedSquitter, false);
            TestUtilities.TestProperty(settings, r => r.ReceiverLocationId, 0, 1);
            TestUtilities.TestProperty(settings, r => r.ReceiverRange, 650, 400);
            TestUtilities.TestProperty(settings, r => r.SlowSurfaceGlobalPositionLimit, 50, 60);
            TestUtilities.TestProperty(settings, r => r.SuppressReceiverRangeCheck, true);
            TestUtilities.TestProperty(settings, r => r.UseLocalDecodeForInitialPosition, false);
            TestUtilities.TestProperty(settings, r => r.AcceptIcaoInPI0Count, 1, 10);
            TestUtilities.TestProperty(settings, r => r.AcceptIcaoInPI0Seconds, 1, 199);
            TestUtilities.TestProperty(settings, r => r.AcceptIcaoInNonPICount, 0, 20);
            TestUtilities.TestProperty(settings, r => r.AcceptIcaoInNonPISeconds, 5, 42);
        }
    }
}
