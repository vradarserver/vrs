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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class HashTests
    {
        private Hash _Hash;

        [TestInitialize]
        public void TestInitialise()
        {
            _Hash = new Hash();
        }

        [TestMethod]
        public void Hash_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0, _Hash.Buffer.Count);
            TestUtilities.TestProperty(_Hash, "Version", Hash.LatestVersion, Hash.LatestVersion - 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Hash_String_Throws_If_Text_Is_Null()
        {
            new Hash(null);
        }

        [TestMethod]
        public void Hash_String_Creates_Latest_Hash()
        {
            _Hash = new Hash("Hello");
            Assert.AreEqual(Hash.LatestVersion, _Hash.Version);
            Assert.AreNotEqual(0, _Hash.Buffer.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Hash_PasswordMatches_Throws_If_Passed_Null()
        {
            _Hash.PasswordMatches(null);
        }

        [TestMethod]
        public void Hash_PasswordMatches_Returns_True_If_Password_Matches()
        {
            _Hash = new Hash("This Is My Password");
            Assert.AreEqual(true, _Hash.PasswordMatches("This Is My Password"));
        }

        [TestMethod]
        public void Hash_PasswordMatches_Returns_False_If_Password_Does_Not_Match()
        {
            _Hash = new Hash("This Is My Password");
            Assert.AreEqual(false, _Hash.PasswordMatches("This is My Password"));
        }

        [TestMethod]
        public void Hash_PasswordMatches_Accepts_Empty_String_Passwords()
        {
            _Hash = new Hash("");
            Assert.AreEqual(true, _Hash.PasswordMatches(""));
        }
    }
}
