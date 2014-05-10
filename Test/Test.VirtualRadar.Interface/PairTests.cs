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
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class PairTests
    {
        [TestMethod]
        public void Pair_Constructor_Initialises_To_Known_State()
        {
            var pair = new Pair<int>(1, 2);
            Assert.AreEqual(1, pair.First);
            Assert.AreEqual(2, pair.Second);
        }

        [TestMethod]
        public void Pair_Equals_Returns_True_If_Values_In_One_Pair_Equals_Values_In_Another_Pair()
        {
            var pair1 = new Pair<bool>(false, false);
            var pair2 = new Pair<bool>(true, false);
            var pair3 = new Pair<bool>(false, true);
            var pair4 = new Pair<bool>(true, true);

            var pair5 = new Pair<bool>(false, false);

            Assert.AreNotEqual(pair1, pair2);
            Assert.AreNotEqual(pair1, pair3);
            Assert.AreNotEqual(pair1, pair4);

            Assert.AreEqual(pair1, pair1);
            Assert.AreEqual(pair1, pair5);
        }

        [TestMethod]
        public void Pair_Equals_Can_Cope_When_Values_Are_Null()
        {
            var pair1 = new Pair<string>("A", "B");
            var pair2 = new Pair<string>(null, null);
            var pair3 = new Pair<string>("A", null);
            var pair4 = new Pair<string>(null, "B");
            var pair5 = new Pair<string>("A", "B");

            Assert.AreEqual(pair1, pair5);

            Assert.AreNotEqual(pair1, pair2);
            Assert.AreNotEqual(pair2, pair1);

            Assert.AreNotEqual(pair1, pair3);
            Assert.AreNotEqual(pair3, pair1);

            Assert.AreNotEqual(pair1, pair4);
            Assert.AreNotEqual(pair4, pair1);
        }

        [TestMethod]
        public void Pair_GetHashCode_Returns_Same_Value_For_Two_Objects_That_Compare_As_Equal()
        {
            var pair1 = new Pair<bool>(false, false);
            var pair2 = new Pair<bool>(false, false);

            Assert.AreEqual(pair1.GetHashCode(), pair2.GetHashCode());
        }

        [TestMethod]
        public void Pair_GetHashCode_Can_Cope_When_Values_Are_Null()
        {
            var pair1 = new Pair<string>("A", "B");
            var pair2 = new Pair<string>(null, null);
            var pair3 = new Pair<string>("A", null);
            var pair4 = new Pair<string>(null, "B");
            var pair5 = new Pair<string>("A", "B");

            Assert.AreEqual(pair1.GetHashCode(), pair5.GetHashCode());

            // The others can't be compared for equality because there's no rule to say that if two objects aren't equal
            // then their hashcodes must be different. However it should be able to calculate hashcodes for all of them
            // without any crashes.
            pair2.GetHashCode();
            pair3.GetHashCode();
            pair4.GetHashCode();
        }

        [TestMethod]
        public void Pair_ToString_Returns_Correct_Values()
        {
            Assert.AreEqual("1 / 2", new Pair<int>(1, 2).ToString());
            Assert.AreEqual(" / ", new Pair<string>(null, null).ToString());
        }
    }
}
