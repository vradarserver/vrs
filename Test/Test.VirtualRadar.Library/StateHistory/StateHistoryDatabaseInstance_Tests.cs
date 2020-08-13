using System;
using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

        [TestMethod]
        public void Operator_GetOrCreate_Returns_Null_If_Not_Writeable()
        {
            _DatabaseInstance.Initialise(false, null);
            Assert.IsNull(_DatabaseInstance.Operator_GetOrCreate("ABC", "Def"));
        }

        [TestMethod]
        public void Operator_GetOrCreate_Calls_Repository_GetOrCreate()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Icao = (string)null,  OperatorName = (string)null, },
                new { Icao = "VIR",         OperatorName = "Virgin Atlantic", },
            }, row => {
                var record = new OperatorSnapshot();
                _Repository
                    .Setup(r => r.OperatorSnapshot_GetOrCreate(
                        It.Is<byte[]>(p => p.SequenceEqual(OperatorSnapshot.TakeFingerprint(
                            row.Icao,
                            row.OperatorName
                        ))),
                        It.IsAny<DateTime>(),
                        row.Icao,
                        row.OperatorName
                    ))
                    .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var actual = _DatabaseInstance.Operator_GetOrCreate(
                    row.Icao,
                    row.OperatorName
                );

                Assert.AreSame(record, actual);
            });
        }

        [TestMethod]
        public void Operator_GetOrCreate_Caches_Results()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Icao = "BAW", OperatorName = "British Airways", },
            }, row => {
                var record = new OperatorSnapshot() {
                    Icao =          row.Icao,
                    OperatorName =  row.OperatorName,
                };
                var callCount = 0;
                _Repository
                    .Setup(r => r.OperatorSnapshot_GetOrCreate(
                        It.IsAny<byte[]>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ))
                    .Callback(() => ++callCount)
                    .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var firstResult = _DatabaseInstance.Operator_GetOrCreate(
                    row.Icao,
                    row.OperatorName
                );

                var secondResult = _DatabaseInstance.Operator_GetOrCreate(
                    row.Icao,
                    row.OperatorName
                );

                Assert.AreSame(firstResult, secondResult);
                Assert.AreEqual(1, callCount);
            });
        }

        [TestMethod]
        public void Country_GetOrCreate_Returns_Null_If_Not_Writeable()
        {
            _DatabaseInstance.Initialise(false, null);
            Assert.IsNull(_DatabaseInstance.Country_GetOrCreate("Abc"));
        }

        [TestMethod]
        public void Country_GetOrCreate_Calls_Repository_GetOrCreate()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { CountryName = (string)null, },
                new { CountryName = "Airstrip One", },
            }, row => {
                var record = new CountrySnapshot();
                _Repository
                    .Setup(r => r.CountrySnapshot_GetOrCreate(
                        It.Is<byte[]>(p => p.SequenceEqual(CountrySnapshot.TakeFingerprint(
                            row.CountryName
                        ))),
                        It.IsAny<DateTime>(),
                        row.CountryName
                    ))
                    .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var actual = _DatabaseInstance.Country_GetOrCreate(
                    row.CountryName
                );

                Assert.AreSame(record, actual);
            });
        }

        [TestMethod]
        public void Country_GetOrCreate_Caches_Results()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { CountryName = "Nod", },
            }, row => {
                var record = new CountrySnapshot() {
                    CountryName = row.CountryName,
                };
                var callCount = 0;
                _Repository
                    .Setup(r => r.CountrySnapshot_GetOrCreate(
                        It.IsAny<byte[]>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>()
                    ))
                    .Callback(() => ++callCount)
                    .Returns(record);
                _DatabaseInstance.Initialise(writesEnabled: true, nonStandardFolder: null);

                var firstResult = _DatabaseInstance.Country_GetOrCreate(
                    row.CountryName
                );

                var secondResult = _DatabaseInstance.Country_GetOrCreate(
                    row.CountryName
                );

                Assert.AreSame(firstResult, secondResult);
                Assert.AreEqual(1, callCount);
            });
        }
    }
}
