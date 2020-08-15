using System;
using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using Test.Framework;
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Library.StateHistory
{
    [TestClass]
    public class StateHistoryDatabaseInstance_Tests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory                   _Snapshot;
        private Mock<IStateHistoryRepository>   _Repository;
        private bool                            _RepositoryIsMissing;
        private IStateHistoryDatabaseInstance   _DatabaseInstance;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Repository =           TestUtilities.CreateMockImplementation<IStateHistoryRepository>();
            _RepositoryIsMissing =  false;

            _Repository
                .Setup(r => r.IsMissing)
                .Returns(() => _RepositoryIsMissing);
            _Repository
                .Setup(r => r.Initialise(It.IsAny<IStateHistoryDatabaseInstance>()))
                .Callback((IStateHistoryDatabaseInstance instance) => {
                    _Repository.Setup(r => r.WritesEnabled).Returns(instance.WritesEnabled);
                });

            _DatabaseInstance = Factory.Resolve<IStateHistoryDatabaseInstance>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _DatabaseInstance.Dispose();
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void Ctor_Initialises_To_Known_State()
        {
            Assert.IsNull(_DatabaseInstance.NonStandardFolder);
        }

        [TestMethod]
        public void Initialise_Sets_Properties()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Enabled = false,  NonStandardFolder = (string)null },
                new { Enabled = true,   NonStandardFolder = (string)null },
                new { Enabled = false,  NonStandardFolder = "" },
                new { Enabled = false,  NonStandardFolder = "Hello" },
            }, row => {
                _DatabaseInstance.Initialise(row.Enabled, row.NonStandardFolder);

                Assert.AreEqual(row.Enabled,            _DatabaseInstance.WritesEnabled);
                Assert.AreEqual(row.NonStandardFolder,  _DatabaseInstance.NonStandardFolder);
                _Repository.Verify(r => r.Initialise(_DatabaseInstance), Times.Once());
            });
        }

        [TestMethod]
        public void Initialise_Updates_The_Schema()
        {
            new InlineDataTest(this).TestAndAssert(new bool[] {
                true, false
            }, writesEnabled => {
                _DatabaseInstance.Initialise(writesEnabled, null);

                _Repository.Verify(r => r.Schema_Update(), writesEnabled
                    ? Times.Once()
                    : Times.Never()
                );
            });
        }

        [TestMethod]
        public void Initialise_Creates_DatabaseVersion_On_First_Run()
        {
            new InlineDataTest(this).TestAndAssert(new bool[] {
                true, false
            }, writesEnabled => {
                _Repository
                    .Setup(r => r.DatabaseVersion_GetLatest())
                    .Returns((DatabaseVersion)null);

                _DatabaseInstance.Initialise(writesEnabled, null);

                _Repository.Verify(r => r.DatabaseVersion_Save(It.IsAny<DatabaseVersion>()), writesEnabled
                    ? Times.Once()
                    : Times.Never()
                );
            });
        }

        [TestMethod]
        public void Initialise_Ignores_DatabaseVersion_If_Already_Exists()
        {
            _Repository
                .Setup(r => r.DatabaseVersion_GetLatest())
                .Returns(new DatabaseVersion() {
                    DatabaseVersionID = 1,
                    CreatedUtc =        DateTime.UtcNow.AddDays(-1)
                });

            _DatabaseInstance.Initialise(true, null);

            _Repository.Verify(r => r.DatabaseVersion_Save(It.IsAny<DatabaseVersion>()), Times.Never());
        }

        [TestMethod]
        public void Initialise_Creates_VrsSession()
        {
            new InlineDataTest(this).TestAndAssert(new bool[] {
                true, false
            }, writesEnabled => {
                VrsSession session = null;
                _Repository
                    .Setup(r => r.VrsSession_Insert(It.IsAny<VrsSession>()))
                    .Callback((VrsSession s) => session = s);

                _DatabaseInstance.Initialise(writesEnabled, null);

                _Repository.Verify(r => r.VrsSession_Insert(It.IsAny<VrsSession>()), writesEnabled
                    ? Times.Once()
                    : Times.Never()
                );
                if(writesEnabled) {
                    Assert.AreNotEqual(0, session.DatabaseVersionID);
                    Assert.IsTrue(session.CreatedUtc > DateTime.UtcNow.AddMinutes(-2));
                }
            });
        }

        [TestMethod]
        public void DoIfReadable_Calls_Action_When_Database_Is_Not_Missing()
        {
            new InlineDataTest(this).TestAndAssert(new bool[] {
                true,
                false,
            }, isMissing => {
                _RepositoryIsMissing = isMissing;
                _DatabaseInstance.Initialise(true, null);

                var called = false;
                var canReadDatabase = _DatabaseInstance.DoIfReadable((IStateHistoryRepository repo) => {
                    called = true;
                    Assert.AreSame(repo, _Repository.Object);
                });

                Assert.AreEqual(!isMissing, canReadDatabase);
                Assert.AreEqual(!isMissing, called);
            });
        }

        [TestMethod]
        public void DoIfWriteable_Calls_Action_When_Database_Is_Present_And_Writes_Are_Enabled()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { IsMissing = false, WritesEnabled = false, ExpectWriteable = false },
                new { IsMissing = false, WritesEnabled = true,  ExpectWriteable = true },
                new { IsMissing = true,  WritesEnabled = false, ExpectWriteable = false },
                new { IsMissing = true,  WritesEnabled = true,  ExpectWriteable = false },
            }, row => {
                _RepositoryIsMissing = row.IsMissing;
                _DatabaseInstance.Initialise(row.WritesEnabled, null);

                var called = false;
                var canReadDatabase = _DatabaseInstance.DoIfWriteable((IStateHistoryRepository repo) => {
                    called = true;
                    Assert.AreSame(repo, _Repository.Object);
                });

                Assert.AreEqual(row.ExpectWriteable, canReadDatabase);
                Assert.AreEqual(row.ExpectWriteable, called);
            });
        }

        /// <summary>
        /// Base class for GetOrCreate SnapshotRecord tests that common tests can use.
        /// </summary>
        /// <typeparam name="TSnapshot"></typeparam>
        abstract class GetOrCreate_TestParams<TSnapshot>
            where TSnapshot : SnapshotRecord
        {
            public abstract TSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi);

            public abstract TSnapshot CreateDummyRecord();

            public abstract Moq.Language.Flow.ISetup<IStateHistoryRepository, TSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository);
        }

        /// <summary>
        /// Common GetOrCreate test that confirms that null is always returned when writes are switched off.
        /// </summary>
        /// <typeparam name="TSnapshot"></typeparam>
        /// <param name="rows"></param>
        private void GetOrCreate_Returns_Null_If_Not_Writeable<TSnapshot>(GetOrCreate_TestParams<TSnapshot>[] rows)
            where TSnapshot: SnapshotRecord
        {
            new InlineDataTest(this).TestAndAssert(rows, row => {
                _DatabaseInstance.Initialise(false, null);
                Assert.IsNull(
                    row.CallGetOrCreate(_DatabaseInstance)
                );
            });
        }

        /// <summary>
        /// Common GetOrCreate test that confirms that the repository is called to get or create records.
        /// </summary>
        /// <typeparam name="TSnapshot"></typeparam>
        /// <param name="rows"></param>
        private void GetOrCreate_Calls_Repository_GetOrCreate<TSnapshot>(GetOrCreate_TestParams<TSnapshot>[] rows)
            where TSnapshot: SnapshotRecord
        {
            new InlineDataTest(this).TestAndAssert(rows, row => {
                var record = row.CreateDummyRecord();
                row.RepositorySetup(_Repository)
                   .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var actual = row.CallGetOrCreate(_DatabaseInstance);

                Assert.AreSame(record, actual);
            });
        }

        /// <summary>
        /// Common GetOrCreate test that confirms that once the repository has supplied a record for a fingerprint
        /// it will be cached.
        /// </summary>
        /// <typeparam name="TSnapshot"></typeparam>
        /// <param name="rows"></param>
        private void GetOrCreate_Caches_Results<TSnapshot>(GetOrCreate_TestParams<TSnapshot>[] rows)
            where TSnapshot: SnapshotRecord
        {
            new InlineDataTest(this).TestAndAssert(rows, row => {
                var record = row.CreateDummyRecord();
                var callCount = 0;

                row.RepositorySetup(_Repository)
                   .Callback(() => ++callCount)
                   .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var firstResult = row.CallGetOrCreate(_DatabaseInstance);
                var secondResult = row.CallGetOrCreate(_DatabaseInstance);

                Assert.AreSame(firstResult, secondResult);
                Assert.AreEqual(1, callCount);
            });
        }

        /// <summary>
        /// CountrySnapshot GetOrCreate tests
        /// </summary>
        class Country_GetOrCreate_TestParams : GetOrCreate_TestParams<CountrySnapshot>
        {
            public string CountryName { get; set; }

            public static Country_GetOrCreate_TestParams[] Rows = new Country_GetOrCreate_TestParams[] {
                new Country_GetOrCreate_TestParams() { CountryName = null },
                new Country_GetOrCreate_TestParams() { CountryName = "Airstrip One" },
            };

            public override CountrySnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Country_GetOrCreate(CountryName);

            public override CountrySnapshot CreateDummyRecord() => new CountrySnapshot() {
                CountryName = CountryName,
            };

            public override ISetup<IStateHistoryRepository, CountrySnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.CountrySnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(CountrySnapshot.TakeFingerprint(
                        CountryName
                    ))),
                    It.IsAny<DateTime>(),
                    CountryName
                ));
        }

        [TestMethod]
        public void Country_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(Country_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Country_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(Country_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Country_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(Country_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// EnginePlacementSnapshot GetOrCreate tests
        /// </summary>
        class EnginePlacement_GetOrCreate_TestParams : GetOrCreate_TestParams<EnginePlacementSnapshot>
        {
            public int EnumValue { get; set; }

            public string EnginePlacementName { get; set; }

            public static EnginePlacement_GetOrCreate_TestParams[] Rows = new EnginePlacement_GetOrCreate_TestParams[] {
                new EnginePlacement_GetOrCreate_TestParams() { EnumValue = 72, EnginePlacementName = "Nose", },
            };

            public override EnginePlacementSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.EnginePlacement_GetOrCreate(EnumValue, EnginePlacementName);

            public override EnginePlacementSnapshot CreateDummyRecord() => new EnginePlacementSnapshot() {
                EnumValue =             EnumValue,
                EnginePlacementName =   EnginePlacementName,
            };

            public override ISetup<IStateHistoryRepository, EnginePlacementSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.EnginePlacementSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(EnginePlacementSnapshot.TakeFingerprint(
                        EnumValue,
                        EnginePlacementName
                    ))),
                    It.IsAny<DateTime>(),
                    EnumValue,
                    EnginePlacementName
                ));
        }

        [TestMethod]
        public void EnginePlacement_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(EnginePlacement_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void EnginePlacement_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(EnginePlacement_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void EnginePlacement_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(EnginePlacement_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// EngineTypeSnapshot GetOrCreate tests
        /// </summary>
        class EngineType_GetOrCreate_TestParams : GetOrCreate_TestParams<EngineTypeSnapshot>
        {
            public int EnumValue { get; set; }

            public string EngineTypeName { get; set; }

            public static EngineType_GetOrCreate_TestParams[] Rows = new EngineType_GetOrCreate_TestParams[] {
                new EngineType_GetOrCreate_TestParams() { EnumValue = 72, EngineTypeName = "Piston", },
            };

            public override EngineTypeSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.EngineType_GetOrCreate(EnumValue, EngineTypeName);

            public override EngineTypeSnapshot CreateDummyRecord() => new EngineTypeSnapshot() {
                EnumValue =         EnumValue,
                EngineTypeName =    EngineTypeName,
            };

            public override ISetup<IStateHistoryRepository, EngineTypeSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.EngineTypeSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(EngineTypeSnapshot.TakeFingerprint(
                        EnumValue,
                        EngineTypeName
                    ))),
                    It.IsAny<DateTime>(),
                    EnumValue,
                    EngineTypeName
                ));
        }

        [TestMethod]
        public void EngineType_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(EngineType_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void EngineType_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(EngineType_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void EngineType_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(EngineType_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// ManufacturerSnapshot GetOrCreate tests
        /// </summary>
        class Manufacturer_GetOrCreate_TestParams : GetOrCreate_TestParams<ManufacturerSnapshot>
        {
            public string ManufacturerName { get; set; }

            public static Manufacturer_GetOrCreate_TestParams[] Rows = new Manufacturer_GetOrCreate_TestParams[] {
                new Manufacturer_GetOrCreate_TestParams() { ManufacturerName = null },
                new Manufacturer_GetOrCreate_TestParams() { ManufacturerName = "Boeing" },
            };

            public override ManufacturerSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Manufacturer_GetOrCreate(ManufacturerName);

            public override ManufacturerSnapshot CreateDummyRecord() => new ManufacturerSnapshot() {
                ManufacturerName = ManufacturerName,
            };

            public override ISetup<IStateHistoryRepository, ManufacturerSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.ManufacturerSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(ManufacturerSnapshot.TakeFingerprint(
                        ManufacturerName
                    ))),
                    It.IsAny<DateTime>(),
                    ManufacturerName
                ));
        }

        [TestMethod]
        public void Manufacturer_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(Manufacturer_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Manufacturer_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(Manufacturer_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Manufacturer_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(Manufacturer_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// OperatorSnapshot GetOrCreate tests
        /// </summary>
        class Operator_GetOrCreate_TestParams : GetOrCreate_TestParams<OperatorSnapshot>
        {
            public string Icao { get; set; }

            public string OperatorName { get; set; }

            public static Operator_GetOrCreate_TestParams[] Rows = new Operator_GetOrCreate_TestParams[] {
                new Operator_GetOrCreate_TestParams() { Icao = null,  OperatorName = null },
                new Operator_GetOrCreate_TestParams() { Icao = "VIR", OperatorName = "Virgin Atlantic" },
            };

            public override OperatorSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Operator_GetOrCreate(Icao, OperatorName);

            public override OperatorSnapshot CreateDummyRecord() => new OperatorSnapshot() {
                Icao =         Icao,
                OperatorName = OperatorName,
            };

            public override ISetup<IStateHistoryRepository, OperatorSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.OperatorSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(OperatorSnapshot.TakeFingerprint(
                        Icao,
                        OperatorName
                    ))),
                    It.IsAny<DateTime>(),
                    Icao,
                    OperatorName
                ));
        }

        [TestMethod]
        public void Operator_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(Operator_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Operator_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(Operator_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Operator_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(Operator_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// WakeTurbulenceCategorySnapshot GetOrCreate tests
        /// </summary>
        class WakeTurbulenceCategory_GetOrCreate_TestParams : GetOrCreate_TestParams<WakeTurbulenceCategorySnapshot>
        {
            public int EnumValue { get; set; }

            public string WakeTurbulenceCategoryName { get; set; }

            public static WakeTurbulenceCategory_GetOrCreate_TestParams[] Rows = new WakeTurbulenceCategory_GetOrCreate_TestParams[] {
                new WakeTurbulenceCategory_GetOrCreate_TestParams() { EnumValue = 72, WakeTurbulenceCategoryName = "Heavy", },
            };

            public override WakeTurbulenceCategorySnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.WakeTurbulenceCategory_GetOrCreate(EnumValue, WakeTurbulenceCategoryName);

            public override WakeTurbulenceCategorySnapshot CreateDummyRecord() => new WakeTurbulenceCategorySnapshot() {
                EnumValue =                     EnumValue,
                WakeTurbulenceCategoryName =    WakeTurbulenceCategoryName,
            };

            public override ISetup<IStateHistoryRepository, WakeTurbulenceCategorySnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.WakeTurbulenceCategorySnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(WakeTurbulenceCategorySnapshot.TakeFingerprint(
                        EnumValue,
                        WakeTurbulenceCategoryName
                    ))),
                    It.IsAny<DateTime>(),
                    EnumValue,
                    WakeTurbulenceCategoryName
                ));
        }

        [TestMethod]
        public void WakeTurbulenceCategory_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(WakeTurbulenceCategory_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void WakeTurbulenceCategory_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(WakeTurbulenceCategory_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void WakeTurbulenceCategory_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(WakeTurbulenceCategory_GetOrCreate_TestParams.Rows);
    }
}
