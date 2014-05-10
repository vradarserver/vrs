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
    public class ReportAircraftJsonTests
    {
        [TestMethod]
        public void ReportAircraftJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ReportAircraftJson();

            TestUtilities.TestProperty(json, "AircraftClass", null, "Ab");
            TestUtilities.TestProperty(json, "AircraftId", 0, 1);
            TestUtilities.TestProperty(json, "AirframeReg", null, "Ab");
            TestUtilities.TestProperty(json, "CofACategory", null, "Ab");
            TestUtilities.TestProperty(json, "CofAExpiry", null, "Ab");
            TestUtilities.TestProperty(json, "Country", null, "Ab");
            TestUtilities.TestProperty(json, "CurrentRegDate", null, "Ab");
            TestUtilities.TestProperty(json, "DeRegDate", null, "Ab");
            TestUtilities.TestProperty(json, "Engines", null, "Ab");
            TestUtilities.TestProperty(json, "EngineType", null, 1);
            TestUtilities.TestProperty(json, "FirstRegDate", null, "Ab");
            TestUtilities.TestProperty(json, "GenericName", null, "Ab");
            TestUtilities.TestProperty(json, "HasPicture", false);
            TestUtilities.TestProperty(json, "Icao", null, "Ab");
            TestUtilities.TestProperty(json, "IcaoTypeCode", null, "Ab");
            TestUtilities.TestProperty(json, "InfoUrl", null, "Ab");
            TestUtilities.TestProperty(json, "Interested", false);
            TestUtilities.TestProperty(json, "IsUnknown", false);
            TestUtilities.TestProperty(json, "Manufacturer", null, "Ab");
            TestUtilities.TestProperty(json, "Military", false);
            TestUtilities.TestProperty(json, "ModeSCountry", null, "Ab");
            TestUtilities.TestProperty(json, "MTOW", null, "Ab");
            TestUtilities.TestProperty(json, "Notes", null, "Ab");
            TestUtilities.TestProperty(json, "OperatorFlagCode", null, "Ab");
            TestUtilities.TestProperty(json, "OwnershipStatus", null, "Ab");
            TestUtilities.TestProperty(json, "PictureUrl1", null, "Ab");
            TestUtilities.TestProperty(json, "PictureUrl2", null, "Ab");
            TestUtilities.TestProperty(json, "PictureUrl3", null, "Ab");
            TestUtilities.TestProperty(json, "PictureWidth", 0, 1203);
            TestUtilities.TestProperty(json, "PictureHeight", 0, 9182);
            TestUtilities.TestProperty(json, "PopularName", null, "Ab");
            TestUtilities.TestProperty(json, "PreviousId", null, "Ab");
            TestUtilities.TestProperty(json, "RegisteredOwners", null, "Ab");
            TestUtilities.TestProperty(json, "Registration", null, "Ab");
            TestUtilities.TestProperty(json, "SerialNumber", null, "Ab");
            TestUtilities.TestProperty(json, "Species", null, 1);
            TestUtilities.TestProperty(json, "Status", null, "Ab");
            TestUtilities.TestProperty(json, "TotalHours", null, "Ab");
            TestUtilities.TestProperty(json, "Type", null, "Ab");
            TestUtilities.TestProperty(json, "WakeTurbulenceCategory", null, 1);
            TestUtilities.TestProperty(json, "YearBuilt", null, "Ab");
        }
    }
}
