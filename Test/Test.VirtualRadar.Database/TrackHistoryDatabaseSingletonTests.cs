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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class TrackHistoryDatabaseSingletonTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private ITrackHistoryDatabaseSingleton _Singleton;
        private Configuration _Configuration;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _Configuration = new Configuration();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);

            _Singleton = Factory.ResolveNewInstance<ITrackHistoryDatabaseSingleton>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_Singleton != null) {
                _Singleton.Dispose();
            }
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void TrackHistoryDatabaseSingleton_Database_Is_Initialised_By_Initialise()
        {
            Assert.IsNull(_Singleton.Database);

            _Singleton.Initialise();

            Assert.IsNotNull(_Singleton.Database);
        }

        [TestMethod]
        public void TrackHistoryDatabaseSingleton_Initialise_Will_Not_Reinitilise_Database()
        {
            _Singleton.Initialise();
            var reference = _Singleton.Database;

            _Singleton.Initialise();
            Assert.AreSame(reference, _Singleton.Database);
        }
    }
}
