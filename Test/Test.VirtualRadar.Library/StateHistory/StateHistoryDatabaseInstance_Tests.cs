using System;
using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using Test.Framework;
using VirtualRadar.Interface.StandingData;
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
            public abstract bool ExpectNullSnapshot();

            public abstract TSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi);

            public abstract TSnapshot CreateDummyRecord(long id);

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
                TSnapshot record = null;

                if(!row.ExpectNullSnapshot()) {
                    record = row.CreateDummyRecord(1);
                    row.RepositorySetup(_Repository)
                       .Returns(record);
                }

                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);
                var actual = row.CallGetOrCreate(_DatabaseInstance);

                if(!row.ExpectNullSnapshot()) {
                    Assert.AreSame(record, actual);
                } else {
                    Assert.IsNull(actual);
                }
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
                TSnapshot record = null;
                var callCount = 0;

                if(!row.ExpectNullSnapshot()) {
                    record = row.CreateDummyRecord(1);

                    row.RepositorySetup(_Repository)
                       .Callback(() => ++callCount)
                       .Returns(record);
                }

                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);
                var firstResult = row.CallGetOrCreate(_DatabaseInstance);
                var secondResult = row.CallGetOrCreate(_DatabaseInstance);

                if(!row.ExpectNullSnapshot()) {
                    Assert.AreSame(firstResult, secondResult);
                    Assert.AreEqual(1, callCount);
                } else {
                    Assert.IsNull(firstResult);
                    Assert.IsNull(secondResult);
                }
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
                new Country_GetOrCreate_TestParams() { CountryName = "" },
                new Country_GetOrCreate_TestParams() { CountryName = "Airstrip One" },
            };

            public override bool ExpectNullSnapshot() => CountryName == null;

            public override CountrySnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Country_GetOrCreate(CountryName);

            public override CountrySnapshot CreateDummyRecord(long id) => new CountrySnapshot() {
                CountrySnapshotID = id,
                CountryName =       CountryName,
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
            public EnginePlacement? EnginePlacement { get; set; }

            public static EnginePlacement_GetOrCreate_TestParams[] Rows = new EnginePlacement_GetOrCreate_TestParams[] {
                new EnginePlacement_GetOrCreate_TestParams() { EnginePlacement = null, },
                new EnginePlacement_GetOrCreate_TestParams() { EnginePlacement = global::VirtualRadar.Interface.StandingData.EnginePlacement.FuselageBuried, },
            };

            public override bool ExpectNullSnapshot() => EnginePlacement == null;

            public override EnginePlacementSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.EnginePlacement_GetOrCreate(EnginePlacement);

            public override EnginePlacementSnapshot CreateDummyRecord(long id) => new EnginePlacementSnapshot() {
                EnginePlacementSnapshotID = id,
                EnumValue =                 (int)EnginePlacement,
                EnginePlacementName =       EnginePlacement.ToString(),
            };

            public override ISetup<IStateHistoryRepository, EnginePlacementSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.EnginePlacementSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(EnginePlacementSnapshot.TakeFingerprint(
                        (int)EnginePlacement,
                        EnginePlacement.ToString()
                    ))),
                    It.IsAny<DateTime>(),
                    (int)EnginePlacement,
                    EnginePlacement.ToString()
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
            public EngineType? EngineType { get; set; }

            public static EngineType_GetOrCreate_TestParams[] Rows = new EngineType_GetOrCreate_TestParams[] {
                new EngineType_GetOrCreate_TestParams() { EngineType = null, },
                new EngineType_GetOrCreate_TestParams() { EngineType = global::VirtualRadar.Interface.StandingData.EngineType.Piston, },
            };

            public override bool ExpectNullSnapshot() => EngineType == null;

            public override EngineTypeSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.EngineType_GetOrCreate(EngineType);

            public override EngineTypeSnapshot CreateDummyRecord(long id) => new EngineTypeSnapshot() {
                EngineTypeSnapshotID =  id,
                EnumValue =             (int)EngineType,
                EngineTypeName =        EngineType.ToString(),
            };

            public override ISetup<IStateHistoryRepository, EngineTypeSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.EngineTypeSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(EngineTypeSnapshot.TakeFingerprint(
                        (int)EngineType,
                        EngineType.ToString()
                    ))),
                    It.IsAny<DateTime>(),
                    (int)EngineType,
                    EngineType.ToString()
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
                new Manufacturer_GetOrCreate_TestParams() { ManufacturerName = "" },
                new Manufacturer_GetOrCreate_TestParams() { ManufacturerName = "Boeing" },
            };

            public override bool ExpectNullSnapshot() => ManufacturerName == null;

            public override ManufacturerSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Manufacturer_GetOrCreate(ManufacturerName);

            public override ManufacturerSnapshot CreateDummyRecord(long id) => new ManufacturerSnapshot() {
                ManufacturerSnapshotID =    id,
                ManufacturerName =          ManufacturerName,
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
        /// ModelSnapshot GetOrCreate tests
        /// </summary>
        /// <remarks>
        /// These do not use the common tests, the child records introduce complications that the common tests can't cope with.
        /// There are only a small number of snapshots with child records, I'm not going to have common tests for them.
        /// </remarks>
        class Model_GetOrCreate_TestParams
        {
            public string Icao { get; set; }

            public string ModelName { get; set; }

            public string NumberOfEngines { get; set; }

            public static Model_GetOrCreate_TestParams[] Rows = new Model_GetOrCreate_TestParams[] {
                new Model_GetOrCreate_TestParams() { Icao = null,   ModelName = null,       NumberOfEngines = null, },
                new Model_GetOrCreate_TestParams() { Icao = "",     ModelName = "",         NumberOfEngines = "", },
                new Model_GetOrCreate_TestParams() { Icao = "B788", ModelName = "B747-800", NumberOfEngines = "4", },
            };
        }

        [TestMethod]
        public void Model_GetOrCreate_Returns_Null_If_Not_Writeable()
        {
            new InlineDataTest(this).TestAndAssert(Model_GetOrCreate_TestParams.Rows, row => {
                _DatabaseInstance.Initialise(false, null);
                Assert.IsNull(
                    _DatabaseInstance.Model_GetOrCreate(
                        row.Icao,
                        row.ModelName,
                        row.NumberOfEngines,
                        null,
                        null,
                        null,
                        null,
                        null
                    )
                );
            });
        }

        [TestMethod]
        public void Model_GetOrCreate_Calls_Repository_GetOrCreate()
        {
            new InlineDataTest(this).TestAndAssert(Model_GetOrCreate_TestParams.Rows, row => {
                foreach(var manufacturer in Manufacturer_GetOrCreate_TestParams.Rows) {
                    foreach(var wakeTurbulenceCategory in WakeTurbulenceCategory_GetOrCreate_TestParams.Rows) {
                        foreach(var engineType in EngineType_GetOrCreate_TestParams.Rows) {
                            foreach(var enginePlacement in EnginePlacement_GetOrCreate_TestParams.Rows) {
                                foreach(var species in Species_GetOrCreate_TestParams.Rows) {

                                    TestCleanup();
                                    TestInitialise();

                                    ManufacturerSnapshot manufacturerSnapshot = null;
                                    if(!manufacturer.ExpectNullSnapshot()) {
                                        manufacturerSnapshot = manufacturer.CreateDummyRecord(12);
                                        manufacturer.RepositorySetup(_Repository).Returns(manufacturerSnapshot);
                                    }

                                    WakeTurbulenceCategorySnapshot wtcSnapshot = null;
                                    if(!wakeTurbulenceCategory.ExpectNullSnapshot()) {
                                        wtcSnapshot = wakeTurbulenceCategory.CreateDummyRecord(81);
                                        wakeTurbulenceCategory.RepositorySetup(_Repository).Returns(wtcSnapshot);
                                    }

                                    EngineTypeSnapshot engineTypeSnapshot = null;
                                    if(!engineType.ExpectNullSnapshot()) {
                                        engineTypeSnapshot = engineType.CreateDummyRecord(67);
                                        engineType.RepositorySetup(_Repository).Returns(engineTypeSnapshot);
                                    }

                                    EnginePlacementSnapshot enginePlacementSnapshot = null;
                                    if(!enginePlacement.ExpectNullSnapshot()) {
                                        enginePlacementSnapshot = enginePlacement.CreateDummyRecord(33);
                                        enginePlacement.RepositorySetup(_Repository).Returns(enginePlacementSnapshot);
                                    }

                                    SpeciesSnapshot speciesSnapshot = null;
                                    if(!species.ExpectNullSnapshot()) {
                                        speciesSnapshot = species.CreateDummyRecord(43);
                                        species.RepositorySetup(_Repository).Returns(speciesSnapshot);
                                    }

                                    var expected = new ModelSnapshot() {
                                        Icao =                          row.Icao,
                                        ModelName =                     row.ModelName,
                                        Fingerprint =                   ModelSnapshot.TakeFingerprint(
                                                                            row.Icao,
                                                                            row.ModelName,
                                                                            row.NumberOfEngines,
                                                                            manufacturerSnapshot?.ManufacturerSnapshotID,
                                                                            wtcSnapshot?.WakeTurbulenceCategorySnapshotID,
                                                                            engineTypeSnapshot?.EngineTypeSnapshotID,
                                                                            enginePlacementSnapshot?.EnginePlacementSnapshotID,
                                                                            speciesSnapshot?.SpeciesSnapshotID
                                                                        ),
                                        EnginePlacementSnapshotID =     enginePlacementSnapshot?.EnginePlacementSnapshotID,
                                        EngineTypeSnapshotID =          engineTypeSnapshot?.EngineTypeSnapshotID,
                                        ManufacturerSnapshotID =        manufacturerSnapshot?.ManufacturerSnapshotID,
                                        NumberOfEngines =               row.NumberOfEngines,
                                        SpeciesSnapshotID =             speciesSnapshot?.SpeciesSnapshotID,
                                        WakeTurbulenceCodeSnapshotID =  wtcSnapshot?.WakeTurbulenceCategorySnapshotID,
                                    };

                                    var expectNull = expected.Icao == null
                                        && expected.ModelName == null
                                        && expected.EnginePlacementSnapshotID == null
                                        && expected.EngineTypeSnapshotID  == null
                                        && expected.ManufacturerSnapshotID == null
                                        && expected.NumberOfEngines == null
                                        && expected.SpeciesSnapshotID == null
                                        && expected.WakeTurbulenceCodeSnapshotID == null;

                                    _Repository.Setup(r => r.ModelSnapshot_GetOrCreate(
                                        It.Is<byte[]>(p => p.SequenceEqual(expected.Fingerprint)),
                                        It.IsAny<DateTime>(),
                                        expected.Icao,
                                        expected.ModelName,
                                        expected.NumberOfEngines,
                                        expected.ManufacturerSnapshotID,
                                        expected.WakeTurbulenceCodeSnapshotID,
                                        expected.EngineTypeSnapshotID,
                                        expected.EnginePlacementSnapshotID,
                                        expected.SpeciesSnapshotID
                                    ))
                                    .Returns(expected);

                                    _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);
                                    var actual = _DatabaseInstance.Model_GetOrCreate(
                                        row.Icao,
                                        row.ModelName,
                                        row.NumberOfEngines,
                                        manufacturer.ManufacturerName,
                                        wakeTurbulenceCategory.WakeTurbulenceCategory,
                                        engineType.EngineType,
                                        enginePlacement.EnginePlacement,
                                        species.Species
                                    );

                                    if(!expectNull) {
                                        Assert.AreSame(expected, actual);
                                    } else {
                                        Assert.IsNull(actual);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }


        /// <summary>
        /// OperatorSnapshot GetOrCreate tests
        /// </summary>
        class Operator_GetOrCreate_TestParams : GetOrCreate_TestParams<OperatorSnapshot>
        {
            public string Icao { get; set; }

            public string OperatorName { get; set; }

            public static Operator_GetOrCreate_TestParams[] Rows = new Operator_GetOrCreate_TestParams[] {
                new Operator_GetOrCreate_TestParams() { Icao = null,  OperatorName = null },
                new Operator_GetOrCreate_TestParams() { Icao = "",    OperatorName = "" },
                new Operator_GetOrCreate_TestParams() { Icao = "VIR", OperatorName = "Virgin Atlantic" },
            };

            public override bool ExpectNullSnapshot() => Icao == null && OperatorName == null;

            public override OperatorSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Operator_GetOrCreate(Icao, OperatorName);

            public override OperatorSnapshot CreateDummyRecord(long id) => new OperatorSnapshot() {
                OperatorSnapshotID =    id,
                Icao =                  Icao,
                OperatorName =          OperatorName,
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
        /// ReceiverSnapshot GetOrCreate tests
        /// </summary>
        class Receiver_GetOrCreate_TestParams : GetOrCreate_TestParams<ReceiverSnapshot>
        {
            public int? ReceiverID { get; set; }

            public Guid? Key { get; set; }

            public string ReceiverName { get; set; }

            public static Receiver_GetOrCreate_TestParams[] Rows = new Receiver_GetOrCreate_TestParams[] {
                new Receiver_GetOrCreate_TestParams() { ReceiverID = null, Key = null,           ReceiverName = null },
                new Receiver_GetOrCreate_TestParams() { ReceiverID = null, Key = null,           ReceiverName = "" },
                new Receiver_GetOrCreate_TestParams() { ReceiverID = 1,    Key = null,           ReceiverName = "" },
                new Receiver_GetOrCreate_TestParams() { ReceiverID = 1,    Key = Guid.NewGuid(), ReceiverName = "" },
                new Receiver_GetOrCreate_TestParams() { ReceiverID = 1,    Key = Guid.NewGuid(), ReceiverName = "Home" },
            };

            public override bool ExpectNullSnapshot() => ReceiverID == null || Key == null;

            public override ReceiverSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Receiver_GetOrCreate(ReceiverID, Key, ReceiverName);

            public override ReceiverSnapshot CreateDummyRecord(long id) => new ReceiverSnapshot() {
                ReceiverSnapshotID = id,
            };

            public override ISetup<IStateHistoryRepository, ReceiverSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.ReceiverSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(ReceiverSnapshot.TakeFingerprint(
                        (int)ReceiverID,
                        (Guid)Key,
                        ReceiverName
                    ))),
                    It.IsAny<DateTime>(),
                    (int)ReceiverID,
                    (Guid)Key,
                    ReceiverName
                ));
        }

        [TestMethod]
        public void Receiver_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(Receiver_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Receiver_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(Receiver_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Receiver_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(Receiver_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// SpeciesSnapshot GetOrCreate tests
        /// </summary>
        class Species_GetOrCreate_TestParams : GetOrCreate_TestParams<SpeciesSnapshot>
        {
            public Species? Species { get; set; }

            public static Species_GetOrCreate_TestParams[] Rows = new Species_GetOrCreate_TestParams[] {
                new Species_GetOrCreate_TestParams() { Species = null, },
                new Species_GetOrCreate_TestParams() { Species = global::VirtualRadar.Interface.StandingData.Species.Helicopter, },
            };

            public override bool ExpectNullSnapshot() => Species == null;

            public override SpeciesSnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.Species_GetOrCreate(Species);

            public override SpeciesSnapshot CreateDummyRecord(long id) => new SpeciesSnapshot() {
                SpeciesSnapshotID = id,
                EnumValue =         (int)Species,
                SpeciesName =       Species.ToString(),
            };

            public override ISetup<IStateHistoryRepository, SpeciesSnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.SpeciesSnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(SpeciesSnapshot.TakeFingerprint(
                        (int)Species,
                        Species.ToString()
                    ))),
                    It.IsAny<DateTime>(),
                    (int)Species,
                    Species.ToString()
                ));
        }

        [TestMethod]
        public void Species_GetOrCreate_Returns_Null_If_Not_Writeable() => GetOrCreate_Returns_Null_If_Not_Writeable(Species_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Species_GetOrCreate_Calls_Repository_GetOrCreate() => GetOrCreate_Calls_Repository_GetOrCreate(Species_GetOrCreate_TestParams.Rows);

        [TestMethod]
        public void Species_GetOrCreate_Caches_Results() => GetOrCreate_Caches_Results(Species_GetOrCreate_TestParams.Rows);


        /// <summary>
        /// WakeTurbulenceCategorySnapshot GetOrCreate tests
        /// </summary>
        class WakeTurbulenceCategory_GetOrCreate_TestParams : GetOrCreate_TestParams<WakeTurbulenceCategorySnapshot>
        {
            public WakeTurbulenceCategory? WakeTurbulenceCategory { get; set; }

            public static WakeTurbulenceCategory_GetOrCreate_TestParams[] Rows = new WakeTurbulenceCategory_GetOrCreate_TestParams[] {
                new WakeTurbulenceCategory_GetOrCreate_TestParams() { WakeTurbulenceCategory = null, },
                new WakeTurbulenceCategory_GetOrCreate_TestParams() { WakeTurbulenceCategory = global::VirtualRadar.Interface.StandingData.WakeTurbulenceCategory.Heavy, },
            };

            public override bool ExpectNullSnapshot() => WakeTurbulenceCategory == null;

            public override WakeTurbulenceCategorySnapshot CallGetOrCreate(IStateHistoryDatabaseInstance dbi) => dbi.WakeTurbulenceCategory_GetOrCreate(WakeTurbulenceCategory);

            public override WakeTurbulenceCategorySnapshot CreateDummyRecord(long id) => new WakeTurbulenceCategorySnapshot() {
                WakeTurbulenceCategorySnapshotID =  id,
                EnumValue =                         (int)WakeTurbulenceCategory,
                WakeTurbulenceCategoryName =        WakeTurbulenceCategory.ToString(),
            };

            public override ISetup<IStateHistoryRepository, WakeTurbulenceCategorySnapshot> RepositorySetup(Mock<IStateHistoryRepository> repository) => repository
                .Setup(r => r.WakeTurbulenceCategorySnapshot_GetOrCreate(
                    It.Is<byte[]>(p => p.SequenceEqual(WakeTurbulenceCategorySnapshot.TakeFingerprint(
                        (int)WakeTurbulenceCategory,
                        WakeTurbulenceCategory.ToString()
                    ))),
                    It.IsAny<DateTime>(),
                    (int)WakeTurbulenceCategory,
                    WakeTurbulenceCategory.ToString()
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
