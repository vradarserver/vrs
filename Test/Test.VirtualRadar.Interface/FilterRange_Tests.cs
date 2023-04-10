// Copyright © 2013 onwards, Andrew Whewell
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
    public class FilterRange_Tests
    {
        [TestMethod]
        public void IsValid_Reports_Correct_Value()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(var lowerValue in new int?[] { null, 0, 1, 2 }) {
                        foreach(var upperValue in new int?[] { null, 0, 1, 2 }) {
                            var filter = new FilterRange<int>() {
                                Condition = condition,
                                ReverseCondition = reverseCondition,
                                LowerValue = lowerValue,
                                UpperValue = upperValue,
                            };

                            var result = filter.IsValid;

                            var expectedResult = condition == FilterCondition.Between && (lowerValue != null || upperValue != null);
                            Assert.AreEqual(expectedResult, result, "{0}/{1}/{2}/{3}", condition, reverseCondition, lowerValue, upperValue);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NormaliseRange_Swaps_Upper_And_Lower_When_Appropriate()
        {
            foreach(var lowerValue in new int?[] { null, 0, 1, 2 }) {
                foreach(var upperValue in new int?[] { null, 0, 1, 2 }) {
                    var filter = new FilterRange<int>() {
                        LowerValue = lowerValue,
                        UpperValue = upperValue,
                    };

                    filter.NormaliseRange();

                    var expectedLower = lowerValue;
                    var expectedUpper = upperValue;
                    if(expectedLower != null && expectedUpper != null && expectedLower > expectedUpper) {
                        expectedLower = upperValue;
                        expectedUpper = lowerValue;
                    }

                    Assert.AreEqual(expectedLower, filter.LowerValue, "{0}/{1}", lowerValue, upperValue);
                    Assert.AreEqual(expectedUpper, filter.UpperValue, "{0}/{1}", lowerValue, upperValue);
                }
            }
        }

        [TestMethod]
        public void Passes_Returns_Correct_Results()
        {
            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                foreach(var reverseCondition in new bool[] { false, true }) {
                    foreach(var lowerValue in new int?[] { null, 0, 1, 2 }) {
                        foreach(var upperValue in new int?[] { null, 0, 1, 2 }) {
                            foreach(var value in new int?[] { null, -1, 0, 1, 2, 3 }) {
                                var filter = new FilterRange<int>() {
                                    Condition = condition,
                                    ReverseCondition = reverseCondition,
                                    LowerValue = lowerValue,
                                    UpperValue = upperValue,
                                };

                                var result = filter.Passes(value);

                                var expectedResult = true;
                                if(filter.IsValid) {
                                    expectedResult = value == null ? false : (lowerValue == null || value >= lowerValue) && (upperValue == null || value <= upperValue);
                                    if(reverseCondition) expectedResult = !expectedResult;
                                }

                                Assert.AreEqual(expectedResult, result, "{0}/{1}/{2}/{3}/{4}", condition, reverseCondition, value, lowerValue, upperValue);
                            }
                        }
                    }
                }
            }
        }
    }
}
