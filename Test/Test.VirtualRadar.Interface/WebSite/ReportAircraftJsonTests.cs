﻿// Copyright © 2010 onwards, Andrew Whewell
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

            TestUtilities.TestProperty(json, r => r.AircraftClass, null, "Ab");
            TestUtilities.TestProperty(json, r => r.AircraftId, 0, 1);
            TestUtilities.TestProperty(json, "AirframeReg", null, "Ab");        // Tagged obsolete
            TestUtilities.TestProperty(json, r => r.CofACategory, null, "Ab");
            TestUtilities.TestProperty(json, r => r.CofAExpiry, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Country, null, "Ab");
            TestUtilities.TestProperty(json, r => r.CurrentRegDate, null, "Ab");
            TestUtilities.TestProperty(json, r => r.DeRegDate, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Engines, null, "Ab");
            TestUtilities.TestProperty(json, r => r.EngineType, null, 1);
            TestUtilities.TestProperty(json, r => r.FirstRegDate, null, "Ab");
            TestUtilities.TestProperty(json, r => r.GenericName, null, "Ab");
            TestUtilities.TestProperty(json, r => r.HasPicture, false);
            TestUtilities.TestProperty(json, r => r.Icao, null, "Ab");
            TestUtilities.TestProperty(json, r => r.IcaoTypeCode, null, "Ab");
            TestUtilities.TestProperty(json, r => r.InfoUrl, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Interested, false);
            TestUtilities.TestProperty(json, r => r.IsUnknown, false);
            TestUtilities.TestProperty(json, r => r.Manufacturer, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Military, false);
            TestUtilities.TestProperty(json, r => r.ModeSCountry, null, "Ab");
            TestUtilities.TestProperty(json, r => r.MTOW, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Notes, null, "Ab");
            TestUtilities.TestProperty(json, r => r.OperatorFlagCode, null, "Ab");
            TestUtilities.TestProperty(json, r => r.OwnershipStatus, null, "Ab");
            TestUtilities.TestProperty(json, r => r.PictureUrl1, null, "Ab");
            TestUtilities.TestProperty(json, r => r.PictureUrl2, null, "Ab");
            TestUtilities.TestProperty(json, r => r.PictureUrl3, null, "Ab");
            TestUtilities.TestProperty(json, r => r.PictureWidth, 0, 1203);
            TestUtilities.TestProperty(json, r => r.PictureHeight, 0, 9182);
            TestUtilities.TestProperty(json, r => r.PopularName, null, "Ab");
            TestUtilities.TestProperty(json, r => r.PreviousId, null, "Ab");
            TestUtilities.TestProperty(json, r => r.RegisteredOwners, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Registration, null, "Ab");
            TestUtilities.TestProperty(json, r => r.SerialNumber, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Species, null, 1);
            TestUtilities.TestProperty(json, r => r.Status, null, "Ab");
            TestUtilities.TestProperty(json, r => r.TotalHours, null, "Ab");
            TestUtilities.TestProperty(json, r => r.Type, null, "Ab");
            TestUtilities.TestProperty(json, r => r.UserTag, null, "Ab");
            TestUtilities.TestProperty(json, r => r.WakeTurbulenceCategory, null, 1);
            TestUtilities.TestProperty(json, r => r.YearBuilt, null, "Ab");
        }
    }
}