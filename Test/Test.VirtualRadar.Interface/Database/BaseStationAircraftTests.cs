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
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Database;

namespace Test.VirtualRadar.Interface.Database
{
    [TestClass]
    public class BaseStationAircraftTests
    {
        [TestMethod]
        public void BaseStationAircraft_Initialises_To_Known_State_And_Properties_Work()
        {
            var baseStationAircraft = new BaseStationAircraft();

            TestUtilities.TestProperty(baseStationAircraft, r => r.AircraftClass, null, "Ab");
            TestUtilities.TestProperty(baseStationAircraft, r => r.AircraftID, 0, 129);
            TestUtilities.TestProperty(baseStationAircraft, r => r.CofACategory, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.CofAExpiry, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Country, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.CurrentRegDate, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.DeRegDate, null, "Nn");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Engines, null, "Xx");
            TestUtilities.TestProperty(baseStationAircraft, r => r.FirstCreated, DateTime.MinValue, DateTime.Today);
            TestUtilities.TestProperty(baseStationAircraft, r => r.FirstRegDate, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.GenericName, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.ICAOTypeCode, null, "Uu");
            TestUtilities.TestProperty(baseStationAircraft, r => r.InfoUrl, null, "Uu");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Interested, false);
            TestUtilities.TestProperty(baseStationAircraft, r => r.LastModified, DateTime.MinValue, DateTime.Today);
            TestUtilities.TestProperty(baseStationAircraft, r => r.Manufacturer, null, "Pp");
            TestUtilities.TestProperty(baseStationAircraft, r => r.ModeS, null, "Ii");
            TestUtilities.TestProperty(baseStationAircraft, r => r.ModeSCountry, null, "Ll");
            TestUtilities.TestProperty(baseStationAircraft, r => r.MTOW, null, "Bb");
            TestUtilities.TestProperty(baseStationAircraft, r => r.OperatorFlagCode, null, "Wx");
            TestUtilities.TestProperty(baseStationAircraft, r => r.OwnershipStatus, null, "Ww");
            TestUtilities.TestProperty(baseStationAircraft, r => r.PictureUrl1, null, "Uu");
            TestUtilities.TestProperty(baseStationAircraft, r => r.PictureUrl2, null, "Uu");
            TestUtilities.TestProperty(baseStationAircraft, r => r.PictureUrl3, null, "Uu");
            TestUtilities.TestProperty(baseStationAircraft, r => r.PopularName, null, "Hg");
            TestUtilities.TestProperty(baseStationAircraft, r => r.PreviousID, null, "Ds");
            TestUtilities.TestProperty(baseStationAircraft, r => r.RegisteredOwners, null, "xZ");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Registration, null, "Vv");
            TestUtilities.TestProperty(baseStationAircraft, r => r.SerialNo, null, "EWe");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Status, null, "St");
            TestUtilities.TestProperty(baseStationAircraft, r => r.TotalHours, null, "Jjk");
            TestUtilities.TestProperty(baseStationAircraft, r => r.Type, null, "Jjk");
            TestUtilities.TestProperty(baseStationAircraft, r => r.UserNotes, null, "Jjk");
            TestUtilities.TestProperty(baseStationAircraft, r => r.UserTag, null, "Abc");
            TestUtilities.TestProperty(baseStationAircraft, r => r.YearBuilt, null, "Jjk");
        }

        [TestMethod]
        public void BaseStationAircraft_Equals_Returns_True_When_Comparing_Identical_Objects()
        {
            var aircraft = new BaseStationAircraft();
            Assert.AreEqual(true, aircraft.Equals(aircraft));
        }

        [TestMethod]
        public void BaseStationAircraft_Equals_Returns_False_When_Comparing_Against_Null()
        {
            Assert.AreEqual(false, new BaseStationAircraft().Equals(null));
        }

        [TestMethod]
        public void BaseStationAircraft_Equals_Returns_False_When_Comparing_Against_Different_Object_Type()
        {
            Assert.AreEqual(false, new BaseStationAircraft().Equals("Hello"));
        }

        [TestMethod]
        public void BaseStationAircraft_Equals_Returns_True_When_Comparing_Against_Object_With_Identical_Properties()
        {
            TestUtilities.TestSimpleEquals(typeof(BaseStationAircraft), true);
        }

        [TestMethod]
        public void BaseStationAircraft_Equals_Returns_False_When_Comparing_Against_Object_With_Different_Properties()
        {
            TestUtilities.TestSimpleEquals(typeof(BaseStationAircraft), false);
        }
    }
}
