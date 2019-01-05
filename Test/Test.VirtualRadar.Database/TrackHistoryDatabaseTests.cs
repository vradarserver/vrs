// Copyright © 2018 onwards, Andrew Whewell
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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    public class TrackHistoryDatabaseTests<T>
        where T : ITrackHistoryDatabase
    {
        public TestContext TestContext { get; set; }

        protected T _Database;
        protected Func<IDbConnection> _CreateConnection;
        protected Action<T> _InitialiseImplementation;
        protected string _SqlReturnNewIdentity;
        protected ClockMock _Clock;

        protected IClassFactory _Snapshot;

        protected void CommonTestInitialise(Action initialiseDatabase, Func<IDbConnection> createConnection, Action<T> initialiseImplementation, string sqlReturnNewIdentity)
        {
            _Snapshot = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            initialiseDatabase?.Invoke();
            _CreateConnection = createConnection;
            _SqlReturnNewIdentity = sqlReturnNewIdentity;

            _InitialiseImplementation = initialiseImplementation;
            EstablishDatabaseImplementation();
        }

        protected void EstablishDatabaseImplementation()
        {
            var implementation = (T)Factory.Resolve(typeof(T));
            _Database = implementation;
            _InitialiseImplementation?.Invoke(implementation);
        }

        protected void CommonTestCleanup()
        {
            if(_Database != null) {
                _Database = default(T);
            }

            Factory.RestoreSnapshot(_Snapshot);
        }

        private MethodInfo _TestInitialise;
        private void RunTestInitialise()
        {
            if(_TestInitialise == null) {
                _TestInitialise = GetType().GetMethods().Single(r => r.GetCustomAttributes(typeof(TestInitializeAttribute), inherit: false).Length != 0);
            }
            _TestInitialise.Invoke(this, new object[0]);
        }

        private MethodInfo _TestCleanup;
        private void RunTestCleanup()
        {
            if(_TestCleanup == null) {
                _TestCleanup = GetType().GetMethods().Single(r => r.GetCustomAttributes(typeof(TestCleanupAttribute), inherit: false).Length != 0);
            }
            _TestCleanup.Invoke(this, new object[0]);
        }

        #region Aircraft
        protected void Aircraft_Save_Creates_New_Aircraft_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var aircraft in SampleAircraft(true, created, updated)) {
                _Database.Aircraft_Save(aircraft);

                Assert.AreNotEqual(0, aircraft.AircraftID);

                var readBack = _Database.Aircraft_GetByID(aircraft.AircraftID);
                AssertAircraftAreEqual(aircraft, readBack);
            }
        }

        protected void Aircraft_Save_Updates_Existing_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddSeconds(9);

            var allSavedAircraft =   SampleAircraft(generateForCreate: true,  createdUtc: created, updatedUtc: created);
            var allUpdatedAircraft = SampleAircraft(generateForCreate: false, createdUtc: created, updatedUtc: updated);

            foreach(var aircraft in allSavedAircraft) {
                _Database.Aircraft_Save(aircraft);
            }

            for(var i = 0;i < allSavedAircraft.Count;++i) {
                var savedAircraft = allSavedAircraft[i];
                var updatedAircraft = allUpdatedAircraft[i];

                updatedAircraft.AircraftID = savedAircraft.AircraftID;
                _Database.Aircraft_Save(updatedAircraft);

                var readBack = _Database.Aircraft_GetByID(savedAircraft.AircraftID);
                AssertAircraftAreEqual(updatedAircraft, readBack);
            }
        }

        protected void Aircraft_GetByIcao_Returns_Aircraft_For_Icao()
        {
            foreach(var aircraft in SampleAircraft(true)) {
                _Database.Aircraft_Save(aircraft);

                var readBack = _Database.Aircraft_GetByIcao(aircraft.Icao);
                AssertAircraftAreEqual(aircraft, readBack);
            }
        }

        protected void Aircraft_GetByIcao_Is_Case_Insensitive()
        {
            foreach(var aircraft in SampleAircraft(true)) {
                _Database.Aircraft_Save(aircraft);

                var flippedIcao = TestUtilities.FlipCase(aircraft.Icao);

                var readBack = _Database.Aircraft_GetByIcao(flippedIcao);
                AssertAircraftAreEqual(aircraft, readBack);
            }
        }

        protected void Aircraft_Delete_Deletes_Aircraft()
        {
            foreach(var aircraft in SampleAircraft(true)) {
                _Database.Aircraft_Save(aircraft);

                var id = aircraft.AircraftID;
                _Database.Aircraft_Delete(aircraft);

                var readBack = _Database.Aircraft_GetByID(id);
                Assert.IsNull(readBack);
            }
        }

        protected void Aircraft_Delete_Removes_Child_TrackHistories()
        {
            var trackHistory = SampleTrackHistories().First();
            _Database.TrackHistory_Save(trackHistory);

            // Check that states are removed as well as histories
            var state = new TrackHistoryState() {
                TrackHistoryID = trackHistory.TrackHistoryID,
                TimestampUtc =   DateTime.UtcNow,
                SequenceNumber = 1,
            };
            _Database.TrackHistoryState_Save(state);

            var aircraftID = trackHistory.AircraftID;
            var aircraft = _Database.Aircraft_GetByID(aircraftID);
            _Database.Aircraft_Delete(aircraft);

            var readBackState = _Database.TrackHistoryState_GetByID(state.TrackHistoryStateID);
            Assert.IsNull(readBackState);

            var readBackHistory = _Database.TrackHistory_GetByID(trackHistory.TrackHistoryID);
            Assert.IsNull(readBackHistory);
        }

        private List<TrackHistoryAircraft> SampleAircraft(bool generateForCreate, DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            var result = new List<TrackHistoryAircraft>();

            var previousIcao = 0;
            foreach(var property in typeof(TrackHistoryAircraft).GetProperties()) {
                var aircraft = new TrackHistoryAircraft() {
                    Icao =          (++previousIcao).ToString("X6"),
                    CreatedUtc =    created,
                    UpdatedUtc =    updated,
                };

                var propertyValue = GenerateAircraftPropertyValue(property, generateForCreate);
                if(propertyValue != null) {
                    property.SetValue(aircraft, propertyValue);
                    result.Add(aircraft);
                }
            }

            return result;
        }

        private object GenerateAircraftPropertyValue(PropertyInfo property, bool generateForCreate)
        {
            object value = null;

            var countryName = generateForCreate ? "Faerûn" : "Tyr";
            var modelIcao = generateForCreate ? "A319" : "B744";

            switch(property.Name) {
                case nameof(TrackHistoryAircraft.AircraftTypeID):
                    // At time of writing I only have AircraftType_Save and _GetByID... when higher-level creation methods are added I can simplify this
                    var aircraftType = new TrackHistoryAircraftType() { Icao = modelIcao, CreatedUtc = DateTime.UtcNow, UpdatedUtc = DateTime.UtcNow };
                    _Database.AircraftType_Save(aircraftType);
                    value = aircraftType.AircraftTypeID;
                    break;
                case nameof(TrackHistoryAircraft.IcaoCountryID):    value = _Database.Country_GetOrCreateByName(countryName).CountryID; break;
                case nameof(TrackHistoryAircraft.Notes):            value = generateForCreate ? new String('Ă', 2000) : new string('b', 2000); break;
                case nameof(TrackHistoryAircraft.Registration):     value = generateForCreate ? new String('A', 20)   : new string('b', 20); break;
                case nameof(TrackHistoryAircraft.Serial):           value = generateForCreate ? new String('Ă', 200)  : new string('b', 200); break;

                case nameof(TrackHistoryAircraft.LastLookupUtc):
                    value = generateForCreate ? new DateTime(2019, 2, 1, 17, 16, 15, 143) : new DateTime(2009, 8, 7, 6, 5, 4, 321);
                    break;
                case nameof(TrackHistoryAircraft.YearBuilt):
                    value = generateForCreate ? 25 : 50;
                    break;
                case nameof(TrackHistoryAircraft.IsInteresting):
                case nameof(TrackHistoryAircraft.IsMissingFromLookup):
                case nameof(TrackHistoryAircraft.SuppressAutoUpdates):
                    value = generateForCreate;
                    break;

                case nameof(TrackHistoryAircraft.AircraftID):
                case nameof(TrackHistoryAircraft.CreatedUtc):
                case nameof(TrackHistoryAircraft.Icao):
                case nameof(TrackHistoryAircraft.UpdatedUtc):
                    break;

                default:
                    throw new NotImplementedException($"Need code for {nameof(TrackHistoryAircraft)}.{property.Name}");
            }

            return value;
        }

        private void AssertAircraftAreEqual(TrackHistoryAircraft expected, TrackHistoryAircraft actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region AircraftType
        protected void AircraftType_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var aircraftType in SampleAircraftTypes(true, created, updated)) {
                _Database.AircraftType_Save(aircraftType);

                Assert.AreNotEqual(0, aircraftType.AircraftTypeID);

                var readBack = _Database.AircraftType_GetByID(aircraftType.AircraftTypeID);
                AssertAircraftTypesAreEqual(aircraftType, readBack);
            }
        }

        protected void AircraftType_Save_Updates_Existing_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddSeconds(9);

            var allSavedAircraftTypes =   SampleAircraftTypes(generateForCreate: true,  createdUtc: created, updatedUtc: created);
            var allUpdatedAircraftTypes = SampleAircraftTypes(generateForCreate: false, createdUtc: created, updatedUtc: updated);

            foreach(var aircraftType in allSavedAircraftTypes) {
                _Database.AircraftType_Save(aircraftType);
            }

            for(var i = 0;i < allSavedAircraftTypes.Count;++i) {
                var savedAircraftType = allSavedAircraftTypes[i];
                var updatedAircraftType = allUpdatedAircraftTypes[i];

                updatedAircraftType.AircraftTypeID = savedAircraftType.AircraftTypeID;
                _Database.AircraftType_Save(updatedAircraftType);

                var readBack = _Database.AircraftType_GetByID(savedAircraftType.AircraftTypeID);
                AssertAircraftTypesAreEqual(updatedAircraftType, readBack);
            }
        }

        private List<TrackHistoryAircraftType> SampleAircraftTypes(bool generateForCreate, DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            var result = new List<TrackHistoryAircraftType>();

            var previousIcao = 0;
            foreach(var property in typeof(TrackHistoryAircraftType).GetProperties()) {
                var aircraftType = new TrackHistoryAircraftType() {
                    Icao =          $"A{++previousIcao:X3}",
                    CreatedUtc =    created,
                    UpdatedUtc =    updated,
                };

                var propertyValue = GenerateAircraftTypePropertyValue(property, generateForCreate);
                if(propertyValue != null) {
                    property.SetValue(aircraftType, propertyValue);
                    result.Add(aircraftType);
                }
            }

            return result;
        }

        private object GenerateAircraftTypePropertyValue(PropertyInfo property, bool generateForCreate)
        {
            object value = null;

            var manufacturerName = generateForCreate ? "Airbus" : "Boeing";
            var modelName = generateForCreate ? "A320" : "B744";

            switch(property.Name) {
                case nameof(TrackHistoryAircraftType.EngineCount):              value = generateForCreate ? "2" : "4"; break;
                case nameof(TrackHistoryAircraftType.EnginePlacementID):        value = generateForCreate ? EnginePlacement.AftMounted : EnginePlacement.FuselageBuried; break;
                case nameof(TrackHistoryAircraftType.EngineTypeID):             value = generateForCreate ? EngineType.Electric : EngineType.Jet; break;
                case nameof(TrackHistoryAircraftType.ManufacturerID):           value = _Database.Manufacturer_GetOrCreateByName(manufacturerName).ManufacturerID; break;
                case nameof(TrackHistoryAircraftType.ModelID):                  value = _Database.Model_GetOrCreateByName(manufacturerName).ModelID; break;
                case nameof(TrackHistoryAircraftType.WakeTurbulenceCategoryID): value = generateForCreate ? WakeTurbulenceCategory.Medium : WakeTurbulenceCategory.Heavy; break;

                case nameof(TrackHistoryAircraftType.AircraftTypeID):
                case nameof(TrackHistoryAircraftType.CreatedUtc):
                case nameof(TrackHistoryAircraftType.Icao):
                case nameof(TrackHistoryAircraftType.UpdatedUtc):
                    break;

                default:
                    throw new NotImplementedException($"Need code for {nameof(TrackHistoryAircraftType)}.{property.Name}");
            }

            return value;
        }

        private void AssertAircraftTypesAreEqual(TrackHistoryAircraftType expected, TrackHistoryAircraftType actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region Country
        protected void Country_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var country in SampleCountries(created, updated)) {
                _Database.Country_Save(country);

                Assert.AreNotEqual(0, country.CountryID);

                var readBack = _Database.Country_GetByID(country.CountryID);
                AssertCountriesAreEqual(country, readBack);
            }
        }

        protected void Country_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createCountrys = SampleCountries(now, now);
            foreach(var country in createCountrys) {
                _Database.Country_Save(country);
            }

            var updatedTime = now.AddSeconds(7);
            var updateCountrys = SampleCountries(now, updatedTime);
            for(var i = 0;i < updateCountrys.Length;++i) {
                var createCountry = createCountrys[i];
                var updateCountry = updateCountrys[i];

                var expectedName = new String(createCountry.Name.Reverse().ToArray());

                updateCountry.CountryID = createCountry.CountryID;
                updateCountry.Name = expectedName;

                _Database.Country_Save(updateCountry);

                Assert.AreEqual(createCountry.CountryID, updateCountry.CountryID);
                Assert.AreEqual(expectedName,              updateCountry.Name);
                Assert.AreEqual(createCountry.CreatedUtc, updateCountry.CreatedUtc);
                Assert.AreEqual(updatedTime,               updateCountry.UpdatedUtc);

                var readBack = _Database.Country_GetByID(updateCountry.CountryID);
                AssertCountriesAreEqual(updateCountry, readBack);
            }
        }

        protected void Country_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            foreach(var country in SampleCountries()) {
                _Database.Country_Save(country);

                var sameCaseReadBack = _Database.Country_GetByName(country.Name);
                AssertCountriesAreEqual(country, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(country.Name);
                var flippedCaseReadBack = _Database.Country_GetByName(flippedName);
                AssertCountriesAreEqual(country, flippedCaseReadBack);
            }
        }

        protected void Country_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            _Clock.UtcNowValue = now;

            foreach(var name in SampleCountries().Select(r => r.Name)) {
                var saved = _Database.Country_GetOrCreateByName(name);

                Assert.AreNotEqual(0, saved.CountryID);
                Assert.AreEqual(now, saved.CreatedUtc);
                Assert.AreEqual(now, saved.UpdatedUtc);

                var readBack = _Database.Country_GetByID(saved.CountryID);
                AssertCountriesAreEqual(saved, readBack);
            }
        }

        protected void Country_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            foreach(var country in SampleCountries()) {
                _Database.Country_Save(country);

                var sameCaseReadBack = _Database.Country_GetOrCreateByName(country.Name);
                AssertCountriesAreEqual(country, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(country.Name);
                var flippedCaseReadBack = _Database.Country_GetOrCreateByName(flippedName);
                AssertCountriesAreEqual(country, flippedCaseReadBack);
            }
        }

        protected void Country_Delete_Deletes_Countries()
        {
            foreach(var country in SampleCountries()) {
                _Database.Country_Save(country);

                var id = country.CountryID;
                _Database.Country_Delete(country);

                var readBack = _Database.Country_GetByID(id);
                Assert.IsNull(readBack);
            }
        }

        protected void Country_Delete_Nulls_Out_References_To_Country()
        {
            var aircraft = SampleAircraft(true).First(r => r.IcaoCountryID != null);
            _Database.Aircraft_Save(aircraft);

            var country = _Database.Country_GetByID(aircraft.IcaoCountryID.Value);
            Assert.IsNotNull(country);

            _Database.Country_Delete(country);

            var readBackAircraft = _Database.Aircraft_GetByID(aircraft.AircraftID);
            Assert.IsNull(readBackAircraft.IcaoCountryID);
        }

        protected void Country_Delete_Ignores_Deleted_Countries()
        {
            var doesNotExist = new TrackHistoryCountry() {
                CountryID =  1,
                Name =       "sqrt(-1)",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
            };

            // This just needs to not throw an exception
            _Database.Country_Delete(doesNotExist);
        }

        private TrackHistoryCountry[] SampleCountries(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistoryCountry[] {
                new TrackHistoryCountry() { Name = "Temperance Building", CreatedUtc = created, UpdatedUtc = updated, },
                new TrackHistoryCountry() { Name = "Poacher's Hill",      CreatedUtc = created, UpdatedUtc = updated, },
            };
        }

        private void AssertCountriesAreEqual(TrackHistoryCountry expected, TrackHistoryCountry actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region EnginePlacement
        public void EnginePlacement_GetByID_Returns_Correct_EnginePlacement()
        {
            foreach(EnginePlacement enumEnginePlacement in Enum.GetValues(typeof(EnginePlacement))) {
                var dbEnginePlacement = _Database.EnginePlacement_GetByID(enumEnginePlacement);
                if(enumEnginePlacement == EnginePlacement.Unknown) {
                    Assert.IsNull(dbEnginePlacement);
                } else {
                    AssertEnginePlacementIsCorrect(enumEnginePlacement, dbEnginePlacement);
                }
            }
        }

        public void EnginePlacement_GetAll_Returns_All_EnginePlacement()
        {
            var allEnginePlacement = Enum.GetValues(typeof(EnginePlacement))
                .OfType<EnginePlacement>()
                .Where(r => r != EnginePlacement.Unknown)
                .ToArray();
            var actual = _Database.EnginePlacement_GetAll().ToArray();
            Assert.AreEqual(allEnginePlacement.Length, actual.Length);

            foreach(EnginePlacement enumEnginePlacement in allEnginePlacement) {
                AssertEnginePlacementIsCorrect(enumEnginePlacement, actual.Single(r => r.EnginePlacementID == enumEnginePlacement));
            }
        }

        private static void AssertEnginePlacementIsCorrect(EnginePlacement enumEnginePlacement, TrackHistoryEnginePlacement dbEnginePlacement)
        {
            Assert.AreEqual(enumEnginePlacement, dbEnginePlacement.EnginePlacementID);

            var expectedCode = "";
            var expectedDesc = "";

            switch(enumEnginePlacement) {
                case EnginePlacement.AftMounted:        expectedCode = "AM"; expectedDesc = "Aft Mounted"; break;
                case EnginePlacement.FuselageBuried:    expectedCode = "FB"; expectedDesc = "Fuselage Buried"; break;
                case EnginePlacement.NoseMounted:       expectedCode = "NM"; expectedDesc = "Nose Mounted"; break;
                case EnginePlacement.WingBuried:        expectedCode = "WB"; expectedDesc = "Wing Buried"; break;
                case EnginePlacement.WingMounted:       expectedCode = "WM"; expectedDesc = "Wing Mounted"; break;
                default: throw new NotImplementedException($"Need code for {enumEnginePlacement}");
            }

            Assert.AreEqual(expectedCode, dbEnginePlacement.Code);
            Assert.AreEqual(expectedDesc, dbEnginePlacement.Description);
        }
        #endregion

        #region EngineType
        public void EngineType_GetByID_Returns_Correct_EngineType()
        {
            foreach(EngineType enumEngineType in Enum.GetValues(typeof(EngineType))) {
                var dbEngineType = _Database.EngineType_GetByID(enumEngineType);
                if(enumEngineType == EngineType.None) {
                    Assert.IsNull(dbEngineType);
                } else {
                    AssertEngineTypeIsCorrect(enumEngineType, dbEngineType);
                }
            }
        }

        public void EngineType_GetAll_Returns_All_EngineType()
        {
            var allEngineType = Enum.GetValues(typeof(EngineType))
                .OfType<EngineType>()
                .Where(r => r != EngineType.None)
                .ToArray();
            var actual = _Database.EngineType_GetAll().ToArray();
            Assert.AreEqual(allEngineType.Length, actual.Length);

            foreach(EngineType enumEngineType in allEngineType) {
                AssertEngineTypeIsCorrect(enumEngineType, actual.Single(r => r.EngineTypeID == enumEngineType));
            }
        }

        private static void AssertEngineTypeIsCorrect(EngineType enumEngineType, TrackHistoryEngineType dbEngineType)
        {
            Assert.AreEqual(enumEngineType, dbEngineType.EngineTypeID);

            var expectedCode = "";
            var expectedDesc = "";

            switch(enumEngineType) {
                case EngineType.Electric:   expectedCode = "E"; expectedDesc = "Electric"; break;
                case EngineType.Jet:        expectedCode = "J"; expectedDesc = "Jet"; break;
                case EngineType.Piston:     expectedCode = "P"; expectedDesc = "Piston"; break;
                case EngineType.Rocket:     expectedCode = "R"; expectedDesc = "Rocket"; break;
                case EngineType.Turboprop:  expectedCode = "T"; expectedDesc = "Turboprop"; break;
                default: throw new NotImplementedException($"Need code for {enumEngineType}");
            }

            Assert.AreEqual(expectedCode, dbEngineType.Code);
            Assert.AreEqual(expectedDesc, dbEngineType.Description);
        }
        #endregion

        #region Manufacturer
        protected void Manufacturer_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var manufacturer in SampleManufacturers(created, updated)) {
                _Database.Manufacturer_Save(manufacturer);

                Assert.AreNotEqual(0, manufacturer.ManufacturerID);

                var readBack = _Database.Manufacturer_GetByID(manufacturer.ManufacturerID);
                AssertManufacturersAreEqual(manufacturer, readBack);
            }
        }

        protected void Manufacturer_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createManufacturers = SampleManufacturers(now, now);
            foreach(var manufacturer in createManufacturers) {
                _Database.Manufacturer_Save(manufacturer);
            }

            var updatedTime = now.AddSeconds(7);
            var updateManufacturers = SampleManufacturers(now, updatedTime);
            for(var i = 0;i < updateManufacturers.Length;++i) {
                var createManufacturer = createManufacturers[i];
                var updateManufacturer = updateManufacturers[i];

                var expectedName = new String(createManufacturer.Name.Reverse().ToArray());

                updateManufacturer.ManufacturerID = createManufacturer.ManufacturerID;
                updateManufacturer.Name = expectedName;

                _Database.Manufacturer_Save(updateManufacturer);

                Assert.AreEqual(createManufacturer.ManufacturerID, updateManufacturer.ManufacturerID);
                Assert.AreEqual(expectedName,              updateManufacturer.Name);
                Assert.AreEqual(createManufacturer.CreatedUtc, updateManufacturer.CreatedUtc);
                Assert.AreEqual(updatedTime,               updateManufacturer.UpdatedUtc);

                var readBack = _Database.Manufacturer_GetByID(updateManufacturer.ManufacturerID);
                AssertManufacturersAreEqual(updateManufacturer, readBack);
            }
        }

        protected void Manufacturer_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            foreach(var manufacturer in SampleManufacturers()) {
                _Database.Manufacturer_Save(manufacturer);

                var sameCaseReadBack = _Database.Manufacturer_GetByName(manufacturer.Name);
                AssertManufacturersAreEqual(manufacturer, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(manufacturer.Name);
                var flippedCaseReadBack = _Database.Manufacturer_GetByName(flippedName);
                AssertManufacturersAreEqual(manufacturer, flippedCaseReadBack);
            }
        }

        protected void Manufacturer_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            _Clock.UtcNowValue = now;

            foreach(var name in SampleManufacturers().Select(r => r.Name)) {
                var saved = _Database.Manufacturer_GetOrCreateByName(name);

                Assert.AreNotEqual(0, saved.ManufacturerID);
                Assert.AreEqual(now, saved.CreatedUtc);
                Assert.AreEqual(now, saved.UpdatedUtc);

                var readBack = _Database.Manufacturer_GetByID(saved.ManufacturerID);
                AssertManufacturersAreEqual(saved, readBack);
            }
        }

        protected void Manufacturer_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            foreach(var manufacturer in SampleManufacturers()) {
                _Database.Manufacturer_Save(manufacturer);

                var sameCaseReadBack = _Database.Manufacturer_GetOrCreateByName(manufacturer.Name);
                AssertManufacturersAreEqual(manufacturer, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(manufacturer.Name);
                var flippedCaseReadBack = _Database.Manufacturer_GetOrCreateByName(flippedName);
                AssertManufacturersAreEqual(manufacturer, flippedCaseReadBack);
            }
        }

        protected void Manufacturer_Delete_Deletes_Manufacturers()
        {
            foreach(var manufacturer in SampleManufacturers()) {
                _Database.Manufacturer_Save(manufacturer);

                var id = manufacturer.ManufacturerID;
                _Database.Manufacturer_Delete(manufacturer);

                var readBack = _Database.Manufacturer_GetByID(id);
                Assert.IsNull(readBack);
            }
        }

        protected void Manufacturer_Delete_Nulls_Out_References_To_Manufacturer()
        {
            var aircraftType = SampleAircraftTypes(true).First(r => r.ManufacturerID != null);
            _Database.AircraftType_Save(aircraftType);
        
            var manufacturer = _Database.Manufacturer_GetByID(aircraftType.ManufacturerID.Value);
            Assert.IsNotNull(manufacturer);
        
            _Database.Manufacturer_Delete(manufacturer);
        
            var readBackAircraftType = _Database.AircraftType_GetByID(aircraftType.AircraftTypeID);
            Assert.IsNull(readBackAircraftType.ManufacturerID);
        }

        protected void Manufacturer_Delete_Ignores_Deleted_Manufacturers()
        {
            var doesNotExist = new TrackHistoryManufacturer() {
                ManufacturerID =  1,
                Name =       "sqrt(-1)",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
            };

            // This just needs to not throw an exception
            _Database.Manufacturer_Delete(doesNotExist);
        }

        private TrackHistoryManufacturer[] SampleManufacturers(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistoryManufacturer[] {
                new TrackHistoryManufacturer() { Name = "Airbus", CreatedUtc = created, UpdatedUtc = updated, },
                new TrackHistoryManufacturer() { Name = "Boeing", CreatedUtc = created, UpdatedUtc = updated, },
            };
        }

        private void AssertManufacturersAreEqual(TrackHistoryManufacturer expected, TrackHistoryManufacturer actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region Model
        protected void Model_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var model in SampleModels(created, updated)) {
                _Database.Model_Save(model);

                Assert.AreNotEqual(0, model.ModelID);

                var readBack = _Database.Model_GetByID(model.ModelID);
                AssertModelsAreEqual(model, readBack);
            }
        }

        protected void Model_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createModels = SampleModels(now, now);
            foreach(var model in createModels) {
                _Database.Model_Save(model);
            }

            var updatedTime = now.AddSeconds(7);
            var updateModels = SampleModels(now, updatedTime);
            for(var i = 0;i < updateModels.Length;++i) {
                var createModel = createModels[i];
                var updateModel = updateModels[i];

                var expectedName = new String(createModel.Name.Reverse().ToArray());

                updateModel.ModelID = createModel.ModelID;
                updateModel.Name = expectedName;

                _Database.Model_Save(updateModel);

                Assert.AreEqual(createModel.ModelID, updateModel.ModelID);
                Assert.AreEqual(expectedName,              updateModel.Name);
                Assert.AreEqual(createModel.CreatedUtc, updateModel.CreatedUtc);
                Assert.AreEqual(updatedTime,               updateModel.UpdatedUtc);

                var readBack = _Database.Model_GetByID(updateModel.ModelID);
                AssertModelsAreEqual(updateModel, readBack);
            }
        }

        protected void Model_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            foreach(var model in SampleModels()) {
                _Database.Model_Save(model);

                var sameCaseReadBack = _Database.Model_GetByName(model.Name);
                AssertModelsAreEqual(model, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(model.Name);
                var flippedCaseReadBack = _Database.Model_GetByName(flippedName);
                AssertModelsAreEqual(model, flippedCaseReadBack);
            }
        }

        protected void Model_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            _Clock.UtcNowValue = now;

            foreach(var name in SampleModels().Select(r => r.Name)) {
                var saved = _Database.Model_GetOrCreateByName(name);

                Assert.AreNotEqual(0, saved.ModelID);
                Assert.AreEqual(now, saved.CreatedUtc);
                Assert.AreEqual(now, saved.UpdatedUtc);

                var readBack = _Database.Model_GetByID(saved.ModelID);
                AssertModelsAreEqual(saved, readBack);
            }
        }

        protected void Model_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            foreach(var model in SampleModels()) {
                _Database.Model_Save(model);

                var sameCaseReadBack = _Database.Model_GetOrCreateByName(model.Name);
                AssertModelsAreEqual(model, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(model.Name);
                var flippedCaseReadBack = _Database.Model_GetOrCreateByName(flippedName);
                AssertModelsAreEqual(model, flippedCaseReadBack);
            }
        }

        protected void Model_Delete_Deletes_Models()
        {
            foreach(var model in SampleModels()) {
                _Database.Model_Save(model);

                var id = model.ModelID;
                _Database.Model_Delete(model);

                var readBack = _Database.Model_GetByID(id);
                Assert.IsNull(readBack);
            }
        }

        protected void Model_Delete_Nulls_Out_References_To_Model()
        {
            var aircraftType = SampleAircraftTypes(true).First(r => r.ModelID != null);
            _Database.AircraftType_Save(aircraftType);
        
            var model = _Database.Model_GetByID(aircraftType.ModelID.Value);
            Assert.IsNotNull(model);
        
            _Database.Model_Delete(model);
        
            var readBackAircraftType = _Database.AircraftType_GetByID(aircraftType.AircraftTypeID);
            Assert.IsNull(readBackAircraftType.ModelID);
        }

        protected void Model_Delete_Ignores_Deleted_Models()
        {
            var doesNotExist = new TrackHistoryModel() {
                ModelID =  1,
                Name =       "sqrt(-1)",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
            };

            // This just needs to not throw an exception
            _Database.Model_Delete(doesNotExist);
        }

        private TrackHistoryModel[] SampleModels(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistoryModel[] {
                new TrackHistoryModel() { Name = "Airbus", CreatedUtc = created, UpdatedUtc = updated, },
                new TrackHistoryModel() { Name = "Boeing", CreatedUtc = created, UpdatedUtc = updated, },
            };
        }

        private void AssertModelsAreEqual(TrackHistoryModel expected, TrackHistoryModel actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region Operator
        protected void Operator_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var acOperator in SampleOperators(created, updated)) {
                _Database.Operator_Save(acOperator);

                Assert.AreNotEqual(0, acOperator.OperatorID);

                var readBack = _Database.Operator_GetByID(acOperator.OperatorID);
                AssertOperatorsAreEqual(acOperator, readBack);
            }
        }

        protected void Operator_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createOperators = SampleOperators(now, now);
            foreach(var acOperator in createOperators) {
                _Database.Operator_Save(acOperator);
            }

            var updatedTime = now.AddSeconds(7);
            var updateOperators = SampleOperators(now, updatedTime);
            for(var i = 0;i < updateOperators.Length;++i) {
                var createOperator = createOperators[i];
                var updateOperator = updateOperators[i];

                var expectedIcao = new String(createOperator.Icao.Reverse().ToArray());
                var expectedName = new String(createOperator.Name.Reverse().ToArray());

                updateOperator.OperatorID = createOperator.OperatorID;
                updateOperator.Icao = expectedIcao;
                updateOperator.Name = expectedName;

                _Database.Operator_Save(updateOperator);

                Assert.AreEqual(createOperator.OperatorID, updateOperator.OperatorID);
                Assert.AreEqual(expectedIcao,              updateOperator.Icao);
                Assert.AreEqual(expectedName,              updateOperator.Name);
                Assert.AreEqual(createOperator.CreatedUtc, updateOperator.CreatedUtc);
                Assert.AreEqual(updatedTime,               updateOperator.UpdatedUtc);

                var readBack = _Database.Operator_GetByID(updateOperator.OperatorID);
                AssertOperatorsAreEqual(updateOperator, readBack);
            }
        }

        protected void Operator_GetByKey_Fetches_By_Case_Insensitive_Keys()
        {
            foreach(var acOperator in SampleOperators()) {
                _Database.Operator_Save(acOperator);

                var sameCaseReadBack = _Database.Operator_GetByKey(acOperator.Icao, acOperator.Name);
                AssertOperatorsAreEqual(acOperator, sameCaseReadBack);

                var flippedIcao = TestUtilities.FlipCase(acOperator.Icao);
                var flippedName = TestUtilities.FlipCase(acOperator.Name);
                var flippedCaseReadBack = _Database.Operator_GetByKey(flippedIcao, flippedName);
                AssertOperatorsAreEqual(acOperator, flippedCaseReadBack);
            }
        }

        protected void Operator_GetOrCreateByKey_Creates_New_Records_Correctly()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            _Clock.UtcNowValue = now;

            foreach(var uniqueKey in SampleOperators().Select(r => new { r.Icao, r.Name })) {
                var saved = _Database.Operator_GetOrCreateByKey(uniqueKey.Icao, uniqueKey.Name);

                Assert.AreNotEqual(0, saved.OperatorID);
                Assert.AreEqual(now, saved.CreatedUtc);
                Assert.AreEqual(now, saved.UpdatedUtc);

                var readBack = _Database.Operator_GetByID(saved.OperatorID);
                AssertOperatorsAreEqual(saved, readBack);
            }
        }

        protected void Operator_GetOrCreateByKey_Fetches_Existing_Records_Correctly()
        {
            foreach(var acOperator in SampleOperators()) {
                _Database.Operator_Save(acOperator);

                var sameCaseReadBack = _Database.Operator_GetOrCreateByKey(acOperator.Icao, acOperator.Name);
                AssertOperatorsAreEqual(acOperator, sameCaseReadBack);

                var flippedIcao = TestUtilities.FlipCase(acOperator.Icao);
                var flippedName = TestUtilities.FlipCase(acOperator.Name);
                var flippedCaseReadBack = _Database.Operator_GetByKey(flippedIcao, flippedName);
                AssertOperatorsAreEqual(acOperator, flippedCaseReadBack);
            }
        }

        private TrackHistoryOperator[] SampleOperators(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistoryOperator[] {
                new TrackHistoryOperator() { Icao = "BAW", Name = "British Airways", CreatedUtc = created, UpdatedUtc = updated, },
                new TrackHistoryOperator() { Icao = "VIR", Name = "Virgin",          CreatedUtc = created, UpdatedUtc = updated, },
            };
        }

        private void AssertOperatorsAreEqual(TrackHistoryOperator expected, TrackHistoryOperator actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region Receiver
        protected void Receiver_Save_Creates_New_Records_Correctly()
        {
            var created = DateTime.UtcNow;
            var updated = created.AddMilliseconds(7);
            foreach(var receiver in SampleReceivers(created, updated)) {
                _Database.Receiver_Save(receiver);

                Assert.AreNotEqual(0, receiver.ReceiverID);

                var readBack = _Database.Receiver_GetByID(receiver.ReceiverID);
                AssertReceiversAreEqual(receiver, readBack);
            }
        }

        protected void Receiver_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createReceivers = SampleReceivers(now, now);
            foreach(var receiver in createReceivers) {
                _Database.Receiver_Save(receiver);
            }

            var updatedTime = now.AddSeconds(7);
            var updateReceivers = SampleReceivers(now, updatedTime);
            for(var i = 0;i < updateReceivers.Length;++i) {
                var createReceiver = createReceivers[i];
                var updateReceiver = updateReceivers[i];

                var expectedName = new String(createReceiver.Name.Reverse().ToArray());

                updateReceiver.ReceiverID = createReceiver.ReceiverID;
                updateReceiver.Name = expectedName;

                _Database.Receiver_Save(updateReceiver);

                Assert.AreEqual(createReceiver.ReceiverID, updateReceiver.ReceiverID);
                Assert.AreEqual(expectedName,              updateReceiver.Name);
                Assert.AreEqual(createReceiver.CreatedUtc, updateReceiver.CreatedUtc);
                Assert.AreEqual(updatedTime,               updateReceiver.UpdatedUtc);

                var readBack = _Database.Receiver_GetByID(updateReceiver.ReceiverID);
                AssertReceiversAreEqual(updateReceiver, readBack);
            }
        }

        protected void Receiver_GetByName_Fetches_By_Case_Insensitive_Name()
        {
            foreach(var receiver in SampleReceivers()) {
                _Database.Receiver_Save(receiver);

                var sameCaseReadBack = _Database.Receiver_GetByName(receiver.Name);
                AssertReceiversAreEqual(receiver, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(receiver.Name);
                var flippedCaseReadBack = _Database.Receiver_GetByName(flippedName);
                AssertReceiversAreEqual(receiver, flippedCaseReadBack);
            }
        }

        protected void Receiver_GetOrCreateByName_Creates_New_Records_Correctly()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            _Clock.UtcNowValue = now;

            foreach(var name in SampleReceivers().Select(r => r.Name)) {
                var saved = _Database.Receiver_GetOrCreateByName(name);

                Assert.AreNotEqual(0, saved.ReceiverID);
                Assert.AreEqual(now, saved.CreatedUtc);
                Assert.AreEqual(now, saved.UpdatedUtc);

                var readBack = _Database.Receiver_GetByID(saved.ReceiverID);
                AssertReceiversAreEqual(saved, readBack);
            }
        }

        protected void Receiver_GetOrCreateByName_Fetches_Existing_Records_Correctly()
        {
            foreach(var receiver in SampleReceivers()) {
                _Database.Receiver_Save(receiver);

                var sameCaseReadBack = _Database.Receiver_GetOrCreateByName(receiver.Name);
                AssertReceiversAreEqual(receiver, sameCaseReadBack);

                var flippedName = TestUtilities.FlipCase(receiver.Name);
                var flippedCaseReadBack = _Database.Receiver_GetOrCreateByName(flippedName);
                AssertReceiversAreEqual(receiver, flippedCaseReadBack);
            }
        }

        protected void Receiver_Delete_Deletes_Receivers()
        {
            foreach(var receiver in SampleReceivers()) {
                _Database.Receiver_Save(receiver);

                var id = receiver.ReceiverID;
                _Database.Receiver_Delete(receiver);

                var readBack = _Database.Receiver_GetByID(id);
                Assert.IsNull(readBack);
            }
        }

        protected void Receiver_Delete_Nulls_Out_References_To_Receiver()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();
            var state = SampleTrackHistoryStates(trackHistory, generateForCreate: true).First(r => r.ReceiverID != null);
            _Database.TrackHistoryState_Save(state);

            var receiver = _Database.Receiver_GetByID(state.ReceiverID.Value);
            Assert.IsNotNull(receiver);

            _Database.Receiver_Delete(receiver);

            var readBackState = _Database.TrackHistoryState_GetByID(state.TrackHistoryStateID);
            Assert.IsNull(readBackState.ReceiverID);
        }

        protected void Receiver_Delete_Ignores_Deleted_Receivers()
        {
            var doesNotExist = new TrackHistoryReceiver() {
                ReceiverID = 1,
                Name =       "sqrt(-1)",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
            };

            // This just needs to not throw an exception
            _Database.Receiver_Delete(doesNotExist);
        }

        private TrackHistoryReceiver[] SampleReceivers(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var created = createdUtc ?? DateTime.UtcNow;
            var updated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistoryReceiver[] {
                new TrackHistoryReceiver() { Name = "Strix", CreatedUtc = created, UpdatedUtc = updated, },
                new TrackHistoryReceiver() { Name = "Diath", CreatedUtc = created, UpdatedUtc = updated, },
            };
        }

        private void AssertReceiversAreEqual(TrackHistoryReceiver expected, TrackHistoryReceiver actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }
        #endregion

        #region Species
        public void Species_GetByID_Returns_Correct_Species()
        {
            foreach(Species enumSpecies in Enum.GetValues(typeof(Species))) {
                var dbSpecies = _Database.Species_GetByID(enumSpecies);
                if(enumSpecies == Species.None) {
                    Assert.IsNull(dbSpecies);
                } else {
                    AssertSpeciesIsCorrect(enumSpecies, dbSpecies);
                }
            }
        }

        public void Species_GetAll_Returns_All_Species()
        {
            var allSpecies = Enum.GetValues(typeof(Species))
                .OfType<Species>()
                .Where(r => r != Species.None)
                .ToArray();
            var actual = _Database.Species_GetAll().ToArray();
            Assert.AreEqual(allSpecies.Length, actual.Length);

            foreach(Species enumSpecies in allSpecies) {
                AssertSpeciesIsCorrect(enumSpecies, actual.Single(r => r.SpeciesID == enumSpecies));
            }
        }

        private static void AssertSpeciesIsCorrect(Species enumSpecies, TrackHistorySpecies dbSpecies)
        {
            Assert.AreEqual(enumSpecies, dbSpecies.SpeciesID);

            var expectedCode = "";
            var expectedDesc = "";
            var isFake = false;

            switch(enumSpecies) {
                case Species.Amphibian:     expectedCode = "A";     expectedDesc = "Amphibian"; break;
                case Species.GroundVehicle: expectedCode = "-GND";  expectedDesc = "Ground Vehicle"; isFake = true; break;
                case Species.Gyrocopter:    expectedCode = "G";     expectedDesc = "Gyrocopter"; break;
                case Species.Helicopter:    expectedCode = "H";     expectedDesc = "Helicopter"; break;
                case Species.Landplane:     expectedCode = "L";     expectedDesc = "Landplane"; break;
                case Species.Seaplane:      expectedCode = "S";     expectedDesc = "Seaplane"; break;
                case Species.TiltWing:      expectedCode = "T";     expectedDesc = "Tiltwing"; break;
                case Species.Tower:         expectedCode = "-TWR";  expectedDesc = "Radio Mast"; isFake = true; break;
                default: throw new NotImplementedException($"Need code for {enumSpecies}");
            }

            Assert.AreEqual(expectedCode, dbSpecies.Code);
            Assert.AreEqual(expectedDesc, dbSpecies.Description);
            Assert.AreEqual(isFake, dbSpecies.IsFake);
        }
        #endregion

        #region TrackHistory
        protected void TrackHistory_Save_Creates_New_Records_Correctly()
        {
            foreach(var trackHistory in SampleTrackHistories()) {
                _Database.TrackHistory_Save(trackHistory);

                Assert.AreNotEqual(0, trackHistory.TrackHistoryID);

                var readBack = _Database.TrackHistory_GetByID(trackHistory.TrackHistoryID);
                AssertTrackHistoriesAreEqual(trackHistory, readBack);
            }
        }

        protected void TrackHistory_Save_Updates_Existing_Records_Correctly()
        {
            var now = DateTime.UtcNow;

            var createTrackHistories = SampleTrackHistories(now, now);
            foreach(var trackHistory in createTrackHistories) {
                _Database.TrackHistory_Save(trackHistory);
            }

            var updatedTime = now.AddSeconds(7);
            var updateTrackHistories = SampleTrackHistories(now, updatedTime);
            for(var i = 0;i < updateTrackHistories.Length;++i) {
                var createHistory = createTrackHistories[i];
                var updateHistory = updateTrackHistories[i];

                var originalAircraft = _Database.Aircraft_GetByID(createHistory.AircraftID);

                var expectedAircraftID = AircraftIDForIcao(new String(originalAircraft.Icao.Reverse().ToArray()));
                var expectedIsPreserved = !createHistory.IsPreserved;

                updateHistory.TrackHistoryID = createHistory.TrackHistoryID;
                updateHistory.AircraftID = expectedAircraftID;
                updateHistory.IsPreserved = expectedIsPreserved;

                _Database.TrackHistory_Save(updateHistory);

                Assert.AreEqual(createHistory.TrackHistoryID, updateHistory.TrackHistoryID);
                Assert.AreEqual(expectedAircraftID,           updateHistory.AircraftID);
                Assert.AreEqual(expectedIsPreserved,          updateHistory.IsPreserved);
                Assert.AreEqual(createHistory.CreatedUtc,     updateHistory.CreatedUtc);
                Assert.AreEqual(updatedTime,                  updateHistory.UpdatedUtc);

                var readBack = _Database.TrackHistory_GetByID(updateHistory.TrackHistoryID);
                AssertTrackHistoriesAreEqual(updateHistory, readBack);
            }
        }

        protected void TrackHistory_GetByAircraftID_With_No_Criteria_Returns_Correct_Records()
        {
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);
            var savedHistories = SaveTrackHistoriesForTimes(new DateTime[] { today, tomorrow });
            var todayHistories = savedHistories.Where(r => r.CreatedUtc == today).ToArray();
            var tomorrowHistories = savedHistories.Where(r => r.CreatedUtc == tomorrow).ToArray();

            for(var historyIdx = 0;historyIdx < todayHistories.Length;++historyIdx) {
                var expected = new TrackHistory[] {
                    todayHistories[historyIdx],
                    tomorrowHistories[historyIdx],
                };

                var aircraftID = expected[0].AircraftID;
                var readBack = _Database.TrackHistory_GetByAircraftID(aircraftID, null, null).ToArray();

                AssertTrackHistoriesAreEqual(expected, readBack);
            }
        }

        private List<TrackHistory> SaveTrackHistoriesForTimes(DateTime[] utcTimes)
        {
            var result = new List<TrackHistory>();

            foreach(var utcTime in utcTimes) {
                foreach(var trackHistory in SampleTrackHistories(utcTime, utcTime)) {
                    _Database.TrackHistory_Save(trackHistory);
                    result.Add(trackHistory);
                }
            }

            return result;
        }

        protected void TrackHistory_GetByAircraftID_With_Criteria_Returns_Correct_Records()
        {
            var today = DateTime.UtcNow;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var savedHistories = SaveTrackHistoriesForTimes(new DateTime[] { today, yesterday, tomorrow });

            var aircraftIDs = savedHistories.Select(r => r.AircraftID).Distinct().ToArray();

            foreach(var dateRange in SampleTrackHistoryDateRanges(yesterday, today, tomorrow)) {
                foreach(var aircraftID in aircraftIDs) {
                    var from = dateRange.Item1;
                    var to =   dateRange.Item2;
                    var expected = ExtractExpectedTrackHistoriesForDateTime(savedHistories, from, to, filterToAircraftID: true, aircraftID: aircraftID);

                    var readbackHistories = _Database.TrackHistory_GetByAircraftID(aircraftID, from, to);
                    AssertTrackHistoriesAreEqual(expected, readbackHistories);
                }
            }
        }

        protected void TrackHistory_GetByDateRange_Returns_Correct_Records()
        {
            var today = DateTime.UtcNow;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var savedHistories = SaveTrackHistoriesForTimes(new DateTime[] { today, yesterday, tomorrow });

            var aircraftIDs = savedHistories.Select(r => r.AircraftID).Distinct().ToArray();

            foreach(var dateRange in SampleTrackHistoryDateRanges(yesterday, today, tomorrow)) {
                foreach(var aircraftID in aircraftIDs) {
                    var from = dateRange.Item1;
                    var to =   dateRange.Item2;
                    var expected = ExtractExpectedTrackHistoriesForDateTime(savedHistories, from, to);

                    var actual = _Database.TrackHistory_GetByDateRange(from, to);
                    AssertTrackHistoriesAreEqual(expected, actual);
                }
            }
        }

        protected void TrackHistory_Delete_Removes_TrackHistory_Records()
        {
            foreach(var isPreserved in new bool[] { false, true }) {
                var trackHistory = new TrackHistory() {
                    AircraftID =  AircraftIDForIcao("123456"),
                    IsPreserved = isPreserved,
                };
                _Database.TrackHistory_Save(trackHistory);

                var deleted = _Database.TrackHistory_Delete(trackHistory);

                Assert.AreEqual(1, deleted.CountTrackHistories);
                Assert.AreEqual(0, deleted.CountTrackHistoryStates);
                Assert.AreEqual(trackHistory.CreatedUtc, deleted.EarliestHistoryUtc);
                Assert.AreEqual(trackHistory.CreatedUtc, deleted.LatestHistoryUtc);

                var readBack = _Database.TrackHistory_GetByID(trackHistory.TrackHistoryID);
                Assert.IsNull(readBack);
            }
        }

        protected void TrackHistory_Delete_Can_Be_Called_Within_A_Transaction()
        {
            var record = new TrackHistory() {
                AircraftID = AircraftIDForIcao("123456"),
            };
            _Database.TrackHistory_Save(record);

            _Database.PerformInTransaction(() => {
                _Database.TrackHistory_Delete(record);

                var inTransReadBack = _Database.TrackHistory_GetByID(record.TrackHistoryID);
                Assert.IsNull(inTransReadBack);

                return false;
            });

            var readBack = _Database.TrackHistory_GetByID(record.TrackHistoryID);
            AssertTrackHistoriesAreEqual(record, readBack);
        }

        protected void TrackHistory_DeleteExpired_Deletes_Appropriate_Transactions()
        {
            var today = DateTime.UtcNow;
            var yesterday = today.AddDays(-1);
            var dayBeforeYesterday = yesterday.AddDays(-1);
            var savedHistories = SaveTrackHistoriesForTimes(new DateTime[] { yesterday, today, dayBeforeYesterday });

            var expectDeleted = savedHistories.Where(r => r.CreatedUtc <= yesterday && !r.IsPreserved).ToArray();

            var deleted = _Database.TrackHistory_DeleteExpired(yesterday);
            Assert.AreEqual(expectDeleted.Length, deleted.CountTrackHistories);
            Assert.AreEqual(0, deleted.CountTrackHistoryStates);
            Assert.AreEqual(expectDeleted.Select(r => r.CreatedUtc).Min(), deleted.EarliestHistoryUtc);
            Assert.AreEqual(expectDeleted.Select(r => r.CreatedUtc).Max(), deleted.LatestHistoryUtc);
        }

        protected void TrackHistory_Truncate_Removes_All_But_First_And_Last_States()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();
            var allStates = SampleTrackHistoryStates(trackHistory, generateForCreate: true);
            _Database.TrackHistoryState_SaveMany(allStates);

            var expected1st = allStates[0];
            var expected2nd = TrackHistoryState.MergeStates(allStates);
            var expectedStates = new TrackHistoryState[] { expected1st, expected2nd };

            var now = trackHistory.UpdatedUtc.AddSeconds(19);

            var truncateResult = _Database.TrackHistory_Truncate(trackHistory, now);
            Assert.AreEqual(1,                          truncateResult.CountTrackHistories);
            Assert.AreEqual(allStates.Count - 2,        truncateResult.CountTrackHistoryStates);
            Assert.AreEqual(trackHistory.CreatedUtc,    truncateResult.EarliestHistoryUtc);
            Assert.AreEqual(trackHistory.UpdatedUtc,    truncateResult.LatestHistoryUtc);

            var readBackStates = _Database.TrackHistoryState_GetByTrackHistory(trackHistory).ToArray();
            Assert.AreEqual(2, readBackStates.Length);

            // Bit of a kludge, we don't know the ID of the merged record
            Assert.AreNotEqual(0, readBackStates[1].TrackHistoryStateID);
            expected2nd.TrackHistoryStateID = readBackStates[1].TrackHistoryStateID;

            AssertTrackHistoryStatesAreEqual(expectedStates, readBackStates);
        }

        protected void TrackHistory_Truncate_Ignores_Preserved_Histories()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates(isPreserved: true);
            var allStates = SampleTrackHistoryStates(trackHistory, generateForCreate: true);
            _Database.TrackHistoryState_SaveMany(allStates);

            var now = trackHistory.UpdatedUtc.AddSeconds(19);

            var truncateResult = _Database.TrackHistory_Truncate(trackHistory, now);
            Assert.AreEqual(0,                  truncateResult.CountTrackHistories);
            Assert.AreEqual(0,                  truncateResult.CountTrackHistoryStates);
            Assert.AreEqual(default(DateTime),  truncateResult.EarliestHistoryUtc);
            Assert.AreEqual(default(DateTime),  truncateResult.LatestHistoryUtc);

            var readBackStates = _Database.TrackHistoryState_GetByTrackHistory(trackHistory).ToArray();
            AssertTrackHistoryStatesAreEqual(allStates, readBackStates);
        }

        protected void TrackHistory_TruncateExpired_Truncates_Histories()
        {
            var now = DateTime.UtcNow;
            var threshold = now.AddDays(-7);

            var truncatable1 =  SampleTrackHistoryForHistoryStates(utcNow: threshold);
            var preserved =     SampleTrackHistoryForHistoryStates(utcNow: threshold, isPreserved: true);
            var pastThreshold = SampleTrackHistoryForHistoryStates(utcNow: threshold.AddSeconds(1));
            var truncatable2 =  SampleTrackHistoryForHistoryStates(utcNow: threshold.AddSeconds(-1));

            var truncatable1States =  SampleTrackHistoryStates(truncatable1, generateForCreate: true);
            var preservedStates =     SampleTrackHistoryStates(preserved, generateForCreate: true);
            var pastThresholdStates = SampleTrackHistoryStates(pastThreshold, generateForCreate: true);
            var truncatable2States =  SampleTrackHistoryStates(truncatable2, generateForCreate: true);

            _Database.TrackHistoryState_SaveMany(truncatable1States);
            _Database.TrackHistoryState_SaveMany(preservedStates);
            _Database.TrackHistoryState_SaveMany(pastThresholdStates);
            _Database.TrackHistoryState_SaveMany(truncatable2States);

            var truncateResult = _Database.TrackHistory_TruncateExpired(threshold, now);
            Assert.AreEqual(2,                                                               truncateResult.CountTrackHistories);
            Assert.AreEqual((truncatable1States.Count - 2) + (truncatable2States.Count - 2), truncateResult.CountTrackHistoryStates);
            Assert.AreEqual(truncatable2.CreatedUtc,                                         truncateResult.EarliestHistoryUtc);
            Assert.AreEqual(truncatable1.CreatedUtc,                                         truncateResult.LatestHistoryUtc);

            Assert.AreEqual(2, _Database.TrackHistoryState_GetByTrackHistory(truncatable1).Count());
            Assert.AreEqual(2, _Database.TrackHistoryState_GetByTrackHistory(truncatable2).Count());
            Assert.AreEqual(preservedStates.Count, _Database.TrackHistoryState_GetByTrackHistory(preserved).Count());
            Assert.AreEqual(pastThresholdStates.Count, _Database.TrackHistoryState_GetByTrackHistory(pastThreshold).Count());
        }

        private Tuple<DateTime?, DateTime?>[] SampleTrackHistoryDateRanges(DateTime yesterday, DateTime today, DateTime tomorrow)
        {
            return new Tuple<DateTime?, DateTime?>[] {
                new Tuple<DateTime?, DateTime?>(null, today),
                new Tuple<DateTime?, DateTime?>(today, null),
                new Tuple<DateTime?, DateTime?>(today, tomorrow),

                new Tuple<DateTime?, DateTime?>(null, today.AddMilliseconds(-1)),
                new Tuple<DateTime?, DateTime?>(today.AddMilliseconds(-1), null),
                new Tuple<DateTime?, DateTime?>(today, tomorrow.AddMilliseconds(-1)),

                new Tuple<DateTime?, DateTime?>(null, today.AddMilliseconds(1)),
                new Tuple<DateTime?, DateTime?>(today.AddMilliseconds(1), null),
                new Tuple<DateTime?, DateTime?>(today, tomorrow.AddMilliseconds(1)),

                new Tuple<DateTime?, DateTime?>(today, yesterday),
            };
        }

        private TrackHistory[] SampleTrackHistories(DateTime? createdUtc = null, DateTime? updatedUtc = null)
        {
            var coalescedCreated = createdUtc ?? DateTime.UtcNow;
            var coalescedUpdated = updatedUtc ?? DateTime.UtcNow;

            return new TrackHistory[] {
                new TrackHistory() { AircraftID = AircraftIDForIcao("ABC123"), IsPreserved = true,  CreatedUtc = coalescedCreated, UpdatedUtc = coalescedUpdated, },
                new TrackHistory() { AircraftID = AircraftIDForIcao("987654"), IsPreserved = false, CreatedUtc = coalescedCreated, UpdatedUtc = coalescedUpdated, },
            };
        }

        private long AircraftIDForIcao(string icao)
        {
            var result = _Database.Aircraft_GetByIcao(icao);
            if(result == null) {
                result = new TrackHistoryAircraft() {
                    Icao =          icao,
                    CreatedUtc =    DateTime.UtcNow,
                    UpdatedUtc =    DateTime.UtcNow,
                };
                _Database.Aircraft_Save(result);
            }

            return result.AircraftID;
        }

        private void AssertTrackHistoriesAreEqual(TrackHistory expected, TrackHistory actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }

        private void AssertTrackHistoriesAreEqual(IEnumerable<TrackHistory> expected, IEnumerable<TrackHistory> actual)
        {
            var expectedArray = expected?.ToArray() ?? new TrackHistory[0];
            var actualArray =   actual?.ToArray() ?? new TrackHistory[0];

            Assert.AreEqual(expectedArray.Length, actualArray.Length);
            for(var i = 0;i < expectedArray.Length;++i) {
                AssertTrackHistoriesAreEqual(expectedArray[i], actualArray[i]);
            }
        }

        private TrackHistory[] ExtractExpectedTrackHistoriesForDateTime(IEnumerable<TrackHistory> histories, DateTime? from, DateTime? to, bool filterToAircraftID = false, long? aircraftID = null)
        {
            return histories.Where(r =>
                   r.CreatedUtc >= from.GetValueOrDefault()
                && r.CreatedUtc <= (to ?? new DateTime(9999, 12, 31))
                && (!filterToAircraftID || r.AircraftID == aircraftID)
            )
            .OrderBy(r => r.CreatedUtc)
            .ThenBy(r => r.AircraftID)
            .ToArray();
        }
        #endregion

        #region TrackHistoryState
        protected void TrackHistoryState_Save_Creates_New_Records_Correctly()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();

            var seenStateIDs = new HashSet<long>();

            foreach(var state in SampleTrackHistoryStates(trackHistory, generateForCreate: true)) {
                _Database.TrackHistoryState_Save(state);

                Assert.AreNotEqual(0, state.TrackHistoryStateID);

                Assert.IsFalse(seenStateIDs.Contains(state.TrackHistoryStateID));
                seenStateIDs.Add(state.TrackHistoryStateID);

                var readBack = _Database.TrackHistoryState_GetByID(state.TrackHistoryStateID);
                AssertTrackHistoryStatesAreEqual(state, readBack);
            }
        }

        protected void TrackHistoryState_Save_Updates_Existing_Records_Correctly()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();
            var created = DateTime.UtcNow;
            var updated = created.AddSeconds(9);

            var savedStates =   SampleTrackHistoryStates(trackHistory, generateForCreate: true,  timestampUtc: created);
            var updatedStates = SampleTrackHistoryStates(trackHistory, generateForCreate: false, timestampUtc: updated);

            foreach(var state in savedStates) {
                _Database.TrackHistoryState_Save(state);
            }

            for(var i = 0;i < savedStates.Count;++i) {
                var savedState = savedStates[i];
                var updatedState = updatedStates[i];

                updatedState.TrackHistoryStateID = savedState.TrackHistoryStateID;
                _Database.TrackHistoryState_Save(updatedState);

                var readBack = _Database.TrackHistoryState_GetByID(savedState.TrackHistoryStateID);
                AssertTrackHistoryStatesAreEqual(updatedState, readBack);
            }
        }

        protected void TrackHistoryState_Save_Can_Move_To_Another_TrackHistory()
        {
            var now = DateTime.UtcNow;
            var originalHistory = SampleTrackHistoryForHistoryStates(utcNow: now);
            var newHistory      = SampleTrackHistoryForHistoryStates(utcNow: now.AddSeconds(1));

            var state = new TrackHistoryState() {
                TrackHistoryID = originalHistory.TrackHistoryID,
                SequenceNumber = 1,
                TimestampUtc =   now.AddSeconds(2),
            };
            _Database.TrackHistoryState_Save(state);

            state.TrackHistoryID = newHistory.TrackHistoryID;
            _Database.TrackHistoryState_Save(state);

            var readBack = _Database.TrackHistoryState_GetByID(state.TrackHistoryStateID);
            Assert.AreEqual(newHistory.TrackHistoryID, readBack.TrackHistoryID);
        }

        protected void TrackHistoryState_Save_Throws_If_TrackHistoryID_Is_Zero()
        {
            _Database.TrackHistoryState_Save(new TrackHistoryState() { TrackHistoryID = 0, SequenceNumber = 1, TimestampUtc = DateTime.UtcNow, });
        }

        protected void TrackHistoryState_Save_Throws_If_SequenceNumber_Is_Zero()
        {
            _Database.TrackHistoryState_Save(new TrackHistoryState() { TrackHistoryID = 1, SequenceNumber = 0, TimestampUtc = DateTime.UtcNow, });
        }

        protected void TrackHistoryState_Save_Throws_If_Timestamp_Is_Default()
        {
            _Database.TrackHistoryState_Save(new TrackHistoryState() { TrackHistoryID = 1, SequenceNumber = 1, });
        }

        protected void TrackHistoryState_SaveMany_Creates_New_Records_Correctly()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();

            var seenStateIDs = new HashSet<long>();
            var allStates = SampleTrackHistoryStates(trackHistory, generateForCreate: true);
            _Database.TrackHistoryState_SaveMany(allStates);

            foreach(var state in allStates) {
                Assert.AreNotEqual(0, state.TrackHistoryStateID);

                Assert.IsFalse(seenStateIDs.Contains(state.TrackHistoryStateID));
                seenStateIDs.Add(state.TrackHistoryStateID);

                var readBack = _Database.TrackHistoryState_GetByID(state.TrackHistoryStateID);
                AssertTrackHistoryStatesAreEqual(state, readBack);
            }
        }

        protected void TrackHistoryState_SaveMany_Updates_Existing_Records_Correctly()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();
            var created = DateTime.UtcNow;
            var updated = created.AddSeconds(9);

            var savedStates =   SampleTrackHistoryStates(trackHistory, generateForCreate: true,  timestampUtc: created);
            var updatedStates = SampleTrackHistoryStates(trackHistory, generateForCreate: false, timestampUtc: updated);

            for(var i = 0;i < savedStates.Count;++i) {
                var saveState = savedStates[i];
                _Database.TrackHistoryState_Save(saveState);

                var updateState = updatedStates[i];
                updateState.TrackHistoryStateID = saveState.TrackHistoryStateID;
            }

            _Database.TrackHistoryState_SaveMany(updatedStates);

            for(var i = 0;i < savedStates.Count;++i) {
                var savedState = savedStates[i];
                var updatedState = updatedStates[i];

                var readBack = _Database.TrackHistoryState_GetByID(savedState.TrackHistoryStateID);
                AssertTrackHistoryStatesAreEqual(updatedState, readBack);
            }
        }

        protected void TrackHistoryState_GetByTrackHistory_Returns_Saved_IDs_In_Correct_Order()
        {
            var trackHistory = SampleTrackHistoryForHistoryStates();
            var now = DateTime.UtcNow;

            foreach(var state in new TrackHistoryState[] {
                new TrackHistoryState() { TrackHistoryID = trackHistory.TrackHistoryID, SequenceNumber = 2, TimestampUtc = now.AddSeconds(2) },
                new TrackHistoryState() { TrackHistoryID = trackHistory.TrackHistoryID, SequenceNumber = 1, TimestampUtc = now.AddSeconds(3) },
                new TrackHistoryState() { TrackHistoryID = trackHistory.TrackHistoryID, SequenceNumber = 4, TimestampUtc = now.AddSeconds(1) },
                new TrackHistoryState() { TrackHistoryID = trackHistory.TrackHistoryID, SequenceNumber = 3, TimestampUtc = now.AddSeconds(4) },
            }) {
                _Database.TrackHistoryState_Save(state);
            }

            var states = _Database.TrackHistoryState_GetByTrackHistory(trackHistory);

            // Expected wrong orders:
            // SequenceNumbers  Issue
            // ---------------  -----
            // 2, 1, 4, 3       No sorting, result is in insert order
            // 4, 2, 1, 3       TimestampUtc ascending order
            // 3, 1, 2, 4       TimestampUtc descending order
            // 4, 3, 2, 1       SequenceNumber descending order

            var expected = "1, 2, 3, 4";
            var actual = String.Join(", ", states.Select(r => r.SequenceNumber.ToString()));
            Assert.AreEqual(expected, actual, $"Was expecting order of sequence numbers to be {expected}, was actually {actual}");
        }

        private TrackHistory SampleTrackHistoryForHistoryStates(string icao = "ABC123", bool isPreserved = false, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;

            var result = new TrackHistory() {
                AircraftID = AircraftIDForIcao(icao),
                IsPreserved = isPreserved,
                CreatedUtc = now,
                UpdatedUtc = now,
            };
            _Database.TrackHistory_Save(result);

            return result;
        }

        private List<TrackHistoryState> SampleTrackHistoryStates(TrackHistory parent, bool generateForCreate, DateTime? timestampUtc = null)
        {
            var result = new List<TrackHistoryState>();

            foreach(var property in typeof(TrackHistoryState).GetProperties()) {
                var state = new TrackHistoryState() {
                    TrackHistoryID = parent.TrackHistoryID,
                };

                object value = GenerateTrackHistoryStatePropertyValue(property, generateForCreate: true);

                if(value != null) {
                    property.SetValue(state, value);

                    if(result.Count == 0) {
                        state.SequenceNumber = 1;
                    } else {
                        state.SequenceNumber = result[result.Count - 1].SequenceNumber + 1;
                    }
                    state.TimestampUtc = timestampUtc ?? DateTime.UtcNow;

                    result.Add(state);
                }
            }

            return result;
        }

        private object GenerateTrackHistoryStatePropertyValue(PropertyInfo property, bool generateForCreate)
        {
            object value = null;

            var receiver = _Database.Receiver_GetOrCreateByName(generateForCreate ? "Evelyn" : "Paulton");

            switch(property.Name) {
                case nameof(TrackHistoryState.Callsign):    value = generateForCreate ? "BAW1" : "VIR25"; break;
                case nameof(TrackHistoryState.SpeedType):   value = generateForCreate ? SpeedType.GroundSpeedReversing : SpeedType.IndicatedAirSpeed; break;
                case nameof(TrackHistoryState.ReceiverID):  value = receiver.ReceiverID; break;

                case nameof(TrackHistoryState.Latitude):
                case nameof(TrackHistoryState.Longitude):
                    value = generateForCreate ? 1.2 : 7.4;
                    break;
                case nameof(TrackHistoryState.AirPressureInHg):
                case nameof(TrackHistoryState.GroundSpeedKnots):
                case nameof(TrackHistoryState.TargetTrack):
                case nameof(TrackHistoryState.TrackDegrees):
                    value = generateForCreate ? 1.2F : 7.4F;
                    break;
                case nameof(TrackHistoryState.AltitudeFeet):
                case nameof(TrackHistoryState.SignalLevel):
                case nameof(TrackHistoryState.SquawkOctal):
                case nameof(TrackHistoryState.TargetAltitudeFeet):
                case nameof(TrackHistoryState.VerticalRateFeetMin):
                    value = generateForCreate ? 25 : 50;
                    break;
                case nameof(TrackHistoryState.IdentActive):
                case nameof(TrackHistoryState.IsCallsignSuspect):
                case nameof(TrackHistoryState.IsMlat):
                case nameof(TrackHistoryState.IsTisb):
                case nameof(TrackHistoryState.TrackIsHeading):
                    value = generateForCreate;
                    break;
                case nameof(TrackHistoryState.AltitudeType):
                case nameof(TrackHistoryState.VerticalRateType):
                    value = generateForCreate ? AltitudeType.Geometric : AltitudeType.Barometric;
                    break;

                case nameof(TrackHistoryState.SequenceNumber):
                case nameof(TrackHistoryState.TimestampUtc):
                case nameof(TrackHistoryState.TrackHistoryID):
                case nameof(TrackHistoryState.TrackHistoryStateID):
                    break;

                default:
                    throw new NotImplementedException($"Need code for {nameof(TrackHistoryState)}.{property.Name}");
            }

            return value;
        }

        private void AssertTrackHistoryStatesAreEqual(TrackHistoryState expected, TrackHistoryState actual)
        {
            TestUtilities.TestObjectPropertiesAreEqual(expected, actual);
        }

        private void AssertTrackHistoryStatesAreEqual(IEnumerable<TrackHistoryState> expected, IEnumerable<TrackHistoryState> actual)
        {
            var expectedArray = expected?.ToArray() ?? new TrackHistoryState[0];
            var actualArray =   actual?.ToArray() ?? new TrackHistoryState[0];

            Assert.AreEqual(expectedArray.Length, actualArray.Length);
            for(var i = 0;i < expectedArray.Length;++i) {
                AssertTrackHistoryStatesAreEqual(expectedArray[i], actualArray[i]);
            }
        }
        #endregion

        #region WakeTurbulenceCategory
        public void WakeTurbulenceCategory_GetByID_Returns_Correct_WakeTurbulenceCategory()
        {
            foreach(WakeTurbulenceCategory enumWTC in Enum.GetValues(typeof(WakeTurbulenceCategory))) {
                var dbWTC = _Database.WakeTurbulenceCategory_GetByID(enumWTC);
                if(enumWTC == WakeTurbulenceCategory.None) {
                    Assert.IsNull(dbWTC);
                } else {
                    AssertWakeTurbulenceCategoryIsCorrect(enumWTC, dbWTC);
                }
            }
        }

        public void WakeTurbulenceCategory_GetAll_Returns_All_WakeTurbulenceCategory()
        {
            var allWTC = Enum.GetValues(typeof(WakeTurbulenceCategory))
                .OfType<WakeTurbulenceCategory>()
                .Where(r => r != WakeTurbulenceCategory.None)
                .ToArray();
            var actual = _Database.WakeTurbulenceCategory_GetAll().ToArray();
            Assert.AreEqual(allWTC.Length, actual.Length);

            foreach(WakeTurbulenceCategory enumWakeTurbulenceCategory in allWTC) {
                AssertWakeTurbulenceCategoryIsCorrect(enumWakeTurbulenceCategory, actual.Single(r => r.WakeTurbulenceCategoryID == enumWakeTurbulenceCategory));
            }
        }

        private static void AssertWakeTurbulenceCategoryIsCorrect(WakeTurbulenceCategory enumWTC, TrackHistoryWakeTurbulenceCategory dbWTC)
        {
            Assert.AreEqual(enumWTC, dbWTC.WakeTurbulenceCategoryID);

            var expectedCode = "";
            var expectedDesc = "";

            switch(enumWTC) {
                case WakeTurbulenceCategory.Heavy:  expectedCode = "H"; expectedDesc = "Heavy"; break;
                case WakeTurbulenceCategory.Light:  expectedCode = "L"; expectedDesc = "Light"; break;
                case WakeTurbulenceCategory.Medium: expectedCode = "M"; expectedDesc = "Medium"; break;
                default: throw new NotImplementedException($"Need code for {enumWTC}");
            }

            Assert.AreEqual(expectedCode, dbWTC.Code);
            Assert.AreEqual(expectedDesc, dbWTC.Description);
        }
        #endregion
    }
}
