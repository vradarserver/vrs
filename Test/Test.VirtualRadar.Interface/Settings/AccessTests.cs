// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class AccessTests
    {
        private Access _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = new Access();
        }

        [TestMethod]
        public void Access_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(_Implementation);
        }

        public static void CheckProperties(Access settings)
        {
            Assert.AreEqual(0, settings.Addresses.Count);
            TestUtilities.TestProperty(settings, r => r.DefaultAccess, DefaultAccess.Unrestricted, DefaultAccess.Deny);
        }

        [TestMethod]
        public void Access_Equals_Compares_True_When_Two_Access_Objects_Have_The_Same_Properties()
        {
            Assert.IsFalse(_Implementation.Equals(null));
            Assert.IsTrue(_Implementation.Equals(new Access()));

            var obj1 = new Access() { DefaultAccess = DefaultAccess.Allow, Addresses = { "ADDR1", "ADDR2" } };
            foreach(var propertyName in typeof(Access).GetProperties().Select(r => r.Name)) {
                var obj2 = new Access() { DefaultAccess = DefaultAccess.Allow, Addresses = { "ADDR1", "ADDR2" } };
                Assert.IsTrue(obj1.Equals(obj2));

                switch(propertyName) {
                    case "Addresses":           obj2.Addresses.Clear(); break;
                    case "DefaultAccess":       obj2.DefaultAccess = DefaultAccess.Deny; break;
                    default:                    throw new NotImplementedException();
                }
                Assert.IsFalse(obj1.Equals(obj2), propertyName);
            }
        }
    }
}
