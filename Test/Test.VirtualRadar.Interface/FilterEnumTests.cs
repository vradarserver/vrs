// Copyright © 2013 onwards, Andrew Whewell
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class FilterEnumTests
    {
        enum MyEnum {
            Value1,
            Value2
        }

        [TestMethod]
        public void FilterEnum_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var filter = new FilterEnum<MyEnum>();
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Invalid, FilterCondition.Equals);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, default(MyEnum), MyEnum.Value2);

            filter = new FilterEnum<MyEnum>(MyEnum.Value2);
            TestUtilities.TestProperty(filter, r => r.Condition, FilterCondition.Equals, FilterCondition.Contains);
            TestUtilities.TestProperty(filter, r => r.ReverseCondition, false);
            TestUtilities.TestProperty(filter, r => r.Value, MyEnum.Value2, MyEnum.Value1);
        }

        [TestMethod]
        public void FilterEnum_Passes_Returns_Correct_Results()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(MyEnum value in Enum.GetValues(typeof(MyEnum))) {
                        foreach(var testValue in new MyEnum?[] { null, MyEnum.Value1, MyEnum.Value2 }) {
                            var filter = new FilterEnum<MyEnum>() {
                                Condition = condition,
                                ReverseCondition = reverseCondition,
                                Value = value
                            };

                            var result = filter.Passes(testValue);

                            var expectedResult = true;
                            if(condition == FilterCondition.Equals) {
                                expectedResult = testValue == null ? false : value == testValue;
                                if(reverseCondition) expectedResult = !expectedResult;
                            }

                            Assert.AreEqual(expectedResult, result, "{0}/{1}/{2}/{3}", condition, reverseCondition, value, testValue);
                        }
                    }
                }
            }
        }
    }
}
