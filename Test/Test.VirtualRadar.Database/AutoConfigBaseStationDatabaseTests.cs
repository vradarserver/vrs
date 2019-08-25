// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Database;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class AutoConfigBaseStationDatabaseTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IAutoConfigBaseStationDatabase _AutoConfigDatabase;
        private Mock<IBaseStationDatabase> _Database;
        private IClassFactory _ClassFactorySnapshot;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();

            _Database = TestUtilities.CreateMockImplementation<IBaseStationDatabase>();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(s => s.Load()).Returns(_Configuration);

            _AutoConfigDatabase = Factory.Resolve<IAutoConfigBaseStationDatabase>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void AutoConfigBaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            Assert.IsNull(_AutoConfigDatabase.Database);
        }

        [TestMethod]
        public void AutoConfigBaseStationDatabase_Singleton_Returns_Same_Instance_For_All_References()
        {
            var instance1 = Factory.Resolve<IAutoConfigBaseStationDatabase>();
            var instance2 = Factory.Resolve<IAutoConfigBaseStationDatabase>();

            Assert.IsNotNull(instance1.Singleton);
            Assert.AreNotSame(instance1, instance2);
            Assert.AreSame(instance1.Singleton, instance2.Singleton);
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void AutoConfigBaseStationDatabase_Initialise_Fills_Database_Property()
        {
            _AutoConfigDatabase.Initialise();
            Assert.AreSame(_Database.Object, _AutoConfigDatabase.Database);
        }

        [TestMethod]
        public void AutoConfigBaseStationDatabase_Initialise_Loads_Configuration_Into_Database()
        {
            _Configuration.BaseStationSettings.DatabaseFileName = "ABC";

            _AutoConfigDatabase.Initialise();

            Assert.AreEqual("ABC", _Database.Object.FileName);
        }
        #endregion

        #region Configuration Changes
        [TestMethod]
        public void AutoConfigBaseStationDatabase_Configuration_Change_Copies_FileName_To_Database()
        {
            _AutoConfigDatabase.Initialise();

            _Configuration.BaseStationSettings.DatabaseFileName = "xyz";
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual("xyz", _Database.Object.FileName);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void AutoConfigBaseStationDatabase_Dispose_Calls_Database_Dispose()
        {
            _AutoConfigDatabase.Initialise();

            _AutoConfigDatabase.Dispose();
            _Database.Verify(d => d.Dispose(), Times.Once());
        }
        #endregion
    }
}
