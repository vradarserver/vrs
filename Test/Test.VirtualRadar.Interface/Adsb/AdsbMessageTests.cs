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
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Adsb
{
    [TestClass]
    public class AdsbMessageTests
    {
        [TestMethod]
        public void AdsbMessage_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            var modeSMessage = new ModeSMessage();
            var message = new AdsbMessage(modeSMessage);
            Assert.AreSame(modeSMessage, message.ModeSMessage);

            TestUtilities.TestProperty(message, m => m.AirbornePosition, null, new AirbornePositionMessage());
            TestUtilities.TestProperty(message, m => m.AirborneVelocity, null, new AirborneVelocityMessage());
            TestUtilities.TestProperty(message, m => m.IdentifierAndCategory, null, new IdentifierAndCategoryMessage());
            TestUtilities.TestProperty(message, m => m.MessageFormat, MessageFormat.None, MessageFormat.AircraftOperationalStatus);
            TestUtilities.TestProperty(message, m => m.SurfacePosition, null, new SurfacePositionMessage());
            TestUtilities.TestProperty(message, m => m.TargetStateAndStatus, null, new TargetStateAndStatusMessage());
            TestUtilities.TestProperty(message, m => m.TisbIcaoModeAFlag, null, (byte)1);
            TestUtilities.TestProperty(message, m => m.Type, (byte)0, (byte)255);
            TestUtilities.TestProperty(message, m => m.AircraftStatus, null, new AircraftStatusMessage());
            TestUtilities.TestProperty(message, m => m.AircraftOperationalStatus, null, new AircraftOperationalStatusMessage());
            TestUtilities.TestProperty(message, m => m.CoarseTisbAirbornePosition, null, new CoarseTisbAirbornePosition());
        }
    }
}
