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
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Library;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class AircraftComparerTests
    {
        #region TestContext, fields, TestInitialise
        public TestContext TestContext { get; set; }

        private IAircraftComparer _Comparer;
        private IAircraft _Lhs;
        private IAircraft _Rhs;

        [TestInitialize]
        public void TestInitialise()
        {
            _Comparer = Factory.Resolve<IAircraftComparer>();

            _Lhs = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _Rhs = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
        }
        #endregion

        #region InitialiseAircraft
        /// <summary>
        /// Sets the appropriate column on the aircraft passed across to either a high or low value, depending on the parameters.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="columnName"></param>
        /// <param name="setLowValue"></param>
        private void InitialiseAircraft(IAircraft aircraft, string columnName, bool setLowValue)
        {
            switch(columnName) {
                case AircraftComparerColumn.Altitude:               aircraft.Altitude = setLowValue ? 100 : 200; break;
                case AircraftComparerColumn.Callsign:               aircraft.Callsign = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Destination:            aircraft.Destination = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.DistanceFromHere:       aircraft.Latitude = aircraft.Longitude = setLowValue ? 1F : 2F; break;
                case AircraftComparerColumn.FirstSeen:              aircraft.FirstSeen = setLowValue ? new DateTime(2001, 1, 2, 10, 20, 21, 100) : new DateTime(2001, 1, 2, 10, 20, 21, 200); break;
                case AircraftComparerColumn.FlightsCount:           aircraft.FlightsCount = setLowValue ? 100 : 200; break;
                case AircraftComparerColumn.GroundSpeed:            aircraft.GroundSpeed = setLowValue ? 100 : 200; break;
                case AircraftComparerColumn.Icao24:                 aircraft.Icao24 = setLowValue ? "123456" : "ABCDEF"; break;
                case AircraftComparerColumn.Icao24Country:          aircraft.Icao24Country = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Model:                  aircraft.Model = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.NumberOfEngines:        aircraft.NumberOfEngines = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Operator:               aircraft.Operator = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.OperatorIcao:           aircraft.OperatorIcao = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Origin:                 aircraft.Origin = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Registration:           aircraft.Registration = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.Species:                aircraft.Species = setLowValue ? Species.Amphibian : Species.Landplane; break;
                case AircraftComparerColumn.Squawk:                 aircraft.Squawk = setLowValue ? 5 : 5000; break;
                case AircraftComparerColumn.Type:                   aircraft.Type = setLowValue ? "ABC" : "XYZ"; break;
                case AircraftComparerColumn.VerticalRate:           aircraft.VerticalRate = setLowValue ? 100 : 200; break;
                case AircraftComparerColumn.WakeTurbulenceCategory: aircraft.WakeTurbulenceCategory = setLowValue ? WakeTurbulenceCategory.Light : WakeTurbulenceCategory.Heavy; break;
                default:                                            throw new NotImplementedException();
            }
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void AircraftComparer_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0, _Comparer.SortBy.Count);
            Assert.AreEqual(0, _Comparer.PrecalculatedDistances.Count);
            TestUtilities.TestProperty(_Comparer, "BrowserLocation", null, new Coordinate(1, 2));
        }
        #endregion

        #region Compare
        [TestMethod]
        public void AircraftComparer_Compare_Returns_Correct_Sort_Order_On_Single_Column()
        {
            foreach(var columnField in typeof(AircraftComparerColumn).GetFields()) {
                var columnName = (string)columnField.GetValue(null);

                for(int sortOrder = 0;sortOrder < 2;++sortOrder) {
                    TestInitialise();

                    _Comparer.BrowserLocation = new Coordinate(0, 0);

                    bool sortAscending = sortOrder == 0;
                    _Comparer.SortBy.Add(new KeyValuePair<string,bool>(columnName, sortAscending));

                    var nullAircraft1 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
                    var nullAircraft2 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;

                    var lowAircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
                    var highAircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
                    var lowAgain = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;

                    InitialiseAircraft(lowAircraft, columnName, true);
                    InitialiseAircraft(highAircraft, columnName, false);
                    InitialiseAircraft(lowAgain, columnName, true);

                    Assert.AreEqual(0, _Comparer.Compare(nullAircraft1, nullAircraft2), columnName);
                    Assert.AreEqual(0, _Comparer.Compare(lowAircraft, lowAircraft), columnName);
                    Assert.AreEqual(0, _Comparer.Compare(highAircraft, highAircraft), columnName);
                    Assert.AreEqual(0, _Comparer.Compare(lowAircraft, lowAgain), columnName);

                    if(sortAscending) {
                        Assert.IsTrue(_Comparer.Compare(lowAircraft, highAircraft) < 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(highAircraft, lowAircraft) > 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(nullAircraft1, lowAircraft) < 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(lowAircraft, nullAircraft1) > 0, columnName);
                    } else {
                        Assert.IsTrue(_Comparer.Compare(lowAircraft, highAircraft) > 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(highAircraft, lowAircraft) < 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(nullAircraft1, lowAircraft) > 0, columnName);
                        Assert.IsTrue(_Comparer.Compare(lowAircraft, nullAircraft1) < 0, columnName);
                    }
                }
            }
        }

        [TestMethod]
        public void AircraftComparer_Compare_Returns_Correct_Sort_Order_On_Many_Columns()
        {
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.Registration, true));
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.Altitude, false));

            _Lhs.Registration = _Rhs.Registration = null;

            _Lhs.Altitude = 10000;
            _Rhs.Altitude = 20000;
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) > 0);

            _Rhs.Registration = "G-VWEB";
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) < 0);
        }

        [TestMethod]
        public void AircraftComparer_Compare_Copes_When_Comparing_Distances_For_Unknown_Browser_Location()
        {
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.DistanceFromHere, true));
            _Comparer.BrowserLocation = null;

            _Lhs.Latitude = _Lhs.Longitude = 1;
            _Rhs.Latitude = _Rhs.Longitude = 2;

            Assert.AreEqual(0, _Comparer.Compare(_Lhs, _Rhs));
        }

        [TestMethod]
        public void AircraftComparer_Compare_Treats_Unknown_Sort_Columns_As_FirstSeen()
        {
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>("THIS IS GIBBERISH", true));
            _Comparer.BrowserLocation = null;

            _Lhs.FirstSeen = new DateTime(2001, 12, 31);
            _Rhs.FirstSeen = new DateTime(1999, 12, 31);

            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) > 0);
            Assert.IsTrue(_Comparer.Compare(_Rhs, _Lhs) < 0);
        }

        [TestMethod]
        public void AircraftComparer_Compare_Uses_PrecalculatedDistances_When_Available()
        {
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.DistanceFromHere, true));
            _Comparer.BrowserLocation = new Coordinate(0, 0);

            _Lhs.UniqueId = 1;
            _Rhs.UniqueId = 2;

            _Lhs.Latitude = _Lhs.Longitude = 1;  // about 157 km
            _Rhs.Latitude = _Rhs.Longitude = 2;  // about 314 km

            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) < 0);

            _Comparer.PrecalculatedDistances.Add(_Rhs.UniqueId, 100);
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) > 0);

            _Comparer.PrecalculatedDistances.Add(_Lhs.UniqueId, null);
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) < 0);

            _Comparer.PrecalculatedDistances[_Rhs.UniqueId] = null;
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) == 0);
        }

        [TestMethod]
        public void AircraftComparer_Compare_NumberOfEngines_Sorts_By_Engine_Type_When_Equal()
        {
            _Comparer.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.NumberOfEngines, true));

            _Lhs.EngineType = EngineType.Electric;
            _Rhs.EngineType = EngineType.Jet;

            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) < 0);
            Assert.IsTrue(_Comparer.Compare(_Rhs, _Lhs) > 0);

            _Comparer.SortBy[0] = new KeyValuePair<string,bool>(AircraftComparerColumn.NumberOfEngines, false);
            Assert.IsTrue(_Comparer.Compare(_Lhs, _Rhs) > 0);
            Assert.IsTrue(_Comparer.Compare(_Rhs, _Lhs) < 0);

            _Rhs.EngineType = EngineType.Electric;
            Assert.AreEqual(0, _Comparer.Compare(_Lhs, _Rhs));
        }
        #endregion
    }
}
