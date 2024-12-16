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
using VirtualRadar.Interface.Database;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Database
{
    [TestClass]
    public class BaseStationDBHistoryTests
    {
        [TestMethod]
        public void BaseStationDBHistory_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var record = new BaseStationDBHistory();

            TestUtilities.TestProperty(record, "DBHistoryID", 0, 1202);
            TestUtilities.TestProperty(record, "Description", null, "ANV");
            TestUtilities.TestProperty(record, "TimeStamp", DateTime.MinValue, DateTime.Now);
        }

        [TestMethod]
        public void BaseStationDBHistory_IsCreationOfDatabaseByBaseStation_Returns_True_When_Matching_Description_Seen()
        {
            var record = new BaseStationDBHistory();
            Assert.IsFalse(record.IsCreationOfDatabaseByBaseStation);

            record.Description = "Database autocreated by Snoopy";
            Assert.IsTrue(record.IsCreationOfDatabaseByBaseStation);

            record.Description = "DATABASE AUTOCREATED BY SNOOPY";
            Assert.IsTrue(record.IsCreationOfDatabaseByBaseStation);

            record.Description = "Database created by Virtual Radar Server";
            Assert.IsFalse(record.IsCreationOfDatabaseByBaseStation);
        }

        [TestMethod]
        public void BaseStationDBHistory_IsCreationOfDatabaseByVirtualRadarServer_Returns_True_When_Matching_Description_Seen()
        {
            var record = new BaseStationDBHistory();
            Assert.IsFalse(record.IsCreationOfDatabaseByVirtualRadarServer);

            record.Description = "Database autocreated by Virtual Radar Server";
            Assert.IsTrue(record.IsCreationOfDatabaseByVirtualRadarServer);

            record.Description = "DATABASE AUTOCREATED BY VIRTUAL RADAR SERVER";
            Assert.IsTrue(record.IsCreationOfDatabaseByVirtualRadarServer);

            record.Description = "Database created by Snoopy";
            Assert.IsFalse(record.IsCreationOfDatabaseByVirtualRadarServer);
        }
    }
}