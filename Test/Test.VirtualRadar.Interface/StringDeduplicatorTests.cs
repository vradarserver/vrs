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
    public class StringDeduplicatorTests
    {
        private StringDeduplicator _StringDeduplicator;

        [TestInitialize]
        public void TestInitialise()
        {
            _StringDeduplicator = new StringDeduplicator();
        }

        [TestMethod]
        public void StringDeduplicator_Deduplicate_Returns_Null_If_Passed_Null()
        {
            Assert.AreEqual(null, _StringDeduplicator.Deduplicate((string)null));
        }

        [TestMethod]
        public void StringDeduplicator_Deduplicate_Returns_Same_String_If_Not_Duplicate()
        {
            string t1 = "Text";
            Assert.AreSame(t1, _StringDeduplicator.Deduplicate(t1));
        }

        [TestMethod]
        public void StringDeduplicator_Deduplicate_Returns_Different_Strings_If_Two_Different_Texts_Are_Passed()
        {
            string t1 = "Text1";
            string t2 = "Text2";

            Assert.AreSame(t1, _StringDeduplicator.Deduplicate(t1));
            Assert.AreSame(t2, _StringDeduplicator.Deduplicate(t2));
        }

        [TestMethod]
        public void StringDeduplicator_Deduplicate_Returns_Same_String_If_Two_Different_Instances_Of_The_Same_Text_Are_Passed()
        {
            string t1 = "Text";
            string t2 = String.Format("{0}", t1); // <-- if we don't do this we just have t1 and t2 interned to the same instance before we start :)

            Assert.AreSame(t1, _StringDeduplicator.Deduplicate(t1));
            Assert.AreSame(t1, _StringDeduplicator.Deduplicate(t2));
        }

        [TestMethod]
        public void StringDeduplicator_DeduplicateArray_Does_Nothing_If_Passed_Null()
        {
            _StringDeduplicator.Deduplicate((string[])null);
        }

        [TestMethod]
        public void StringDeduplicator_DeduplicateArray_Removes_Duplicates()
        {
            string t1 = "X";
            string[] array = new string[] { t1, String.Format("{0}", t1), "Y" };

            _StringDeduplicator.Deduplicate(array);
            Assert.AreEqual(3, array.Length);
            Assert.AreEqual("X", array[0]);
            Assert.AreSame(array[0], array[1]);
            Assert.AreEqual("Y", array[2]);
        }

        [TestMethod]
        public void StringDeduplicator_DeduplicateList_Ignores_Null_Lists()
        {
            _StringDeduplicator.Deduplicate((IList<string>)null);
        }

        [TestMethod]
        public void StringDeduplicator_DeduplicateList_Removes_Duplicates()
        {
            string t1 = "X";
            List<string> list = new List<string> { t1, String.Format("{0}", t1), "Y" };

            _StringDeduplicator.Deduplicate(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("X", list[0]);
            Assert.AreSame(list[0], list[1]);
            Assert.AreEqual("Y", list[2]);
        }
    }
}
