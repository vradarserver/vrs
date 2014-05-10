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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebSite;
using Test.Framework;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ProximityGadgetClosestAircraftJsonTests
    {
        [TestMethod]
        public void ProximityGadgetClosestAircraftJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ProximityGadgetClosestAircraftJson();

            TestUtilities.TestProperty(json, "Altitude", null, "Abc");
            TestUtilities.TestProperty(json, "BearingFromHere", null, "Abc");
            TestUtilities.TestProperty(json, "Callsign", null, "Abc");
            TestUtilities.TestProperty(json, "Destination", null, "Abc");
            TestUtilities.TestProperty(json, "DistanceFromHere", null, "Abc");
            TestUtilities.TestProperty(json, "Emergency", false);
            TestUtilities.TestProperty(json, "GroundSpeed", null, "Abc");
            TestUtilities.TestProperty(json, "HasPicture", false);
            TestUtilities.TestProperty(json, "Icao24", null, "Abc");
            TestUtilities.TestProperty(json, "Icao24Invalid", false);
            TestUtilities.TestProperty(json, "Latitude", null, "Abc");
            TestUtilities.TestProperty(json, "Longitude", null, "Abc");
            TestUtilities.TestProperty(json, "Manufacturer", null, "Abc");
            TestUtilities.TestProperty(json, "Model", null, "Abc");
            TestUtilities.TestProperty(json, "Operator", null, "Abc");
            TestUtilities.TestProperty(json, "OperatorIcao", null, "Abc");
            TestUtilities.TestProperty(json, "Origin", null, "Abc");
            TestUtilities.TestProperty(json, "Registration", null, "Abc");
            TestUtilities.TestProperty(json, "Squawk", null, "Abc");
            TestUtilities.TestProperty(json, "Track", null, "Abc");
            TestUtilities.TestProperty(json, "Type", null, "Abc");
            TestUtilities.TestProperty(json, "VerticalRate", null, "Abc");
            Assert.AreEqual(0, json.Stopovers.Count);
        }
    }
}
