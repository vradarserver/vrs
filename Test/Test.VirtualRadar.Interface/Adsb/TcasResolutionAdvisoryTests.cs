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
using VirtualRadar.Interface.Adsb;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Adsb
{
    [TestClass]
    public class TcasResolutionAdvisoryTests
    {
        [TestMethod]
        public void TcasResolutionAdvisory_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var message = new TcasResolutionAdvisory();

            TestUtilities.TestProperty(message, m => m.MultipleThreatEncounter, false);
            TestUtilities.TestProperty(message, m => m.MultipleThreatResolutionAdvisory, null, MultipleThreatResolutionAdvisory.RequiresPositiveClimb);
            TestUtilities.TestProperty(message, m => m.ResolutionAdvisoryComplement, (ResolutionAdvisoryComplement)0, ResolutionAdvisoryComplement.DoNotTurnLeft);
            TestUtilities.TestProperty(message, m => m.ResolutionAdvisoryTerminated, false);
            TestUtilities.TestProperty(message, m => m.SingleThreatResolutionAdvisory, null, SingleThreatResolutionAdvisory.IsCorrective);
            TestUtilities.TestProperty(message, m => m.ThreatAltitude, null, 123);
            TestUtilities.TestProperty(message, m => m.ThreatBearing, null, (short)456);
            TestUtilities.TestProperty(message, m => m.ThreatIcao24, 0, 123456);
            TestUtilities.TestProperty(message, m => m.ThreatRange, null, 123F);
            TestUtilities.TestProperty(message, m => m.ThreatRangeExceeded, false);
        }

        [TestMethod]
        public void TcasResolutionAdvisory_FormattedThreatIcao24_Formats_ThreatIcao24_Property()
        {
            var message = new TcasResolutionAdvisory();
            Assert.AreEqual(null, message.FormattedThreatIcao24);

            message.ThreatIcao24 = 0x0000ab;
            Assert.AreEqual("0000AB", message.FormattedThreatIcao24);

            message.ThreatIcao24 = 0x010203;
            Assert.AreEqual("010203", message.FormattedThreatIcao24);

            message.ThreatIcao24 = 0xffffff;
            Assert.AreEqual("FFFFFF", message.FormattedThreatIcao24);

            message.ThreatIcao24 = 0;
            Assert.AreEqual(null, message.FormattedThreatIcao24);

            // The formatting of ICAO24 values larger than 24 bits is undefined, we don't test it.
        }
    }
}
