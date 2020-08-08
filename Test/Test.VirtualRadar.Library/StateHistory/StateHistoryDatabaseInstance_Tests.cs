using System;
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
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void Ctor_Initialises_To_Known_State()
        {
            Assert.IsNull(_DatabaseInstance.NonStandardFolder);
            Assert.IsNull(_DatabaseInstance.Repository);
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
                Assert.AreSame(_Repository.Object,      _DatabaseInstance.Repository);
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
    }
}
