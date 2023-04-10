// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.KineticData;

namespace Test.VirtualRadar.Interface.Database
{
    [TestClass]
    public class KineticDBHistory_Tests
    {
        [TestMethod]
        [DataRow("Database autocreated by Snoopy", true)]
        [DataRow("DATABASE AUTOCREATED BY SNOOPY", true)]
        [DataRow("Database created by Virtual Radar Server", false)]
        public void IsCreationOfDatabaseByBaseStation_Returns_Correct_Value(string description, bool expected)
        {
            var record = new KineticDBHistory() { Description = description, };
            Assert.AreEqual(expected, record.IsCreationOfDatabaseByBaseStation);
        }

        [TestMethod]
        [DataRow("Database autocreated by Virtual Radar Server", true)]
        [DataRow("DATABASE AUTOCREATED BY VIRTUAL RADAR SERVER", true)]
        [DataRow("Database Database created by Snoopy", false)]
        public void IsCreationOfDatabaseByVirtualRadarServer_Returns_Correct_Value(string description, bool expected)
        {
            var record = new KineticDBHistory() { Description = description, };
            Assert.AreEqual(expected, record.IsCreationOfDatabaseByVirtualRadarServer);
        }
    }
}
