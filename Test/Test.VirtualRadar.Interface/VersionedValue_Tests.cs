// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class VersionedValue_Tests
    {
        [TestMethod]
        public void SetValue_Assigns_Both_Value_And_Version()
        {
            var value = new VersionedValue<string>();
            value.SetValue("123", 987);

            Assert.AreEqual("123", value.Value);
            Assert.AreEqual(987, value.DataVersion);
        }

        [TestMethod]
        public void SetValue_Does_Not_Change_Version_If_Value_Is_Unchanged()
        {
            var value = new VersionedValue<string>();
            value.SetValue("123", 654);
            value.SetValue("123", 321);

            Assert.AreEqual("123", value.Value);
            Assert.AreEqual(654, value.DataVersion);
        }

        [TestMethod]
        public void SetValue_Returns_True_If_Value_Has_Changed()
        {
            var value = new VersionedValue<int>();
            Assert.IsTrue(value.SetValue(2, 3));
        }

        [TestMethod]
        public void SetValue_Returns_False_If_Value_Is_Unchanged()
        {
            var value = new VersionedValue<int?>();
            value.SetValue(3, 4);
            Assert.IsFalse(value.SetValue(3, 4));
        }

        [TestMethod]
        [DataRow(2, 1)]
        [DataRow(2, 2)]
        [DataRow(2, 3)]
        public void SetValue_Does_Not_Insist_That_New_Version_Is_Greater_Than_Old_Version(int originalVersion, int newVersion)
        {
            var value = new VersionedValue<double>(4, originalVersion);
            Assert.IsTrue(value.SetValue(4.1, newVersion));
            Assert.AreEqual(newVersion, value.DataVersion);
        }

        [TestMethod]
        public void ImplicitOperator_Returns_Value_Wherever_T_Is_Required()
        {
            var value = new VersionedValue<char>('a', 0);
            Assert.IsTrue(Char.IsLetter(value));
        }

        [TestMethod]
        [DataRow(2, 1, true)]
        [DataRow(2, 2, false)]
        [DataRow(2, 3, false)]
        public void IsAfter_Returns_True_If_Version_Is_After_One_Passed_Across(int valueVersion, int compareAgainstVersion, bool expected)
        {
            var value = new VersionedValue<decimal>(0, valueVersion);
            var actual = value.IsAfter(compareAgainstVersion);
            Assert.AreEqual(expected, actual);
        }
    }
}
