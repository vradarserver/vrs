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
        public void SetValue_Returns_True_If_The_Value_Changed()
        {
            var value = new VersionedValue<int>(7, 14);
            Assert.IsTrue(value.SetValue(8, 14));
        }

        [TestMethod]
        public void SetValue_Returns_False_If_The_Value_Is_Unchanged()
        {
            var value = new VersionedValue<int>(7,14);
            Assert.IsFalse(value.SetValue(7, 14));
        }

        [TestMethod]
        [DataRow(2, 1)]
        [DataRow(2, 2)]
        [DataRow(2, 3)]
        public void SetValue_Does_Not_Insist_That_New_Version_Is_Greater_Than_Old_Version(int originalVersion, int newVersion)
        {
            var value = new VersionedValue<double>(4, originalVersion);
            value.SetValue(5, newVersion);
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

        [TestMethod]
        public void CopyFrom_Overwrites_Existing_Values()
        {
            var value = new VersionedValue<int>(1, 2);
            value.CopyFrom(new VersionedValue<int>(3, 4));

            Assert.AreEqual(3, value.Value);
            Assert.AreEqual(4, value.DataVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyFrom_Throws_If_Passed_Null()
        {
            new VersionedValue<int>().CopyFrom(null);
        }

        [TestMethod]
        public void CopyFrom_Does_Not_Change_Original()
        {
            var original = new VersionedValue<int>(3, 4);
            new VersionedValue<int>(1, 2).CopyFrom(original);

            Assert.AreEqual(3, original.Value);
            Assert.AreEqual(4, original.DataVersion);
        }
    }
}
