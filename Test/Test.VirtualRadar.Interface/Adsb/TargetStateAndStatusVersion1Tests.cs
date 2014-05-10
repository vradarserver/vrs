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
using Test.Framework;
using VirtualRadar.Interface.Adsb;

namespace Test.VirtualRadar.Interface.Adsb
{
    [TestClass]
    public class TargetStateAndStatusVersion1Tests
    {
        [TestMethod]
        public void TargetStateAndStatusVersion1_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var message = new TargetStateAndStatusVersion1();

            TestUtilities.TestProperty(message, m => m.AltitudesAreMeanSeaLevel, false);
            TestUtilities.TestProperty(message, m => m.EmergencyState, EmergencyState.None, EmergencyState.GeneralEmergency);
            TestUtilities.TestProperty(message, m => m.HorizontalDataSource, HorizontalDataSource.None, HorizontalDataSource.Fms);
            TestUtilities.TestProperty(message, m => m.HorizontalModeIndicator, HorizontalModeIndicator.Unknown, HorizontalModeIndicator.Maintaining);
            TestUtilities.TestProperty(message, m => m.NacP, (byte)0, (byte)1);
            TestUtilities.TestProperty(message, m => m.NicBaro, false);
            TestUtilities.TestProperty(message, m => m.Sil, (byte)0, (byte)1);
            TestUtilities.TestProperty(message, m => m.TargetAltitude, null, 12345);
            TestUtilities.TestProperty(message, m => m.TargetAltitudeCapability, TargetAltitudeCapability.HoldingAltitude, TargetAltitudeCapability.HolidingOrAutopilotOrFmsAltitude);
            TestUtilities.TestProperty(message, m => m.TargetHeading, null, (short)1234);
            TestUtilities.TestProperty(message, m => m.TargetHeadingIsTrack, false);
            TestUtilities.TestProperty(message, m => m.TcasCapabilityMode, TcasCapabilityMode.OperationalOrUnknown, TcasCapabilityMode.ResolutionAdvisoryActive);
            TestUtilities.TestProperty(message, m => m.VerticalDataSource, VerticalDataSource.None, VerticalDataSource.Fms);
            TestUtilities.TestProperty(message, m => m.VerticalModeIndicator, VerticalModeIndicator.Unknown, VerticalModeIndicator.Maintaining);
        }
    }
}
