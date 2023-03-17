// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface.Feeds;

namespace Test.VirtualRadar.Interface.Feeds
{
    [TestClass]
    public class PolarPlotSlice_Tests
    {
        [TestMethod]
        public void Clone_Deep_Copies_All_Properties()
        {
            foreach(var property in typeof(PolarPlotSlice).GetProperties()) {
                var original = new PolarPlotSlice();
                switch(property.Name) {
                    case nameof(PolarPlotSlice.PolarPlots): original.PolarPlots.Add(17, new PolarPlot() { Altitude = 1 }); break;
                    default:                                TestDataGenerator.AssignPropertyValue(original, property, true); break;
                }

                var copy = original.Clone();

                foreach(var compareProperty in typeof(PolarPlotSlice).GetProperties()) {
                    var originalValue = compareProperty.GetValue(original, null);
                    var newValue = compareProperty.GetValue(copy, null);

                    switch(compareProperty.Name) {
                        case "PolarPlots":
                            var originalDict = (Dictionary<int, PolarPlot>)originalValue;
                            var newDict = (Dictionary<int, PolarPlot>)newValue;
                            Assert.IsFalse(Object.ReferenceEquals(originalDict, newDict));
                            Assert.AreEqual(originalDict.Count, newDict.Count);
                            for(var i = 0;i < originalDict.Count;++i) {
                                var originalPlot = originalDict[17];
                                var newPlot = newDict[17];
                                Assert.IsFalse(object.ReferenceEquals(originalPlot, newPlot));
                                Assert.AreEqual(originalPlot.Altitude, newPlot.Altitude);
                            }
                            break;
                        default:
                            Assert.AreEqual(originalValue, newValue, compareProperty.Name);
                            break;
                    }
                }
            }
        }
    }
}
