// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class PolarPlotsSliceJsonTests
    {
        private PolarPlotsSliceJson _Json;

        [TestInitialize]
        public void TestInitialise()
        {
            _Json = new PolarPlotsSliceJson();
        }

        [TestMethod]
        public void PolarPlotsSliceJson_Constructor_Initialises_Properties_To_Known_Values()
        {
            TestUtilities.TestProperty(_Json, r => r.FinishAltitude, 0, 1);
            TestUtilities.TestProperty(_Json, r => r.StartAltitude, 0, 2);
            Assert.AreEqual(0, _Json.Plots.Count);
        }

        [TestMethod]
        public void PolarPlotsSliceJson_ToModel_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(PolarPlotsSliceJson.ToModel(null));
        }

        [TestMethod]
        public void PolarPlotsSliceJson_ToModel_Converts_PolarPlotSlice()
        {
            var polarPlotSlice = new PolarPlotSlice() {
                AltitudeLower =  12000,
                AltitudeHigher = 13499,
            };
            polarPlotSlice.PolarPlots.Add(10, new PolarPlot() {
                Altitude =  12000,
                Angle =     10,
                Distance =  0,
                Latitude =  1.23,
                Longitude = 4.56,
            });

            var model = PolarPlotsSliceJson.ToModel(polarPlotSlice);

            Assert.AreEqual(12000, model.StartAltitude);
            Assert.AreEqual(13499, model.FinishAltitude);
            Assert.AreEqual(1, model.Plots.Count);
            Assert.AreEqual(1.23F, model.Plots[0].Latitude);
            Assert.AreEqual(4.56F, model.Plots[0].Longitude);
        }

        [TestMethod]
        public void PolarPlotsSliceJson_ToModel_Returns_Plots_In_Angle_Order()
        {
            var polarPlotSlice = new PolarPlotSlice();
            polarPlotSlice.PolarPlots.Add(99, new PolarPlot() { Angle =     99, Latitude =  99, Longitude = 99, });
            polarPlotSlice.PolarPlots.Add(30, new PolarPlot() { Angle =     30, Latitude =  30, Longitude = 30, });
            polarPlotSlice.PolarPlots.Add(45, new PolarPlot() { Angle =     45, Latitude =  45, Longitude = 45, });

            var model = PolarPlotsSliceJson.ToModel(polarPlotSlice);

            Assert.AreEqual(30F, model.Plots[0].Latitude);
            Assert.AreEqual(45F, model.Plots[1].Latitude);
            Assert.AreEqual(99F, model.Plots[2].Latitude);
        }
    }
}
