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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Listener.BaseStation
{
    [TestClass]
    public class PolarPlotTests
    {
        [TestMethod]
        public void PolarPlot_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            var plot = new PolarPlot();
            TestUtilities.TestProperty(plot, r => r.Altitude, 0, int.MaxValue);
            TestUtilities.TestProperty(plot, r => r.Angle, 0, int.MinValue);
            TestUtilities.TestProperty(plot, r => r.Distance, 0.0, double.MaxValue);
            TestUtilities.TestProperty(plot, r => r.Latitude, 0.0, double.MinValue);
            TestUtilities.TestProperty(plot, r => r.Longitude, 0.0, double.MaxValue);
        }

        [TestMethod]
        public void PolarPlot_Clone_Copies_All_Properties()
        {
            foreach(var property in typeof(PolarPlot).GetProperties()) {
                var original = new PolarPlot();
                TestUtilities.AssignPropertyValue(original, property, true);
                var copy = original.Clone();

                foreach(var compareProperty in typeof(PolarPlot).GetProperties()) {
                    var originalValue = compareProperty.GetValue(original, null);
                    var newValue = compareProperty.GetValue(copy, null);
                    Assert.AreEqual(originalValue, newValue, compareProperty.Name);
                }
            }
        }
    }
}
